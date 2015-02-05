using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ShadowSwitch
{
    public partial class Form1 : Form
    {
        string _curDir = null;
        private bool _isModifing = false;
        private string _curFile = "";

        public Form1()
        {
            InitializeComponent();
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (_isModifing &&
                    MessageBox.Show("The file has changed, do you want to save it before leave?", "Information",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (!SaveFile())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                if (MessageBox.Show("Close window will close shadowsock, are you sure?", "Warning",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            CloseAllChildProcess();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void miExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _curDir = AppDomain.CurrentDomain.BaseDirectory;
            LoadFiles();
        }

        private void LoadFiles()
        {
            DirectoryInfo di = new DirectoryInfo(_curDir);
            var flist = di.GetFiles("*.json");
            listBox1.Items.Clear();
            var item = contextMenuStrip1.Items[contextMenuStrip1.Items.Count - 1];
            contextMenuStrip1.Items.Clear();
            foreach (var fileInfo in flist)
            {
                listBox1.Items.Add(fileInfo.Name);
                contextMenuStrip1.Items.Add(fileInfo.Name, null, miConfig_OnClick);
            }
            contextMenuStrip1.Items.Add(item);
        }

        private void miConfig_OnClick(object sender, EventArgs e)
        {
            var mi = (ToolStripMenuItem)sender;
            if (mi != null)
            {
                var name = mi.Text;
                if (!string.IsNullOrEmpty(name))
                {
                    SwitchTo(name);
                }
            }
        }

        private void SwitchTo(string name)
        {
            var fullName = Path.Combine(_curDir, name);
            if (File.Exists(fullName))
            {
                CloseAllChildProcess();
                var parameter = String.Format("-c \"{0}\"", fullName);
                var psi = new ProcessStartInfo(Path.Combine(_curDir, "shadowsocks-local.exe"), parameter);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                var newproc = Process.Start(psi);
            }
            else
            {
                MessageBox.Show("File not exists!" + fullName, "Error");
            }
        }

        private static void CloseAllChildProcess()
        {
            var processes = Process.GetProcessesByName("shadowsocks-local");
            foreach (var process in processes)
            {
                process.Kill();
                process.Close();
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = listBox1.SelectedItem;
            if (item != null)
            {
                SwitchTo(item.ToString());
            }
        }

        private void txt_OnInput(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_curFile))
            {
                _isModifing = true;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = listBox1.SelectedIndex;
            if (idx >= 0)
            {
                if (_isModifing &&
                    MessageBox.Show("The file has changed, do you want to save it before leave?", "Information",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveFile();
                }

                _curFile = listBox1.Items[idx].ToString();
                LoadFile(_curFile);
            }
        }

        private bool SaveFile()
        {
            if (!String.IsNullOrEmpty(_curFile))
            {
                if (!_isModifing)
                {
                    ResetForm();
                    return true;
                }

                var cfg = new Config();
                if (txtServer.Text != "")
                {
                    cfg.server = txtServer.Text;
                }
                else
                {
                    MessageBox.Show("Server is empty!");
                    txtServer.Focus();
                    return false;
                }
                int port = 0;
                if (txtServerPort.Text != "" && int.TryParse(txtServerPort.Text, out port))
                {
                    cfg.server_port = port;
                }
                else
                {
                    MessageBox.Show("Server port is invalid!");
                    txtServerPort.Focus();
                    return false;
                }
                if (txtLocalPort.Text != "" && int.TryParse(txtLocalPort.Text, out port))
                {
                    cfg.local_port = port;
                }
                else
                {
                    MessageBox.Show("Local port is invalid!");
                    txtLocalPort.Focus();
                    return false;
                }
                if (txtPassword.Text != "")
                {
                    cfg.password = txtPassword.Text;
                }
                else
                {
                    MessageBox.Show("Password is empty!");
                    txtPassword.Focus();
                    return false;
                }
                if (txtMethod.Text != "")
                {
                    cfg.method = txtMethod.Text;
                }
                else
                {
                    MessageBox.Show("Method is empty!");
                    txtMethod.Focus();
                    return false;
                }
                if (txtTimeOut.Text != "" && int.TryParse(txtTimeOut.Text, out port))
                {
                    cfg.timeout = port;
                }
                else
                {
                    MessageBox.Show("Timeout is invalid!");
                    txtTimeOut.Focus();
                    return false;
                }
                var content = JsonConvert.SerializeObject(cfg, Formatting.Indented);

                var txtNewFileName = txtFileName.Text + ".json";
                var saveAs = txtNewFileName != _curFile;
                var fullName = Path.Combine(_curDir, txtNewFileName);

                if (File.Exists(fullName))
                {
                    File.Delete(fullName);
                }
                using (var sw = File.CreateText(fullName))
                {
                    sw.Write(content);
                    sw.Flush();
                    sw.Close();
                }
                if (saveAs)
                {
                    File.Delete(Path.Combine(_curDir, _curFile));
                }
                ResetForm();
                LoadFiles();
                return true;
            }
            else
            {
                MessageBox.Show("There is no file currently been edit.");
                return false;
            }
        }

        private void ResetForm()
        {
            _isModifing = false;
            _curFile = "";
            txtFileName.Text = "";
            txtServer.Text = "";
            txtServerPort.Text = "";
            txtLocalPort.Text = "";
            txtPassword.Text = "";
            txtMethod.Text = "";
            txtTimeOut.Text = "";
            txtFileName.Enabled = false;
            txtServer.Enabled = false;
            txtServerPort.Enabled = false;
            txtLocalPort.Enabled = false;
            txtPassword.Enabled = false;
            txtMethod.Enabled = false;
            txtTimeOut.Enabled = false;
        }

        private void LoadFile(string file)
        {
            string fullName = Path.Combine(_curDir, file);
            if (File.Exists(fullName))
            {
                try
                {
                    txtFileName.Text = file.Substring(0, file.Length - 5);
                    using (var sr = new StreamReader(fullName))
                    {
                        var content = sr.ReadToEnd();
                        Config cfg = null;
                        try
                        {
                            cfg = JsonConvert.DeserializeObject<Config>(content);
                        }
                        catch (Exception ex)
                        {
                            cfg = new Config();
                        }
                        BindForm(cfg);
                        _isModifing = false;
                        sr.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load config file fails," + ex.ToString());
                }
            }
            else
            {
                ResetForm();
                BindForm(new Config());
                _curFile = file;
                _isModifing = false;
            }
        }

        private void BindForm(Config config)
        {
            if (config == null)
                return;

            txtServer.Text = config.server;
            txtServerPort.Text = config.server_port.ToString();
            txtLocalPort.Text = config.local_port.ToString();
            txtPassword.Text = config.password;
            txtMethod.Text = config.method;
            txtTimeOut.Text = config.timeout.ToString();
            txtFileName.Enabled = true;
            txtServer.Enabled = true;
            txtServerPort.Enabled = true;
            txtLocalPort.Enabled = true;
            txtPassword.Enabled = true;
            txtMethod.Enabled = true;
            txtTimeOut.Enabled = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem;
            if (item != null)
            {
                var file = item.ToString();
                if (file == _curFile)
                {
                    _isModifing = false;
                    ResetForm();
                }
                File.Delete(Path.Combine(_curDir, file));
                listBox1.Items.Remove(item);
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var item = listBox1.SelectedItem;
            if (item != null)
            {
                var file = item.ToString();
                SwitchTo(file);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var i = listBox1.Items.Add("newconfig.json");
            listBox1.SelectedIndex = i;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                var bound = Screen.PrimaryScreen.Bounds;
                this.SetDesktopLocation((bound.Width - this.Width) / 2, (bound.Height - this.Height) / 2);
            }
        }

        private void Form1_StyleChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}
