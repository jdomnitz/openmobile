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

namespace OpenMobile
{
    public sealed class IconManager
    {
        public delegate void IconsChanged();
        public event IconsChanged OnIconsChanged;
        public sealed class UIIcon
        {
            public Image image;
            public ePriority priority;
            public bool full;
            public string plugin;
            public string tag;
            public UIIcon() { }
            public UIIcon(Image i,ePriority p,bool f)
            {
                image=i;
                priority=p;
                full=f;
            }
            public UIIcon(Image i, ePriority p, bool f,string plugin)
            {
                image = i;
                priority = p;
                full = f;
                this.plugin = plugin;
            }
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
        public void AddIcon(UIIcon icon)
        {
            icons.Add(icon);
            icons.Sort(new iconSort());
            if (OnIconsChanged != null)
                OnIconsChanged();
        }
        public void RemoveIcon(UIIcon icon)
        {
            if (icons.Remove(icon))
                if (OnIconsChanged != null)
                    OnIconsChanged();
        }
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
