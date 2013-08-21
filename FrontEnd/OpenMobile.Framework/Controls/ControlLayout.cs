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
using System.Collections.Generic;
using System.Text;
using OpenMobile.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A group of controls referenced by a text string
    /// </summary>
    public class ControlLayout
    {
        /// <summary>
        /// The panel that holds the controls
        /// </summary>
        public OMPanel Panel
        {
            get
            {
                return this._Panel;
            }
            set
            {
                if (this._Panel != value)
                {
                    this._Panel = value;
                }
            }
        }
        private OMPanel _Panel;

        /// <summary>
        /// A string that identifies the controls
        /// </summary>
        public string IDString
        {
            get
            {
                return this._IDString;
            }
            set
            {
                if (this._IDString != value)
                {
                    this._IDString = value;

                    // Find controls that matches the ID string
                    _Controls = _Panel.Controls.FindAll(x => x.Name.Contains(_IDString));
                }
            }
        }
        private string _IDString;

        /// <summary>
        /// The controls that matches the ID string
        /// </summary>
        public List<OMControl> Controls
        {
            get
            {
                return this._Controls;
            }
            set
            {
                if (this._Controls != value)
                {
                    this._Controls = value;
                }
            }
        }
        private List<OMControl> _Controls;        

        /// <summary>
        /// Initialize a new ControlGroup
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="idString"></param>
        public ControlLayout(OMPanel panel, string idString)
        {
            this.Panel = panel;
            this.IDString = idString;
        }

        /// <summary>
        /// Adds controls which matches the specified name to this group
        /// </summary>
        /// <param name="control"></param>
        public void AddControls(string idString)
        {
            _Controls.AddRange(_Panel.Controls.FindAll(x => x.Name.Contains(_IDString)));            
        }

        /// <summary>
        /// Returns the control with the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public OMControl this[string name]
        {
            get
            {
                return _Controls.Find(x => x.Name == name);
            }
        }

        /// <summary>
        /// Returns the control at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public OMControl this[int index]
        {
            get
            {
                return _Controls[index];
            }
        }

        /// <summary>
        /// The total area the controls cover
        /// </summary>
        public Rectangle Region
        {
            get
            {
                return OMControl.GetControlsArea(_Controls);
            }
        }

        /// <summary>
        /// Offsets the controls
        /// </summary>
        /// <param name="offset"></param>
        public void Offset(Point offset)
        {
            foreach (OMControl control in _Controls)
            {
                control.Left += offset.X;
                control.Top += offset.Y;
            }
        }
        /// <summary>
        /// Offsets the controls
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void Offset(int X, int Y)
        {
            foreach (OMControl control in _Controls)
            {
                control.Left += X;
                control.Top += Y;
            }
        }

        /// <summary>
        /// The absolute top value of this group
        /// </summary>
        /// <param name="Y"></param>
        public int Top
        {
            get
            {
                return this.Region.Top;
            }
            set
            {
                int OffsetY = value - this.Region.Top;
                Offset(0, OffsetY);
            }
        }

        /// <summary>
        /// The absolute left value of this group
        /// </summary>
        /// <param name="X"></param>
        public int Left
        {
            get
            {
                return this.Region.Left;
            }
            set
            {
                int OffsetX = value - this.Region.Left;
                Offset(OffsetX, 0);
            }
        }

        /// <summary>
        /// The absolute bottom value of this group (NB! This moves the whole group up, does not change the height)
        /// </summary>
        /// <param name="X"></param>
        public int Bottom
        {
            get
            {
                return this.Region.Bottom;
            }
            set
            {
                int OffsetY = value - this.Region.Bottom;
                Offset(0, OffsetY);
            }
        }

        /// <summary>
        /// The absolute right value of this group (NB! This moves the whole group to the left, does not change the width)
        /// </summary>
        /// <param name="X"></param>
        public int Right
        {
            get
            {
                return this.Region.Right;
            }
            set
            {
                int OffsetX = value - this.Region.Right;
                Offset(OffsetX, 0);
            }
        }

        /// <summary>
        /// Shows / hides the controls (NB! Will only show controls that was previously hidden by a controlLayout)
        /// </summary>
        public bool Visible
        {
            get
            {
                for (int i = 0; i < _Controls.Count; i++)
                {
                    if (_Controls[i].Visible)
                        return true;
                }
                return false;
            }
            set
            {
                foreach (OMControl control in _Controls)
                    control.Visible = value;            
            }
        }

        /// <summary>
        /// Sets the opacity level of the controls (0 transparent - 255 solid)
        /// </summary>
        public int Opacity
        {
            get
            {
                if (_Controls.Count > 0)
                    return _Controls[0].Opacity;
                return this._Opacity;
            }
            set
            {
                this._Opacity = value;
                foreach (OMControl control in _Controls)
                    control.Opacity = _Opacity;
            }
        }
        private int _Opacity;

        /// <summary>
        /// The parent for this group (Read only)
        /// </summary>
        public OMPanel Parent
        {
            get
            {
                if (_Controls.Count > 0)
                    return _Controls[0].Parent;
                return null;
            }
        }

        /// <summary>
        /// Requests a redraw/update of this group
        /// </summary>
        public void Refresh()
        {
            Parent.Refresh();
        }


    }
}
