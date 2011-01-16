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

namespace OpenMobile.Controls
{
    /// <summary>
    /// The default control container
    /// </summary>
    public class OMPanel
    {
        private List<OMControl> containedControls=new List<OMControl>();
        private imageItem background;
        public event refreshNeeded UpdateThisControl;

        /// <summary>
        /// Returns the OMControl at the given index
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
            control.UpdateThisControl +=raiseUpdate;
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
            containedControls.Insert(0,control);
            raiseUpdate(false);
        }

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
                    g.FillRectangle(new Brush(Color.FromArgb((int)(tmp*BackgroundColor1.A),BackgroundColor1), Color.FromArgb((int)(tmp*BackgroundColor2.A),BackgroundColor2), Gradient.Vertical), 0, 0, 1000, 600);
                    break;
                case backgroundStyle.SolidColor:
                    g.FillRectangle(new Brush(Color.FromArgb((int)(tmp*BackgroundColor1.A), BackgroundColor1)),0,0,1000,600);
                    break;
                case backgroundStyle.Image:
                    if (BackgroundImage.image != null)
                        g.DrawImage(BackgroundImage.image, new Rectangle(0, 0, 1000, 600), 0, 0, BackgroundImage.image.Width, BackgroundImage.image.Height,tmp);
                    break;
            }
            for (int i = 0; i < containedControls.Count; i++)
                if (containedControls[i].Visible)
                    containedControls[i].Render(g, e);
        }
        /// <summary>
        /// The panel priority (used to set panel layer)
        /// </summary>
        public ePriority Priority=ePriority.Normal;
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
            OMPanel two= (OMPanel)this.MemberwiseClone();
            two.containedControls = new List<OMControl>(this.containedControls.Capacity);
            for (int i=0;i<containedControls.Count;i++)
            {
                two.addControl((OMControl)this.containedControls[i].Clone());
                two[two.controlCount - 1].Parent = two;
            }
            // Start of code added by borte     
            if (this.tag is System.ICloneable)
                two.tag = ((System.ICloneable)this.tag).Clone();
            // End of code added by borte
            return two;
        }

        /// <summary>
        /// Gets the control at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The requested control</returns>
        public OMControl getControl(int index)
        {
            if ((index>=0)&&(index < containedControls.Count))
            {
                return containedControls[index];
            }else{
                return null;
            }
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

        private Color color1=Color.DarkBlue;
        private Color color2=Color.MediumSlateBlue;
        
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
        private string name=string.Empty;
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
        public OMPanel()
        {
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

        public void clear()
        {
            for (int i = containedControls.Count - 1; i >= 0; i--)
            {
                containedControls[i].UpdateThisControl -= raiseUpdate;
                containedControls.RemoveAt(i);
            }
        }

        public OMControl controlAtPoint(Point p)
        {
            for (int i = containedControls.Count - 1; i >= 0;i--)
                if (containedControls[i].toRegion().Contains(p))
                    return containedControls[i];
            return null;
        }

        public override string ToString()
        {
            if (this.name == null)
                return "";
            return this.name;
        }
    }
}
