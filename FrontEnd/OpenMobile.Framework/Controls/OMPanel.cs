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
using System.Drawing;

namespace OpenMobile.Controls
{
    /// <summary>
    /// The default control container
    /// </summary>
    public class OMPanel
    {
        private List<OMControl> containedControls=new List<OMControl>();
        private imageItem background;
        private bool doubleClickable;

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
            set
            {
                OMControl c = containedControls.Find(p => p.Name == s);
                c = value;
            }
        }

        /// <summary>
        /// Adds a control to the container
        /// </summary>
        /// <param name="control"></param>
        public void addControl(OMControl control)
        {
            containedControls.Add(control);
        }
        /// <summary>
        /// Adds a control to the top of the container
        /// </summary>
        /// <param name="control"></param>
        public void insertControl(OMControl control)
        {
            containedControls.Insert(0,control);
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
        /// Removes the given control from this panel
        /// </summary>
        /// <param name="control"></param>
        public void Remove(OMControl control)
        {
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

        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static string TypeName
        {
            get
            {
                return "Panel";
            }
        }
        private Color color1=Color.DarkBlue;
        private Color color2=Color.MediumSlateBlue;
        
        /// <summary>
        /// The panels background color
        /// </summary>
        [Editor(typeof(OpenMobile.transparentColor),typeof(System.Drawing.Design.UITypeEditor)),TypeConverter(typeof(OpenMobile.ColorConvertor))]
        [Category("Panel")]
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
        [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
        [Category("Panel")]
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
        [Category("Panel")]
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
        [Category("Panel")]
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
        /// <summary>
        /// Can controls on this panel be double clicked (setting to false increases responsiveness)
        /// </summary>
        [Category("Panel")]
        public bool DoubleClickable
        {
            get
            {
                return doubleClickable;
            }
            set
            {
                doubleClickable = value;
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
            containedControls.RemoveAll(c => c.Name == name);
        }
        /// <summary>
        /// Create a new panel (Added by Borte)
        /// </summary>
        public OMPanel()
        {
        }
        /// <summary>
        /// Create a new panel with an given name (Added by Borte)
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
        //***
    }
}
