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
using System.Runtime.InteropServices;
using System.Text;
using OpenMobile;
using OSSpecificLib.CoreAudioApi;
using System.Management;

namespace OMHal
{
    /// <summary>
    /// OS Specific Functions
    /// </summary>
    public static class Specific
    {
        static MMDevice[] device;
        private static OperatingSystem os = System.Environment.OSVersion;
        const int SC_MONITORPOWER = 0xF170;
        const int WM_SYSCOMMAND = 0x0112;
        private static bool monitorOn = true;
        /// <summary>
        /// Gets the system volume
        /// </summary>
        /// <returns>Int (Range 0-100)</returns>
        public static int getVolume(int instance)
        {
            if (os.Version.Major < 6)
            {
                uint volume = XPVolume.GetVolume();
                if (XPVolume.IsMuted() == true)
                    return -1; //Mute
                return (int)volume;
            }
            else
            {
                if (device != null)
                {
                    if (instance<device.Length)
                        return (int)(device[instance].AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
                return -1;
            }
        }
        /// <summary>
        /// Sets the system volume
        /// </summary>
        /// <param name="volume">Integer (Range: 0-100)</param>
        public static void setVolume(int volume,int instance)
        {
            if (os.Version.Major < 6)
            {
                if (volume == -1)
                {
                    XPVolume.SetMute(true);
                }
                else if (volume == -2)
                {
                    XPVolume.SetMute(false);
                }
                XPVolume.MixerInfo mi = XPVolume.GetMixerControls();
                mi.muteCtl = 0;
                uint vol;
                vol = (uint)((volume - 1) * (ushort.MaxValue) / 100);
                vol = (vol & (vol << 16));
                XPVolume.SetVolume(mi);
                XPVolume.waveOutSetVolume(IntPtr.Zero, vol);
            }
            else
            {
                if (device != null)
                {
                    if (volume == -1)
                        device[instance].AudioEndpointVolume.Mute = true;
                    else if(volume==-2)
                        device[instance].AudioEndpointVolume.Mute=false;
                    else
                    {
                        device[instance].AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);
                        device[instance].AudioEndpointVolume.Mute = false;
                    }
                }
            }
        }
        /// <summary>
        /// Creates a callback for volume change notifications
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public static bool hookVolume(IntPtr handle)
        {
            if (os.Version.Major < 6) //Xp
            {
                uint CALLBACK_WINDOW = 0x00010000;
                IntPtr tmp;
                int result = XPVolume.mixerOpen(out tmp, 0, handle, IntPtr.Zero, CALLBACK_WINDOW);
                if (XPVolume.IsMuted() == true)
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1","0","");
                return (result == 0);
            }
            else
            {
                try
                {
                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    MMDeviceCollection col= DevEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);
                    if (col.Count == 0)
                        return false;
                    device = new MMDevice[col.Count + 1];
                    device[0] = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                    for (int i = 0; i < col.Count; i++)
                    {
                        device[i + 1] = col[col.Count-i-1];
                        col[i].AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
                    }
                    for(int i=0;i<device.Length;i++)
                        if (device[i].AudioEndpointVolume.Mute == true)
                            Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1",i.ToString(),"");
                }
                catch (Exception) { }
                return (device!=null);
            }
        }
        static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            for (int i = 0; i < device.Length; i++)
            {
                if ((data.MasterVolume != device[i].AudioEndpointVolume.MasterVolumeLevelScalar)||(device[i].AudioEndpointVolume.Mute!=data.Muted))
                    continue;
                if (data.Muted == false)
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, ((int)(data.MasterVolume * 100)).ToString(), i.ToString(), "");
                else
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1", i.ToString(), "");
            }
        }
        /// <summary>
        /// Lists all audio devices present on the system
        /// </summary>
        /// <returns></returns>
        public static string[] getAudioDevices()
        {
            if (os.Version.Major < 6) //Xp
            {
                //TODO - Implement on XP
                return null;
            }
            else
            {
                return device[0].AudioEndpointVolume.getDevices();
            }
        }

        internal static void eject()
        {
            StringBuilder buf = new StringBuilder();
            mciSendString("SET CDAudio door open", buf, 127, IntPtr.Zero);
        }
        internal static void SetBrightness(int value)
        {
            if ((!monitorOn) && (value != 0))
            {
                monitorOn = true;
                SwitchOnMonitor();
            }
            if (value == 0)
            {
                monitorOn = false;
                SwitchOffMonitor();
                return;
            }
            if (value == 1) //round down
                value = 0;
            Set((byte)(value * 2.55));
        }

        private static void SwitchOffMonitor()
        {
            SendMessage(Form1.ActiveForm.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }
        private static void SwitchOnMonitor()
        {
            SendMessage(Form1.ActiveForm.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, -1);
        }
        private static void Set(byte targetBrightness)
        {
            ManagementScope scope = new ManagementScope(@"root\WMI");
            SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
            using(ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                foreach (ManagementObject obj2 in objects)
                {
                    obj2.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, targetBrightness });
                    break;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);
        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);
    }
}
