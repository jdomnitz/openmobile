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
        }

        private void Refresh_Commands()
        {
            this.dataGridView2.DataSource = null;
            _Commands = new SortableBindingList<Command>(BuiltInComponents.Host.CommandHandler.GetCommands(""));
            this.dataGridView2.DataSource = _Commands;
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
            sb.AppendLine(String.Format("{0,-40} {1,-8} {2,-8} {3,-20} Description", "FullName", "ReqPar", "RetVal", "Provider"));
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------");

            foreach (Command command in dataCommands)
            {
                try
                {
                    sb.Append(String.Format("{0,-40} {1,-8} {2,-8} {3,-20} {4}", command.FullName, command.RequiresParameters, command.ReturnsValue, command.Provider, command.Description));
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
                    MessageBox.Show(String.Format("An error has occured while saving the file\n{0}\n\nError:\n{1}", sfDialog.FileName, ex.ToString()));
                }

            }

        }
    }
}
