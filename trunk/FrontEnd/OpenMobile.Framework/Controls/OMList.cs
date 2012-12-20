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
using System.ComponentModel;
using System.Timers;
using OpenMobile.Graphics;
using OpenMobile.Input;
using OpenMobile.Threading;
using OpenMobile.helperFunctions.Graphics;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A listbox control
    /// </summary>
    [System.Serializable]
    public class OMList : OMLabel, IClickable, IHighlightable, IKey, IList, IThrow, IMouse, IKeyboard
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
        /// <summary>
        /// Occurs when the highlighted item changes
        /// </summary>
        public event IndexChangedDelegate HighlightedIndexChanged;
        /// <summary>
        /// The index of the selected item
        /// </summary>
        protected int selectedIndex = -1;
        /// <summary>
        /// the internal list of items
        /// </summary>
        protected List<OMListItem> items;
        
        [System.NonSerialized]
        System.Timers.Timer throwtmr;
        /// <summary>
        /// Item background color
        /// </summary>
        protected Color itemColor1 = Color.DarkSlateGray;
        /// <summary>
        /// item background color 2 (for gradiants)
        /// </summary>
        protected Color itemColor2 = Color.Black;
        /// <summary>
        /// text highlighted color
        /// </summary>
        protected Color highlightColor = Color.Black;
        /// <summary>
        /// text highlighted color outline
        /// </summary>
        protected Color highlightColorOutline = Color.White;
        /// <summary>
        /// selected item background color
        /// </summary>
        protected Color selectedItemColor1 = Color.DarkBlue;
        /// <summary>
        /// selected item background color 2 (for gradiants)
        /// </summary>
        protected Color selectedItemColor2 = Color.Gray;
        /// <summary>
        /// background color
        /// </summary>
        protected Color background = Color.Transparent;
        private bool selectQueued;
        /// <summary>
        /// height of the list (OM units)
        /// </summary>
        public int listItemHeight;
        /// <summary>
        /// list vertical ofset
        /// </summary>
        protected int moved;
        /// <summary>
        /// Start of the list (OM units)
        /// </summary>
        protected int listStart;
        /// <summary>
        /// list acceleration
        /// </summary>
        protected int thrown;
        /// <summary>
        /// text ofset
        /// </summary>
        protected int listViewItemOffset;
        /// <summary>
        /// Used for keyboard navigation
        /// </summary>
        protected bool showSelectedItemOnlyOnFocus;
        /// <summary>
        /// Index of the currently highlighted item
        /// </summary>
        protected int highlightedIndex = -1;
        /// <summary>
        /// list selection follows highlighted list item
        /// </summary>
        protected bool selectFollowsHighlight;
        /// <summary>
        /// list style
        /// </summary>
        protected eListStyle style;

        /// <summary>
        /// List items
        /// </summary>
        public List<OMListItem> Items
        {
            get
            {
                return items;
            }
            set
            {
                Clear();
                items = value;
            }
        }

        /// <summary>
        /// Enables a fading effect from the listbackground to the list
        /// </summary>
        public bool UseSoftEdges { get; set; }
        /// <summary>
        /// Data to use for softedge
        /// </summary>
        public FadingEdge.GraphicData SoftEdgeData { get; set; }
        /// <summary>
        /// SoftEdge image for the sides
        /// </summary>
        private OImage imgSoftEdgeSides = null;
        /// <summary>
        /// SoftEdge image for the top
        /// </summary>
        private OImage imgSoftEdgeTop = null;
        /// <summary>
        /// SoftEdge image for the bottom
        /// </summary>
        private OImage imgSoftEdgeBottom = null;
        
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
        /// <para>Use -1 to clear current selection</para>
        /// </summary>
        /// <param name="index">List index to select</param>
        public void Select(int index)
        {
            if (index < -1)
                index = -1;
            Select(index, true, this.containingScreen());
        }
        /// <summary>
        /// Select an item in the list
        /// </summary>
        /// <param name="index">List index to select</param>
        /// <para>Use -1 to clear current selection</para>
        /// <param name="moveToSelectedItem">Scroll list so selected item is visible</param>
        /// <param name="screen"></param>
        public void Select(int index, bool moveToSelectedItem, int screen)
        {
            lock (this)
            {
                if ((index < -1) || (index >= items.Count))
                    return;
                if ((selectedIndex >= 0) && (selectedIndex < items.Count))
                    //items[selectedIndex].textTex = null;
                    items[selectedIndex].RefreshGraphic = true;
                if (index >= 0)
                    //items[index].textTex = null;
                    items[index].RefreshGraphic = true;

                selectedIndex = index;
                if (!selectFollowsHighlight)    // this is already done in the highlight function if selectFollowsHighilight is true
                    highlightedIndex = selectedIndex;

                // Trigger event
                if (SelectedIndexChanged != null)
                    SafeThread.Asynchronous(delegate() { if (SelectedIndexChanged != null) SelectedIndexChanged(this, screen); }, null);
                //   ^ This still isn't ideal...we really should not be queuing all those tasks..only the one when scrolling finishes

                if ((listItemHeight == 0) && (index != -1))
                {
                    selectQueued = true;
                    return;
                }
                if ((moveToSelectedItem) && (index >= 0))
                {
                    if (!((listStart <= selectedIndex) && (selectedIndex <= listStart - (Height / listItemHeight))))
                    {
                        moved = -(selectedIndex * listItemHeight);
                    }
                }
                selectQueued = false;
                raiseUpdate(false);
            }
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
        /// <summary>
        /// Reserved
        /// </summary>
        public int TargetWidth
        {
            set { targetWidth = value; }
        }
        /// <summary>
        /// Reserved
        /// </summary>
        public int TargetHeight
        {
            set { targetHeight = value; }
        }
        /// <summary>
        /// Placeholder method
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string Text
        {
            get
            {
                return String.Empty;
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
        /// <summary>
        /// The zero based index of the currently selected item (-1 for none)
        /// </summary>
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
        /// <summary>
        /// The currently selected item (null for none)
        /// </summary>
        public OMListItem SelectedItem
        {

            get
            {
                if (selectedIndex < 0)
                    return null;
                return this[selectedIndex];
            }
        }
        /// <summary>
        /// The currently highlighted item (null for none)
        /// </summary>
        public OMListItem HighlightedItem
        {

            get
            {
                if (highlightedIndex < 0)
                    return null;
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
            lock (this)
            {
                for (int i = 0; i < source.Count; i++)
                    items.Add(new OMListItem(source[i]));
            }
        }
        /// <summary>
        /// Add a group of list items
        /// </summary>
        /// <param name="source"></param>
        public void AddRange(string[] source)
        {
            lock (this)
            {
            for (int i = 0; i < source.Length; i++)
                items.Add(new OMListItem(source[i]));
            }
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
            : base("", 0, 0, 300, 300)
        {
            UseSoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
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
            : base("", x, y, w, h)
        {
            UseSoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
            declare();
        }
        /// <summary>
        /// A listview control
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="x">Left</param>
        /// <param name="y">Top</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public OMList(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            UseSoftEdges = false;
            SoftEdgeData = new FadingEdge.GraphicData();
            declare();
        }

        private void declare()
        {
            // Set default selection color (this is set to a darker shade of the current focus color)
            Color tmpColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            selectedItemColor1 = Color.FromArgb(tmpColor.A,
                (tmpColor.R == 255 ? tmpColor.R - 139 : tmpColor.R),
                (tmpColor.G == 255 ? tmpColor.G - 139 : tmpColor.G),
                (tmpColor.B == 255 ? tmpColor.B - 139 : tmpColor.B));

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
                if ((style == eListStyle.MultiList) || (style == eListStyle.MultiListText))
                    _textAlignment = OpenMobile.Graphics.Alignment.TopLeft;
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
        private bool scrollbars = true;
        /// <summary>
        /// Display scrollbars
        /// </summary>
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
            base.RenderBegin(g, e);

            lock (this)
            {
                try
                {
                    if ((width == 0) || (height == 0))
                        return;

                    //float tmp = OpacityFloat;
                    //if (this.Mode == eModeType.transitioningIn)
                    //    tmp = e.globalTransitionIn;
                    //else if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                    //    tmp = e.globalTransitionOut;
                    Rectangle r = g.Clip; //Save the drawing size
                    g.Clip = this.toRegion(); //But only draw out control
                    if (background != Color.Transparent)
                        g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * background.A), background)), Left + 1, Top + 1, Width - 2, Height - 2);

                    int minListHeight = (int)(Graphics.Graphics.MeasureString("A", Font).Height + 0.5); //Round up
                    if (((style == eListStyle.MultiList) || (style == eListStyle.MultiListText)) && (items.Count > 0) && (items[0].subitemFormat != null))
                        minListHeight += (int)(Graphics.Graphics.MeasureString("A", items[0].subitemFormat.font).Height + 0.5);
                    if (listItemHeight < minListHeight)
                        listItemHeight = minListHeight;
                    if (selectQueued == true)
                        Select(selectedIndex);

                    if (((int)style & 1) == 1) //Is it an image list
                    {
                        if (listViewItemOffset == 0)
                            listViewItemOffset = listItemHeight;
                        else
                            listItemHeight = (listItemHeight > listViewItemOffset) ? listItemHeight : listViewItemOffset;
                    }
                    count = (this.Height / listItemHeight);
                    int imgSze = 4;
                    if ((moved > 0) || (items.Count * listItemHeight < Height)) //List start below top
                    {
                        moved = 0;
                        thrown = 0;
                    }
                    else if (((items.Count * listItemHeight) > Height) && (moved < Height - (items.Count * listItemHeight))) //Top of the list
                    {
                        moved = Height - (items.Count * listItemHeight);
                        thrown = 0;
                    }
                    listStart = -(moved / listItemHeight);

                    #region SoftEdge

                    bool SoftEdgeTop = false;
                    bool SoftEdgeBottom = false;

                    // Use soft edges?
                    if (UseSoftEdges)
                    {
                        if (Background != Color.Transparent)
                        {
                            if (moved < 0)
                                SoftEdgeTop = true;

                            if (moved > Height - (items.Count * listItemHeight))
                                SoftEdgeBottom = true;

                            if (imgSoftEdgeSides == null || imgSoftEdgeSides.Width != Width || imgSoftEdgeSides.Height != Height)
                            {   // Generate image
                                FadingEdge.GraphicData gd = SoftEdgeData.Clone();
                                if ((gd.Sides & FadingEdge.GraphicSides.Top) == FadingEdge.GraphicSides.Top)
                                    gd.Sides -= FadingEdge.GraphicSides.Top;
                                if ((gd.Sides & FadingEdge.GraphicSides.Bottom) == FadingEdge.GraphicSides.Bottom)
                                    gd.Sides -= FadingEdge.GraphicSides.Bottom;
                                if (gd.Sides != FadingEdge.GraphicSides.None)
                                {
                                    gd.Width = Width;
                                    gd.Height = Height;
                                    imgSoftEdgeSides = FadingEdge.GetImage(gd);
                                }
                            }
                            if (imgSoftEdgeTop == null || imgSoftEdgeTop.Width != Width || imgSoftEdgeTop.Height != Height)
                            {   // Generate image
                                FadingEdge.GraphicData gd = SoftEdgeData.Clone();
                                if ((gd.Sides & FadingEdge.GraphicSides.Left) == FadingEdge.GraphicSides.Left)
                                    gd.Sides -= FadingEdge.GraphicSides.Left;
                                if ((gd.Sides & FadingEdge.GraphicSides.Right) == FadingEdge.GraphicSides.Right)
                                    gd.Sides -= FadingEdge.GraphicSides.Right;
                                if ((gd.Sides & FadingEdge.GraphicSides.Bottom) == FadingEdge.GraphicSides.Bottom)
                                    gd.Sides -= FadingEdge.GraphicSides.Bottom;
                                if (gd.Sides != FadingEdge.GraphicSides.None)
                                {
                                    gd.Width = Width;
                                    gd.Height = Height;
                                    imgSoftEdgeTop = FadingEdge.GetImage(gd);
                                }
                            }
                            if (imgSoftEdgeBottom == null || imgSoftEdgeBottom.Width != Width || imgSoftEdgeBottom.Height != Height)
                            {   // Generate image
                                FadingEdge.GraphicData gd = SoftEdgeData.Clone();
                                if ((gd.Sides & FadingEdge.GraphicSides.Left) == FadingEdge.GraphicSides.Left)
                                    gd.Sides -= FadingEdge.GraphicSides.Left;
                                if ((gd.Sides & FadingEdge.GraphicSides.Right) == FadingEdge.GraphicSides.Right)
                                    gd.Sides -= FadingEdge.GraphicSides.Right;
                                if ((gd.Sides & FadingEdge.GraphicSides.Top) == FadingEdge.GraphicSides.Top)
                                    gd.Sides -= FadingEdge.GraphicSides.Top;
                                if (gd.Sides != FadingEdge.GraphicSides.None)
                                {
                                    gd.Width = Width;
                                    gd.Height = Height;
                                    imgSoftEdgeBottom = FadingEdge.GetImage(gd);
                                }
                            }

                        }
                    }

                    #endregion

                    for (int i = listStart; i <= (count + listStart + 1); i++)
                    {
                        if ((width == 0) || (height == 0) || (listItemHeight == 0)) //Failsafe -> 1/100 chance that the width changes during rendering
                            return;
                        Rectangle rect = new Rectangle(Left, Top + (moved % listItemHeight) + ((i - listStart) * listItemHeight), this.Width, listItemHeight);
                        switch (style)
                        {
                            case eListStyle.RoundedImageList:
                            case eListStyle.RoundedTextList:
                                if ((selectedIndex == i) && (focused))
                                    g.FillRoundRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor1.A), selectedItemColor1), Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor2.A), selectedItemColor2), Gradient.Vertical), rect, 10);
                                else
                                    g.FillRoundRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * itemColor1.A), itemColor1), Color.FromArgb((int)(_RenderingValue_Alpha * itemColor2.A), itemColor2), Gradient.Vertical), rect, 10);
                                break;
                            case eListStyle.TextList:
                            case eListStyle.ImageList:
                                if ((selectedIndex == i) && (focused))
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor1.A), selectedItemColor1), Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor2.A), selectedItemColor2), Gradient.Vertical), rect);
                                else
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * itemColor1.A), itemColor1), Color.FromArgb((int)(_RenderingValue_Alpha * itemColor2.A), itemColor2), Gradient.Vertical), rect);
                                break;
                            case eListStyle.TransparentImageList:
                            case eListStyle.TransparentTextList:
                                if ((selectedIndex == i) && (focused))
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor1.A), selectedItemColor1)), rect);
                                break;
                            case eListStyle.DroidStyleImage:
                            case eListStyle.DroidStyleText:
                                imgSze = 6;
                                if ((selectedIndex == i) && (focused))
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor1.A * 0.6), selectedItemColor1)), rect.Left, rect.Top, rect.Width, rect.Height - 2);
                                else
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * itemColor1.A), itemColor1)), rect.Left, rect.Top, rect.Width, rect.Height - 2);
                                break;
                            case eListStyle.MultiList:
                            case eListStyle.MultiListText:
                                if (i > 0)
                                {
                                    int t = ((rect.Top % 2) == 0) ? 1 : 0;
                                    g.DrawLine(new Pen(Color.FromArgb((int)(_RenderingValue_Alpha * background.A), background), 0.5F), rect.Left, rect.Top - t, rect.Left + rect.Width, rect.Top - t);
                                }
                                if ((selectedIndex == i) && (focused))
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * selectedItemColor1.A), selectedItemColor1)), rect.Left, rect.Top, rect.Width, rect.Height);
                                else
                                    g.FillRectangle(new Brush(Color.FromArgb((int)(_RenderingValue_Alpha * itemColor1.A), itemColor1)), rect.Left, rect.Top, rect.Width, rect.Height);
                                break;
                        }
                        if ((i < items.Count) && (i >= 0))
                        {
                            using (System.Drawing.StringFormat f = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoWrap))
                            {
                                if ((ListStyle == eListStyle.MultiList) || (ListStyle == eListStyle.MultiListText))
                                {
                                    if (items[i].subitemFormat != null)
                                    {
                                        if ((selectedIndex == i) && (focused))
                                        {
                                            if (items[i].RefreshGraphic)
                                                items[i].textTex = g.GenerateTextTexture(items[i].textTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].text, this.Font, this._textFormat, this._textAlignment, highlightColor, highlightColor);
                                            g.DrawImage(items[i].textTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                            if (items[i].RefreshGraphic)
                                                items[i].subitemTex = g.GenerateTextTexture(items[i].subitemTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].subItem, items[i].subitemFormat.font, items[i].subitemFormat.textFormat, items[i].subitemFormat.textAlignment, items[i].subitemFormat.highlightColor, items[i].subitemFormat.highlightColor);
                                            g.DrawImage(items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                        }
                                        else
                                        {
                                            if (items[i].RefreshGraphic)
                                                items[i].textTex = g.GenerateTextTexture(items[i].textTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].text, this.Font, this._textFormat, this._textAlignment, _color, _color);
                                            g.DrawImage(items[i].textTex, (int)(rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                            if (items[i].RefreshGraphic)
                                                items[i].subitemTex = g.GenerateTextTexture(items[i].subitemTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].subItem, items[i].subitemFormat.font, items[i].subitemFormat.textFormat, items[i].subitemFormat.textAlignment, items[i].subitemFormat.color, items[i].subitemFormat.color);
                                            g.DrawImage(items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                        }
                                    }
                                    else
                                    {
                                        if ((i < items.Count) && (items[i].RefreshGraphic))
                                        {
                                            if (targetWidth == 0)
                                                targetWidth = width;
                                            if (targetHeight == 0)
                                                targetHeight = listItemHeight;
                                            if ((selectedIndex == i) && (focused))
                                                items[i].textTex = g.GenerateTextTexture(items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, _font, _textFormat, _textAlignment, highlightColor, highlightColor);
                                            else
                                                items[i].textTex = g.GenerateTextTexture(items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, _font, _textFormat, _textAlignment, _color, _color);
                                        }
                                        g.DrawImage(items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height, _RenderingValue_Alpha);
                                    }
                                }
                                else
                                {
                                    if ((i < items.Count) && (items[i].RefreshGraphic))
                                    {
                                        if (targetWidth == 0)
                                            targetWidth = width;
                                        if (targetHeight == 0)
                                            targetHeight = listItemHeight;
                                        if ((selectedIndex == i) && (focused))
                                            items[i].textTex = g.GenerateTextTexture(items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, _font, _textFormat, _textAlignment, highlightColor, highlightColor);
                                        else
                                            items[i].textTex = g.GenerateTextTexture(items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, _font, _textFormat, _textAlignment, _color, _color);
                                    }
                                    g.DrawImage(items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height, _RenderingValue_Alpha);
                                }
                            }
                            if (listViewItemOffset == 0)
                                continue;
                            if ((items.Count > i) && (items[i].RefreshGraphic)) //rare thread collision
                                g.DrawImage(items[i].image, rect.Left + 5, rect.Top + 2, rect.Height - 5, rect.Height - imgSze, _RenderingValue_Alpha);
                        }
                    }
                    g.Clip = r; //Reset the clip size for the rest of the controls

                    // Draw soft edges
                    if (UseSoftEdges)
                    {
                        // Draw sides
                        if (imgSoftEdgeSides != null)
                            g.DrawImage(imgSoftEdgeSides, Left, Top, imgSoftEdgeSides.Width, imgSoftEdgeSides.Height, _RenderingValue_Alpha);

                        // Draw top edge
                        if (SoftEdgeTop)
                            if (imgSoftEdgeTop != null)
                                g.DrawImage(imgSoftEdgeTop, Left, Top, imgSoftEdgeTop.Width, imgSoftEdgeTop.Height, _RenderingValue_Alpha);

                        // Draw bottom edge
                        if (SoftEdgeBottom)
                            if (imgSoftEdgeBottom != null)
                                g.DrawImage(imgSoftEdgeBottom, Left, Top, imgSoftEdgeBottom.Width, imgSoftEdgeBottom.Height, _RenderingValue_Alpha);
                    }

                    // Draw scrollbar
                    if ((scrollbars) && (count < items.Count))
                    {
                        float nheight = height * ((float)height) / (listItemHeight * items.Count);
                        float ntop = top + height * ((float)-moved) / (listItemHeight * items.Count);
                        g.FillRoundRectangle(new Brush(_color), left + width - 5, (int)ntop, 10, (int)nheight, 6);
                    }
                }
                catch 
                {
                    // No action 
                }
                
            }

            base.RenderFinish(g, e);
        }
        /*
        public override void Render(Graphics.Graphics g, renderingParams e)
        {
            lock (g)
            {
                if ((width == 0) || (height == 0))
                    return;
                float tmp = 1;
                if (this.Mode == eModeType.transitioningIn)
                    tmp = e.globalTransitionIn;
                else if ((this.Mode == eModeType.transitioningOut) || (this.Mode == eModeType.ClickedAndTransitioningOut))
                    tmp = e.globalTransitionOut;
                Rectangle r = g.Clip; //Save the drawing size
                g.Clip = this.toRegion(); //But only draw out control
                if (background != Color.Transparent)
                    g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * background.A), background)), Left + 1, Top + 1, Width - 2, Height - 2);

                int minListHeight = (int)(Graphics.Graphics.MeasureString("A", Font).Height + 0.5); //Round up
                if (((style == eListStyle.MultiList) || (style == eListStyle.MultiListText)) && (items.Count > 0) && (items[0].subitemFormat != null))
                    minListHeight += (int)(Graphics.Graphics.MeasureString("A", items[0].subitemFormat.font).Height + 0.5);
                if (listItemHeight < minListHeight)
                    listItemHeight = minListHeight;
                if (selectQueued == true)
                    Select(selectedIndex);

                if (((int)style & 1) == 1) //Is it an image list
                {
                    if (listViewItemOffset == 0)
                        listViewItemOffset = listItemHeight;
                    else
                        listItemHeight = (listItemHeight > listViewItemOffset) ? listItemHeight : listViewItemOffset;
                }
                count = (this.Height / listItemHeight);
                int imgSze = 4;
                if ((moved > 0) || (items.Count * listItemHeight < Height)) //List start below top
                {
                    moved = 0;
                    thrown = 0;
                }
                else if (((items.Count * listItemHeight) > Height) && (moved < Height - (items.Count * listItemHeight))) //Top of the list
                {
                    moved = Height - (items.Count * listItemHeight);
                    thrown = 0;
                }
                listStart = -(moved / listItemHeight);


                for (int i = listStart; i <= (count + listStart + 1); i++)
                {
                    if ((width == 0) || (height == 0) || (listItemHeight == 0)) //Failsafe -> 1/100 chance that the width changes during rendering
                        return;
                    Rectangle rect = new Rectangle(Left, Top + (moved % listItemHeight) + ((i - listStart) * listItemHeight), this.Width, listItemHeight);
                    switch (style)
                    {
                        case eListStyle.RoundedImageList:
                        case eListStyle.RoundedTextList:
                            if ((selectedIndex == i) && (focused))
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
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1)), rect);
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
                        case eListStyle.MultiListText:
                            if (i > 0)
                            {
                                int t = ((rect.Top % 2) == 0) ? 1 : 0;
                                g.DrawLine(new Pen(Color.FromArgb((int)(tmp * background.A), background), 2F), rect.Left, rect.Top - t, rect.Left + rect.Width, rect.Top - t);
                            }
                            if ((selectedIndex == i) && (focused))
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * selectedItemColor1.A), selectedItemColor1)), rect.Left, rect.Top, rect.Width, rect.Height);
                            else
                                g.FillRectangle(new Brush(Color.FromArgb((int)(tmp * itemColor1.A), itemColor1)), rect.Left, rect.Top, rect.Width, rect.Height);
                            break;
                    }
                    if ((i < items.Count) && (i >= 0))
                    {
                        using (System.Drawing.StringFormat f = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoWrap))
                        {
                            if ((ListStyle == eListStyle.MultiList) || (ListStyle == eListStyle.MultiListText))
                            {
                                if (items[i].subitemFormat != null)
                                {
                                    if ((selectedIndex == i) && (focused))
                                    {
                                        if (items[i].textTex == null)
                                            items[i].textTex = g.GenerateTextTexture(0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].text, this.Font, this.textFormat, this.textAlignment, highlightColor, highlightColor);
                                        g.DrawImage(items[i].textTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                        if (items[i].subitemTex == null)
                                            items[i].subitemTex = g.GenerateTextTexture(0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].subItem, items[i].subitemFormat.font, items[i].subitemFormat.textFormat, items[i].subitemFormat.textAlignment, items[i].subitemFormat.highlightColor, items[i].subitemFormat.highlightColor);
                                        g.DrawImage(items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                    }
                                    else
                                    {
                                        if (items[i].textTex == null)
                                            items[i].textTex = g.GenerateTextTexture(0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].text, this.Font, this.textFormat, this.textAlignment, color, color);
                                        g.DrawImage(items[i].textTex, (int)(rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, tmp);
                                        if (items[i].subitemTex == null)
                                            items[i].subitemTex = g.GenerateTextTexture(0, 0, (rect.Width - listViewItemOffset), rect.Height, items[i].subItem, items[i].subitemFormat.font, items[i].subitemFormat.textFormat, items[i].subitemFormat.textAlignment, items[i].subitemFormat.color, items[i].subitemFormat.color);
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
                                            targetHeight = listItemHeight;
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
                                if ((i < items.Count) && (items[i].textTex == null))
                                {
                                    if (targetWidth == 0)
                                        targetWidth = width;
                                    if (targetHeight == 0)
                                        targetHeight = listItemHeight;
                                    if ((selectedIndex == i) && (focused))
                                        items[i].textTex = g.GenerateTextTexture(0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, font, textFormat, textAlignment, highlightColor, highlightColor);
                                    else
                                        items[i].textTex = g.GenerateTextTexture(0, 0, (targetWidth - listViewItemOffset), targetHeight, items[i].text, font, textFormat, textAlignment, color, color);
                                }
                                g.DrawImage(items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height, tmp);
                            }
                        }
                        if (listViewItemOffset == 0)
                            continue;
                        if ((items.Count > i) && (items[i].image != null)) //rare thread collision
                            g.DrawImage(items[i].image, rect.Left + 5, rect.Top + 2, rect.Height - 5, rect.Height - imgSze, tmp);
                    }
                }
                g.Clip = r; //Reset the clip size for the rest of the controls
                if ((scrollbars) && (count < items.Count))
                {
                    float nheight = height * ((float)height) / (listItemHeight * items.Count);
                    float ntop = top + height * ((float)-moved) / (listItemHeight * items.Count);
                    g.FillRoundRectangle(new Brush(color), left + width - 5, (int)ntop, 10, (int)nheight, 6);
                }
            }
        }
        */

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
        /// Occurs when the control is clicked
        /// </summary>
        public event userInteraction OnClick;
        /// <summary>
        /// Occurs when the control is held
        /// </summary>
        public event userInteraction OnHoldClick;
        /// <summary>
        /// Occurs when the control is long clicked
        /// </summary>
        public event userInteraction OnLongClick;
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
            raiseUpdate(false);
        }

        /// <summary>
        /// Hold the control 
        /// </summary>
        public void holdClickMe(int screen)
        {
            if (OnHoldClick != null)
                OnHoldClick(this, screen);
            raiseUpdate(false);
        }

        /// <summary>
        /// Raise the long click event
        /// </summary>
        /// <param name="screen"></param>
        public void longClickMe(int screen)
        {
            if (OnLongClick != null)
                OnLongClick(this, screen);
            raiseUpdate(false);
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
        public virtual bool KeyDown_BeforeUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors)
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
        public virtual void KeyDown_AfterUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors)
        {
        }
        /// <summary>
        /// Occurs when a Key is pressed while the control is highlighted
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        /// <returns></returns>
        public virtual bool KeyUp_BeforeUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors)
        {
            return false;
        }
        public virtual void KeyUp_AfterUI(int screen, OpenMobile.Input.KeyboardKeyEventArgs e, PointF scaleFactors)
        {
        }
        #endregion

        #region IThrow Members

        void IThrow.MouseThrowStart(int screen, Point StartLocation, PointF CursorSpeed, PointF scaleFactors, ref bool Cancel)
        {
            thrown = 0;
        }

        void IThrow.MouseThrowEnd(int screen, Point StartLocation, Point TotalDistance, Point EndLocation, PointF CursorSpeed)
        {
            if (thrown != 0)
                throwtmr.Enabled = true;
        }

        void IThrow.MouseThrow(int screen, Point StartLocation, Point TotalDistance, Point RelativeDistance, PointF CursorSpeed)
        {
            throwtmr.Enabled = false;
            thrown = 0;
            if (System.Math.Abs(RelativeDistance.Y) > 3)
                thrown = RelativeDistance.Y;
            moved += RelativeDistance.Y;
            if (System.Math.Abs(TotalDistance.Y) > 3)
            {
                if (selectedIndex >= 0)
                    items[selectedIndex].RefreshGraphic = true; 
                selectedIndex = -1;
                raiseUpdate(false);
            }
        }

        #endregion

        #region IMouse Members
        /// <summary>
        /// The mouse has been moved
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public virtual void MouseMove(int screen, MouseMoveEventArgs e, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            if (listItemHeight > 0) //<-Just in case
                //Highlight((((int)(e.Y / HeightScale) - top - (moved % listItemHeight)) / listItemHeight) + listStart);
                Highlight(((e.Y - top - (moved % listItemHeight)) / listItemHeight) + listStart);
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
        public int HighlightedIndex
        {
            get
            {
                return highlightedIndex;
            }
        }

        private void Highlight(int index)
        {
            lock (this)
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
                    SafeThread.Asynchronous(delegate() { HighlightedIndexChanged(this, this.containingScreen()); }, null);
            }
        }
        private int lastSelected = -1;
        /// <summary>
        /// The mouse has been pressed
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public virtual void MouseDown(int screen, OpenMobile.Input.MouseButtonEventArgs e, Point StartLocation)
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
        /// <summary>
        /// The mouse has been released
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="WidthScale"></param>
        /// <param name="HeightScale"></param>
        public virtual void MouseUp(int screen, OpenMobile.Input.MouseButtonEventArgs e, Point StartLocation, Point TotalDistance)
        {
            //
        }

        #endregion

        #region IKeyboard Members

        bool focused = true;
        /// <summary>
        /// Set keyboard focus
        /// </summary>
        /// <param name="screen"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void KeyboardEnter(int screen)
        {
            focused = true;
            if (selectedIndex > -1)
                items[selectedIndex].RefreshGraphic = true; 
            if (selectedIndex == -1)
                Select(0, false, screen);
        }
        /// <summary>
        /// Lose keyboard focus
        /// </summary>
        /// <param name="screen"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void KeyboardExit(int screen)
        {
            focused = false;
            if (selectedIndex > -1)
                items[selectedIndex].RefreshGraphic = true; 
        }

        #endregion
    }
}
