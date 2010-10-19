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
                Thread.Sleep(0);
                OpenMobile.Platform.Windows.MSG msg = new OpenMobile.Platform.Windows.MSG();
                while (msg.Message!=Platform.Windows.WindowMessage.CLOSE)
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
        public static void ShowError(object window, string text, string title)
        {
            if (Configuration.RunningOnWindows)
            {
                MessageBox((IntPtr)window, text, title, 0x10);
            }
            else if (Configuration.RunningOnLinux)
            {
                //TODO
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
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
