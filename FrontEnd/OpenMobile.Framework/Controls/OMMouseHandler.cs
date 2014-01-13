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
using System;
using System.ComponentModel;
using OpenMobile.Graphics;
using OpenMobile.Input;

namespace OpenMobile.Controls
{
    public class OMMouseHandler : OMControl, IMouse, IClickableAdvanced, IClickEvents, IHighlightable
    {
        public class MouseEventArgs : OMEventArgs
        {
            /// <summary>
            /// The mouse absolute position
            /// </summary>
            public Point MouseAbsolutePosition
            {
                get
                {
                    return this._MouseAbsolutePosition;
                }
                set
                {
                    if (this._MouseAbsolutePosition != value)
                    {
                        this._MouseAbsolutePosition = value;
                    }
                }
            }
            private Point _MouseAbsolutePosition;

            /// <summary>
            /// The mouse local position relative to the controls basepoint
            /// </summary>
            public Point MouseLocalPosition
            {
                get
                {
                    return this._MouseLocalPosition;
                }
                set
                {
                    if (this._MouseLocalPosition != value)
                    {
                        this._MouseLocalPosition = value;
                    }
                }
            }
            private Point _MouseLocalPosition;

            /// <summary>
            /// The mouse button that was clicked
            /// </summary>
            public MouseButton Button
            {
                get
                {
                    return this._Button;
                }
                set
                {
                    if (this._Button != value)
                    {
                        this._Button = value;
                    }
                }
            }
            private MouseButton _Button;

            public MouseEventArgs(int screen, OMControl sender, Point mouseAbsolutePosition, Point mouseLocalPosition, MouseButton button)
                : base(screen, sender)
            {
                _MouseAbsolutePosition = mouseAbsolutePosition;
                _MouseLocalPosition = mouseLocalPosition;
                _Button = button;
            }
        }

        public delegate void MouseEventArgsHandler(int screen, OMControl sender, MouseEventArgs e);

        public event MouseEventArgsHandler OnMouseMove;
        public event MouseEventArgsHandler OnMouseDown;
        public event MouseEventArgsHandler OnMouseUp;
        public event MouseEventArgsHandler OnClick;
        public event MouseEventArgsHandler OnLongClick;
        public event MouseEventArgsHandler OnHoldClick;

        public OMMouseHandler(string name, OMControl parentControl)
            : base(name, parentControl.Left, parentControl.Top, parentControl.Width, parentControl.Height)
        {
        }
        public OMMouseHandler(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {            
        }


        void IMouse.MouseMove(int screen, Input.MouseMoveEventArgs e, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            if (OnMouseMove != null)
                OnMouseMove(screen, this, new MouseEventArgs(screen, this, e.Location, base.GetLocalControlPoint(e.Location), e.Buttons));
        }

        void IMouse.MouseDown(int screen, Input.MouseButtonEventArgs e, Point StartLocation)
        {
            if (OnMouseDown != null)
                OnMouseDown(screen, this, new MouseEventArgs(screen, this, e.Location, base.GetLocalControlPoint(e.Location), e.Buttons));
        }

        void IMouse.MouseUp(int screen, Input.MouseButtonEventArgs e, Point StartLocation, Point TotalDistance)
        {
            if (OnMouseUp != null)
                OnMouseUp(screen, this, new MouseEventArgs(screen, this, e.Location, base.GetLocalControlPoint(e.Location), e.Buttons));
        }

        void IClickableAdvanced.clickMe(int screen, MouseButtonEventArgs e)
        {
            if (OnClick != null)
                OnClick(screen, this, new MouseEventArgs(screen, this, e.Location, base.GetLocalControlPoint(e.Location), e.Buttons));
        }

        void IClickableAdvanced.longClickMe(int screen, MouseButtonEventArgs e)
        {
            if (OnLongClick != null)
                OnLongClick(screen, this, new MouseEventArgs(screen, this, e.Location, base.GetLocalControlPoint(e.Location), e.Buttons));
        }

        void IClickableAdvanced.holdClickMe(int screen, MouseButtonEventArgs e)
        {
            if (OnHoldClick != null)
                OnHoldClick(screen, this, new MouseEventArgs(screen, this, e.Location, base.GetLocalControlPoint(e.Location), e.Buttons));
        }

        event userInteraction IClickEvents.OnClick
        {
            add {  }
            remove {  }
        }

        event userInteraction IClickEvents.OnLongClick
        {
            add {  }
            remove {  }
        }

        event userInteraction IClickEvents.OnHoldClick
        {
            add {  }
            remove {  }
        }
    }
}
