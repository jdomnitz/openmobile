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
using OpenMobile.Graphics;
using System.IO;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.Data;
using OpenMobile.Media;
using OpenMobile.Threading;
using System.Threading;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Forms;
using System.Collections.Generic;

namespace OMMenu
{
    [SkinIcon("*@")]
    [PluginLevel(PluginLevels.System)]
    public class OMMenu : IHighLevel
    {
        IPluginHost theHost;
        ScreenManager Manager;
        string name;

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            return null;
        }
        public Settings loadSettings()
        {
            return null;
        }

        #region Attributes
        public string displayName
        {
            get { return "OMMenu"; }
        }
        public string authorName
        {
            get { return "Bjørn Morten Orderløkken"; }
        }
        public string authorEmail
        {
            get { return "Boorte@gmail.com"; }
        }
        public string pluginName
        {
            get { return "OMMenu"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "MenuHandling for the framework"; }
        }
        #endregion

        public bool incomingMessage(string message, string source)
        {
            return false; // No action
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            // Convert input data to local data object
            OMPanel Panel = data as OMPanel;

            // Extract Menu data
            MenuData DT = (MenuData)Panel.Tag;
            Manager = DT.Manager;
            name = Panel.Name;

            /* Items has to be named according to this:
             *  Menu_Button_Cancel      : Cancel button
             *  Menu_List               : Menu list
            */

            // What type of Menu is this?
            switch (message.ToLower())
            {
                case "menupopup":
                    {
                        #region Create menu popup panel

                        // Default Menu data
                        DT.Left = (DT.Left == 0 ? 250 : DT.Left);
                        DT.Top = (DT.Top == 0 ? 175 : DT.Top);
                        DT.Width = (DT.Width == 0 ? 500 : DT.Width);
                        DT.Height = (DT.Height == 0 ? 250 : DT.Height);

                        Font f = new Font(Font.GenericSansSerif, 18F);
                        // Calculate max width of menu
                        int Width = 0;
                        for (int i = 0; i < DT.items.Count; i++)
                        {
                            int w = (int)Graphics.MeasureString(DT.items[i].text, f).Width + 50;
                            if (w > Width)
                                Width = w;
                        }
                        if ((Width > 500) && (DT.Width == 500))
                            DT.Width = Width;
                        if (DT.Width > 1000)
                            DT.Width = 1000;

                        // Calculate height of menu
                        int Height = (75 * DT.items.Count) + 49;
                        if ((Height > 250) && (DT.Height == 250))
                            DT.Height = Height;
                        if (DT.Height > 600)
                            DT.Height = 600;

                        // Position Menu box horisontally on screen (unless provided as a parameter)
                        if ((DT.Width >= 500) && (DT.Left == 250))
                        {
                            DT.Left = (500 - (DT.Width / 2));
                        }

                        // Position Menu box vertically on screen (unless provided as a parameter)
                        if ((DT.Height >= 250) && (DT.Top == 175))
                        {
                            DT.Top = (300 - (DT.Height / 2));
                        }

                        // Replace existing panel with new panel
                        DialogPanel panelMenu = new DialogPanel(Panel.Name, DT.Header, DT.Left, DT.Top, DT.Width, DT.Height, false, true);

                        //OMList List_Menu = new OMList(panelMenu.Shape_Border.Left + 1, panelMenu.Shape_Border.Top + 35, panelMenu.Shape_Border.Width - 1, panelMenu.Shape_Border.Height - 43);
                        OMList List_Menu = new OMList(panelMenu.ClientArea.Left, panelMenu.ClientArea.Top, panelMenu.ClientArea.Width, panelMenu.ClientArea.Height);
                        List_Menu.Name = "Menu_List";
                        List_Menu.Scrollbars = true;
                        List_Menu.ListStyle = eListStyle.MultiList;
                        List_Menu.Background = Color.Silver;
                        List_Menu.ItemColor1 = Color.Black;
                        List_Menu.Font = new Font(Font.GenericSansSerif, DT.FontSize);
                        List_Menu.Color = Color.White;
                        List_Menu.HighlightColor = Color.White;
                        List_Menu.SelectedItemColor1 = Color.DarkBlue;
                        List_Menu.SoftEdgeData.Color1 = Color.Black;
                        List_Menu.SoftEdgeData.Sides[0] = true;
                        List_Menu.SoftEdgeData.Sides[1] = false;
                        List_Menu.SoftEdgeData.Sides[2] = true;
                        List_Menu.SoftEdgeData.Sides[3] = false;
                        List_Menu.UseSoftEdges = true;
                        //List_Menu.ListItemHeight = 70;
                        panelMenu.addControl(List_Menu);

                        // Configure return data
                        panelMenu.Tag = Panel.Tag;
                        for (int i = 0; i < panelMenu.controlCount; i++)
                            Panel.addControl(panelMenu.getControl(i));
                        Panel.Forgotten = true;
                        //Panel.Priority = ePriority.High;
                        
                        #endregion
                    }
                    break;
                default:
                    break;
            }

            // Return data
            data = (T)Convert.ChangeType(Panel, typeof(T));
            return true;
        }

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            return eLoadStatus.LoadSuccessful;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
