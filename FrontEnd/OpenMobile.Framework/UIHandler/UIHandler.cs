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
using OpenMobile;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;


namespace OpenMobile.UI
{
    /// <summary>
    /// A handler for status bar objects (Top bar, bottom bar and notifications)
    /// </summary>
    public class UIHandler
    {
        #region DropDown menu show / hide

        /// <summary>
        /// Delegate to show / hide ui elements
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public delegate void ShowHideControlDelegate(int screen, bool fast);

        /// <summary>
        /// Event is raised when the dropdown menu is requested to hide
        /// </summary>
        public event ShowHideControlDelegate OnHideDropDown;
        private void Raise_OnHideDropDown(int screen, bool fast)
        {
            if (OnHideDropDown != null)
                OnHideDropDown(screen, fast);
        }

        /// <summary>
        /// Event is raised when the dropdown menu is requested to show
        /// </summary>
        public event ShowHideControlDelegate OnShowDropDown;
        private void Raise_OnShowDropDown(int screen, bool fast)
        {
            if (OnShowDropDown != null)
                OnShowDropDown(screen, fast);
        }

        /// <summary>
        /// Show the drop down menu
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void DropDown_Show(int screen, bool fast)
        {
            // Call event
            Raise_OnShowDropDown(screen, fast);
        }

        /// <summary>
        /// Hide the drop down menu
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void DropDown_Hide(int screen, bool fast)
        {
            // Call event
            Raise_OnHideDropDown(screen, fast);
        }

        #endregion

        #region Top and bottom bars show / hide

        /// <summary>
        /// UI Bars
        /// </summary>
        [Flags]
        public enum Bars 
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// Top bar
            /// </summary>
            Top = 1, 
            /// <summary>
            /// Bottom bar
            /// </summary>
            Bottom = 2, 
            /// <summary>
            /// Left bar
            /// </summary>
            Left = 4, 
            /// <summary>
            /// Right bar
            /// </summary>
            Right = 8,
            /// <summary>
            /// All bars
            /// </summary>
            All = Bars.Bottom | Bars.Left | Bars.Right | Bars.Top
        }

        /// <summary>
        /// Delegate to show / hide UI bars
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        /// <param name="executeOnBars"></param>
        public delegate void ShowHideBarsControlDelegate(int screen, bool fast, Bars executeOnBars);

        /// <summary>
        /// Event is raised when the top and bottom bars is requested to hide
        /// </summary>
        public event ShowHideBarsControlDelegate OnHideBars;
        private void Raise_OnHideBars(int screen, bool fast, Bars executeOnBars)
        {
            if (OnHideBars != null)
                OnHideBars(screen, fast, executeOnBars);
        }

        /// <summary>
        /// Event is raised when the top and bottom bars is requested to show
        /// </summary>
        public event ShowHideBarsControlDelegate OnShowBars;
        private void Raise_OnShowBars(int screen, bool fast, Bars executeOnBars)
        {
            if (OnShowBars != null)
                OnShowBars(screen, fast, executeOnBars);
        }

        /// <summary>
        /// Show the top and bottom bars
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        /// <param name="executeOnBars"></param>
        public void Bars_Show(int screen, bool fast, Bars executeOnBars)
        {
            // Call event
            Raise_OnShowBars(screen, fast, executeOnBars);
        }

        /// <summary>
        /// Hide the top and bottom bars
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        /// <param name="executeOnBars"></param>
        public void Bars_Hide(int screen, bool fast, Bars executeOnBars)
        {
            // Call event
            Raise_OnHideBars(screen, fast, executeOnBars);
            Raise_OnHideDropDown(screen, true);
        }

        #endregion

        #region DropDown ButtonStrip

        /// <summary>
        /// The ButtonStrip container for the drop down menu
        /// <para>This contaienr has to be loaded into the UI skin before it will show</para>
        /// </summary>
        public ButtonStripContainer DropDown_ButtonStripContainer
        {
            get
            {
                return this._DropDown_ButtonStripContainer;
            }
            set
            {
                if (this._DropDown_ButtonStripContainer != value)
                {
                    this._DropDown_ButtonStripContainer = value;
                }
            }
        }
        private ButtonStripContainer _DropDown_ButtonStripContainer;

        /// <summary>
        /// The main ButtonStrip for the drop down menu (other button strips are implemented by the skins themself)
        /// </summary>
        public ButtonStrip DropDown_MainButtonStrip
        {
            get
            {
                return this._DropDown_MainButtonStrip;
            }
            set
            {
                if (this._DropDown_MainButtonStrip != value)
                {
                    this._DropDown_MainButtonStrip = value;
                }
            }
        }
        private ButtonStrip _DropDown_MainButtonStrip;

        /// <summary>
        /// Activates the main button strip for the dropdown menu 
        /// <para>Specifiy a screen number to activate on a specific screen, use null to activate on all screens</para>
        /// </summary>
        /// <param name="screen"></param>
        public void DropDown_ShowMainButtonStrip(int? screen)
        {
            if (_DropDown_ButtonStripContainer != null)
                _DropDown_ButtonStripContainer.SetButtonStrip(screen, _DropDown_MainButtonStrip);
        }

        #endregion

        #region Control Buttons (show/hide and properties)

        private Timer[] _tmrControlButtonsAutoHide;

        /// <summary>
        /// The main ButtonStrip for the bottom bar menu (other button strips are implemented by the skins themself)
        /// </summary>
        public ButtonStrip ControlButtons_MainButtonStrip
        {
            get
            {
                return this._ControlButtons_MainButtonStrip;
            }
            set
            {
                if (this._ControlButtons_MainButtonStrip != value)
                {
                    this._ControlButtons_MainButtonStrip = value;
                }
            }
        }
        private ButtonStrip _ControlButtons_MainButtonStrip;

        /// <summary>
        /// The ButtonStrip container for the ControlButtons in the bottom bar
        /// <para>This container has to be loaded into the UI skin before it will show</para>
        /// </summary>
        public ButtonStripContainer ControlButtons
        {
            get
            {
                return this._ControlButtons_ButtonStripContainer;
            }
            set
            {
                if (this._ControlButtons_ButtonStripContainer != value)
                {
                    if (this._ControlButtons_ButtonStripContainer != null)
                        this._ControlButtons_ButtonStripContainer.OnButtonStripSet -= _ControlButtons_ButtonStripContainer_OnButtonStripSet_Handler;
                    this._ControlButtons_ButtonStripContainer = value;
                    this._ControlButtons_ButtonStripContainer.OnButtonStripSet += _ControlButtons_ButtonStripContainer_OnButtonStripSet_Handler;
                }
            }
        }
        private ButtonStripContainer _ControlButtons_ButtonStripContainer;

        ButtonStripContainer.ButtonStripChangedEventHandler _ControlButtons_ButtonStripContainer_OnButtonStripSet_Handler;
        void ControlButtons_ButtonStripContainer_OnButtonStripSet(object sender, int screen, ButtonStrip buttonStrip)
        {
            // Raise event for buttonstrip set / removed
            if (buttonStrip == null)
                // strip removed
                Raise_OnControlButtonsChanged(screen, false);
            else
                // Strip set
                Raise_OnControlButtonsChanged(screen, true);
        }

        /// <summary>
        /// Event is raised when the ControlButtons is changed
        /// </summary>
        public event PopupMenuEventHandler OnControlButtonsChanged;
        private void Raise_OnControlButtonsChanged(int screen, bool available)
        {
            if (OnControlButtonsChanged != null)
                OnControlButtonsChanged(_ControlButtons_ButtonStripContainer, screen, available);
        }

        /// <summary>
        /// Resets the button strip for the ControlButtons 
        /// <para>Specifiy a screen number to reset on a specific screen, use null to reset on all screens</para>
        /// </summary>
        /// <param name="screen"></param>
        public void ControlButtons_Reset(int? screen)
        {
            if (_ControlButtons_ButtonStripContainer != null)
                _ControlButtons_ButtonStripContainer.SetButtonStrip(screen, _ControlButtons_MainButtonStrip);
        }

        /// <summary>
        /// Clears the button strip for the ControlButtons 
        /// <para>Specifiy a screen number to clear on a specific screen, use null to clear on all screens</para>
        /// </summary>
        /// <param name="screen"></param>
        public void ControlButtons_Clear(int? screen)
        {
            if (_ControlButtons_ButtonStripContainer != null)
                _ControlButtons_ButtonStripContainer.SetButtonStrip(screen, null);
        }

        /// <summary>
        /// Event is raised when the ControlButtons is requested to hide
        /// </summary>
        public event ShowHideControlDelegate OnHideControlButtons;
        private void Raise_OnHideControlButtons(int screen, bool fast)
        {
            if (OnHideControlButtons != null)
                OnHideControlButtons(screen, fast);
        }

        /// <summary>
        /// Event is raised when the ControlButtons is requested to show
        /// </summary>
        public event ShowHideControlDelegate OnShowControlButtons;
        private void Raise_OnShowControlButtons(int screen, bool fast)
        {
            if (OnShowControlButtons != null)
                OnShowControlButtons(screen, fast);
        }

        public void ControlButtons_AutoHideTimer_Reset(int screen)
        {
            if ((_tmrControlButtonsAutoHide.Length <= screen && screen < 0) || _tmrControlButtonsAutoHide[screen] == null)
                return;

            if (_tmrControlButtonsAutoHide[screen].Enabled)
            {
                _tmrControlButtonsAutoHide[screen].Enabled = false;
                _tmrControlButtonsAutoHide[screen].Enabled = true;
            }
        }

        public void ControlButtons_Show(int screen, bool fast, int secondsToShow)
        {
            // Call event
            Raise_OnShowControlButtons(screen, fast);

            if (_tmrControlButtonsAutoHide[screen] == null)
            {
                _tmrControlButtonsAutoHide[screen] = new Timer(secondsToShow * 1000);
                _tmrControlButtonsAutoHide[screen].Elapsed += _tmrControlButtonsAutoHide_Elapsed;
                _tmrControlButtonsAutoHide[screen].AutoReset = false;
            }
            _tmrControlButtonsAutoHide[screen].Interval = secondsToShow * 1000;
            _tmrControlButtonsAutoHide[screen].Screen = screen;
            _tmrControlButtonsAutoHide[screen].Tag = fast;
            _tmrControlButtonsAutoHide[screen].Enabled = true;
        }

        void _tmrControlButtonsAutoHide_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer tmr = sender as Timer;
            tmr.Enabled = false;
            Raise_OnHideControlButtons(tmr.Screen, (bool)tmr.Tag);
        }

        /// <summary>
        /// Show the ControlButtons
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void ControlButtons_Show(int screen, bool fast)
        {
            // Call event
            Raise_OnShowControlButtons(screen, fast);
        }

        /// <summary>
        /// Hide the ControlButtons
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void ControlButtons_Hide(int screen, bool fast)
        {
            // Call event
            Raise_OnHideControlButtons(screen, fast);
        }

        #endregion

        #region Popup menu (show/hide and properties)

        /// <summary>
        /// The ButtonStrip container for the popup menu in the bottom bar
        /// <para>This container has to be loaded into the UI skin before it will show</para>
        /// </summary>
        public ButtonStripContainer PopUpMenu
        {
            get
            {
                return this._PopUpMenu_ButtonStripContainer;
            }
            set
            {
                if (this._PopUpMenu_ButtonStripContainer != value)
                {
                    if (this._PopUpMenu_ButtonStripContainer != null)
                        this._PopUpMenu_ButtonStripContainer.OnButtonStripSet -= _PopUpMenu_ButtonStripContainer_OnButtonStripSet_Handler;
                    this._PopUpMenu_ButtonStripContainer = value;
                    this._PopUpMenu_ButtonStripContainer.OnButtonStripSet += _PopUpMenu_ButtonStripContainer_OnButtonStripSet_Handler;
                }
            }
        }
        private ButtonStripContainer _PopUpMenu_ButtonStripContainer;

        ButtonStripContainer.ButtonStripChangedEventHandler _PopUpMenu_ButtonStripContainer_OnButtonStripSet_Handler;
        void PopUpMenu_ButtonStripContainer_OnButtonStripSet(object sender, int screen, ButtonStrip buttonStrip)
        {
            // Raise event for buttonstrip set / removed
            if (buttonStrip == null)
                // strip removed
                Raise_OnPopupMenuChanged(screen, false);
            else
                // Strip set
                Raise_OnPopupMenuChanged(screen, true);
        }

        /// <summary>
        /// PopUpMenu event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        /// <param name="popupAvailable"></param>
        public delegate void PopupMenuEventHandler(object sender, int screen, bool popupAvailable);

        /// <summary>
        /// Event is raised when the popup menu is changed
        /// </summary>
        public event PopupMenuEventHandler OnPopupMenuChanged;
        private void Raise_OnPopupMenuChanged(int screen, bool popupAvailable)
        {
            if (OnPopupMenuChanged != null)
                OnPopupMenuChanged(_PopUpMenu_ButtonStripContainer, screen, popupAvailable);
        }

        /// <summary>
        /// Clears the button strip for the bottom bar popup menu 
        /// <para>Specifiy a screen number to clear on a specific screen, use null to clear on all screens</para>
        /// </summary>
        /// <param name="screen"></param>
        public void PopUpMenu_Clear(int? screen)
        {
            if (_PopUpMenu_ButtonStripContainer != null)
                _PopUpMenu_ButtonStripContainer.SetButtonStrip(screen, null);
        }

        /// <summary>
        /// Event is raised when the popup menu is requested to hide
        /// </summary>
        public event ShowHideControlDelegate OnHidePopUpMenu;
        private void Raise_OnHidePopUpMenu(int screen, bool fast)
        {
            if (OnHidePopUpMenu != null)
                OnHidePopUpMenu(screen, fast);
        }

        /// <summary>
        /// Event is raised when the popup menu is requested to show
        /// </summary>
        public event ShowHideControlDelegate OnShowPopUpMenu;
        private void Raise_OnShowPopUpMenu(int screen, bool fast)
        {
            if (OnShowPopUpMenu != null)
                OnShowPopUpMenu(screen, fast);
        }

        /// <summary>
        /// Show the PopUp Menu
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void PopUpMenu_Show(int screen, bool fast)
        {
            // Call event
            Raise_OnShowPopUpMenu(screen, fast);
        }

        /// <summary>
        /// Hide the PopUp Menu
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void PopUpMenu_Hide(int screen, bool fast)
        {
            // Call event
            Raise_OnHidePopUpMenu(screen, fast);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new handler for Notifications
        /// </summary>
        public UIHandler()
        {
            // Initialize notification lists
            _Notifications = new List<Notification>[BuiltInComponents.Host.ScreenCount];
            for (int i = 0; i < _Notifications.Length; i++)
                _Notifications[i] = new List<Notification>();
            _QueuedNotifications = new List<Notification>();

            _IconContainer = new OMContainer("StatusBarHandler_IconContainer", 0, 0, 1000, 40);
            _IconContainer.ScrollBar_Horizontal_Enabled = false;
            _IconContainer.ScrollBar_Vertical_Enabled = false;

            _NotificationList = new OMContainer("StatusBarHandler_DropDown_NotificationList", 0, 0, 1000, 600);
            _DropDown_ButtonStripContainer = new ButtonStripContainer();

            _PopUpMenu_ButtonStripContainer = new ButtonStripContainer();
            _PopUpMenu_ButtonStripContainer_OnButtonStripSet_Handler = new ButtonStripContainer.ButtonStripChangedEventHandler(PopUpMenu_ButtonStripContainer_OnButtonStripSet);
            _PopUpMenu_ButtonStripContainer.OnButtonStripSet += _PopUpMenu_ButtonStripContainer_OnButtonStripSet_Handler;

            _ControlButtons_ButtonStripContainer = new ButtonStripContainer();
            _ControlButtons_ButtonStripContainer_OnButtonStripSet_Handler = new ButtonStripContainer.ButtonStripChangedEventHandler(ControlButtons_ButtonStripContainer_OnButtonStripSet);
            _ControlButtons_ButtonStripContainer.OnButtonStripSet += _ControlButtons_ButtonStripContainer_OnButtonStripSet_Handler;

            _tmrControlButtonsAutoHide = new Timer[BuiltInComponents.Host.ScreenCount];
            _tmrMediaBannerAutoHide = new Timer[BuiltInComponents.Host.ScreenCount];

            _MediaBanner_Enable = new bool[BuiltInComponents.Host.ScreenCount];
            for (int i = 0; i < _MediaBanner_Enable.Length; i++)
                _MediaBanner_Enable[i] = true;

            BuiltInComponents.Host.OnSystemEvent += new SystemEvent(Host_OnSystemEvent);
        }

        public void Dispose()
        {
            for (int i = 0; i < _tmrMediaBannerAutoHide.Length; i++)
            {
                if (_tmrMediaBannerAutoHide[i] != null)
                    _tmrMediaBannerAutoHide[i].Dispose();
            }
            for (int i = 0; i < _tmrControlButtonsAutoHide.Length; i++)
            {
                if (_tmrControlButtonsAutoHide[i] != null)
                    _tmrControlButtonsAutoHide[i].Dispose();
            }
        }

        ~UIHandler()
        {
            Dispose();
        }

        #endregion

        #region host events

        void Host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.TransitionToPanel)
            {
                OpenMobile.Threading.SafeThread.Asynchronous(() =>
                {
                    System.Threading.Thread.Sleep(500);
                    OM.Host.ForEachScreen(delegate(int screen)
                        {
                            Notifications_Enable(screen);
                        });
                });
            }

            else if (function == eFunction.TransitionFromPanel)
            {   // Clear popup menu when the panel that loaded it is unloaded
                int screen = 0;
                if (args != null && args.Length >= 3)
                {
                    if (int.TryParse(args[0] as string, out screen))
                    {
                        ButtonStrip strip = _PopUpMenu_ButtonStripContainer.GetButtonStrip(screen);
                        if (strip != null)
                        {
                            if (args[1] as string == strip.PluginName && args[2] as string == strip.PanelName)
                                BuiltInComponents.Host.UIHandler.PopUpMenu.ClearButtonStrip(screen);
                        }

                        // Reenable the media banner by default when leaving a panel
                        MediaBanner_Enable(screen);
                    }
                }
            }
            else if (function == eFunction.TransitionFromAny)
            {   // Clear popup menu when going home
                int screen = 0;
                if (args != null && args.Length >= 1)
                {
                    if (int.TryParse(args[0] as string, out screen))
                    {
                        BuiltInComponents.Host.UIHandler.PopUpMenu.ClearButtonStrip(screen);

                        // Reenable the media banner by default when leaving a panel
                        MediaBanner_Enable(screen);
                    }
                }
            }
        }

        #endregion

        #region Icon container

        /// <summary>
        /// A OMContainer for notification icons
        /// </summary>
        public OMContainer StatusBarIconsContainer
        {
            get
            {
                return _IconContainer;
            }
        }
        private OMContainer _IconContainer = null;
        private string _IconContainerItemName = "StatusBarHandler_IconContainer_Icon";

        private void IconContainer_UpdateNotificationIcons(int screen)
        {
            // Lock to prevent multiple icons being shown
            lock (this)
            {
                // Find icons to show
                bool InfoIconShow = (_Notifications[screen].Find(x => (x.Style == Notification.Styles.Normal && x.IconStatusBar == null)) != null);
                bool WarnIconShow = (_Notifications[screen].Find(x => (x.Style == Notification.Styles.Warning && x.IconStatusBar == null)) != null);

                // Get screen specific container (to support multiscreens)
                OMContainer ScreenSpecific_IconContainer = null;
                if (_IconContainer.Parent != null)
                    ScreenSpecific_IconContainer = (OMContainer)_IconContainer.GetControlAtScreen(screen);

                // Show info icon
                string InfoIconName = "UITopBarInfoIcon";
                // Check if icon is already shown
                ControlGroup Icon = ScreenSpecific_IconContainer[String.Format("{0}{1}", _IconContainerItemName, InfoIconName)];
                bool InfoIconPresent = (Icon != null);
                if (InfoIconShow & !InfoIconPresent)
                {   // Show icon
                    OMImage Image_NotificationIcon = new OMImage(_IconContainerItemName, 0, 0, ScreenSpecific_IconContainer.Region.Height, ScreenSpecific_IconContainer.Region.Height, BuiltInComponents.Host.getSkinImage("Icons|Icon-Info"));
                    Image_NotificationIcon.Name += InfoIconName;
                    if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                        Image_NotificationIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                    Image_NotificationIcon.Opacity = 178;
                    ScreenSpecific_IconContainer.addControl(Image_NotificationIcon, ControlDirections.Left);
                }
                else if (!InfoIconShow & InfoIconPresent)
                {   // Hide icon
                    ScreenSpecific_IconContainer.RemoveControl(Icon);
                }

                // Show warning icon
                string WarnIconName = "UITopBarWarnIcon";
                // Check if icon is already shown
                Icon = ScreenSpecific_IconContainer[String.Format("{0}{1}", _IconContainerItemName, WarnIconName)];
                bool WarnIconPresent = (Icon != null);
                if (WarnIconShow & !WarnIconPresent)
                {   // Show icon
                    OMImage Image_NotificationIcon = new OMImage(_IconContainerItemName, 0, 0, ScreenSpecific_IconContainer.Region.Height, ScreenSpecific_IconContainer.Region.Height, BuiltInComponents.Host.getSkinImage("AIcons|11-alerts-and-states-warning"));
                    Image_NotificationIcon.Name += WarnIconName;
                    if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                        Image_NotificationIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                    else
                        // Overlay icon with a yellow warning color
                        Image_NotificationIcon.Image.image.Overlay(Color.Yellow); 
                    Image_NotificationIcon.Opacity = 178;
                    ScreenSpecific_IconContainer.addControl(Image_NotificationIcon, ControlDirections.Left);
                }
                else if (!WarnIconShow & WarnIconPresent)
                {   // Hide icon
                    ScreenSpecific_IconContainer.RemoveControl(Icon);
                }
            }
        }

        #endregion

        #region Notifications

        List<Notification> _QueuedNotifications = null;
        bool _NotificationsEnabled = false;

        /// <summary>
        /// Enables notifications (any queued notifications will be shown)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="fast"></param>
        public void Notifications_Enable(int screen)
        {
            _NotificationsEnabled = true;
            // Add any queued notifications
            lock (_QueuedNotifications)
            {
                while (_QueuedNotifications.Count > 0)
                {
                    Notification notification = _QueuedNotifications[0];
                    _QueuedNotifications.RemoveAt(0);
                    if (notification.Screen >= 0)
                        AddNotification(notification.Screen, notification);
                    else
                        AddNotification(notification);
                }
            }
        }

        /// <summary>
        /// Disables notifications (incoming notifications will be queued)
        /// </summary>
        /// <param name="screen"></param>
        public void Notifications_Disable(int screen)
        {
            _NotificationsEnabled = false;
        }
        
        #region Notification list and container

        /// <summary>
        ///Get a list of available notifications
        /// </summary>
        public List<Notification> GetNotifications(int screen)
        {
            return _Notifications[screen];
        }
        private List<Notification>[] _Notifications = null;

        /// <summary>
        /// A OMContainer for notification list
        /// </summary>
        public OMContainer NotificationListControl
        {
            get
            {
                return _NotificationList;
            }
        }
        private OMContainer _NotificationList = null;

        private ControlGroup NotificationList_CreateItem(Notification notification)
        {
            ControlGroup ItemBase = new ControlGroup();
            ItemBase.Key = notification.FullID;

            OMButton Button_ListItem = new OMButton("Button_ListItem", 0, 0, 1000, 64);
            Button_ListItem.Name += notification.FullID;
            Button_ListItem.BackgroundColor = Color.Transparent;
            //Button_ListItem.FocusImage = new imageItem(Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor), Button_ListItem.Region.Width, Button_ListItem.Region.Height);

            // List item background
            OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            gd.BackgroundColor1 = Color.Transparent;
            gd.BorderColor = Color.Transparent;
            gd.Width = Button_ListItem.Width;
            gd.Height = Button_ListItem.Height;
            gd.CornerRadius = 0;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            Button_ListItem.DownImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            Button_ListItem.FocusImage = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            Button_ListItem.OnClick += new userInteraction(Notification_Button_OnClick);
            Button_ListItem.Tag = notification;
            ItemBase.Add(Button_ListItem);

            OMBasicShape Shape_ListItem_Separator = new OMBasicShape("Shape_ListItem_Separator", 0, 64, 1000, 1, 
                new ShapeData(shapes.Rectangle, Color.FromArgb(50, Color.White)));
            Shape_ListItem_Separator.Name += notification.FullID;
            Shape_ListItem_Separator.NoUserInteraction = true;
            ItemBase.Add(Shape_ListItem_Separator);

            OMImage Image_ListItem_Icon = new OMImage("Image_ListItem_Icon", 0, 0, 64, 64);
            Image_ListItem_Icon.Name += notification.FullID;
            if (notification.Icon != null)
                Image_ListItem_Icon.Image = new imageItem(notification.Icon);
            else
            {
                switch (notification.Style)
                {
                    case Notification.Styles.Normal:
                        {   // Use default info icon
                            Image_ListItem_Icon.Image = BuiltInComponents.Host.getSkinImage("Icons|Icon-Info");
                        }
                        break;  
                    case Notification.Styles.IconOnly:
                        break; // No icon
                    case Notification.Styles.Warning:
                        {   // Use default warning icon
                            Image_ListItem_Icon.Image = BuiltInComponents.Host.getSkinImage("AIcons|11-alerts-and-states-warning");
                        }
                        break;
                    default:
                        break;
                }
            }
            //Image_ListItem_Icon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
            Image_ListItem_Icon.NoUserInteraction = true;
            ItemBase.Add(Image_ListItem_Icon);

            OMLabel Label_ListItem_Header = new OMLabel("Label_ListItem_Header", Image_ListItem_Icon.Region.Right, 5, 836, 28);
            Label_ListItem_Header.Name += notification.FullID;
            Label_ListItem_Header.Text = notification.Header;
            Label_ListItem_Header.NoUserInteraction = true;
            Label_ListItem_Header.Font = new Font(Font.GenericSansSerif, 20F);
            Label_ListItem_Header.TextAlignment = Alignment.CenterLeft;
            Label_ListItem_Header.Format = eTextFormat.Normal;
            ItemBase.Add(Label_ListItem_Header);

            OMLabel Label_ListItem_TimeStamp = new OMLabel("Label_ListItem_TimeStamp", Label_ListItem_Header.Region.Right, Label_ListItem_Header.Region.Top, 1000 - Label_ListItem_Header.Region.Right, 20);
            Label_ListItem_TimeStamp.Name += notification.FullID;
            Label_ListItem_TimeStamp.Text = notification.TimeStampText;
            Label_ListItem_TimeStamp.NoUserInteraction = true;
            Label_ListItem_TimeStamp.Font = new Font(Font.GenericSansSerif, 16F);
            Label_ListItem_TimeStamp.TextAlignment = Alignment.CenterCenter;
            Label_ListItem_TimeStamp.Format = eTextFormat.Normal;
            Label_ListItem_TimeStamp.Opacity = 178;
            ItemBase.Add(Label_ListItem_TimeStamp);

            OMLabel Label_ListItem_Description = new OMLabel("Label_ListItem_Description", Image_ListItem_Icon.Region.Right + 2, Label_ListItem_Header.Region.Bottom, 934, 30);
            Label_ListItem_Description.Name += notification.FullID;
            Label_ListItem_Description.Text = notification.Text;
            Label_ListItem_Description.NoUserInteraction = true;
            Label_ListItem_Description.Font = new Font(Font.GenericSansSerif, 16F);
            Label_ListItem_Description.TextAlignment = Alignment.CenterLeft;
            Label_ListItem_Description.Format = eTextFormat.Normal;
            Label_ListItem_Description.Opacity = 178;
            ItemBase.Add(Label_ListItem_Description);

            // Set placement of header text if there is no description text available
            //if (String.IsNullOrEmpty(Label_ListItem_Description.Text))
            //    Label_ListItem_Header.Top += 15;

            return ItemBase;
        }

        /// <summary>
        /// Handle when a user clicks the notification message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="screen"></param>
        void Notification_Button_OnClick(OMControl sender, int screen)
        {
            // Extract notification
            Notification notification = sender.Tag as Notification;
            if (notification == null)
                return;

            // hide dropdown if a click action is defined
            if (notification.HasClickAction)
                DropDown_Hide(screen, true);

            // Execute notification delegates
            bool cancel = false;
            notification.RaiseClickAction(screen, ref cancel);

            if (!cancel)
            {
                // Remove notification from this list
                RemoveNotification(screen, notification);
            }
        }

        #endregion

        #region Add Notification

        /// <summary>
        /// Adds a new notification to the list of active notifications (all screens)
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public string AddNotification(Notification notification)
        {
            // Do we need to queue notifications?
            if (!_NotificationsEnabled)
            {
                notification.Screen = -1;
                _QueuedNotifications.Add(notification);
                return notification.ID;
            }

            BuiltInComponents.Host.ForEachScreen(delegate(int screen)
            {
                AddNotification_Internal(screen, notification, true);
            });
            return notification.ID;
        }
        /// <summary>
        /// Adds a new notification to the list of active notifications (specific screen)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public string AddNotification(int screen, Notification notification)
        {
            // Do we need to queue notifications?
            if (!_NotificationsEnabled)
            {
                notification.Screen = screen;
                _QueuedNotifications.Add(notification);
                return notification.ID;
            }

            return AddNotification_Internal(screen, notification, false);
        }

        /// <summary>
        /// Adds a new notification to the list of active notifications (specific screen)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="notification"></param>
        /// <param name="Global"></param>
        /// <returns></returns>
        private string AddNotification_Internal(int screen, Notification notification, bool Global)
        {
            notification.Global = Global;
            notification.Screen = screen;

            _Notifications[screen].Add(notification);

            notification.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(notification_PropertyChanged);

            // TODO: Add queue for notifications

            // Get screen specific container (to support multiscreens)
            OMContainer ScreenSpecific_NotificationList = null;
            if (_NotificationList.Parent != null)
                ScreenSpecific_NotificationList = (OMContainer)_NotificationList.GetControlAtScreen(screen);

            // Get screen specific container (to support multiscreens)
            OMContainer ScreenSpecific_IconContainer = null;
            if (_IconContainer.Parent != null)
                ScreenSpecific_IconContainer = (OMContainer)_IconContainer.GetControlAtScreen(screen);

            if (ScreenSpecific_IconContainer == null || ScreenSpecific_NotificationList == null)
                return "";

            lock (ScreenSpecific_NotificationList)
            {
                lock (ScreenSpecific_IconContainer)
                {
                    switch (notification.Style)
                    {
                        case Notification.Styles.Normal:
                        case Notification.Styles.Warning:
                            {
                                // Add notification to notification list
                                ScreenSpecific_NotificationList.addControl(NotificationList_CreateItem(notification), ControlDirections.Down);
                            }
                            break;
                        case Notification.Styles.IconOnly:
                            {
                                // Add notification icon to Icon container
                                if (notification.Icon != null)
                                {
                                    int iconHeight = _IconContainer.Region.Height;
                                    if (notification.IconSize_Height > 0)
                                        iconHeight = notification.IconSize_Height;
                                    int iconWidth = _IconContainer.Region.Height;
                                    if (notification.IconSize_Width > 0)
                                        iconWidth = notification.IconSize_Width;

                                    OMImage Image_NotificationIcon = new OMImage(_IconContainerItemName, 0, 0, iconWidth, iconHeight, new imageItem(notification.Icon, notification.ID.ToString()));
                                    Image_NotificationIcon.Name = String.Format("{0}{1}{2}", _IconContainerItemName, notification.OwnerPlugin.pluginName, notification.ID.ToString());
                                    if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                                        Image_NotificationIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                                    Image_NotificationIcon.Opacity = 178;
                                    ScreenSpecific_IconContainer.addControl(Image_NotificationIcon, ControlDirections.Left);
                                }
                            }
                            break;
                    }

                    // Add notification statusbar icon to Icon container if present
                    if (notification.IconStatusBar != null)
                    {

                        int iconHeight = _IconContainer.Region.Height;
                        if (notification.IconSize_Height > 0)
                            iconHeight = notification.IconSize_Height;
                        int iconWidth = _IconContainer.Region.Height;
                        if (notification.IconSize_Width > 0)
                            iconWidth = notification.IconSize_Width;

                        OMImage Image_NotificationIcon = new OMImage(_IconContainerItemName, 0, 0, iconWidth, iconHeight, new imageItem(notification.IconStatusBar, notification.ID.ToString()));
                        Image_NotificationIcon.Name = String.Format("{0}{1}{2}", _IconContainerItemName, notification.OwnerPlugin.pluginName, notification.ID.ToString());
                        if (BuiltInComponents.SystemSettings.UseIconOverlayColor)
                            Image_NotificationIcon.Image.image.Overlay(BuiltInComponents.SystemSettings.SkinTextColor); // Support overlay of skin colors
                        Image_NotificationIcon.Opacity = 178;
                        ScreenSpecific_IconContainer.addControl(Image_NotificationIcon, ControlDirections.Left);
                    }

                    // Show / Hide statusbar notification icons
                    IconContainer_UpdateNotificationIcons(screen);
                }
            }
            return notification.ID;

        }

        #endregion

        #region Update notification

        void notification_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateNotification((Notification)sender);
        }

        /// <summary>
        /// Updates the contents of an existing notification
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        private void UpdateNotification(IBasePlugin OwnerPlugin, string ID)
        {
            BuiltInComponents.Host.ForEachScreen(delegate(int screen)
            {
                // Check if this is a queued notification
                Notification notification = _QueuedNotifications.Find(x => x.OwnerPlugin == OwnerPlugin && x.ID == ID);
                if (notification != null)
                    return;

                // Get notification to update
                notification = _Notifications[screen].Find(x => x.OwnerPlugin == OwnerPlugin && x.ID == ID);
                if (notification == null)
                    return;

                UpdateNotification_Internal(screen, notification);

                // Don't do any more updates if this is not a global notification
                if (!notification.Global)
                    return;
            });
        }
        
        /// <summary>
        /// Updates the contents of an existing notification
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        private void UpdateNotification(int screen, IBasePlugin OwnerPlugin, string ID)
        {
            // Check if this is a queued notification
            Notification notification = _QueuedNotifications.Find(x => x.OwnerPlugin == OwnerPlugin && x.ID == ID);
            if (notification != null)
                return;
            
            // Get notification to update
            notification = _Notifications[screen].Find(x => x.OwnerPlugin == OwnerPlugin && x.ID == ID);
            if (notification == null)
                return;

            // If this is a global notification then remove all instances
            if (notification.Global)
                UpdateNotification(OwnerPlugin, ID);
            else
                // Remove a single instance
                UpdateNotification_Internal(screen, notification);
            
        }

        /// <summary>
        /// Updates the contents of an existing notification
        /// </summary>
        /// <param name="notification"></param>
        private void UpdateNotification(Notification notification)
        {
            if (notification == null)
                return;

            // Check if this is a queued notification
            Notification queuedNotification = _QueuedNotifications.Find(x => x.OwnerPlugin == notification.OwnerPlugin && x.ID == notification.ID);
            if (queuedNotification != null)
                return;

            // If this is a global notification then remove all instances
            if (notification.Global)
                UpdateNotification(notification.OwnerPlugin, notification.ID);
            else
                // Remove a single instance
                UpdateNotification_Internal(notification.Screen, notification);

        }

        /// <summary>
        /// Internal: Updates the contents of an existing notification
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="notification"></param>
        private void UpdateNotification_Internal(int screen, Notification notification)
        {
            // Get screen specific container (to support multiscreens)
            OMContainer ScreenSpecific_IconContainer = null;
            if (_IconContainer.Parent != null)
                ScreenSpecific_IconContainer = (OMContainer)_IconContainer.GetControlAtScreen(screen);
            OMContainer ScreenSpecific_NotificationList = null;
            if (_NotificationList.Parent != null)
                ScreenSpecific_NotificationList = (OMContainer)_NotificationList.GetControlAtScreen(screen);

            // Error check
            if (ScreenSpecific_IconContainer == null || ScreenSpecific_NotificationList == null)
                return;

            // Also update corresponding controlgroup for the notification list
            ControlGroup NotificationCG = ScreenSpecific_NotificationList[String.Format("{0}{1}", "Label_ListItem_Description", notification.FullID)];
            if (NotificationCG != null)
            {
                if (notification.Style == Notification.Styles.Normal || notification.Style == Notification.Styles.Warning)
                {
                    // Header text
                    OMLabel lblHeader = NotificationCG[String.Format("{0}{1}", "Header", notification.FullID)] as OMLabel;
                    if (lblHeader.Text != notification.Header)
                    {
                        lblHeader.Text = notification.Header;
                        // Set placement of header text if there is no description text available
                        if (String.IsNullOrEmpty(notification.Text))
                            lblHeader.Top += 15;
                    }

                    // Description text
                    OMLabel lblText = NotificationCG[String.Format("{0}{1}", "Description", notification.FullID)] as OMLabel;
                    if (lblText.Text != notification.Text)
                        lblText.Text = notification.Text;

                    // Icon
                    OMImage imgIcon = NotificationCG[String.Format("{0}{1}", "Icon", notification.FullID)] as OMImage;
                    if (imgIcon.Image.image != notification.Icon)
                        imgIcon.Image = new imageItem(notification.Icon);

                    // Timestamp
                    OMLabel lblTS = NotificationCG[String.Format("{0}{1}", "TimeStamp", notification.FullID)] as OMLabel;
                    if (lblTS.Text != notification.TimeStampText)
                        lblTS.Text = notification.TimeStampText;
                }
                else
                {   // Remove notification item from list
                    ScreenSpecific_NotificationList.RemoveControl(NotificationCG);
                    IconContainer_UpdateNotificationIcons(screen);
                }
            }

            // Update corresponding controlgroup for the statusbar list
            string StatusBarIconName = String.Format("{0}{1}{2}", _IconContainerItemName, notification.OwnerPlugin.pluginName, notification.ID.ToString());
            ControlGroup StatusBarCG = ScreenSpecific_IconContainer[StatusBarIconName];
            if (StatusBarCG != null)
            {
                // Icon
                OMImage imgIcon = StatusBarCG[StatusBarIconName] as OMImage;

                if (notification.IconStatusBar == null)
                {
                    // Remove this notification icon
                    ScreenSpecific_IconContainer.RemoveControl(StatusBarCG);
                    IconContainer_UpdateNotificationIcons(screen);
                }
                else
                {
                    if (imgIcon.Image.image != notification.IconStatusBar)
                        imgIcon.Image = new imageItem(notification.IconStatusBar);
                }
            }

        }

        #endregion

        #region Remove notification

        /// <summary>
        /// Removes a notification from the list of active notifications (all screens)
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool RemoveNotification(IBasePlugin OwnerPlugin, string ID)
        {
            BuiltInComponents.Host.ForEachScreen(delegate(int screen)
            {
                RemoveNotification(screen, OwnerPlugin, ID);
            });
            return true;
        }
        /// <summary>
        /// Removes a notification from the list of active notifications (spesific screen)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="OwnerPlugin"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool RemoveNotification(int screen, IBasePlugin OwnerPlugin, string ID)
        {
            List<Notification> NotificationsToRemove = _Notifications[screen].FindAll(x => (x.ID == ID && x.OwnerPlugin == OwnerPlugin));

            //int i = _Notifications.RemoveAll(x => (x.ID == ID && x.OwnerPlugin == OwnerPlugin));
            for (int i = 0; i < NotificationsToRemove.Count; i++)
            {
                Notification notification = NotificationsToRemove[i];
                RemoveNotification(screen, notification);
            }

            // Show / Hide statusbar notification icons
            IconContainer_UpdateNotificationIcons(screen);

            return true;
        }

        /// <summary>
        /// Removes a notification from the list of active notifications
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="removeAllStates"></param>
        /// <returns></returns>
        public bool RemoveNotification(Notification notification, bool removeAllStates)
        {
            // If this is a global notification then remove all instances
            if (notification.Global)
            {
                BuiltInComponents.Host.ForEachScreen(delegate(int Screen)
                {
                    RemoveNotification_Internal(Screen, notification, removeAllStates);
                });
                return true;
            }
            else
                // Remove a single instance
                return RemoveNotification_Internal(notification.Screen, notification, false);
        }

        /// <summary>
        /// Removes a notification from the list of active notifications (one screen)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public bool RemoveNotification(int screen, Notification notification)
        {
            // If this is a global notification then remove all instances
            if (notification.Global)
            {
                BuiltInComponents.Host.ForEachScreen(delegate(int Screen)
                {
                    RemoveNotification_Internal(Screen, notification, false);
                });
                return true;
            }
            else
                // Remove a single instance
                return RemoveNotification_Internal(screen, notification, false);
        }
        /// <summary>
        /// Removes a notification from the list of active notifications (one screen)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="notification"></param>
        /// <param name="RemoveAllStates"></param>
        /// <returns></returns>
        private bool RemoveNotification_Internal(int screen, Notification notification, bool RemoveAllStates)
        {
            try
            {
                // Execute notification delegates (action delegates are always executed regardless of state)
                bool cancel = false;
                notification.RaiseClearAction(screen, ref cancel);
                if (cancel)
                    return false;

                // Don't remove an active notification
                if (!RemoveAllStates)
                    if (notification.State != Notification.States.Passive)
                        return false;

                bool result = _Notifications[screen].Remove(notification);

                // Get screen specific container (to support multiscreens)
                OMContainer ScreenSpecific_IconContainer = null;
                if (_IconContainer.Parent != null)
                    ScreenSpecific_IconContainer = (OMContainer)_IconContainer.GetControlAtScreen(screen);
                OMContainer ScreenSpecific_NotificationList = null;
                if (_NotificationList.Parent != null)
                    ScreenSpecific_NotificationList = (OMContainer)_NotificationList.GetControlAtScreen(screen);

                ScreenSpecific_IconContainer.RemoveControl(String.Format("{0}{1}{2}", _IconContainerItemName, notification.OwnerPlugin.pluginName, notification.ID.ToString()));
                ScreenSpecific_NotificationList.RemoveControlByKey(notification.FullID);
                notification.Dispose();

                // Show / Hide statusbar notification icons
                IconContainer_UpdateNotificationIcons(screen);

                // Execute notification delegate
                //notification.RaiseClearAction(screen);

                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes all notifications that belongs to the specified plugin (all screens)
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <returns></returns>
        public bool RemoveAllMyNotifications(IBasePlugin OwnerPlugin)
        {
            BuiltInComponents.Host.ForEachScreen(delegate(int screen)
            {
                RemoveAllMyNotifications(screen, OwnerPlugin, true);
            });
            return true;
        }
        /// <summary>
        /// Removes all notifications that belongs to the specified plugin at the specified screen
        /// </summary>
        /// <param name="OwnerPlugin"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool RemoveAllMyNotifications(IBasePlugin OwnerPlugin, int screen)
        {
            return RemoveAllMyNotifications(screen, OwnerPlugin, true);
        }       
        /// <summary>
        /// Removes all notifications that belongs to the specified plugin (specific screen)
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="OwnerPlugin"></param>
        /// <param name="RemoveAllStates"></param>
        /// <returns></returns>
        private bool RemoveAllMyNotifications(int screen, IBasePlugin OwnerPlugin, bool RemoveAllStates)
        {
            // removed queued notifications
            List<Notification> queudNotificationsToRemove = _QueuedNotifications.FindAll(x => x.OwnerPlugin == OwnerPlugin);
            for (int i = 0; i < queudNotificationsToRemove.Count; i++)
            {
                Notification notification = queudNotificationsToRemove[i];
                _QueuedNotifications.Remove(notification);
            }
            
            List<Notification> NotificationsToRemove = _Notifications[screen].FindAll(x => x.OwnerPlugin == OwnerPlugin);
            for (int i = 0; i < NotificationsToRemove.Count; i++)
            {
                Notification notification = NotificationsToRemove[i];
                RemoveNotification_Internal(screen, notification, RemoveAllStates);
            }

            // Show / Hide statusbar notification icons
            IconContainer_UpdateNotificationIcons(screen);

            return true;
        }

        /// <summary>
        /// Removes all notifications
        /// </summary>
        /// <returns></returns>
        public bool RemoveAllNotifications()
        {
            BuiltInComponents.Host.ForEachScreen(delegate(int screen)
            {
                RemoveAllNotifications_Internal(screen);
                _QueuedNotifications.Clear();

            });
            return true;
        }

        /// <summary>
        /// Removes all notifications from a specific screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="RemoveAllStates"></param>
        /// <returns></returns>
        public bool RemoveAllNotifications(int screen, bool RemoveAllStates)
        {
            _QueuedNotifications.Clear();
            List<Notification> NotificationsToRemove = _Notifications[screen].FindAll(x => x.State == Notification.States.Passive);
            for (int i = 0; i < NotificationsToRemove.Count; i++)
            {
                Notification notification = NotificationsToRemove[i];

                // If this is a global notification then remove all instances
                if (notification.Global)
                    RemoveNotification(notification, RemoveAllStates);
                else
                    // Remove a single instance
                    RemoveNotification_Internal(screen, notification, RemoveAllStates);
            }
            return true;
        }

        /// <summary>
        /// Removes all notifications from a specific screen
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public bool RemoveAllNotifications_Internal(int screen)
        {
            for (int i = 0; i < _Notifications[screen].Count; i++)
            {
                Notification notification = _Notifications[screen][i];
                RemoveNotification_Internal(screen, notification, false);
            }
            return true;
        }

        #endregion

        /// <summary>
        /// The default size of icons on the statusbar
        /// </summary>
        public Size StatusBar_DefaultIconSize
        {
            get
            {
                _StatusBar_DefaultIconSize.Width = _IconContainer.Region.Height;
                _StatusBar_DefaultIconSize.Height = _IconContainer.Region.Height;
                return _StatusBar_DefaultIconSize;
            }
        }
        private Size _StatusBar_DefaultIconSize = new Size();

        #endregion

        #region InfoBanner

        public delegate void ShowInfoBannerDelegate(int screen, InfoBanner bannerData);
        public delegate void HideInfoBannerDelegate(int screen);

        /// <summary>
        /// Event is raised when the infobanner is requested to hide
        /// </summary>
        public event HideInfoBannerDelegate OnHideInfoBanner;
        private void Raise_OnHideInfoBanner(int screen)
        {
            if (OnHideInfoBanner != null)
                OnHideInfoBanner(screen);
        }

        /// <summary>
        /// Event is raised when the infobanner is requested to show
        /// </summary>
        public event ShowInfoBannerDelegate OnShowInfoBanner;
        private void Raise_OnShowInfoBanner(int screen, InfoBanner bannerData)
        {
            if (OnShowInfoBanner != null)
                OnShowInfoBanner(screen, bannerData);
        }

        /// <summary>
        /// Show the infobanner
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="bannerData"></param>
        public void InfoBanner_Show(int screen, InfoBanner bannerData)
        {
            // Call event
            Raise_OnShowInfoBanner(screen, bannerData);
        }

        /// <summary>
        /// Hide the infobanner 
        /// </summary>
        /// <param name="screen"></param>
        public void InfoBanner_Hide(int screen)
        {
            // Call event
            Raise_OnHideInfoBanner(screen);
        }

        #endregion

        #region MediaBanner

        private Timer[] _tmrMediaBannerAutoHide;
        public delegate void ShowMediaBannerDelegate(int screen, bool fast);
        public delegate void HideMediaBannerDelegate(int screen, bool fast);
        public delegate void EnableMediaBannerDelegate(int screen, bool fast);

        /// <summary>
        /// Event is raised when the Mediabanner is requested to hide
        /// </summary>
        public event HideMediaBannerDelegate OnHideMediaBanner;
        private void Raise_OnHideMediaBanner(int screen, bool fast)
        {
            if (OnHideMediaBanner != null)
                OnHideMediaBanner(screen, fast);
        }

        /// <summary>
        /// Event is raised when the Mediabanner is requested to show
        /// </summary>
        public event ShowMediaBannerDelegate OnShowMediaBanner;
        private void Raise_OnShowMediaBanner(int screen, bool fast)
        {
            if (OnShowMediaBanner != null)
                OnShowMediaBanner(screen, fast);
        }

        /// <summary>
        /// Event is raised when the Mediabanner is enabled
        /// </summary>
        public event EnableMediaBannerDelegate OnEnableMediaBanner;
        private void Raise_OnEnableMediaBanner(int screen, bool fast)
        {
            if (OnEnableMediaBanner != null)
                OnEnableMediaBanner(screen, fast);
        }

        public void MediaBanner_AutoHideTimer_Reset(int screen)
        {
            if (_tmrMediaBannerAutoHide.Length <= screen && screen < 0)
                return;

            if (_tmrMediaBannerAutoHide[screen].Enabled)
            {
                _tmrMediaBannerAutoHide[screen].Enabled = false;
                _tmrMediaBannerAutoHide[screen].Enabled = true;
            }
        }

        /// <summary>
        /// Show the Mediabanner
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="bannerData"></param>
        public void MediaBanner_Show(int screen, bool fast = false, int secondsToShow = 0)
        {
            // Call event
            if (_MediaBanner_Enable[screen])
                Raise_OnShowMediaBanner(screen, fast);

            if (secondsToShow > 0)
            {
                if (_tmrMediaBannerAutoHide[screen] == null)
                {
                    _tmrMediaBannerAutoHide[screen] = new Timer(secondsToShow * 1000);
                    _tmrMediaBannerAutoHide[screen].Elapsed += _tmrMediaBannerAutoHide_Elapsed;
                    _tmrMediaBannerAutoHide[screen].AutoReset = false;
                }
                _tmrMediaBannerAutoHide[screen].Interval = secondsToShow * 1000;
                _tmrMediaBannerAutoHide[screen].Screen = screen;
                _tmrMediaBannerAutoHide[screen].Tag = fast;
                _tmrMediaBannerAutoHide[screen].Enabled = true;
            }
        }

        void _tmrMediaBannerAutoHide_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer tmr = sender as Timer;
            tmr.Enabled = false;
            Raise_OnHideMediaBanner(tmr.Screen, (bool)tmr.Tag);
        }

        /// <summary>
        /// Hide the Mediabanner 
        /// </summary>
        /// <param name="screen"></param>
        public void MediaBanner_Hide(int screen, bool fast = false)
        {
            // Call event
            Raise_OnHideMediaBanner(screen, fast);
        }

        private bool[] _MediaBanner_Enable;

        public bool MediaBanner_IsEnabled(int screen)
        {
            return _MediaBanner_Enable[screen];
        }

        public void MediaBanner_Enable(int screen, bool fast = false)
        {
            if (screen < 0 || screen >= _MediaBanner_Enable.Length)
                return;

            _MediaBanner_Enable[screen] = true;
            Raise_OnEnableMediaBanner(screen, fast);
        }
        public void MediaBanner_Disable(int screen, bool fast = false)
        {
            if (screen < 0 || screen >= _MediaBanner_Enable.Length)
                return;

            _MediaBanner_Enable[screen] = false;
            MediaBanner_Hide(screen, fast);
        }

        

        #endregion

        #region TopBar InfoText (infobar)

        public delegate void ShowInfoBarDelegate(int screen, InfoBar barData);
        public delegate void HideInfoBarDelegate(int screen);

        /// <summary>
        /// Event is raised when the infobar is requested to hide
        /// </summary>
        public event HideInfoBarDelegate OnHideInfoBar;
        private void Raise_OnHideInfoBar(int screen)
        {
            if (OnHideInfoBar != null)
                OnHideInfoBar(screen);
        }

        /// <summary>
        /// Event is raised when the infobar is requested to show
        /// </summary>
        public event ShowInfoBarDelegate OnShowInfoBar;
        private void Raise_OnShowInfoBar(int screen, InfoBar barData)
        {
            if (OnShowInfoBar != null)
                OnShowInfoBar(screen, barData);
        }

        /// <summary>
        /// Show the infobar
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="bannerData"></param>
        public void InfoBar_Show(int screen, InfoBar barData)
        {
            // Call event
            Raise_OnShowInfoBar(screen, barData);
        }

        /// <summary>
        /// Hide the infobar 
        /// </summary>
        /// <param name="screen"></param>
        public void InfoBar_Hide(int screen)
        {
            // Call event
            Raise_OnHideInfoBar(screen);
        }

        #endregion

    }
}
