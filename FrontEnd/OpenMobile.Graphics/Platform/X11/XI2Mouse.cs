#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2010 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenMobile.Input;
using OpenMobile.Graphics;
using System.Threading;

namespace OpenMobile.Platform.X11
{
    internal sealed class XI2Mouse : IMouseDriver,IDisposable
    {
        List<MouseDevice> mice = new List<MouseDevice>();
        Dictionary<int, int> rawids = new Dictionary<int, int>(); // maps raw ids to mouse ids
        readonly X11WindowInfo window;
        static int XIOpCode;

        public XI2Mouse()
        {
            Debug.WriteLine("Using XI2Mouse.");

            using (new XLock(API.DefaultDisplay))
            {
                window = new X11WindowInfo();
                window.Display = API.DefaultDisplay;
                window.Screen = Functions.XDefaultScreen(window.Display);
                window.RootWindow = Functions.XRootWindow(window.Display, window.Screen);
                window.WindowHandle = window.RootWindow;
            }

            using (XIEventMask mask = new XIEventMask(0, XIEventMasks.RawButtonPressMask |
                    XIEventMasks.RawButtonReleaseMask | XIEventMasks.RawMotionMask))
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
	            Debug.Print(count + " available devices");;
				int dummy;
				for(int i=0;i<count;i++)
				{
					IntPtr devPtr=Functions.XIQueryDevice(window.Display,i+2,out dummy);
					XIDeviceInfo devs=(XIDeviceInfo) Marshal.PtrToStructure(devPtr,typeof(XIDeviceInfo));
					XIValuatorInfo xInfo=new XIValuatorInfo();
					XIValuatorInfo yInfo=new XIValuatorInfo();
					if (devs.num_classes>0)
					{
						IntPtr[] classPtrs=new IntPtr[devs.num_classes];
						Marshal.Copy(devs.type,classPtrs,0,devs.num_classes);
						for(int j=0;j<devs.num_classes;j++)
						{
							XIAnyClassInfo info=(XIAnyClassInfo)Marshal.PtrToStructure(classPtrs[j],typeof(XIAnyClassInfo));
							if (info.type==2)
							{
								XIValuatorInfo valInfo=(XIValuatorInfo)Marshal.PtrToStructure(classPtrs[j],typeof(XIValuatorInfo));
								switch(Functions.XGetAtomName(Display,valInfo.label))
								{
								case "Rel X":
								case "Abs X":	
									xInfo=valInfo;
									break;
								case "Rel Y":
								case "Abs Y":
									yInfo=valInfo;
									break;
								}
							}
						}
					}
					if (devs.enabled)
					{
						if (devs.use==3)
						{
							MouseDevice dev=new MouseDevice();
							dev.Description=devs.name;
							dev.DeviceID=new IntPtr(i+2);
							dev.Instance=mice.Count;
							dev.NumberOfButtons=2;
							if (xInfo.label>0)
							{
								dev.minx=xInfo.min;
								dev.maxx=xInfo.max;
								dev.Absolute=xInfo.mode!=0;
							}
							if (yInfo.label>0)
							{
								dev.miny=yInfo.min;
								dev.maxy=yInfo.max;
							}
							if (!rawids.ContainsKey(i+2))
							{
								mice.Add(dev);
								rawids.Add(i+2,mice.Count-1);
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

        public IList<MouseDevice> Mouse 
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
							state.Position = current;
                            break;

                        case XIEventType.RawButtonPress:
                            switch (raw.detail)
                            {
                                case 1: state[MouseButton.Left]=true; break;
                                case 2: state[MouseButton.Middle]=true; break;
                                case 3: state[MouseButton.Right]=true; break;
                                case 4: state.WheelPrecise++; break;
                                case 5: state.WheelPrecise--; break;
                                case 6: state[MouseButton.Button1]=true; break;
                                case 7: state[MouseButton.Button2]=true; break;
                                case 8: state[MouseButton.Button3]=true; break;
                                case 9: state[MouseButton.Button4]=true; break;
                                case 10: state[MouseButton.Button5]=true; break;
                                case 11: state[MouseButton.Button6]=true; break;
                                case 12: state[MouseButton.Button7]=true; break;
                                case 13: state[MouseButton.Button8]=true; break;
                                case 14: state[MouseButton.Button9]=true; break;
                            }
                            break;

                        case XIEventType.RawButtonRelease:
                            switch (raw.detail)
                            {
                                case 1: state[MouseButton.Left] = false; break;
                                case 2: state[MouseButton.Middle] = false; break;
                                case 3: state[MouseButton.Right] = false; break;
                                case 6: state[MouseButton.Button1] = false; break;
                                case 7: state[MouseButton.Button2] = false; break;
                                case 8: state[MouseButton.Button3] = false; break;
                                case 9: state[MouseButton.Button4] = false; break;
                                case 10: state[MouseButton.Button5] = false; break;
                                case 11: state[MouseButton.Button6] = false; break;
                                case 12: state[MouseButton.Button7] = false; break;
                                case 13: state[MouseButton.Button8] = false; break;
                                case 14: state[MouseButton.Button9] = false; break;
                            }
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