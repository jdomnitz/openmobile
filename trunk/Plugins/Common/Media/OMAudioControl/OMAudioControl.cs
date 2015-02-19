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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Data;
using System.Collections.Generic;
using System.Linq;
//using VistaAudio.CoreAudioApi;
using CoreAudioApi;

namespace OMDataSourceSample
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows)]
    public sealed class OMAudioControl : BasePluginCode, IAudioDeviceProvider
    {
        private MMDeviceEnumerator _AudioDeviceFactory = new MMDeviceEnumerator();
        private AudioDevice _DefaultAudioDevice;
        private List<AudioDevice> _OMAudioDevices = new List<AudioDevice>();

        public OMAudioControl()
            : base("OMAudioControl", OM.Host.getSkinImage("Icons|Icon-OM"), 1f, "Audio control provider", "OM Dev team", "")
        {
        }

        public override eLoadStatus initialize(IPluginHost host)
        {
            // Currently this plugin only supports windows
            if (!Configuration.RunningOnWindows)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;

            // TODO: Add support for WinXP (WinXP or older is currently dropped)
            if (Environment.OSVersion.Version.Major < 6)
                return eLoadStatus.LoadFailedGracefulUnloadRequested;

            // Enumerate audio devices
            InitAudioDevices();

            return eLoadStatus.LoadSuccessful;
        }

        private void InitAudioDevices()
        {
            string info = "OMAudioControl detected the following devices:\r\n";

            _OMAudioDevices = new List<AudioDevice>();

            // Add default device
            OM.Host.DebugMsg(DebugMessageType.Info, this.pluginName, "trying to detect the default audio device");
            var defaultEndpoint = _AudioDeviceFactory.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            // Try to get the name of the device
            string friendlyNameDefaultUnit = defaultEndpoint.FriendlyName;
            OM.Host.DebugMsg(DebugMessageType.Info, this.pluginName, String.Format("Default audio device detected as {0}", friendlyNameDefaultUnit));
            OM.Host.DebugMsg(DebugMessageType.Info, this.pluginName, String.Format("Trying to hook events on device {0}", friendlyNameDefaultUnit));
            AudioDevice_HookEvents(defaultEndpoint);
            AudioDevice device = new AudioDevice(0, AudioDevice_Get, AudioDevice_Set, defaultEndpoint, true);
            info += device.Name + "(" + friendlyNameDefaultUnit + ")\r\n";
            _OMAudioDevices.Add(device);
            OM.Host.DebugMsg(DebugMessageType.Info, this.pluginName, String.Format("OM successfully created AudioDevice for device {0}", friendlyNameDefaultUnit));

            // Add other devices
            var audioDevices = _AudioDeviceFactory.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);
            for (int i = 0; i < audioDevices.Count; i++)
            {
                // Try to get the name of the device
                string friendlyName = audioDevices[i].FriendlyName;
                OM.Host.DebugMsg(DebugMessageType.Info, this.pluginName, String.Format("Trying to hook events on device {0}", friendlyName));
                AudioDevice_HookEvents(audioDevices[i]);

                device = new AudioDevice(friendlyName, i + 1, AudioDevice_Get, AudioDevice_Set, audioDevices[i]);
                info += device.Name + "\r\n";
                _OMAudioDevices.Add(device);

                OM.Host.DebugMsg(DebugMessageType.Info, this.pluginName, String.Format("OM successfully created AudioDevice for device {0}", friendlyName));
            }
            OM.Host.DebugMsg(DebugMessageType.Info, info);
        }

        private void AudioDevice_HookEvents(MMDevice device)
        {
            device.AudioEndpointVolume.OnVolumeNotification += (AudioVolumeNotificationData data) =>
            {
                AudioEndpointVolume_OnVolumeNotification(device, data);
            };
        }

        void AudioEndpointVolume_OnVolumeNotification(MMDevice device, AudioVolumeNotificationData data)
        {
            var omDevice = _OMAudioDevices.FirstOrDefault(x => ((MMDevice)x.Tag).Equals(device));
            if (omDevice != null)
            {
                // Match found, raise event on audiodevice (we raise both the mute and the volume event as separating between them is not easy)
                omDevice.Raise_DataUpdated(AudioDevice.Actions.Mute, data.Muted);
                omDevice.Raise_DataUpdated(AudioDevice.Actions.Volume, ((int)Math.Round(data.MasterVolume * 100)));
            }
        }

        private void AudioDevice_Set(AudioDevice audioDevice, AudioDevice.Actions action, object value)
        {
            MMDevice device = audioDevice.Tag as MMDevice;
            if (device == null)
                return;

            switch (action)
            {
                case AudioDevice.Actions.Mute:
                    device.AudioEndpointVolume.Mute = (bool)value;
                    break;
                case AudioDevice.Actions.Volume:
                    {
                        double volumeLevel = Convert.ToDouble(value);
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volumeLevel / 100.0f);
                        device.AudioEndpointVolume.Mute = false;
                    }
                    break;
                default:
                    break;
            }
        }
        private object AudioDevice_Get(AudioDevice audioDevice, AudioDevice.Actions action)
        {
            MMDevice device = audioDevice.Tag as MMDevice;

            switch (action)
            {
                case AudioDevice.Actions.Mute:
                    {
                        if (device == null)
                            return false;
                        return device.AudioEndpointVolume.Mute;
                    }

                case AudioDevice.Actions.Volume:
                    {
                        if (device == null)
                            return 0;
                        return (int)Math.Round(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    }
                default:
                    break;
            }

            return null;
        }

        public AudioDevice[] OutputDevices
        {
            get { return _OMAudioDevices.ToArray(); }
        }


    }
}
