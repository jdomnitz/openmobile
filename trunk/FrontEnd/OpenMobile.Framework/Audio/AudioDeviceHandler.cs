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
                    _AudioDeviceProvider = plugins[0] as IAudioDeviceProvider;

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
                if (_AudioDeviceProvider != null)
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
                if (_AudioDeviceProvider != null)
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
        public bool ActivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            if (_AudioDeviceProvider == null)
                return false;

            return _AudioDeviceProvider.ActivateRoute(sourceDevice, targetDevice);
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
                return false;

            return _AudioDeviceProvider.DeactivateRoute(sourceDevice, targetDevice);
        }

    }
}
