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
using System.Drawing.Drawing2D;
using System.Threading;
using System.Timers;
using System;
using OpenMobile.Graphics;
using OpenMobile;
using OpenMobile.Input;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A listbox control
    /// </summary>
    public class OMList : OMLabel, IClickable, IHighlightable, IKey, IList, IThrow, IMouse,IKeyboard
    {
        /// <summary>
        /// Occurs when the list index changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        public delegate void IndexChangedDelegate(OMList sender, int screen);
        /// <summary>
        /// Occurs when the list index changes
        /// </summary>
        public event IndexChangedDelegate SelectedIndexChanged;
        public event IndexChangedDelegate HighlightedIndexChanged;
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
        protected bool selectQueued = false;
        protected int listHeight;
        protected int moved;
        protected int listStart;
        protected int thrown;
        protected int listViewItemOffset;
        protected bool showSelectedItemOnlyOnFocus = false;
        protected int highlightedIndex = -1;
        protected bool selectFollowsHighlight;
        protected eListStyle style;

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
            if (index < -1)
                index = -1;
            Select(index, true, this.containingScreen());
        }
        void raiseSelect(object state)
        {
            SelectedIndexChanged(this, (int)state);
        }
        public void Select(int index, bool moveToSelectedItem, int screen)
        {
            if ((index < -1) || (index >= items.Count))
                return;
            if ((selectedIndex >= 0)&&(selectedIndex<items.Count))
                items[selectedIndex].subitemTex = items[selectedIndex].textTex = null;
            if (index>=0)
                items[index].textTex = items[index].subitemTex = null;
            selectedIndex = index;
            if (!selectFollowsHighlight)    // this is already done in the highlight function if selectFollowsHighilight is true
                highlightedIndex = selectedIndex;

            // Trigger event
            if (SelectedIndexChanged != null)
                ThreadPool.QueueUserWorkItem(new WaitCallback(raiseSelect),screen);
            //   ^ This still isn't ideal...we really should not be queuing all those tasks..only the one when scrolling finishes

            if ((listHeight == 0) && (index != -1))
            {
                selectQueued = true;
                return;
            }
            if ((moveToSelectedItem) && (index >= 0))
            {
                if (!((listStart <= selectedIndex) && (selectedIndex <= listStart - (Height / listHeight))))
                {
                    moved = -(selectedIndex * listHeight);
                }
            }
            selectQueued = false;
            raiseUpdate(false);
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
                return listHeight;
            }
            set
            {
                listHeight = value;
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
            moved = 0;
            Select(-1);
        }
        /// <summary>
        /// List Start
        /// </summary>
        [Browsable(false)]
        public int Start
        {
            get
            {
                return listStart;
            }
        }

        private int targetWidth;
        private int targetHeight;
        public int TargetWidth
        {
            set { targetWidth = value; }
        }
        public int TargetHeight
        {
            set { targetHeight = value; }
        }
        /// <summary>
        /// Placeholder method
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
        public OMListItem this[int index]
        {
            get
            {
                if ((index < (items.Count)) && (index >= 0))
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
                Select(value);
            }
        }

        public OMListItem SelectedItem
        {

            get
            {
                return this[selectedIndex];
            }
        }

        public OMListItem HighlightedItem
        {

            get
            {
                return this[highlightedIndex];
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
                if (it != null)
                {
                    if (it.image == null)
                    {
                        it.image = item.image;
                        return false;
                    }
                    else
                        return false;
                }
                else
                {
                    items.Add(item);
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
        }
        /// <summary>
        /// Add a group of list items
        /// </summary>
        /// <param name="source"></param>
        public void AddRange(string[] source)
        {
            for (int i = 0; i < source.Length; i++)
                items.Add(new OMListItem(source[i]));
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
                if (items.Count < count)
                    raiseUpdate(false);
            }
        }
        /// <summary>
        /// Returns the given range of items
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<OMListItem> getRange(int index, int count)
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
                if (items.Count < count)
                    raiseUpdate(false);
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
        public OMList(int x, int y, int w, int h)
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
        private bool clickSelect;
        /// <summary>
        /// Click only fires when an already selected item is clicked
        /// </summary>
        public bool ClickToSelect
        {
            get
            {
                return clickSelect;
            }
            set
            {
                clickSelect = value;
            }
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
            raiseUpdate(false);
        }
        /// <summary>
        /// Gets or Sets the list style
        /// </summary>
        public eListStyle ListStyle
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
                if (style == eListStyle.MultiList)
                    textAlignment = OpenMobile.Graphics.Alignment.TopLeft;
            }
        }
        /// <summary>
        /// The first background color of a non-selected item
        /// </summary>
        public Color ItemColor1
        {
            get { return itemColor1; }
            set { itemColor1 = value; }

        }
        /// <summary>
        /// The second background color of a non-selected item
        /// </summary>
        public Color ItemColor2
        {
            get { return itemColor2; }
            set { itemColor2 = value; }
        }
        /// <summary>
        /// The Text Color for highlighted items
        /// </summary>
        public Color HighlightColor
        {
            get { return highlightColor; }
            set { highlightColor = value; }
        }
        /// <summary>
        /// The Text Outline/Effect Color for highlighted items
        /// </summary>
        public Color HighlightedOutlineColor
        {
            get { return highlightColorOutline; }
            set { highlightColorOutline = value; }
        }
        OpenMobile.Graphics.eTextFormat highlightedItemFormat;
        /// <summary>
        /// The highlighted item text format
        /// </summary>
        public OpenMobile.Graphics.eTextFormat HighlightedItemFormat
        {
            get { return highlightedItemFormat; }
            set { highlightedItemFormat = value; }
        }
        /// <summary>
        /// The first background color of selected items
        /// </summary>
        public Color SelectedItemColor1
        {
            get { return selectedItemColor1; }
            set { selectedItemColor1 = value; }

        }
        /// <summary>
        /// The second background color of selected items
        /// </summary>
        public Color SelectedItemColor2
        {
            get { return selectedItemColor2; }
            set { selectedItemColor2 = value; }
        }
        private bool scrollbars;
        public bool Scrollbars
        {
            get { return scrollbars; }
            set { scrollbars = value; }
        }
        int count;
        /// <summary>
        /// Draws the control
        /// </summary>
        /// <param name="g">The UI's graphics object</param>
        /// <param name="e">Rendering Parameters</param>
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            lock (g)
            {
                if ((width == 0)||(height==0))
                    return;
                float tmp = 1;
                if (this.Mode == eModeType.transitioningIn)
                    tmp = e.globalTransitionIn;
                else if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                    tmp = e.globalTransitionOut;
                Rectangle r = g.Clip; //Save the drawing size
                g.Clip=this.toRegion(); //But only draw out control
                if (background != Color.Transparent)
                    g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * background.A), background)),Left + 1, Top + 1, Width - 2, Height - 2);
                
                int minListHeight = (int)(Graphics.Graphics.MeasureString("A", Font).Height + 0.5); //Round up
                if ((style == eListStyle.MultiList) && (items.Count > 0)&&(items[0].subitemFormat!=null))
                    minListHeight += (int)(Graphics.Graphics.MeasureString("A", items[0].subitemFormat.font).Height + 0.5);
                if (listHeight < minListHeight)
                    listHeight = minListHeight;
                if (selectQueued == true)
                    Select(selectedIndex);

                if (((int)style & 1) == 1) //Is it an image list
                {
                    if (listViewItemOffset == 0)
                        listViewItemOffset = listHeight;
                    else
                        listHeight = (listHeight > listViewItemOffset) ? listHeight : listViewItemOffset;
                }
                count = (this.Height / listHeight);
                int imgSze = 4;
                if ((moved > 0) || (items.Count * listHeight < Height)) //List start below top
                {
                    moved = 0;
                    thrown = 0;
                }
                else if (((items.Count * listHeight) > Height) && (moved < Height - (items.Count * listHeight))) //Top of the list
                {
                    moved = Height - (items.Count * listHeight);
                    thrown = 0;
                }
                listStart = -(moved / listHeight);


                for (int i = listStart; i <= (count + listStart + 1); i++)
                {
                    if ((width == 0)||(height==0)||(listHeight==0)) //Failsafe -> 1/100 chance that the width changes during rendering
                        return;
                    Rectangle rect = new Rectangle(Left, Top + (moved % listHeight) + ((i - listStart) * listHeight), this.Width, listHeight);
                    switch (style)
                    {
                        case eListStyle.RoundedImageList:
                        case eListStyle.RoundedTextList:
                            if ((selectedIndex == i)&&(focused))
                                g.FillRoundRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1), Color.FromArgb((int)(tmp * selectedItemColor2.A), selectedItemColor2), Gradient.Vertical), rect, 10);
                            else
                                g.FillRoundRectangle(new Brush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1), Color.FromArgb((int)(tmp * itemColor2.A), itemColor2), Gradient.Vertical), rect, 10);
                            break;
                        case eListStyle.TextList:
                        case eListStyle.ImageList:
                            if ((selectedIndex == i) && (focused))
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1), Color.FromArgb((int)(tmp * selectedItemColor2.A), selectedItemColor2), Gradient.Vertical), rect);
                            else
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1), Color.FromArgb((int)(tmp * itemColor2.A), itemColor2), Gradient.Vertical), rect);
                            break;
                        case eListStyle.TransparentImageList:
                        case eListStyle.TransparentTextList:
                            if ((selectedIndex == i) && (focused))
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A),selectedItemColor1)), rect);
                            break;
                        case eListStyle.DroidStyleImage:
                        case eListStyle.DroidStyleText:
                            imgSze = 6;
                            if ((selectedIndex == i) && (focused))
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A * 0.6), selectedItemColor1)), rect.Left, rect.Top, rect.Width, rect.Height - 2);
                            else
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1)), rect.Left, rect.Top, rect.Width, rect.Height - 2);
                            break;
                        case eListStyle.MultiList:
                            if (i > 0)
                            {
                                int t = ((rect.Top % 2) == 0) ? 1 : 0;
                                g.DrawLine(new Pen(Color.FromArgb((int)(tmp*background.A), background), 2F), rect.Left, rect.Top-t, rect.Left + rect.Width, rect.Top-t);
                            }
                            if ((selectedIndex == i) && (focused))
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1)), rect.Left, rect.Top, rect.Width, rect.Height);
                            else
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1)),rect.Left, rect.Top, rect.Width, rect.Height);
                            break;
                    }
                    if ((i < items.Count) && (i >= 0))
                    {
                        using (System.Drawing.StringFormat f = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoWrap))
                        {
                            if (ListStyle == eListStyle.MultiList)
                            {
                                if (items[i].subitemFormat != null)
                                {
                                    if ((selectedIndex == i) && (focused))
                                    {
                                        if (items[i].textTex == null)
                                            items[i].textTex = g.GenerateTextTexture(0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].text, this.Font, this.textFormat, this.textAlignment, highlightColor, highlightColor);
                                        g.DrawImage(items[i].textTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                        if (items[i].subitemTex == null)
                                            items[i].subitemTex = g.GenerateTextTexture((rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, items[i].subItem, items[i].subitemFormat.font, items[i].subitemFormat.textFormat, items[i].subitemFormat.textAlignment, items[i].subitemFormat.highlightColor, items[i].subitemFormat.highlightColor);
                                        g.DrawImage(items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                    }
                                    else
                                    {
                                        if (items[i].textTex == null)
                                            items[i].textTex = g.GenerateTextTexture(0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].text, this.Font, this.textFormat, this.textAlignment, color, color);
                                        g.DrawImage(items[i].textTex, (int)(rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                        if (items[i].subitemTex == null)
                                            items[i].subitemTex = g.GenerateTextTexture((rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, items[i].subItem, items[i].subitemFormat.font, items[i].subitemFormat.textFormat, items[i].subitemFormat.textAlignment, items[i].subitemFormat.color, items[i].subitemFormat.color);
                                        g.DrawImage(items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                    }
                                }
                                else
                                {
                                    if ((i < items.Count) && (items[i].textTex == null))
                                    {
                                        if (targetWidth == 0)
                                            targetWidth = width;
                                        if (targetHeight == 0)
                                            targetHeight = listHeight;
                                        if ((selectedIndex == i) && (focused))
                                            items[i].textTex = g.GenerateTextTexture(0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, font, textFormat, textAlignment, highlightColor, highlightColor);
                                        else
                                            items[i].textTex = g.GenerateTextTexture(0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, font, textFormat, textAlignment, color, color);
                                    }
                                    g.DrawImage(items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height, tmp);
                                }
                            }
                            else
                            {
                                if ((i<items.Count)&&(items[i].textTex == null))
                                {
                                    if (targetWidth == 0)
                                        targetWidth = width;
                                    if (targetHeight == 0)
                                        targetHeight = listHeight;
                                    if ((selectedIndex == i) && (focused))
                                        items[i].textTex = g.GenerateTextTexture(0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, font, textFormat, textAlignment, highlightColor, highlightColor);
                                    else
                                        items[i].textTex = g.GenerateTextTexture(0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, font, textFormat, textAlignment, color, color);
                                }
                                g.DrawImage(items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height,tmp);
                            }
                        }
                        if (listViewItemOffset == 0)
                            continue;
                        if ((items.Count > i) && (items[i].image != null)) //rare thread collision
                            g.DrawImage(items[i].image, rect.Left + 5, rect.Top + 2, rect.Height-5, rect.Height - imgSze, tmp);
                    }
                }
                g.Clip = r; //Reset the clip size for the rest of the controls
                if ((scrollbars)&&(count<items.Count))
                {
                    int nheight = (int)(height * ((float)(height / listHeight) / items.Count));
                    //TODO - Actually calculate ntop correctly
                    int ntop = Top - (int)((moved*((float)items.Count-count+1)/items.Count));
                    g.FillRoundRectangle(new Brush(color), left + width - 3, ntop, 6, nheight, 3);
                }
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
            if (clickSelect == true)
                if ((selectedIndex != lastSelected) || (mode == eModeType.Scrolling))
                    return;
            if (selectedIndex == -1)
                return;
            if (OnClick != null)
                OnClick(this, screen);
        }
        /// <summary>
        /// Raise the double click event
        /// </summary>
        /// <param name="screen"></param>
        public void doubleClickMe(int screen)
        {
            if (clickSelect == true)
                if ((selectedIndex != lastSelected) || (mode == eModeType.Scrolling))
                    return;
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
                OnLongClick(this, screen);
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
        public virtual bool KeyDown(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, float WidthScale, float HeightScale)
        {
            if (e.Key == Key.PageUp)
            {
                Select(SelectedIndex - 1, true, screen);
                return true;
            }
            if (e.Key == Key.PageDown)
            {
                Select(SelectedIndex + 1, true, screen);
                return true;
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
        public virtual bool KeyUp(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, float WidthScale, float HeightScale)
        {
            return false;
        }
        #endregion

        #region IThrow Members

        void IThrow.MouseThrowStart(int screen, Point StartLocation, PointF scaleFactors, ref bool Cancel)
        {
            thrown = 0;
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
                thrown = RelativeDistance.Y;
            moved += RelativeDistance.Y;
            if (Math.Abs(TotalDistance.Y) > 3)
            {
                if (selectedIndex >= 0)
                    items[selectedIndex].textTex = items[selectedIndex].subitemTex = null;
                selectedIndex = -1;
                raiseUpdate(false);
            }
        }

        #endregion

        #region IMouse Members

        public virtual void MouseMove(int screen, MouseMoveEventArgs e, float WidthScale, float HeightScale)
        {
            if (listHeight > 0) //<-Just in case
                Highlight((((int)(e.Y / HeightScale) - top - (moved % listHeight)) / listHeight) + listStart);
        }

        /// <summary>
        /// The offset in Pixels of each list item (X direction offset relative to the left edge of the list control)
        /// </summary>
        public int ListItemOffset
        {
            get
            {
                return listViewItemOffset;
            }
            set
            {
                listViewItemOffset = value;
            }
        }
        /// <summary>
        /// Selected item follows highlighted item
        /// </summary>
        public bool SelectFollowsHighlight
        {
            get
            {
                return selectFollowsHighlight;
            }
            set
            {
                selectFollowsHighlight = value;
            }
        }

        /// <summary>
        /// Returns the index of the item under the mouse
        /// </summary>
        [Category("List"), Description("Returns the index of the item under the mouse")]
        public int HighlightedIndex
        {
            get
            {
                return highlightedIndex;
            }
        }

        private void Highlight(int index)
        {
            // Deselect if index is out of range
            if ((index < 0) || (index >= items.Count))
                index = -1;

            // Only trigg if this is actually a new item
            if (index == highlightedIndex)
                return;
            highlightedIndex = index;
            if (selectFollowsHighlight)
                selectedIndex = highlightedIndex;

            // Throw event
            if (HighlightedIndexChanged != null)
                new Thread(delegate() { HighlightedIndexChanged(this, this.containingScreen()); }).Start();
        }
        private int lastSelected = -1;
        public virtual void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            throwtmr.Enabled = false;
            lastSelected = selectedIndex;
            focused = true;
            Select(highlightedIndex, false, screen);
            if (selectedIndex == lastSelected)
                return;
            if (clickSelect)
                mode = eModeType.Scrolling;
        }

        public virtual void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, float WidthScale, float HeightScale)
        {
            //
        }

        #endregion

        #region IKeyboard Members
        bool focused=true;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void KeyboardEnter(int screen)
        {
            focused = true;
            if (selectedIndex>-1)
                items[selectedIndex].textTex = null;
            if (selectedIndex == -1)
                Select(0,false,screen);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void KeyboardExit(int screen)
        {
            focused = false;
            if (selectedIndex > -1)
            {
                items[selectedIndex].textTex = null;
                items[selectedIndex].subitemTex = null;
            }
        }

        #endregion
    }
}
