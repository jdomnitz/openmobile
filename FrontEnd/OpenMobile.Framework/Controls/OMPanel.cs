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
    /// <summary>
    /// The default control container
    /// </summary>
    public class OMPanel
    {
        /// <summary>
        /// Contains the number of the currently assigned screen for this panel
        /// </summary>
        public int ActiveScreen { get; set; }
        
        private List<OMControl> containedControls = new List<OMControl>();
        private imageItem background;
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
        /// <para>OBSOLOTE, Use named access instead!</para>
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns></returns>
        [Obsolete("Obsolete, Use named access instead!")]
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
        /// Adds a control to the container
        /// </summary>
        /// <param name="control"></param>
        public void addControl(OMControl control)
        {
            addControl(control, true);
        }
        /// <summary>
        /// Adds a control to the container
        /// </summary>
        /// <param name="control"></param>
        /// <param name="changeParent"></param>
        public void addControl(OMControl control, bool changeParent)
        {
            if (control == null)
                return;
            if (changeParent)
                control.Parent = this;
            control.UpdateThisControl += raiseUpdate;
            containedControls.Add(control);
            raiseUpdate(false);
        }

        private void raiseUpdate(bool refreshNeeded)
        {
            if (UpdateThisControl != null)
                UpdateThisControl(refreshNeeded);
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
            float tmp = 1F;
            if (Mode == eModeType.transitioningIn)
                tmp = e.globalTransitionIn;
            else if (Mode == eModeType.transitioningOut)
                tmp = e.globalTransitionOut;
            switch (BackgroundType)
            {
                case backgroundStyle.Gradiant:
                    g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * BackgroundColor1.A), BackgroundColor1), Color.FromArgb((int)(tmp * BackgroundColor2.A), BackgroundColor2), Gradient.Vertical), 0, 0, 1000, 600);
                    break;
                case backgroundStyle.SolidColor:
                    g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * BackgroundColor1.A), BackgroundColor1)), 0, 0, 1000, 600);
                    break;
                case backgroundStyle.Image:
                    if (BackgroundImage.image != null)
                        g.DrawImage(BackgroundImage.image, new Rectangle(0, 0, 1000, 600), 0, 0, BackgroundImage.image.Width, BackgroundImage.image.Height, tmp);
                    break;
            }
            for (int i = 0; i < containedControls.Count; i++)
                if (containedControls[i].Visible)
                    containedControls[i].Render(g, e);
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
            ScreenManager manager = this.Manager;
            OMPanel two = (OMPanel)this.MemberwiseClone();
            two.Manager = manager;
            two.containedControls = new List<OMControl>(this.containedControls.Capacity);
            for (int i = 0; i < containedControls.Count; i++)
            {
                two.addControl((OMControl)this.containedControls[i].Clone());
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
        {
            this.name = string.Empty;
        }
        /// <summary>
        /// Create a new panel with an given name
        /// <param name="Name">Name of the panel</param>
        /// </summary>
        public OMPanel(string Name)
        {
            this.name = Name;
        }
        //Added by Borte
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
        //***
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void RaiseEvent(int screen, eEventType eventType)
        {
            //BuiltInComponents.Host.DebugMsg(DebugMessageType.Info, "OMPanel (" + this.name + ") Event: " + eventType.ToString() + " (screen: " + screen.ToString() + ")");
            switch (eventType)
            {
                case eEventType.Entering:
                    {   // Raise event
                        if (Entering != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Entering(this, screen));
                    }
                    break;
                case eEventType.Leaving:
                    {   // Raise event
                        if (Leaving != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Leaving(this, screen));
                    }
                    break;
                case eEventType.Loaded:
                    {   // Raise event
                        if (Loaded != null)
                            OpenMobile.Threading.SafeThread.Asynchronous(() => Loaded(this, screen));
                    }
                    break;
                case eEventType.Unloaded:
                    {   // Raise event
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
