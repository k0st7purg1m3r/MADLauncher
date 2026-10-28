using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Minelauncher
{
    public partial class Form1 : Form
    {
        private const string ManifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
        private const string AssetsUrl = "https://resources.download.minecraft.net/";
        private const string LibrariesUrl = "https://libraries.minecraft.net/";

        private static readonly string GameDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".minecraft");
        private static readonly string VersionsDirectory = Path.Combine(GameDirectory, "versions");
        private static readonly string LibrariesDirectory = Path.Combine(GameDirectory, "libraries");
        private static readonly string AssetsDirectory = Path.Combine(GameDirectory, "assets");

        private VersionEntry SelectedVersion;
        private bool IsJavaAvailable = false;
        private SettingsForm SettingsWindow;
        private List<VersionEntry> AllVersions = new List<VersionEntry>();
        private ConsoleForm LogWindow;

        public Form1()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;

            InitializeComponent();
            Load += Form1_Load;
            comboBox1.SelectedIndexChanged += VersionComboBox_SelectedIndexChanged;
            installorplay.Click += InstallOrPlay_Click;
            settingsbtn.Click += SettingsButton_Click;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            LauncherSettings config = LauncherSettings.Load();
            IsJavaAvailable = CheckJava(config.JavaPath);
            label1.Text = IsJavaAvailable ? "Java: OK" : "Java: не найдена";

            try { await LoadAllVersionsAsync(); }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки версий: " + ex.Message); }
        }

        private bool CheckJava(string javaPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(javaPath)) javaPath = "java";
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = javaPath;
                startInfo.Arguments = "-version";
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(3000);
                    return process.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            if (SettingsWindow == null || SettingsWindow.IsDisposed)
                SettingsWindow = new SettingsForm();
            SettingsWindow.Show();
            SettingsWindow.BringToFront();
            SettingsWindow.FormClosed += (s, ev) =>
            {
                LauncherSettings config = LauncherSettings.Load();
                IsJavaAvailable = CheckJava(config.JavaPath);
                label1.Text = IsJavaAvailable ? "Java: OK" : "Java: не найдена";
                VersionComboBox_SelectedIndexChanged(this, EventArgs.Empty);
            };
        }

        private Task LoadAllVersionsAsync()
        {
            return Task.Run(() =>
            {
                List<VersionEntry> versionsList = new List<VersionEntry>();

                try
                {
                    string json = DownloadString(ManifestUrl);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = int.MaxValue;
                    Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(json);
                    ArrayList versions = (ArrayList)root["versions"];

                    foreach (object versionObj in versions)
                    {
                        Dictionary<string, object> versionData = (Dictionary<string, object>)versionObj;
                        string type = (string)versionData["type"];

                        if (type != "release") continue;

                        string id = (string)versionData["id"];
                        DateTime releaseDate = DateTime.Parse((string)versionData["releaseTime"]);

                        versionsList.Add(new VersionEntry
                        {
                            Id = id,
                            Url = (string)versionData["url"],
                            Release = releaseDate
                        });
                    }
                }
                catch { }

                versionsList = versionsList.OrderBy(x => x.Release).ToList();
                AllVersions = versionsList;

                Invoke((Action)(() =>
                {
                    comboBox1.Items.Clear();
                    comboBox1.DisplayMember = "Display";
                    foreach (VersionEntry entry in versionsList) comboBox1.Items.Add(entry);
                    comboBox1.Enabled = true;
                    if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
                }));
            });
        }

        private void VersionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedVersion = comboBox1.SelectedItem as VersionEntry;
            if (SelectedVersion == null) { installorplay.Text = "Выберите версию"; installorplay.Enabled = false; return; }
            bool installed = IsVersionInstalled(SelectedVersion.Id);
            installorplay.Text = installed ? "Играть" : "Установить";
            installorplay.Enabled = IsJavaAvailable || !installed;
        }

        private bool IsVersionInstalled(string versionId)
        {
            string versionDir = Path.Combine(VersionsDirectory, versionId);
            return Directory.Exists(versionDir) && File.Exists(Path.Combine(versionDir, versionId + ".json"));
        }

        private async void InstallOrPlay_Click(object sender, EventArgs e)
        {
            if (SelectedVersion == null) return;

            if (installorplay.Text == "Играть")
            {
                if (!IsJavaAvailable) { MessageBox.Show("Java не найдена. Проверьте настройки."); return; }
                try { LaunchGame(SelectedVersion.Id); }
                catch (Exception ex) { MessageBox.Show("Ошибка запуска: " + ex.Message); }
                return;
            }

            installorplay.Enabled = false;
            comboBox1.Enabled = false;
            installorplay.Text = "Установка...";
            progressBar1.Value = 0;

            VersionEntry versionToInstall = SelectedVersion;
            try
            {
                await Task.Run(() => InstallVersion(versionToInstall));
                MessageBox.Show("Версия " + versionToInstall.Id + " установлена.");
            }
            catch (Exception ex) { MessageBox.Show("Ошибка установки: " + ex.Message); }
            finally
            {
                installorplay.Enabled = true;
                comboBox1.Enabled = true;
                VersionComboBox_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void InstallVersion(VersionEntry version)
        {
            string versionDir = Path.Combine(VersionsDirectory, version.Id);
            Directory.CreateDirectory(versionDir);

            string versionJsonPath = Path.Combine(versionDir, version.Id + ".json");
            DownloadFile(version.Url, versionJsonPath, true, null, 0);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(versionJsonPath));

            if (root.ContainsKey("downloads"))
            {
                Dictionary<string, object> downloads = (Dictionary<string, object>)root["downloads"];
                if (downloads.ContainsKey("client"))
                {
                    Dictionary<string, object> client = (Dictionary<string, object>)downloads["client"];
                    if (client.ContainsKey("url"))
                    {
                        string clientUrl = (string)client["url"];
                        string clientSha1 = client.ContainsKey("sha1") ? (string)client["sha1"] : null;
                        long clientSize = client.ContainsKey("size") ? Convert.ToInt64(client["size"]) : 0;
                        DownloadFile(clientUrl, Path.Combine(versionDir, "client.jar"), true, clientSha1, clientSize);
                    }
                }
            }

            if (root.ContainsKey("libraries"))
            {
                string osKey = GetNativeKey();
                ArrayList libraries = (ArrayList)root["libraries"];
                List<string[]> filesToDownload = new List<string[]>();

                foreach (object libraryObj in libraries)
                {
                    Dictionary<string, object> library = (Dictionary<string, object>)libraryObj;
                    if (library.ContainsKey("rules") && !RulesAllow((ArrayList)library["rules"])) continue;

                    if (library.ContainsKey("downloads"))
                    {
                        Dictionary<string, object> libraryDownloads = (Dictionary<string, object>)library["downloads"];

                        if (libraryDownloads.ContainsKey("artifact"))
                        {
                            Dictionary<string, object> artifact = (Dictionary<string, object>)libraryDownloads["artifact"];
                            string path = (string)artifact["path"];
                            string url = (string)artifact["url"];
                            string sha1 = artifact.ContainsKey("sha1") ? (string)artifact["sha1"] : null;
                            long size = artifact.ContainsKey("size") ? Convert.ToInt64(artifact["size"]) : 0;
                            filesToDownload.Add(new string[] { path, url, sha1, size.ToString() });
                        }

                        if (libraryDownloads.ContainsKey("classifiers"))
                        {
                            Dictionary<string, object> classifiers = (Dictionary<string, object>)libraryDownloads["classifiers"];
                            string[] possibleKeys = new string[] 
                            { 
                                "natives-" + osKey + "-64", 
                                "natives-" + osKey + "-32", 
                                "natives-" + osKey 
                            };
                            foreach (string key in possibleKeys)
                            {
                                if (classifiers.ContainsKey(key))
                                {
                                    Dictionary<string, object> nativeFile = (Dictionary<string, object>)classifiers[key];
                                    string path = (string)nativeFile["path"];
                                    string url = (string)nativeFile["url"];
                                    string sha1 = nativeFile.ContainsKey("sha1") ? (string)nativeFile["sha1"] : null;
                                    long size = nativeFile.ContainsKey("size") ? Convert.ToInt64(nativeFile["size"]) : 0;
                                    filesToDownload.Add(new string[] { path, url, sha1, size.ToString() });
                                    break;
                                }
                            }
                        }
                    }
                    else if (library.ContainsKey("name"))
                    {
                        string path = MavenToPath((string)library["name"]);
                        filesToDownload.Add(new string[] { path, LibrariesUrl + path, null, "0" });
                    }
                }

                SetProgressMax(filesToDownload.Count);
                int downloadedCount = 0;
                foreach (string[] fileInfo in filesToDownload)
                {
                    string destination = Path.Combine(LibrariesDirectory, fileInfo[0].Replace('/', Path.DirectorySeparatorChar));
                    string sha1 = fileInfo[2];
                    long size = long.Parse(fileInfo[3]);

                    bool needDownload = !File.Exists(destination);
                    if (!needDownload && sha1 != null && sha1.Length > 0)
                    {
                        if (GetSha1(destination) != sha1.ToLower()) needDownload = true;
                    }

                    if (needDownload)
                    {
                        DownloadFile(fileInfo[1], destination, false, sha1, size);
                    }
                    downloadedCount++;
                    SetProgress(downloadedCount);
                }
            }

            if (root.ContainsKey("assetIndex"))
            {
                Dictionary<string, object> assetIndex = (Dictionary<string, object>)root["assetIndex"];
                string indexUrl = (string)assetIndex["url"];
                string indexId = (string)assetIndex["id"];
                string indexDir = Path.Combine(AssetsDirectory, "indexes");
                Directory.CreateDirectory(indexDir);
                string indexPath = Path.Combine(indexDir, indexId + ".json");
                if (!File.Exists(indexPath)) DownloadFile(indexUrl, indexPath, false, null, 0);

                Dictionary<string, object> indexRoot = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(indexPath));
                Dictionary<string, object> objects = (Dictionary<string, object>)indexRoot["objects"];

                List<string> hashes = new List<string>();
                foreach (KeyValuePair<string, object> objectEntry in objects)
                {
                    Dictionary<string, object> objectData = (Dictionary<string, object>)objectEntry.Value;
                    hashes.Add((string)objectData["hash"]);
                }

                SetProgressMax(hashes.Count);
                int downloadedCount = 0;
                foreach (string hash in hashes)
                {
                    string prefix = hash.Substring(0, 2);
                    string destination = Path.Combine(AssetsDirectory, "objects", prefix, hash);
                    if (!File.Exists(destination))
                        DownloadFile(AssetsUrl + prefix + "/" + hash, destination, false, hash, 0);
                    downloadedCount++;
                    SetProgress(downloadedCount);
                }
            }

            string nativesDir = Path.Combine(versionDir, "natives");
            Directory.CreateDirectory(nativesDir);
            if (root.ContainsKey("libraries"))
            {
                string osKey = GetNativeKey();
                ArrayList libraries = (ArrayList)root["libraries"];
                foreach (object libraryObj in libraries)
                {
                    Dictionary<string, object> library = (Dictionary<string, object>)libraryObj;
                    if (library.ContainsKey("rules") && !RulesAllow((ArrayList)library["rules"])) continue;

                    string jarPath = null;

                    if (library.ContainsKey("downloads"))
                    {
                        Dictionary<string, object> libraryDownloads = (Dictionary<string, object>)library["downloads"];
                        if (libraryDownloads.ContainsKey("classifiers"))
                        {
                            Dictionary<string, object> classifiers = (Dictionary<string, object>)libraryDownloads["classifiers"];
                            string[] possibleKeys = new string[] 
                            { 
                                "natives-" + osKey + "-64", 
                                "natives-" + osKey + "-32", 
                                "natives-" + osKey 
                            };
                            foreach (string key in possibleKeys)
                            {
                                if (classifiers.ContainsKey(key))
                                {
                                    Dictionary<string, object> nativeFile = (Dictionary<string, object>)classifiers[key];
                                    jarPath = (string)nativeFile["path"];
                                    break;
                                }
                            }
                        }
                    }

                    if (jarPath == null && library.ContainsKey("natives"))
                    {
                        Dictionary<string, object> nativesMap = (Dictionary<string, object>)library["natives"];
                        if (nativesMap.ContainsKey(osKey))
                        {
                            string classifier = (string)nativesMap[osKey];
                            classifier = classifier.Replace("${arch}", Environment.Is64BitProcess ? "64" : "32");

                            string name = (string)library["name"];
                            string[] parts = name.Split(':');
                            string basePath = parts[0].Replace('.', '/') + "/" + parts[1] + "/" + parts[2];
                            string fileName = parts[1] + "-" + parts[2] + "-" + classifier + ".jar";
                            jarPath = basePath + "/" + fileName;
                        }
                    }

                    if (jarPath == null) continue;

                    string fullJarPath = Path.Combine(LibrariesDirectory, jarPath.Replace('/', Path.DirectorySeparatorChar));
                    if (!File.Exists(fullJarPath)) continue;

                    using (ZipArchive zip = ZipFile.OpenRead(fullJarPath))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            if (entry.FullName.StartsWith("META-INF")) continue;
                            if (string.IsNullOrEmpty(entry.Name)) continue;
                            string outputPath = Path.Combine(nativesDir, entry.Name);
                            if (File.Exists(outputPath)) File.Delete(outputPath);
                            entry.ExtractToFile(outputPath);
                        }
                    }
                }
            }
        }

        private static string GetSha1(string filePath)
        {
            try
            {
                using (var sha1 = SHA1.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch { return null; }
        }

        private void SetProgressMax(int max)
        {
            Invoke((Action)(() =>
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = max > 0 ? max : 1;
                progressBar1.Value = 0;
            }));
        }

        private void SetProgress(int value)
        {
            Invoke((Action)(() =>
            {
                progressBar1.Value = Math.Min(value, progressBar1.Maximum);
            }));
        }

        private string GetNativeKey()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) return "windows";
            if (Environment.OSVersion.Platform == PlatformID.MacOSX) return "osx";
            return "linux";
        }

        private bool RulesAllow(ArrayList rules)
        {
            bool allow = false;
            string os = Environment.OSVersion.Platform == PlatformID.Win32NT ? "windows"
                      : Environment.OSVersion.Platform == PlatformID.MacOSX ? "osx" : "linux";

            foreach (object ruleObj in rules)
            {
                Dictionary<string, object> rule = (Dictionary<string, object>)ruleObj;
                string action = (string)rule["action"];
                bool match = true;

                if (rule.ContainsKey("os"))
                {
                    Dictionary<string, object> osData = (Dictionary<string, object>)rule["os"];
                    if (osData.ContainsKey("name"))
                    {
                        match = (string)osData["name"] == os;
                    }
                }
                if (match) allow = action == "allow";
            }
            return allow;
        }

        private static string MavenToPath(string name)
        {
            string[] parts = name.Split(':');
            string group = parts[0].Replace('.', '/');
            string artifact = parts[1];
            string version = parts[2];
            string classifier = parts.Length > 3 ? "-" + parts[3] : "";
            return group + "/" + artifact + "/" + version + "/" + artifact + "-" + version + classifier + ".jar";
        }

        private void LaunchGame(string versionId)
        {
            LauncherSettings config = LauncherSettings.Load();

            string versionDir = Path.Combine(VersionsDirectory, versionId);
            string versionJsonPath = Path.Combine(versionDir, versionId + ".json");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(versionJsonPath));

            string username = string.IsNullOrWhiteSpace(textBox1.Text) ? "Player" : textBox1.Text.Trim();
            string uuid = GenerateOfflineUuid(username);
            string nativesDir = Path.Combine(versionDir, "natives");

            List<string> classpath = new List<string>();

            string clientJarPath = Path.Combine(versionDir, "client.jar");
            if (File.Exists(clientJarPath)) classpath.Add(clientJarPath);

            if (root.ContainsKey("libraries"))
            {
                ArrayList libraries = (ArrayList)root["libraries"];
                foreach (object libraryObj in libraries)
                {
                    Dictionary<string, object> library = (Dictionary<string, object>)libraryObj;
                    if (library.ContainsKey("rules") && !RulesAllow((ArrayList)library["rules"])) continue;
                    string libraryPath = null;

                    if (library.ContainsKey("downloads"))
                    {
                        Dictionary<string, object> libraryDownloads = (Dictionary<string, object>)library["downloads"];
                        if (libraryDownloads.ContainsKey("artifact"))
                        {
                            Dictionary<string, object> artifact = (Dictionary<string, object>)libraryDownloads["artifact"];
                            libraryPath = (string)artifact["path"];
                        }
                    }
                    if (libraryPath == null && library.ContainsKey("name"))
                    {
                        libraryPath = MavenToPath((string)library["name"]);
                    }
                    if (libraryPath != null)
                    {
                        string fullPath = Path.Combine(LibrariesDirectory, libraryPath.Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(fullPath)) classpath.Add(fullPath);
                    }
                }
            }
            string classpathString = string.Join(Path.PathSeparator.ToString(), classpath.ToArray());

            List<string> jvmArguments = new List<string>();
            jvmArguments.Add("-Xmx" + config.RamMb + "M");
            jvmArguments.Add("-Djava.library.path=" + nativesDir);

            if (!string.IsNullOrWhiteSpace(config.ExtraArgs))
            {
                string[] extraArgs = config.ExtraArgs.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string arg in extraArgs) jvmArguments.Add(arg);
            }

            string mainClass = (string)root["mainClass"];

            if (root.ContainsKey("arguments"))
            {
                Dictionary<string, object> arguments = (Dictionary<string, object>)root["arguments"];
                if (arguments.ContainsKey("jvm"))
                {
                    ArrayList jvmArray = (ArrayList)arguments["jvm"];
                    foreach (object arg in jvmArray)
                    {
                        string resolvedArg = ResolveArgument(arg, username, versionId, nativesDir, classpathString, uuid);
                        if (resolvedArg != null) jvmArguments.Add(resolvedArg);
                    }
                }
            }

            jvmArguments.Add("-cp");
            jvmArguments.Add(classpathString);
            jvmArguments.Add(mainClass);

            List<string> gameArguments = new List<string>();
            if (root.ContainsKey("arguments"))
            {
                Dictionary<string, object> arguments = (Dictionary<string, object>)root["arguments"];
                if (arguments.ContainsKey("game"))
                {
                    ArrayList gameArray = (ArrayList)arguments["game"];
                    foreach (object arg in gameArray)
                    {
                        string resolvedArg = ResolveArgument(arg, username, versionId, nativesDir, classpathString, uuid);
                        if (resolvedArg != null) gameArguments.Add(resolvedArg);
                    }
                }
            }
            else if (root.ContainsKey("minecraftArguments"))
            {
                Dictionary<string, object> assetIndex = (Dictionary<string, object>)root["assetIndex"];
                string rawArguments = ((string)root["minecraftArguments"])
                    .Replace("${auth_player_name}", username)
                    .Replace("${version_name}", versionId)
                    .Replace("${game_directory}", "\"" + GameDirectory + "\"")
                    .Replace("${assets_root}", "\"" + AssetsDirectory + "\"")
                    .Replace("${assets_index_name}", (string)assetIndex["id"])
                    .Replace("${auth_uuid}", uuid)
                    .Replace("${auth_access_token}", "0")
                    .Replace("${user_type}", "legacy")
                    .Replace("${user_properties}", "{}");

                string[] parts = SplitArguments(rawArguments);
                gameArguments.AddRange(parts);
            }

            string javaArgs = BuildArgumentsString(jvmArguments) + " " + string.Join(" ", gameArguments.ToArray());

            string javaExe = string.IsNullOrWhiteSpace(config.JavaPath) ? "java" : config.JavaPath;

            if (javaExe != "java" && !File.Exists(javaExe))
            {
                MessageBox.Show("Файл Java не найден по пути: " + javaExe);
                return;
            }

            if (LogWindow == null || LogWindow.IsDisposed)
            {
                LogWindow = new ConsoleForm();
            }
            LogWindow.Show();
            LogWindow.BringToFront();
            LogWindow.ClearLog();
            LogWindow.AppendLog("Запуск: " + javaExe + " " + javaArgs);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = javaExe;
            startInfo.Arguments = javaArgs;
            startInfo.WorkingDirectory = GameDirectory;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            Process process = new Process();
            process.StartInfo = startInfo;

            process.OutputDataReceived += (s, ev) =>
            {
                if (ev.Data != null && LogWindow != null && !LogWindow.IsDisposed)
                {
                    LogWindow.AppendLog(ev.Data);
                }
            };
            process.ErrorDataReceived += (s, ev) =>
            {
                if (ev.Data != null && LogWindow != null && !LogWindow.IsDisposed)
                {
                    LogWindow.AppendLog(ev.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            LogWindow.FormClosed += (s, ev) =>
            {
                try { if (!process.HasExited) process.Kill(); }
                catch { }
            };
        }

        private static string[] SplitArguments(string commandLine)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in commandLine)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    current.Append(c);
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            return result.ToArray();
        }

        private static string BuildArgumentsString(List<string> args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string arg in args)
            {
                if (stringBuilder.Length > 0) stringBuilder.Append(' ');

                if (arg.IndexOf(' ') >= 0 || arg.IndexOf('"') >= 0 || arg.Contains(":\\") || arg.Contains(":/"))
                {
                    stringBuilder.Append('"');
                    stringBuilder.Append(arg.Replace("\"", "\\\""));
                    stringBuilder.Append('"');
                }
                else
                {
                    stringBuilder.Append(arg);
                }
            }
            return stringBuilder.ToString();
        }

        private string ResolveArgument(object arg, string username, string versionId, string nativesDir, string classpath, string uuid)
        {
            if (arg is string)
                return ReplacePlaceholders((string)arg, username, versionId, nativesDir, classpath, uuid);

            Dictionary<string, object> argDict = arg as Dictionary<string, object>;
            if (argDict != null)
            {
                if (argDict.ContainsKey("rules") && !RulesAllow((ArrayList)argDict["rules"])) return null;
                if (argDict.ContainsKey("value"))
                {
                    object value = argDict["value"];
                    if (value is string) return ReplacePlaceholders((string)value, username, versionId, nativesDir, classpath, uuid);
                    ArrayList valueArray = value as ArrayList;
                    if (valueArray != null)
                    {
                        foreach (object val in valueArray)
                            if (val is string)
                                return ReplacePlaceholders((string)val, username, versionId, nativesDir, classpath, uuid);
                    }
                }
            }
            return null;
        }

        private string ReplacePlaceholders(string text, string username, string versionId, string nativesDir, string classpath, string uuid)
        {
            if (text == null) return null;
            string assetIndexId = "";
            try
            {
                string jsonPath = Path.Combine(VersionsDirectory, versionId, versionId + ".json");
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(jsonPath));
                Dictionary<string, object> assetIndex = (Dictionary<string, object>)root["assetIndex"];
                assetIndexId = (string)assetIndex["id"];
            }
            catch { }

            return text.Replace("${auth_player_name}", username)
                    .Replace("${version_name}", versionId)
                    .Replace("${game_directory}", GameDirectory)
                    .Replace("${assets_root}", AssetsDirectory)
                    .Replace("${assets_index_name}", assetIndexId)
                    .Replace("${auth_uuid}", uuid)
                    .Replace("${auth_access_token}", "0")
                    .Replace("${user_type}", "legacy")
                    .Replace("${natives_directory}", nativesDir)
                    .Replace("${classpath}", classpath);
        }

        private static string GenerateOfflineUuid(string username)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes("OfflinePlayer:" + username));
                hash[6] = (byte)((hash[6] & 0x0F) | 0x30);
                hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
                return new Guid(hash).ToString("N");
            }
        }

        private static string DownloadString(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "Mozilla/5.0");
                return webClient.DownloadString(url);
            }
        }

        private void DownloadFile(string url, string destination, bool showProgress, string expectedSha1, long expectedSize)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination));

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "Mozilla/5.0");

                if (showProgress)
                {
                    Invoke((Action)(() =>
                    {
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 100;
                        progressBar1.Value = 0;
                    }));

                    webClient.DownloadProgressChanged += (s, e) =>
                    {
                        Invoke((Action)(() => progressBar1.Value = Math.Min(e.ProgressPercentage, 100)));
                    };
                }

                try
                {
                    Task downloadTask = webClient.DownloadFileTaskAsync(new Uri(url), destination);
                    downloadTask.Wait();
                }
                catch (AggregateException ae)
                {
                    WebException webEx = ae.InnerException as WebException;
                    if (webEx != null && webEx.Response is HttpWebResponse)
                    {
                        HttpWebResponse response = (HttpWebResponse)webEx.Response;
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            if (File.Exists(destination)) File.Delete(destination);
                            return;
                        }
                    }
                    throw;
                }

                if (File.Exists(destination))
                {
                    bool isValid = true;
                    if (expectedSize > 0 && new FileInfo(destination).Length != expectedSize)
                    {
                        isValid = false;
                    }
                    if (isValid && expectedSha1 != null && expectedSha1.Length > 0)
                    {
                        if (GetSha1(destination) != expectedSha1.ToLower())
                        {
                            isValid = false;
                        }
                    }
                    if (!isValid)
                    {
                        File.Delete(destination);
                    }
                }

                if (showProgress)
                    Invoke((Action)(() => progressBar1.Value = 100));
            }
        }

        private class VersionEntry
        {
            public string Id;
            public string Url;
            public DateTime Release;
            public string Display
            {
                get
                {
                    return Id + " (" + Release.ToString("yyyy-MM-dd") + ")";
                }
            }
        }
    }

    public class ConsoleForm : Form
    {
        private RichTextBox LogBox;

        public ConsoleForm()
        {
            this.Text = "Логи Minecraft";
            this.Size = new Size(800, 500);
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;

            LogBox = new RichTextBox();
            LogBox.Dock = DockStyle.Fill;
            LogBox.BackColor = Color.Black;
            LogBox.ForeColor = Color.White;
            LogBox.Font = new Font("Consolas", 10f);
            LogBox.ReadOnly = true;
            LogBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.Controls.Add(LogBox);
        }

        public void AppendLog(string text)
        {
            if (LogBox.InvokeRequired)
            {
                LogBox.Invoke(new Action(() =>
                {
                    LogBox.AppendText(text + Environment.NewLine);
                    LogBox.ScrollToCaret();
                }));
            }
            else
            {
                LogBox.AppendText(text + Environment.NewLine);
                LogBox.ScrollToCaret();
            }
        }

        public void ClearLog()
        {
            if (LogBox.InvokeRequired)
            {
                LogBox.Invoke(new Action(() => LogBox.Clear()));
            }
            else
            {
                LogBox.Clear();
            }
        }
    }
}