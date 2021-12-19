using HostsFileEditor;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hosts_File_Editor
{
    public partial class AddEntry : Form
    {
        public string UrlToRedirect;
        public string TargetIP;
        public HostEntry hostEntry;

        public AddEntry()
        {
            InitializeComponent();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox_requestURL.Text))
            {
                if (!NetHelper.ValidateIP(textBox_IP.Text))
                {
                    MessageBox.Show("The IP address entered is invalid.", "IP invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    TargetIP = textBox_IP.Text;
                    UrlToRedirect = textBox_requestURL.Text;
                    hostEntry = new HostEntry(TargetIP, UrlToRedirect);
                    this.DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageBox.Show("You must enter the URL to be redirected.", "URL required", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox_IP_TextChanged(object sender, EventArgs e)
        {
            //IPAddress IP;
            //if (!IPAddress.TryParse(textBox_IP.Text, out IP))
            if (!NetHelper.ValidateIP(textBox_IP.Text))
            {
                textBox_IP.ForeColor = Color.Red;
            }
            else
            {
                textBox_IP.ForeColor = DefaultForeColor;
            }
        }

        private void AddEntry_Load(object sender, EventArgs e)
        {

        }
    }
}