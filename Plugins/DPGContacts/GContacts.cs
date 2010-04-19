/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Plugin;

namespace DPGContacts
{
    public sealed class GContacts:IDataProvider
    {
        private static string dataPath;
        void getContacts()
        {
            status = 2;
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create("https://www.google.com/accounts/ClientLogin");
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            string data;
            string email = "";
            string pass = "";
            Personal.readInfo();
            email=Collections.personalInfo.googleUsername;
            if (email == "")
            {
                status = -1;
                return;
            }
            pass = Personal.getPassword(Personal.ePassword.google, "GOOGLEPW");
            if (pass== "")
            {
                status = -1;
                return;
            }
            data = "accountType=GOOGLE&Email=" + email + "&Passwd=" + pass + "&service=cp&source=DomnitzSolutions-GoogleSync-0.10";

            req.ContentLength = data.Length;
            Stream sReq = req.GetRequestStream();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] bytes = encoding.GetBytes(data);
            sReq.Write(bytes, 0, bytes.Length);
            WebResponse response = null;
            Stream responseStream = null;
            try
            {
                response = req.GetResponse();
                responseStream = response.GetResponseStream();
            }
            catch (System.Net.WebException)
            {
                status = -1;
                return;
            }

            StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
            string result = readStream.ReadToEnd();
            string lastModified;
            using(PluginSettings s=new PluginSettings())
                lastModified= s.getSetting("Plugins.DPGContacts.LastUpdate");
            if (lastModified=="")
                lastModified=DateTime.MinValue.Date.ToString("s");
            HttpWebRequest req2 = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com/m8/feeds/contacts/" + email + "/full?max-results=10000&updated-min="+lastModified);
            req2.ContentType = "application/x-www-form-urlencoded";
            req2.Method = "GET";
            req2.Headers[HttpRequestHeader.Authorization] = "GoogleLogin " + result.Split('\n')[2].Replace("Auth=", "auth=");

            req2.AllowAutoRedirect = false;
            try
            {
                response = req2.GetResponse();
                responseStream = response.GetResponseStream();
            }
            catch (System.Net.WebException)
            {
                status = -1;
                return;
            }
            if (Directory.Exists(OpenMobile.Path.Combine(dataPath, "Contacts")) == false)
                Directory.CreateDirectory(OpenMobile.Path.Combine(dataPath, "Contacts"));
            XmlDocument reader = new XmlDocument();
            reader.Load(response.GetResponseStream());
            XmlNodeList l = reader.GetElementsByTagName("entry");
            Contacts cdat = new Contacts();
            cdat.beginWrite();
            foreach (XmlNode n in l)
            {
                contact c = new contact();

                foreach (XmlNode n2 in n.ChildNodes)
                {
                    switch (n2.LocalName)
                    {
                        case "title":
                            if (n2.HasChildNodes == false)
                                continue;
                            c.name = n2.ChildNodes[0].Value;
                            break;
                        case "content":
                            if (n2.HasChildNodes == true)
                                c.comments = n2.ChildNodes[0].Value;
                            break;
                        case "link":
                            if (n2.Attributes["rel"].Value == "http://schemas.google.com/contacts/2008/rel#photo")
                            {
                                string id=n.ChildNodes[0].InnerText;
                                id=id.Substring(id.LastIndexOf('/')+1);
                                c.imageURL=OpenMobile.Path.Combine(dataPath, "Contacts", id+".png");
                                OpenMobile.Net.Network.downloadFile(n2.Attributes["href"].Value, c.imageURL, "Authorization: GoogleLogin " + result.Split('\n')[2].Replace("Auth=", "auth="));
                            }
                            break;
                        case "email":
                            c.email = n2.Attributes["address"].Value;
                            break;
                        case "phoneNumber":
                            if (n2.Attributes[0].Value.Contains("#mobile"))
                                if (c.cell1 == null)
                                    c.cell1 = n2.ChildNodes[0].Value;
                                else
                                    c.cell2 = n2.ChildNodes[0].Value;
                            else if (n2.Attributes[0].Value.Contains("#home"))
                                c.home = n2.ChildNodes[0].Value;
                            else if (n2.Attributes[0].Value.Contains("#fax"))
                                c.fax = n2.ChildNodes[0].Value;
                            else if (n2.Attributes[0].Value.Contains("#work"))
                                if (c.work1 == null)
                                    c.work1 = n2.ChildNodes[0].Value;
                                else
                                    c.work2 = n2.ChildNodes[0].Value;
                            break;
                    }
                }
                cdat.writeNext(c);
            }
            cdat.Dispose();
            response.Close();
            responseStream.Close();
            using (PluginSettings s = new PluginSettings())
                s.setSetting("Plugins.DPGContacts.LastUpdate", DateTime.Now.ToString("s"));
            status = 1;
        }

        public bool refreshData()
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                string dat = "";
                using (PluginSettings s = new PluginSettings())
                    dat = s.getSetting("Plugins.DPGContacts.LastUpdate");
                if ((dat == "") || ((DateTime.Now - DateTime.Parse(dat)).Minutes > 60))
                {
                    status = 0;
                    OpenMobile.Threading.TaskManager.QueueTask(getContacts, OpenMobile.ePriority.Normal);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                status = -1;
                return false;
            }
        }

        public bool refreshData(string arg)
        {
            return refreshData();
        }

        public bool refreshData(string arg1, string arg2)
        {
            return refreshData();
        }
        static int status;

        public int updaterStatus()
        {
            return status;
        }

        public string pluginType()
        {
            return "Contacts";
        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "DPGContacts"; }
        }

        public float pluginVersion
        {
            get { return 0.2F; }
        }

        public string pluginDescription
        {
            get { return "Google Contacts Provider"; }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            dataPath = host.DataPath;
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnPowerChange += new PowerEvent(host_OnPowerChange);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void host_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
            {
                string dat = "";
                using (PluginSettings s = new PluginSettings())
                    dat = s.getSetting("Plugins.DPGContacts.LastUpdate");
                if ((dat == "") || ((DateTime.Now - DateTime.Parse(dat)).Minutes > 60))
                    refreshData();
            }
        }

        void host_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.connectedToInternet)
            {
                string dat = "";
                using (PluginSettings s = new PluginSettings())
                    dat=s.getSetting("Plugins.DPGContacts.LastUpdate");
                if ((dat=="")||((DateTime.Now- DateTime.Parse(dat)).Minutes>60))
                    refreshData();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
