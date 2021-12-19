using System;
using System.Drawing;
using System.Windows.Forms;

namespace HostsFileEditor
{
    public partial class ReformatHosts_Preview : Form
    {
        public ReformatHosts_Preview(string hostsContent_Reformatted, Font previewFont)
        {
            InitializeComponent();
            textBox_preview.Text = hostsContent_Reformatted;
            textBox_preview.Font = previewFont;
        }

        private void ReformatHosts_Preview_Load(object sender, EventArgs e)
        {

        }
    }
}
