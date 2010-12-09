 #region License
 //
 // The Open Toolkit Library License
 //
 // Copyright (c) 2006 - 2009 the Open Toolkit library.
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
using System.Text;
using OpenMobile.Graphics;
using System.ComponentModel;
using OpenMobile.Platform;

namespace OpenMobile.Input
{
    /// <summary>
    /// Represents a mouse device and provides methods to query its status.
    /// </summary>
    public sealed class MouseDevice : IInputDevice
    {
        #region --- Fields ---
        int instance=-1;
        string description;
        IntPtr id;
        readonly bool[] button_state = new bool[Enum.GetValues(typeof(MouseButton)).Length];
        Point last_pos = new Point();
        MouseMoveEventArgs move_args = new MouseMoveEventArgs();
        MouseButtonEventArgs button_args = new MouseButtonEventArgs();
		internal bool Absolute;
		internal double minx=0,miny=0,maxx=0,maxy=0;

        #endregion

        #region --- IInputDevice Members ---

        #region public string Description

        /// <summary>
        /// Gets a string describing this MouseDevice.
        /// </summary>
        public string Description
        {
            get { return description; }
            internal set { description = value; }
        }

        #endregion

        #region public InputDeviceType DeviceType

        /// <summary>
        /// Gets a value indicating the InputDeviceType of this InputDevice. 
        /// </summary>
        public InputDeviceType DeviceType
        {
            get { return InputDeviceType.Mouse; }
        }

        #endregion

        #endregion

        #region --- Public Members ---

        #region public IntPtr DeviceID

        /// <summary>
        /// Gets an IntPtr representing a device dependent ID.
        /// </summary>
        public IntPtr DeviceID
        {
            get { return id; }
            internal set { id = value; }
        }

        #endregion

        #region public int X

        /// <summary>
        /// Gets an integer representing the absolute x position of the pointer, in window pixel coordinates.
        /// </summary>
        public int X
        {
            get { return move_args.X; }
        }

        #endregion
        public int Instance
        {
            get { return instance; }
            set { instance = value; }
        }
        public Point Location
        {
            get { return new Point(move_args.X,move_args.Y); }
            set 
            {
                move_args.X = value.X;
                move_args.Y = value.Y;
                #if WINDOWS
                if (Configuration.RunningOnWindows)
                    Platform.Windows.Functions.SetCursorPos(move_args.X, move_args.Y);
                #endif
                #if LINUX
                if(Configuration.RunningOnLinux)
                    Platform.X11.Functions.XIWarpPointer(Platform.X11.API.DefaultDisplay, 0, IntPtr.Zero, Platform.X11.API.RootWindow, 0, 0, 0, 0, move_args.X, move_args.Y);
                #endif
                //TODO - OSX
            }
        }
        #region public int Y

        /// <summary>
        /// Gets an integer representing the absolute y position of the pointer, in window pixel coordinates.
        /// </summary>
        public int Y
        {
            get { return move_args.Y; }
        }

        #endregion

        #region public bool this[MouseButton b]

        /// <summary>
        /// Gets a System.Boolean indicating the state of the specified MouseButton.
        /// </summary>
        /// <param name="button">The MouseButton to check.</param>
        /// <returns>True if the MouseButton is pressed, false otherwise.</returns>
        public bool this[MouseButton button]
        {
            get
            {
                return button_state[(int)button];
            }
            internal set
            {
                bool previous_state = button_state[(int)button];
                if (!value && previous_state)
                    MouseClick(instance, button_args);
                button_state[(int)button] = value;

                button_args.X = move_args.X;
                button_args.Y = move_args.Y;
                button_args.Button = button;
                button_args.IsPressed = value;
                if (value && !previous_state)
                    ButtonDown(instance, button_args);
                else if (!value && previous_state)
                    ButtonUp(instance, button_args);
            }
        }

        #endregion
        int height, width;
        internal int Width
        {
            get
            {
                return width;
            }
        }
        internal int Height
        {
            get
            {
                return height;
            }
        }
        public void SetBounds(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public void ShowCursor(IWindowInfo info)
        {
            #if WINDOWS
            if (Configuration.RunningOnWindows)
                Platform.Windows.Functions.ShowCursor(true);
            #endif
            #if LINUX
            #if WINDOWS
            else 
            #endif
            if (Configuration.RunningOnX11)
            {
                Platform.X11.X11WindowInfo x11 = (Platform.X11.X11WindowInfo)info;
                using (new Platform.X11.XLock(x11.Display))
                {
                    Platform.X11.Functions.XUndefineCursor(x11.Display, x11.WindowHandle);
                }
            }
            #endif
            #if OSX
            #if (WINDOWS||LINUX)
            else 
            #endif
            if (Configuration.RunningOnMacOS)
            {
                Platform.MacOS.Carbon.CG.CGDisplayShowCursor(IntPtr.Zero);
            }
            #endif
        }
        public void TrapCursor()
        {
            #if WINDOWS
            if (Configuration.RunningOnWindows)
            {
                OpenMobile.Platform.Windows.Win32Rectangle r = new OpenMobile.Platform.Windows.Win32Rectangle(1,1);
                Platform.Windows.Functions.ClipCursor(ref r);
            }
            #endif
        }
        public void UntrapCursor()
        {
            #if WINDOWS
            if (Configuration.RunningOnWindows)
            {
                Platform.Windows.Functions.ClipCursor(IntPtr.Zero);
            }
            #endif
        }
        internal void Reset()
        {
            for (int i = 0; i < button_state.Length; i++)
                button_state[i] = false;
        }
        public void HideCursor(IWindowInfo info)
        {
            #if WINDOWS
            if (Configuration.RunningOnWindows)
                Platform.Windows.Functions.ShowCursor(false);
            #endif
            #if LINUX
            #if WINDOWS
            else 
            #endif
                if (Configuration.RunningOnX11)
            {
                Platform.X11.X11WindowInfo window = (Platform.X11.X11WindowInfo)info;
                using (new Platform.X11.XLock(window.Display))
                {
                    Platform.X11.XColor black, dummy;
                    IntPtr cmap = Platform.X11.Functions.XDefaultColormap(window.Display, window.Screen);
                    Platform.X11.Functions.XAllocNamedColor(window.Display, cmap, "black", out black, out dummy);
                    IntPtr bmp_empty = Platform.X11.Functions.XCreateBitmapFromData(window.Display,
                        window.WindowHandle, new byte[,] { { 0 } });
                    IntPtr cursor_empty = Platform.X11.Functions.XCreatePixmapCursor(window.Display,
                    bmp_empty, bmp_empty, ref black, ref black, 0, 0);

                    Platform.X11.Functions.XDefineCursor(window.Display, window.WindowHandle, cursor_empty);
                    Platform.X11.Functions.XFreeCursor(window.Display, cursor_empty);
                }
            }
            #endif
            #if OSX
            #if (WINDOWS||LINUX)
            else 
            #endif
            if (Configuration.RunningOnMacOS)
            {
                Platform.MacOS.Carbon.CG.CGDisplayHideCursor(IntPtr.Zero);
            }
            #endif
        }
        #endregion

        #region --- Internal Members ---

        #region internal Point Position

        internal void SetPosition(int x, int y)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x > width)
                x = width;
            if (y > height)
                y = height;
            move_args.X = x;
            move_args.Y = y;
            move_args.XDelta = x - last_pos.X;
            move_args.YDelta = y - last_pos.Y;
            if (button_state[0])
                move_args.Buttons = MouseButton.Left;
            else
                move_args.Buttons = MouseButton.None;
            Move(instance, move_args);
            last_pos.X = move_args.X;
            last_pos.Y = move_args.Y;
        }

        #endregion

        #endregion

        #region --- Events ---

        /// <summary>
        /// Occurs when the mouse's position is moved.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> Move = delegate { };

        /// <summary>
        /// Occurs when a button is pressed.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> ButtonDown = delegate { };

        /// <summary>
        /// Occurs when a button is released.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> ButtonUp = delegate { };

        /// <summary>
        /// Occurs when a button is released.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseClick = delegate { };

        #region --- Overrides ---

        /// <summary>
        /// Calculates the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(id.GetHashCode() ^ description.GetHashCode());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that describes this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that describes this instance.</returns>
        public override string ToString()
        {
            return String.Format("ID: {0} ({1}).",
                DeviceID, Description);
        }

        #endregion

        #endregion

        #region COMPAT_REV1519

#if COMPAT_REV1519

        #region public int WheelDelta

        /// <summary>
        /// Gets an integer representing the relative wheel movement.
        /// </summary>
        [Obsolete("WheelDelta is only defined for a single WheelChanged event.  Use the OpenMobile.Input.MouseWheelEventArgs::Delta property with the OpenMobile.Input.MouseDevice::WheelChanged event.", false)]
        public int WheelDelta
        {
            get
            {
                int result = (int)Math.Round(wheel - wheel_last_accessed, MidpointRounding.AwayFromZero);
                wheel_last_accessed = (int)wheel;
                return result;
            }
        }

        #endregion

        #region public int XDelta

        /// <summary>
        /// Gets an integer representing the relative x movement of the pointer, in pixel coordinates.
        /// </summary>
        [Obsolete("XDelta is only defined for a single Move event.  Use the OpenMobile.Input.MouseMoveEventArgs::Delta property with the OpenMobile.Input.MouseDevice::Move event.", false)]
        public int XDelta
        {
            get
            {
                int result = pos.X - pos_last_accessed.X;
                pos_last_accessed.X = pos.X;
                return result;
            }
        }

        #endregion

        #region public int YDelta

        /// <summary>
        /// Gets an integer representing the relative y movement of the pointer, in pixel coordinates.
        /// </summary>
        [Obsolete("YDelta is only defined for a single Move event.  Use the OpenMobile.Input.MouseMoveEventArgs::Delta property with the OpenMobile.Input.MouseDevice::Move event.", false)]
        public int YDelta
        {
            get
            {
                int result = pos.Y - pos_last_accessed.Y;
                pos_last_accessed.Y = pos.Y;
                return result;
            }
        }

        #endregion

#endif

        #endregion
    }

    #region Event Arguments

    /// <summary>
    /// Defines the event data for <see cref="MouseDevice"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseEventArgs(MouseEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseEventArgs : EventArgs
    {
        #region Fields

        int x, y;
        MouseButton button;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public MouseEventArgs()
        {
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        public MouseEventArgs(int x, int y,MouseButton button)
        {
            this.x = x;
            this.y = y;
            this.button = button;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="args">The <see cref="MouseEventArgs"/> instance to clone.</param>
        public MouseEventArgs(MouseEventArgs args)
            : this(args.x, args.y,args.button)
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Gets the X position of the mouse for the event.
        /// </summary>
        public int X { get { return x; } internal set { x = value; } }

        /// <summary>
        /// Gets the Y position of the mouse for the event.
        /// </summary>
        public int Y { get { return y; } internal set { y = value; } }

        /// <summary>
        /// Gets a System.Drawing.Points representing the location of the mouse for the event.
        /// </summary>
        public Point Location { get { return new Point(x, y); } }

        public MouseButton Buttons { get { return button; } set { button = value; } }

        #endregion
    }

    /// <summary>
    /// Defines the event data for <see cref="MouseDevice.Move"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseMoveEventArgs(MouseMoveEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseMoveEventArgs : MouseEventArgs
    {
        #region Fields

        int x_delta, y_delta;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new <see cref="MouseMoveEventArgs"/> instance.
        /// </summary>
        public MouseMoveEventArgs() { }

        /// <summary>
        /// Constructs a new <see cref="MouseMoveEventArgs"/> instance.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        /// <param name="xDelta">The change in X position produced by this event.</param>
        /// <param name="yDelta">The change in Y position produced by this event.</param>
        public MouseMoveEventArgs(int x, int y, int xDelta, int yDelta,MouseButton button)
            : base(x, y,button)
        {
            XDelta = xDelta;
            YDelta = yDelta;
            Buttons = button;
        }

        /// <summary>
        /// Constructs a new <see cref="MouseMoveEventArgs"/> instance.
        /// </summary>
        /// <param name="args">The <see cref="MouseMoveEventArgs"/> instance to clone.</param>
        public MouseMoveEventArgs(MouseMoveEventArgs args)
            : this(args.X, args.Y, args.XDelta, args.YDelta,args.Buttons)
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Gets the change in X position produced by this event.
        /// </summary>
        public int XDelta { get { return x_delta; } internal set { x_delta = value; } }

        /// <summary>
        /// Gets the change in Y position produced by this event.
        /// </summary>
        public int YDelta { get { return y_delta; } internal set { y_delta = value; } }
        #endregion
    }

    /// <summary>
    /// Defines the event data for <see cref="MouseDevice.ButtonDown"/> and <see cref="MouseDevice.ButtonUp"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone an instance using the 
    /// <see cref="MouseButtonEventArgs(MouseButtonEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class MouseButtonEventArgs : MouseEventArgs
    {
        #region Fields

        MouseButton button;
        bool pressed;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new <see cref="MouseButtonEventArgs"/> instance.
        /// </summary>
        public MouseButtonEventArgs() { }

        /// <summary>
        /// Constructs a new <see cref="MouseButtonEventArgs"/> instance.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        /// <param name="button">The mouse button for the event.</param>
        /// <param name="pressed">The current state of the button.</param>
        public MouseButtonEventArgs(int x, int y, MouseButton button, bool pressed)
            : base(x, y,button)
        {
            this.button = button;
            this.pressed = pressed;
        }

        /// <summary>
        /// Constructs a new <see cref="MouseButtonEventArgs"/> instance.
        /// </summary>
        /// <param name="args">The <see cref="MouseButtonEventArgs"/> instance to clone.</param>
        public MouseButtonEventArgs(MouseButtonEventArgs args)
            : this(args.X, args.Y, args.Button, args.IsPressed)
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        /// The mouse button for the event.
        /// </summary>
        public MouseButton Button { get { return button; } internal set { button = value; } }

        /// <summary>
        /// Gets a System.Boolean representing the state of the mouse button for the event.
        /// </summary>
        public bool IsPressed { get { return pressed; } internal set { pressed = value; } }

        #endregion
    }

    #endregion
}
