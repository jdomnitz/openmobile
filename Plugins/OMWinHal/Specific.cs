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
using WaveLib.AudioMixer;

namespace OMHal
{
    /// <summary>
    /// OS Specific Functions
    /// </summary>
    public static class Specific
    {
        static MMDevice[] device;
        static WaveLib.AudioMixer.Mixers m;
        static int[] lastVolume;
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
                if (instance > m.Playback.Devices.Count)
                    return -1;
                if (instance == 0)
                    m.Playback.DeviceId = m.Playback.DeviceIdDefault;
                else
                    m.Playback.DeviceId = m.Playback.Devices[instance - 1].DeviceId;
                MixerLine l = m.Playback.Lines.GetMixerFirstLineByComponentType(WaveLib.AudioMixer.MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                lastVolume[instance] = l.Volume;
                if (l.Mute == true)
                {
                    lastVolume[instance] = -1;
                    return lastVolume[instance];
                }
                return (int)Math.Round(((double)l.Volume/l.VolumeMax)*100.0);
            }
            else
            {
                if (device != null)
                {
                    if (instance < device.Length)
                    {
                        if (device[instance].AudioEndpointVolume.Mute == true)
                            return -1;
                        else
                            return (int)(device[instance].AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    }
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
                if (instance > m.Playback.Devices.Count)
                    return;
                if (instance == 0)
                    m.Playback.DeviceId = m.Playback.DeviceIdDefault;
                else
                    m.Playback.DeviceId = m.Playback.Devices[instance-1].DeviceId;
                MixerLine l=m.Playback.Lines.GetMixerFirstLineByComponentType(WaveLib.AudioMixer.MIXERLINE_COMPONENTTYPE.DST_SPEAKERS);
                if (volume == -1)
                    l.Mute = true;
                else
                {
                    l.Mute = false;
                    if (volume >= 0)
                        l.Volume = (int)((volume / 100.0) * l.VolumeMax);
                }
            }
            else
            {
                if (device != null)
                {
                    if (instance >= device.Length)
                        return;
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

        public static bool hookVolume(IntPtr handle)
        {
            if (os.Version.Major < 6) //Xp
            {
                m = new WaveLib.AudioMixer.Mixers();
                lastVolume = new int[m.Devices.Count + 1];
                m.Playback.MixerLineChanged += new Mixer.MixerLineChangeHandler(Playback_MixerLineChanged);
                return true;
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
                        else
                            Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(i).ToString(), i.ToString(), "");
                }
                catch (Exception) { }
                return (device!=null);
            }
        }

        static void Playback_MixerLineChanged(Mixer mixer, MixerLine line)
        {
            if (lastVolume[mixer.DeviceId+1] == line.Volume)
                return;
            if (line.Mute && (lastVolume[mixer.DeviceId+1] == -1))
                return;
            Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(mixer.DeviceId+1).ToString(), (mixer.DeviceId+1).ToString(), "");
            if (mixer.DeviceIdDefault==mixer.DeviceId)
                Form1.raiseSystemEvent(eFunction.systemVolumeChanged, getVolume(mixer.DeviceId + 1).ToString(), "0", "");
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
