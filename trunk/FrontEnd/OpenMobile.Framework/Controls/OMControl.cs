using System;
using System.ComponentModel;
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
using System.Drawing;
using System.Reflection;
namespace OpenMobile.Controls
{
    /// <summary>
    /// Forces the renderer to redraw this control
    /// </summary>
    public delegate void refreshNeeded(Rectangle region);

    /// <summary>
    /// The base control type
    /// </summary>
    public abstract class OMControl : ICloneable
    {
        protected modeType mode;
        protected bool visible = true;
        protected object tag;
        protected string name;

        /// <summary>
        /// Forces the renderer to redraw this control
        /// </summary>
        public event refreshNeeded UpdateThisControl;

        /// <summary>
        /// Forces the renderer to redraw this control
        /// </summary>
        protected void refreshMe(Rectangle region)
        {
            if ((UpdateThisControl != null)&&(visible==true))
            {
                UpdateThisControl(region);
            }
        }
        /// <summary>
        /// Returns the screen the control is currently being rendered on.
        /// Note this function uses reflection and is quite slow, use it only when other alternatives do not exist.
        /// Returns -1 if not attached to any screen
        /// </summary>
        /// <returns></returns>
        public int containingScreen()
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
        private int container = -1;
        /// <summary>
        /// If the control has been attached to a user interface (aka is currently loaded)
        /// </summary>
        /// <returns></returns>
        public bool hooked()
        {
            return (UpdateThisControl != null);
        }
        /// <summary>
        /// The rendering mode of the control
        /// </summary>
        [Browsable(false)]
        public virtual modeType Mode
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
        [Description("The globally unique name for the control")]
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
        [Description("Should the control be displayed")]
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
                    UpdateThisControl(this.toRegion());
            }
        }

        /// <summary>
        /// The controls height in pixels
        /// </summary>
        [Description("The controls height in pixels")]
        public abstract int Height
        {
            get;
            set;
        }
        /// <summary>
        /// The controls width in pixels
        /// </summary>
        [Description("The controls width in pixels")]
        public abstract int Width
        {
            get;
            set;
        }
        /// <summary>
        /// Used to store additional information about a control
        /// </summary>
        [Description("Used to store additional information about a control")]
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
        [Description("The distance between the top of the UI and the Top of the control")]
        public abstract int Top
        {
            get;
            set;
        }
        /// <summary>
        /// The distance between the left of the UI and the Left of the control
        /// </summary>
        [Description("The distance between the Left of the UI and the Left of the control")]
        public abstract int Left
        {
            get;
            set;
        }
        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="g">The User Interfaces graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public abstract void Render(Graphics g, renderingParams e);

        /// <summary>
        /// Returns the region occupied by the control
        /// </summary>
        /// <returns></returns>
        public abstract Rectangle toRegion();
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
                if ((propInfo.CanWrite == true) && (propInfo.GetGetMethod() != null) && (propInfo.GetGetMethod().GetParameters().Length == 0))
                    try
                    {
                        propInfo.SetValue(returnData, propInfo.GetValue(this, null), null);
                    }
                    catch (TargetInvocationException) { }
            }
            return returnData;
        }
    }
}
