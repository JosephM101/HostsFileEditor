using HostsFileEditor;
using HostsFileEditor.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Forms;

namespace Hosts_File_Editor
{
    public partial class Form1 : Form
    {
        string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
        string hostFileContentBackup = "";
        ContextMenuStrip listView_contextMenuStrip = new ContextMenuStrip();
        bool ResetSettings = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BringToFront();

            // Load settings
            rememberWindowLayoutToolStripMenuItem.Checked = Settings.Default.RememberWindowLayout;
            showGridlinesToolStripMenuItem.Checked = Settings.Default.ShowGridlines;

            if (rememberWindowLayoutToolStripMenuItem.Checked)
            {
                this.Hide();
                this.Size = Settings.Default.WindowSize;
                this.Location = Settings.Default.WindowPosition;
                this.WindowState = Settings.Default.WindowState;
                this.Show();
            }
            textBox.Font = Settings.Default.Font;

            if (IsUserAdministrator())
            {
                string content = File.ReadAllText(hostsFilePath, Encoding.UTF8);
                ProcessHosts(content, true);
            }
            else
            {
                MessageBox.Show("This application needs to be run with administrator privileges.", "Hosts File Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            //listView_contextMenuStrip.Opening += (y, z) =>
            //{
            //    
            //};
            ToolStripMenuItem removeItem = new ToolStripMenuItem("Remove");
            removeItem.Click += (o, r) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    ListViewItem selected = listView.SelectedItems[0];
                    listView.Items.Remove(selected);

                    List<string> lines = textBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                    // Find line containing hostname
                    int index = lines.FindIndex(x => x.Contains(selected.SubItems[1].Text));
                    lines.RemoveAt(index);
                    string newString = string.Join(Environment.NewLine, lines.ToArray());
                    ProcessHosts(newString, false);
                }
            };
            listView_contextMenuStrip.Items.Add(removeItem);
            listView.ContextMenuStrip = listView_contextMenuStrip;

            RefreshMenuStrip();
        }

        public static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                // Get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        void ProcessHosts(string content, bool refreshList)
        {
            hostFileContentBackup = content;
            textBox.Text = content;
            textBox.DeselectAll();
            textBox.SelectionLength = 0;
            if (refreshList)
            {
                List<HostEntry> hosts = NetHelper.ParseHosts(content);
                listView.Items.Clear();
                foreach (HostEntry host in hosts)
                {
                    ListViewItem item = new ListViewItem(host.IPToRedirectTo);
                    item.SubItems.Add(host.UrlToIntercept);
                    listView.Items.Add(item);
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Undo();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text += (string)Clipboard.GetData("Text");
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        private void addEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddEntry addEntry = new AddEntry();
            DialogResult dialogResult = addEntry.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                string currentContent = textBox.Text;
                // Check if a newline is needed
                if (!currentContent.EndsWith(Environment.NewLine))
                {
                    currentContent += Environment.NewLine;
                }
                currentContent += addEntry.hostEntry.ToString();
                //currentContent += Environment.NewLine;
                ProcessHosts(currentContent, true);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText(hostsFilePath, textBox.Text);
        }

        private void refreshListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessHosts(textBox.Text, true);
        }

        private void listView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView.SelectedItems.Count > 0)
                {
                    listView_contextMenuStrip.Show(listView, new Point(e.X, e.Y));
                }
            }
        }

        private void reformatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to reformat the Hosts file? You will see a preview before changes are made.", "Reformat Hosts file", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                string reformattedHostsFile = NetHelper.ReformatHosts(textBox.Text);
                if (new ReformatHosts_Preview(reformattedHostsFile, textBox.Font).ShowDialog() == DialogResult.OK)
                {
                    ProcessHosts(reformattedHostsFile, true);
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshMenuStrip();
            toolStripStatusLabel_SelectedItemOrPosition.Text = "";
            RefreshStatusStrip();
        }

        void RefreshStatusStrip()
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (listView.SelectedItems.Count > 0)
                {
                    toolStripStatusLabel_SelectedItemOrPosition.Text = String.Format("Item {0}", listView.SelectedItems[0].Index);
                }
            }
            else
            {
                //// Get cursor row
                //int row = textBox.GetLineFromCharIndex(textBox.SelectionStart);
                //// Get cursor column
                //int column = textBox.SelectionStart - textBox.GetFirstCharIndexFromLine(row);
//
                //// If content is being selected downwards, show the end of the selection
                //if (textBox.SelectionLength > 0 && textBox.SelectionStart < textBox.Text.Length - 1)
                //{
                //    row = textBox.GetLineFromCharIndex(textBox.SelectionStart + textBox.SelectionLength);
                //    column = textBox.SelectionStart + textBox.SelectionLength - textBox.GetFirstCharIndexFromLine(row);
                //}
//
                //// If a full line is selected, subtract one from the row
                //try
                //{
                //    if (textBox.SelectionLength == textBox.Lines[row - 1].Length)
                //    {
                //        row--;
                //    }
                //}
                //catch
                //{
//
                //}
                //
                ////int index = textBox.SelectionStart;
                ////int line = textBox.GetLineFromCharIndex(index);
//
                ////// Get the column.
                ////int firstChar = textBox.GetFirstCharIndexFromLine(line);
                ////int column = index - firstChar;

                // Get row and column from plain text box

                //toolStripStatusLabel_SelectedItemOrPosition.Text = String.Format("Line: {0}  Col: {1}", row + 1, column + 1);
            }
        }

        void RefreshMenuStrip()
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    editToolStripMenuItem.Enabled = false;
                    break;

                case 1:
                    editToolStripMenuItem.Enabled = true;
                    break;
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = textBox.Font;
            fontDialog.ShowApply = true;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Font = fontDialog.Font;
                Settings.Default.Font = fontDialog.Font;
                Settings.Default.Save();
            }
        }

        private void resetSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Reset application settings? \nThis will restart the application.", "Reset settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ResetSettings = true;
                Application.Restart();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ResetSettings)
            {
                Settings.Default.Reset();
            }
            else
            {
                if (WindowState == FormWindowState.Maximized)
                {
                    Settings.Default.WindowPosition = RestoreBounds.Location;
                    Settings.Default.WindowSize = RestoreBounds.Size;
                    Settings.Default.WindowState = WindowState;
                }
                else if (WindowState == FormWindowState.Normal)
                {
                    Settings.Default.WindowPosition = Location;
                    Settings.Default.WindowSize = Size;
                    Settings.Default.WindowState = WindowState;
                }
                else
                {
                    Settings.Default.WindowPosition = RestoreBounds.Location;
                    Settings.Default.WindowSize = RestoreBounds.Size;
                    Settings.Default.WindowState = WindowState;
                }
            }
            Settings.Default.Save();
        }

        private void rememberWindowLayoutToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            Settings.Default.RememberWindowLayout = rememberWindowLayoutToolStripMenuItem.Checked;
        }

        private void showGridlinesToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
        {
            listView.GridLines = showGridlinesToolStripMenuItem.Checked;
            Settings.Default.ShowGridlines = showGridlinesToolStripMenuItem.Checked;
        }

        private void textBox_SelectionChanged(object sender, EventArgs e)
        {
            RefreshStatusStrip();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Cut();
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshStatusStrip();
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            undoToolStripMenuItem.Enabled = textBox.CanUndo;
        }
    }
}