using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenMobile.Data;
using System.IO;

namespace OMWinInfo
{
    public partial class frmDetails : Form
    {
        /// <summary>
        /// A dataset
        /// </summary>
        private class DataSet
        {
            /// <summary>
            /// Key
            /// </summary>
            public string Key
            {
                get
                {
                    return this._Key;
                }
                set
                {
                    if (this._Key != value)
                    {
                        this._Key = value;
                    }
                }
            }
            private string _Key;

            /// <summary>
            /// Value
            /// </summary>
            public object Value
            {
                get
                {
                    return this._Value;
                }
                set
                {
                    if (this._Value != value)
                    {
                        this._Value = value;
                    }
                }
            }
            private object _Value;
        
        }

        private List<DataSet> _DataSet;
        private DataSource _DataSource;

        public frmDetails()
        {
            InitializeComponent();
        }

        public void Show(IWin32Window owner, DataSource dataSource)
        {
            _DataSource = dataSource;
            this.lblDataSourceName.Text = dataSource.FullName;
            
            var value = dataSource.Value;

            // Display dictionary values if present
            if (value is Dictionary<string, object>)
            {
                var valueDictionary = value as Dictionary<string, object>;
                _DataSet = valueDictionary.AsEnumerable().Select(x => new DataSet { Key = x.Key, Value = x.Value }).ToList();
                this.dataGridView1.DataSource = _DataSet;
                button2.Visible = true;
            }
            else
            {
                this.dataGridView1.DataSource = dataSource.Value;
                button2.Visible = false;
            }

            this.propertyGrid1.SelectedObject = dataSource;
            base.Show(owner);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmDetails_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            var dataSources = _DataSet.OrderBy(x => x.Key);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("OpenMobile DataSource details export - {0} - DataSource: {1}", DateTime.Now, _DataSource.FullName));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");
            sb.AppendLine(String.Format("{0,-40} {1}", "Key", "Value"));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");

            foreach (var data in dataSources)
            {
                try
                {
                    sb.Append(String.Format("{0,-40} {1}", data.Key, data.Value));
                    sb.AppendLine();
                }
                catch { }

            }

            SaveFileDialog sfDialog = new SaveFileDialog();
            sfDialog.FileName = String.Format("OpenMobile_DataSourceDetails_{0}.txt", _DataSource.FullName);
            sfDialog.OverwritePrompt = true;
            if (sfDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfDialog.FileName, sb.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("An error has occurred while saving the file\n{0}\n\nError:\n{1}", sfDialog.FileName, ex.ToString()));
                }

            }
        }
    }
}
