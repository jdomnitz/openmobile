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
using System.ComponentModel;
using OpenMobile.Graphics;
using System;
using System.Reflection;
using OpenMobile.Framework;

namespace OpenMobile.Controls
{
    public interface iPanelEvents
    {
        void RaiseEvent(int screen, eEventType eventType);
    }


    /// <summary>
    /// The default control container
    /// </summary>
    [Serializable]
    public class OMPanel : iPanelEvents
    {
        /// <summary>
        /// Contains the number of the currently assigned screen for this panel
        /// </summary>
        public int ActiveScreen { get; set; }
        
        private List<OMControl> containedControls = new List<OMControl>();
        private imageItem background;

        public enum PanelTypes { Normal, Modal }
        public PanelTypes PanelType = PanelTypes.Normal;
        

        /// <summary>
        /// Request a screen refresh
        /// </summary>
        public event refreshNeeded UpdateThisControl;

        /// <summary>
        /// The plugin that last transitioned in this panel
        /// </summary>
        public OpenMobile.Plugin.IBasePlugin OwnerPlugin { get; set; }            

        /// <summary>
        /// Returns the OMControl at the given index
        /// <para>Please use named access in skins instead of indexed access</para>
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        public OMControl this[int i]
        {
            get
            {
                return containedControls[i];
            }
            set
            {
                containedControls[i] = value;
            }
        }
        /// <summary>
        /// Returns the OMControl with the given name
        /// </summary>
        /// <param name="s">The Name of the control</param>
        /// <returns></returns>
        public OMControl this[string s]
        {
            get
            {
                return containedControls.Find(p => p.Name == s);
            }
        }
        /// <summary>
        /// Returns the OMControl with the given name for a specific screen
        /// <para>Returns the same as this[string s] if this panel is not loaded to a screen manager</para>
        /// <para>if loaded it returns the requested control from the specified screen</para>
        /// </summary>
        /// <param name="screen">The screen to access</param>
        /// <param name="s">The Name of the control</param>
        /// <returns></returns>
        public OMControl this[int screen, string s]
        {
            get
            {
                if (Manager == null)
                    return this[s];
                else
                    return Manager[screen, this.name][s];
            }
        }
        
        /// <summary>
        /// Add all controls from an existing panel
        /// </summary>
        /// <param name="source"></param>
        public void addRange(OMPanel source)
        {
            foreach (OMControl c in source.containedControls)
                addControl(c);
        }

        /// <summary>
        /// Adds all controls from a controlgroup to a panel relative to a basepoint
        /// </summary>
        /// <param name="basePoint">Base point to use when placing controls</param>
        /// <param name="cg"></param>
        public void addControlGroup(Point basePoint, ControlGroup cg)
        {
            foreach (OMControl control in cg)
            {
                control.Translate(basePoint);
                addControl_Internal(control, true);
            }
        }

        /// <summary>
        /// Adds all controls from a controlgroup to a panel relative to a basepoint
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="cg"></param>
        public void addControlGroup(int left, int top, ControlGroup cg)
        {
            addControlGroup(new Point(left, top), cg);
        }

        /// <summary>
        /// Adds all controls from a controlgroup to a panel relative to another control
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="cg"></param>
        public void addControlGroup(OMControl control, ControlGroup cg)
        {
            addControlGroup(new Point(control.Left, control.Top), cg);
        }
        private OMControl _LastAddedControl = null;

        /// <summary>
        /// Adds a control relative to a reference control
        /// </summary>
        /// <param name="control"></param>
        /// <param name="placementReference"></param>
        /// <param name="direction"></param>
        public void addControl(OMControl control, OMControl placementReference, ControlDirections direction, ControlSizeControl sizeControl = ControlSizeControl.None)
        {
            helperFunctions.Controls.Controls.PlaceControl(placementReference, control, direction, sizeControl);
            addControl(control);
        }

        /// <summary>
        /// Adds a control relative to the last added control
        /// </summary>
        /// <param name="control"></param>
        /// <param name="direction"></param>
        public void addControl(OMControl control, ControlDirections direction, ControlSizeControl sizeControl = ControlSizeControl.None)
        {
            helperFunctions.Controls.Controls.PlaceControl(_LastAddedControl, control, direction, sizeControl);
            addControl(control);
        }

        /// <summary>
        /// Adds a control relative to the last added control with an additional offset
        /// </summary>
        /// <param name="control"></param>
        /// <param name="direction"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public void addControl(OMControl control, ControlDirections direction, int offsetX, int offsetY)
        {
            addControl(control, direction);
            control.Left += offsetX;
            control.Top += offsetY;
        }

        /// <summary>
        /// Adds a control to the container
        /// </summary>
        /// <param name="control"></param>
        public void addControl(OMControl control)
        {
            addControl_Internal(control, true);
        }
        /// <summary>
        /// Adds a control to the container
        /// </summary>
        /// <param name="control"></param>
        /// <param name="changeParent"></param>
        private void addControl_Internal(OMControl control, bool changeParent)
        {
            if (control == null)
                return;
            if (changeParent)
                control.Parent = this;
            control.UpdateThisControl += raiseUpdate;
            
            // Offset control relative to the base point
            control.Translate(_BasePoint);

            containedControls.Add(control);
            _LastAddedControl = control;
            raiseUpdate(false);
        }

        internal void raiseUpdate(bool resetHighlighted)
        {
            if (UpdateThisControl != null)
                UpdateThisControl(resetHighlighted);
        }

        /// <summary>
        /// Adds a control to the top of the container
        /// </summary>
        /// <param name="control"></param>
        public void insertControl(OMControl control)
        {
            control.Parent = this;
            control.UpdateThisControl += raiseUpdate;
            containedControls.Insert(0, control);
            raiseUpdate(false);
        }

        /// <summary>
        /// Requests a redraw of this panel
        /// </summary>
        public void Refresh()
        {
            raiseUpdate(false);
        }

        #region decreaseZOrder

        /// <summary>
        /// Moves the control back in the display order
        /// </summary>
        /// <param name="control"></param>
        /// <returns>If successful</returns>
        public bool decreaseZOrder(int control)
        {
            if (control > 0)
            {
                OMControl tmp = containedControls[control - 1];
                containedControls[control - 1] = containedControls[control];
                containedControls[control] = tmp;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Moves the control back in the display order
        /// </summary>
        /// <param name="control"></param>
        /// <returns>If successful</returns>
        public bool decreaseZOrder(string name)
        {
            int control;
            try
            {
                control = containedControls.FindIndex(c => c.Name == name);
            }
            catch (System.ArgumentNullException)
            {
                return false;
            }
            return decreaseZOrder(control);
        }
        /// <summary>
        /// Moves the control back in the display order
        /// </summary>
        /// <param name="control"></param>
        /// <returns>If successful</returns>
        public bool decreaseZOrder(OMControl c)
        {
            int control = containedControls.IndexOf(c);
            return decreaseZOrder(control);
        }

        #endregion

        #region increaseZOrder

        /// <summary>
        /// Moves the control forward in the display order
        /// </summary>
        /// <param name="control"></param>
        /// <returns>If successful</returns>
        public bool increaseZOrder(int control)
        {
            if ((control >= 0) && (control < containedControls.Count - 1))
            {
                OMControl tmp = containedControls[control + 1];
                containedControls[control + 1] = containedControls[control];
                containedControls[control] = tmp;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Moves the control forward in the display order
        /// </summary>
        /// <param name="control"></param>
        /// <returns>If successful</returns>
        public bool increaseZOrder(string name)
        {
            int control;
            try
            {
                control = containedControls.FindIndex(c => c.Name == name);
            }
            catch (System.ArgumentNullException)
            {
                return false;
            }
            return increaseZOrder(control);
        }
        /// <summary>
        /// Moves the control forward in the display order
        /// </summary>
        /// <param name="control"></param>
        /// <returns>If successful</returns>
        public bool increaseZOrder(OMControl c)
        {
            int control = containedControls.IndexOf(c);
            return increaseZOrder(control);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <returns>True if the panel contains the given control</returns>
        public bool contains(OMControl control)
        {
            return containedControls.Contains(control);
        }
        /// <summary>
        /// Finds the control with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public OMControl Find(string name)
        {
            return containedControls.Find(p => p.Name == name);
        }

        /// <summary>
        /// Set to true to render the screen with as high FPS as possible when rendering this panel
        /// </summary>
        public bool FastRendering
        {
            get
            {
                return this._FastRendering;
            }
            set
            {
                if (this._FastRendering != value)
                {
                    this._FastRendering = value;
                }
            }
        }
        private bool _FastRendering;        

        /// <summary>
        /// The current mode of the panel (used for transitions)
        /// </summary>
        public eModeType Mode = eModeType.Normal;
        /// <summary>
        /// Render the panel to the given graphics context
        /// </summary>
        /// <param name="g"></param>
        /// <param name="e"></param>
        public void Render(Graphics.Graphics g, renderingParams e)
        {
            //float tmp = 1F;
            //if (Mode == eModeType.transitioningIn)
            //    tmp = e.globalTransitionIn;
            //else if (Mode == eModeType.transitioningOut)
            //    tmp = e.globalTransitionOut;
            switch (BackgroundType)
            {
                case backgroundStyle.Gradiant:
                    g.FillRectangle(new Brush(Color.FromArgb((int)(e.Alpha * BackgroundColor1.A), BackgroundColor1), Color.FromArgb((int)(e.Alpha * BackgroundColor2.A), BackgroundColor2), Gradient.Vertical), 0, 0, 1000, 600);
                    break;
                case backgroundStyle.SolidColor:
                    g.FillRectangle(new Brush(Color.FromArgb((int)(e.Alpha * BackgroundColor1.A), BackgroundColor1)), 0, 0, 1000, 600);
                    break;
                case backgroundStyle.Image:
                    if (BackgroundImage.image != null)
                        g.DrawImage(BackgroundImage.image, new Rectangle(0, 0, 1000, 600), 0, 0, BackgroundImage.image.Width, BackgroundImage.image.Height, e.Alpha);
                    break;
            }
            for (int i = 0; i < containedControls.Count; i++)
            {
                if (containedControls[i].IsControlRenderable(false))
                {
                    containedControls[i].Render(g, e);
                }
                else
                {
                    containedControls[i].RenderEmpty(g, e);
                }
            }
        }
        /// <summary>
        /// The panel priority (used to set panel layer)
        /// </summary>
        public ePriority Priority = ePriority.Normal;
        /// <summary>
        /// If true, transitionFromAll does not effect this panel
        /// </summary>
        public bool UIPanel;
        /// <summary>
        /// Removes the given control from this panel
        /// </summary>
        /// <param name="control"></param>
        public void Remove(OMControl control)
        {
            control.UpdateThisControl -= UpdateThisControl;
            containedControls.Remove(control);
        }

        /// <summary>
        /// Creates a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public OMPanel Clone()
        {
            return Clone(0);
        }

        /// <summary>
        /// Creates a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public OMPanel Clone(int screen)
        {
            ScreenManager manager = this.Manager;
            OMPanel two = (OMPanel)this.MemberwiseClone();
            two.Manager = manager;
            two.ActiveScreen = screen;

            //two.containedControls = DeepCopy.DeepCopyBinary<List<OMControl>>(containedControls);

            two.containedControls = new List<OMControl>(this.containedControls.Capacity);
            for (int i = 0; i < containedControls.Count; i++)
            {
                two.addControl((OMControl)this.containedControls[i].Clone(two));
                two[two.controlCount - 1].Parent = two;
            }
            if (this.tag is System.ICloneable)
                two.tag = ((System.ICloneable)this.tag).Clone();
            return two;
        }

        /// <summary>
        /// Gets the control at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The requested control</returns>
        public OMControl getControl(int index)
        {
            if ((index >= 0) && (index < containedControls.Count))
            {
                return containedControls[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the panel that's loaded at the specified screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns>The requested panel</returns>
        public OMPanel getPanelAtScreen(int screen)
        {
            if (Manager == null)
                return null;
            else
                return Manager[screen, this.name];
        }
        /// <summary>
        /// Get's the panels that's currently used for this control instance
        /// </summary>
        /// <returns></returns>
        public OMPanel getActivePanel()
        {
            if (Manager == null)
                return null;
            else
                return Manager[ActiveScreen, this.name];
        }

        /// <summary>
        /// Returns the number of controls contained in this panel
        /// </summary>
        [Browsable(false)]
        public int controlCount
        {
            get
            {
                return containedControls.Count;
            }
        }

        public List<OMControl> Controls
        {
            get
            {
                return containedControls;
            }
        }

        private Color color1 = Color.DarkBlue;
        private Color color2 = Color.MediumSlateBlue;

        /// <summary>
        /// The panels background color
        /// </summary>
        public Color BackgroundColor1
        {
            get
            {
                return color1;
            }
            set
            {
                if (color1 == value)
                    return;
                color1 = value;
            }
        }
        /// <summary>
        /// The second color in a background gradiant
        /// </summary>
        public Color BackgroundColor2
        {
            get
            {
                return color2;
            }
            set
            {
                if (color2 == value)
                    return;
                color2 = value;
            }
        }

        private backgroundStyle style;
        /// <summary>
        /// Sets the type of background drawn for this panel
        /// </summary>
        public backgroundStyle BackgroundType
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
            }
        }

        /// <summary>
        /// The background image rendered behind the controls
        /// </summary>
        public imageItem BackgroundImage
        {
            get
            {
                return background;
            }
            set
            {
                if (background == value)
                    return;
                background = value;
            }
        }
        // Start of code added by Borte
        private object tag;
        /// <summary>
        /// Used to store additional information about an panel (Added by Borte)
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
        private bool forgotten;
        /// <summary>
        /// If set to true, this panel is not stored in the history (and will be skipped by the GoBack function)
        /// </summary>
        public bool Forgotten
        {
            get
            {
                return forgotten;
            }
            set
            {
                forgotten = value;
            }
        }
        private string name;
        /// <summary>
        /// The name for this panel
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
        /// Removes the given control from this panel (Added by Borte)
        /// </summary>
        /// <param name="name">Name of control to remove</param>
        public void Remove(string name)
        {
            List<OMControl> controls = containedControls.FindAll(c => c.Name == name);
            foreach (OMControl c in controls)
            {
                c.UpdateThisControl -= raiseUpdate;
                Remove(c);
            }
        }
        /// <summary>
        /// Create a new panel
        /// </summary>
        [Obsolete("Always provide a panel name. Method will be removed in next release")]
        public OMPanel()
            : this(string.Empty)
        {
        }
        /// <summary>
        /// Create a new panel with an given name
        /// <param name="Name">Name of the panel</param>
        /// </summary>
        public OMPanel(string Name)
        {
            this.name = Name;
            _IsLoaded = new bool[BuiltInComponents.Host.ScreenCount];
            _IsVisible = new bool[BuiltInComponents.Host.ScreenCount];
        }
        /// <summary>
        /// Create a new panel with an given name
        /// <param name="Name">Name of the panel</param>
        /// <param name="Header">Header of the panel (Descriptive text that can be shown to the user)</param>
        /// </summary>
        public OMPanel(string Name, string Header)
            : this(Name)
        {
            this._Header = Header;
        }

        /// <summary>
        /// Create a new panel with an given name
        /// <param name="Name">Name of the panel</param>
        /// <param name="Header">Header of the panel (Descriptive text that can be shown to the user)</param>
        /// <param name="icon">icon for the panel (Icon that will be shown when the panel is showing)</param>
        /// </summary>
        public OMPanel(string Name, string Header, imageItem icon)
            : this(Name)
        {
            this._Header = Header;
            this._Icon = icon;
        }

        /// <summary>
        /// Create a new panel with an given name
        /// <param name="Name">Name of the panel</param>
        /// <param name="Header">Header of the panel (Descriptive text that can be shown to the user)</param>
        /// <param name="backgroundColor">background color to use for the panel</param>
        /// </summary>
        public OMPanel(string Name, string Header, Color backgroundColor)
            : this(Name)
        {
            this._Header = Header;
            this.BackgroundType = backgroundStyle.SolidColor;
            this.BackgroundColor1 = backgroundColor;
        }

        /// <summary>
        /// Moves the control to the back of the display order
        /// </summary>
        /// <param name="name">Name of control</param>
        /// <returns>If successful</returns>
        public bool MoveControlToBack(string name)
        {
            int control;
            try
            {
                control = containedControls.FindIndex(c => c.Name == name);
            }
            catch (System.ArgumentNullException)
            {
                return false;
            }
            return MoveControlToBack(control);
        }
        /// <summary>
        /// Moves the control to the back of the display order
        /// </summary>
        /// <param name="c">Control</param>
        /// <returns>If successful</returns>
        public bool MoveControlToBack(OMControl c)
        {
            int control = containedControls.IndexOf(c);
            return MoveControlToBack(control);
        }
        /// <summary>
        /// Moves the control to the back of the display order
        /// </summary>
        /// <param name="control">Index of control</param>
        /// <returns>If successful</returns>
        public bool MoveControlToBack(int control)
        {
            if ((control > 0) && (control <= containedControls.Count))
            {
                for (int i = control; i > 1; i--)
                {
                    OMControl tmp = containedControls[i - 1];
                    containedControls[i - 1] = containedControls[i];
                    containedControls[i] = tmp;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Moves the control to the front of the display order
        /// </summary>
        /// <param name="name">Name of control</param>
        /// <returns>If successful</returns>
        public bool MoveControlToFront(string name)
        {
            int control;
            try
            {
                control = containedControls.FindIndex(c => c.Name == name);
            }
            catch (System.ArgumentNullException)
            {
                return false;
            }
            return MoveControlToFront(control);
        }
        /// <summary>
        /// Moves the control to the front of the display order
        /// </summary>
        /// <param name="c">Control</param>
        /// <returns>If successful</returns>
        public bool MoveControlToFront(OMControl c)
        {
            int control = containedControls.IndexOf(c);
            return MoveControlToFront(control);
        }
        /// <summary>
        /// Moves the control to the front of the display order
        /// </summary>
        /// <param name="control">Index of control</param>
        /// <returns>If successful</returns>
        public bool MoveControlToFront(int control)
        {
            if ((control > 0) && (control <= containedControls.Count))
            {
                for (int i = control; i < containedControls.Count - 1; i++)
                {
                    OMControl tmp = containedControls[i + 1];
                    containedControls[i + 1] = containedControls[i];
                    containedControls[i] = tmp;
                }
                return true;
            }
            return false;
        }
        internal bool hooked()
        {
            return UpdateThisControl != null;
        }
        private int container = -1;
        internal int containingScreen()
        {
            if (container != -1)
                return container;
            if (UpdateThisControl != null)
            {
                object UI = UpdateThisControl.GetInvocationList()[0].Target;
                PropertyInfo info = UI.GetType().GetProperty("Screen");
                container = (int)info.GetValue(UI, null);
                return container;
            }
            return -1;
        }
        /// <summary>
        /// Clear all contained controls
        /// </summary>
        public void clear()
        {
            for (int i = containedControls.Count - 1; i >= 0; i--)
            {
                containedControls[i].UpdateThisControl -= raiseUpdate;
                containedControls.RemoveAt(i);
            }
        }
        /// <summary>
        /// Hit test the panel and return the resulting control
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public OMControl controlAtPoint(Point p)
        {
            for (int i = containedControls.Count - 1; i >= 0; i--)
                if (containedControls[i].toRegion().Contains(p))
                    return containedControls[i];
            return null;
        }
        /// <summary>
        /// Returns the panel name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.name == null)
                return String.Format("({0})", this.GetHashCode());
            return String.Format("{0}({1})", this.name, this.GetHashCode());
        }

        /// <summary>
        /// Is this panel loaded for the separate screens?
        /// </summary>
        public bool IsClonedForScreens()
        {
            return (this.Manager != null);
        }

        /// <summary>
        /// Is this panel loaded and ready for transition
        /// </summary>
        public bool IsLoaded(int screen)
        {
            if (screen >= _IsLoaded.Length)
                return false;
            return _IsLoaded[screen];
        }
        private bool[] _IsLoaded = null; 

        /// <summary>
        /// Is this panel currently visible
        /// </summary>
        public bool IsVisible(int screen)
        {
            if (screen >= _IsVisible.Length)
                return false;
            return _IsVisible[screen];
        }
        private bool[] _IsVisible = null; 

        #region Events

        /// <summary>
        /// Occurs when entering the panel (about to be shown on screen)
        /// <para>NB! Events has to be mapped before the panel is added to a screen manager</para>
        /// </summary>
        public event PanelEvent Entering;
        /// <summary>
        /// Occurs when leaving the panel (about to be removed from screen)
        /// <para>NB! Events has to be mapped before the panel is added to a screen manager</para>
        /// </summary>
        public event PanelEvent Leaving;
        /// <summary>
        /// Occurs when the panel is loaded and is ready to be shown on screen
        /// <para>NB! Events has to be mapped before the panel is added to a screen manager</para>
        /// </summary>
        public event PanelEvent Loaded;
        /// <summary>
        /// Occurs when the panel is unloaded and ready to be removed from screen
        /// <para>NB! Events has to be mapped before the panel is added to a screen manager</para>
        /// </summary>
        public event PanelEvent Unloaded;

        /// <summary>
        /// Raises a panel event
        /// This method is reserved for internal usage
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="eventType"></param>
        void iPanelEvents.RaiseEvent(int screen, eEventType eventType)
        {
            //BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "OMPanel (" + this.name + ") Event: " + eventType.ToString() + " (screen: " + screen.ToString() + ")");
            switch (eventType)
            {
                case eEventType.Entering:
                    {
                        _IsVisible[screen] = true;

                        // Show any popup menu set for the panel
                        if (_PopUpMenu != null)
                            OM.Host.UIHandler.PopUpMenu.SetButtonStrip(screen, _PopUpMenu);

                        // Raise event
                        if (Entering != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Entering(this, screen));
                    }
                    break;
                case eEventType.Leaving:
                    {
                        _IsVisible[screen] = false;
                        
                        // Raise event
                        if (Leaving != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Leaving(this, screen));
                    }
                    break;
                case eEventType.Loaded:
                    {
                        _IsLoaded[screen] = true;

                        // Raise event
                        if (Loaded != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Loaded(this, screen));
                    }
                    break;
                case eEventType.Unloaded:
                    {
                        _IsLoaded[screen] = false;

                        // Raise event
                        if (Unloaded != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Unloaded(this, screen));
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

        /// <summary>
        /// The current screenmanager handling this panel
        /// </summary>
        public OpenMobile.Framework.ScreenManager Manager { get; set; }

        /// <summary>
        /// The base point for placing controls on this panel
        /// </summary>
        public Point BasePoint
        {
            get
            {
                return this._BasePoint;
            }
            set
            {
                if (this._BasePoint != value)
                {
                    this._BasePoint = value;

                    // Offset any controls that's already loaded
                    Point Offset = _BasePoint - _BasePointOld;
                    for (int i = 0; i < containedControls.Count; i++)
                        containedControls[i].Translate(Offset);
                }
            }
        }
        private Point _BasePoint;
        private Point _BasePointOld;

        /// <summary>
        /// Sets or gets the header text for the panel
        /// </summary>
        public string Header
        {
            get
            {
                return this._Header;
            }
            set
            {
                if (this._Header != value)
                {
                    this._Header = value;
                }
            }
        }
        private string _Header;

        /// <summary>
        /// The icon for this panel
        /// </summary>
        public imageItem Icon
        {
            get
            {
                return this._Icon;
            }
            set
            {
                if (this._Icon != value)
                {
                    this._Icon = value;
                }
            }
        }
        private imageItem _Icon;        

        /// <summary>
        /// The transition effect to use when showing this panel
        /// <para>NB! This will override what ever effect is requested by the transition command but only if this is the only panel being transitioned in</para>
        /// </summary>
        public eGlobalTransition TransitionEffect_Show
        {
            get
            {
                return this._TransitionEffect_Show;
            }
            set
            {
                if (this._TransitionEffect_Show != value)
                {
                    this._TransitionEffect_Show = value;
                }
            }
        }
        private eGlobalTransition _TransitionEffect_Show;

        /// <summary>
        /// The transition effect to use when hideing this panel
        /// <para>NB! This will override what ever effect is requested by the transition command but only if this is the only panel being transitioned in</para>
        /// </summary>
        public eGlobalTransition TransitionEffect_Hide
        {
            get
            {
                return this._TransitionEffect_Hide;
            }
            set
            {
                if (this._TransitionEffect_Hide != value)
                {
                    this._TransitionEffect_Hide = value;
                }
            }
        }
        private eGlobalTransition _TransitionEffect_Hide;

        /// <summary>
        /// The button strip to use as a popup menu
        /// </summary>
        public ButtonStrip PopUpMenu
        {
            get
            {
                return this._PopUpMenu;
            }
            set
            {
                if (this._PopUpMenu != value)
                {
                    this._PopUpMenu = value;
                }
            }
        }
        private ButtonStrip _PopUpMenu;
    }

    /// <summary>
    /// Panel event
    /// </summary>
    /// <param name="sender">reference to the source panel</param>
    /// <param name="screen">current screen</param>
    public delegate void PanelEvent(OMPanel sender, int screen);

    /// <summary>
    /// Panel event types
    /// </summary>
    public enum eEventType 
    { 
        /// <summary>
        /// Entering the panel (about to be shown on screen)
        /// </summary>
        Entering, 
        /// <summary>
        /// Leaving the panel (about to be removed from screen)
        /// </summary>
        Leaving, 
        /// <summary>
        /// Panel is loaded and is ready to be shown on screen
        /// </summary>
        Loaded, 
        /// <summary>
        /// Panel is unloaded and ready to be removed from screen
        /// </summary>
        Unloaded 
    };

        
    
}
