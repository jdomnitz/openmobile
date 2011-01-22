#if LINUX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.Input;
using OpenMobile.Graphics;
using System.Threading;

namespace OpenMobile.Platform.X11
{
    internal sealed class XI2Driver : IMouseDriver,IKeyboardDriver,IDisposable
    {
        List<MouseDevice> mice = new List<MouseDevice>();
		List<KeyboardDevice> keyboards=new List<KeyboardDevice>();
        Dictionary<int, int> rawids = new Dictionary<int, int>(); // maps raw ids to mouse or keyboard ids
        readonly X11WindowInfo window;
        static int XIOpCode;
        static bool XInputMissing;
        public XI2Driver()
        {
            using (new XLock(API.DefaultDisplay))
            {
                window = new X11WindowInfo();
                window.Display = API.DefaultDisplay;
                window.Screen = Functions.XDefaultScreen(window.Display);
                window.RootWindow = Functions.XRootWindow(window.Display, window.Screen);
                window.WindowHandle = window.RootWindow;
            }

            using (XIEventMask mask = new XIEventMask(0, XIEventMasks.RawButtonPressMask |
                    XIEventMasks.RawButtonReleaseMask | XIEventMasks.RawMotionMask |
			        XIEventMasks.RawKeyPressMask| XIEventMasks.RawKeyReleaseMask))
		            {
                        try
                        {
                            Functions.XISelectEvents(window.Display, window.WindowHandle, mask);
                        }
                        catch (DllNotFoundException)
                        {
                            XInputMissing = true;
                        }
		            }
        }
        public void Initialize()
        {
            if (XInputMissing)
                return;
            IntPtr Display = Functions.XOpenDisplay(IntPtr.Zero);
            int count = 0;
            if (IsSupported(Display))
            {
                IntPtr tmp = Functions.XIQueryDevice(Display, 0, out count);
                if (IntPtr.Size == 4)
                    count *= 28;
                else
                    count *= 36;
				byte[] buf=new byte[count];
				Marshal.Copy(tmp,buf,0,count);
                Functions.XIFreeDeviceInfo(tmp);
                int dummy = 0;
                for (int pos = 0; pos < count; pos+=28)
                {
					int i=buf[pos];
                    IntPtr devPtr = Functions.XIQueryDevice(Display, i, out dummy);
                    XIDeviceInfo devs = (XIDeviceInfo)Marshal.PtrToStructure(devPtr, typeof(XIDeviceInfo));
                    XIValuatorInfo xInfo = new XIValuatorInfo();
                    XIValuatorInfo yInfo = new XIValuatorInfo();
                    if (devs.num_classes > 0)
                    {
                        IntPtr[] classPtrs = new IntPtr[devs.num_classes];
                        Marshal.Copy(devs.type, classPtrs, 0, devs.num_classes);
                        for (int j = 0; j < devs.num_classes; j++)
                        {
                            XIAnyClassInfo info = (XIAnyClassInfo)Marshal.PtrToStructure(classPtrs[j], typeof(XIAnyClassInfo));
                            if (info.type == 2)
                            {
                                XIValuatorInfo valInfo = (XIValuatorInfo)Marshal.PtrToStructure(classPtrs[j], typeof(XIValuatorInfo));
                                switch (Functions.XGetAtomName(Display, valInfo.label))
                                {
                                    case "Rel X":
                                    case "Abs X":
                                        xInfo = valInfo;
                                        break;
                                    case "Rel Y":
                                    case "Abs Y":
                                        yInfo = valInfo;
                                        break;
                                }
                            }
                        }
                    }
                    if (devs.enabled)
                    {
                        if (devs.use == 3)
                        {
                            MouseDevice dev = new MouseDevice();
                            dev.Description = devs.name;
                            dev.DeviceID = new IntPtr(i);
                            dev.Instance = mice.Count;
                            if (xInfo.label > 0)
                            {
                                dev.minx = xInfo.min;
                                dev.maxx = xInfo.max;
                                dev.Absolute = xInfo.mode != 0;
                            }
                            if (yInfo.label > 0)
                            {
                                dev.miny = yInfo.min;
                                dev.maxy = yInfo.max;
                            }
                            if (!rawids.ContainsKey(i))
                            {
                                mice.Add(dev);
                                rawids.Add(i, mice.Count - 1);
                            }
                        }
                        else if (devs.use == 4)
                        {
                            KeyboardDevice dev = new KeyboardDevice();
                            dev.Description = devs.name;
                            dev.DeviceID = new IntPtr(i);
                            dev.Instance = keyboards.Count;
                            if (!rawids.ContainsKey(i))
                            {
                                keyboards.Add(dev);
                                rawids.Add(i, keyboards.Count - 1);
                            }
                        }
                    }
                    Functions.XIFreeDeviceInfo(devPtr);
                }
                new Thread(delegate() { ProcessEvents(); }).Start();
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

        public IList<MouseDevice> Mouse 
        { 
            get 
            {
                return mice; 
            } 
        }
		public IList<KeyboardDevice> Keyboard
		{
			get
			{
				return keyboards;
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
					switch(raw.evtype)
					{
					case XIEventType.RawMotion:
					case XIEventType.RawButtonPress:
					case XIEventType.RawButtonRelease:
	                    MouseDevice state = mice[rawids[raw.deviceid]];
						switch (raw.evtype)
	                    {
	                        case XIEventType.RawMotion:

	                            Point current=state.Location;
								int x=0,y=0;
	                            if (IsBitSet(raw.valuators.mask, 0))
	                                x= (int)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(raw.raw_values, 0));
	                            if (IsBitSet(raw.valuators.mask, 1))
	                                y= (int)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(raw.raw_values, 8));
	                            if (state.Absolute)
								{
									if (state.minx!=state.miny)
									{
										x=(int)(((x-state.minx)/state.minx)*state.Width);
										y=(int)(((y-state.miny)/state.miny)*state.Height);
									}
								}
								current.X+=x;
								current.Y+=y;
								state.SetPosition(current.X,current.Y);
	                            break;
	
	                        case XIEventType.RawButtonPress:
	                            switch (raw.detail)
	                            {
	                                case 1: state[MouseButton.Left]=true; break;
	                                case 2: state[MouseButton.Middle]=true; break;
	                                case 3: state[MouseButton.Right]=true; break;
	                            }
	                            break;
	
	                        case XIEventType.RawButtonRelease:
	                            switch (raw.detail)
	                            {
	                                case 1: state[MouseButton.Left] = false; break;
	                                case 2: state[MouseButton.Middle] = false; break;
	                                case 3: state[MouseButton.Right] = false; break;
	                            }
	                            break;
	                    }
                    	mice[rawids[raw.deviceid]] = state;
						break;
					case XIEventType.RawKeyPress:
					case XIEventType.RawKeyRelease:
						KeyboardDevice state2 = keyboards[rawids[raw.deviceid]];
						IntPtr sym=API.XKeycodeToKeysym(window.Display,raw.detail,0);
						Key key=X11Input.keymap[(XKey)sym];
	                    switch (raw.evtype)
	                    {
							case XIEventType.RawKeyPress:
								if (key==Key.CapsLock)
									state2[Key.CapsLock]=state2.CapsLock;
								else
									state2[key]=true;
								break;
							case XIEventType.RawKeyRelease:
								if (key!=Key.CapsLock)
									state2[key]=false;
								break;
	                    }
	                    keyboards[rawids[raw.deviceid]] = state2;
						break;
					}
                }
                Functions.XFreeEventData(window.Display, ref cookie);
            }
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
#endif