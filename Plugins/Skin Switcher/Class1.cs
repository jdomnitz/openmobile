using System;
using OpenMobile.Plugin;
using OpenMobile.Data;

namespace Skin_Switcher
{
    public class Class1:IHighLevel
    {

        #region IHighLevel Members
        IPluginHost theHost;
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (theHost.SkinPath.EndsWith("Default"))
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("UI.Skin", "Highway");
            }
            else
            {
                using (PluginSettings s = new PluginSettings())
                    s.setSetting("UI.Skin", "Default");
            }
            theHost.execute(OpenMobile.eFunction.restartProgram);
            return null;
        }

        public string displayName
        {
            get { return "Skin Switcher"; }
        }

        #endregion

        #region IBasePlugin Members

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { throw new NotImplementedException(); }
        }

        public string authorEmail
        {
            get { throw new NotImplementedException(); }
        }

        public string pluginName
        {
            get { return "SkinSwitcher"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { throw new NotImplementedException(); }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
