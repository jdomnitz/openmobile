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
using OpenMobile.Plugin;

namespace OpenMobile.Media
{
    /// <summary>
    /// A handler for audio devices
    /// </summary>
    public class AudioDeviceHandler
    {
        private IAudioDeviceProvider _AudioDeviceProvider;
 
        /// <summary>
        /// Creates a new audio device handler
        /// </summary>
        public AudioDeviceHandler()
        {
            OM.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);
        }

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.AudioDevicesAvailable)
            {
                var plugins = OM.Host.GetPlugins<IAudioDeviceProvider>();
                if (plugins != null)
                {
                    _AudioDeviceProvider = plugins[0] as IAudioDeviceProvider;
                    OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Audio device provider registered: {0}", ((IBasePlugin)_AudioDeviceProvider).pluginName));
                }
                else
                {
                    OM.Host.DebugMsg(DebugMessageType.Error, "No audio device provider registered. Audio device control will not be available");
                }
            }
        }

        /// <summary>
        /// Audio input devices
        /// </summary>
        public AudioDevice[] InputDevices
        {
            get
            {
                if (_AudioDeviceProvider != null)
                    return _AudioDeviceProvider.InputDevices;
                else
                    return null;
            }
        }

        /// <summary>
        /// Default input device
        /// </summary>
        public AudioDevice DefaultInputDevice
        {
            get
            {
                if (_AudioDeviceProvider != null && _AudioDeviceProvider.InputDevices != null && _AudioDeviceProvider.InputDevices.Length >= 1)
                    return _AudioDeviceProvider.InputDevices[0];
                else
                    return null;
            }
        }

        /// <summary>
        /// Audio output devices
        /// </summary>
        public AudioDevice[] OutputDevices
        {
            get
            {
                if (_AudioDeviceProvider != null)
                    return _AudioDeviceProvider.OutputDevices;
                else
                    return null;
            }
        }

        /// <summary>
        /// Default output device
        /// </summary>
        public AudioDevice DefaultOutputDevice
        {
            get
            {
                if (_AudioDeviceProvider != null && _AudioDeviceProvider.OutputDevices != null && _AudioDeviceProvider.OutputDevices.Length >= 1)
                    return _AudioDeviceProvider.OutputDevices[0];
                else
                    return null;
            }
        }

        /// <summary>
        /// Get an audio device by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AudioDevice GetInputDeviceByName(string name)
        {
            if (_AudioDeviceProvider == null)
                return null;

            var device = _AudioDeviceProvider.InputDevices.ToList().Find(x => x.Name == name);
            return device;
        }

        /// <summary>
        /// Get an audio device by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AudioDevice GetOutputDeviceByName(string name)
        {
            if (_AudioDeviceProvider == null)
                return null;

            return _AudioDeviceProvider.OutputDevices.First(x => x.Name == name);
        }

        /// <summary>
        /// Activates an audio route
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public AudioRoute ActivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            if (_AudioDeviceProvider == null)
            {
                OM.Host.DebugMsg(DebugMessageType.Error, String.Format("Failed to activate audio route from {0} to {1}, no _AudioDeviceProvider active", sourceDevice, targetDevice));
                return null;
            }

            AudioRoute audioRoute = new AudioRoute(sourceDevice, targetDevice);

            if (!_AudioDeviceProvider.ActivateRoute(sourceDevice, targetDevice))
            {
                OM.Host.DebugMsg(DebugMessageType.Error, String.Format("Failed to activate audio route from {0} to {1}", sourceDevice, targetDevice));
                return null;
            }
            else
            {
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Activated audio route from {0} to {1}", sourceDevice, targetDevice));
                return audioRoute;
            }
        }

        /// <summary>
        /// Activates a route from a input audio device to a zone
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public AudioRoute ActivateRoute(AudioDevice sourceDevice, Zone zone)
        {
            return ActivateRoute(sourceDevice, zone.AudioDevice);
        }

        /// <summary>
        /// Activates a route from a input audio device specified by name to a zone. If source name is empty then the default input device is used.
        /// </summary>
        /// <param name="sourceDeviceName"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public AudioRoute ActivateRoute(string sourceDeviceName, Zone zone)
        {
            AudioDevice sourceDevice = null;
            if (String.IsNullOrWhiteSpace(sourceDeviceName))
                sourceDevice = DefaultInputDevice;
            else
                sourceDevice = OM.Host.AudioDeviceHandler.GetInputDeviceByName(sourceDeviceName);
            if (sourceDevice == null)
                return null;
            
            return ActivateRoute(sourceDevice, zone.AudioDevice);
        }

        /// <summary>
        /// Deactivates an audio route
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public bool DeactivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            if (_AudioDeviceProvider == null)
            {
                OM.Host.DebugMsg(DebugMessageType.Error, String.Format("Failed to deactivate audio route from {0} to {1}, no _AudioDeviceProvider active", sourceDevice, targetDevice));
                return false;
            }

            if (_AudioDeviceProvider.DeactivateRoute(sourceDevice, targetDevice))
            {
                OM.Host.DebugMsg(DebugMessageType.Error, String.Format("Failed to deactivate audio route from {0} to {1}", sourceDevice, targetDevice));
                return true;
            }
            else
            {
                OM.Host.DebugMsg(DebugMessageType.Info, String.Format("Deactivated audio route from {0} to {1}", sourceDevice, targetDevice));
                return false;
            }
        }

        /// <summary>
        /// Deactivates a route from a input audio device to a zone
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool DeactivateRoute(AudioDevice sourceDevice, Zone zone)
        {
            return DeactivateRoute(sourceDevice, zone.AudioDevice);
        }

        /// <summary>
        /// Deactivates a route from a input audio device specified by name to a zone
        /// </summary>
        /// <param name="sourceDeviceName"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool DeactivateRoute(string sourceDeviceName, Zone zone)
        {
            AudioDevice sourceDevice = OM.Host.AudioDeviceHandler.GetInputDeviceByName(sourceDeviceName);
            if (sourceDevice == null)
                return false;

            return DeactivateRoute(sourceDevice, zone.AudioDevice);
        }

        /// <summary>
        /// Deactivates an audio route
        /// </summary>
        /// <param name="audioRoute"></param>
        /// <returns></returns>
        public bool DeactivateRoute(AudioRoute audioRoute)
        {
            if (audioRoute == null)
                return false;

            return DeactivateRoute(audioRoute.SourceDevice, audioRoute.TargetDevice);
        }

    }
}
