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
using System.Collections.ObjectModel;

namespace OpenMobile.Controls
{
    /// <summary>
    /// A listbox style control
    /// </summary>
    public class OMObjectList3 : OMControl
    {
        public delegate void SetItemDelegate(OMObjectList3 sender, int screen, object item);


        private Action<Rectangle, object> _Render_Default = (Rectangle rect, object item) =>
        {

        };

        /// <summary>
        /// The rendering code for an item
        /// </summary>
        public Action<Rectangle, object> ItemRender
        {
            get
            {
                return this._ItemRender;
            }
            set
            {
                if (this._ItemRender != value)
                {
                    this._ItemRender = value;
                }
            }
        }
        private Action<Rectangle, object> _ItemRender;        

        /// <summary>
        /// The size of an item in the list. A value of 0 means adjust automatically
        /// </summary>
        public Size ItemSize
        {
            get
            {
                return this._ItemSize;
            }
            set
            {
                if (this._ItemSize != value)
                {
                    this._ItemSize = value;
                }
            }
        }
        private Size _ItemSize;

        /// <summary>
        /// The items contained in this list
        /// </summary>
        public ObservableCollection<object> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                if (this._Items != value)
                {
                    this._Items = value;
                }
            }
        }
        private ObservableCollection<object> _Items;

        public OMObjectList3(string name, int x, int y, int w, int h)
            : base(name, x, y, w, h)
        {
            _Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_Items_CollectionChanged);
        }

        void _Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

    }
}
