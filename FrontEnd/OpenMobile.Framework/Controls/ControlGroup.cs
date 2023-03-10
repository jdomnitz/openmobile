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

namespace OpenMobile.Controls
{
    /// <summary>
    /// Directions to use when adding items relative to each other
    /// </summary>
    public enum ControlDirections
    {
        /// <summary>
        /// No direction
        /// </summary>
        None,

        /// <summary>
        /// Down
        /// </summary>
        Down = 1,

        /// <summary>
        /// Up
        /// </summary>
        Up = -1,

        /// <summary>
        /// Right
        /// </summary>
        Right = 2,

        /// <summary>
        /// Left
        /// </summary>
        Left = -2,

        /// <summary>
        /// Centered horizontally
        /// </summary>
        CenterHorizontally = 3,

        /// <summary>
        /// Placement is relative to the parent
        /// </summary>
        RelativeToParent = 4,

        /// <summary>
        /// Placement is absolute on the screen
        /// </summary>
        Absolute = 5,

        /// <summary>
        /// Centered horizontally
        /// </summary>
        CenterVertically = 6

    }

    /// <summary>
    /// Controls how the size of a control is affected
    /// </summary>
    public enum ControlSizeControl
    {
        /// <summary>
        /// No change
        /// </summary>
        None,

        /// <summary>
        /// Set same size 
        /// </summary>
        SameSize,

        /// <summary>
        /// Set same height
        /// </summary>
        SameHeight,

        /// <summary>
        /// Set same width
        /// </summary>
        SameWidth
    }

    /// <summary>
    /// A collection of controls which can be used during initialization (NB! This list may not be valid during runtime)
    /// </summary>
    public class ControlGroup : List<OMControl>
    {
        public Rectangle Region
        {
            get
            {
                if (this.Count == 0)
                    return new Rectangle();

                Rectangle TotalRegion = this[0].Region;
                // Return the combined area of contained controls
                for (int i = 1; i < this.Count; i++)
                    TotalRegion.Union(this[i].Region);

                // Check for size override
                if (!_ItemSize.IsEmpty)
                    //return new Rectangle(TotalRegion.Center.X - (_ItemSize.Width / 2), TotalRegion.Center.Y - (_ItemSize.Height / 2), _ItemSize.Width, _ItemSize.Height);
                    return new Rectangle(TotalRegion.Left, TotalRegion.Top, _ItemSize.Width, _ItemSize.Height);
                
                // No override                
                return TotalRegion;
            }
        }

        /// <summary>
        /// The X placement of this control group
        /// </summary>
        public int X
        {
            get
            {
                return Region.X;
            }
            set
            {
                Rectangle r = Region;
                if (r.X != value)
                {
                    // Calculate offset 
                    int offset = value - r.X;
                    Translate(offset, 0);
                }
            }
        }

        /// <summary>
        /// The Y placement of this control group
        /// </summary>
        public int Y
        {
            get
            {
                return Region.Y;
            }
            set
            {
                Rectangle r = Region;
                if (r.Y != value)
                {
                    // Calculate offset 
                    int offset = value - r.Y;
                    Translate(0, offset);
                }
            }
        }

        public void Translate(Rectangle r)
        {
            // Run command on each contained control
            foreach (OMControl control in this)
                control.Translate(r);
        }

        public void Translate(int x, int y)
        {
            // Run command on each contained control
            foreach (OMControl control in this)
                control.Translate(x, y);
        }

        public bool Contains(string name)
        {
            OMControl c = this.Find(x => x.Name == name);
            return (c != null);
        }

        public ControlGroup()
        {
        }

        public ControlGroup(OMControl control)
        {
            this.Add(control);
        }

        public bool IsHighlighted()
        {
            foreach (OMControl control in this)
            {
                if (control.Mode == eModeType.Highlighted)
                    return true;
            }
            return false;
        }

        public OMControl this[string name]
        {
            get
            {
                return this.Find(x => x.Name.Contains(name));
            }
            set
            {
                int index = this.FindIndex(x => x.Name.Contains(name));
                this.RemoveAt(index);
                this.Insert(index, value);
            }
        }

        public ControlDirections PlacementDirection { get; set; }

        public string Key { get; set; }

        public ControlGroup Clone(OMPanel parent)
        {
            ControlGroup newCG = new ControlGroup();
            foreach (OMControl control in this)
                newCG.Add((OMControl)control.Clone(parent));
            return newCG;
        }

        protected Size _ItemSize = new Size();
        public Size ItemSize
        {
            get
            {
                return _ItemSize;
            }
            set
            {
                _ItemSize = value;
            }
        }

        /// <summary>
        /// Controls the visibility for the controls contained in this group
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="screen"></param>
        /// <param name="visible"></param>
        public void SetVisible(OMPanel panel, int screen, bool visible)
        {
            foreach (var control in this)
                panel[screen, control.Name].Visible = visible;
        }
    }
}
