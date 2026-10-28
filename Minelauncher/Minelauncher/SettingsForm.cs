using System;
using System.Windows.Forms;

namespace Minelauncher
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettingsToUI();
            savebtn.Click += Savebtn_Click;
            browseJavaBtn.Click += BrowseJavaBtn_Click;
        }

        private void LoadSettingsToUI()
        {
            LauncherSettings s = LauncherSettings.Load();
            int ram = s.RamMb;
            if (ram < (int)ramupdown.Minimum) ram = (int)ramupdown.Minimum;
            if (ram > (int)ramupdown.Maximum) ram = (int)ramupdown.Maximum;
            ramupdown.Value = ram;
            javaBox.Text = s.JavaPath;
            argumentBox.Text = s.ExtraArgs;
        }

        private void Savebtn_Click(object sender, EventArgs e)
        {
            LauncherSettings s = new LauncherSettings();
            s.RamMb = (int)ramupdown.Value;
            s.JavaPath = string.IsNullOrWhiteSpace(javaBox.Text) ? "java" : javaBox.Text.Trim();
            s.ExtraArgs = argumentBox.Text;
            s.Save();
            MessageBox.Show("Настройки сохранены.");
            this.Close();
        }

        private void BrowseJavaBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Java executable (javaw.exe;java.exe)|javaw.exe;java.exe|Все файлы (*.*)|*.*";
                dlg.Title = "Выберите javaw.exe";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    javaBox.Text = dlg.FileName;
                }
            }
        }
    }
}