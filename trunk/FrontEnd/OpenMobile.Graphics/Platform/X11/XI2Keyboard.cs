using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.Input;
using OpenMobile.Graphics;
using System.Threading;

namespace OpenMobile.Platform.X11
{
    internal sealed class XI2Keyboard : IKeyboardDriver,IDisposable
    {
        List<KeyboardDevice> mice = new List<KeyboardDevice>();
        Dictionary<int, int> rawids = new Dictionary<int, int>(); // maps raw ids to mouse ids
        readonly X11WindowInfo window;
        static int XIOpCode;

        public XI2Keyboard()
        {
            Debug.WriteLine("Using XI2Keyboard.");

            using (new XLock(API.DefaultDisplay))
            {
                window = new X11WindowInfo();
                window.Display = Functions.XOpenDisplay(IntPtr.Zero);
                window.Screen = Functions.XDefaultScreen(window.Display);
                window.RootWindow = Functions.XRootWindow(window.Display, window.Screen);
                window.WindowHandle = window.RootWindow;
            }

            using (XIEventMask mask = new XIEventMask(0, XIEventMasks.RawKeyPressMask | XIEventMasks.RawKeyReleaseMask))
            {
                Functions.XISelectEvents(window.Display, window.WindowHandle, mask);
            }
        }
        public void Initialize()
        {
			IntPtr Display=Functions.XOpenDisplay(IntPtr.Zero);
            int count=4;
			if (IsSupported(Display))
			{
	            IntPtr tmp= Functions.XIQueryDevice(Display,0,out count);
				Functions.XIFreeDeviceInfo(tmp);
				int dummy=0;
				for(int i=2;i<count;i++)
				{
					IntPtr devPtr=Functions.XIQueryDevice(Display,i,out dummy);
					XIDeviceInfo devs=(XIDeviceInfo) Marshal.PtrToStructure(devPtr,typeof(XIDeviceInfo));
					if (devs.enabled)
					{
						if (devs.use==4)
						{
							KeyboardDevice dev=new KeyboardDevice();
							dev.Description=devs.name;
							dev.DeviceID=new IntPtr(i);
							dev.Instance=mice.Count;
							if (!rawids.ContainsKey(i))
							{
								mice.Add(dev);
								rawids.Add(i,mice.Count-1);
							}
						}
					}
					Functions.XIFreeDeviceInfo(devPtr);
				}
				new Thread(delegate(){ ProcessEvents();}).Start();
			}
        }
        // Checks whether XInput2 is supported on the specified display.
        // If a display is not specified, the default display is used.
        internal static bool IsSupported(IntPtr display)
        {
            using (new XLock(display))
            {
                int major=2;
				int ev, error;
                if (Functions.XQueryExtension(display, "XInputExtension", out major, out ev, out error) == 0)
                {
                    return false;
                }
                XIOpCode = major;
            }

            return true;
        }

        public IList<KeyboardDevice> Keyboard 
        { 
            get 
            {
                return mice; 
            } 
        }

        void ProcessEvents()
        {
            while (true)
            {
                XEvent e = new XEvent();
                XGenericEventCookie cookie;

                if (Functions.XNextEvent(window.Display, ref e)!=IntPtr.Zero)
                    return;
				if (e.GenericEvent.extension!=XIOpCode)
					continue;
                cookie = e.GenericEventCookie;
                if (Functions.XGetEventData(window.Display, ref cookie) != 0)
                {
                    XIRawEvent raw = (XIRawEvent)
                        Marshal.PtrToStructure(cookie.data, typeof(XIRawEvent));

                    if (!rawids.ContainsKey(raw.deviceid))
                    {
                        continue;
                    }
                    KeyboardDevice state = mice[rawids[raw.deviceid]];
					IntPtr sym=API.XKeycodeToKeysym(window.Display,raw.detail,0);
                    switch (raw.evtype)
                    {
						case XIEventType.RawKeyPress:
							state[X11Input.keymap[(XKey)sym]]=true;
							break;
						case XIEventType.RawKeyRelease:
							state[X11Input.keymap[(XKey)sym]]=false;
							break;
                    }
                    mice[rawids[raw.deviceid]] = state;
                }
                Functions.XFreeEventData(window.Display, ref cookie);
            }
        }

        static bool IsEventValid(IntPtr display, ref XEvent e, IntPtr arg)
        {
            return e.GenericEventCookie.extension == arg.ToInt32() &&
                (e.GenericEventCookie.evtype == (int)XIEventType.RawMotion ||
                e.GenericEventCookie.evtype == (int)XIEventType.RawButtonPress ||
                e.GenericEventCookie.evtype == (int)XIEventType.RawButtonRelease);
        }

        static bool IsBitSet(IntPtr mask, int bit)
        {
            unsafe
            {
                return (*((byte*)mask + (bit >> 3)) & (1 << (bit & 7))) != 0;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //TODO
        }

        #endregion
    }
}