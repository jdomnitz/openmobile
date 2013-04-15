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
using System.Reflection;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A listbox style control
    /// </summary>
    public class OMObjectList : OMContainer, IMousePreview
    {
        #region Preconfigured controls

        public static OMObjectList PreConfigLayout_ImageFlow(string name, int left, int top, int width, int height, Size itemSize, int overlap)
        {
            OMObjectList list = new OMObjectList(name, left, top, width, height);

            OMObjectList.ListItem ItemBase = new OMObjectList.ListItem();

            ItemBase.ItemSize = new Size(itemSize.Width - overlap, itemSize.Height);

            OMImage imgImage = new OMImage("imgImage", 0, (height / 2) - (itemSize.Height / 2), itemSize.Width, itemSize.Height);
            imgImage.Rotation = new Math.Vector3(0, -50, 0);
            //imgImage.Transparency = 20;
            ItemBase.Add(imgImage);

            //OMButton btnImage = new OMButton("btnImage", 0, 0, itemSize.Width, itemSize.Height);
            //ItemBase.Add(btnImage);

            ItemBase.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
            {
                //OMButton btnItemCard = item["btnImage"] as OMButton;
                //if (helperFunctions.Params.IsParamsValid(values, 1) && values[0] != null)
                //{
                //    if (values[0] is imageItem)
                //        btnItemCard.Image = (imageItem)values[0];
                //    else if (values[0] is OImage)
                //        btnItemCard.Image = new imageItem((OImage)values[0]);
                //}

                OMImage imgItemCover = item["imgImage"] as OMImage;
                if (helperFunctions.Params.IsParamsValid(values, 1) && values[0] != null)
                {
                    if (values[0] is imageItem)
                        imgItemCover.Image = (imageItem)values[0];
                    else if (values[0] is OImage)
                        imgItemCover.Image = new imageItem((OImage)values[0]);
                }
            };

            list.ItemBase = ItemBase;

            return list;
        }

        #endregion

        public class ListItem : ControlGroup
        {
            public SetItemInfoDelegate Action_SetItemInfo = null;
            public ItemActionDelegate Action_Select = null;
            public ItemActionDelegate Action_Deselect = null;
            public ItemActionDelegate Action_Highlight = null;
            public ItemActionDelegate Action_Unhighlight = null;

            internal int myIndex = -1;

            internal bool hitTest(Point p)
            {
                return OMControl.GetControlsArea(this).Contains(p);
            }

            public OMControl this[string name]
            {
                get
                {
                    name = String.Format("{0}:", name);
                    return this.Find(x => x.Name.Contains(name));
                }
                set
                {
                    name = String.Format("{0}:", name);
                    int index = this.FindIndex(x => x.Name.Contains(name));
                    this.RemoveAt(index);
                    this.Insert(index, value);
                }
            }

            public ListItem()
            {
            }

            private void SetControlName(int myIndex)
            {
                foreach (OMControl control in this)
                    if (!control.Name.Contains(":"))
                        control.Name = String.Format("{0}:{1}", control.Name, myIndex);
            }

            internal void ExecuteAction_Select(OMObjectList sender, int screen, int myIndex)
            {
                this.myIndex = myIndex;
                SetControlName(myIndex);
                if (Action_Select != null)
                    Action_Select(sender, screen, this);
            }
            internal void ExecuteAction_Deselect(OMObjectList sender, int screen, int myIndex)
            {
                this.myIndex = myIndex;
                SetControlName(myIndex);
                if (Action_Deselect != null)
                    Action_Deselect(sender, screen, this);
            }
            internal void ExecuteAction_Highlight(OMObjectList sender, int screen, int myIndex)
            {
                this.myIndex = myIndex;
                SetControlName(myIndex);

                // Use highlight if present if not use select
                if (Action_Highlight != null)
                    Action_Highlight(sender, screen, this);
                else
                    if (Action_Select != null)
                        Action_Select(sender, screen, this);
            }
            internal void ExecuteAction_Unhighlight(OMObjectList sender, int screen, int myIndex)
            {
                this.myIndex = myIndex;
                SetControlName(myIndex);

                // Use highlight if present if not use select
                if (Action_Unhighlight != null)
                    Action_Unhighlight(sender, screen, this);
                else
                    if (Action_Deselect != null)
                        Action_Deselect(sender, screen, this);
            }

            internal void ExecuteAction_SetValues(OMObjectList sender, int screen, int myIndex, object[] values)
            {
                this.myIndex = myIndex;
                SetControlName(myIndex);
                if (Action_SetItemInfo != null)
                    Action_SetItemInfo(sender, screen, this, values);
            }

            /// <summary>
            /// Clones the object
            /// </summary>
            /// <returns></returns>
            public ListItem Clone()
            {
                ListItem newListItem = new ListItem();
                newListItem._ItemSize = this._ItemSize;
                newListItem.Action_SetItemInfo = Action_SetItemInfo;
                newListItem.Action_Select = Action_Select;
                newListItem.Action_Deselect = Action_Deselect;
                newListItem.Action_Highlight = Action_Highlight;
                newListItem.Action_Unhighlight = Action_Unhighlight;
                for (int i = 0; i < this.Count; i++)
                    newListItem.Add((OMControl)this[i].Clone());
                return newListItem;
            }

            public Rectangle Region
            {
                get
                {
                    Rectangle area = base.Region;

                    // Check for size override
                    if (!_ItemSize.IsEmpty)
                    {
                        area.Width = _ItemSize.Width;
                        area.Height = _ItemSize.Height;
                    }
                    return area;
                }
            }
        }

        public delegate void IndexChangedDelegate(OMObjectList sender, int screen);
        /// <summary>
        /// Occurs when the list index changes
        /// </summary>
        public event IndexChangedDelegate OnSelectedIndexChanged;
        /// <summary>
        /// Occurs when the highlighted item changes
        /// </summary>
        public event IndexChangedDelegate OnHighlightedIndexChanged;

        private ListItem _ItemBase = null;
        public delegate void SetItemInfoDelegate(OMObjectList sender, int screen, ListItem item, object[] values);
        public delegate void ItemActionDelegate(OMObjectList sender, int screen, ListItem item);
        private Rectangle _ItemBase_Region;
        private int _Items_SelectedIndex = -1;
        private int _Items_HighlightedIndex = -1;

        private List<ListItem> _Items = new List<ListItem>();

        ///// <summary>
        ///// Directions to use when adding list items
        ///// </summary>
        //public enum ListDirection
        //{
        //    Down,
        //    Right,
        //    Left,
        //    Up
        //}

        #region Constructors

        /// <summary>
        /// Creates a new instance of the control
        /// </summary>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public OMObjectList(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            //AddListItems(50);
        }

        #endregion

        /// <summary>
        /// Index of the selected item
        /// </summary>
        public int SelectedItemIndex
        {
            get
            {
                return _Items_SelectedIndex;
            }
            set
            {
                Item_Select(value);
            }
        }

        /// <summary>
        /// Index of the selected item
        /// </summary>
        public ListItem SelectedItem
        {
            get
            {
                return _Items[_Items_SelectedIndex];
            }
        }
        /// <summary>
        /// Index of the highlighted item
        /// </summary>
        public int HighlightedItemIndex
        {
            get
            {
                return _Items_HighlightedIndex;
            }
            set
            {
                Item_Highlight(value);
            }
        }
        /// <summary>
        /// Index of the selected item
        /// </summary>
        public ListItem HighlightedItem
        {
            get
            {
                return _Items[_Items_HighlightedIndex];
            }
        }

        public ListItem ItemBase
        {
            get
            {
                return _ItemBase;
            }
            set
            {
                _ItemBase = value;
            }
        }

        public List<ListItem> Items
        {
            get
            {
                return _Items; 
            }
        }
        
        //private void ListItem_SetPlacement(int index, ListItem controls, ListDirection direction)
        //{
        //    // Set placement 
        //    switch (direction)
        //    {
        //        case ListDirection.Down:
        //            {
        //                // Get placement of control above this one
        //                Rectangle ControlPreviousArea = new Rectangle();
        //                Rectangle ListControlArea = new Rectangle();
        //                if (index > 0)
        //                {
        //                    ControlPreviousArea = _Items[index - 1].Region;//OMControl.GetControlsArea(_Items[index - 1].Controls);
        //                    ListControlArea = this.Region;
        //                }

        //                foreach (OMControl control in controls.Controls)
        //                    control.Top = (ControlPreviousArea.Bottom - ListControlArea.Top) + control.Top;
        //            }
        //            break;
        //        case ListDirection.Right:
        //            {
        //                // Get placement of control to the left of this one
        //                Rectangle ControlPreviousArea = new Rectangle();
        //                Rectangle ListControlArea = new Rectangle();
        //                if (index > 0)
        //                {
        //                    ControlPreviousArea = _Items[index - 1].Region;//OMControl.GetControlsArea(_Items[index - 1].Controls);
        //                    ListControlArea = this.Region;
        //                }

        //                foreach (OMControl control in controls.Controls)
        //                    control.Left = (ControlPreviousArea.Right - ListControlArea.Left) + control.Left;
        //            }
        //            break;
        //        case ListDirection.Left:
        //            break;
        //        case ListDirection.Up:
        //            break;
        //        default:
        //            break;
        //    }

        //    if (ControlAndOffsetDataOverride != null)
        //    {
        //        Rectangle Offset = new Rectangle();
        //        ControlAndOffsetDataOverride(controls.Controls, ref Offset);
        //        controls.Controls.Translate(Offset);
        //    }
        //}

        public void AddItemFromItemBase(ControlDirections direction, params object[] values)
        {
            AddItemFromItemBase(values, direction);
        }

        public void AddItemFromItemBase(object[] values, ControlDirections direction)
        {
            if (this.parent == null)
                throw new Exception("OMObjectList needs a valid parent. Ensure the control is added to a panel before adding items to it");

            ListItem listItem = null;

            // Ensure ItemBase can't be changed while we're updating it
            lock (_ItemBase)
                listItem = _ItemBase.Clone();

            // Add new item to items list
            _Items.Add(listItem);
            listItem.ExecuteAction_SetValues(this, this.parent.ActiveScreen, _Items.Count - 1, values);

            // Add controls to renderinglist
            base.addControl(listItem, true, direction);

            if (ControlAndOffsetDataOverride != null)
            {
                Rectangle Offset = new Rectangle();
                ControlAndOffsetDataOverride(listItem, ref Offset);
                listItem.Translate(Offset);
            }
        }

        public void AddItem(ListItem baseItem, ControlDirections direction, params object[] values)
        {
            AddItem(baseItem, values, direction);
        }
        public void AddItem(ListItem baseItem, object[] values, ControlDirections direction)
        {
            if (this.parent == null)
                throw new Exception("OMObjectList needs a valid parent. Ensure the control is added to a panel before adding items to it");

            ListItem listItem = null;

            // Ensure ItemBase can't be changed while we're updating it
            lock (baseItem)
                listItem = baseItem.Clone();

            // Add new item to items list
            _Items.Add(listItem);
            listItem.ExecuteAction_SetValues(this, this.parent.ActiveScreen, _Items.Count - 1, values);

            // Add controls to renderinglist
            base.addControl(listItem, true, direction);

            if (ControlAndOffsetDataOverride != null)
            {
                Rectangle Offset = new Rectangle();
                ControlAndOffsetDataOverride(listItem, ref Offset);
                listItem.Translate(Offset);
            }
        }


        /// <summary>
        /// Clear list items
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            this.ClearControls();
            Refresh();
        }

        private int GetItemIndexFromPoint(Point p)
        {
            // Loop trough all items in list
            for (int i = 0; i < _Items.Count; i++)
            {
                // Is point within a controls area?
                if (_Items[i].hitTest(p))
                    return i;                
            }
            return -1;
        }

        private void Item_Select(int index)
        {
            // Deselect selected item
            if (_Items_SelectedIndex >= 0)
                _Items[_Items_SelectedIndex].ExecuteAction_Deselect(this, this.parent.ActiveScreen, _Items_SelectedIndex);

            // Selected new item
            _Items_SelectedIndex = index;
            if (_Items_SelectedIndex >= 0)
                _Items[_Items_SelectedIndex].ExecuteAction_Select(this, this.parent.ActiveScreen, _Items_SelectedIndex);

            // Trigger event
            if (OnSelectedIndexChanged != null)
                OnSelectedIndexChanged(this, this.parent.ActiveScreen);
        }
        private void Item_Highlight(int index)
        {
            // Deselect selected item
            if (_Items_HighlightedIndex >= 0)
                _Items[_Items_HighlightedIndex].ExecuteAction_Unhighlight(this, this.parent.ActiveScreen, _Items_HighlightedIndex);

            // Selected new item
            _Items_HighlightedIndex = index;
            if (_Items_HighlightedIndex >= 0)
                _Items[_Items_HighlightedIndex].ExecuteAction_Highlight(this, this.parent.ActiveScreen, _Items_HighlightedIndex);

            // Trigger event
            if (OnHighlightedIndexChanged != null)
                OnHighlightedIndexChanged(this, this.parent.ActiveScreen);
        }

        #region IMousePreview Members

        public void MouseMove(int screen, MouseMoveEventArgs e, Point StartLocation, Point TotalDistance, Point RelativeDistance)
        {
            // Highlight item under mouse
            Item_Highlight(GetItemIndexFromPoint(e.Location));
        }

        public void MouseDown(int screen, MouseButtonEventArgs e, Point StartLocation)
        {
            // Cancel any active throw 
            _ThrowRun = false;

            // Unselect currently selected item
            //Item_Select(-1);

            // Highlight item under mouse
            Item_Highlight(GetItemIndexFromPoint(e.Location));
        }

        public void MouseUp(int screen, MouseButtonEventArgs e, Point StartLocation, Point TotalDistance)
        {
            if (!ThrowActive)
            {
                // unhighlight currently highlighted item
                Item_Highlight(-1);

                // Select item
                Item_Select(GetItemIndexFromPoint(e.Location));
            }
        }

        #endregion

        private bool ThrowActive = false;
        public override void MouseThrowStart(int screen, Point StartLocation, PointF CursorSpeed, PointF scaleFactors, ref bool Cancel)
        {
            // Unselect any items when a throw starts
            Item_Highlight(-1);
            base.MouseThrowStart(screen, StartLocation, CursorSpeed, scaleFactors, ref Cancel);
            ThrowActive = true;
        }

        public override void MouseThrowEnd(int screen, Point StartLocation, Point TotalDistance, Point EndLocation, PointF CursorSpeed)
        {
            base.MouseThrowEnd(screen, StartLocation, TotalDistance, EndLocation, CursorSpeed);
            ThrowActive = false;
        }

        public override object Clone(OMPanel parent)
        {
            OMObjectList newList = (OMObjectList)base.Clone(parent);
            newList._Items = this._Items.ConvertAll(x => (ListItem)x.Clone());
            return newList;
        }


        public delegate void ControlAndOffsetDataDelegate(ControlGroup control, ref Rectangle Offset);
        public ControlAndOffsetDataDelegate ControlAndOffsetDataOverride = null;
        internal override void ModifyControlAndOffsetData(ControlGroup control, ref Rectangle Offset)
        {
            if (ControlAndOffsetDataOverride == null) 
                base.ModifyControlAndOffsetData(control, ref Offset);
            else
                ControlAndOffsetDataOverride(control, ref Offset);
        }

        public delegate void RenderOrder_SetDelegate(List<ControlGroup> controls, Rectangle Offset, ref List<int> renderOrder);
        public RenderOrder_SetDelegate ListItems_RenderOrderCalc = null;
        protected override void RenderOrder_Set(List<ControlGroup> controls, Rectangle Offset, ref List<int> renderOrder)
        {
            if (ListItems_RenderOrderCalc == null)
                base.RenderOrder_Set(controls, Offset, ref renderOrder);
            else
                ListItems_RenderOrderCalc(controls, Offset, ref renderOrder);
        }

        #region Base Method hiding

        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(OMControl control)
        {
            return base.addControl(control);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControlRelative(OMControl control)
        {
            return base.addControlRelative(control);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControlAbsolute(OMControl control)
        {
            return base.addControlAbsolute(control);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(OMControl control, bool Relative)
        {
            return base.addControl(control, Relative);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(OMControl control, ControlDirections direction)
        {
            return base.addControl(control, direction);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(ControlGroup cg, bool Relative)
        {
            return base.addControl(cg, Relative);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(ControlGroup cg, ControlDirections direction)
        {
            return base.addControl(cg, direction);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(ControlGroup cg, bool relative, ControlDirections direction)
        {
            return base.addControl(cg, relative, direction);
        }
        [Obsolete("This method is not supported in the OMObjectList control", true)]
        public new bool addControl(int index, ControlGroup cg, bool relative, ControlDirections direction)
        {
            return base.addControl(index, cg, relative, direction);
        }

        #endregion

    }
}
