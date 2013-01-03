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
using System.Text;
using System.ComponentModel;
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;

namespace OpenMobile
{
    /// <summary>
    /// A notification
    /// </summary>
    public class Notification : INotifyPropertyChanged, ICloneable
    {
        #region Properties

        /// <summary>
        /// Notification styles
        /// </summary>
        public enum Styles 
        { 
            /// <summary>
            /// Normal information notification consisting of a Icon, Header, Text and an optional action
            /// </summary>
            Normal,
            /// <summary>
            /// A Statusbar icon notification only (like a WiFi strength icon)
            /// </summary>
            IconOnly,
            /// <summary>
            /// Warning notification consisting of a Icon, Header, Text and an optional action
            /// </summary>
            Warning 
        }

        /// <summary>
        /// The style for this notification
        /// </summary>
        public Styles Style
        {
            get
            {
                return this._Style;
            }
            set
            {
                if (this._Style != value)
                {
                    this._Style = value;
                    this.OnPropertyChanged("Style");
                }
            }
        }
        private Styles _Style;        

        /// <summary>
        /// Notification states
        /// </summary>
        public enum States
        {
            /// <summary>
            /// Notification is passive and can be cleared by the user
            /// </summary>
            Passive,
            /// <summary>
            /// Notification is active and can not be cleared by the user
            /// </summary>
            Active
        }

        /// <summary>
        /// The state for this notification
        /// </summary>
        public States State
        {
            get
            {
                return this._State;
            }
            set
            {
                if (this._State != value)
                {
                    this._State = value;
                    this.OnPropertyChanged("State");
                }
            }
        }
        private States _State;        

        /// <summary>
        /// Screen number this notification is used on (if global is set then it's used on all screens and this number has no effect)
        /// </summary>
        public int Screen
        {
            get
            {
                return this._Screen;
            }
            set
            {
                if (this._Screen != value)
                {
                    this._Screen = value;
                }
            }
        }
        private int _Screen;        

        /// <summary>
        /// This notification is global (shown on all screens)
        /// </summary>
        public bool Global { get; set; }

        /// <summary>
        /// A unique ID for this notification
        /// </summary>
        public string ID
        {
            get
            {
                if (_ID == null)
                    return this.GetHashCode().ToString();
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }
        private string _ID = null;

        /// <summary>
        /// The full unique ID of this notification
        /// </summary>
        public string FullID
        {
            get
            {
                return String.Format("{0}_{1}_{2}",this.GetHashCode().ToString(), OwnerPlugin.pluginName, _ID);
            }
        }

        /// <summary>
        /// An icon for this notification
        /// </summary>
        public OImage Icon
        {
            get
            {
                return this._Icon;
            }
            set
            {
                if (this._Icon != value)
                {
                    this._Icon = value;
                    this.OnPropertyChanged("Icon");
                }
            }
        }
        private OImage _Icon;

        /// <summary>
        /// An icon for this notification
        /// </summary>
        public OImage IconStatusBar
        {
            get
            {
                return this._IconStatusBar;
            }
            set
            {
                if (this._IconStatusBar != value)
                {
                    this._IconStatusBar = value;
                    this.OnPropertyChanged("IconStatusBar");
                }
            }
        }
        private OImage _IconStatusBar;        

        /// <summary>
        /// Header text
        /// </summary>
        public string Header
        {
            get
            {
                return _Header;
            }
            set
            {
                _Header = value;
                this.OnPropertyChanged("Header");
            }
        }
        private string _Header = null;

        /// <summary>
        /// Item text
        /// </summary>
        public string Text
        {
            get
            {
                return this._Text;
            }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                    this.OnPropertyChanged("Text");
                }
            }
        }
        private string _Text = null;

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime TimeStamp
        {
            get
            {
                return _TimeStamp;
            }
            set
            {
                _TimeStamp = value;
                this.OnPropertyChanged("TimeStamp");
            }
        }
        private DateTime _TimeStamp;
        /// <summary>
        /// Timestamp as text
        /// </summary>
        public string TimeStampText
        {
            get
            {
                return _TimeStamp.ToShortTimeString();
            }
        }

        /// <summary>
        /// Owner of this notification (IBasePlugin)
        /// </summary>
        public IBasePlugin OwnerPlugin
        {
            get
            {
                return _OwnerPlugin;
            }
        }
        private IBasePlugin _OwnerPlugin;

        #endregion

        #region events

        public delegate void NotificationAction(Notification notification, int screen, ref bool cancel);

        /// <summary>
        /// The code to execute when the notification is clicked
        /// </summary>
        public event NotificationAction ClickAction;

        /// <summary>
        /// The code to execute when the notification is cleared
        /// </summary>
        public event NotificationAction ClearAction;

        /// <summary>
        /// Returns true if this notifications has click actions defined
        /// </summary>
        internal bool HasClickAction
        {
            get
            {
                return ClickAction != null;
            }
        }

        /// <summary>
        /// Returns true if this notifications has clear actions defined
        /// </summary>
        internal bool HasClearAction
        {
            get
            {
                return ClearAction != null;
            }
        }

        /// <summary>
        /// Raises the click action of the notification
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="screen"></param>
        internal bool RaiseClickAction(int screen, ref bool cancel)
        {
            if (ClickAction != null)
            {
                ClickAction(this, screen, ref cancel);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Raises the clear action of the notification
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="screen"></param>
        internal bool RaiseClearAction(int screen, ref bool cancel)
        {
            if (ClearAction != null)
            {
                ClearAction(this, screen, ref cancel);
                return true;
            }
            return false;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="timeStamp"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(Styles Style, States State, IBasePlugin OwnerPlugin, string ID, DateTime timeStamp, OImage Icon, string Header, string Text)
            : this(Style, OwnerPlugin, ID, timeStamp, Icon, Header, Text)
        {
            this.State = State;
        }
        public Notification(Styles Style, States State, IBasePlugin OwnerPlugin, string ID, DateTime timeStamp, OImage Icon, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
            : this(Style, OwnerPlugin, ID, timeStamp, Icon, Header, Text, clickAction, clearAction)
        {
            this.State = State;
        }

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="timeStamp"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, OImage Icon)
            : this(Style, OwnerPlugin, ID, DateTime.Now, Icon, "", "")
        {
            this.State = States.Active;
        }
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, OImage Icon, NotificationAction clickAction, NotificationAction clearAction)
            : this(Style, OwnerPlugin, ID, DateTime.Now, Icon, "", "", clickAction, clearAction)
        {
            this.State = States.Active;
        }

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="timeStamp"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, DateTime timeStamp, OImage Icon, string Header, string Text)
            : this(Style, OwnerPlugin, ID, timeStamp, Icon, null, Header, Text)
        {
        }
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, DateTime timeStamp, OImage Icon, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
            : this(Style, OwnerPlugin, ID, timeStamp, Icon, null, Header, Text, clickAction, clearAction)
        {
        }

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="timeStamp"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, DateTime timeStamp, OImage Icon, OImage IconStatusBar, string Header, string Text)
        {
            this.Style = Style;
            this._OwnerPlugin = OwnerPlugin;
            this._Icon = Icon;
            this._IconStatusBar = IconStatusBar;
            this._Header = Header;
            this._Text = Text;
            this._ID = ID;
            this._TimeStamp = timeStamp;
        }
         
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, DateTime timeStamp, OImage Icon, OImage IconStatusBar, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
        {
            this.Style = Style;
            this._OwnerPlugin = OwnerPlugin;
            this._Icon = Icon;
            this._IconStatusBar = IconStatusBar;
            this._Header = Header;
            this._Text = Text;
            this._ID = ID;
            this._TimeStamp = timeStamp;
            this.ClearAction = clearAction;
            this.ClickAction = clickAction;
        }
        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(Styles Style, States State, IBasePlugin OwnerPlugin, string ID, OImage Icon, string Header, string Text)
            : this(Style, OwnerPlugin, ID, DateTime.Now, Icon, Header, Text)
        {
            this.State = State;
        }
        public Notification(Styles Style, States State, IBasePlugin OwnerPlugin, string ID, OImage Icon, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
            : this(Style, OwnerPlugin, ID, DateTime.Now, Icon, Header, Text, clickAction, clearAction)
        {
            this.State = State;
        }

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, OImage Icon, string Header, string Text)
            : this(Style, OwnerPlugin, ID, DateTime.Now, Icon, Header, Text)
        {
        }

        public Notification(Styles Style, IBasePlugin OwnerPlugin, string ID, OImage Icon, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
            : this(Style, OwnerPlugin, ID, DateTime.Now, Icon, Header, Text, clickAction, clearAction)
        {
        }

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(IBasePlugin OwnerPlugin, string ID, OImage Icon, string Header, string Text)
            : this(Styles.Normal, OwnerPlugin, ID, DateTime.Now, Icon, Header, Text)
        {
        }

        public Notification(IBasePlugin OwnerPlugin, string ID, OImage Icon, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
            : this(Styles.Normal, OwnerPlugin, ID, DateTime.Now, Icon, Header, Text, clickAction, clearAction)
        {
        }

        /// <summary>
        /// Initializes a new notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <param name="Icon"></param>
        /// <param name="Header"></param>
        /// <param name="Text"></param>
        public Notification(IBasePlugin OwnerPlugin, string ID, OImage Icon, OImage IconStatusBar, string Header, string Text)
            : this(Styles.Normal, OwnerPlugin, ID, DateTime.Now, Icon, IconStatusBar, Header, Text)
        {
        }

        public Notification(IBasePlugin OwnerPlugin, string ID, OImage Icon, OImage IconStatusBar, string Header, string Text, NotificationAction clickAction, NotificationAction clearAction)
            : this(Styles.Normal, OwnerPlugin, ID, DateTime.Now, Icon, IconStatusBar, Header, Text, clickAction, clearAction)
        {
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes this object
        /// </summary>
        ~Notification()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            // Do not dispose icon in case others are using it as well
            //_Icon.Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Get's a shallow copy of this notification
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
