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
using System.Collections.Generic;
using OpenMobile.Graphics;
using System;
using OpenMobile.Input;

namespace OpenMobile.Controls
{
    [System.Serializable]
    public class OMContainer : OMControl, IContainer2, IHighlightable, IThrow, IKey
    {        
        List<OMControl> _Controls = new List<OMControl>();
        Rectangle oldRegion = new Rectangle();

        Rectangle ScrollRegion = new Rectangle();
        Rectangle oldScrollRegion = new Rectangle();

        #region ScrollBar properties

        private bool _ScrollBar_Vertical_Enabled = true;
        private bool _ScrollBar_Horizontal_Enabled = true;
        private bool _ScrollBar_Vertical_Visible = false;
        private bool _ScrollBar_Horizontal_Visible = false;
        private bool _ScrollBar_Vertical_ScrollInProgress = false;
        private bool _ScrollBar_Horizontal_ScrollInProgress = false;
        private int _ScrollBar_ClearanceToEdge = 3;
        private int _ScrollBar_Thickness = 7;
        private Color _ScrollBar_ColorNormal = Color.FromArgb(90, Color.White);
        private Color _ScrollBar_ColorHighlighted = Color.FromArgb(180, Color.White);

        /// <summary>
        /// Enable the vertical scrollbar
        /// </summary>
        public bool ScrollBar_Vertical_Enabled
        {
            get
            {
                return _ScrollBar_Vertical_Enabled;
            }
            set
            {
                _ScrollBar_Vertical_Enabled = value;
                Refresh();
            }
        }
        /// <summary>
        /// Enable the horizontal scrollbar
        /// </summary>
        public bool ScrollBar_Horizontal_Enabled
        {
            get
            {
                return _ScrollBar_Horizontal_Enabled;
            }
            set
            {
                _ScrollBar_Horizontal_Enabled = value;
                Refresh();
            }
        }

        /// <summary>
        /// The clearance for the scrollbar from the edge of the control
        /// </summary>
        public int ScrollBar_ClearanceToEdge
        {
            get
            {
                return _ScrollBar_ClearanceToEdge;
            }
            set
            {
                _ScrollBar_ClearanceToEdge = value;
                Refresh();
            }
        }

        /// <summary>
        /// The thickness of the scrollbar
        /// </summary>
        public int ScrollBar_Thickness
        {
            get
            {
                return _ScrollBar_Thickness;
            }
            set
            {
                _ScrollBar_Thickness = value;
                Refresh();
            }
        }

        /// <summary>
        /// The color of the scrollbar when rendered in the normal state
        /// </summary>
        public Color ScrollBar_ColorNormal
        {
            get
            {
                return _ScrollBar_ColorNormal;
            }
            set
            {
                _ScrollBar_ColorNormal = value;
                Refresh();
            }
        }

        /// <summary>
        /// The color of the scrollbar when rendered in the highlighted state
        /// </summary>
        public Color ScrollBar_ColorHighlighted
        {
            get
            {
                return _ScrollBar_ColorHighlighted;
            }
            set
            {
                _ScrollBar_ColorHighlighted = value;
                Refresh();
            }
        }

        #endregion

        #region Background

        private imageItem image;
        private byte transparency = 100;
        /// <summary>
        /// Opacity (0-100%)
        /// </summary>
        public byte Transparency
        {
            get
            {
                return transparency;
            }
            set
            {
                if (transparency == value)
                    return;
                transparency = value;
                opacity = (byte)(255 * (transparency / 100F));
                raiseUpdate(false);
            }
        }

        private Color _BackgroundColor = Color.Transparent;

        public Color BackgroundColor
        {
            get
            {
                return _BackgroundColor;
            }
            set
            {
                if (value == _BackgroundColor)
                    return;

                _BackgroundColor = value;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// The image to be rendered
        /// </summary>
        public imageItem Image
        {
            get
            {
                return image;
            }
            set
            {
                if (image == value)
                    return;

                image = value;
                raiseUpdate(false);
            }
        }

        #endregion

        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            // Limit rendering to this controls region
            g.SetClip(Region);

            #region Draw background 

            // Draw image background (if any)
            if (_BackgroundColor != Color.Transparent && _RenderingValue_Alpha == 1)
                g.FillRectangle(new Brush(Color.FromArgb((int)(_BackgroundColor.A * OpacityFloat), _BackgroundColor)), new Rectangle(left + 1, top + 1, width - 2, height - 2));

            // Draw image (if any)
            if (image.image != null)
                lock (image.image)
                    g.DrawImage(image.image, left, top, width, height, _RenderingValue_Alpha, eAngle.Normal);

            #endregion

            // Find region of contained controls
            Rectangle ControlsArea = GetControlsArea();

            // Move controls with this control
            if (!oldRegion.IsEmpty)
                e.Offset = Region - oldRegion;
            oldRegion = Region;

            // Pass offset data along with renderingparameters to move contained controls
            e.Offset += ScrollRegion;
            // Reset scroll data so we don't accumulate offset values
            ScrollRegion = new Rectangle();

            #region Limit offset so we don't move the controls outside the bounds

            // Horizontal offset
            if (e.Offset.X > 0)
            {
                if (System.Math.Abs(e.Offset.X) > System.Math.Abs(Region.Left - ControlsArea.Left))
                {   // X offset is outside of bounds, limit value
                    e.Offset.X = System.Math.Abs(Region.Left - ControlsArea.Left);
                }
            }
            else if (e.Offset.X < 0)
            {
                if (System.Math.Abs(e.Offset.X) > System.Math.Abs(Region.Right - ControlsArea.Right))
                {   // X offset is outside of bounds, limit value
                    e.Offset.X = -System.Math.Abs(Region.Right - ControlsArea.Right);
                }
            }

            // Vertical offset
            if (e.Offset.Y > 0)
            {
                if (System.Math.Abs(e.Offset.Y) > System.Math.Abs(Region.Top - ControlsArea.Top))
                {   // Y offset is outside of bounds, limit value
                    e.Offset.Y = System.Math.Abs(Region.Top - ControlsArea.Top);
                }
            }
            else if (e.Offset.Y < 0)
            {
                if (System.Math.Abs(e.Offset.Y) > System.Math.Abs(Region.Bottom - ControlsArea.Bottom))
                {   // Y offset is outside of bounds, limit value
                    e.Offset.Y = -System.Math.Abs(Region.Bottom - ControlsArea.Bottom);
                }
            }

            #endregion

            for (int i = 0; i < _Controls.Count; i++)
                if (_Controls[i].IsControlRenderable(true))
                    _Controls[i].Render(g, e);

            // Reset offset data
            e.Offset = new Rectangle();

            // Reset clip
            g.ResetClip();

            #region Render scrollbar

            _ScrollBar_Horizontal_Visible = ((ControlsArea.Left < Region.Left || ControlsArea.Right > Region.Right) && _ScrollBar_Horizontal_Enabled);
            if (_ScrollBar_Horizontal_Visible)
            {   // Horisontal scrollbar

                // Set scrollbar color
                Brush ScrollBarBrush;
                if (_ScrollBar_Horizontal_ScrollInProgress | this.mode == eModeType.Highlighted)
                    ScrollBarBrush = new Brush(_ScrollBar_ColorHighlighted);
                else
                    ScrollBarBrush = new Brush(_ScrollBar_ColorNormal);

                // Calculate scrollbar length
                int ScrollBarSize = (int)(((float)Region.Width) * (((float)Region.Width) / ((float)ControlsArea.Width)));
                // Set minimum size of scrollbar
                if (ScrollBarSize <= 0)
                    ScrollBarSize = 5;

                // Calculate scrollbar location
                float MaxTravelScrollBar = System.Math.Abs(Region.Width - ScrollBarSize);
                float MaxTravelControls = System.Math.Abs(ControlsArea.Width - Region.Width);
                float CurrentTravel = System.Math.Abs(Region.Left - ControlsArea.Left);
                float TravelFactor = CurrentTravel / MaxTravelControls;
                int ScrollBarLocation = Region.Left + (int)(MaxTravelScrollBar * TravelFactor);

                // Render scrollbar
                g.FillRectangle(ScrollBarBrush, ScrollBarLocation, Region.Bottom - _ScrollBar_Thickness - _ScrollBar_ClearanceToEdge, ScrollBarSize, _ScrollBar_Thickness);

                // Release resources
                ScrollBarBrush.Dispose();
            }

            _ScrollBar_Vertical_Visible = ((ControlsArea.Bottom > Region.Bottom || ControlsArea.Top < Region.Top) && _ScrollBar_Vertical_Enabled);
            if (_ScrollBar_Vertical_Visible)
            {   // Vertical scrollbar

                // Set scrollbar color
                Brush ScrollBarBrush = new Brush(Color.FromArgb(90, Color.White));
                if (_ScrollBar_Vertical_ScrollInProgress | this.mode == eModeType.Highlighted)
                    ScrollBarBrush = new Brush(Color.FromArgb(180, Color.White));

                // Calculate scrollbar length
                int ScrollBarSize = (int)(((float)Region.Height) * (((float)Region.Height) / ((float)ControlsArea.Height)));
                // Set minimum size of scrollbar
                if (ScrollBarSize <= 0)
                    ScrollBarSize = 5;

                // Calculate scrollbar location
                float MaxTravelScrollBar = System.Math.Abs(Region.Height - ScrollBarSize);
                float MaxTravelControls = System.Math.Abs(ControlsArea.Height - Region.Height);
                float CurrentTravel = System.Math.Abs(Region.Top - ControlsArea.Top);
                float TravelFactor = CurrentTravel / MaxTravelControls;
                int ScrollBarLocation = Region.Top + (int)(MaxTravelScrollBar * TravelFactor);

                // Render scrollbar
                g.FillRectangle(ScrollBarBrush, Region.Right - _ScrollBar_Thickness - _ScrollBar_ClearanceToEdge, ScrollBarLocation, _ScrollBar_Thickness, ScrollBarSize);

                // Release resources
                ScrollBarBrush.Dispose();
            }

            #endregion

            base.RenderFinish(g, e);
        }

        private Rectangle GetControlsArea()
        {
            int l = int.MaxValue;
            int t = int.MaxValue;
            int r = int.MinValue;
            int b = int.MinValue;

            foreach (OMControl control in _Controls)
	        {
                // Width
                if (control.Region.Right > r)
                    r = control.Region.Right;

                // Height
                if (control.Region.Bottom > b)
                    b = control.Region.Bottom;

                // Left side
                if (control.Region.Left < l)
                    l = control.Region.Left;

                // Top side
                if (control.Region.Top < t)
                    t = control.Region.Top;
            }
            return new Rectangle(l, t, r - l, b - t);
        }

        private void Control_Configure(OMControl control)
        {
            // Connect controls parent
            control.Parent = this.parent;

            // Connect refresh requestor to control
            control.UpdateThisControl += this.parent.raiseUpdate;
        }
        private void Controls_Configure()
        {
            foreach (OMControl control in _Controls)
                Control_Configure(control);
        }

        private void Control_PlaceRelative(OMControl control)
        {
            // Change position of this control from relative to this container to absolute on the panel
            control.Left = this.Left + control.Left;
            control.Top = this.Top + control.Top;
        }
        private void Controls_PlaceRelative()
        {
            foreach (OMControl control in _Controls)
                Control_PlaceRelative(control);
        }

        /// <summary>
        /// Scrolls to the specified control
        /// </summary>
        /// <param name="control"></param>
        public void ScrollToControl(OMControl control)
        {
            ScrollToControl(control.Name);
        }
        /// <summary>
        /// Scrolls to the specified control
        /// </summary>
        /// <param name="name"></param>
        public void ScrollToControl(string name)
        {
            OMControl controlFound = _Controls.Find(x => x.Name == name);
            if (controlFound != null)
            {
                // Calculate distance from current location to control's location
                Point Distance = (this.Region.Center - controlFound.Region.Center);
                ScrollRegion.X = Distance.X;
                ScrollRegion.Y = Distance.Y;
            }
        }

        public OMContainer(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
        }

        public override OMPanel Parent
        {
            get
            {
                return base.Parent;
            }
            internal set
            {
                base.Parent = value;
                if (value != null)
                    Controls_Configure();
                //    Controls_PlaceRelative();
            }
        }

        /// <summary>
        /// Gets/Sets the given control
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public OMControl this[int index]
        {
            get
            {
                return _Controls[index];
            }
            set
            {
                _Controls[index] = value;
            }
        }
        /// <summary>
        /// Gets a control by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public OMControl this[string name]
        {
            get
            {
                return _Controls.Find(p => p.Name == name);
            }
        }

        #region IContainer2 Members

        public List<OMControl> Controls
        {
            get { return _Controls; }
        }

        /// <summary>
        /// Adds a control with absolute placement to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControl(OMControl control)
        {
            return addControl(control, false);
        }
        /// <summary>
        /// Adds a control with relative placement to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControlRelative(OMControl control)
        {
            return addControl(control, true);
        }
        /// <summary>
        /// Adds a control with absolute placement to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControlAbsolute(OMControl control)
        {
            return addControl(control, false);
        }
        
        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControl(OMControl control, bool Relative)
        {
            // Only add controls that aren't already loaded
            //if (_Controls.Find(x => x.Name == control.Name) == null)
            {
                // Add control
                _Controls.Add(control);

                // Add this control to the panel
                if (this.parent != null)
                {
                    // Configure control
                    Control_Configure(control);

                    // Place control
                    if (Relative)
                        Control_PlaceRelative(control);

                    // Block controls automatic rendering
                    control.ManualRendering = true;

                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            OMContainer newObject = (OMContainer)this.MemberwiseClone();
            //OMContainer newObject = (OMContainer)base.Clone();
            newObject._Controls = new List<OMControl>();
            foreach (OMControl control in _Controls)
                newObject._Controls.Add((OMControl)control.Clone());
            return newObject;
        }

        #endregion

        #region IThrow Members

        public void MouseThrow(int screen, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            // Find region of contained controls
            Rectangle ControlsArea = GetControlsArea();

            if (_ScrollBar_Horizontal_Visible)
            {
                // Control motion vs limits
                if (RelativeDistance.X > 0)
                {
                    if (Region.Left > ControlsArea.Left)
                    {
                        _ScrollBar_Horizontal_ScrollInProgress = true;
                        ScrollRegion.Left += RelativeDistance.X;
                    }
                }
                else if (RelativeDistance.X < 0)
                {
                    if (Region.Right < ControlsArea.Right)
                    {
                        _ScrollBar_Horizontal_ScrollInProgress = true;
                        ScrollRegion.Left += RelativeDistance.X;
                    }
                }
            }

            if (_ScrollBar_Vertical_Visible)
            {
                // Control motion vs limits
                if (RelativeDistance.Y > 0)
                {
                    if (Region.Top > ControlsArea.Top)
                    {
                        _ScrollBar_Vertical_ScrollInProgress = true;
                        ScrollRegion.Top += RelativeDistance.Y;
                    }
                }
                else if (RelativeDistance.Y < 0)
                {
                    if (Region.Bottom < ControlsArea.Bottom)
                    {
                        _ScrollBar_Vertical_ScrollInProgress = true;
                        ScrollRegion.Top += RelativeDistance.Y;
                    }
                }
            }
        }

        public void MouseThrowStart(int screen, Point StartLocation, PointF scaleFactors, ref bool Cancel)
        {
        }

        public void MouseThrowEnd(int screen, Point StartLocation, Point TotalDistance, Point EndLocation)
        {
            _ScrollBar_Vertical_ScrollInProgress = false;
            _ScrollBar_Horizontal_ScrollInProgress = false;
        }

        #endregion

        #region IKey Members

        public bool KeyDown_BeforeUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            return false;
        }

        public void KeyDown_AfterUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            if ((e.Key == Key.Left) || (e.Key == Key.Right) || (e.Key == Key.Up) || (e.Key == Key.Down))
            {
                _Controls.ForEach(delegate(OMControl control)
                {
                    if (control.Mode == eModeType.Highlighted)
                        ScrollToControl(control);
                });
            }
        }

        public bool KeyUp_BeforeUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            return false;
        }

        public void KeyUp_AfterUI(int screen, KeyboardKeyEventArgs e, PointF scaleFactors)
        {
        }

        #endregion
    }
}
