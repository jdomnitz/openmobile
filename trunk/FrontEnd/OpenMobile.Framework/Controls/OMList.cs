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
using System.Drawing.Drawing2D;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A listbox control
    /// </summary>
    public class OMList:OMLabel,IClickable,IHighlightable,IKey, IList,IThrow
    {
        /// <summary>
        /// Occurs when the list index changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        public delegate void IndexChangedDelegate(OMList sender,int screen);
        /// <summary>
        /// Occurs when the list index changes
        /// </summary>
        public event IndexChangedDelegate SelectedIndexChanged;
        protected int selectedIndex = -1;
        protected List<OMListItem> items;
        System.Timers.Timer throwtmr;
        protected Color itemColor1 = Color.DarkSlateGray;
        protected Color itemColor2 = Color.Black;
        protected Color highlightColor = Color.Black;
        protected Color highlightColorOutline = Color.White;
        protected Color selectedItemColor1 = Color.Silver;
        protected Color selectedItemColor2 = Color.Gray;
        protected Color background = Color.Transparent;
        protected int listItemHeight=60;
        protected bool selectQueued = false;
        protected int h;
        protected int moved;
        protected int start;
        protected int thrown;

        /// <summary>
        /// The background color of the list (Default: Transparent)
        /// </summary>
        public Color Background
        {
            get
            {
                return background;
            }
            set
            {
                background = value;
            }
        }

        /// <summary>
        /// Changes the selected index and scrolls the list so the selected item is visible
        /// </summary>
        /// <param name="index"></param>
        public void Select(int index)
        {
            if ((index < 0) || (index >= items.Count))
                return;
            selectedIndex = index;
            if (h == 0)
            {
                selectQueued = true;
                return;
            }
            if (!((start <= selectedIndex) && (selectedIndex <= start - (Height / h))))
            {
                moved = -(selectedIndex * h);
            }
            selectQueued = false;
            this.refreshMe(this.toRegion());
        }

        /// <summary>
        /// Sorts the list
        /// </summary>
        public void Sort()
        {
            items.Sort();
        }

        /// <summary>
        /// The height in Pixels of each list item
        /// </summary>
        public int ListItemHeight
        {
            get
            {
                return listItemHeight;
            }
            set
            {
                listItemHeight = value;
            }
        }
        
        /// <summary>
        /// Returns the type of control
        /// </summary>
        public static new string TypeName
        {
            get
            {
                return "List";
            }
        }
        /// <summary>
        /// Clears the list
        /// </summary>
        public void Clear()
        {
            items.Clear();
            selectedIndex = -1;
            moved = 0;
            refreshMe(toRegion());
        }
        /// <summary>
        /// List Start
        /// </summary>
        [Browsable(false)]
        public int Start
        {
            get
            {
                return start;
            }
        }
        /// <summary>
        /// Placeholder method
        /// </summary>
        [Browsable(false)]
        public new string Text
        {
            get
            {
                return "";
            }
        }
        
        /// <summary>
        /// Gets or Sets list items
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
            public OMListItem this[int index]{
                get
                {
                    if ((index < (items.Count))&&(index>=0))
                    {
                        return items[index];
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    items[index] = value;
                }
            }
            
        /// <summary>
        /// Returns the list count
        /// </summary>
            public int Count
            {
                get
                {
                    return items.Count;
                }
            }
        /// <summary>
        /// Returns the index of the currently selected item
        /// </summary>
            [Category("List"),Description("Returns the index of the currently selected item")]
            public int SelectedIndex
            {
                get
                {
                    return selectedIndex;
                }
                set
                {
                    if (selectedIndex == value)
                        return;
                    selectedIndex = value;
                }
            }
            /// <summary>
            /// Only add this item if it does not already exist
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool AddDistinct(OMListItem item)
            {
                lock (this)
                {
                    OMListItem it = items.Find(i => i.text == item.text);
                    if ((it == null) == false)
                    {
                        if (it.image == null)
                            it.image = item.image;
                        else
                            return false;
                    }
                    else
                    {
                        items.Add(item);
                        if (this.Width != 0)
                            refreshMe(this.toRegion());
                    }
                    return true;
                }
            }
            /// <summary>
            /// Add a group of list items
            /// </summary>
            /// <param name="source"></param>
            public void AddRange(List<OMListItem> source)
            {
                items.AddRange(source);
                if (this.Width != 0)
                    refreshMe(this.toRegion());
            }
            /// <summary>
            /// Returns the index of the given string
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public int indexOf(string item)
            {
                return items.FindIndex(a => a.text == item);
            }
            /// <summary>
            /// Add a group of list items
            /// </summary>
            /// <param name="source"></param>
            public void AddRange(List<string> source)
            {
                for (int i = 0; i < source.Count; i++)
                    items.Add(new OMListItem(source[i]));
                if (this.Width != 0)
                    refreshMe(this.toRegion());
                }
            /// <summary>
            /// Add a group of list items
            /// </summary>
            /// <param name="source"></param>
            public void AddRange(string[] source)
            {
                for (int i = 0; i < source.Length; i++)
                    items.Add(new OMListItem(source[i]));
                if (this.Width != 0)
                    refreshMe(this.toRegion());
            }
            /// <summary>
            /// Add an item to the list
            /// </summary>
            /// <param name="item">the item</param>
            public void Add(OMListItem item)
            {
                lock (this)
                {
                    items.Add(item);
                    if (this.Width != 0)
                        refreshMe(this.toRegion());
                }
            }
            /// <summary>
            /// Returns the given range of items
            /// </summary>
            /// <param name="index"></param>
            /// <param name="count"></param>
            /// <returns></returns>
            public List<OMListItem> getRange(int index,int count)
            {
                return items.GetRange(index, count);
            }

        /// <summary>
        /// Add an item to the list
        /// </summary>
        /// <param name="item"></param>
            public void Add(string item)
            {
                lock (this)
                {
                    items.Add(new OMListItem(item));
                    if ((this.Width != 0)&&(this.Count<10))
                        refreshMe(this.toRegion());
                }
            }

            /// <summary>
            /// A listview control
            /// </summary>
            public OMList()
            {
                declare();
            }
            /// <summary>
            /// A listview control
            /// </summary>
            /// <param name="x">Left</param>
            /// <param name="y">Top</param>
            /// <param name="w">Width</param>
            /// <param name="h">Height</param>
            public OMList(int x,int y,int w,int h)
            {
                Left = x;
                Top = y;
                Width = w;
                Height = h;
                declare();
            }
            private void declare()
            {
                throwtmr = new System.Timers.Timer(50);
                throwtmr.Elapsed += new ElapsedEventHandler(throwtmr_Elapsed);
                items = new List<OMListItem>();
            }
            /// <summary>
            /// UI Use Only
            /// </summary>
            /// <param name="mouseY"></param>
            /// <param name="scale"></param>
            /// <param name="screen"></param>
            protected virtual void listThrown(int mouseY,float scale,int screen)
            {
                int value = (((int)(mouseY / scale) - top - (moved % h)) / h) + start;
                if (value >= Count)
                    return;
                if (selectedIndex == value)
                {
                    new Thread(delegate() { clickMe(screen); }).Start();
                    return;
                }
                selectedIndex = value;
                if (SelectedIndexChanged != null)
                    new Thread(delegate() { SelectedIndexChanged(this, screen); }).Start();
            }
            void throwtmr_Elapsed(object sender, ElapsedEventArgs e)
            {
                if (thrown == 0)
                {
                    throwtmr.Enabled = false;
                    return;
                }
                if (thrown > 0)
                    thrown -= 1;
                else
                    thrown += 1;
                moved += thrown;
                refreshMe(this.toRegion());
            }
            private eListStyle style;
            /// <summary>
            /// Gets or Sets the list style
            /// </summary>
            [Description("The rendering style of the list"), Category("List")]
            public eListStyle ListStyle
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
            /// The first background color of a non-selected item
            /// </summary>
            [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
            [Description("The second background color of a non-selected item"), Category("List")]
            public Color ItemColor1
            {
                get { return itemColor1; }
                set { itemColor1 = value; }

            }
            /// <summary>
            /// The second background color of a non-selected item
            /// </summary>
            [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
            [Description("The second background color of a non-selected item"), Category("List")]
            public Color ItemColor2
            {
                get { return itemColor2; }
                set { itemColor2 = value; }
            }
            /// <summary>
            /// The Text Color for highlighted items
            /// </summary>
            [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
            [Description("The Text Color for highlighted items"),Category("Text")]
            public Color HighlightColor
            {
                get { return highlightColor; }
                set { highlightColor = value; }
            }
            /// <summary>
            /// The Text Outline/Effect Color for highlighted items
            /// </summary>
            [Description("The Text Outline/Effect Color for highlighted items"),Category("Text")]
            [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
            public Color HighlightedOutlineColor
            {
                get { return highlightColorOutline; }
                set { highlightColorOutline=value; }
            }
            textFormat highlightedItemFormat;
            /// <summary>
            /// The highlighted item text format
            /// </summary>
            [Description("The highlighted item text format"),Category("Text")]
            public textFormat HighlightedItemFormat
            {
                get { return highlightedItemFormat; }
                set { highlightedItemFormat = value; }
            }
            /// <summary>
            /// The first background color of selected items
            /// </summary>
            [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]
            [Description("The first background color of selected items"),Category("List")]
            public Color SelectedItemColor1
            {
                get { return selectedItemColor1; }
                set { selectedItemColor1 = value; }

            }
            /// <summary>
            /// The second background color of selected items
            /// </summary>
            [Description("The second background color of selected items"),Category("List")]
            [Editor(typeof(OpenMobile.transparentColor), typeof(System.Drawing.Design.UITypeEditor)), TypeConverter(typeof(OpenMobile.ColorConvertor))]    
            public Color SelectedItemColor2
            {
                get { return selectedItemColor2; }
                set { selectedItemColor2 = value; }
            }
            /// <summary>
            /// Draws the control
            /// </summary>
            /// <param name="g">The UI's graphics object</param>
            /// <param name="e">Rendering Parameters</param>
            public override void Render(Graphics g,renderingParams e)
            {
                lock (g)
                {
                    if (this.Width == 0)
                        return;
                    float tmp = 1;
                    if (this.Mode == modeType.transitioningIn)
                        tmp = e.globalTransitionIn;
                    else if ((this.Mode == modeType.transitioningOut) || (this.Mode == modeType.ClickedAndTransitioningOut))
                        tmp = e.globalTransitionOut;
                    Region r = g.Clip; //Save the drawing size
                    g.SetClip(this.toRegion()); //But only draw out control
                    if (background!=Color.Transparent)
                        g.FillRectangle(new SolidBrush(Color.FromArgb((int)(tmp*background.A), background)), new Rectangle(Left+1,Top+1,Width-2,Height-2));
                    h = (int)(g.MeasureString("A", Font).Height + 0.5); //Round up
                    if (selectQueued == true)
                    {
                        Select(selectedIndex);
                    }
                    if (ListStyle == eListStyle.MultiList)
                        h = (int)(h*1.75);
                    int ofset = 0;
                    if (((int)style & 1)  == 1)
                    {
                        ofset = listItemHeight;
                        h = (h > ofset) ? h : ofset;
                    }
                    int count = (this.Height / h);
                    int imgSze = 4;
                    if ((moved > 0) || (items.Count * h < Height)) //List start below top
                    {
                        moved = 0;
                        thrown = 0;
                    }
                    else if (((items.Count * h) > Height) && (moved < Height - (items.Count * h))) //Top of the list
                    {
                        moved = Height - (items.Count * h);
                        thrown = 0;
                    }
                    start = -(moved / h);


                    for (int i = start; i <= (count + start + 1); i++)
                    {
                        RectangleF rect = new RectangleF(new PointF(Left, Top + (moved % h) + ((i - start) * h)), new SizeF(this.Width, h));
                        switch(style)
                        {
                            case eListStyle.RoundedImageList:
                            case eListStyle.RoundedTextList:
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                if (selectedIndex == i)
                                    Renderer.FillRoundRectangle(g, new LinearGradientBrush(new Rectangle(new Point(Left, Top + (moved % h)), new Size(this.Width, h)), Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1), Color.FromArgb((int)(tmp * selectedItemColor2.A), selectedItemColor2), LinearGradientMode.Vertical), rect, 10F);
                                else
                                    Renderer.FillRoundRectangle(g, new LinearGradientBrush(new Rectangle(new Point(Left, Top + (moved % h)), new Size(this.Width, h)), Color.FromArgb((int)(tmp * itemColor1.A), itemColor1), Color.FromArgb((int)(tmp * itemColor2.A), itemColor2), LinearGradientMode.Vertical), rect, 10F);
                                g.SmoothingMode = SmoothingMode.Default;
                            break;
                            case eListStyle.TextList:
                            case eListStyle.ImageList:
                            if (selectedIndex == i)
                                    g.FillRectangle(new LinearGradientBrush(new Rectangle(new Point(Left, Top + (moved % h)), new Size(this.Width, h)), Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1), Color.FromArgb((int)(tmp * selectedItemColor2.A), selectedItemColor2), LinearGradientMode.Vertical), rect);
                                else
                                    g.FillRectangle(new LinearGradientBrush(new Rectangle(new Point(Left, Top + (moved % h)), new Size(this.Width, h)), Color.FromArgb((int)(tmp * itemColor1.A), itemColor1), Color.FromArgb((int)(tmp * itemColor2.A), itemColor2), LinearGradientMode.Vertical), rect);
                                break;
                            case eListStyle.TransparentImageList:
                            case eListStyle.TransparentTextList:
                                if (selectedIndex == i)
                                    g.FillRectangle(new SolidBrush(selectedItemColor1), rect);
                                break;
                            case eListStyle.DroidStyleImage:
                            case eListStyle.DroidStyleText:
                                imgSze = 6;
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                if (selectedIndex == i)
                                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(tmp*selectedItemColor1.A*0.6),selectedItemColor1)), new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height - 2));
                                else
                                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1)), new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height - 2));
                                g.SmoothingMode = SmoothingMode.Default;
                                break;
                            case eListStyle.MultiList:
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                if (selectedIndex == i)
                                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1)), new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height - 1));
                                else
                                    g.FillRectangle(new SolidBrush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1)), new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height - 1));
                                g.SmoothingMode = SmoothingMode.Default;
                                break;
                        }
                        if ((i < items.Count) && (i >= 0))
                        {
                            using (StringFormat f = new StringFormat(StringFormatFlags.NoWrap))
                            {
                                if (ListStyle == eListStyle.MultiList)
                                {
                                    f.Alignment = StringAlignment.Near;
                                    f.LineAlignment = StringAlignment.Near;
                                    using (Font fnt = new Font(this.Font.FontFamily.Name, this.Font.Size * 0.7F,FontStyle.Bold))
                                    {
                                        if (selectedIndex == i)
                                        {
                                            g.DrawString(items[i].text, this.Font, new SolidBrush(Color.FromArgb((int)(tmp * highlightColor.A), highlightColor)), new RectangleF(rect.Left + ofset, rect.Top, rect.Width - ofset, rect.Height), f);
                                            f.LineAlignment = StringAlignment.Far;
                                            g.DrawString(items[i].subItem, fnt, new SolidBrush(Color.FromArgb((int)(0.5 * tmp * highlightColor.A), highlightColor)), new RectangleF(rect.Left + ofset, rect.Top, rect.Width - ofset, rect.Height), f);
                                        }
                                        else
                                        {
                                            g.DrawString(items[i].text, this.Font, new SolidBrush(Color.FromArgb((int)(tmp * Color.A), Color)), new RectangleF(rect.Left + ofset, rect.Top, rect.Width - ofset, rect.Height), f);
                                            f.LineAlignment = StringAlignment.Far;
                                            g.DrawString(items[i].subItem, fnt, new SolidBrush(Color.FromArgb((int)(tmp * 0.5 * Color.A), Color)), new RectangleF(rect.Left + ofset, rect.Top, rect.Width - ofset, rect.Height), f);
                                        }
                                    }
                                }
                                else
                                {
                                    f.Alignment = StringAlignment.Center;
                                    f.LineAlignment = StringAlignment.Center;
                                    f.Trimming = StringTrimming.EllipsisCharacter;
                                    if (selectedIndex == i)
                                        g.DrawString(items[i].text, this.Font, new SolidBrush(Color.FromArgb((int)(tmp * highlightColor.A), highlightColor)), new RectangleF(rect.Left + ofset, rect.Top, rect.Width - ofset, rect.Height), f);
                                    else
                                        g.DrawString(items[i].text, this.Font, new SolidBrush(Color.FromArgb((int)(tmp * Color.A), Color)), new RectangleF(rect.Left + ofset, rect.Top, rect.Width - ofset, rect.Height), f);
                                }
                            }
                            if (ofset == 0)
                                continue;
                            if ((items.Count>i)&&(items[i].image != null)) //rare thread collision
                                Renderer.drawTransparentImage(g, items[i].image, (int)rect.Left + 10, (int)rect.Top+2, (int)rect.Height, (int)rect.Height-imgSze, tmp);
                        }
                    }
                    g.Flush();
                    g.Clip = r; //Reset the clip size for the rest of the controls
                }
            }

            #region ICloneable Members
            /// <summary>
            /// Provides a deep copy of this control
            /// </summary>
            /// <returns></returns>
            public override object Clone()
            {
                OMList ret = (OMList)this.MemberwiseClone();
                ret.declare();
                if (this.items.Count > 0)
                    ret.items.AddRange(this.items.GetRange(0, this.items.Count));
                return ret;
            }
            
            #endregion

            #region IClickable Members
            /// <summary>
            /// Button Clicked
            /// </summary>
            public event userInteraction OnClick;
            /// <summary>
            /// Button clicked and held
            /// </summary>
            public event userInteraction OnLongClick;
            /// <summary>
            /// Button double clicked
            /// </summary>
            public event userInteraction OnDoubleClick;
            /// <summary>
            /// Raise the click event
            /// </summary>
            /// <param name="screen"></param>
            public void clickMe(int screen)
            {
                if (OnClick != null)
                    OnClick(this, screen);
            }
            /// <summary>
            /// Raise the double click event
            /// </summary>
            /// <param name="screen"></param>
            public void doubleClickMe(int screen)
            {
                if (OnDoubleClick != null)
                    OnDoubleClick(this, screen);
            }
            /// <summary>
            /// Raise the double click event
            /// </summary>
            /// <param name="screen"></param>
            public void longClickMe(int screen)
            {
                if (OnLongClick != null)
                    OnLongClick(this,screen);
            }

            #endregion

            #region IKey Members
            /// <summary>
            /// Occurs when a Key is pressed while the control is highlighted
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="e"></param>
            /// <param name="WidthScale"></param>
            /// <param name="HeightScale"></param>
            /// <returns></returns>
            public bool KeyDown(int screen, System.Windows.Forms.KeyEventArgs e, float WidthScale, float HeightScale)
            {
                if (e.KeyCode == Keys.PageUp)
                {
                    Select(SelectedIndex-1);
                    return true;
                }
                if (e.KeyCode == Keys.PageDown)
                {
                    Select(SelectedIndex+1);
                    return true;
                }
                if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) || (e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))
                {
                    selectedIndex = -1;
                    this.refreshMe(this.toRegion());
                }
                return false;
            }
            /// <summary>
            /// Occurs when a Key is pressed while the control is highlighted
            /// </summary>
            /// <param name="screen"></param>
            /// <param name="e"></param>
            /// <param name="WidthScale"></param>
            /// <param name="HeightScale"></param>
            /// <returns></returns>
            public bool KeyUp(int screen, System.Windows.Forms.KeyEventArgs e, float WidthScale, float HeightScale)
            {
                return false;
            }
            #endregion

            #region IThrow Members

            void IThrow.MouseThrowStart(int screen, Point StartLocation,PointF scaleFactors, ref bool Cancel)
            {
                listThrown(StartLocation.Y, scaleFactors.Y, screen);
                thrown = 0;
                throwtmr.Enabled = true;
                refreshMe(this.toRegion());
            }

            void IThrow.MouseThrowEnd(int screen, Point EndLocation)
            {
                if (thrown != 0)
                    throwtmr.Enabled = true;
            }

            void IThrow.MouseThrow(int screen, Point TotalDistance, Point RelativeDistance)
            {
                throwtmr.Enabled = false;
                thrown = 0;
                if (Math.Abs(RelativeDistance.Y) > 3)
                {
                    thrown = RelativeDistance.Y;
                    throwtmr.Enabled = true;
                }
                moved +=RelativeDistance.Y;
                if (Math.Abs(TotalDistance.Y)>3)
                    selectedIndex = -1;
                refreshMe(toRegion());
            }

            #endregion
    }
}
