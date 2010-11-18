#region --- License ---
/* Copyright (c) 2006, 2007 Stefanos Apostolopoulos
 * See license.txt for license info
 */
#endregion
#if LINUX
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using OpenMobile.Input;
using OpenMobile.Graphics;

namespace OpenMobile.Platform.X11
{
    /// \internal
    /// <summary>
    /// Drives the InputDriver on X11.
    /// This class supports OpenMobile, and is not intended for users of OpenMobile.
    /// </summary>
    internal sealed class X11Input:IInputDriver
    {
        KeyboardDevice keyboard = new KeyboardDevice();
        MouseDevice mouse = new MouseDevice();
        List<KeyboardDevice> dummy_keyboard_list = new List<KeyboardDevice>(1);
        List<MouseDevice> dummy_mice_list = new List<MouseDevice>(1);

        internal static X11KeyMap keymap = new X11KeyMap();

        #region --- Constructors ---

        /// <summary>
        /// Constructs a new X11Input driver. Creates a hidden InputOnly window, child to
        /// the main application window, which selects input events and routes them to 
        /// the device specific drivers (Keyboard, Mouse, Hid).
        /// </summary>
        /// <param name="attach">The window which the InputDriver will attach itself on.</param>
        public X11Input(IWindowInfo attach)
        {
            Debug.WriteLine("Initalizing X11 input driver.");
            Debug.Indent();

            if (attach == null)
                throw new ArgumentException("A valid parent window must be defined, in order to create an X11Input driver.");

            //window = new X11WindowInfo(attach);
            X11WindowInfo window = (X11WindowInfo)attach;

            // Init mouse
            mouse.Description = "Default Mouse";
            mouse.DeviceID = IntPtr.Zero;
            dummy_mice_list.Add(mouse);
            
            using (new XLock(window.Display))
            {
                // Init keyboard
                KeyboardDevice kb = new KeyboardDevice();
                keyboard.Description = "Default Keyboard";
                keyboard.DeviceID = IntPtr.Zero;
                dummy_keyboard_list.Add(keyboard);
    
                // Request that auto-repeat is only set on devices that support it physically.
                // This typically means that it's turned off for keyboards (which is what we want).
                // We prefer this method over XAutoRepeatOff/On, because the latter needs to
                // be reset before the program exits.
                bool supported;
                Functions.XkbSetDetectableAutoRepeat(window.Display, true, out supported);
            }

            Debug.Unindent();
        }

        #endregion

        #region internal void ProcessEvent(ref XEvent e)

        internal void ProcessEvent(ref XEvent e)
        {
            switch (e.type)
            {
                case XEventName.KeyPress:
                case XEventName.KeyRelease:
                    bool pressed = e.type == XEventName.KeyPress;
					
                    IntPtr keysym = API.LookupKeysym(ref e.KeyEvent, 0);
                    IntPtr keysym2 = API.LookupKeysym(ref e.KeyEvent, 1);

                    if (keymap.ContainsKey((XKey)keysym))
                        keyboard[keymap[(XKey)keysym]] = pressed;
                    else if (keymap.ContainsKey((XKey)keysym2))
                        keyboard[keymap[(XKey)keysym2]] = pressed;
                    else
                        Debug.Print("KeyCode {0} (Keysym: {1}, {2}) not mapped.", e.KeyEvent.keycode, (XKey)keysym, (XKey)keysym2);
                    keyboard[Key.CapsLock] = ((e.KeyEvent.state & 2) == 2);
                    break;

                case XEventName.ButtonPress:
                    if      (e.ButtonEvent.button == 1) mouse[OpenMobile.Input.MouseButton.Left] = true;
                    else if (e.ButtonEvent.button == 2) mouse[OpenMobile.Input.MouseButton.Middle] = true;
                    else if (e.ButtonEvent.button == 3) mouse[OpenMobile.Input.MouseButton.Right] = true;
                    else if (e.ButtonEvent.button == 4) mouse.Wheel++;
                    else if (e.ButtonEvent.button == 5) mouse.Wheel--;
                    else if (e.ButtonEvent.button == 6) mouse[OpenMobile.Input.MouseButton.Button1] = true;
                    else if (e.ButtonEvent.button == 7) mouse[OpenMobile.Input.MouseButton.Button2] = true;
                    else if (e.ButtonEvent.button == 8) mouse[OpenMobile.Input.MouseButton.Button3] = true;
                    else if (e.ButtonEvent.button == 9) mouse[OpenMobile.Input.MouseButton.Button4] = true;
                    else if (e.ButtonEvent.button == 10) mouse[OpenMobile.Input.MouseButton.Button5] = true;
                    else if (e.ButtonEvent.button == 11) mouse[OpenMobile.Input.MouseButton.Button6] = true;
                    else if (e.ButtonEvent.button == 12) mouse[OpenMobile.Input.MouseButton.Button7] = true;
                    else if (e.ButtonEvent.button == 13) mouse[OpenMobile.Input.MouseButton.Button8] = true;
                    else if (e.ButtonEvent.button == 14) mouse[OpenMobile.Input.MouseButton.Button9] = true;
                    //if ((e.state & (int)X11.MouseMask.Button4Mask) != 0) m.Wheel++;
                    //if ((e.state & (int)X11.MouseMask.Button5Mask) != 0) m.Wheel--;
                    //Debug.Print("Button pressed: {0}", e.ButtonEvent.button);
                    break;

                case XEventName.ButtonRelease:
                    if      (e.ButtonEvent.button == 1) mouse[OpenMobile.Input.MouseButton.Left] = false;
                    else if (e.ButtonEvent.button == 2) mouse[OpenMobile.Input.MouseButton.Middle] = false;
                    else if (e.ButtonEvent.button == 3) mouse[OpenMobile.Input.MouseButton.Right] = false;
                    else if (e.ButtonEvent.button == 6) mouse[OpenMobile.Input.MouseButton.Button1] = false;
                    else if (e.ButtonEvent.button == 7) mouse[OpenMobile.Input.MouseButton.Button2] = false;
                    else if (e.ButtonEvent.button == 8) mouse[OpenMobile.Input.MouseButton.Button3] = false;
                    else if (e.ButtonEvent.button == 9) mouse[OpenMobile.Input.MouseButton.Button4] = false;
                    else if (e.ButtonEvent.button == 10) mouse[OpenMobile.Input.MouseButton.Button5] = false;
                    else if (e.ButtonEvent.button == 11) mouse[OpenMobile.Input.MouseButton.Button6] = false;
                    else if (e.ButtonEvent.button == 12) mouse[OpenMobile.Input.MouseButton.Button7] = false;
                    else if (e.ButtonEvent.button == 13) mouse[OpenMobile.Input.MouseButton.Button8] = false;
                    else if (e.ButtonEvent.button == 14) mouse[OpenMobile.Input.MouseButton.Button9] = false;
                    break;

                case XEventName.MotionNotify:
                    mouse.Position = new Point(e.MotionEvent.x, e.MotionEvent.y);
                    break;
            }
        }

        #endregion

        #region --- IInputDriver Members ---

        #region public IList<Keyboard> Keyboard

        public IList<KeyboardDevice> Keyboard
        {
            get { return dummy_keyboard_list;  }//return keyboardDriver.Keyboard;
        }

        #endregion

        #region public IList<Mouse> Mouse

        public IList<MouseDevice> Mouse
        {
            get { return (IList<MouseDevice>)dummy_mice_list; } //return mouseDriver.Mouse;
        }

        #endregion

        #region public IList<JoystickDevice> Joysticks

        public IList<JoystickDevice> Joysticks
        {
            get { return new List<JoystickDevice>();}// joystick_driver.Joysticks; }
        }

        #endregion

        #endregion

        #region --- IDisposable Members ---

        public void Dispose()
        {
            //this.Dispose(true);
            //GC.SuppressFinalize(this);
        }

        //private void Dispose(bool manual)
        //{
        //    if (!disposed)
        //    {
        //        //disposing = true;
        //        if (pollingThread != null && pollingThread.IsAlive)
        //            pollingThread.Abort();

        //        if (manual)
        //        {
        //        }

        //        disposed = true;
        //    }
        //}

        //~X11Input()
        //{
        //    this.Dispose(false);
        //}

        #endregion
    }
}
#endif