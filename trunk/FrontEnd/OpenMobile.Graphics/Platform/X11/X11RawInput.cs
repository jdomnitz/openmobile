using OpenMobile.Input;
using System;
using OpenMobile.Platform.X11;

namespace OpenMobile.Platform.X11
{
    public sealed class X11RawInput : IInputDriver
    {
        XI2Mouse mouseDriver = new XI2Mouse();
        public X11RawInput()
        {
            mouseDriver.Initialize();
        }
        public System.Collections.Generic.IList<KeyboardDevice> Keyboard
        {
            get { throw new System.NotImplementedException(); }
        }

        public System.Collections.Generic.IList<MouseDevice> Mouse
        {
            get { return mouseDriver.Mouse; }
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
                    //keyboardDriver.Dispose();
                    mouseDriver.Dispose();
                    //if (Native != null)
                    //    Native.Dispose();
                }

                disposed = true;
            }
        }
    }
}
