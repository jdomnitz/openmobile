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
// WARNING: Modified HEAVILY for use in the OpenMobile Project
#endregion
#if WINDOWS
#region --- Using directives ---

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using OpenMobile.Input;

#endregion

namespace OpenMobile.Platform.Windows
{
    internal class WinRawKeyboard : IKeyboardDriver, IDisposable
    {
        //readonly List<KeyboardState> keyboards = new List<KeyboardState>();
        internal static readonly WinKeyMap KeyMap = new WinKeyMap();
        // ContextHandle instead of IntPtr for fast dictionary access
        readonly Dictionary<ContextHandle, int> rawids = new Dictionary<ContextHandle, int>();
        private List<KeyboardDevice> keyboards = new List<KeyboardDevice>();
        private IntPtr window;
        readonly object UpdateLock = new object();

        #region --- Constructors ---

        internal WinRawKeyboard()
            : this(IntPtr.Zero)
        {
        }

        internal WinRawKeyboard(IntPtr windowHandle)
        {
            //Debug.WriteLine("Initializing keyboard driver (WinRawKeyboard).");
            //Debug.Indent();

            this.window = windowHandle;

            UpdateKeyboardList();

            //Debug.Unindent();
        }

        #endregion

        #region UpdateKeyboardList

        internal void UpdateKeyboardList()
        {
            int count = WinRawInput.DeviceCount;
            RawInputDeviceList[] ridl = new RawInputDeviceList[count];
            for (int i = 0; i < count; i++)
                ridl[i] = new RawInputDeviceList();
            Functions.GetRawInputDeviceList(ridl, ref count, API.RawInputDeviceListSize);

            // Discover keyboard devices:
            for (int i = 0; i < count; i++)
            {
                uint size = 0;
                Functions.GetRawInputDeviceInfo(ridl[i].Device, RawInputDeviceInfoEnum.DEVICENAME, IntPtr.Zero, ref size);
                IntPtr name_ptr = Marshal.AllocHGlobal((IntPtr)size);
                Functions.GetRawInputDeviceInfo(ridl[i].Device, RawInputDeviceInfoEnum.DEVICENAME, name_ptr, ref size);
                string name = Marshal.PtrToStringAnsi(name_ptr);
                Marshal.FreeHGlobal(name_ptr);
                if (name.ToLower().Contains("root"))
                {
                    // This is a terminal services device, skip it.
                    continue;
                }
                else if (ridl[i].Type == RawInputDeviceType.KEYBOARD || ridl[i].Type == RawInputDeviceType.HID)
                {
                    // This is a keyboard or USB keyboard device. In the latter case, discover if it really is a
                    // keyboard device by qeurying the registry.

                    // remove the \??\
                    name = name.Substring(4);

                    string[] split = name.Split('#');

                    string id_01 = split[0];    // ACPI (Class code)
                    string id_02 = split[1];    // PNP0303 (SubClass code)
                    string id_03 = split[2];    // 3&13c0b0c5&0 (Protocol code)
                    // The final part is the class GUID and is not needed here

                    string findme = string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}",id_01, id_02, id_03);

                    RegistryKey regkey = Registry.LocalMachine.OpenSubKey(findme);

                    string deviceDesc = (string)regkey.GetValue("DeviceDesc");
                    string deviceClass = (string)regkey.GetValue("Class");
                    if (!String.IsNullOrEmpty(deviceClass) && deviceClass.ToLower().Equals("keyboard"))
                    {
                        KeyboardDevice kb = new KeyboardDevice();
                        // Register the keyboard:
                        string[] parts = deviceDesc.Split(new char[] { ';' });
                        if (parts.Length == 2)
                            kb.Description = parts[1] + " (" + id_02.GetHashCode().ToString() + ")";
                        else
                            kb.Description = deviceDesc;

                        kb.DeviceID = (IntPtr)id_02.GetHashCode();
                        this.RegisterKeyboardDevice(kb);
                        kb.Instance = keyboards.Count;
                        keyboards.Add(kb);
                        rawids.Add(new ContextHandle(ridl[i].Device), keyboards.Count - 1);
                    }
                }
            }
        }

        #endregion

        #region internal void RegisterKeyboardDevice(Keyboard kb)

        internal void RegisterKeyboardDevice(KeyboardDevice kb)
        {
            RawInputDevice[] rid = new RawInputDevice[1];
            // Keyboard is 1/6 (page/id). See http://www.microsoft.com/whdc/device/input/HID_HWID.mspx
            rid[0] = new RawInputDevice();
            rid[0].UsagePage = 1;
            rid[0].Usage = 6;
            rid[0].Flags = RawInputDeviceFlags.INPUTSINK;
            rid[0].Target = window;

            if (!Functions.RegisterRawInputDevices(rid, 1, API.RawInputDeviceSize))
            {
                Debug.Print(
                    String.Format(
                        "Raw input registration failed with error: {0}. Device: {1}",
                        Marshal.GetLastWin32Error(),
                        rid[0].ToString())
                );
            }
            #if DEBUG
            else
            {
                Debug.Print("Registered keyboard {0}", kb.ToString());
            }
            #endif
        }

        #endregion

        #region internal bool ProcessKeyboardEvent(API.RawInput rin)

        /// <summary>
        /// Processes raw input events.
        /// </summary>
        /// <param name="rin"></param>
        /// <returns></returns>
        internal bool ProcessKeyboardEvent(RawInput rin)
        {
            bool processed = false;

            bool pressed =
                rin.Data.Keyboard.Message == (int)WindowMessage.KEYDOWN ||
                rin.Data.Keyboard.Message == (int)WindowMessage.SYSKEYDOWN;

            ContextHandle handle = new ContextHandle(rin.Header.Device);
            KeyboardDevice keyboard;
            if (!rawids.ContainsKey(handle))
            {
                return false;
            }
            keyboard = keyboards[rawids[handle]];

            // Generic control, shift, alt keys may be sent instead of left/right.
            // It seems you have to explicitly register left/right events.
            switch (rin.Data.Keyboard.VKey)
            {
                case VirtualKeys.SHIFT:
                    keyboard[Input.Key.ShiftLeft] = keyboard[Input.Key.ShiftRight] = pressed;
                    processed = true;
                    break;

                case VirtualKeys.CONTROL:
                    keyboard[Input.Key.ControlLeft] = keyboard[Input.Key.ControlRight] = pressed;
                    processed = true;
                    break;

                case VirtualKeys.MENU:
                    keyboard[Input.Key.AltLeft] = keyboard[Input.Key.AltRight] = pressed;
                    processed = true;
                    break;

                default:
                    if (!KeyMap.ContainsKey(rin.Data.Keyboard.VKey))
                    {
                        Debug.Print("Virtual key {0} ({1}) not mapped.",
                                    rin.Data.Keyboard.VKey, (int)rin.Data.Keyboard.VKey);
                    }
                    else
                    {
                        if (KeyMap[rin.Data.Keyboard.VKey] == Key.CapsLock)
                        {
                            if(pressed)
                                keyboard[Key.CapsLock] = !keyboard[Key.CapsLock];
                        }
                        else
                            keyboard[KeyMap[rin.Data.Keyboard.VKey]] = pressed;
                        processed = true;
                    }
                    break;
            }

            lock (UpdateLock)
            {
                keyboards[rawids[handle]] = keyboard;
                return processed;
            }
        }

        #endregion

        #region --- IKeyboardDriver Members ---

        public IList<KeyboardDevice> Keyboard
        {
            get { return keyboards; }
        }

        #endregion

        #region --- IDisposable Members ---

        private bool disposed;

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
                    keyboards.Clear();
                }
                disposed = true;
            }
        }

        ~WinRawKeyboard()
        {
            Dispose(false);
        }

        #endregion
    }
}
#endif