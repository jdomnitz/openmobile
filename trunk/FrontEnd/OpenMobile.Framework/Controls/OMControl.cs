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
using System.Reflection;
using OpenMobile.Graphics;
using OpenMobile;
using System.ComponentModel;

namespace OpenMobile.Controls
{
    /// <summary>
    /// Forces the renderer to redraw this control
    /// </summary>
    public delegate void refreshNeeded(bool resetHighlighted);

    /// <summary>
    /// The base control type
    /// </summary>
    public abstract class OMControl : ICloneable
    {
        /// <summary>
        /// Indicates that graphics needs a refresh
        /// </summary>
        protected bool _RefreshGraphic = true;
        /// <summary>
        /// The rendering mode of the control
        /// </summary>
        protected eModeType mode;
        /// <summary>
        /// Skin debug mode (Shows borders or other features)
        /// </summary>
        protected bool _SkinDebug = false;
        /// <summary>
        /// Is the control visible
        /// </summary>
        protected bool visible = true;
        /// <summary>
        /// Is the control disabled (Usage of this property depends upon type of control)
        /// </summary>
        protected bool disabled = false;
        /// <summary>
        /// Should this control be "transparent" for user interaction?
        /// </summary>
        protected bool noUserInteraction = false;
        /// <summary>
        /// A control specific object
        /// </summary>
        protected object tag;
        /// <summary>
        /// The control name
        /// </summary>
        protected string name;
        /// <summary>
        /// Control location and size
        /// </summary>
        protected int height, width, top, left;
        /// <summary>
        /// Control opacity level (0 transparent - 255 solid)
        /// </summary>
        protected byte opacity = 255;
        /// <summary>
        /// Forces the renderer to redraw this control
        /// </summary>
        public event refreshNeeded UpdateThisControl;
        /// <summary>
        /// Requests the control be redrawn
        /// </summary>
        /// <param name="resetHighlighted"></param>
        protected void raiseUpdate(bool resetHighlighted)
        {
            if (!visible)
                return;
            if (UpdateThisControl != null)
                UpdateThisControl(resetHighlighted);
        }

        /// <summary>
        /// Requests a redraw/update of this control
        /// </summary>
        public void Refresh()
        {
            raiseUpdate(false);
        }

        /// <summary>
        /// Returns the screen the control is currently being rendered on.
        /// Note this function uses reflection and is quite slow, use it only when other alternatives do not exist.
        /// Returns -1 if not attached to any screen
        /// </summary>
        /// <returns></returns>
        public int containingScreen()
        {
            if (parent == null)
                return -1;
            return parent.containingScreen();
        }
        /// <summary>
        /// The OMPanel that contains this control
        /// </summary>
        protected OMPanel parent;
        /// <summary>
        /// The OMPanel that contains this control
        /// </summary>
        public virtual OMPanel Parent
        {
            get
            {
                return parent;
            }
            internal set
            {
                parent = value;
            }
        }
        /// <summary>
        /// If the control has been attached to a user interface (aka is currently loaded)
        /// </summary>
        /// <returns></returns>
        public bool hooked()
        {
            return parent.hooked();
        }
        
        /// <summary>
        /// Skin debug feature
        /// </summary>
        [Browsable(false)]
        public virtual bool SkinDebug
        {
            get
            {
                return _SkinDebug;
            }
            set
            {
                _SkinDebug = value;
            }
        }
        /// <summary>
        /// The rendering mode of the control
        /// </summary>
        [Browsable(false)]
        public virtual eModeType Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }
        /// <summary>
        /// The globally unique name for the control
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Should the control be displayed
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible == value)
                    return;
                visible = value;
                if (UpdateThisControl != null)
                    UpdateThisControl(true);
            }
        }

        /// <summary>
        /// Disable control
        /// </summary>
        public bool Disabled
        {
            get
            {
                return disabled;
            }
            set
            {
                if (disabled == value)
                    return;
                disabled = value;
                if (UpdateThisControl != null)
                    UpdateThisControl(true);
            }
        }

        /// <summary>
        /// Should the control be "transparent" for user interaction. TRUE = any clicks or events will be passed onto the next item underneath
        /// </summary>
        public bool NoUserInteraction
        {
            get
            {
                return noUserInteraction;
            }
            set
            {
                if (noUserInteraction == value)
                    return;
                noUserInteraction = value;
                raiseUpdate(false);
            }
        }

        /// <summary>
        /// The controls height in OM units
        /// </summary>
        public virtual int Height
        {
            get { return height; }
            set { height = value; raiseUpdate(true); }
        }
        /// <summary>
        /// The controls width in OM units
        /// </summary>
        public virtual int Width
        {
            get { return width; }
            set { width = value; raiseUpdate(true); }
        }
        /// <summary>
        /// Used to store additional information about a control
        /// </summary>
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }
        /// <summary>
        /// The distance between the top of the UI and the Top of the control
        /// </summary>
        public virtual int Top
        {
            get { return top; }
            set { top = value; raiseUpdate(true); }
        }
        /// <summary>
        /// The distance between the left of the UI and the Left of the control
        /// </summary>
        public virtual int Left
        {
            get { return left; }
            set { left = value; raiseUpdate(true); }
        }

        /// <summary>
        /// Control opacity level (0 transparent - 255 solid)
        /// </summary>
        public virtual int Opacity
        {
            get { return opacity; }
            set 
            {
                if (value <= 0)
                    opacity = 0;
                else if (value >= 255)
                    opacity = 255;
                else
                    opacity = (byte)value; 
                raiseUpdate(false); 
            }
        }
        /// <summary>
        /// Current opacity level as a float value
        /// </summary>
        protected float OpacityFloat
        {
            get
            {
                return (Opacity / 255F);
            }
        }
        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public virtual void Render(Graphics.Graphics g, renderingParams e)
        {
            _RefreshGraphic = false;
            // Skin debug function 
            if (_SkinDebug)
                DrawSkinDebugInfo(g, Color.Yellow);
        }

        /// <summary>
        /// Returns the region occupied by the control
        /// </summary>
        /// <returns></returns>
        public virtual Rectangle toRegion()
        {
            return new Rectangle(Left, Top, Width, Height);
        }
        /// <summary>
        /// Create a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            OMControl returnData = (OMControl)this.MemberwiseClone();
            Type type = returnData.GetType();

            foreach (PropertyInfo propInfo in type.GetProperties())
            {
                if (propInfo.CanWrite && propInfo.CanRead && (propInfo.GetGetMethod().GetParameters().Length == 0))
                    try
                    {
                        propInfo.SetValue(returnData, propInfo.GetValue(this, null), null);
                    }
                    catch (TargetInvocationException) { }
            }
            return returnData;
        }

        /// <summary>
        /// Draws skin debug info when called from a rendering method
        /// </summary>
        /// <param name="g"></param>
        /// <param name="c"></param>
        protected void DrawSkinDebugInfo(Graphics.Graphics g, Color c)
        {
            // Cancel if debug request is not active
            if (!_SkinDebug)
                return;

            Rectangle old = g.Clip;
            g.SetClipFast(left - 2, top - 2, width + 4, height + 4);
            g.DrawRectangle(new Pen(c, 1), left, top, (width == 0 ? 1 : width), (height == 0 ? 1 : height));
            g.Clip = old;
            //g.DrawImage(g.GenerateTextTexture(left, top, width, height, this.GetType().Name.ToString(), new Font(Font.Arial, 12), eTextFormat.Normal, Alignment.TopLeft, c, c), left, top, width, height, 255);
        }

        /// <summary>
        /// Create a new control
        /// </summary>
        [Obsolete("Always provide a control name. Method will be removed in next release")]
        public OMControl()
        {
        }
        /// <summary>
        /// Create a new control
        /// </summary>
        public OMControl(string Name)
        {
            this.Name = Name;
        }
        /// <summary>
        /// Create a new control
        /// </summary>
        public OMControl(string Name, int Left, int Top, int Width, int Height)
        {
            this.Name = Name;
            this.Left = Left;
            this.Top = Top;
            this.Width = Width;
            this.Height = Height;
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})",base.ToString(),name);
        }
    }
}
