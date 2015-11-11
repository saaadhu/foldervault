using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderVault
{
    public partial class FolderVaultForm : Form
    {
        System.Timers.Timer timer;
        public FolderVaultForm()
        {
            InitializeComponent();
            timer = new System.Timers.Timer(1 * 60 * 1000);
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SaveContents();
        }

        private static bool RunGit (string args)
        {
            var p = new Process();
            var pinfo = new ProcessStartInfo()
            {
                FileName = @"C:\Program Files (x86)\Git\bin\git.exe",
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = pathToWatch,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            p.StartInfo = pinfo;
            p.Start();
            var res = p.StandardError.ReadToEnd() + " : " + p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0 && !res.Contains("nothing to commit"))
            {
                MessageBox.Show(res);
                return false;
            }
            return true;
        }

        private static bool SaveContents()
        {
            if (!RunGit("init ."))
                return false;

            if (!RunGit("add --all"))
                return false;

            if (!RunGit(string.Format(@"commit -am ""{0}""", DateTime.Now)))
                return false;

            return true;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = pathTextBox.Text;
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            pathTextBox.Text = dialog.SelectedPath;
        }

        static bool monitoring = false;
        static string pathToWatch;
        private void button2_Click(object sender, EventArgs e)
        {
            if (monitoring == false)
            {
                var text = pathTextBox.Text;
                if (text == "" || !Directory.Exists(text))
                {
                    MessageBox.Show(this, "Invalid folder path specified.", "Failed to start monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                pathToWatch = text;
                if (!SaveContents())
                    return;
                timer.Start();
                pathTextBox.Enabled = false;
                button2.Text = "Stop monitoring";
                this.Text = "Folder Vault - " + pathToWatch;
                
            }
            else
            {
                timer.Stop();
                pathTextBox.Enabled = true;
                button2.Text = "Start monitoring";
                this.Text = "Folder Vault";
            }
            monitoring = !monitoring;
        }
    }
}
