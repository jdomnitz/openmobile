using System;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.Controls;
using System.Collections.Generic;
using OpenMobile;
namespace Radio
{
    public class Radio:IHighLevel
    {
        #region IHighLevel Members
        ScreenManager manager;
        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;
            OMList l1 = (OMList)manager[screen][0];
            object o;
            theHost.getData(OpenMobile.eGetData.GetPlugins, "", out o);
            List<IBasePlugin> lst = (List<IBasePlugin>)o;
            lst = lst.FindAll(p => typeof(ITunedContent).IsInstanceOfType(p));
            foreach (IBasePlugin b in lst)
                l1.Add(b.pluginName);
            return manager[screen];
        }

        public string displayName
        {
            get { return "Radio"; }
        }

        #endregion

        #region IBasePlugin Members
        IPluginHost theHost;
        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            manager = new ScreenManager(host.ScreenCount);
            OMPanel p = new OMPanel();
            OMList l1 = new OMList(30, 110, 600, 80);
            l1.OnClick += new userInteraction(l1_OnClick);
            OMList l2 = new OMList(30, 220, 600, 300);
            l2.OnClick += new userInteraction(l2_OnClick);
            p.addControl(l1);
            p.addControl(l2);
            manager.loadPanel(p);
            theHost = host;
            theHost.OnMediaEvent += new MediaEvent(theHost_OnMediaEvent);
            return eLoadStatus.LoadSuccessful;
        }

        void l2_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.tuneTo, theHost.instanceForScreen(screen).ToString(), ((OMList)sender).SelectedItem.subItem);
        }

        void theHost_OnMediaEvent(eFunction function, int instance, string arg)
        {
            if (function == eFunction.stationListUpdated)
            {
                for (int i = 0; i < theHost.ScreenCount; i++)
                    if (instance == theHost.instanceForScreen(i))
                        updateStations(i);
            }
        }

        private void updateStations(int i)
        {
            object o;
            theHost.getData(eGetData.GetStationList,"",theHost.instanceForScreen(i).ToString(),out o);
            if (o==null)
                return;
            stationInfo[] list=(stationInfo[])o;
            OMList l = (OMList)manager[i][1];
            l.Clear();
            foreach (stationInfo info in list)
                l.Add(new OMListItem(info.stationName,info.stationID));
        }

        void l1_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.loadTunedContent, theHost.instanceForScreen(screen).ToString(), ((OMList)sender).SelectedItem.text);
        }

        public Settings loadSettings()
        {
            throw new NotImplementedException();
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
            get { return "Radio"; }
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
            throw new NotImplementedException();
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
