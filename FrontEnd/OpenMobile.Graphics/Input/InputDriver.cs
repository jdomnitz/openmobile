#region --- License ---
/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion

using System;
using System.Collections.Generic;
using System.Text;

using OpenMobile.Input;
using OpenMobile.Platform;

namespace OpenMobile
{
    internal class InputDriver : IInputDriver
    {
        private IInputDriver inputDriver;

        #region --- Constructors ---
        
        public InputDriver(GameWindow parent)
        {
            if (parent == null)
                throw new ArgumentException("A valid window (IWindowInfo) must be specified to construct an InputDriver");

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.WinCE:
                    if (Environment.OSVersion.Version.Major > 5 ||
                        (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1))
                    {
                        //inputDriver = new OpenMobile.Platform.Windows.WinRawInput((OpenMobile.Platform.Windows.WinWindowInfo)parent.WindowInfo);
                    }
                    break;

                case PlatformID.Unix:
                    // TODO: Input is currently handled asychronously by the driver in X11GLNative.
                    //inputDriver = new OpenMobile.Platform.X11.X11Input(parent.WindowInfo);
                    
                    break;

                default:
                    throw new PlatformNotSupportedException(
    "Input handling is not supported on the current platform. Please report the problem to http://OpenMobile.sourceforge.net");

            }
        }
        
        #endregion

        #region --- IInputDriver Members ---

        public void Poll()
        {
            inputDriver.Poll();
        }

        #endregion

        #region --- IKeyboardDriver Members ---

        public IList<KeyboardDevice> Keyboard
        {
            get { return inputDriver.Keyboard; }
        }

        #endregion

        #region --- IMouseDriver Members ---

        public IList<MouseDevice> Mouse
        {
            get { return inputDriver.Mouse; }
        }

        #endregion

        #region --- IJoystickDriver Members ---

        public IList<JoystickDevice> Joysticks
        {
            get { return inputDriver.Joysticks; }
        }

        #endregion

        #region --- IDisposable Members ---

        private bool disposed;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    inputDriver.Dispose();
                }

                disposed = true;
            }
        }

        ~InputDriver()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
