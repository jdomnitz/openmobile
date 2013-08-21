using System;
using OpenMobile.Plugin;
using System.Runtime.InteropServices;

namespace QTPlayer
{
    public class QTPlayer:IAVPlayer
    {
        public bool play(int instance)
        {
            return false;
        }

        public bool stop(int instance)
        {
            return false;
        }

        public bool setPosition(int instance, float seconds)
        {
            return false;
        }

        public bool setPlaybackSpeed(int instance, float speed)
        {
            return false;
        }

        public bool play(int instance, string url, OpenMobile.eMediaType t)
        {
            IntPtr file=API.CFURLCreateFromFileSystemRepresentation(0, url, url.Length, false);
            return true;
        }

        public float getCurrentPosition(int instance)
        {
            return 1F;
        }

        public OpenMobile.ePlayerStatus getPlayerStatus(int instance)
        {
            return OpenMobile.ePlayerStatus.Playing;
        }

        public float getPlaybackSpeed(int instance)
        {
            return 1F;
        }

        public bool setVolume(int instance, int percent)
        {
            return false;
        }

        public int getVolume(int instance)
        {
            return 100;
        }

        public OpenMobile.mediaInfo getMediaInfo(int instance)
        {
            return new OpenMobile.mediaInfo();
        }

        public event MediaEvent OnMediaEvent;

        public string[] OutputDevices
        {
            get { return new string[]{"Default Device"}; }
        }

        public bool SupportsAdvancedFeatures
        {
            get { return false; }
        }

        public bool SetVideoVisible(int instance, bool visible)
        {
            return false;
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            return OpenMobile.eLoadStatus.LoadSuccessful;
        }
        
        #region IBasePlugin Members
        public Settings loadSettings()
        {
            return null;
        }

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return "jdomnitz@gmail.com"; }
        }

        public string pluginName
        {
            get { return "QTPlayer"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Quicktime media player"; }
        }

        public bool incomingMessage(string message, string source)
        {
            return false;
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

        #region IPausable Members

        public bool pause(int instance)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
