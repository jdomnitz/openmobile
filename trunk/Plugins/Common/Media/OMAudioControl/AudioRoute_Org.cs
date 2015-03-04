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
    public class AudioRoute_Org : IDisposable
    {
        private EventWaitHandle _CancelThread = new EventWaitHandle(false, EventResetMode.AutoReset);

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
        /// The processing thread for this route
        /// </summary>
        public Thread ProcessingThread
        {
            get
            {
                return this._ProcessingThread;
            }
            set
            {
                if (this._ProcessingThread != value)
                {
                    this._ProcessingThread = value;
                }
            }
        }
        private Thread _ProcessingThread;

        /// <summary>
        /// Returns true if this route is active
        /// </summary>
        public bool IsActive 
        {
            get
            {
                if (_ProcessingThread == null)
                    return false;
                return _ProcessingThread.IsAlive;
            }
        }

        /// <summary>
        /// Creates a new audio route without activating it
        /// </summary>
        /// <param name="sourceDevice"></param>
        /// <param name="targetDevice"></param>
        public AudioRoute_Org(MMDevice sourceDevice, MMDevice targetDevice)
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

            _ProcessingThread = new Thread(() =>
            {
                try
                {
                    _CancelThread.Reset();

                    using (var capture = new WasapiCapture())
                    {
                        capture.Device = _SourceDevice;
                        capture.Initialize();

                        using (var source = new SoundInSource(capture))
                        {
                            using (var soundOut = new WasapiOut())
                            {
                                capture.Start();

                                soundOut.Device = _TargetDevice;
                                soundOut.Initialize(source);
                                soundOut.Play();

                                _CancelThread.WaitOne();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OM.Host.DebugMsg("Exception in AudioDevice Thread", ex);
                }
            });
            _ProcessingThread.Name = "OM_AudioRoute_Processing";
            _ProcessingThread.IsBackground = true;
            _ProcessingThread.SetApartmentState(ApartmentState.STA);
            _ProcessingThread.Start();
        }

        /// <summary>
        /// Deactivate the route
        /// </summary>
        public void Deactivate()
        {
            _CancelThread.Set();
            if (_ProcessingThread != null)
            {
                if (!_ProcessingThread.Join(1000))
                    _ProcessingThread.Abort();
                _ProcessingThread = null;
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
    }
}
