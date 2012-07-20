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
    public class OMContainer : OMControl, IContainer2
    {
        private class ControlData
        {
            public OMControl Control { get; set; }
            public int Left { get; set; }
            public int Top { get; set; }

            public ControlData(OMControl control)
            {
                this.Control = control;
                this.Left = control.Left;
                this.Top = control.Top;
            }
        }

        List<ControlData> _Controls = new List<ControlData>();

        public override void Render(OpenMobile.Graphics.Graphics g, renderingParams e)
        {
            // No special rendering need for this control, use base rendering
            base.RenderBegin(g, e);
            base.RenderFinish(g, e);
        }

        /// <summary>
        /// Adds a control to this container
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool Add(OMControl control)
        {
            // Only add controls that aren't already loaded
            if (_Controls.Find(x => x.Control.Name == control.Name) == null)
            {
                // Add control
                _Controls.Add(new ControlData(control));

                // Add this control to the panel
                if (this.parent != null)
                {
                    AddAndConfigureControl(control);
                    return true;
                }
            }
            return false;
        }

        private void AddAndConfigureControl(OMControl control)
        {
            // Place control
            PlaceControl(control);

            // Add control to parent object
            this.parent.addControl(control);
        }

        private void PlaceControls()
        {
            foreach (ControlData cd in _Controls)
            {
                // Change position of this control from relative to this container to absolute on the panel
                cd.Left = this.Left + cd.Left;
                cd.Top = this.Top + cd.Top;
            }
        }
        private void PlaceControlsAtRunTime()
        {
            if (this.parent == null)
                return;

            OMPanel panelInstance = this.parent.getPanelAtScreen(this.parent.ActiveScreen);//.getActivePanel();
            if (panelInstance == null)
                return;
 
            foreach (ControlData cd in _Controls)
            {
                OMControl ControlInstance = panelInstance[cd.Control.Name];
                // Change position of this control from relative to this container to absolute on the panel
                ControlInstance.Left = this.Left + cd.Left;
                ControlInstance.Top = this.Top + cd.Top;
            }
        }
        private void PlaceControl(OMControl control)
        {
            ControlData cd = _Controls.Find(x => x.Control == control);
            if (cd != null)
            {
                // Change position of this control from relative to this container to absolute on the panel
                control.Left = this.Left + cd.Left;
                control.Top = this.Top + cd.Top;
            }
        }

        public OMContainer(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
        }

        public override OMPanel Parent
        {
            get
            {
                return base.Parent;
            }
            internal set
            {
                base.Parent = value;

                // Register controls
                foreach (ControlData cd in _Controls)
                    AddAndConfigureControl(cd.Control);
            }
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
                PlaceControlsAtRunTime();
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
                PlaceControlsAtRunTime();
            }
        }


        #region IContainer2 Members

        public List<OMControl> Controls
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IContainer2 Members

        List<OMControl> IContainer2.Controls
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
