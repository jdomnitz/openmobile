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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile;
using CSCore.CoreAudioAPI;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System.Threading;

namespace OMAudioControl
{

    public class AudioRouteHandler : IDisposable
    {
        private class AudioEndpointVolumeCallbackDevice : AudioEndpointVolumeCallback
        {
            /// <summary>
            /// The device
            /// </summary>
            public AudioDevice Device
            {
                get
                {
                    return this._Device;
                }
                set
                {
                    if (this._Device != value)
                    {
                        this._Device = value;
                    }
                }
            }
            private AudioDevice _Device;
        
        }


        /// <summary>
        /// The discovered output devices
        /// </summary>
        public List<AudioDevice> OutputDevices
        {
            get
            {
                return this._OutputDevices;
            }
            set
            {
                if (this._OutputDevices != value)
                {
                    this._OutputDevices = value;
                }
            }
        }
        private List<AudioDevice> _OutputDevices;

        /// <summary>
        /// The discovered input devices
        /// </summary>
        public List<AudioDevice> InputDevices
        {
            get
            {
                return this._InputDevices;
            }
            set
            {
                if (this._InputDevices != value)
                {
                    this._InputDevices = value;
                }
            }
        }
        private List<AudioDevice> _InputDevices;

        private MMDeviceEnumerator _AudioDeviceFactory = new MMDeviceEnumerator();
        private List<AudioRoute> _AudioRoutes = new List<AudioRoute>();
        private Dictionary<MMDevice, AudioEndpointVolume> _AudioControls = new Dictionary<MMDevice, AudioEndpointVolume>();
        
        public AudioRouteHandler()
        {
            InitAudioDevices();

            OM.Host.OnPowerChange += new OpenMobile.Plugin.PowerEvent(Host_OnPowerChange);
        }

        void Host_OnPowerChange(ePowerEvent type)
        {
            switch (type)
            {
                case ePowerEvent.Unknown:
                    break;
                case ePowerEvent.ShutdownPending:
                    break;
                case ePowerEvent.LogoffPending:
                    break;
                case ePowerEvent.SleepOrHibernatePending:
                    // Stop all routes
                    OM.Host.DebugMsg(DebugMessageType.Info, "Hibernate pending, pausing audio routes");
                    PauseRoutes();
                    break;
                case ePowerEvent.SystemResumed:
                    OM.Host.DebugMsg(DebugMessageType.Info, "System resuming, starting audio routes");
                    ResumeRoutes();
                    break;
                case ePowerEvent.BatteryLow:
                    break;
                case ePowerEvent.BatteryCritical:
                    break;
                case ePowerEvent.SystemOnBattery:
                    break;
                case ePowerEvent.SystemPluggedIn:
                    break;
                default:
                    break;
            }
        }

        private void PauseRoutes()
        {
            OM.Host.DebugMsg(DebugMessageType.Info, "Pauses audio routes");
            foreach (var route in _AudioRoutes)
            {
                route.Deactivate();
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Stopped audio route {0}", route));
            }
        }
        private void ResumeRoutes()
        {
            OM.Host.DebugMsg(DebugMessageType.Info, "Resumes audio routes");
            foreach (var route in _AudioRoutes)
            {
                route.Activate();
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Resumed audio route {0}", route));
            }
        }

        private void InitAudioDevices()
        {
            _InputDevices = GetAudioDevices(DataFlow.Capture);
            _OutputDevices = GetAudioDevices(DataFlow.Render);
            MapAudioControls(_OutputDevices);

            ReportDetectedDevicedToDebug("input", _InputDevices);
            ReportDetectedDevicedToDebug("output", _OutputDevices);
        }

        private void MapAudioControls(List<AudioDevice> devices)
        {
            foreach (var device in devices)
            {
                MMDevice mmDevice = device.Tag as MMDevice;
                var endpointVolume = AudioEndpointVolume.FromDevice(mmDevice);

                AudioEndpointVolumeCallbackDevice callback = new AudioEndpointVolumeCallbackDevice();
                callback.Device = device;
                callback.NotifyRecived += (s, e) =>
                {
                    var omDevice = _OutputDevices.FirstOrDefault(x => x.Equals(((AudioEndpointVolumeCallbackDevice)s).Device));
                    if (omDevice != null)
                    {
                        // Match found, raise event on audio device (we raise both the mute and the volume event as separating between them is not easy)
                        omDevice.Raise_DataUpdated(AudioDevice.Actions.Mute, e.Muted);
                        omDevice.Raise_DataUpdated(AudioDevice.Actions.Volume, ((int)Math.Round(e.MasterVolume * 100)));
                    }

                };
                endpointVolume.RegisterControlChangeNotify(callback);

                _AudioControls.Add(mmDevice, endpointVolume);
            }
        }

        private List<AudioDevice> GetAudioDevices(DataFlow audioType)
        {
            List<AudioDevice> devices = new List<AudioDevice>();

            // Element zero of the devices list should be the default audio device

            //// Add default device
            //var defaultEndpoint = _AudioDeviceFactory.GetDefaultAudioEndpoint(audioType, Role.Multimedia);
            //// Try to get the name of the device
            //string friendlyNameDefaultUnit = defaultEndpoint.FriendlyName;
            //AudioDevice device = new AudioDevice(0, AudioDevice_Get, AudioDevice_Set, defaultEndpoint, true);
            //devices.Add(device);

            // Add other devices
            var audioDevices = _AudioDeviceFactory.EnumAudioEndpoints(audioType, DeviceState.Active);
            for (int i = 0; i < audioDevices.Count; i++)
            {
                try
                {
                    // Try to get the name of the device
                    string friendlyName = audioDevices[i].FriendlyName;

                    AudioDevice deviceLoop = new AudioDevice(friendlyName, i + 1, AudioDevice_Get, AudioDevice_Set, audioDevices[i]);
                    devices.Add(deviceLoop);
                }
                catch (Exception ex)
                {
                    OM.Host.DebugMsg(String.Format("OMAudioControl.GetAudioDevices -> Exception while getting info for audio device (type: {0})", audioType), ex);
                }
            }

            // Add default device
            try
            {   // Try to detect default device
                var defaultEndpoint = _AudioDeviceFactory.GetDefaultAudioEndpoint(audioType, Role.Multimedia);
                // Try to get the name of the device
                string friendlyNameDefaultUnit = defaultEndpoint.FriendlyName;
                AudioDevice device = new AudioDevice(0, AudioDevice_Get, AudioDevice_Set, defaultEndpoint, true);
                devices.Insert(0, device);
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Detected default audio device: {0}", friendlyNameDefaultUnit));
            }
            catch
            {   // Default device detection failed, let's set the first detected device as default
                if (audioDevices.Count >= 1)
                {
                    AudioDevice defaultDevice = new AudioDevice(0, AudioDevice_Get, AudioDevice_Set, audioDevices[0], true);
                    devices.Insert(0, defaultDevice);
                    OM.Host.DebugMsg(DebugMessageType.Warning, String.Format("Failed to detect default audio device, using first device instead: {0}", audioDevices[0].FriendlyName));
                }
                else
                {   // No audio devices available
                    OM.Host.DebugMsg(DebugMessageType.Warning, "No audio devices available, failed to set default device");
                }
            }
            return devices;
        }

        private void ReportDetectedDevicedToDebug(string deviceType, List<AudioDevice> devices)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("OMAudioControl detected the following {0} devices:", deviceType));
            foreach (var device in devices)
            {
                if (_AudioControls.ContainsKey(device.Tag as MMDevice))
                    sb.AppendLine(String.Format("{0} (AudioControl initialized)", device.Name));
                else
                    sb.AppendLine(device.Name);
            }
            OM.Host.DebugMsg(DebugMessageType.Info, sb.ToString());
        }

        private void AudioDevice_Set(AudioDevice audioDevice, AudioDevice.Actions action, object value)
        {
            MMDevice device = audioDevice.Tag as MMDevice;
            if (device == null)
                return;

            if (!_AudioControls.ContainsKey(device))
                return;

            AudioEndpointVolume volumeControl = _AudioControls[device];

            switch (action)
            {
                case AudioDevice.Actions.Mute:
                    volumeControl.SetMute((bool)value, Guid.Empty);
                    break;
                case AudioDevice.Actions.Volume:
                    {
                        double volumeLevel = Convert.ToDouble(value);
                        volumeControl.SetMasterVolumeLevelScalar(((float)volumeLevel / 100.0f), Guid.Empty);
                        volumeControl.SetMute(false, Guid.Empty);
                    }
                    break;
                default:
                    break;
            }
        }
        private object AudioDevice_Get(AudioDevice audioDevice, AudioDevice.Actions action)
        {
            MMDevice device = audioDevice.Tag as MMDevice;
            if (device == null)
                return null;

            if (!_AudioControls.ContainsKey(device))
                return null;

            AudioEndpointVolume volumeControl = _AudioControls[device];

            switch (action)
            {
                case AudioDevice.Actions.Mute:
                    {
                        if (device == null)
                            return false;
                        return volumeControl.GetMute();
                    }

                case AudioDevice.Actions.Volume:
                    {
                        if (device == null)
                            return 0;
                        return (int)Math.Round(volumeControl.GetMasterVolumeLevelScalar() * 100);
                    }
                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// Activates the audio route
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public bool ActivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            if (sourceDevice == null || targetDevice == null || sourceDevice.Tag == null || targetDevice.Tag == null)
                return false;

            try
            {
                var sourceMMDevice = sourceDevice.Tag as MMDevice;
                var targetMMDevice = targetDevice.Tag as MMDevice;
                var audioRoute = _AudioRoutes.Find(x => x.MatchesRoute(sourceMMDevice, targetMMDevice));
                if (audioRoute == null)
                {   // New route
                    audioRoute = new AudioRoute(sourceMMDevice, targetMMDevice);
                    _AudioRoutes.Add(audioRoute);
                }

                // Activate the audio route
                audioRoute.Activate();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deactivates an audio route
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public bool DeactivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            if (sourceDevice == null || targetDevice == null || sourceDevice.Tag == null || targetDevice.Tag == null)
                return false;

            try
            {
                var sourceMMDevice = sourceDevice.Tag as MMDevice;
                var targetMMDevice = targetDevice.Tag as MMDevice;
                var audioRoute = _AudioRoutes.Find(x => x.MatchesRoute(sourceMMDevice, targetMMDevice));
                if (audioRoute == null)
                {   // Non existent route, exit
                    return false;
                }

                // Deactivate the audio route
                audioRoute.Deactivate();

                _AudioRoutes.Remove(audioRoute);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            foreach (var audioRoute in _AudioRoutes)
                audioRoute.Dispose();
        }
    }
}
