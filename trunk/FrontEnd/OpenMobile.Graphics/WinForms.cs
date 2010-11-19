using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
            #if WINDOWS
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
            #endif
        }
        public static void ShowError(object window, string text, string title)
        {
            if (Configuration.RunningOnWindows)
            {
                MessageBox((IntPtr)window, text, title, 0x10);
            }
            else if (Configuration.RunningOnLinux)
            {
				if (File.Exists("/usr/bin/zenity"))
				{
	                try
					{
						Process p=Process.Start("zenity","--error --text='"+text+"' --title='"+title+"'");
						p.WaitForExit();
					}
					catch(Exception){}
				}else if (File.Exists("/usr/bin/kdialog"))
				{
					 try
					{
						Process p=Process.Start("kdialog","--caption '"+title+"' --error '"+text+"'");
						p.WaitForExit();
					}
					catch(Exception){}
				}
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }

    public class TouchEventArgs : EventArgs
    {
        string _name;
        double _arg1;
        string _arg2;
        Point _pos;
        public TouchEventArgs(string name, Point pos,double arg1, string arg2)
        {
            _name = name;
            _arg1 = arg1;
            _arg2 = arg2;
            _pos = pos;
        }
        public Point Position
        {
            get { return _pos; }
        }
        public string Name
        {
            get { return _name; }
        }
        public double Arg1
        {
            get { return _arg1; }
        }
        public string Arg2
        {
            get { return _arg2; }
        }
    }
    public class ResolutionChange : EventArgs
    {
        int _width, _height;
        public ResolutionChange(int width, int height)
        {
            _width = width;
            _height = height;
        }
        public int Width
        {
            get { return _width; }
        }
        public int Height
        {
            get { return _height; }
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
