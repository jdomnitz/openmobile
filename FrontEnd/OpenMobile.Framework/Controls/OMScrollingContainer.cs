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
namespace OpenMobile.Controls
{
    /// <summary>
    /// Contains and Renders a collection of OMControls
    /// </summary>
    public class OMScrollingContainer:OMControl,IMouse,IThrow,IClickable
    {
        /// <summary>
        /// the collection of controls
        /// </summary>
        protected List<OMControl> Controls = new List<OMControl>();
        /// <summary>
        /// The control area
        /// </summary>
        protected Rectangle area = new Rectangle();
        /// <summary>
        /// the vertical ofset
        /// </summary>
        protected int scrolly;
        /// <summary>
        /// the scrollbar width
        /// </summary>
        protected int scrollwidth;
        private Point[] up;
        private Point[] down;
        /// <summary>
        /// The scrollbar top ofset
        /// </summary>
        protected int scrolltop;
        /// <summary>
        /// the scroll bar height
        /// </summary>
        protected int scrollheight;
        /// <summary>
        /// The currently highlighted control
        /// </summary>
        protected OMControl highlighted;



        /// <summary>
        /// Width of the scrollbar (0 for none)
        /// </summary>
        public int ScrollBarWidth
        {
            get
            {
                return scrollwidth;
            }
            set
            {
                up = new Point[3];
                down = new Point[3];
                up[0] = new Point(left+width - value/2, top);
                up[1] = new Point(left + width, value+top);
                up[2] = new Point(left + width - value, value+top);
                down[0] = new Point(left + width - value / 2, height+top);
                down[1] = new Point(left + width, height - value+top);
                down[2] = new Point(left + width - value, height - value+top);
                scrollwidth = value;
                updateScroll();
            }
        }
        /// <summary>
        /// Create a new OMScrollingContainer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMScrollingContainer(int x, int y, int w, int h)
        {
            area.X = left = x;
            area.Y = top = y;
            area.Width = width = w;
            area.Height = height = h;
        }
        /// <summary>
        /// Find a control matching the given predicate
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public OMControl Find(Predicate<OMControl> match)
        {
            return Controls.Find(match);
        }
        /// <summary>
        /// The area that controls occupy
        /// </summary>
        public Rectangle ControlArea
        {
            get
            {
                return area;
            }
            set
            {
                if (area == value)
                    return;
                area = value;
                updateScroll();
            }
        }
        /// <summary>
        /// The OMPanel that contains this control
        /// </summary>
        public override OMPanel Parent
        {
            get
            {
                return parent;
            }
            internal set
            {
                parent = value;
                foreach (OMControl c in Controls)
                    c.Parent = parent;
            }
        }
        /// <summary>
        /// Add a new control to the collection
        /// </summary>
        /// <param name="control"></param>
        public void Add(OMControl control)
        {
            control.Parent = this.parent;
            control.UpdateThisControl += raiseUpdate;
            lock(Controls)
                Controls.Add(control);
        }
        /// <summary>
        /// Remove a control from the collection
        /// </summary>
        /// <param name="control"></param>
        public void Remove(OMControl control)
        {
            lock (Controls)
                Controls.Remove(control);
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
                return Controls[index];
            }
            set
            {
                Controls[index] = value;
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
                return Controls.Find(p => p.Name == name);
            }
        }
        /// <summary>
        /// Draw the control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            g.SetClip(this.toRegion());
            g.TranslateTransform(left, top-scrolly);
            lock (Controls)
            {
                foreach (OMControl c in Controls)
                    if (c.Visible)
                        c.Render(g, e);
            }
            g.TranslateTransform(-left, -top+scrolly);
            if (scrollwidth > 0)
            {
                if (area.Height > height)
                {
                    Brush white=new Brush(Color.White);
                    g.FillPolygon(white, up);
                    g.FillRoundRectangle(white, left+width - scrollwidth, scrolltop, scrollwidth, scrollheight, 7);
                    g.FillPolygon(white, down);
                }
            }
            g.ResetClip();
        }


        #region IMouse Members
        /// <summary>
        /// Mouse Moved
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public void MouseMove(int screen, OpenMobile.Input.MouseMoveEventArgs e, float WidthScale, float HeightScale)
        {
            Point loc = e.Location;
            loc.Scale(WidthScale, HeightScale);
            loc.Translate(-left, -top+scrolly);
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].toRegion().Contains(loc))
                {
                    if (highlighted != null)
                        highlighted.Mode = eModeType.Normal;
                    highlighted = Controls[i];
                    highlighted.Mode = mode;
                    raiseUpdate(false);
                    if (typeof(IMouse).IsInstanceOfType(highlighted))
                        ((IMouse)highlighted).MouseMove(screen, e, WidthScale, HeightScale);
                    return;
                }
            }
            if (highlighted != null)
            {
                highlighted.Mode = eModeType.Normal;
                raiseUpdate(false);
            }
            highlighted = null;
        }
        /// <summary>
        /// Mouse Down
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            Point loc = e.Location;
            loc.Scale(WidthScale, HeightScale);
            loc.Translate(-left, -top+scrolly);
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].toRegion().Contains(loc))
                {
                    highlighted = Controls[i];
                    if (typeof(IMouse).IsInstanceOfType(highlighted))
                        ((IMouse)highlighted).MouseDown(screen,e,WidthScale,HeightScale);
                    break;
                }
            }
        }
        /// <summary>
        /// Mouse Up
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            Point loc = e.Location;
            loc.Scale(WidthScale, HeightScale);
            loc.Translate(-left, -top+scrolly);
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].toRegion().Contains(loc))
                {
                    highlighted = Controls[i];
                    if (typeof(IMouse).IsInstanceOfType(highlighted))
                        ((IMouse)highlighted).MouseUp(screen, e, WidthScale, HeightScale);
                    break;
                }
            }
        }

        #endregion

        #region IThrow Members
        /// <summary>
        /// Mouse Throw
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="TotalDistance"></param>
        /// <param name="RelativeDistance"></param>
        public void MouseThrow(int screen, OpenMobile.Graphics.Point TotalDistance, OpenMobile.Graphics.Point RelativeDistance)
        {
            if (area.Height <= height)
                return;
            scrolly -= RelativeDistance.Y;
            if (scrolly > (area.Height-height))
                scrolly = (area.Height - height);
            if (scrolly < 0)
                scrolly = 0;
            updateScroll();
            raiseUpdate(false);
        }
        private void updateScroll()
        {
            if (scrollwidth > 0)
            {
                int factor = (height - 6 - 2 * scrollwidth);
                scrollheight = (int)((height / (float)area.Height) * factor);
                scrolltop = top+scrollwidth + 3 + (int)((scrolly / (float)area.Height) * factor);
            }
        }
        /// <summary>
        /// Mouse Throw Start
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="StartLocation"></param>
        /// <param name="scaleFactors"></param>
        /// <param name="Cancel"></param>
        public void MouseThrowStart(int screen, OpenMobile.Graphics.Point StartLocation, OpenMobile.Graphics.PointF scaleFactors, ref bool Cancel)
        {
            //
        }

        /// <summary>
        /// Mouse Throw End
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="EndLocation"></param>
        public void MouseThrowEnd(int screen, OpenMobile.Graphics.Point EndLocation)
        {
            //
        }

        #endregion

        #region IClickable Members
        /// <summary>
        /// Clicked
        /// </summary>
        /// <param name="screen"></param>
        public void clickMe(int screen)
        {
            if (highlighted != null)
                if (typeof(IClickable).IsInstanceOfType(highlighted))
                    ((IClickable)highlighted).clickMe(screen);
        }

        /// <summary>
        /// Long Clicked
        /// </summary>
        /// <param name="screen"></param>
        public void longClickMe(int screen)
        {
            if (highlighted != null)
                if (typeof(IClickable).IsInstanceOfType(highlighted))
                    ((IClickable)highlighted).longClickMe(screen);
        }

        #endregion
    }
}
