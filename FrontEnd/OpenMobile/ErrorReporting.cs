using System;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.IO;

namespace OpenMobile
{
    public partial class ErrorReporting : Form
    {
        public ErrorReporting(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
            button1.Select();
            try
            {
                Clipboard.SetText(text);
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string result=HttpPost("http://openmobile.sourceforge.net/main/BugReports/index.php","REPORT="+textBox1.Text.Replace("\r\n","|"));
            if (result == "File saved")
                MessageBox.Show("Report Submitted Successfully!  Thank you for helping to improve openMobile.");
            else
                MessageBox.Show("Unable to submit error report");
        }

        string HttpPost (string uri, string parameters)
        { 
           // parameters: name1=value1&name2=value2	
           WebRequest webRequest = WebRequest.Create (uri);
           //string ProxyString = 
           //   System.Configuration.ConfigurationManager.AppSettings
           //   [GetConfigKey("proxy")];
           //webRequest.Proxy = new WebProxy (ProxyString, true);
           //Commenting out above required change to App.Config
           webRequest.ContentType = "application/x-www-form-urlencoded";
           webRequest.Method = "POST";
           byte[] bytes = Encoding.ASCII.GetBytes (parameters);
           Stream os = null;
           try
           { // send the Post
              webRequest.ContentLength = bytes.Length;   //Count bytes to send
              os = webRequest.GetRequestStream();
              os.Write (bytes, 0, bytes.Length);         //Send it
           }
           catch (WebException ex)
           {
               return null;
           }
           finally
           {
              if (os != null)
              {
                 os.Close();
              }
           }
         
           try
           { // get the response
              WebResponse webResponse = webRequest.GetResponse();
              if (webResponse == null) 
                 { return null; }
              StreamReader sr = new StreamReader (webResponse.GetResponseStream());
              return sr.ReadToEnd ().Trim ();
           }
           catch (WebException ex)
           {
               return null;
           }
           return null;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            button3.Select();
        }
    }
}
