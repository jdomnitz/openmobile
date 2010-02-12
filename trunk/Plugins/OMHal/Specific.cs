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
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using OMHal;
using OpenMobile.Plugin;
using OSSpecificLib.CoreAudioApi;

namespace OpenMobile.Framework
{
    /// <summary>
    /// OS Specific Functions
    /// </summary>
    public static class Specific
    {
        static MMDevice device;
        private static OperatingSystem os = System.Environment.OSVersion;
        /// <summary>
        /// Gets the system volume
        /// </summary>
        /// <returns>Int (Range 0-100)</returns>
        public static int getVolume()
        {
            if (os.Version.Major < 6)
            {
                uint volume = OpenMobile.Framework.OSSpecificLib.XPVolume.GetVolume();
                if (OpenMobile.Framework.OSSpecificLib.XPVolume.IsMuted() == true)
                    return -1; //Mute
                return (int)volume;
            }
            else
            {
                if (device != null)
                    return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                else
                    return -1;
            }
        }
        /// <summary>
        /// Sets the system volume
        /// </summary>
        /// <param name="volume">Integer (Range: 0-100)</param>
        public static void setVolume(int volume)
        {
            if (os.Version.Major < 6)
            {
                if (volume == -1)
                {
                    OpenMobile.Framework.OSSpecificLib.XPVolume.SetMute(true);
                }
                OpenMobile.Framework.OSSpecificLib.XPVolume.MixerInfo mi = OpenMobile.Framework.OSSpecificLib.XPVolume.GetMixerControls();
                mi.muteCtl = 0;
                uint vol;
                vol = (uint)((volume - 1) * (ushort.MaxValue) / 100);
                vol = (vol & (vol << 16));
                OpenMobile.Framework.OSSpecificLib.XPVolume.SetVolume(mi);
                OpenMobile.Framework.OSSpecificLib.XPVolume.waveOutSetVolume(IntPtr.Zero, vol);
            }
            else
            {
                if (device != null)
                {
                    if (volume == -1)
                        device.AudioEndpointVolume.Mute = true;
                    else
                    {
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);
                        device.AudioEndpointVolume.Mute = false;
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
                int result = OpenMobile.Framework.OSSpecificLib.XPVolume.mixerOpen(out tmp, 0, handle, IntPtr.Zero, CALLBACK_WINDOW);
                if (OpenMobile.Framework.OSSpecificLib.XPVolume.IsMuted() == true)
                    Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1","","");
                return (result == 0);
            }
            else
            {
                try
                {
                    MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                    device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                    device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
                    if (device.AudioEndpointVolume.Mute == true)
                        Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1","","");
                }
                catch (Exception) { }
                return (device!=null);
            }
        }
        static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (data.Muted == false)
                Form1.raiseSystemEvent(eFunction.systemVolumeChanged, ((int)(data.MasterVolume * 100)).ToString(),"","");
            else
                Form1.raiseSystemEvent(eFunction.systemVolumeChanged, "-1","","");
        }
        /// <summary>
        /// Lists all audio devices present on the system
        /// </summary>
        /// <returns></returns>
        public static string[] getAudioDevices()
        {
            if (os.Version.Major < 6) //Xp
            {
                //ToDo - Implement on XP
                return null;
            }
            else
            {
                return device.AudioEndpointVolume.getDevices();
            }
        }
    }
}
