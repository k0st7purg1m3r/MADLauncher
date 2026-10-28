using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minelauncher
{
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();
        }

        public void AppendLog(string text)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() =>
                {
                    logBox.AppendText(text + Environment.NewLine);
                    logBox.ScrollToCaret();
                }));
            }
            else
            {
                logBox.AppendText(text + Environment.NewLine);
                logBox.ScrollToCaret();
            }
        }

        public void ClearLog()
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => logBox.Clear()));
            }
            else
            {
                logBox.Clear();
            }
        }
    }
}