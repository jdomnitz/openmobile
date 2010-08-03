using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

namespace OpenMobile.Graphics
{
    public static class Application
    {
        public static string StartupPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }
        public static void Run()
        {
            int ret;
            if (Configuration.RunningOnWindows)
            {
                OpenMobile.Platform.Windows.MSG msg = new OpenMobile.Platform.Windows.MSG();
                while (true)
                {
                    ret = OpenMobile.Platform.Windows.Functions.GetMessage(ref msg, IntPtr.Zero, 0, 0);
                    if (ret == -1)
                    {
                        throw new PlatformException(String.Format(
                            "An error happened while processing the message queue. Windows error: {0}",
                            Marshal.GetLastWin32Error()));
                    }

                    OpenMobile.Platform.Windows.Functions.TranslateMessage(ref msg);
                    OpenMobile.Platform.Windows.Functions.DispatchMessage(ref msg);
                }
            }
        }
    }
    public class KeyPressEventArgs : EventArgs
    {
        private bool handled;
        private char keychar;
        public KeyPressEventArgs(char keyChar)
        {
            keychar = keyChar;
        }
        public bool Handled
        {
            get{ return handled; }
            set { handled = value; }
        }
        public char KeyChar 
        {
            get { return keychar; }
            set { keychar = value; }
        }
    }
}
