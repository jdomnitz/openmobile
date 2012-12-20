using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace OMWinInfo
{
    public partial class frmWinInfo : Form
    {
        List<DataSource> datasources = null;

        public frmWinInfo()
        {
            InitializeComponent();

            datasources = BuiltInComponents.Host.DataHandler.GetDataSources("");

            this.dataGridView1.DataSource = datasources;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }
    }
}
