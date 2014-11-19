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
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.Input;
using OpenMobile.Threading;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A listbox control
    /// </summary>
    [System.Serializable]
    public class OMList : OMLabel, IClickable, IHighlightable, IKey, IList, IThrow, IMouse, IKeyboard
    {
        /// <summary>
        /// List scroll modes
        /// </summary>
        public enum Scrollmodes
        {
            /// <summary>
            /// Flick to throw the list
            /// </summary>
            FlickToThrow,

            /// <summary>
            /// Drag the list to scroll it's contents
            /// </summary>
            DragToScroll
        }

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
        protected List<OMListItem> _Items;
        
        [System.NonSerialized]
        Timer[] throwtmr;
        private float[] _Throw_Speed;
        private float[] _Throw_SpeedInitial;

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
        protected float thrown;
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
        protected bool _ThrowRun = true;
        protected SmoothAnimator Animation = new SmoothAnimator();
        protected Timer _tmrListScroll = new Timer(10);

        /// <summary>
        /// The current scroll mode of the list
        /// </summary>
        public Scrollmodes Scrollmode
        {
            get
            {
                return this._Scrollmode;
            }
            set
            {
                if (this._Scrollmode != value)
                {
                    this._Scrollmode = value;
                }
            }
        }
        private Scrollmodes _Scrollmode = Scrollmodes.FlickToThrow;        

        /// <summary>
        /// List items
        /// </summary>
        public List<OMListItem> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                Clear();
                _Items = value;
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
        /// The color of the scrollbar
        /// </summary>
        public Color ScrollbarColor
        {
            get
            {
                return this._ScrollbarColor;
            }
            set
            {
                if (this._ScrollbarColor != value)
                {
                    this._ScrollbarColor = value;
                }
            }
        }
        private Color _ScrollbarColor = Color.White;        

        /// <summary>
        /// The color of the separator
        /// </summary>
        public Color SeparatorColor
        {
            get
            {
                return this._SeparatorColor;
            }
            set
            {
                if (this._SeparatorColor != value)
                {
                    this._SeparatorColor = value;
                }
            }
        }
        private Color _SeparatorColor;

        /// <summary>
        /// The size of the separator in pixels
        /// </summary>
        public int SeparatorSize
        {
            get
            {
                return this._SeparatorSize;
            }
            set
            {
                if (this._SeparatorSize != value)
                {
                    this._SeparatorSize = value;
                }
            }
        }
        private int _SeparatorSize = 1;        

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
                if ((index < -1) || (index >= _Items.Count))
                    return;
                if ((selectedIndex >= 0) && (selectedIndex < _Items.Count))
                    //items[selectedIndex].textTex = null;
                    _Items[selectedIndex].RefreshGraphic = true;
                if (index >= 0)
                    //items[index].textTex = null;
                    _Items[index].RefreshGraphic = true;

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
            _Items.Sort();
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
            _Items.Clear();
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
                if ((index < (_Items.Count)) && (index >= 0))
                {
                    return _Items[index];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _Items[index] = value;
            }
        }

        /// <summary>
        /// Gets the list count
        /// </summary>
        public int Count
        {
            get
            {
                return _Items.Count;
            }
        }
        /// <summary>
        /// Gets or sets the zero based index of the currently selected item (-1 for none)
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
                base.Refresh();
            }
        }
        /// <summary>
        /// Gets the currently selected item (null for none)
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
        /// Gets the currently highlighted item (null for none)
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
                OMListItem it = _Items.Find(i => i.text == item.text);
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
                    _Items.Add(item);
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
            _Items.AddRange(source);
        }
        /// <summary>
        /// Returns the index of the given string
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int indexOf(string item)
        {
            return _Items.FindIndex(a => a.text == item);
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
                    _Items.Add(new OMListItem(source[i]));
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
                _Items.Add(new OMListItem(source[i]));
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
                _Items.Add(item);
                if (_Items.Count < count)
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
            return _Items.GetRange(index, count);
        }

        /// <summary>
        /// Add an item to the list
        /// </summary>
        /// <param name="item"></param>
        public void Add(string item)
        {
            lock (this)
            {
                _Items.Add(new OMListItem(item));
                if (_Items.Count < count)
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

        ~OMList()
        {
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                if (throwtmr[i] != null)
                    throwtmr[i].Dispose();
            }
        }

        private void declare()
        {
            //// Set default selection color (this is set to a darker shade of the current focus color)
            //Color tmpColor = BuiltInComponents.SystemSettings.SkinFocusColor;
            //selectedItemColor1 = Color.FromArgb(tmpColor.A,
            //    (tmpColor.R == 255 ? tmpColor.R - 139 : tmpColor.R),
            //    (tmpColor.G == 255 ? tmpColor.G - 139 : tmpColor.G),
            //    (tmpColor.B == 255 ? tmpColor.B - 139 : tmpColor.B));


            throwtmr = new Timer[OM.Host.ScreenCount];
            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                throwtmr[i] = new Timer(5);
                throwtmr[i].Screen = i;
                throwtmr[i].Elapsed += new ElapsedEventHandler(throwtmr_Elapsed);
            }
            _Throw_Speed = new float[OM.Host.ScreenCount];
            _Throw_SpeedInitial = new float[OM.Host.ScreenCount];

            _Items = new List<OMListItem>();
            _tmrListScroll = new Timer(1);
            _tmrListScroll.Elapsed += new ElapsedEventHandler(_tmrListScroll_Elapsed);
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
            Timer tmr = sender as Timer;

            tmr.Enabled = false;            

            // Initialize animation data
            if (tmr.Tag2 != null)
            {
                var distance = (int)tmr.Tag2;
                thrown = distance;

                _Throw_Speed[tmr.Screen] = (int)tmr.Tag * 10;
                _Throw_SpeedInitial[tmr.Screen] = _Throw_Speed[tmr.Screen];

                tmr.Tag2 = null;
                tmr.Tag = null;
            }

            if (thrown > 0)
            {
                if (thrown <= 0)
                {
                    tmr.Enabled = false;
                    Raise_OnScrollEnd();
                    return;
                }

                thrown -= 1f;
                moved += (int)_Throw_Speed[tmr.Screen];
            }
            else
            {
                if (thrown >= 0)
                {
                    tmr.Enabled = false;
                    Raise_OnScrollEnd();
                    return;
                }

                thrown += 1f;
                moved -= (int)_Throw_Speed[tmr.Screen];
            }

            //_Throw_Speed -= _Throw_Friction;
            if (System.Math.Abs(thrown) <= 100)
            {
                _Throw_Speed[tmr.Screen] = System.Math.Abs(_Throw_SpeedInitial[tmr.Screen] * (thrown / 100f));
            }

            Raise_OnScrolling(tmr.Screen);
            Refresh();
            tmr.Enabled = true;
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

        /// <summary>
        /// Have we scrolled to the bottom of the list
        /// </summary>
        public bool ScrolledToBottomOfList
        {
            get
            {
                return moved <= Height - (_Items.Count * listItemHeight);
            }
        }

        /// <summary>
        /// Have we scrolled to the top of the list
        /// </summary>
        public bool ScrolledToTopOfList
        {
            get
            {
                return moved >= 0;
            }
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

            lock (_Items)
            {
                try
                {
                    if ((width == 0) || (height == 0))
                        return;
                    if (_Items.Count == 0)
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
                    if (((style == eListStyle.MultiList) || (style == eListStyle.MultiListText)) && (_Items.Count > 0) && (_Items[0].subitemFormat != null))
                        minListHeight += (int)(Graphics.Graphics.MeasureString("A", _Items[0].subitemFormat.font).Height + 0.5);
                    if (listItemHeight < minListHeight)
                        listItemHeight = minListHeight;
                    if (selectQueued == true)
                        Select(selectedIndex);

                    if (((int)style & 1) == 1) //Is it an image list
                    {
                        if (listViewItemOffset == 0)
                            listViewItemOffset = listItemHeight;
                        //else
                        //    listItemHeight = (listItemHeight > listViewItemOffset) ? listItemHeight : listViewItemOffset;
                    }
                    count = (this.Height / listItemHeight);
                    int imgSze = 4;
                    if ((moved > 0) || (_Items.Count * listItemHeight < Height)) //List start below top
                    {
                        moved = 0;
                        thrown = 0;
                    }
                    else if (((_Items.Count * listItemHeight) > Height) && (moved < Height - (_Items.Count * listItemHeight))) //Top of the list
                    {
                        moved = Height - (_Items.Count * listItemHeight);
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

                            if (moved > Height - (_Items.Count * listItemHeight))
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
                        if ((i < _Items.Count) && (i >= 0))
                        {
                            using (System.Drawing.StringFormat f = new System.Drawing.StringFormat(System.Drawing.StringFormatFlags.NoWrap))
                            {
                                if ((ListStyle == eListStyle.MultiList) || (ListStyle == eListStyle.MultiListText))
                                {
                                    if (_Items[i].subitemFormat != null)
                                    {
                                        if ((selectedIndex == i) && (focused))
                                        {
                                            if (_Items[i].RefreshGraphic)
                                                _Items[i].textTex = g.GenerateTextTexture(_Items[i].textTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, _Items[i].text, this.Font, this._textFormat, this._textAlignment, highlightColor, highlightColor);
                                            g.DrawImage(_Items[i].textTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                            if (_Items[i].RefreshGraphic)
                                                _Items[i].subitemTex = g.GenerateTextTexture(_Items[i].subitemTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, _Items[i].subItem, _Items[i].subitemFormat.font, _Items[i].subitemFormat.textFormat, _Items[i].subitemFormat.textAlignment, _Items[i].subitemFormat.highlightColor, _Items[i].subitemFormat.highlightColor);
                                            g.DrawImage(_Items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                        }
                                        else
                                        {
                                            if (_Items[i].RefreshGraphic)
                                                _Items[i].textTex = g.GenerateTextTexture(_Items[i].textTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, _Items[i].text, this.Font, this._textFormat, this._textAlignment, _color, _color);
                                            g.DrawImage(_Items[i].textTex, (int)(rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                            if (_Items[i].RefreshGraphic)
                                                _Items[i].subitemTex = g.GenerateTextTexture(_Items[i].subitemTex, 0, 0, (rect.Width - listViewItemOffset), rect.Height, _Items[i].subItem, _Items[i].subitemFormat.font, _Items[i].subitemFormat.textFormat, _Items[i].subitemFormat.textAlignment, _Items[i].subitemFormat.color, _Items[i].subitemFormat.color);
                                            g.DrawImage(_Items[i].subitemTex, (rect.Left + listViewItemOffset), rect.Top, (rect.Width - listViewItemOffset), rect.Height, _RenderingValue_Alpha);
                                        }
                                    }
                                    else
                                    {
                                        if ((i < _Items.Count) && (_Items[i].RefreshGraphic))
                                        {
                                            if (targetWidth == 0)
                                                targetWidth = width;
                                            if (targetHeight == 0)
                                                targetHeight = listItemHeight;
                                            if ((selectedIndex == i) && (focused))
                                                _Items[i].textTex = g.GenerateTextTexture(_Items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, _Items[i].text, _font, _textFormat, _textAlignment, highlightColor, highlightColor);
                                            else
                                                _Items[i].textTex = g.GenerateTextTexture(_Items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, _Items[i].text, _font, _textFormat, _textAlignment, _color, _color);
                                        }
                                        g.DrawImage(_Items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height, _RenderingValue_Alpha);
                                    }
                                }
                                else
                                {
                                    if ((i < _Items.Count) && (_Items[i].RefreshGraphic))
                                    {
                                        if (targetWidth == 0)
                                            targetWidth = width;
                                        if (targetHeight == 0)
                                            targetHeight = listItemHeight;
                                        if ((selectedIndex == i) && (focused))
                                            _Items[i].textTex = g.GenerateTextTexture(_Items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, _Items[i].text, _font, _textFormat, _textAlignment, highlightColor, highlightColor);
                                        else
                                            _Items[i].textTex = g.GenerateTextTexture(_Items[i].textTex, 0, 0, (targetWidth - listViewItemOffset), targetHeight, _Items[i].text, _font, _textFormat, _textAlignment, _color, _color);
                                    }
                                    g.DrawImage(_Items[i].textTex, rect.Left + listViewItemOffset, rect.Top, rect.Width - listViewItemOffset, rect.Height, _RenderingValue_Alpha);
                                }
                            }
                            if (listViewItemOffset == 0)
                                continue;
                            if ((_Items.Count > i) && (_Items[i].RefreshGraphic)) //rare thread collision
                                g.DrawImage(_Items[i].image, rect.Left + 5, rect.Top + 2, rect.Height - 5, rect.Height - 5, _RenderingValue_Alpha);
                        }

                        //draw separator?
                        if (_SeparatorColor != Color.Transparent && !_SeparatorColor.IsEmpty && (i < _Items.Count - 1))
                            g.DrawLine(new Pen(Color.FromArgb((int)base.GetAlphaValue255(_SeparatorColor.A), _SeparatorColor), _SeparatorSize), new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));

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
                    if ((scrollbars) && (count < _Items.Count))
                    {
                        float nheight = height * ((float)height) / (listItemHeight * _Items.Count);
                        float ntop = top + height * ((float)-moved) / (listItemHeight * _Items.Count);
                        using (Brush b = new Brush(_ScrollbarColor))
                            g.FillRoundRectangle(b, left + width - 5, (int)ntop, 10, (int)nheight, 6);
                    }
                }
                catch (Exception ex)
                {
                    // No action 
                    System.Diagnostics.Debug.WriteLine(String.Format("OMList rendering exception: {1}", ex.Message));
                }
                
            }

            if (throwtmr[g.screen].Enabled)
                Refresh();

            base.RenderFinish(g, e);
        }

        #region ICloneable Members
        /// <summary>
        /// Provides a deep copy of this control
        /// </summary>
        /// <returns></returns>
        public override object Clone(OMPanel parent)
        {
            OMList ret = (OMList)this.MemberwiseClone();
            ret.parent = parent;
            ret.declare();
            if (this._Items.Count > 0)
                ret._Items.AddRange(this._Items.GetRange(0, this._Items.Count));
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
            throwtmr[screen].Enabled = false;
            switch (_Scrollmode)
            {
                case Scrollmodes.FlickToThrow:
                    thrown = 0;
                    break;
                case Scrollmodes.DragToScroll:
                    break;
                default:
                    break;
            }
        }

        void IThrow.MouseThrowEnd(int screen, Point StartLocation, Point TotalDistance, Point EndLocation, PointF CursorSpeed)
        {
            switch (_Scrollmode)
            {
                case Scrollmodes.FlickToThrow:
                    {
                        // Calculate distance and speed
                        var speed = System.Math.Abs(CursorSpeed.Y);
                        if (speed > 0.2f)
                        {
                            speed = (int)(System.Math.Ceiling(speed));
                            var distance = TotalDistance.Y * (int)speed;
                            throwtmr[screen].Tag = (int)speed; 
                            throwtmr[screen].Tag2 = distance;  

                            Raise_OnScrollStart();
                            throwtmr[screen].Enabled = true;
                        }
                    }
                    break;
                case Scrollmodes.DragToScroll:
                    break;
                default:
                    break;
            }
        }

        void IThrow.MouseThrow(int screen, Point StartLocation, Point TotalDistance, Point RelativeDistance, PointF CursorSpeed)
        {
            switch (_Scrollmode)
            {
                case Scrollmodes.FlickToThrow:
                    throwtmr[screen].Enabled = false;
                    thrown = 0;
                    //if (System.Math.Abs(RelativeDistance.Y) > 3)
                    //    thrown = RelativeDistance.Y;
                    moved += RelativeDistance.Y;
                    if (System.Math.Abs(TotalDistance.Y) > 3)
                    {
                        if (selectedIndex >= 0)
                            _Items[selectedIndex].RefreshGraphic = true; 
                        selectedIndex = -1;
                        raiseUpdate(false);
                    }
                    break;
                case Scrollmodes.DragToScroll:
                    break;
                default:
                    break;
            }

        }

        #endregion

        /// <summary>
        /// Gets the index of the centered visible item
        /// </summary>
        /// <returns></returns>
        public int GetCenterVisibleIndex()
        {
            if (count == 0)
                return 0;
            return listStart + (count / 2);
        }

        /// <summary>
        /// Gets the index of the topmost visible item
        /// </summary>
        /// <returns></returns>
        public int GetTopVisibleIndex()
        {
            if (count == 0)
                return 0;
            return listStart;
        }

        /// <summary>
        /// Gets the index of the bottommost visible item
        /// </summary>
        /// <returns></returns>
        public int GetBottomVisibleIndex()
        {
            if (count == 0)
                return 0;
            return listStart + count;
        }

        public void ScrollToIndex(int index, bool animate, float animationSpeed, float animationMinTime = 250f, float animationMaxTime = 1000f)
        {
            // Cancel any ongoing scrolling
            ScrollCancel(this.parent.containingScreen());

            Raise_OnScrollStart();

            if (animationSpeed == 0)
                animationSpeed = 1.0f;

            if (animate)
            {
                #region Animate scrolling

                lock (Animation)
                {
                    float StepSize = 1;
                    // Calculate distance from current location to control's location

                    int centerIndex = listStart + (count / 2);

                    int Distance_Start = (index - centerIndex) * listItemHeight;
                    int Distance_Current = Distance_Start;

                    // Calculate animation speed, this is based on the total distance to move but is limited to a max time and a min time
                    float maxTimeMS = animationMaxTime;
                    float minTimeMS = animationMinTime;
                    float TravelDistance = System.Math.Abs(Distance_Current);
                    float TravelSpeedMS = TravelDistance / animationSpeed;
                    if (TravelSpeedMS > maxTimeMS)
                        animationSpeed = TravelDistance / maxTimeMS;
                    if (TravelSpeedMS < minTimeMS)
                        animationSpeed = TravelDistance / minTimeMS;

                    Animation.Speed = 1f * animationSpeed;
                    float AnimationValue = 0;
                    _ThrowRun = true;

                    Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                    {
                        // Cancel animation
                        if (Distance_Current == 0 | !_ThrowRun)
                            return false;

                        // Calculate animation value
                        AnimationValue += AnimationStepF;

                        // Animation step large enough?
                        if (AnimationValue > StepSize)
                        {
                            AnimationStep = (int)AnimationValue;
                            AnimationValue -= AnimationStep;

                            if (Distance_Current > 0)
                            {
                                moved -= AnimationStep;
                                Distance_Current -= AnimationStep;
                                if (Distance_Current < 0)
                                    Distance_Current = 0;
                            }
                            else if (Distance_Current < 0)
                            {
                                moved += AnimationStep;
                                Distance_Current += AnimationStep;
                                if (Distance_Current > 0)
                                    Distance_Current = 0;
                            }
                        }

                        Raise_OnScrolling(this.parent.containingScreen());

                        Refresh();

                        //System.Threading.Thread.Sleep(500);
                        return true;
                    });

                }
                Refresh();

                #endregion
            }
            else
            {
                // Calculate distance from current location to control's location
                int centerIndex = listStart + (count / 2);
                int Distance_Start = (index - centerIndex) * listItemHeight;
                moved -= Distance_Start;

                Raise_OnScrolling(this.parent.containingScreen());

                Refresh();
            }
            Raise_OnScrollEnd();
        }

        /// <summary>
        /// Cancels any ongoing scrolling
        /// </summary>
        public void ScrollCancel(int screen)
        {
            _ThrowRun = false;
            throwtmr[screen].Enabled = false;
        }

        #region IMouse Members
        /// <summary>
        /// The mouse has been moved
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="e"></param>
        /// <param name="StartLocation"></param>
        /// <param name="TotalDistance"></param>
        /// <param name="RelativeDistance"></param>
        public virtual void MouseMove(int screen, MouseMoveEventArgs e, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            if (listItemHeight > 0) //<-Just in case
                Highlight(((e.Y - top - (moved % listItemHeight)) / listItemHeight) + listStart);

            switch (_Scrollmode)
            {
                case Scrollmodes.FlickToThrow:
                    break;
                case Scrollmodes.DragToScroll:
                    {
                        if (TotalDistance.Y > 5)
                        {
                            if (!ScrolledToTopOfList)
                            {
                                _tmrListScroll.Enabled = true;
                                Raise_OnScrollStart();
                                Highlight(-1);
                                Select(-1);
                            }
                            _ListScrollStepSize = TotalDistance.Y / 7;
                        }
                        else if (TotalDistance.Y < -5)
                        {
                            if (!ScrolledToBottomOfList)
                            {
                                _tmrListScroll.Enabled = true;
                                Raise_OnScrollStart();
                                Highlight(-1);
                                Select(-1);
                            }
                            _ListScrollStepSize = TotalDistance.Y / 7;
                        }
                    }
                    break;
                default:
                    break;
            }

        }

        protected int _ListScrollStepSize = 1;
        void _tmrListScroll_Elapsed(object sender, ElapsedEventArgs e)
        {
            moved += _ListScrollStepSize;

            Raise_OnScrolling(this.parent.containingScreen());

            // Disable timer if we have scrolled to the end
            if (ScrolledToBottomOfList || ScrolledToTopOfList)
            {
                _tmrListScroll.Enabled = false;
                Raise_OnScrollEnd();
            }

            base.Refresh();
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
            set
            {
                Highlight(value);
                base.Refresh();
            }
        }

        public void Highlight(int index)
        {
            lock (this)
            {
                // Deselect if index is out of range
                if ((index < 0) || (index >= _Items.Count))
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
            throwtmr[screen].Enabled = false;
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
            switch (_Scrollmode)
            {
                case Scrollmodes.FlickToThrow:
                    {
                        if (TotalDistance.X < 5 && TotalDistance.Y < 5)
                        {
                            if (listItemHeight > 0) //<-Just in case
                                Highlight(((e.Y - top - (moved % listItemHeight)) / listItemHeight) + listStart);

                            lastSelected = selectedIndex;
                            focused = true;
                            Select(highlightedIndex, false, screen);
                            //if (selectedIndex == lastSelected)
                            //    return;
                            //if (clickSelect)
                            //    mode = eModeType.Scrolling;
                        }
                    }
                    break;
                case Scrollmodes.DragToScroll:
                    {
                        // Was scrolling active?
                        if (_tmrListScroll.Enabled)
                            // Yes, raise event that this ends
                            Raise_OnScrollEnd();

                        _tmrListScroll.Enabled = false;
                    }
                    break;
                default:
                    break;
            }

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
                _Items[selectedIndex].RefreshGraphic = true; 
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
                _Items[selectedIndex].RefreshGraphic = true; 
        }

        #endregion

        internal override void DataSource_OnChanged(OpenMobile.Data.DataSource dataSource)
        {
            try
            {
                // Is this a binary data source, if so use the true/false state to show/hide control
                if (dataSource.DataType == OpenMobile.Data.DataSource.DataTypes.binary)
                {
                    try
                    {
                        base.Visible = (bool)dataSource.Value;
                    }
                    catch
                    {
                        base.Visible = false;
                    }
                }
                else if (dataSource.Value is List<string>)
                {   // List data present
                    List<string> Items = dataSource.Value as List<string>;
                    if (Items != null)
                        return;
                    this.Clear();
                    // Add items from list data
                    foreach (string s in Items)
                    {
                        this.Add(new OMListItem(s));
                    }
                }
            }
            catch
            {
                return;
            }
            _RefreshGraphic = true;
            Refresh();
        }

        #region Events

        #region OnScrollStart

        /// <summary>
        /// Event that's raised when starting to scroll
        /// </summary>
        public event userInteraction OnScrollStart;
        private void Raise_OnScrollStart()
        {
            // Cancel event if no parent is present
            if (this.parent == null)
                return;

            if (OnScrollStart != null)
                OnScrollStart((OMList)this.parent[this.parent.ActiveScreen, this.name], this.parent.ActiveScreen);
        }

        #endregion

        #region OnScrolling

        /// <summary>
        /// Event that's raised while scrolling the list
        /// </summary>
        public event userInteraction OnScrolling;
        private void Raise_OnScrolling(int screen)
        {
            // Cancel event if no parent is present
            if (this.parent == null)
                return;

            if (OnScrolling != null)
                OnScrolling((OMList)this.parent[screen, this.name], this.parent.ActiveScreen);
        }

        #endregion

        #region OnScrollEnd

        /// <summary>
        /// Event that's raised when scroll has ended
        /// </summary>
        public event userInteraction OnScrollEnd;
        private void Raise_OnScrollEnd()
        {
            // Cancel event if no parent is present
            if (this.parent == null)
                return;

            if (OnScrollEnd != null)
                OnScrollEnd((OMList)this.parent[this.parent.ActiveScreen, this.name], this.parent.ActiveScreen);
        }

        #endregion

        #endregion

        /// <summary>
        /// The source of the list items
        /// </summary>
        public object ListSource
        {
            get
            {
                return this._ListSource;
            }
            set
            {
                if (this._ListSource != value)
                {
                    ListSource_DisconnectEvents();

                    // Activate new object
                    this._ListSource = value;

                    // Add items 
                    AddItemsFromListSource();

                    ListSource_ConnectEvents();
                }
            }
        }
        private object _ListSource;

        private void ListSource_DisconnectEvents()
        {
            // Unsubscribe from events for current object
            if (_ListSource != null)
            {
                if (typeof(IBindingList).IsInstanceOfType(_ListSource))
                {
                    var listSource = _ListSource as IBindingList;

                    if (listSource_ListChangedEventHandler == null)
                        listSource_ListChangedEventHandler = new ListChangedEventHandler(listSource_ListChanged);
                    listSource.ListChanged -= listSource_ListChangedEventHandler;
                }
            }
        }

        private void ListSource_ConnectEvents()
        {
            // Subscribe to events of new object
            if (typeof(IBindingList).IsInstanceOfType(_ListSource))
            {
                var listSource = _ListSource as IBindingList;

                if (listSource_ListChangedEventHandler == null)
                    listSource_ListChangedEventHandler = new ListChangedEventHandler(listSource_ListChanged);
                listSource.ListChanged += listSource_ListChangedEventHandler;
            }
        }

        private void AddItemsFromListSource()
        {
            // Add items 
            if (typeof(System.Collections.IEnumerable).IsInstanceOfType(_ListSource))
            {
                var listObject = _ListSource as System.Collections.IEnumerable;
                foreach (var item in listObject)
                {
                    if (_AssignListItemAction == null)
                        this.Add(item.ToString());
                    else
                        this.Add(_AssignListItemAction(item));
                }
            }
        }

        private ListChangedEventHandler listSource_ListChangedEventHandler;
        void listSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            var listSource = _ListSource as IBindingList;

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    {
                        // Create internal list item
                        OMListItem listItem = null;
                        if (e.NewIndex >= 0)
                        {
                            if (_AssignListItemAction == null)
                                listItem = new OMListItem(listSource[e.NewIndex].ToString());
                            else
                                listItem = _AssignListItemAction(listSource[e.NewIndex]);
                        }
                        _Items.Insert(e.NewIndex, listItem);

                        // Redraw control
                        if (e.NewIndex >= GetTopVisibleIndex() && e.NewIndex <= GetBottomVisibleIndex())
                            base.Refresh();

                        // Raise list changed event
                        Raise_OnListChanged(parent.ActiveScreen);
                    }
                    break;
                case ListChangedType.ItemChanged:
                    {
                        // Create internal list item
                        OMListItem listItem = null;
                        if (e.NewIndex >= 0)
                        {
                            if (_AssignListItemAction == null)
                                listItem = new OMListItem(listSource[e.NewIndex].ToString());
                            else
                                listItem = _AssignListItemAction(listSource[e.NewIndex]);
                        }
                        _Items[e.NewIndex] = listItem;

                        // Redraw control
                        if (e.NewIndex >= GetTopVisibleIndex() && e.NewIndex <= GetBottomVisibleIndex())
                            base.Refresh();

                        // Raise list changed event
                        Raise_OnListChanged(parent.ActiveScreen);
                    }
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        _Items.RemoveAt(e.NewIndex);

                        // Redraw control
                        if (e.NewIndex >= GetTopVisibleIndex() && e.NewIndex <= GetBottomVisibleIndex())
                            base.Refresh();

                        // Raise list changed event
                        Raise_OnListChanged(parent.ActiveScreen);
                    }
                    break;
                case ListChangedType.ItemMoved:
                    {
                        var item = _Items[e.OldIndex];
                        _Items.RemoveAt(e.OldIndex);
                        _Items.Insert(e.NewIndex, item);

                        // Redraw control
                        if ((e.NewIndex >= GetTopVisibleIndex() && e.NewIndex <= GetBottomVisibleIndex())
                            || (e.OldIndex >= GetTopVisibleIndex() && e.OldIndex <= GetBottomVisibleIndex()))
                            base.Refresh();

                        // Raise list changed event
                        Raise_OnListChanged(parent.ActiveScreen);
                    }
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                    break;
                case ListChangedType.PropertyDescriptorChanged:
                    break;
                case ListChangedType.PropertyDescriptorDeleted:
                    break;
                case ListChangedType.Reset:
                    {
                        ListSource_DisconnectEvents();
                        _Items.Clear();

                        // Re add items 
                        AddItemsFromListSource();

                        ListSource_ConnectEvents();

                        // Raise list changed event
                        Raise_OnListChanged(parent.ActiveScreen);

                        base.Refresh();
                    }
                    break;
                default:
                    break;
            }

            //// Redraw control
            //base.Refresh();
        }

        /// <summary>
        /// The action to use when assigning an item to the list
        /// </summary>
        public Func<object, OMListItem> AssignListItemAction
        {
            get
            {
                return this._AssignListItemAction;
            }
            set
            {
                if (this._AssignListItemAction != value)
                {
                    this._AssignListItemAction = value;
                }
            }
        }
        private Func<object, OMListItem> _AssignListItemAction;

        public event IndexChangedDelegate OnListChanged;
        private void Raise_OnListChanged(int screen)
        {
            if (OnListChanged != null)
            {
                OnListChanged((OMList)this.parent[screen, this.name], screen);
            }
        }
        
    }
}
