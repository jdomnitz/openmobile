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
using OpenMobile.Data;
using OpenMobile.Framework;
using OpenMobile.Graphics;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.helperFunctions.Plugins;


namespace OMSettings
{
    [SkinIcon("*@")] //*'")]
    public class Settings : IHighLevel
    {
        ScreenManager manager;
        IPluginHost theHost;

        imageItem listPluginItem_Selected;
        imageItem listPluginItem_Highlighted;

        OMObjectList.ListItem listItemSeparator = null;
        

        public OpenMobile.eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            host.OnSystemEvent += new SystemEvent(host_OnSystemEvent);
            manager = new ScreenManager(this);

            OMLabel Label = new OMLabel("Label", 383, 160, 450, 100);
            Label.Font = new Font(Font.GenericSansSerif, 36F);
            Label.Text = "Audio Zone";

            OMButton Cancel = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("Media.Cancel", 14, 255, 200, 110, "", "Cancel");
            //OMButton Cancel = new OMButton("Media.Cancel", 14, 255, 200, 110);
            //Cancel.Image = theHost.getSkinImage("Full");
            //Cancel.FocusImage = theHost.getSkinImage("Full.Highlighted");
            //Cancel.Text = "Cancel";
            Cancel.Transition = eButtonTransition.None;
            Cancel.OnClick += new userInteraction(Cancel_OnClick);

            #region menu
            OMPanel main = new OMPanel("Main");
            main.BackgroundColor1 = Color.Black;
            main.BackgroundType = backgroundStyle.SolidColor;
            OMList menu = new OMList("list_Menu", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height);
            menu.ListStyle = eListStyle.MultiListText;
            menu.Background = Color.Silver;
            menu.ItemColor1 = Color.Black;
            menu.Font = new Font(Font.GenericSansSerif, 30F);
            menu.HighlightColor = Color.White;
            menu.SelectedItemColor1 = BuiltInComponents.SystemSettings.SkinFocusColor;
            menu.SoftEdgeData.Color1 = Color.Black;
            menu.SoftEdgeData.Sides = FadingEdge.GraphicSides.Top | FadingEdge.GraphicSides.Bottom;
            menu.UseSoftEdges = true;
            OMListItem.subItemFormat format=new OMListItem.subItemFormat();
            format.color = Color.FromArgb(128, menu.Color);
            format.font = new Font(Font.GenericSansSerif, 21F);
            menu.Add(new OMListItem("About", "Credits and information", format, "About"));
            menu.Add(new OMListItem("System Settings", "User Interface and System Settings", format, "System Settings"));
            //menu.Add(new OMListItem("Personal Settings", "Usernames and Passwords", format, "personal"));
            //menu.Add(new OMListItem("Data Settings", "Settings for each of the Data Providers", format, "data"));
            menu.Add(new OMListItem("Input routing", "Routing of data between a input device and a screen", format, "InputRouterScreenSelection"));
            menu.Add(new OMListItem("Multi-Zone Settings", "Displays, Sound Cards and other zone specific settings", format, "Zones"));
            menu.Add(new OMListItem("Plugin Settings", "Settings for all low level plugins", format, "Plugins"));
            menu.OnClick += new userInteraction(menu_OnClick);
            main.addControl(menu);
            manager.loadPanel(main, true);
            #endregion

            #region general
            LayoutManager generalLayout = new LayoutManager();
            OpenMobile.Plugin.Settings SystemSettings = BuiltInComponents.GlobalSettings();
            SystemSettings.OnSettingChanged += new SettingChanged(SystemSettings_OnSettingChanged);
            OMPanel[] general = generalLayout.layout(theHost, SystemSettings);
            manager.loadPanel(general);
            #endregion

            #region Disabled options

            /*
            #region personal
            OMPanel personal = new OMPanel("personal");
            OMButton Save2 = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("Media.Save", 14, 136, 200, 110, "", "Save");
            //OMButton Save2 = new OMButton(13, 136, 200, 110);
            //Save2.Image = theHost.getSkinImage("Full");
            //Save2.FocusImage = theHost.getSkinImage("Full.Highlighted");
            //Save2.Text = "Save";
            //Save2.Name = "Media.Save";
            Save2.OnClick += new userInteraction(Save2_OnClick);
            Save2.Transition = eButtonTransition.None;
            OMLabel Heading = new OMLabel(300, 80, 600, 100);
            Heading.Font = new Font(Font.GenericSansSerif, 36F);
            Heading.Text = "Google Account Info";
            Label.Name = "Label";
            OMLabel udesc = new OMLabel(220, 180, 200, 50);
            udesc.Text = "Username:";
            udesc.Font = new Font(Font.GenericSansSerif, 24F);
            OMLabel pdesc = new OMLabel(220, 240, 200, 50);
            pdesc.Text = "Password:";
            pdesc.Font = new Font(Font.GenericSansSerif, 24F);
            OMTextBox user = new OMTextBox(450, 180, 500, 50);
            user.Font = new Font(Font.GenericSansSerif, 28F);
            user.TextAlignment = Alignment.CenterLeft;
            user.OSKType = OSKInputTypes.Keypad;
            user.OSKDescription = "Enter username";
            user.OSKHelpText = "username";
            OMTextBox pass = new OMTextBox(450, 240, 500, 50);
            pass.Font = new Font(Font.GenericSansSerif, 28F);
            pass.Flags = textboxFlags.Password;
            pass.OSKType = OSKInputTypes.Keypad;
            pass.OSKDescription = "Password";
            pass.OSKHelpText = "password";
            pass.TextAlignment = Alignment.CenterLeft;
            personal.addControl(Save2);
            personal.addControl(Cancel);
            personal.addControl(Heading);
            personal.addControl(udesc);
            personal.addControl(pdesc);
            personal.addControl(user);
            personal.addControl(pass);
            manager.loadPanel(personal);
            #endregion
            #region data
            OMPanel data = new OMPanel("data");
            OMButton Save4 = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("Media.Save", 14, 136, 200, 110, "", "Save");
            //OMButton Save4 = new OMButton(13, 136, 200, 110);
            //Save4.Image = theHost.getSkinImage("Full");
            //Save4.FocusImage = theHost.getSkinImage("Full.Highlighted");
            //Save4.Text = "Save";
            //Save4.Name = "Media.Save";
            Save4.OnClick += new userInteraction(Save4_OnClick);
            Save4.Transition = eButtonTransition.None;
            OMLabel Heading3 = new OMLabel(300, 80, 600, 100);
            Heading3.Font = new Font(Font.GenericSansSerif, 36F);
            Heading3.Text = "Data Provider Settings";
            Label.Name = "Label";
            OMLabel ldesc = new OMLabel(210, 180, 280, 50);
            ldesc.Text = "Default Location:";
            ldesc.Font = new Font(Font.GenericSansSerif, 24F);
            OMTextBox location = new OMTextBox(525, 180, 450, 50);
            location.Font = new Font(Font.GenericSansSerif, 28F);
            location.TextAlignment = Alignment.CenterLeft;
            location.OSKType = OSKInputTypes.Keypad;
            location.OSKDescription = "Location";
            location.OSKHelpText = "Enter location";
            OMLabel explanation = new OMLabel(525,230,450,30);
            explanation.Text = "Postcode, city and state, etc.";
            OMList providers = new OMList(275, 280, 650, 240);
            providers.ListStyle = eListStyle.MultiList;
            providers.Background = Color.Transparent;
            providers.ItemColor1 = Color.Transparent;
            providers.SelectedItemColor1 = Color.Blue;
            providers.HighlightColor = Color.White;
            providers.ListItemOffset = 80;
            OMBasicShape backdrop = new OMBasicShape("", 270, 275, 660, 250,
                new ShapeData(shapes.RoundedRectangle, Color.Black, Color.WhiteSmoke, 2, 10));
            data.addControl(Save4);
            data.addControl(Cancel);
            data.addControl(Heading3);
            data.addControl(ldesc);
            data.addControl(location);
            data.addControl(explanation);
            data.addControl(backdrop);
            data.addControl(providers);
            manager.loadPanel(data);
            #endregion
            */

            #endregion

            #region Plugins
            //OMPanel panelPlugins = new OMPanel("Plugins");
            //panelPlugins.BackgroundColor1 = Color.Black;
            //panelPlugins.BackgroundType = backgroundStyle.SolidColor;
            ////OMList lstplugins = new OMList(10, 100, 980, 433);
            //OMList lstplugins = new OMList("list_Plugins", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height);
            //lstplugins.ListStyle = eListStyle.MultiListText;
            //lstplugins.Background = Color.Silver;
            //lstplugins.ItemColor1 = Color.Black;
            //lstplugins.Font = new Font(Font.GenericSansSerif, 30F);
            //lstplugins.HighlightColor = Color.White;
            //lstplugins.OnClick += new userInteraction(lstplugins_OnClick);
            //lstplugins.Add(new OMListItem("Loading . . .","",format));
            //lstplugins.Scrollbars = true;
            //panelPlugins.addControl(lstplugins);
            //manager.loadPanel(panelPlugins);

            OMPanel panelPlugins = new OMPanel("Plugins");
            panelPlugins.BackgroundColor1 = Color.Black;
            panelPlugins.BackgroundType = backgroundStyle.SolidColor;

            OMObjectList listPlugins = new OMObjectList("listPlugins", theHost.ClientArea[0].Left, theHost.ClientArea[0].Top, theHost.ClientArea[0].Width, theHost.ClientArea[0].Height);
            panelPlugins.addControl(listPlugins);

            #region Create list item

            OMObjectList.ListItem ItemBase = new OMObjectList.ListItem();

            int itemHeight = 100;
            
            // Generate list item background images
            OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData gd = new OpenMobile.helperFunctions.Graphics.ButtonGraphic.GraphicData();
            gd.BackgroundColor1 = Color.Transparent;
            gd.BorderColor = Color.Transparent;
            gd.Width = listPlugins.Width;
            gd.Height = itemHeight;
            gd.CornerRadius = 0;
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundFocused;
            listPluginItem_Selected = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));
            gd.ImageType = OpenMobile.helperFunctions.Graphics.ButtonGraphic.ImageTypes.ButtonBackgroundClicked;
            listPluginItem_Highlighted = new imageItem(OpenMobile.helperFunctions.Graphics.ButtonGraphic.GetImage(gd));

            OMButton btnPluginListItem = new OMButton("btnPluginListItem", 0, 0, listPlugins.Width, itemHeight);
            btnPluginListItem.FocusImage = listPluginItem_Highlighted;
            btnPluginListItem.DownImage = listPluginItem_Selected;
            btnPluginListItem.OnClick += new userInteraction(btnPluginListItem_OnClick);
            ItemBase.Add(btnPluginListItem);

            OMImage imgPluginListIcon = new OMImage("imgPluginListIcon", 5, 0, 64, 64);
            ItemBase.Add(imgPluginListIcon);

            OMCheckbox chkPluginListItemEnabled = new OMCheckbox("chkPluginListItemEnabled", listPlugins.Width - 180, 5, 160, 30);
            chkPluginListItemEnabled.OutlineColor = Color.White;
            chkPluginListItemEnabled.CheckedColor = Color.FromArgb(128, Color.White);
            chkPluginListItemEnabled.OnClick += new userInteraction(chkPluginListItemEnabled_OnClick);
            chkPluginListItemEnabled.Text = "Enabled:";
            chkPluginListItemEnabled.Style = OMCheckbox.Styles.TextToTheLeft;
            ItemBase.Add(chkPluginListItemEnabled);

            OMLabel lblPluginListItemHeader = new OMLabel("lblPluginListItemHeader", imgPluginListIcon.Region.Right, 0, listPlugins.Width - imgPluginListIcon.Region.Right, 50);
            lblPluginListItemHeader.Font = new Font(Font.GenericSansSerif, 30F);
            lblPluginListItemHeader.TextAlignment = Alignment.CenterLeft;
            lblPluginListItemHeader.NoUserInteraction = true;
            ItemBase.Add(lblPluginListItemHeader);

            OMLabel lblPluginListItemDescription = new OMLabel("lblPluginListItemDescription", imgPluginListIcon.Region.Right, 50, listPlugins.Width - imgPluginListIcon.Region.Right, 45);
            lblPluginListItemDescription.TextAlignment = Alignment.CenterLeft;
            lblPluginListItemDescription.Color = Color.FromArgb(128, lblPluginListItemHeader.Color);
            lblPluginListItemDescription.Font = new Font(Font.GenericSansSerif, 16F);
            lblPluginListItemDescription.NoUserInteraction = true;
            ItemBase.Add(lblPluginListItemDescription);

            OMImage imgPluginListItemSeparator = new OMImage("imgPluginListItemSeparator", 0, itemHeight, listPlugins.Width, 1);
            imgPluginListItemSeparator.BackgroundColor = Color.FromArgb(50, Color.White);
            ItemBase.Add(imgPluginListItemSeparator);

            OMLabel lblPluginListItemDisabledInfo = new OMLabel("lblPluginListItemDisabledInfo", 0, 0, listPlugins.Width, itemHeight);
            lblPluginListItemDisabledInfo.TextAlignment = Alignment.CenterCenter;
            lblPluginListItemDisabledInfo.Color = Color.Red;
            lblPluginListItemDisabledInfo.BackgroundColor = Color.FromArgb(178, Color.Black);
            lblPluginListItemDisabledInfo.Transparency = 100;
            lblPluginListItemDisabledInfo.Font = new Font(Font.GenericSansSerif, 26F);
            lblPluginListItemDisabledInfo.NoUserInteraction = true;
            ItemBase.Add(lblPluginListItemDisabledInfo);

            // Method for setting values when adding list items
            ItemBase.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
            {
                IBasePlugin plugin = values[8] as IBasePlugin;
                PluginLevels pluginLevel = OpenMobile.helperFunctions.Plugins.Plugins.getPluginLevel(plugin.pluginName);
                OSConfigurationFlags OSConfFlags = OpenMobile.helperFunctions.Plugins.Plugins.GetPluginOSConfigurationFlags(plugin);
                bool OSConfFlagsValid = OpenMobile.helperFunctions.Plugins.Plugins.IsPluginOSConfigurationFlagsValid(plugin);

                // Save button tag to go to correct plugin
                OMButton btnItemButton = item["btnPluginListItem"] as OMButton;
                btnItemButton.Tag = values[0] as string;
                
                // Icon
                OMImage imgItemIcon = item["imgPluginListIcon"] as OMImage;
                imgItemIcon.Image = (imageItem)values[1];

                // Header text
                OMLabel lblItemHeader = item["lblPluginListItemHeader"] as OMLabel;
                lblItemHeader.Text = values[2] as string;

                // Description text
                OMLabel lblItemDescription = item["lblPluginListItemDescription"] as OMLabel;
                lblItemDescription.Text = String.Format("{0}\n", values[3] as string);
                lblItemDescription.Text += String.Format("Version {0}", values[6]);                
                string author = values[4] as string;
                if (!String.IsNullOrEmpty(author))
                    lblItemDescription.Text += String.Format(" By {0}", author);
                string email = values[5] as string;
                if (!String.IsNullOrEmpty(email))
                    lblItemDescription.Text += String.Format(" ({0})", email);

                OMCheckbox chkItemEnabled = item["chkPluginListItemEnabled"] as OMCheckbox;
                chkItemEnabled.Checked = (bool)values[7];
                chkItemEnabled.Tag = plugin;

                if (((pluginLevel & PluginLevels.Normal) == PluginLevels.Normal) ||
                    ((pluginLevel & PluginLevels.UserInput) == PluginLevels.UserInput))
                {
                    chkItemEnabled.Transparency = 0;
                    chkItemEnabled.NoUserInteraction = false;
                }
                else
                {
                    chkItemEnabled.Transparency = 100;
                    chkItemEnabled.NoUserInteraction = true;
                }

                // Disabled text
                if (!OSConfFlagsValid)
                {
                    OMLabel lblItemDisabledInfo = item["lblPluginListItemDisabledInfo"] as OMLabel;
                    lblItemDisabledInfo.Transparency = 0;
                    lblItemDisabledInfo.Text = "Plugin not supported on this system!";
                    chkItemEnabled.Transparency = 100;
                    chkItemEnabled.NoUserInteraction = true;
                }
            };
            listPlugins.ItemBase = ItemBase;

            #endregion

            #region Create list item separator

            listItemSeparator = new OMObjectList.ListItem();

            OMLabel lblSeparator = new OMLabel("lblSeparator", 0, 0, listPlugins.Width, 20);
            lblSeparator.BackgroundColor = Color.FromArgb(50, Color.White);
            lblSeparator.FontSize = 14;
            lblSeparator.Color = Color.FromArgb(178, Color.White);
            lblSeparator.TextAlignment = Alignment.CenterLeft;
            listItemSeparator.Add(lblSeparator);

            // Method for adding list items
            listItemSeparator.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
            {
                // Item header
                OMLabel lblItemSeparator = item["lblSeparator"] as OMLabel;
                lblItemSeparator.Text = values[0] as string;
            };

            #endregion

            manager.loadPanel(panelPlugins);

            #endregion

            // Load input router panels
            OMSettings.InputRouterScreens.Initialize(this.pluginName, manager, theHost);

            // Load zones panels
            OMSettings.ZoneSettings.Initialize(this.pluginName, manager, theHost);

            return OpenMobile.eLoadStatus.LoadSuccessful;
        }

        void chkPluginListItemEnabled_OnClick(OMControl sender, int screen)
        {
            IBasePlugin plugin = sender.Tag as IBasePlugin;
            if (plugin == null)
                return;

            OMCheckbox chk = (OMCheckbox)sender;

            // Only allow items that has full opacity on the checkbox to be set (lower opacity indicates that this checkbox is not enabled)
            if (chk.Opacity == 255)
                OpenMobile.helperFunctions.Plugins.Plugins.SetPluginEnabled(plugin.pluginName, chk.Checked);
            else
                chk.Checked = true;
        }

        void btnPluginListItem_OnClick(OMControl sender, int screen)
        {   // Goto plugin settings panel
            if (sender.Tag == null || String.IsNullOrEmpty(sender.Tag as string))
                return;

            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", sender.Tag as string) == false)
                return;
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings", "Plugins");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void SystemSettings_OnSettingChanged(int screen, Setting setting)
        {
            // Verify settings input
            switch (setting.Name)
            {
                case "UI.SkinFocusColor":
                case "UI.SkinTextColor":
                    {   // Ensure that it's written in valid format 
                        if (!System.Text.RegularExpressions.Regex.IsMatch(setting.Value, @"[0-9a-fA-F][0-9a-fA-F],[0-9a-fA-F][0-9a-fA-F],[0-9a-fA-F][0-9a-fA-F]"))
                        {   // Invalid value, show error message
                            OpenMobile.helperFunctions.Forms.dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                            dialogInfo.Header = "Data error";
                            dialogInfo.Text = String.Format("{0} must be written in a format of Red,Green,Blue where each value can be a number from 00 to FF (including leading zeros)", setting.Name.Replace("UI.",""));
                            dialogInfo.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                            dialogInfo.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                            dialogInfo.ShowMsgBox(screen);
                        }

                        OpenMobile.helperFunctions.Forms.dialog dialogInfo2 = new OpenMobile.helperFunctions.Forms.dialog();
                        dialogInfo2.Header = "Color change";
                        dialogInfo2.Text = "Please restart OM to ensure the new color settings is applied to all objects!";
                        dialogInfo2.Icon = OpenMobile.helperFunctions.Forms.icons.Information;
                        dialogInfo2.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                        dialogInfo2.ShowMsgBox(screen);
                    }
                    break;
            }
        }

        private void loadPluginSettings()
        {
            // Get a list of all plugins
            List<IBasePlugin> plugins = Plugins.getPluginsOfType<IBasePlugin>();
            List<IBasePlugin> pluginsWithSettings = new List<IBasePlugin>();
            List<IBasePlugin> pluginsDisabled = BuiltInComponents.Host.getData(eGetData.GetPluginsDisabled, String.Empty) as List<IBasePlugin>;

            // Errorcheck
            if (plugins == null) return;

            // Sort lists
            plugins.Sort((a, b) => a.pluginName.CompareTo(b.pluginName));
            pluginsDisabled.Sort((a, b) => a.pluginName.CompareTo(b.pluginName));

            // Loop trough plugins
            foreach (IBasePlugin plugin in plugins)
            {
                // Get and configure settings for each plugin
                LayoutManager lm = new LayoutManager();
                OMPanel[] panel;

                // Skip plugins that's not enabled
                if (OpenMobile.helperFunctions.Plugins.Plugins.GetPluginEnabled(plugin.pluginName))
                {
                    try
                    {
                        panel = lm.layout(theHost, plugin.loadSettings());
                    }
                    //only allow a not implemented exception or null as a result
                    //anything else is an error
                    catch (NotImplementedException) { continue; }
                    catch (Exception e)
                    {
                        BuiltInComponents.Host.DebugMsg("OMSettings.loadPluginSettings", e);
                        continue;
                    }

                    // Load panels
                    if (panel != null)
                    {
                        for (int i = 0; i < panel.Length; i++)
                            panel[i].Name = plugin.pluginName + "_Settings";
                        manager.loadPanel(panel);
                        pluginsWithSettings.Add(plugin);
                    }
                }
            }

            // Filter plugins list
            foreach (IBasePlugin plugin in pluginsWithSettings)
                plugins.Remove(plugin);

            #region Add plugins with setting

            bool firstItem = true;
            foreach (IBasePlugin plugin in pluginsWithSettings)
            {
                bool enabled = OpenMobile.helperFunctions.Plugins.Plugins.GetPluginEnabled(plugin.pluginName);

                // Add plugin info to list
                for (int screen = 0; screen < BuiltInComponents.Host.ScreenCount; screen++)
                {
                    OMObjectList lst = manager[screen, "Plugins"]["listPlugins"] as OMObjectList;
                    if (lst != null)
                    {
                        if (firstItem)
                        {
                            firstItem = false;
                            lst.AddItem(listItemSeparator, ControlDirections.Down, "Plugins with settings:");
                        }
                        lst.AddItemFromItemBase(ControlDirections.Down,
                            plugin.pluginName + "_Settings",
                            plugin.pluginIcon,
                            (String.IsNullOrEmpty(plugin.pluginName) ? String.Empty : plugin.pluginName),
                            (String.IsNullOrEmpty(plugin.pluginDescription) ? String.Empty : plugin.pluginDescription),
                            (String.IsNullOrEmpty(plugin.authorName) ? String.Empty : plugin.authorName),
                            (String.IsNullOrEmpty(plugin.authorEmail) ? String.Empty : plugin.authorEmail),
                            plugin.pluginVersion,
                            enabled,
                            plugin);
                    }
                }
            }

            #endregion

            #region Add other plugins

            firstItem = true;
            foreach (IBasePlugin plugin in plugins)
            {
                bool enabled = OpenMobile.helperFunctions.Plugins.Plugins.GetPluginEnabled(plugin.pluginName);

                // Add plugin info to list
                for (int screen = 0; screen < BuiltInComponents.Host.ScreenCount; screen++)
                {
                    OMObjectList lst = manager[screen, "Plugins"]["listPlugins"] as OMObjectList;
                    if (lst != null)
                    {
                        if (firstItem)
                        {
                            firstItem = false;
                            lst.AddItem(listItemSeparator, ControlDirections.Down, "Other plugins:");
                        }
                        lst.AddItemFromItemBase(ControlDirections.Down,
                            String.Empty,
                            plugin.pluginIcon,
                            (String.IsNullOrEmpty(plugin.pluginName) ? String.Empty : plugin.pluginName),
                            (String.IsNullOrEmpty(plugin.pluginDescription) ? String.Empty : plugin.pluginDescription),
                            (String.IsNullOrEmpty(plugin.authorName) ? String.Empty : plugin.authorName),
                            (String.IsNullOrEmpty(plugin.authorEmail) ? String.Empty : plugin.authorEmail),
                            plugin.pluginVersion,
                            enabled,
                            plugin);
                    }
                }
            }

            #endregion

            #region Add disabled plugins

            firstItem = true;
            foreach (IBasePlugin plugin in pluginsDisabled)
            {
                // Add plugin info to list
                for (int screen = 0; screen < BuiltInComponents.Host.ScreenCount; screen++)
                {
                    OMObjectList lst = manager[screen, "Plugins"]["listPlugins"] as OMObjectList;
                    if (lst != null)
                    {
                        if (firstItem)
                        {
                            firstItem = false;
                            lst.AddItem(listItemSeparator, ControlDirections.Down, "Disabled plugins:");
                        }
                        lst.AddItemFromItemBase(ControlDirections.Down,
                            String.Empty,
                            plugin.pluginIcon,
                            (String.IsNullOrEmpty(plugin.pluginName) ? String.Empty : plugin.pluginName),
                            (String.IsNullOrEmpty(plugin.pluginDescription) ? String.Empty : plugin.pluginDescription),
                            (String.IsNullOrEmpty(plugin.authorName) ? String.Empty : plugin.authorName),
                            (String.IsNullOrEmpty(plugin.authorEmail) ? String.Empty : plugin.authorEmail),
                            plugin.pluginVersion,
                            false,
                            plugin);
                    }
                }
            }

            #endregion

        }

        void lstplugins_OnClick(OMControl sender, int screen)
        {
            OMList lst=(OMList)sender;
            if (lst.SelectedItem.text == "Loading . . .")
                return;
            if (theHost.execute(eFunction.TransitionToPanel, screen.ToString(),"OMSettings",lst.SelectedItem.text)==false)
                return;
            lst.SelectedIndex = -1;
            theHost.execute(eFunction.TransitionFromPanel,screen.ToString(),"OMSettings","Plugins");
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        void host_OnSystemEvent(eFunction function, object[] args)
        {
            if (function==eFunction.pluginLoadingComplete)
                OpenMobile.Threading.TaskManager.QueueTask(new OpenMobile.Threading.Function(loadPluginSettings), ePriority.Normal,"Load Plugin Settings");
        }

        void menu_OnClick(OMControl sender, int screen)
        {
            OMList list = (OMList)sender;
            string Panel = (string)list.SelectedItem.tag;
            switch (Panel)
            {
                case "About":
                    {
                        // Change screen
                        theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
                        theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "About");
                        theHost.execute(eFunction.ExecuteTransition, screen.ToString());
                        ((OMList)sender).Select(-1);
                        return;
                    }
                case "personal":
                    ((OMTextBox)manager[screen, "personal"][6]).Text = Credentials.getCredential("Google Password");
                    ((OMTextBox)manager[screen, "personal"][5]).Text = Credentials.getCredential("Google Username");
                    break;
                case "data":
                    using (PluginSettings s = new PluginSettings())
                        ((OMTextBox)manager[screen, "data"][4]).Text = s.getSetting(BuiltInComponents.OMInternalPlugin, "Data.DefaultLocation");
                    break;
            }

            // Change screen
            theHost.execute(eFunction.TransitionFromPanel, screen.ToString(), "OMSettings");
            theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", Panel);
            theHost.execute(eFunction.ExecuteTransition, screen.ToString());

            ((OMList)sender).Select(-1);
        }

        void Save4_OnClick(OMControl sender, int screen)
        {
            using (PluginSettings settings = new PluginSettings())
            {
                settings.setSetting(BuiltInComponents.OMInternalPlugin, "Data.DefaultLocation", ((OMTextBox)manager[screen, "data"][4]).Text);
            }
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void Save2_OnClick(OMControl sender, int screen)
        {
            Credentials.setCredential("Google Password", ((OMTextBox)manager[screen, "personal"][6]).Text);
            string googleUsername = ((OMTextBox)manager[screen, "personal"][5]).Text;
            if ((googleUsername!=null)&&(googleUsername.EndsWith("@gmail.com") == false))
                googleUsername += "@gmail.com";
            Credentials.setCredential("Google Username", googleUsername);
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        void Cancel_OnClick(OMControl sender, int screen)
        {
            theHost.execute(eFunction.goBack, screen.ToString());
        }

        public OpenMobile.Controls.OMPanel loadPanel(string name, int screen)
        {
            if (manager == null)
                return null;

            switch (name)
            {
                case "":
                case "Default":
                    ((OMList)manager[screen][0]).SelectedIndex = -1;
                    return manager[screen, "Main"];
                default:
                    return manager[screen, name];
            }
        }

        public OpenMobile.Plugin.Settings loadSettings()
        {
            return null;
        }

        public string displayName
        {
            get { return "Settings"; }
        }

        #region IBasePlugin Members

        public string authorName
        {
            get { return "Justin Domnitz"; }
        }

        public string authorEmail
        {
            get { return ""; }
        }

        public string pluginName
        {
            get { return "OMSettings"; }
        }

        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "Global Settings Manager for openMobile"; }
        }

        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-Settings2"); }
        }

        public bool incomingMessage(string message, string source)
        {
            throw new NotImplementedException();
        }

        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void Dispose()
        {
            if (manager!=null)
                manager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
