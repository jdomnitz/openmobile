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
using System.Collections;
using System.Reflection;
using System.Linq;
using System.IO;

namespace OMWinInfo
{
    public partial class frmWinInfo : Form
    {
        private class SortableBindingList<T> : BindingList<T>
        {
            private ArrayList sortedList;
            private ArrayList unsortedItems;
            private bool isSortedValue;

            public SortableBindingList()
            {
            }

            public SortableBindingList(IList<T> list)
            {
                foreach (object o in list)
                {
                    this.Add((T)o);
                }
            }

            protected override bool SupportsSearchingCore
            {
                get
                {
                    return true;
                }
            }

            protected override int FindCore(PropertyDescriptor prop, object key)
            {
                PropertyInfo propInfo = typeof(T).GetProperty(prop.Name);
                T item;

                if (key != null)
                {
                    for (int i = 0; i < Count; ++i)
                    {
                        item = (T)Items[i];
                        if (propInfo.GetValue(item, null).Equals(key))
                            return i;
                    }
                }
                return -1;
            }

            public int Find(string property, object key)
            {
                PropertyDescriptorCollection properties =
                    TypeDescriptor.GetProperties(typeof(T));
                PropertyDescriptor prop = properties.Find(property, true);

                if (prop == null)
                    return -1;
                else
                    return FindCore(prop, key);
            }

            protected override bool SupportsSortingCore
            {
                get { return true; }
            }


            protected override bool IsSortedCore
            {
                get { return isSortedValue; }
            }

            ListSortDirection sortDirectionValue;
            PropertyDescriptor sortPropertyValue;

            protected override void ApplySortCore(PropertyDescriptor prop,
                ListSortDirection direction)
            {
                sortedList = new ArrayList();

                Type interfaceType = prop.PropertyType.GetInterface("IComparable");

                if (interfaceType == null && prop.PropertyType.IsValueType)
                {
                    Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

                    if (underlyingType != null)
                    {
                        interfaceType = underlyingType.GetInterface("IComparable");
                    }
                }

                if (interfaceType != null)
                {
                    sortPropertyValue = prop;
                    sortDirectionValue = direction;

                    IEnumerable<T> query = base.Items;
                    if (direction == ListSortDirection.Ascending)
                    {
                        query = query.OrderBy(i => prop.GetValue(i));
                    }
                    else
                    {
                        query = query.OrderByDescending(i => prop.GetValue(i));
                    }
                    int newIndex = 0;
                    foreach (object item in query)
                    {
                        this.Items[newIndex] = (T)item;
                        newIndex++;
                    }
                    isSortedValue = true;
                    this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

                }
                else
                {
                    //throw new NotSupportedException("Cannot sort by " + prop.Name +
                    //    ". This" + prop.PropertyType.ToString() +
                    //    " does not implement IComparable");
                }
            }

            protected override void RemoveSortCore()
            {
                int position;
                object temp;

                if (unsortedItems != null)
                {
                    for (int i = 0; i < unsortedItems.Count; )
                    {
                        position = this.Find("LastName",
                            unsortedItems[i].GetType().
                            GetProperty("LastName").GetValue(unsortedItems[i], null));
                        if (position > 0 && position != i)
                        {
                            temp = this[i];
                            this[i] = this[position];
                            this[position] = (T)temp;
                            i++;
                        }
                        else if (position == i)
                            i++;
                        else
                            unsortedItems.RemoveAt(i);
                    }
                    isSortedValue = false;
                    OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
                }
            }

            public void RemoveSort()
            {
                RemoveSortCore();
            }
            protected override PropertyDescriptor SortPropertyCore
            {
                get { return sortPropertyValue; }
            }

            protected override ListSortDirection SortDirectionCore
            {
                get { return sortDirectionValue; }
            }

        }

        //List<DataSource> datasources = null;
        SortableBindingList<DataSource> _DataSources = null;
        SortableBindingList<Command> _Commands = null;

        public frmWinInfo()
        {
            InitializeComponent();

            //datasources = BuiltInComponents.Host.DataHandler.GetDataSources("");
            //this.dataGridView1.DataSource = datasources;
            Refresh_DataSources();
            Refresh_Commands();
        }

        private void Refresh_DataSources()
        {
            this.dataGridView1.DataSource = null;
            _DataSources = new SortableBindingList<DataSource>(BuiltInComponents.Host.DataHandler.GetDataSources(""));
            this.dataGridView1.DataSource = _DataSources;
            this.dataGridView1.Sort(this.dataGridView1.Columns["FullName"], ListSortDirection.Ascending);
            this.dataGridView1.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns["FullName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.DataError += new DataGridViewDataErrorEventHandler(dataGridView1_DataError);
        }

        void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void Refresh_Commands()
        {
            this.dataGridView2.DataSource = null;
            _Commands = new SortableBindingList<Command>(BuiltInComponents.Host.CommandHandler.GetCommands(""));
            this.dataGridView2.DataSource = _Commands;

            foreach (Command command in _Commands)
	        {
                this.cmbCommand.Items.Add(command.FullName);
	        }

            this.dataGridView2.Sort(this.dataGridView2.Columns["FullName"], ListSortDirection.Ascending);
            this.dataGridView2.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView2.Columns["FullName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            Refresh_DataSources();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            var dataSources = _DataSources.OrderBy(x => x.FullName);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("OpenMobile DataSource export - {0}", DateTime.Now));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");
            //sb.AppendLine("FullName                                DataType       Provider                       Description / sample data");
            sb.AppendLine(String.Format("{0,-40} {1,-15} {2,-20} Description / sample data", "FullName", "DataType", "Provider"));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");

            foreach (DataSource datasource in dataSources)
            {
                try
                {
                    sb.Append(String.Format("{0,-40} {1,-15} {2,-20} {3}", datasource.FullName, datasource.DataType, datasource.Provider, datasource.Description));
                    if (datasource.Valid)
                        sb.Append(String.Format(" | Sample data (Raw / formatted): {0} / {1}", datasource.Value, datasource.FormatedValue));
                    sb.AppendLine();
                }
                catch { }

            }

            SaveFileDialog sfDialog = new SaveFileDialog();
            sfDialog.FileName = "OpenMobile_DataSources.txt";
            sfDialog.OverwritePrompt = true;
            if (sfDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfDialog.FileName, sb.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("An error has occured while saving the file\n{0}\n\nError:\n{1}", sfDialog.FileName, ex.ToString()));
                }

            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Refresh_Commands();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dataCommands = _Commands.OrderBy(x => x.FullName);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("OpenMobile Commands export - {0}", DateTime.Now));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");
            //sb.AppendLine("FullName                                DataType       Provider                       Description / sample data");
            sb.AppendLine(String.Format("{0,-60} {1,-8} {2,-8} {3,-20} Description", "FullName", "ReqPar", "RetVal", "Provider"));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");

            foreach (Command command in dataCommands)
            {
                try
                {
                    sb.Append(String.Format("{0,-60} {1,-8} {2,-8} {3,-20} {4}", command.FullName, command.RequiresParameters, command.ReturnsValue, command.Provider, command.Description));
                    sb.AppendLine();
                }
                catch { }

            }

            SaveFileDialog sfDialog = new SaveFileDialog();
            sfDialog.FileName = "OpenMobile_Commands.txt";
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

        private void btnExecCommand_Click(object sender, EventArgs e)
        {
            if (cmbCommand.Text.Contains(Command.CommandSeparator))
            {   // Multiple commands
                BuiltInComponents.Host.CommandHandler.ExecuteCommand(cmbCommand.Text);
            }
            else
            {   // Single command
                Command cmd = BuiltInComponents.Host.CommandHandler.GetCommand(cmbCommand.Text);
                if (cmd != null)
                {
                    object result = null;
                    if (cmd.RequiresParameters)
                    {
                        object[] param = null;
                        frmCmdParams frmParam = new frmCmdParams();
                        if (frmParam.ShowDialog(this, cmd, out param) == System.Windows.Forms.DialogResult.OK)
                        {
                            result = BuiltInComponents.Host.CommandHandler.ExecuteCommand(cmbCommand.Text, param.ToArray());
                        }
                    }
                    else
                    {
                        result = BuiltInComponents.Host.CommandHandler.ExecuteCommand(cmbCommand.Text);
                    }

                    if (result is string)
                    {
                        var resultString = result as string;
                        if (!string.IsNullOrWhiteSpace(resultString))
                        {
                            MessageBox.Show(String.Format("Command\r\n\r\n'{0}'\r\n\r\nreturned:\r\n\r\n{1}", cmd, resultString), "Command reply", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
                this.cmbCommand.Text = (string)dataGridView2.SelectedRows[0].Cells["FullName"].Value;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                this.cmdDataSource.Text = (string)dataGridView1.SelectedRows[0].Cells["FullName"].Value;

                DataSource datasource = BuiltInComponents.Host.DataHandler.GetDataSource(this.cmdDataSource.Text);
                if (datasource != null)
                {
                    if (datasource.Value is OpenMobile.Graphics.OImage)
                    {
                        OpenMobile.Graphics.OImage img = ((OpenMobile.Graphics.OImage)datasource.Value);
                        if (img.image != null)
                            pictureBox1.Image = img.image;
                    }
                    else if (datasource.Value is OpenMobile.imageItem)
                    {
                        imageItem img = ((OpenMobile.imageItem)datasource.Value);
                        if (img.image != null)
                            pictureBox1.Image = img.image.image;
                    }
                    else
                        pictureBox1.Image = null;

                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DataSource datasource = BuiltInComponents.Host.DataHandler.GetDataSource(this.cmdDataSource.Text);
            if (datasource != null)
            {
                datasource.Value = this.txtDataSourceValue.Text;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmHistoryView frmCmdHistory = new frmHistoryView("Commands history", frmHistoryView.DataTypes.Commands);
            frmCmdHistory.Show();
        }

        private Rectangle _pictureBox1_OrgLocation;
        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            if (pictureBox1.Width == groupBox1.Width)
            {
                // Resize imagebox to show large image
                pictureBox1.Left = _pictureBox1_OrgLocation.Left;
                pictureBox1.Top = _pictureBox1_OrgLocation.Top;
                pictureBox1.Width = _pictureBox1_OrgLocation.Width;
                pictureBox1.Height = _pictureBox1_OrgLocation.Height;
            }
            else
            {
                _pictureBox1_OrgLocation = new Rectangle(pictureBox1.Left, pictureBox1.Top, pictureBox1.Width, pictureBox1.Height);

                // Resize imagebox to show large image
                pictureBox1.Left = groupBox1.Left;
                pictureBox1.Top = groupBox1.Top;
                pictureBox1.Width = groupBox1.Width;
                pictureBox1.Height = groupBox1.Height;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dataGridView1.Columns[3].Width--;
            dataGridView1.Columns[3].Width++;
            dataGridView1.Columns[4].Width--;
            dataGridView1.Columns[4].Width++;
        }

        private void chkAuto_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = chkAuto.Checked;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                this.cmdDataSource.Text = (string)dataGridView1.SelectedRows[0].Cells["FullName"].Value;

                DataSource datasource = BuiltInComponents.Host.DataHandler.GetDataSource(this.cmdDataSource.Text);
                if (datasource != null)
                {
                    var formDetails = new frmDetails();
                    formDetails.Show(this, datasource);
                }
            }
        }
    }
}
