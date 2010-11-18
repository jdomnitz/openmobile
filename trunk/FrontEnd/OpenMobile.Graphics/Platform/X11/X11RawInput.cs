#if LINUX
using OpenMobile.Input;
using System;
using OpenMobile.Platform.X11;
using System.Collections.Generic;

namespace OpenMobile.Platform.X11
{
    public sealed class X11RawInput : IInputDriver
    {
        XI2Driver driver=new XI2Driver();
        public X11RawInput()
        {
            driver.Initialize();
        }
        public IList<KeyboardDevice> Keyboard
        {
            get { return driver.Keyboard ; }
        }

        public System.Collections.Generic.IList<MouseDevice> Mouse
        {
            get { return driver.Mouse; }
        }

        public System.Collections.Generic.IList<JoystickDevice> Joysticks
        {
            get { throw new System.NotImplementedException(); }
        }
        bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    driver.Dispose();
                }

                disposed = true;
            }
        }
    }
}
#endif