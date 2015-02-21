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


namespace OMAudioControl
{
    [SupportedOSConfigurations(OSConfigurationFlags.Windows)]
    public sealed class OMAudioControl : BasePluginCode, IAudioDeviceProvider
    {
        private AudioRouteHandler _AudioRouteHandler;

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

            // Start audio handler
            _AudioRouteHandler = new AudioRouteHandler();

            return eLoadStatus.LoadSuccessful;
        }

        public override void Dispose()
        {
            _AudioRouteHandler.Dispose();

            base.Dispose();
        }


        public AudioDevice[] OutputDevices
        {
            get 
            {
                return _AudioRouteHandler.OutputDevices.ToArray();
            }
        }

        public AudioDevice[] InputDevices
        {
            get 
            {
                return _AudioRouteHandler.InputDevices.ToArray();
            }
        }

        public bool ActivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            return _AudioRouteHandler.ActivateRoute(sourceDevice, targetDevice);
        }

        public bool DeactivateRoute(AudioDevice sourceDevice, AudioDevice targetDevice)
        {
            return _AudioRouteHandler.DeactivateRoute(sourceDevice, targetDevice);
        }
    }
}
