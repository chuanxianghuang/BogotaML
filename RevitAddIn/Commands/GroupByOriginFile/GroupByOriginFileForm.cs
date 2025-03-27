using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAddIn.Commands.GroupByOriginFile
{
    public partial class GroupByOriginFileForm : System.Windows.Forms.Form
    {
        public GroupByOriginFileForm()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Tag = "3D";
            this.DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Tag = "NWC";
            this.DialogResult = DialogResult.OK;
        }
    }
}
