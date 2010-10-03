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
using System.Collections.Generic;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Email;
using OpenMobile.Plugin;
using OpenMobile.Threading;

namespace DPEmail
{
    public sealed class DPEmail:IDataProvider
    {
        int status = 0;
        private void updateEmail()
        {
            status = 2;
            try
            {
                Messages msg = new Messages();
                if (!msg.beginWriteMessages())
                {
                    status = -1;
                    theHost.execute(eFunction.dataUpdated, "DPEmail");
                    return;
                }
                PluginSettings s=new PluginSettings();
                List<EmailMessage> list = email.getMessages(Credentials.getCredential("Email Address"), Credentials.getCredential("Email Password"), s.getSetting("Email.InboundServer"), getPort(s.getSetting("Email.ServerType")));
                s.Dispose();
                foreach (EmailMessage em in list)
                {
                    Messages.message tmp = new Messages.message();
                    tmp.content = (em.TextBody==null) ? "" : em.TextBody;
                    tmp.subject = em.Subject;
                    tmp.toName = em.Recipient;
                    tmp.fromAddress = em.SenderEmail;
                    tmp.fromName = em.Sender;
                    tmp.messageReceived = em.Received;
                    if ((em.Flags & Flags.Important) == Flags.Important)
                        tmp.messageFlags |= Messages.flags.Important;
                    if ((em.Flags & Flags.Read) == Flags.Read)
                        tmp.messageFlags |= Messages.flags.Read;
                    if ((em.Flags & Flags.Spam) == Flags.Spam)
                        tmp.messageFlags |= Messages.flags.Spam;
                    if (em.attachments > 0)
                        tmp.messageFlags |= Messages.flags.HasAttachment;
                    tmp.sourceName = "Email";
                    tmp.guid = em.guid;
                    msg.writeNext(tmp);
                }
                msg.Close();
                status = 1;
                using (PluginSettings setting = new PluginSettings())
                    setting.setSetting("Plugins.DPEmail.LastUpdate", DateTime.Now.ToString());
                theHost.execute(eFunction.dataUpdated, "DPEmail");
            }
            catch (Exception)
            {
                status = -1;
                theHost.execute(eFunction.dataUpdated, "DPEmail");
            }
        }

        private int getPort(string p)
        {
            switch (p)
            {
                case "0":
                    return 110;
                case "1":
                    return 995;
                case "2":
                    return 143;
                default:
                    return email.getServerInfo(Credentials.getCredential("Email Address")).inboundPort;
            }
        }
        public bool refreshData()
        {

            if (OpenMobile.Net.Network.IsAvailable == true)
            {
                using (PluginSettings setting = new PluginSettings())
                {
                    if ((DateTime.Now - lastUpdated) < TimeSpan.FromMinutes(30))
                    {
                        status = 1;
                        return false;
                    }
                }
                status = 0;
                TaskManager.QueueTask(updateEmail, ePriority.MediumLow, "Sync Email");
                return true;
            }
            else
            {
                status = -1;
                theHost.execute(eFunction.dataUpdated, "DPEmail");
                return false;
            }
        }

        public bool refreshData(string arg)
        {
            return false;
        }

        public bool refreshData(string arg1, string arg2)
        {
            return false;
        }

        public int updaterStatus()
        {
            return status;
        }

        public DateTime lastUpdated
        {
            get
            {
                DateTime ret;
                using (PluginSettings s = new PluginSettings())
                    if (DateTime.TryParse(s.getSetting("Plugins.DPEmail.LastUpdate"), out ret))
                        return ret;
                    else
                        return DateTime.MinValue;
            }
        }

        public string pluginType()
        {
            return "Email";
        }

        #region IBasePlugin Members
        EmailClient email;
        ServerInfo info;
        IPluginHost theHost;
        public eLoadStatus initialize(IPluginHost host)
        {
            host.OnPowerChange += new PowerEvent(host_OnPowerChange);
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            theHost = host;
            email = new EmailClient();
            Messages.newOutboundMessage += new Messages.newMessage(Messages_newOutboundMessage);
            return eLoadStatus.LoadSuccessful;
        }

        void Messages_newOutboundMessage(Messages.message msg)
        {
            sendMessage(ref msg);
            using (Messages m = new Messages())
            {
                m.beginWriteMessages();
                m.writeNext(msg);
            }
        }

        private void sendMessage(ref Messages.message msg)
        {
            lock (this)
            {
                EmailMessage em=new EmailMessage();
                em.guid=msg.guid;
                em.Recipient=msg.toName;
                em.Sender=msg.fromName;
                em.SenderEmail=msg.fromAddress;
                em.Subject=msg.subject;
                em.TextBody=msg.content;
                if (email.sendMessage(em,Credentials.getCredential("Email Address"),Credentials.getCredential("Email Password")))
                    msg.messageFlags |= Messages.flags.Sent;
            }
        }

        void host_OnSystemEvent(eFunction function, string arg1, string arg2, string arg3)
        {
            if (function == eFunction.connectedToInternet)
                refreshData();
        }

        void host_OnPowerChange(ePowerEvent type)
        {
            if (type == ePowerEvent.SystemResumed)
                    refreshData();
        }
        public Settings settings;
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("Account Settings");
                List<string> options=new List<string>(new string[]{"POP Server","Secure POP Server","IMAP Server","Secure IMAP Server"});
                List<string> values=new List<string>(new string[]{"0","1","2","3"});
                settings.Add(new Setting(SettingTypes.Text,"Email.Username","","Email",Credentials.getCredential("Email Address")));
                settings.Add(new Setting(SettingTypes.Password, "Email.Password", "", "Password",Credentials.getCredential("Email Password")));
                using (PluginSettings s = new PluginSettings())
                {
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Email.ServerType", "Server Type", "", options, values, s.getSetting("Email.ServerType")));
                    settings.Add(new Setting(SettingTypes.Text, "Email.InboundServer", "", "Inbound Server",s.getSetting("Email.InboundServer")));
                    settings.Add(new Setting(SettingTypes.Text, "Email.OutboundServer", "", "Outbound Server",s.getSetting("Email.OutboundServer")));
                    settings.Add(new Setting(SettingTypes.MultiChoice, "Email.SecureSMTP", "", "Secure Outbound", Setting.BooleanList, Setting.BooleanList,s.getSetting("Email.SecureSMTP")));
                }
                settings.OnSettingChanged += new SettingChanged(settings_OnSettingChanged);
            }
            return settings;
        }

        void settings_OnSettingChanged(Setting setting)
        {
            if (setting.Name == "Email.Username")
            {
                if ((setting.Value == null) || (setting.Value.Length < 2))
                    return;
                info = email.getServerInfo(setting.Value);
                load();
                Credentials.setCredential("Email Address", setting.Value);
            }
            else if (setting.Name == "Email.Password")
            {
                Credentials.setCredential("Email Password", setting.Value);
            }
            else
                using (PluginSettings s = new PluginSettings())
                    s.setSetting(setting.Name, setting.Value);
        }

        private void load()
        {
            foreach (Setting s in settings)
            {
                if (s.Name == "Email.ServerType")
                {
                    switch (info.inboundPort)
                    {
                        case 110:
                            s.Value = "0";
                            break;
                        case 995:
                            s.Value = "1";
                            break;
                        case 143:
                            s.Value = "2";
                            break;
                        case 993:
                        case 585:
                            s.Value = "3";
                            break;
                    }
                    settings.changeSetting(s);
                }
                else if (s.Name == "Email.InboundServer")
                {
                    s.Value = info.inboundHost;
                    settings.changeSetting(s);
                }
                else if (s.Name == "Email.OutboundServer")
                {
                    s.Value = info.outboundHost;
                    settings.changeSetting(s);
                }
                else if (s.Name == "Email.SecureSMTP")
                {
                    s.Value = (info.outboundPort == 465).ToString();
                    settings.changeSetting(s);
                }
            }
        }

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
            get { return "DPEmail"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Email Data Provider"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
