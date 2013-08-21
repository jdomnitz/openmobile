using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenMobile;

namespace OMWinInfo
{
    public partial class frmCmdParams : Form
    {
        public frmCmdParams()
        {
            InitializeComponent();

            
        }

        BindingList<string> LocalParam = new BindingList<string>();
        public new DialogResult ShowDialog(IWin32Window owner, Command cmd, out object[] param)
        {
            //BindingList<string> LocalParam = new BindingList<string>();
            //for (int i = 0; i < cmd.RequiredParamCount; i++)
            //    LocalParam.Add("");

            this.label1.Text = cmd.Description;
            this.label2.Text = String.Format("This command requires {0} parameters", cmd.RequiredParamCount);
            this.dataGridView1.DataSource = LocalParam;
            DialogResult result =  base.ShowDialog(owner);
            param = LocalParam.ToArray();
            return result;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            DataGridViewCell cell = dataGridView1.SelectedCells[0];
            dataGridView1.CurrentCell = cell;
            dataGridView1.BeginEdit(true);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = dataGridView1.SelectedCells[0];
            dataGridView1.CurrentCell = cell;
            dataGridView1.BeginEdit(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LocalParam.Add(textBox1.Text);
        }
    }
}
