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
using System.Drawing;
using OpenMobile.Graphics;

namespace OpenMobile
{
    /// <summary>
    /// Manages Icon display in a UI
    /// </summary>
    public sealed class IconManager
    {
        /// <summary>
        /// Icons have changed
        /// </summary>
        public delegate void IconsChanged();
        /// <summary>
        /// Occurs when icons are added/removed
        /// </summary>
        public event IconsChanged OnIconsChanged;
        /// <summary>
        /// Represents an icon for the UI
        /// </summary>
        public sealed class UIIcon
        {
            /// <summary>
            /// The icons image
            /// </summary>
            public OImage image;
            /// <summary>
            /// The icons priority
            /// </summary>
            public ePriority priority;
            /// <summary>
            /// Full or half sized icon
            /// </summary>
            public bool full;
            /// <summary>
            /// Parent plugin
            /// </summary>
            public string plugin;
            /// <summary>
            /// Extra Info
            /// </summary>
            public string tag;
            /// <summary>
            /// Create a new UI Icon
            /// </summary>
            public UIIcon() { }
            /// <summary>
            /// Create a new UI Icon
            /// </summary>
            /// <param name="i"></param>
            /// <param name="p"></param>
            /// <param name="f"></param>
            public UIIcon(OImage i,ePriority p,bool f)
            {
                image=i;
                priority=p;
                full=f;
            }
            /// <summary>
            /// Create a new UI Icon
            /// </summary>
            /// <param name="i"></param>
            /// <param name="p"></param>
            /// <param name="f"></param>
            /// <param name="plugin"></param>
            public UIIcon(OImage i, ePriority p, bool f,string plugin)
            {
                image = i;
                priority = p;
                full = f;
                this.plugin = plugin;
            }
            /// <summary>
            /// The icons are equal
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                UIIcon icn = (UIIcon)obj;
                if (icn.tag == tag)
                    if (icn.priority == priority)
                        if (icn.plugin == plugin)
                            if (icn.image == image)
                                if (icn.full == full)
                                    return true;
                return false;
            }
        }
        List<UIIcon> icons = new List<UIIcon>();
        /// <summary>
        /// Adds an icon to the collection
        /// </summary>
        /// <param name="icon"></param>
        public void AddIcon(UIIcon icon)
        {
            if (icons.Count == 20)
                return; //something is very wrong if we hit 20 icons
            icons.Add(icon);
            icons.Sort(new iconSort());
            if (OnIconsChanged != null)
                OnIconsChanged();
        }
        /// <summary>
        /// Removes an icon from the collection
        /// </summary>
        /// <param name="icon"></param>
        public void RemoveIcon(UIIcon icon)
        {
            if (icons.Remove(icon))
                if (OnIconsChanged != null)
                    OnIconsChanged();
        }
        /// <summary>
        /// Retrieves an icon for the given position
        /// </summary>
        /// <param name="number"></param>
        /// <param name="full"></param>
        /// <returns></returns>
        public UIIcon getIcon(int number, bool full)
        {
            int num = 0;
            for (int i = 0; i < icons.Count; i++)
            {
                if (icons[i].full == full)
                {
                    num++;
                    if (num==number)
                        return icons[i];
                }
            }
            return new UIIcon();
        }
        class iconSort : IComparer<UIIcon>
        {
            public int Compare(UIIcon x, UIIcon y)
            {
                return x.priority.CompareTo(y.priority);
            }
        }
    }
}
