/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using OpenMobile.Graphics;
using System.ComponentModel;

namespace OpenMobile.Input
{
    /// <summary>
    /// Enumerates all possible mouse buttons.
    /// </summary>
    [Flags]
    public enum MouseButton
    {
        /// <summary>
        /// No Buttons Pressed
        /// </summary>
        None = -1,
        /// <summary>
        /// The left mouse button.
        /// </summary>
        Left = 0,
        /// <summary>
        /// The middle mouse button.
        /// </summary>
        Middle,
        /// <summary>
        /// The right mouse button.
        /// </summary>
        Right,
    }

    public class Mouse
    {
        /// <summary>
        /// Reads the mouse buttons states of a mouse device
        /// </summary>
        /// <param name="mouseIndex">The index of the mouse device</param>
        /// <returns></returns>
        public static MouseButton GetMouseButtons(int mouseIndex)
        {
            OpenTK.Input.MouseState state = OpenTK.Input.Mouse.GetState(mouseIndex);
            if (state.LeftButton == OpenTK.Input.ButtonState.Pressed)
                return MouseButton.Left;
            else if (state.RightButton == OpenTK.Input.ButtonState.Pressed)
                return MouseButton.Right;
            else if (state.MiddleButton == OpenTK.Input.ButtonState.Pressed)
                return MouseButton.Middle;
            return MouseButton.None;
        }

        public static MouseButton GetMouseButtons(OpenTK.Input.MouseDevice mouse)
        {
            if (mouse[OpenTK.Input.MouseButton.Left])
                return MouseButton.Left;
            else if (mouse[OpenTK.Input.MouseButton.Right])
                return MouseButton.Right;
            else if (mouse[OpenTK.Input.MouseButton.Middle])
                return MouseButton.Middle;
            return MouseButton.None;
        }

        public static MouseButton GetMouseButtons(OpenTK.Input.MouseState mouseState)
        {
            if (mouseState[OpenTK.Input.MouseButton.Left])
                return MouseButton.Left;
            else if (mouseState[OpenTK.Input.MouseButton.Right])
                return MouseButton.Right;
            else if (mouseState[OpenTK.Input.MouseButton.Middle])
                return MouseButton.Middle;
            return MouseButton.None;
        }


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
        public MouseEventArgs(int x, int y, MouseButton button)
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
            : this(args.x, args.y, args.button)
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

        public static void Scale(MouseEventArgs e, PointF ScaleFactors)
        {
            // Scale location
            e.X = (int)(e.X / ScaleFactors.X);
            e.Y = (int)(e.Y / ScaleFactors.Y);
        }
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
        public MouseMoveEventArgs(int x, int y, int xDelta, int yDelta, MouseButton button)
            : base(x, y, button)
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
            : this(args.X, args.Y, args.XDelta, args.YDelta, args.Buttons)
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

        public static void Scale(MouseMoveEventArgs e, PointF ScaleFactors)
        {
            // Scale location
            MouseEventArgs.Scale((MouseEventArgs)e, ScaleFactors);

            // Scale mouse delta 
            e.XDelta = (int)(e.XDelta / ScaleFactors.X);
            e.YDelta = (int)(e.YDelta / ScaleFactors.Y);
        }
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
            : base(x, y, button)
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
