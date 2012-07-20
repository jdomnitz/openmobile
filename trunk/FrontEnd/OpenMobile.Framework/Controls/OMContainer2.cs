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
    public class OMContainer2 : OMControl, IContainer2
    {
        List<OMControl> _Controls = new List<OMControl>();
        Rectangle oldRegion = new Rectangle();

        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            base.RenderBegin(g, e);

            // Pass offset data along with renderingparameters
            if (!oldRegion.IsEmpty)
                e.Offset = Region - oldRegion;
            oldRegion = Region;

            for (int i = 0; i < _Controls.Count; i++)
                if (_Controls[i].IsControlRenderable(true))
                    _Controls[i].Render(g, e);

            // Reset offset data
            e.Offset = new Rectangle();

            base.RenderFinish(g, e);
        }

        private void AddAndConfigureControl(OMControl control)
        {
            // Place control
            PlaceControl(control);

            // Block controls automatic rendering
            control.ManualRendering = true;

            // Add control to parent object
            //this.parent.addControl(control);
        }

        private void PlaceControls()
        {
            foreach (OMControl control in _Controls)
            {
                // Change position of this control from relative to this container to absolute on the panel
                control.Left = this.Left + control.Left;
                control.Top = this.Top + control.Top;
            }
        }

        private void PlaceControl(OMControl control)
        {
            // Change position of this control from relative to this container to absolute on the panel
            control.Left = this.Left + control.Left;
            control.Top = this.Top + control.Top;
        }

        public OMContainer2(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
        }

        public override int Left
        {
            get
            {
                return base.Left;
            }
            set
            {
                base.Left = value;
            }
        }

        public override int Top
        {
            get
            {
                return base.Top;
            }
            set
            {
                base.Top = value;
            }
        }

        #region IContainer2 Members

        public List<OMControl> Controls
        {
            get { return _Controls; }
        }

        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool addControl(OMControl control)
        {
            // Only add controls that aren't already loaded
            if (_Controls.Find(x => x.Name == control.Name) == null)
            {
                // Add control
                _Controls.Add(control);

                // Add this control to the panel
                if (this.parent != null)
                {
                    AddAndConfigureControl(control);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ICloneable Members

        public override object Clone()
        {
            //OMContainer2 newObject = (OMContainer2)this.MemberwiseClone();
            OMContainer2 newObject = (OMContainer2)base.Clone();
            newObject._Controls = new List<OMControl>();
            foreach (OMControl control in _Controls)
                newObject._Controls.Add((OMControl)control.Clone());
            return newObject;
        }

        #endregion
    }
}
