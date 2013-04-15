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
        CenterHorizontally = 3
    }

    /// <summary>
    /// A collection of controls for usage in the OMContainer control
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

        public void Translate(Rectangle r)
        {
            // Run command on each contained control
            foreach (OMControl control in this)
                control.Region.Translate(r);
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

        public ControlGroup Clone()
        {
            ControlGroup newCG = new ControlGroup();
            foreach (OMControl control in this)
                newCG.Add((OMControl)control.Clone());
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

    }
}
