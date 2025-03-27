using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace RevitAddIn.Commands.SelectByOriginID
{
    public partial class SelectByOriginIDForm : Form
    {
        public SelectByOriginIDForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("请填写需查找的OriginID。", "向日葵", MessageBoxButtons.OK);
            }
            else
            {
               AppSettings.Default.LastInput = textBox2.Text;
                this.Close();
                this.DialogResult = DialogResult.OK;
            }
        }

        private void SelectByOriginIDForms_Load(object sender, EventArgs e)
        {
            textBox2.Text = AppSettings.Default.LastInput;
        }
    }
}
