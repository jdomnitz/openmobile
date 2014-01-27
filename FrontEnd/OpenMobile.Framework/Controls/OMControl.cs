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
using System.Collections.Generic;

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
    /// Control event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="screen"></param>
    public delegate void ControlEventHandler(OMControl sender, int screen);

    /// <summary>
    /// The base control type
    /// </summary>
    [System.Serializable()]
    public abstract class OMControl : ICloneable, IVisibility
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
            Refresh();
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
        /// Control details (multiple uses but for a list item it's the index number)
        /// </summary>
        protected object controlDetail;
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
        /// Indicates wether this control has been rendered
        /// </summary>
        protected bool _ControlRendered = false;

        /// <summary>
        /// The value to use as rendering for the current alpha level
        /// </summary>
        protected float _RenderingValue_Alpha = 1;
        /// <summary>
        /// Forces the renderer to redraw this control
        /// </summary>
        public event refreshNeeded UpdateThisControl;

        /// <summary>
        /// Is the control renderable? Returns true if is, false otherwise
        /// </summary>
        /// <param name="DisregardRenderingType"></param>
        /// <returns></returns>
        public bool IsControlRenderable(bool DisregardRenderingType=true) 
        {
            return Visible & _Internal_Visibility & (!ManualRendering | DisregardRenderingType);
        }

        /// <summary>
        /// Requests the control be redrawn
        /// </summary>
        /// <param name="resetHighlighted"></param>
        protected void raiseUpdate(bool resetHighlighted)
        {
            if (!visible && !raiseUpdate_AllowWhenNotVisible)
                return;
            raiseUpdate_AllowWhenNotVisible = false;
            if (UpdateThisControl != null)
                UpdateThisControl(resetHighlighted);
        }
        private bool raiseUpdate_AllowWhenNotVisible = false;



        /// <summary>
        /// The region this control occupies
        /// <para>Use this as a read only property, use the separate field for left, top, width and height to set the controls values</para>
        /// </summary>
        public Rectangle Region
        {
            get
            {
                //// Connect events to ensure we get updates
                //if (_Region_PropertyChangedHandler == null)
                //{
                //    _Region_PropertyChangedHandler = new PropertyChangedEventHandler(_Region_PropertyChanged);
                //    if (_Region != null)
                //        _Region.PropertyChanged += _Region_PropertyChangedHandler;
                //}                

                return _Region;
            }
            set
            {
                //if (_Region_PropertyChangedHandler == null)
                //{
                //    _Region_PropertyChangedHandler = new PropertyChangedEventHandler(_Region_PropertyChanged);
                //    if (_Region!= null)
                //        _Region.PropertyChanged += _Region_PropertyChangedHandler;
                //}
                
                if (_Region != value)
                {
                    //// Disconnect old events
                    //if (_Region != null)
                    //    _Region.PropertyChanged -= _Region_PropertyChangedHandler;
                    
                    _Region = value;

                    //// Connect new events
                    //_Region.PropertyChanged += _Region_PropertyChangedHandler;

                    left = _Region.Left;
                    top = _Region.Top;
                    width = Region.Width;
                    height = Region.Height;
                    onSizeChanged_Internal();
                    Refresh();
                }
            }
        }

        private PropertyChangedEventHandler _Region_PropertyChangedHandler = null;
        void _Region_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
            // Update local properties
            left = _Region.Left;
            top = _Region.Top;
            width = Region.Width;
            height = Region.Height;
            onSizeChanged_Internal();
            Refresh();
        }

        /// <summary>
        /// Requests a redraw/update of this control
        /// </summary>
        public virtual void Refresh()
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
                Refresh();
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
        public virtual bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible == value)
                    return;
                raiseUpdate_AllowWhenNotVisible = true;
                visible = value;
                Refresh();
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
                Refresh();
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
                Refresh();
            }
        }

        private void UpdateRegion()
        {
            _Region = new Rectangle(left, top, width, height);
            onSizeChanged_Internal();
        }

        /// <summary>
        /// The controls height in OM units
        /// </summary>
        public virtual int Height
        {
            get { return height; }
            set {
                if (height != value)
                {
                    height = value;
                    _Region.Height = value;
                    onSizeChanged_Internal();
                    Refresh();
                }
            }
        }
        /// <summary>
        /// The controls width in OM units
        /// </summary>
        public virtual int Width
        {
            get { return width; }
            set {
                if (width != value)
                {
                    width = value;
                    _Region.Width = value;
                    onSizeChanged_Internal();
                    Refresh();
                }
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
                    Refresh();
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
                    Refresh();
                }
        }

        /// <summary>
        /// Control transparency level in percent (0 solid - 100 transparent)
        /// </summary>
        public int Transparency
        {
            get
            {
                return (int)((opacity / 255F) * 100f);
            }
            set
            {
                int opacityLevel = 255 - (int)(255 * (value * 0.01f));
                if (opacity != opacityLevel)
                    Opacity = opacityLevel;
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
                Refresh();
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
        protected float GetAlphaValue1(float Alpha)
        {
            return System.Math.Min(_RenderingValue_Alpha, Alpha);
        }
        /// <summary>
        /// Get's the correct alpha value to use when rendering the control taking both the current alpha level and a new alpha level into account
        /// </summary>
        /// <param name="Alpha"></param>
        /// <returns></returns>
        protected float GetAlphaValue255(float Alpha)
        {
            float Alpha01 = Alpha / 255f;
            return 255f * System.Math.Min(_RenderingValue_Alpha, Alpha01);
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
        /// Called when the control is removed from the screen and rendering should stop
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public virtual void RenderStop(Graphics.Graphics g, renderingParams e)
        {
        }

        /// <summary>
        /// Executes the renderingparamters whitout rendering the control
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        internal virtual void RenderEmpty(Graphics.Graphics g, renderingParams e)
        {
            RenderBegin(g, e);
        }
        
        /// <summary>
        /// Executes the initial rendering code
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        internal virtual void RenderBegin(Graphics.Graphics g, renderingParams e)
        {
            // Handle parameters
            RenderingParams_Handle(g, e);
        }

        /// <summary>
        /// Executes the final rendering code
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        internal virtual void RenderFinish(Graphics.Graphics g, renderingParams e)
        {
            _ControlRendered = true;
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
            return Clone(this.Parent);
        }

        /// <summary>
        /// Create a deep copy of this control and specifies the parent
        /// </summary>
        /// <returns></returns>
        public virtual object Clone(OMPanel parent)
        {
            OMControl returnData = (OMControl)this.MemberwiseClone();
            returnData.Parent = parent;
            Type type = returnData.GetType();

            // Clone fields
            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                try
                {
                    //Clone IClonable object
                    if (fieldInfo.Name != "parent" && fieldInfo.Name != "Parent")
                    {
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
                }
                catch { }
            }

            // Clone properties
            foreach (PropertyInfo propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            {
                MethodInfo mi = propInfo.GetGetMethod();
                if (propInfo.CanWrite && propInfo.CanRead && (mi != null) && (mi.GetParameters().Length == 0))
                    try
                    {
                        if (propInfo.Name != "parent" && propInfo.Name != "Parent")
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

        /// <summary>
        /// Returns the total area covered by the controls passed along in the parameters
        /// </summary>
        /// <returns></returns>
        static public Rectangle GetControlsArea(List<OMControl> controls)
        {
            int l = int.MaxValue;
            int t = int.MaxValue;
            int r = int.MinValue;
            int b = int.MinValue;

            // Return default area if no controls is available
            if (controls.Count == 0)
                return new Rectangle(0, 0, 0, 0);

            foreach (OMControl control in controls)
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

        /// <summary>
        /// Returns the control that's loaded at the active screen
        /// </summary>
        /// <returns></returns>
        public OMControl GetControlAtActiveScreen()
        {
            if (this.parent == null)
                return null;
            return this.parent[this.parent.ActiveScreen, this.name];
        }
        /// <summary>
        /// Returns the control that's loaded at the specific screen
        /// </summary>
        /// <returns></returns>
        public OMControl GetControlAtScreen(int screen)
        {
            if (this.parent == null)
                return null;
            return this.parent[screen, this.name];
        }

        /// <summary>
        /// Translates this control to a new location
        /// </summary>
        /// <param name="p"></param>
        public void Translate(Point p)
        {
            Left += p.X;
            Top += p.Y;
        }

        /// <summary>
        /// Translates this control to a new location
        /// </summary>
        /// <param name="p"></param>
        public void Translate(int x, int y)
        {
            Left += x;
            Top += y;
        }

        /// <summary>
        /// Translates this control to a new location
        /// </summary>
        /// <param name="p"></param>
        public void Translate(Rectangle r)
        {
            Left += r.X;
            Top += r.Y;
        }

        #region DataSource handling

        /// <summary>
        /// Refreshes the controls underlying datasource subscriptions
        /// </summary>
        public void DataSource_Refresh()
        {
            Data.DataSource ds = BuiltInComponents.Host.DataHandler.GetDataSource(_DataSource);
            if (ds != null)
            {   // Simulate an updated datasource value
                DataSource_OnChanged(ds);
            }

            if (_DataSource_InLine_Sources != null)
            {
                for (int i = 0; i < _DataSource_InLine_Sources.Length; i++)
                {
                    ds = BuiltInComponents.Host.DataHandler.GetDataSource(_DataSource_InLine_Sources[i]);
                    if (ds != null)
                    {   // Simulate an updated datasource value
                        _DataSource_InLine_Changed(ds);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the datasource to use for visibility (a null or binary value can be used)
        /// </summary>
        public string Visible_DataSource
        {
            get
            {
                return this._Visible_DataSource;
            }
            set
            {
                // Unscubscribe to any existing data
                if (!String.IsNullOrEmpty(this._Visible_DataSource))
                    BuiltInComponents.Host.DataHandler.UnsubscribeFromDataSource(this._Visible_DataSource, Visible_DataSource_OnChanged);

                this._Visible_DataSource = value;

                // Don't do anything if a parent is not available
                if (this.parent == null || !this.parent.IsClonedForScreens())
                    return;

                // Check for special dataref of screen present 
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Contains(OpenMobile.Data.DataSource.DataTag_Screen))
                    {   // Present, replace with screen reference
                        value = value.Replace(OpenMobile.Data.DataSource.DataTag_Screen, this.parent.ActiveScreen.ToString());
                        this._Visible_DataSource = value;
                    }
                }

                // Subscribe to updates
                if (!String.IsNullOrEmpty(value))
                    if (!BuiltInComponents.Host.DataHandler.SubscribeToDataSource(this._Visible_DataSource, Visible_DataSource_OnChanged))
                        DataSource_Missing(String.Empty);
            }
        }
        private string _Visible_DataSource;

        /// <summary>
        /// Sets the sensor to subscribe to
        /// </summary>
        public string DataSource
        {
            get
            {
                return this._DataSource;
            }
            set
            {
                // Unscubscribe to any existing data
                if (!String.IsNullOrEmpty(this._DataSource))
                    BuiltInComponents.Host.DataHandler.UnsubscribeFromDataSource(_DataSource, DataSource_OnChanged);

                this._DataSource = value;

                // Don't do anything if a parent is not available
                if (this.parent == null || !this.parent.IsClonedForScreens())
                    return;

                // Check for special dataref of screen present 
                if (!string.IsNullOrEmpty(value))
                {
                    this._DataSource = DataNameBase.GetDataNameWithScreen(this.parent.ActiveScreen, value);
                }
               
                // Subscribe to updates
                if (!String.IsNullOrEmpty(value))
                    if (!BuiltInComponents.Host.DataHandler.SubscribeToDataSource(_DataSource, DataSource_OnChanged))
                        DataSource_Missing(String.Empty);
            }
        }
        private string _DataSource;

        internal virtual void Visible_DataSource_OnChanged(OpenMobile.Data.DataSource dataSource)
        {
            // Is this a binary data source, if so use the true/false state to show/hide image
            if (dataSource.DataType == OpenMobile.Data.DataSource.DataTypes.binary)
            {
                try
                {
                    _Internal_Visibility = (bool)dataSource.Value;
                }
                catch
                {
                    _Internal_Visibility = false;
                }
            }
            else
            {   // This is not a binary datasource, use null to detect state
                _Internal_Visibility = dataSource.Value != null;
            }
            _RefreshGraphic = true;
            Refresh();
        }

        internal virtual void DataSource_OnChanged(OpenMobile.Data.DataSource dataSource)
        { 
        }

        internal virtual void DataSource_Missing(string propertyName)
        {
        }
        
        #region InLine datasource prossessing

        /// <summary>
        /// If true the control can handle inline datasource references like this "cpu load is {System.CPU.Load}"
        /// </summary>
        public bool AllowInLineDataSources
        {
            get
            {
                return this._AllowInLineDataSources;
            }
            set
            {
                if (this._AllowInLineDataSources != value)
                {
                    this._AllowInLineDataSources = value;
                }
            }
        }
        private bool _AllowInLineDataSources = true;

        private string[] _DataSource_InLine_Sources = null;
        private string _DataSource_InLine;
        internal bool DataSource_InLine(ref string s)
        {
            // Don't do anything if we've already processed this string
            if (!_AllowInLineDataSources || string.IsNullOrEmpty(s) || !(s.Contains("{") && s.Contains("}")) || this.parent == null || !this.parent.IsClonedForScreens())
            {
                if (!string.IsNullOrEmpty(s) && s.Contains("{") && s.Contains("}"))
                    return true;
                else
                    return false;
            }

            // Check for special dataref of screen present 
            if (s.Contains(OpenMobile.Data.DataSource.DataTag_Screen))
            {   // Present, replace with screen reference
                s = s.Replace(OpenMobile.Data.DataSource.DataTag_Screen, this.parent.ActiveScreen.ToString());
            }

            // Unsubscribe to existing datasources
            if (_DataSource_InLine_Sources != null)
                for (int i = 0; i < _DataSource_InLine_Sources.Length; i++)
                    BuiltInComponents.Host.DataHandler.UnsubscribeFromDataSource(_DataSource_InLine_Sources[i], _DataSource_InLine_Changed);

            // Save current string to process
            _DataSource_InLine = s;

            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(@"\{.+?\}");
            System.Text.RegularExpressions.MatchCollection Matches = regEx.Matches(s);

            // Loop trough the matches and connect the subscriptions
            _DataSource_InLine_Sources = new string[Matches.Count];
            for (int i = 0; i < Matches.Count; i++)
			{
			    string match = Matches[i].ToString();

                // Save data sources without the brackets
                _DataSource_InLine_Sources[i] = match.Replace("{", "").Replace("}", "");

                // Subscribe to datasource
                BuiltInComponents.Host.DataHandler.SubscribeToDataSource(_DataSource_InLine_Sources[i], _DataSource_InLine_Changed);
			}
            return true;   
        }

        private void _DataSource_InLine_Changed(OpenMobile.Data.DataSource dataSource)
        {
            string s = _DataSource_InLine;

            // Loop trough each source to ensure we've got all the data before we pass the string on
            for (int i = 0; i < _DataSource_InLine_Sources.Length; i++)
            {
                string source = _DataSource_InLine_Sources[i];

                // Cancel if one of the sources is null as this is an incomplete event update
                if (string.IsNullOrEmpty(source))
                    return;

                // Replace datamarkers in string
                if (source == dataSource.FullName)
                    s = s.Replace(string.Format("{{{0}}}", dataSource.FullName), dataSource.FormatedValue);
                else
                {   // Additional data needed, query the datahandler
                    OpenMobile.Data.DataSource AdditionalSource = BuiltInComponents.Host.DataHandler.GetDataSource(_DataSource_InLine_Sources[i]);
                    if (AdditionalSource != null)
                        s = s.Replace(string.Format("{{{0}}}", AdditionalSource.FullName), AdditionalSource.FormatedValue);
                }

            }
            
            // Send data on to the target
            DataSource_InLine_Changed(s);
        }

        internal virtual void DataSource_InLine_Changed(string s)
        {

        }

        #endregion


        #region MultiProperty support

        protected Dictionary<string, string> _DataSource_PropertyList = new Dictionary<string, string>();

        internal virtual void DataSource_RegisterProperty(string propertyName, string datasource)
        {
            // Don't do anything if a parent is not available
            if (this.parent == null || !this.parent.IsClonedForScreens() || datasource == null)
                return;

            // Check for already existing datasource property, if not create it
            if (!_DataSource_PropertyList.ContainsKey(propertyName))
                _DataSource_PropertyList.Add(propertyName, datasource);
            
            // Unscubscribe to any existing data
            if (!String.IsNullOrEmpty(_DataSource_PropertyList[propertyName]))
                BuiltInComponents.Host.DataHandler.UnsubscribeFromDataSource(_DataSource_PropertyList[propertyName], Internal_DataSource_MultiProperty_OnChanged);

            _DataSource_PropertyList[propertyName] = datasource;

            // Check for special dataref of screen present 
            if (!string.IsNullOrEmpty(datasource))
            {
                if (datasource.Contains(OpenMobile.Data.DataSource.DataTag_Screen))
                {   // Present, replace with screen reference
                    datasource = datasource.Replace(OpenMobile.Data.DataSource.DataTag_Screen, this.parent.ActiveScreen.ToString());
                    _DataSource_PropertyList[propertyName] = datasource;
                }
            }

            // Subscribe to updates
            if (!String.IsNullOrEmpty(datasource))
                if (!BuiltInComponents.Host.DataHandler.SubscribeToDataSource(datasource, Internal_DataSource_MultiProperty_OnChanged))
                    DataSource_Missing(propertyName);

        }

        private void Internal_DataSource_MultiProperty_OnChanged(OpenMobile.Data.DataSource dataSource)
        {
            // Find the matching property name for this datasource
            foreach (KeyValuePair<string,string> datasourceProperty in _DataSource_PropertyList)
            {
                if (datasourceProperty.Value == dataSource.FullName)
                {
                    // Send update to this property
                    DataSource_MultiProperty_OnChanged(datasourceProperty.Key, dataSource);
                }
            }
        }

        internal virtual void DataSource_MultiProperty_OnChanged(string propertyName, OpenMobile.Data.DataSource dataSource)
        {
        }

        #endregion

        #endregion



        internal virtual object Command_Execute(string command)
        {
            // Check for special dataref of screen present 
            if (!string.IsNullOrEmpty(command))
            {
                command = DataNameBase.GetDataNameWithScreen(this.parent.ActiveScreen, command);

                //if (command.Contains(OpenMobile.Data.DataSource.DataTag_Screen))
                //{   // Present, replace with screen reference
                //    command = command.Replace(OpenMobile.Data.DataSource.DataTag_Screen, this.parent.ActiveScreen.ToString());
                //}

                return OM.Host.CommandHandler.ExecuteCommand(command);
            }
            return null;
        }

        /// <summary>
        /// A virtual method that is called when the size of this control is changing
        /// </summary>
        protected virtual void onSizeChanged()
        {
            // No action
        }

        public event userInteraction OnResize;
        private void onSizeChanged_Internal()
        {
            onSizeChanged();
            if (this.parent != null)
            {
                if (OnResize != null)
                    OnResize(this, this.containingScreen());
            }
        }

        /// <summary>
        /// Sets or gets the status of the internal visibility control variable
        /// </summary>
        bool IVisibility.Internal_Visibility
        {
            get
            {
                return _Internal_Visibility;
            }
            set
            {
                if (_Internal_Visibility == value)
                    return;

                _Internal_Visibility = value;
                Refresh();
            }
        }
        private bool _Internal_Visibility = true;

        /// <summary>
        /// Converts a global point in OM to a point that is local to a control
        /// </summary>
        /// <param name="absolutePoint"></param>
        /// <returns></returns>
        protected Point GetLocalControlPoint(Point absolutePoint)
        {
            return absolutePoint - this.Region.Location;
        }

    }
}
