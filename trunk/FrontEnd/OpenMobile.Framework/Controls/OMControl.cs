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
    /// Forces the renderer to redraw this control
    /// </summary>
    public delegate void ReDrawTrigger();

    /// <summary>
    /// The base control type
    /// </summary>
    [System.Serializable()]
    public abstract class OMControl : ICloneable
    {
        /// <summary>
        /// True = rendering for this control is not called automatically
        /// </summary>
        public bool ManualRendering = false;
        
        /// <summary>
        /// Indicates that graphics needs a refresh
        /// </summary>
        protected bool _RefreshGraphic = true;

        /// <summary>
        /// Requests a refresh (regeneration) of the graphics in this control
        /// </summary>
        public void RefreshGraphic()
        {
            _RefreshGraphic = true;
        }
        
        /// <summary>
        /// The region this control occupies
        /// </summary>
        protected Rectangle _Region = new Rectangle();

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

        protected float _RenderingValue_Alpha = 1;
        /// <summary>
        /// Forces the renderer to redraw this control
        /// </summary>
        public event refreshNeeded UpdateThisControl;

        public bool IsControlRenderable(bool DisregardRenderingType) 
        {
            return Visible & (!ManualRendering | DisregardRenderingType);
        }


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
        /// The region this control occupies
        /// </summary>
        public Rectangle Region
        {
            get
            {
                return _Region;
            }
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
        [System.NonSerialized] protected OMPanel parent;
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

        private void UpdateRegion()
        {
            _Region = new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// The controls height in OM units
        /// </summary>
        public virtual int Height
        {
            get { return height; }
            set { 
                height = value;
                _Region.Height = value;
                //UpdateRegion();
                raiseUpdate(true); 
            }
        }
        /// <summary>
        /// The controls width in OM units
        /// </summary>
        public virtual int Width
        {
            get { return width; }
            set {
                width = value;
                _Region.Width = value;
                //UpdateRegion();
                raiseUpdate(true);
            }
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
            set 
                {
                    top = value;
                    _Region.Top = value;
                    //UpdateRegion();
                    raiseUpdate(true);
                }
        }
        /// <summary>
        /// The distance between the left of the UI and the Left of the control
        /// </summary>
        public virtual int Left
        {
            get { return left; }
            set 
                { 
                    left = value;
                    _Region.Left = value;
                    //UpdateRegion();
                    raiseUpdate(true);
                }
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

                _RenderingValue_Alpha = (Opacity / 255F);
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
        /// Get's the alpha value to use when rendering the control
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected float RenderingParams_GetAlphaValue(renderingParams e)
        {
            return System.Math.Min(OpacityFloat, e.Alpha);
        }
        /// <summary>
        /// Get's the correct alpha value to use when rendering the control taking both the current alpha level and a new alpha level into account
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected float GetAlphaValue(float Alpha)
        {
            return System.Math.Min(_RenderingValue_Alpha, Alpha);
        }
        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public virtual void Render(Graphics.Graphics g, renderingParams e)
        {
            RenderBegin(g, e);
            RenderFinish(g, e);
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        internal virtual void RenderBegin(Graphics.Graphics g, renderingParams e)
        {
            // Handle parameters
            RenderingParams_Handle(g, e);
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        internal virtual void RenderFinish(Graphics.Graphics g, renderingParams e)
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
            returnData.Parent = this.Parent;
            Type type = returnData.GetType();

            // Clone fields
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                try
                {
                    //Clone IClonable object
                    if (fieldInfo.FieldType.GetInterface("ICloneable", true) != null)
                    {
                        ICloneable clone = (ICloneable)fieldInfo.GetValue(this);
                        fieldInfo.SetValue(returnData, (clone != null ? clone.Clone() : clone));
                    }
                    else
                    {
                        fieldInfo.SetValue(returnData, fieldInfo.GetValue(this));
                    }
                }
                catch { }
            }

            // Clone properties
            foreach (PropertyInfo propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                MethodInfo mi = propInfo.GetGetMethod();
                if (propInfo.CanWrite && propInfo.CanRead && (mi!=null) && (mi.GetParameters().Length == 0))
                    try
                    {
                        //Clone IClonable object
                        if (propInfo.PropertyType.GetInterface("ICloneable", true) != null)
                        {
                            ICloneable clone = (ICloneable)propInfo.GetValue(this, null);
                            propInfo.SetValue(returnData, (clone != null ? clone.Clone() : clone), null);
                        }
                        else
                        {
                            propInfo.SetValue(returnData, propInfo.GetValue(this, null), null);
                        }
                    }
                    catch { }
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

        /// <summary>
        /// Handles the different rendering parameters
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        internal virtual bool RenderingParams_Handle(Graphics.Graphics g, renderingParams e)
        {
            bool dataHandled = false;

            // Apply offset data?
            if (!e.Offset.IsEmpty)
            {
                left += e.Offset.X;
                top += e.Offset.Y;
                width += e.Offset.Width;
                height += e.Offset.Height;
                UpdateRegion();    
                dataHandled = true;
            }

            // Apply alpha value
            if (e.Alpha > 0)
            {
                if (e.Alpha > OpacityFloat)
                    _RenderingValue_Alpha = OpacityFloat;
                else
                    _RenderingValue_Alpha = e.Alpha;
            }


            return dataHandled;
        }
    }
}
