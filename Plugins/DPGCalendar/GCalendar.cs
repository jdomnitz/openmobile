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
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Plugin;
using System.Reflection;
using System.Threading;

namespace DPGCalendar
{
    public sealed class GCalendar:IDataProvider
    {
        static string dataPath;
        static int status = 0;

        void getCal()
        {
            Thread.Sleep(5000);
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
                theHost.execute(eFunction.settingsChanged, "Calendar");
                return;
            }
            pass = Personal.getPassword(Personal.ePassword.google, "GOOGLEPW");
            if (pass == "")
            {
                status = -1;
                theHost.execute(eFunction.settingsChanged, "Calendar");
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
                theHost.execute(eFunction.settingsChanged, "Calendar");
                return;
            }
            StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
            string result = readStream.ReadToEnd();
            string lastModified;
            lastModified = lastUpdated.ToUniversalTime().ToString("s");
            if (lastModified == "")
                lastModified = DateTime.MinValue.Date.ToString("s");
            HttpWebRequest req2;
            if (lastUpdated==DateTime.MinValue)
                req2 = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com/calendar/feeds/" + email + "/private/full");
            else
                req2 = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com/calendar/feeds/" + email + "/private/full?updated-min=" + lastModified);

            req2.ContentType = "application/x-www-form-urlencoded";
            req2.Method = "GET";
            req2.Headers[HttpRequestHeader.Authorization] = "GoogleLogin " + result.Split('\n')[2].Replace("Auth=", "auth=");
            req2.AllowAutoRedirect = false;
            try
            {
                response = req2.GetResponse();
            }
            catch (System.Net.WebException e)
            {
                status = -1;
                theHost.execute(eFunction.settingsChanged, "Calendar");
                return;
            }
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
                theHost.execute(eFunction.settingsChanged, "Calendar");
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
            theHost.execute(eFunction.settingsChanged, "Calendar");
        }

        public DateTime lastUpdated
        {
            get
            {
                DateTime ret;
                using (PluginSettings s = new PluginSettings())
                    if (DateTime.TryParse(s.getSetting("Plugins.DPGCalendar.LastUpdate"), out ret))
                        return ret;
                    else
                        return DateTime.MinValue;
            }
        }

        private PointF parse(string p)
        {
            string[] part = p.Split(new char[] { ' ' });
            if (part.Length != 2)
                return new PointF();
            return new PointF(float.Parse(part[0]), float.Parse(part[1]));
        }

        public static bool SetAllowUnsafeHeaderParsing20()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                      BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #region IDataProvider Members

        public bool refreshData()
        {
            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                if ((DateTime.Now - lastUpdated) > TimeSpan.FromMinutes(30))
                {
                    status = 0;
                    OpenMobile.Threading.TaskManager.QueueTask(getCal, OpenMobile.ePriority.MediumHigh);
                    return true;
                }
                else
                {
                    status = 1;
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
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            dataPath = host.DataPath;
            theHost = host;
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            host.OnPowerChange += new PowerEvent(host_OnPowerChange);
            bool b=SetAllowUnsafeHeaderParsing20();
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void host_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
            {
                refreshData();
            }
        }

        void host_OnSystemEvent(OpenMobile.eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.connectedToInternet)
            {
                refreshData();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
