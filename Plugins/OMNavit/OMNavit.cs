using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.Plugin;
using OpenMobile.Controls;

namespace OpenMobile.Plugins.OMNavit
{
    [PluginLevel(PluginLevels.Normal)]
    public class OMNavit : IHighLevel, INavigation
    {
        private const string PLUGIN_NAME = "OMNavit";

        OMPanel navitPanelContainer;

        #region IHighLevel Members

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (navitPanelContainer == null)
            {
                var omNavit = new NavitOpenGLInterface();
                omNavit.Top = 100;
                omNavit.Width = 1000;
                omNavit.Height = 500;
                navitPanelContainer = new OMPanel();
                navitPanelContainer.addControl(omNavit);
            }

            return navitPanelContainer;
        }

        public string displayName
        {
            get { return PLUGIN_NAME; }
        }

        #endregion

        #region IBasePlugin Members

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        public Settings loadSettings()
        {
            throw new NotImplementedException();
        }

        public string authorName
        {
            get { return "efess"; }
        }

        public string authorEmail
        {
            get { return "efessel@gmail.com"; }
        }

        public string pluginName
        {
            get { return PLUGIN_NAME; }
        }

        public float pluginVersion
        {
            get { return 0.01F; }
        }

        public string pluginDescription
        {
            get { return "Navigation with Navit Integration"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return true;
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return true;
            //throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion

        #region INavigation Members

        public event NavigationEvent OnNavigationEvent;

        public OpenMobile.Position Position
        {
            get { throw new NotImplementedException(); }
        }

        public OpenMobile.Location Location
        {
            get { throw new NotImplementedException(); }
        }

        public OpenMobile.Location Destination
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Detour(int distance)
        {
            throw new NotImplementedException();
        }

        public string[] availablePanels
        {
            get { throw new NotImplementedException(); }
        }

        public bool switchTo(string panel)
        {
            throw new NotImplementedException();
        }

        public bool navigateTo(OpenMobile.Location destination)
        {
            throw new NotImplementedException();
        }

        public bool findPOI(string name)
        {
            throw new NotImplementedException();
        }

        public OpenMobile.Controls.OMControl getMap
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
