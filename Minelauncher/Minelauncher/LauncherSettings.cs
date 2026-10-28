using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Minelauncher
{
    public class LauncherSettings
    {
        public int RamMb { get; set; }
        public string JavaPath { get; set; }
        public string ExtraArgs { get; set; }
        public bool EnableLogs { get; set; }

        public LauncherSettings()
        {
            RamMb = 2048;
            JavaPath = "java";
            ExtraArgs = "";
            EnableLogs = false;
        }

        private static string FilePath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json"); }
        }

        public static LauncherSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new LauncherSettings();
                string json = File.ReadAllText(FilePath);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                LauncherSettings s = ser.Deserialize<LauncherSettings>(json);
                if (s == null) return new LauncherSettings();
                if (string.IsNullOrEmpty(s.JavaPath)) s.JavaPath = "java";
                if (s.RamMb < 1024) s.RamMb = 1024;
                return s;
            }
            catch { return new LauncherSettings(); }
        }

        public void Save()
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string json = ser.Serialize(this);
            File.WriteAllText(FilePath, json);
        }
    }
}