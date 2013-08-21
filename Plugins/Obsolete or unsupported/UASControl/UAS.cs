using OpenMobile.Plugin;
using MJSGUAS;
using OpenMobile.Data;
using System.Collections.Generic;
using OpenMobile;

namespace UASControl
{
    public class UAS:IOther
    {
        MJSGUAS.UASControl device;
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            if (!MJSGUAS.UASControl.DeviceExists)
                return OpenMobile.eLoadStatus.LoadFailedGracefulUnloadRequested;
            device =new MJSGUAS.UASControl();
            theHost = host;
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void theHost_OnMediaEvent(eFunction function, int instance, string arg)
        {
            if (function == eFunction.loadTunedContent)
            {
                if (_settings == null)
                    return;
                foreach(Setting s in _settings)
                {
                    if (s.Value == arg)
                    {
                        switch (s.Name)
                        {
                            case "UAS.Source1":
                                device.AudioInput = AInput.Input_1;
                                break;
                            case "UAS.Source2":
                                device.AudioInput = AInput.Input_2;
                                break;
                            case "UAS.Source3":
                                device.AudioInput = AInput.Input_3;
                                break;
                            case "UAS.Source4":
                                device.AudioInput = AInput.Input_4;
                                break;
                        }
                    }
                }
            }
        }

        Settings _settings;
        public Settings loadSettings()
        {
            if (_settings == null)
            {
                _settings = new Settings("Universal Audio Selector");
                _settings.OnSettingChanged += new SettingChanged(_settings_OnSettingChanged);
                List<string> options = new List<string>();
                List<string> values = new List<string>();
                options.Add("Nothing");
                values.Add("");
                object o;
                theHost.getData(OpenMobile.eGetData.GetPlugins, "", out o);
                List<IBasePlugin> plugins = (List<IBasePlugin>)o;
                for (int i = 0; i < plugins.Count; i++)
                {
                    if (plugins[i] is ITunedContent)
                    {
                        options.Add(plugins[i].pluginName);
                        values.Add(plugins[i].pluginName);
                    }
                }
                using (PluginSettings s = new PluginSettings())
                {
                    _settings.Add(new Setting(SettingTypes.MultiChoice, "UAS.Source1", "Port 1", "", options, values, s.getSetting("UAS.Source1")));
                    _settings.Add(new Setting(SettingTypes.MultiChoice, "UAS.Source2", "Port 2", "", options, values, s.getSetting("UAS.Source2")));
                    _settings.Add(new Setting(SettingTypes.MultiChoice, "UAS.Source3", "Port 3", "", options, values, s.getSetting("UAS.Source3")));
                    _settings.Add(new Setting(SettingTypes.MultiChoice, "UAS.Source4", "Port 4", "", options, values, s.getSetting("UAS.Source4")));
                }
            }
            return _settings;
        }

        void _settings_OnSettingChanged(int screen,Setting setting)
        {
            using(PluginSettings s=new PluginSettings())
                s.setSetting(setting.Name,setting.Value);
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
            get { return "UASControl"; }
        }

        public float pluginVersion
        {
            get { return 1; }
        }

        public string pluginDescription
        {
            get { return "Universal Audio Selector"; }
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
    }
}
