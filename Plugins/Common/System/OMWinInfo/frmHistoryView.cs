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
    public partial class frmHistoryView : Form
    {
        public enum DataTypes { Commands }      
    
        public frmHistoryView(string header, DataTypes dataType)
        {
            InitializeComponent();

            this.Text = header;

            if (dataType == DataTypes.Commands)
            {
                // Connect to command monitoring
                BuiltInComponents.Host.CommandHandler.CommandExecMonitor += new CommandHandler.CommandMonitorDelegate(CommandHandler_CommandExecMonitor);
            }
        }

        void CommandHandler_CommandExecMonitor(Command command, object[] param, string name)
        {
            string txt = String.Format("[{0}] ", DateTime.Now.ToString("yyyy.MM.dd - HH:mm:ss.fff"));
            if (command != null)
                txt += String.Format("Cmd: {0, -55} | Param: {1}", command, (param != null ? param.ToString() : ""));
            else
                txt += String.Format("ERR : {0, -55} | Param: {1}", name, (param != null ? param.ToString() : ""));

            if (this.InvokeRequired)
                this.BeginInvoke((MethodInvoker)delegate()
                {
                    this.listBox1.Items.Add(txt);
                    this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
                });
            else
            {
                this.listBox1.Items.Add(txt);
                this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
        }
    }
}
