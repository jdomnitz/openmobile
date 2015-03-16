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
    /// <summary>
    /// An audio route
    /// </summary>
    public class AudioRoute : IDisposable
    {
        private WasapiCapture _SoundIn;
        private WasapiOut _SoundOut;
        private SoundInSource _SoundInSource;
        private bool _BlockEvents = false;

        /// <summary>
        /// The source device
        /// </summary>
        public MMDevice SourceDevice
        {
            get
            {
                return this._SourceDevice;
            }
            set
            {
                if (this._SourceDevice != value)
                {
                    this._SourceDevice = value;
                }
            }
        }
        private MMDevice _SourceDevice;

        /// <summary>
        /// The target device
        /// </summary>
        public MMDevice TargetDevice
        {
            get
            {
                return this._TargetDevice;
            }
            set
            {
                if (this._TargetDevice != value)
                {
                    this._TargetDevice = value;
                }
            }
        }
        private MMDevice _TargetDevice;

        /// <summary>
        /// Returns true if this route is active
        /// </summary>
        public bool IsActive 
        {
            get
            {
                if (_SoundOut == null)
                    return false;
                return _SoundOut.PlaybackState == PlaybackState.Playing;
            }
        }

        /// <summary>
        /// Creates a new audio route without activating it
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        public AudioRoute(MMDevice sourceDevice, MMDevice targetDevice)
        {
            _SourceDevice = sourceDevice;
            _TargetDevice = targetDevice;
        }

        /// <summary>
        /// Activate the route
        /// </summary>
        public void Activate()
        {
            if (IsActive)
                return;

            if (_SourceDevice == null || _TargetDevice == null)
                throw new Exception("A route needs both source and target device");

            _BlockEvents = false;

            try
            {
                // Declare audio input device
                _SoundIn = new WasapiCapture(false, AudioClientShareMode.Shared, 100, null, ThreadPriority.Highest);
                _SoundIn.Device = _SourceDevice;
                _SoundIn.Initialize();

                _SoundInSource = new SoundInSource(_SoundIn);
                _SoundOut = new WasapiOut();
                _SoundIn.Start();

                // Declare and start audio output device
                _SoundOut.Device = _TargetDevice;
                _SoundOut.Initialize(_SoundInSource);
                _SoundOut.Play();

                _SoundIn.Stopped += new EventHandler<RecordingStoppedEventArgs>(_SoundIn_Stopped);
                _SoundOut.Stopped += new EventHandler<PlaybackStoppedEventArgs>(_SoundOut_Stopped);
            }
            catch (Exception ex)
            {
                OM.Host.DebugMsg("Exception in AudioDevice Thread", ex);
            }
        }

        void _SoundIn_Stopped(object sender, RecordingStoppedEventArgs e)
        {
            if (e.HasError)
                OM.Host.DebugMsg("Sound input stopped unexpectedly", e.Exception);
            else
                if (!_BlockEvents)
                    OM.Host.DebugMsg(DebugMessageType.Info, "Sound input stopped unexpectedly");
        }

        void _SoundOut_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (e.HasError)
                OM.Host.DebugMsg("Sound output stopped unexpectedly", e.Exception);
            else
            {
                if (!_BlockEvents)
                    OM.Host.DebugMsg(DebugMessageType.Info, "Sound output stopped unexpectedly");
            }
        }

        /// <summary>
        /// Deactivate the route
        /// </summary>
        public void Deactivate()
        {
            try
            {
                _BlockEvents = true;
                if (_SoundOut != null)
                    _SoundOut.Dispose();
                _SoundOut = null;
                if (_SoundInSource != null)
                    _SoundInSource.Dispose();
                _SoundInSource = null;
                if (_SoundIn != null)
                    _SoundIn.Dispose();
                _SoundIn = null;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Disposes of this audio route
        /// </summary>
        public void Dispose()
        {
            Deactivate();
        }

        /// <summary>
        /// Returns true if this route matches the source and target devices
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public bool MatchesRoute(MMDevice sourceDevice, MMDevice targetDevice)
        {
            return (_SourceDevice == sourceDevice && _TargetDevice == targetDevice);
        }

        public override string ToString()
        {
            return String.Format("AudioRoute {0} -> {1}", _SourceDevice.FriendlyName, _TargetDevice.FriendlyName);
        }
    }
}
