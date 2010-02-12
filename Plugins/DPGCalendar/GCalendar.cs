using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile;
using System.Drawing;

namespace DPGCalendar
{
    public sealed class GCalendar:IDataProvider
    {
        static string dataPath;
        static int status = 0;

        void getCal()
        {
            status = 2;
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create("https://www.google.com/accounts/ClientLogin");
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            string data;
            string email = "";
            string pass = "";
            Personal.readInfo();
            email = Collections.personalInfo.googleUsername;
            if (email == "")
            {
                status = -1;
                return;
            }
            pass = Personal.getPassword(Personal.ePassword.google, "GOOGLEPW");
            if (pass == "")
            {
                status = -1;
                return;
            }

            data = "accountType=GOOGLE&Email=" + email + "&Passwd=" + pass + "&service=cl&source=DomnitzSolutions-GoogleSync-0.10";

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
            using (PluginSettings s = new PluginSettings())
                lastModified = s.getSetting("Plugins.DPGCalendar.LastUpdate");
            if (lastModified == "")
                lastModified = DateTime.MinValue.Date.ToString("s");
            HttpWebRequest req2 = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com/calendar/feeds/" + email + "/private/full?updated-min="+lastModified);
            req2.ContentType = "application/x-www-form-urlencoded";
            req2.Method = "GET";
            req2.Headers[HttpRequestHeader.Authorization] = "GoogleLogin " + result.Split('\n')[2].Replace("Auth=", "auth=");

            req2.AllowAutoRedirect = false;

            response = req2.GetResponse();

            req2 = (HttpWebRequest)HttpWebRequest.Create(response.Headers.Get("Location"));
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
            XmlDocument reader = new XmlDocument();
            reader.Load(response.GetResponseStream());
            XmlNodeList l = reader.GetElementsByTagName("entry");
            Calendar cdat=new Calendar();
            cdat.beginWrite();
            #region parse
            foreach (XmlNode n in l)
            {
                calendarEvent e = new calendarEvent();
                foreach (XmlNode n2 in n.ChildNodes)
                {
                    switch (n2.LocalName)
                    {
                        case "category":
                            //e.type = n2.ChildNodes[0].Value;
                            break;
                        case "title":
                            e.title = n2.ChildNodes[0].Value;
                            break;
                        case "content":
                            if (n2.HasChildNodes == true)
                                e.description = n2.ChildNodes[0].Value;
                            break;
                        case "when":
                            TimeSpan t = TimeSpan.FromSeconds(0);
                            switch (n2.ChildNodes[0].Attributes[0].LocalName)
                            {
                                case "minutes":
                                    t = TimeSpan.FromMinutes(Convert.ToDouble(n2.ChildNodes[0].Attributes[0].Value));
                                    break;
                                case "hours":
                                    t = TimeSpan.FromHours(Convert.ToDouble(n2.ChildNodes[0].Attributes[0].Value));
                                    break;
                                case "days":
                                    t = TimeSpan.FromDays(Convert.ToDouble(n2.ChildNodes[0].Attributes[0].Value));
                                    break;
                                case "weeks":
                                    t = TimeSpan.FromDays(Convert.ToDouble(n2.ChildNodes[0].Attributes[0].Value) * 7);
                                    break;
                            }
                            e.startTime = DateTime.Parse(n2.Attributes["startTime"].Value);
                            e.endTime = DateTime.Parse(n2.Attributes["endTime"].Value);
                            e.reminder =e.startTime - t;
                            break;
                        case "where":
                            if (n2.Prefix == "gd")
                                e.location = n2.Attributes["valueString"].Value;
                            else
                                e.locationLatLong = parse(n2.ChildNodes[0].ChildNodes[0].ChildNodes[0].Value);
                            break;
                    }
                }
                cdat.writeNext(e);
            }
            #endregion
            cdat.Dispose();
            response.Close();
            responseStream.Close();
            using (PluginSettings s = new PluginSettings())
                s.setSetting("Plugins.DPGCalendar.LastUpdate", DateTime.Now.ToUniversalTime().ToString("s"));
            status = 1;
        }

        private PointF parse(string p)
        {
            string[] part = p.Split(new char[] { ' ' });
            if (part.Length != 2)
                return new PointF();
            return new PointF(float.Parse(part[0]), float.Parse(part[1]));
        }

        #region IDataProvider Members

        public bool refreshData()
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                status = 0;
                OpenMobile.Threading.TaskManager.QueueTask(getCal, OpenMobile.priority.MediumHigh);
                return true;
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

        public int updaterStatus()
        {
            return status;
        }

        public string pluginType()
        {
            return "Calendar";
        }

        #endregion

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
            get { return "DPGCalendar"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Google Calendar Data Provider"; }
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
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void host_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.connectedToInternet)
                refreshData();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
