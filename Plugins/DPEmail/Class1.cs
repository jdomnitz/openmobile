using System;
using System.Collections.Generic;
using OpenMobile.Plugin;
using OpenMobile;
using OpenMobile.Email;

namespace DPEmail
{
    public sealed class DPEmail:IDataProvider
    {
        public bool refreshData()
        {
            return false;
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
            return 0;
        }

        public DateTime lastUpdated
        {
            get { return DateTime.MinValue; }
        }

        public string pluginType()
        {
            return "Email";
        }

        #region IBasePlugin Members
        EmailClient email;
        ServerInfo info;
        public eLoadStatus initialize(IPluginHost host)
        {
            email = new EmailClient();
            return eLoadStatus.LoadSuccessful;
        }
        public Settings settings;
        public Settings loadSettings()
        {
            if (settings == null)
            {
                settings = new Settings("Account Settings");
                List<string> options=new List<string>(new string[]{"POP Server","Secure POP Server","IMAP Server","Secure IMAP Server"});
                List<string> values=new List<string>(new string[]{"0","1","2","3"});
                settings.Add(new Setting(SettingTypes.Text,"Email.Username","","Email"));
                settings.Add(new Setting(SettingTypes.Password, "Email.Password", "", "Password"));
                settings.Add(new Setting(SettingTypes.MultiChoice, "Email.ServerType", "Server Type", "", options, values, "0"));
                settings.Add(new Setting(SettingTypes.Text, "Email.InboundServer", "", "Inbound Server"));
                settings.Add(new Setting(SettingTypes.Text, "Email.OutboundServer", "", "Outbound Server"));
                settings.Add(new Setting(SettingTypes.MultiChoice, "Email.SecureSMTP", "", "Secure Outbound", Setting.BooleanList, Setting.BooleanList));
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
            }
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
