using System;
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
