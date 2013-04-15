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
using System.Threading;
using OpenMobile.Plugin;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using OpenMobile.Framework;

namespace OpenMobile.helperFunctions.MenuObjects
{
    public class MenuData
    {
        public MenuData()
        {
            FontSize = 45F;
        }
        
        public string Header { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public float FontSize { get; set; }
        public ScreenManager Manager { get; set; }
        public List<OMListItem> items = new List<OMListItem>();
    }

    /// <summary>
    /// A menu popup displayed as a list of menu options
    /// </summary>
    public class MenuPopup
    {

        private string Handler = "OMMenu";
        //private string OwnerPlugin, OwnerPanel, OwnerScreen;
        private string[] PanelName = null;
        private MenuData _ConfigData = new MenuData();
        //private OMPanel Panel = null;
        private EventWaitHandle[] MenuSelection = null;
        private int ResultIndex = -1;
        private OMListItem ResultItem = null;

        #region Properties

        /// <summary>
        /// Header
        /// </summary>
        public string Header
        {
            get
            {
                return _ConfigData.Header;
            }
            set
            {
                _ConfigData.Header = value;
            }
        }

        /// <summary>
        /// Left position (absolute)
        /// </summary>
        public int Left
        {
            get
            {
                return _ConfigData.Left;
            }
            set
            {
                _ConfigData.Left = value;
            }
        }

        /// <summary>
        /// Top position (absolute)
        /// </summary>
        public int Top
        {
            get
            {
                return _ConfigData.Top;
            }
            set
            {
                _ConfigData.Top = value;
            }
        }

        /// <summary>
        /// Height of dialog
        /// </summary>
        public int Height
        {
            get
            {
                return _ConfigData.Height;
            }
            set
            {
                _ConfigData.Height = value;
            }
        }

        /// <summary>
        /// Width of dialog
        /// </summary>
        public int Width
        {
            get
            {
                return _ConfigData.Width;
            }
            set
            {
                _ConfigData.Width = value;
            }
        }

        /// <summary>
        /// Font size of text used in menu
        /// </summary>
        public float FontSize
        {
            get
            {
                return _ConfigData.FontSize;
            }
            set
            {
                _ConfigData.FontSize = value;
            }
        }

        /// <summary>
        /// The different data that the menu can return
        /// </summary>
        public enum ReturnTypes 
        { 
            /// <summary>
            /// Returns the zero based index of the selected element
            /// </summary>
            Index, 

            /// <summary>
            /// Returns the tag object of the selected element
            /// </summary>
            Tag,

            /// <summary>
            /// Returns the zero based index and the tag object of the selected element
            /// </summary>
            IndexTag,

            /// <summary>
            /// Returns the full selected item
            /// </summary>
            SelectedItem
        }
        public ReturnTypes ReturnType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the menu popup class
        /// </summary>
        /// <param name="Header"></param>
        public MenuPopup(string Header)
        {
            this.Header = Header;
            Init();
        }
        /// <summary>
        /// Initializes the menu popup class
        /// </summary>
        /// <param name="Header"></param>
        public MenuPopup(string Header, ReturnTypes ReturnType)
        {
            this.Header = Header;
            this.ReturnType = ReturnType;
            Init();
        }
        /// <summary>
        /// Initializes the menu popup class
        /// </summary>
        /// <param name="Header"></param>
        public MenuPopup(string Header, int Left, int Top, int Width, int Height)
        {
            this.Header = Header;
            this.Left = Left;
            this.Top = Top;
            this.Width = Width;
            this.Height = Height;
            Init();
        }
        /// <summary>
        /// Initializes the menu popup class
        /// </summary>
        /// <param name="Header"></param>
        public MenuPopup(string Header, int Left, int Top, int Width, int Height, ReturnTypes ReturnType)
        {
            this.Header = Header;
            this.Left = Left;
            this.Top = Top;
            this.Width = Width;
            this.Height = Height;
            this.ReturnType = ReturnType;
            Init();
        }
        /// <summary>
        /// Initializes the menu popup class
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Handler">Name of plugin that should provide the dialog panel</param>
        public MenuPopup(string Header, string Handler)
        {
            this.Handler = Handler;
            this.Header = Header;
            Init();
        }

        private void Init()
        {
            MenuSelection = new EventWaitHandle[BuiltInComponents.Host.ScreenCount];
            for (int i = 0; i < MenuSelection.Length; i++)
                MenuSelection[i] = new EventWaitHandle(false, EventResetMode.ManualReset);
            PanelName = new string[BuiltInComponents.Host.ScreenCount];
        }
        #endregion

        /// <summary>
        /// Add an item to the menu
        /// </summary>
        /// <param name="item">the item</param>
        public void AddMenuItem(OMListItem item)
        {
            lock (this)
            {
                _ConfigData.items.Add(item);
            }
        }

        private OMPanel CreatePanel(int screen)
        {
            // Create panel
            OMPanel Panel = new OMPanel("");

            // Set panel name
            Panel.Name = String.Format("{0}_{1}", Handler, Panel.GetHashCode());

            PanelName[screen] = Panel.Name;

            // Pack paneldata into tag property
            _ConfigData.Manager = BuiltInComponents.Panels;
            Panel.Tag = _ConfigData;

            if (!BuiltInComponents.Host.sendMessage<OMPanel>(Handler, "OpenMobile.helperFunctions.MenuObjects", "MenuPopup", ref Panel))
            {   // Log this error to the debug log
                BuiltInComponents.Host.DebugMsg("Unable to get MenuPopup panel, plugin " + Handler + " not available");
                return null;
            }

            if (Panel != null)
            {
                Panel.Priority = ePriority.High;
                /* Attach to menu list ( names are set according to this:
                     *  Menu_Button_Cancel      : Cancel button
                     *  Menu_List               : Menu list
                    */

                // Attach to cancel button
                OMButton CancelButton = (OMButton)Panel[Panel.Name + "_Button_Cancel"];
                if (CancelButton != null)
                    CancelButton.OnClick += new userInteraction(CancelButton_OnClick);

                // Attach to menu list
                OMList MenuList = (OMList)Panel["Menu_List"];
                if (MenuList != null)
                {
                    // Add event action
                    MenuList.OnClick += new userInteraction(MenuList_OnClick);

                    // Add menu items
                    MenuList.Items = _ConfigData.items;
                }
            }
            return Panel;
        }

        void  CancelButton_OnClick(OMControl sender, int screen)
        {
            ResultIndex = -1;
            MenuSelection[screen].Set();
        }

        /// <summary>
        /// Shows the menu and returns the zero based index (if not changed by the returntype property) of the selected menu option
        /// <para>A negative number indicates no selection</para>
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public object ShowMenu(int screen)
        {
            OMPanel Panel = CreatePanel(screen);
            MenuSelection[screen].Reset();

            // loadpanel
            BuiltInComponents.Panels.loadSinglePanel(Panel, screen, false);

            // Connect to system events (to detect "goback" event)
            SystemEvent SysEv = new SystemEvent(theHost_OnSystemEvent);
            BuiltInComponents.Host.OnSystemEvent += SysEv;

            // Show the panel
            if (BuiltInComponents.Host.execute(eFunction.TransitionToPanel, screen.ToString(), "", Panel.Name) == false)
                return -1;
            BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());

            // Wait for buttons
            MenuSelection[screen].WaitOne();

            // Remove menu and clean up
            BuiltInComponents.Host.OnSystemEvent -= SysEv;
            BuiltInComponents.Host.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.SlideRight.ToString());
            //BuiltInComponents.Host.execute(eFunction.TransitionFromPanel, screen.ToString(), "", Panel.Name);
            //BuiltInComponents.Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.SlideRight.ToString());
            BuiltInComponents.Panels.unloadPanel(Panel.Name, screen);

            // Return correct data based on returntype
            switch (ReturnType)
            {
                case ReturnTypes.Index:
                    return ResultIndex;
                case ReturnTypes.Tag:
                    if (ResultIndex == -1)
                        return null;
                    return ResultItem.tag;
                case ReturnTypes.IndexTag:
                    return new object[2] { ResultIndex, ResultItem.tag };
                case ReturnTypes.SelectedItem:
                    return ResultItem;
                default:
                    break;
            }
            return ResultIndex;
        }

        /// <summary>
        /// Close the dialog
        /// </summary>
        public void Close(int screen)
        {
            ResultIndex = -1;
            MenuSelection[screen].Set();
        }

        void MenuList_OnClick(OMControl sender, int screen)
        {
            ResultIndex = ((OMList)sender).SelectedIndex;
            ResultItem = (OMListItem)((OMList)sender).SelectedItem.Clone();
            MenuSelection[screen].Set();
        }

        void theHost_OnSystemEvent(eFunction function, object[] args)
        {
            if (function == eFunction.goBack)
            {
                if (args != null && args.Length >= 3)
                {
                    int screen = int.Parse(args[0] as string);
                    if (args[2] as string == PanelName[screen])
                    {
                        ResultIndex = -1;
                        MenuSelection[screen].Set();
                    }
                }
            }
        }
    }
}
