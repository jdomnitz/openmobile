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
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Graphics;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.Plugin;
using OpenMobile.Media;

namespace OMRadio2
{
    internal class panelProviders
    {
        private HighLevelCode _MainPlugin;
        private MenuPopup PopupMenu;
        public const string PanelName = "Providers";
        private List<IMediaProvider> _MediaProviders;

        public panelProviders(HighLevelCode mainPlugin)
        {
            _MainPlugin = mainPlugin;
        }

        public OMPanel Initialize()
        {
            // Create a new panel
            OMPanel panel = new OMPanel(PanelName, "Radio > Providers", _MainPlugin.pluginIcon);

            OMBasicShape shapeBackground = new OMBasicShape("shapeBackground", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width, OM.Host.ClientArea_Init.Height,
                new ShapeData(shapes.Rectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0));
            panel.addControl(shapeBackground);

            OMList lstProviders = new OMList("lstProviders", OM.Host.ClientArea_Init.Left + 190, OM.Host.ClientArea_Init.Top, OM.Host.ClientArea_Init.Width - 380, OM.Host.ClientArea_Init.Height);
            lstProviders.ListStyle = eListStyle.MultiList;
            lstProviders.Color = BuiltInComponents.SystemSettings.SkinTextColor;
            lstProviders.ScrollbarColor = Color.FromArgb(120, Color.White);
            lstProviders.SeparatorColor = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            lstProviders.ItemColor1 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 66);
            lstProviders.ItemColor2 = Color.Transparent;  //Color.FromArgb(58, 58, 58);//Color.FromArgb(0, 0, 10);
            lstProviders.SelectedItemColor1 = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            lstProviders.SelectedItemColor2 = Color.FromArgb(100, BuiltInComponents.SystemSettings.SkinFocusColor);
            //lstMedia.SelectedItemColor1 = Color.Red;
            lstProviders.HighlightColor = Color.White;
            lstProviders.Font = new Font(Font.GenericSansSerif, 26F);
            lstProviders.Scrollbars = true;
            lstProviders.TextAlignment = Alignment.TopLeft;
            lstProviders.ListItemOffset = 85;
            lstProviders.OnClick += new userInteraction(lstProviders_OnClick);
            panel.addControl(lstProviders);

            // Connect panel events
            panel.Entering += new PanelEvent(panel_Entering);
            //panel.Leaving += new PanelEvent(panel_Leaving);

            panel.Forgotten = true;

            return panel;
        }

        void lstProviders_OnClick(OMControl sender, int screen)
        {
            bool goBack = false;
            OMList lst = sender as OMList;
            
            // get plugin name
            var selectedProvider = lst.SelectedItem.tag as string;

            // Activate provider
            if (selectedProvider != null)
            {
                var statusMessage = OM.Host.CommandHandler.ExecuteCommand(screen, "Zone.MediaProvider.Activate", selectedProvider as string) as string;

                // Handle error messages
                if (statusMessage is string && !String.IsNullOrWhiteSpace(statusMessage as string))
                {
                    OpenMobile.helperFunctions.Forms.dialog dialog = new OpenMobile.helperFunctions.Forms.dialog(_MainPlugin.pluginName, sender.Parent.Name);
                    dialog.Header = "Failed to activate provider";
                    dialog.Text = String.Format("Unable to activate provider '{0}'\r\n{1}", selectedProvider as string, statusMessage as string);
                    dialog.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                    dialog.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                    dialog.ShowMsgBox(screen);
                }
                else
                {
                    goBack = true;
                }
            }
            if (goBack)
                _MainPlugin.GoBack(screen);
        }

        void panel_Entering(OMPanel sender, int screen)
        {
            // Map list to the list control
            OMList lstProviders = sender[screen, "lstProviders"] as OMList;

            // Get a list of providers
            var providerList = OM.Host.MediaProviderHandler.GetMediaProvidersByMediaSourceType(MediaSourceTypes.TunedContent.ToString());

            foreach (var provider in providerList)
            {
                IBasePlugin baseProvider = provider as IBasePlugin;
                OMListItem listItem = new OMListItem(baseProvider.pluginName, baseProvider.pluginDescription, baseProvider.pluginName);
                listItem.image = baseProvider.pluginIcon.image;
                lstProviders.Add(listItem);
            }
        }

    }
}