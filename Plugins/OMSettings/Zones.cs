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
using OpenMobile.helperFunctions;
using OpenMobile.helperFunctions.Forms;
using OpenMobile.helperFunctions.MenuObjects;
using OpenMobile.helperFunctions.Controls;
using OpenMobile.Zones;

namespace OMSettings
{
    internal static class ZoneSettings
    {
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static OMListItem.subItemFormat subItemformat;
        static MenuPopup ZoneMenu;
        static MenuPopup ZoneDetail_ScreenList;
        static DialogPanel panelZonesDetail = null;

        static Zone TmpZone = null;

        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {

            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            #region MainPanel

            OMPanel panelZones = new OMPanel("Zones");

            OMLabel Label_PanelHeader = new OMLabel("Zones_Label_PanelHeader", 20, 105, 1000, 40);
            Label_PanelHeader.Text = "Configuration of zones";
            Label_PanelHeader.TextAlignment = Alignment.TopLeft;
            Label_PanelHeader.Font = new Font(Font.GenericSansSerif, 22F, FontStyle.Bold);
            panelZones.addControl(Label_PanelHeader);

            OMImage Image_PanelIcon = new OMImage("Image_PanelIcon", 50, 100, 200, 200);
            Image_PanelIcon.Image = host.getSkinImage("MixerBig");
            panelZones.addControl(Image_PanelIcon);

            OMLabel Label_PanelInfo = new OMLabel("Zones_Label_PanelInfo", 20, 270, 270, 300);
            Label_PanelInfo.Text = "Zone is a User named collection of screen and/or audio\n\nA zone can also include other zones to create new \"virtual zones\"";
            Label_PanelInfo.TextAlignment = Alignment.WordWrapTL;
            panelZones.addControl(Label_PanelInfo);

            OMList List_Zones = new OMList("List_Zones", 300, 140, 650, 400);
            List_Zones.Scrollbars = true;
            List_Zones.ListStyle = eListStyle.MultiList;
            List_Zones.Background = Color.Silver;
            List_Zones.ItemColor1 = Color.Black;
            List_Zones.Font = new Font(Font.GenericSansSerif, 28F);
            List_Zones.Color = Color.White;
            List_Zones.HighlightColor = Color.White;
            List_Zones.ListItemHeight = 70;
            List_Zones.OnLongClick += new userInteraction(List_Zones_OnLongClick);
            subItemformat = new OMListItem.subItemFormat();
            subItemformat.color = Color.FromArgb(128, Color.White);
            subItemformat.font = new Font(Font.GenericSansSerif, 15F);
            panelZones.addControl(List_Zones);

            panelZones.Entering += new PanelEvent(panelZones_Entering);
            panelZones.Leaving += new PanelEvent(panelZones_Leaving);

            #region Bottom menu buttons

            // Calculate where to place the buttons based on amount of buttons needed
            const int MenuButtonCount = 3;
            const int MenuButtonWidth = 160;
            int MenuButtonStartLocation = 500 - ((MenuButtonWidth * MenuButtonCount) / 2);

            OMButton[] Button_BottomBar = new OMButton[MenuButtonCount];
            for (int i = 0; i < Button_BottomBar.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "", "Menu");
                        Button_BottomBar[i].OnClick += new userInteraction(List_Zones_OnLongClick);
                        break;
                    case 1:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "", "Activate");
                        Button_BottomBar[i].OnClick += new userInteraction(Button_BottomBar_Activate_OnClick);
                        break;
                    case 2:
                        Button_BottomBar[i] = DefaultControls.GetHorisontalEdgeButton(String.Format("Button_BottomBar{0}", i), MenuButtonStartLocation + (MenuButtonWidth * i), 540, 160, 70, "", "Restore");
                        Button_BottomBar[i].OnClick += new userInteraction(Button_BottomBar_Restore_OnClick);
                        break;
                    default:
                        break;
                }

                panelZones.addControl(Button_BottomBar[i]);
            }

            #endregion

            Manager.loadPanel(panelZones);

            #endregion

            #region Menu popup

            // Popup menu
            ZoneMenu = new MenuPopup("ZoneMenu", MenuPopup.ReturnTypes.Tag);            

            // Popup menu items
            OMListItem ListItem = new OMListItem("New", "mnuItemNewZone" as object);
            ListItem.image = OImage.FromFont(50, 50, "+", new Font(Font.Arial, 40F), eTextFormat.Outline, Alignment.CenterCenter, Color.Green, Color.Green);
            ZoneMenu.AddMenuItem(ListItem);

            OMListItem ListItem2 = new OMListItem("Edit", "mnuItemEditZone" as object);
            ListItem2.image = OImage.FromWebdingsFont(50, 50, "@", Color.Gray);
            ZoneMenu.AddMenuItem(ListItem2);

            OMListItem ListItem3 = new OMListItem("Remove", "mnuItemRemoveZone" as object);
            ListItem3.image = OImage.FromWebdingsFont(50, 50, "r", Color.Red);
            ZoneMenu.AddMenuItem(ListItem3);

            OMListItem ListItem4 = new OMListItem("Set active", "mnuItemSetActive" as object);
            ListItem4.image = OImage.FromWebdingsFont(50, 50, "a", Color.Gray);
            ZoneMenu.AddMenuItem(ListItem4);

            #endregion

            #region Zone details panel

            panelZonesDetail = new DialogPanel("ZoneDetail"," ", 100, 80, 800, 450, true, false);
            panelZonesDetail.Button_Ok.OnClick += new userInteraction(panelZonesDetail_Button_Ok_OnClick);
            panelZonesDetail.Button_Cancel.OnClick += new userInteraction(panelZonesDetail_Button_Cancel_OnClick);

            OMLabel ZoneDetail_Label_Header = new OMLabel("ZoneDetail_Label_Header", panelZonesDetail.HeaderArea.X, panelZonesDetail.HeaderArea.Y, panelZonesDetail.HeaderArea.Width, panelZonesDetail.HeaderArea.Height);
            ZoneDetail_Label_Header.Text = "ZoneDetail";
            panelZonesDetail.addControl(ZoneDetail_Label_Header);


            #region Zone Name textbox

            OMLabel ZoneDetail_Label_ZoneName = new OMLabel("ZoneDetail_Label_ZoneName", panelZonesDetail.ClientArea.X, panelZonesDetail.ClientArea.Y, 150, 40);
            ZoneDetail_Label_ZoneName.Text = "Name:";
            ZoneDetail_Label_ZoneName.TextAlignment = Alignment.CenterLeft;
            panelZonesDetail.addControl(ZoneDetail_Label_ZoneName);

            OMTextBox ZoneDetail_TextBox_ZoneName = new OMTextBox("ZoneDetail_TextBox_ZoneName", ZoneDetail_Label_ZoneName.Left + ZoneDetail_Label_ZoneName.Width, ZoneDetail_Label_ZoneName.Top, panelZonesDetail.ClientArea.Width - ZoneDetail_Label_ZoneName.Width, ZoneDetail_Label_ZoneName.Height);
            ZoneDetail_TextBox_ZoneName.TextAlignment = Alignment.CenterLeft;
            ZoneDetail_TextBox_ZoneName.OSKType = OSKInputTypes.Keypad;
            ZoneDetail_TextBox_ZoneName.OSKDescription = "Enter name of zone:";
            ZoneDetail_TextBox_ZoneName.OSKHelpText = "Name of zone";
            panelZonesDetail.addControl(ZoneDetail_TextBox_ZoneName);

            #endregion

            #region Zone Description textbox

            OMLabel ZoneDetail_Label_ZoneDescription = (OMLabel)OMControl_CloneAndAlignDown("ZoneDetail_Label_ZoneDescription", ZoneDetail_Label_ZoneName);
            ZoneDetail_Label_ZoneDescription.Text = "Description:";
            panelZonesDetail.addControl(ZoneDetail_Label_ZoneDescription);

            OMTextBox ZoneDetail_TextBox_ZoneDescription = (OMTextBox)OMControl_CloneAndAlignDown("ZoneDetail_TextBox_ZoneDescription", ZoneDetail_TextBox_ZoneName);
            ZoneDetail_TextBox_ZoneDescription.OSKDescription = "Enter description for zone:";
            ZoneDetail_TextBox_ZoneDescription.OSKHelpText = "Zone description";
            panelZonesDetail.addControl(ZoneDetail_TextBox_ZoneDescription);

            #endregion

            #region Zone Screen textbox

            OMLabel ZoneDetail_Label_ZoneScreen = (OMLabel)OMControl_CloneAndAlignDown("ZoneDetail_Label_ZoneScreen", ZoneDetail_Label_ZoneDescription);
            ZoneDetail_Label_ZoneScreen.Text = "Screen:";
            panelZonesDetail.addControl(ZoneDetail_Label_ZoneScreen);

            OMTextBox ZoneDetail_TextBox_ZoneScreen = (OMTextBox)OMControl_CloneAndAlignDown("ZoneDetail_TextBox_ZoneScreen", ZoneDetail_TextBox_ZoneDescription);
            ZoneDetail_TextBox_ZoneScreen.OSKType = OSKInputTypes.Numpad;
            ZoneDetail_TextBox_ZoneScreen.OSKDescription = "Enter assigned screen number:";
            ZoneDetail_TextBox_ZoneScreen.OSKHelpText = "Screen number";
            ZoneDetail_TextBox_ZoneScreen.OSKShowOnLongClick = true;
            ZoneDetail_TextBox_ZoneScreen.OnOSKShow += new userInteraction(ZoneDetail_TextBox_ZoneScreen_OnOSKShow);
            ZoneDetail_TextBox_ZoneScreen.OnClick += new userInteraction(ZoneDetail_TextBox_ZoneScreen_OnClick);
            panelZonesDetail.addControl(ZoneDetail_TextBox_ZoneScreen);

            #endregion

            #region AudioDevice textbox

            OMLabel ZoneDetail_Label_AudioDevice = (OMLabel)OMControl_CloneAndAlignDown("ZoneDetail_Label_AudioDevice", ZoneDetail_Label_ZoneScreen);
            ZoneDetail_Label_AudioDevice.Text = "AudioDevice:";
            panelZonesDetail.addControl(ZoneDetail_Label_AudioDevice);

            OMTextBox ZoneDetail_TextBox_AudioDevice = (OMTextBox)OMControl_CloneAndAlignDown("ZoneDetail_TextBox_AudioDevice", ZoneDetail_TextBox_ZoneScreen);
            ZoneDetail_TextBox_AudioDevice.OSKType = OSKInputTypes.None;
            ZoneDetail_TextBox_AudioDevice.OnClick += new userInteraction(ZoneDetail_TextBox_AudioDevice_OnClick);
            panelZonesDetail.addControl(ZoneDetail_TextBox_AudioDevice);

            #endregion

            #region Subzones textbox

            OMLabel ZoneDetail_Label_SubZones = (OMLabel)OMControl_CloneAndAlignDown("ZoneDetail_Label_SubZones", ZoneDetail_Label_AudioDevice);
            ZoneDetail_Label_SubZones.Text = "SubZones:";
            panelZonesDetail.addControl(ZoneDetail_Label_SubZones);

            OMTextBox ZoneDetail_TextBox_SubZones = (OMTextBox)OMControl_CloneAndAlignDown("ZoneDetail_TextBox_SubZones", ZoneDetail_TextBox_AudioDevice);
            ZoneDetail_TextBox_SubZones.OSKType = OSKInputTypes.None;
            ZoneDetail_TextBox_SubZones.OnClick += new userInteraction(ZoneDetail_TextBox_SubZones_OnClick);
            panelZonesDetail.addControl(ZoneDetail_TextBox_SubZones);

            #endregion

            #region Zone ID textbox

            OMLabel ZoneDetail_Label_ZoneID = (OMLabel)OMControl_CloneAndAlignDown("ZoneDetail_Label_ZoneID", ZoneDetail_Label_SubZones);
            ZoneDetail_Label_ZoneID.Text = "ID:";
            panelZonesDetail.addControl(ZoneDetail_Label_ZoneID);

            OMTextBox ZoneDetail_TextBox_ZoneID = (OMTextBox)OMControl_CloneAndAlignDown("ZoneDetail_TextBox_ZoneID", ZoneDetail_TextBox_SubZones);
            ZoneDetail_TextBox_ZoneID.OSKType = OSKInputTypes.None;
            ZoneDetail_TextBox_ZoneID.Disabled = true;  // This is a read only field
            panelZonesDetail.addControl(ZoneDetail_TextBox_ZoneID);

            #endregion

            panelZonesDetail.Entering += new PanelEvent(panelZonesDetail_Entering);
            panelZonesDetail.Leaving += new PanelEvent(panelZonesDetail_Leaving);
            panelZonesDetail.Loaded += new PanelEvent(panelZonesDetail_Loaded);

            panelZonesDetail.Priority = ePriority.High;
            Manager.loadPanel(panelZonesDetail);
            
            #endregion

            #region Screen list popup

            // Popup menu - Screen numbers. Returns the tag in the selected item which holds the screen number.
            ZoneDetail_ScreenList = new MenuPopup("Select a screen", MenuPopup.ReturnTypes.Tag);
            Host.ForEachScreen(delegate(int Screen)
            {
                OMListItem ScreenItem = new OMListItem(String.Format("Screen {0}", Screen), Screen as object);
                ScreenItem.image = OImage.FromWebdingsFont(50, 50, "¾", Color.Gray);
                ZoneDetail_ScreenList.AddMenuItem(ScreenItem);
            });

            // Add clear item
            OMListItem ClearItem = new OMListItem("None", null as object);
            ClearItem.image = OImage.FromWebdingsFont(50, 50, "r", Color.Red);
            ZoneDetail_ScreenList.AddMenuItem(ClearItem);

            #endregion
        }

        static void panelZonesDetail_Loaded(OMPanel sender, int screen)
        {
            // Update data fields
            OMTextBox txtName = (OMTextBox)sender[screen, "ZoneDetail_TextBox_ZoneName"];
            txtName.BackgroundColor = Color.White;
            OMTextBox txtDescription = (OMTextBox)sender[screen, "ZoneDetail_TextBox_ZoneDescription"];
            txtDescription.BackgroundColor = Color.White;
            OMTextBox txtScreen = (OMTextBox)sender[screen, "ZoneDetail_TextBox_ZoneScreen"];
            txtScreen.BackgroundColor = Color.White;
            OMTextBox txtAudioDevice = (OMTextBox)sender[screen, "ZoneDetail_TextBox_AudioDevice"];
            txtAudioDevice.BackgroundColor = Color.White;
            OMTextBox txtSubZones = (OMTextBox)sender[screen, "ZoneDetail_TextBox_SubZones"];
            txtSubZones.BackgroundColor = Color.White;
            OMLabel lblHeader = (OMLabel)sender[screen, "ZoneDetail_Label_Header"];
            OMTextBox txtID = (OMTextBox)sender[screen, "ZoneDetail_TextBox_ZoneID"];

            // Enable controls
            txtName.Disabled = false;
            txtDescription.Disabled = false;
            txtScreen.Disabled = false;
            txtAudioDevice.Disabled = false;
            txtSubZones.Disabled = false;

            if (TmpZone != null)
            {   // Edit current zone, fill with existing data

                // Is this a editable zon (Autogenerated zones can not be changed)
                if (TmpZone.AutoGenerated)
                {
                    txtName.Disabled = true;
                    txtDescription.Disabled = true;
                    txtScreen.Disabled = true;
                    txtAudioDevice.Disabled = true;
                    txtSubZones.Disabled = true;
                }

                lblHeader.Text = String.Format("Details for {0} {1}", TmpZone.Name, (TmpZone.AutoGenerated ? "(Read Only!)" : ""));
                // Fill data into controls
                txtName.Text = TmpZone.Name;
                txtDescription.Text = TmpZone.Description;
                txtScreen.Text = TmpZone.Screen.ToString();
                txtAudioDevice.Text = TmpZone.AudioDeviceName;
                txtID.Text = TmpZone.ID.ToString();

                // List subzones
                if (TmpZone.SubZones.Count > 0)
                {
                    txtSubZones.Text = Host.ZoneHandler.GetZone(TmpZone.SubZones[0]).Name;
                    for (int i = 1; i < TmpZone.SubZones.Count; i++)
                        txtSubZones.Text += "|" + Host.ZoneHandler.GetZone(TmpZone.SubZones[i]).Name;
                }
                
            }
            else
            {   // Add new zone
                lblHeader.Text = "Add new zone";
                // Fill data into controls
                txtName.Text = String.Format("Zone{0}", Host.ZoneHandler.Zones.Count);
                txtDescription.Text = "";
                txtScreen.Text = "0"; //Default screen
                txtAudioDevice.Text = Host.GetAudioDeviceDefaultName(); // Default audio device
                txtSubZones.Text = "";
                txtID.Text = "";
            }
        }

        static void ZoneDetail_TextBox_AudioDevice_OnClick(OMControl sender, int screen)
        {
            OMTextBox txtBox = sender as OMTextBox;

            // Popup menu - Zones. Returns the tag in the selected item which holds the zone object.
            MenuPopup MenuPopup = new MenuPopup("Assign audiodevice", MenuPopup.ReturnTypes.Tag);
            MenuPopup.FontSize = 30f;
            MenuPopup.Width = 900;

            // Add items to the list (Excluding if already present in the sender textbox)
            for (int i = 0; i < Host.AudioDeviceCount; i++)
            {
                string AudioDeviceName = Host.getAudioDeviceName(i);

                // Add item to list
                OMListItem Item = new OMListItem(AudioDeviceName, AudioDeviceName as object);
                Item.image = OImage.FromWebdingsFont(50, 50, "¯", Color.Gray);
                MenuPopup.AddMenuItem(Item);
            }
            

            // Add clear item
            OMListItem ClearItem = new OMListItem("None", null as object);
            ClearItem.image = OImage.FromWebdingsFont(50, 50, "r", Color.Red);
            MenuPopup.AddMenuItem(ClearItem);

            // Show menu and get selected item
            string AudioDevice = MenuPopup.ShowMenu(screen) as string;
            if (AudioDevice == null)
                // Clear zones
                txtBox.Text = "";
            else
                // Add new zone
                txtBox.Text = AudioDevice;
        }

        static void ZoneDetail_TextBox_SubZones_OnClick(OMControl sender, int screen)
        {
            OMTextBox txtBox = sender as OMTextBox;

            // Popup menu - Zones. Returns the tag in the selected item which holds the zone object.
            MenuPopup Zones = new MenuPopup("Select a zone", MenuPopup.ReturnTypes.Tag);

            // Add zones to the list (Excluding if already present in the sender textbox)
            foreach (Zone zone in Host.ZoneHandler.Zones)
            {
                // Make sure this a valid zone to be used as subzone
                if (zone.BlockSubZoneUsage)
                    continue;
                
                // Check for already present zone
                if (txtBox.Text.Contains(zone.Name))
                    continue;

                // Add zone to list
                OMListItem ZoneItem = new OMListItem(zone.Name, zone as object);
                ZoneItem.image = OImage.FromWebdingsFont(50, 50, "²", Color.Gray);
                Zones.AddMenuItem(ZoneItem);
            }

            // Add clear item
            OMListItem ClearItem = new OMListItem("Clear", null as object);
            ClearItem.image = OImage.FromWebdingsFont(50, 50, "r", Color.Red);
            Zones.AddMenuItem(ClearItem);

            // Show menu and get selected item
            Zone SelectedZone = Zones.ShowMenu(screen) as Zone;
            if (SelectedZone == null)
                // Clear zones
                txtBox.Text = ""; 
            else
                // Add new zone
                if (txtBox.Text == "")
                    txtBox.Text += SelectedZone.Name;
                else
                    txtBox.Text += "|" + SelectedZone.Name;
        }

        static void ZoneDetail_TextBox_ZoneScreen_OnClick(OMControl sender, int screen)
        {
            Host.ScreenShowIdentity(true,0.45f);

            // Get selected item
            object SelectedScreen = ZoneDetail_ScreenList.ShowMenu(screen);
            if (SelectedScreen != null)
                ((OMTextBox)sender).Text = ((int)SelectedScreen).ToString();
            Host.ScreenShowIdentity(false);
        }

        static void ZoneDetail_TextBox_ZoneScreen_OnOSKShow(OMControl sender, int screen)
        {
            Host.ScreenShowIdentity(2000);
        }

        static private OMControl OMControl_CloneAndAlignDown(string Name, OMControl Reference)
        {
            OMControl Target = null;
            if (Reference is OMTextBox)
                Target = ((OMTextBox)Reference).Clone(true) as OMTextBox;
            else
                Target = Reference.Clone() as OMControl;
            Target.Name = Name;
            Target.Top += Reference.Height + 10;
            return Target;
        }

        static void panelZonesDetail_Button_Cancel_OnClick(OMControl sender, int screen)
        {
            TmpZone = null;
            Host.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
        }

        static void panelZonesDetail_Button_Ok_OnClick(OMControl sender, int screen)
        {
            // Get data fields
            OMTextBox txtName = (OMTextBox)sender.Parent[screen, "ZoneDetail_TextBox_ZoneName"];
            txtName.BackgroundColor = Color.White;
            OMTextBox txtDescription = (OMTextBox)sender.Parent[screen, "ZoneDetail_TextBox_ZoneDescription"];
            txtDescription.BackgroundColor = Color.White;
            OMTextBox txtScreen = (OMTextBox)sender.Parent[screen, "ZoneDetail_TextBox_ZoneScreen"];
            txtScreen.BackgroundColor = Color.White;
            OMTextBox txtAudioDevice = (OMTextBox)sender.Parent[screen, "ZoneDetail_TextBox_AudioDevice"];
            txtAudioDevice.BackgroundColor = Color.White;
            OMTextBox txtSubZones = (OMTextBox)sender.Parent[screen, "ZoneDetail_TextBox_SubZones"];
            txtSubZones.BackgroundColor = Color.White;

            // Only validate data and create new zone if controls aren't disabled
            if (!txtName.Disabled)
            {

                // Check for unique name (only do this if we're adding a new zone)
                if (TmpZone == null)
                {
                    if (Host.ZoneHandler.Zones.Find(x => x.Name == txtName.Text) != null)
                    {
                        txtName.BackgroundColor = Color.Red;
                        dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                        dialogInfo.Header = "Data error";
                        dialogInfo.Text = "Zone name must be unique!\nPlease use a different name";
                        dialogInfo.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                        dialogInfo.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                        dialogInfo.ShowMsgBox(screen);
                        return;
                    }
                }

                // Check for valid screen number
                int ZoneScreen = -1;
                int.TryParse(txtScreen.Text, out ZoneScreen);
                if (ZoneScreen < 0 | ZoneScreen > Host.ScreenCount | String.IsNullOrEmpty(txtScreen.Text))
                {
                    txtScreen.BackgroundColor = Color.Red;
                    dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                    dialogInfo.Header = "Data error";
                    dialogInfo.Text = String.Format("Screen must be a valid screen!\nValid range is {0} to {1}", 0, Host.ScreenCount - 1);
                    dialogInfo.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                    dialogInfo.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                    dialogInfo.ShowMsgBox(screen);
                    return;
                }

                // Check for valid audio device
                int AudioDeviceInstance = Host.getAudioDeviceInstance(txtAudioDevice.Text);
                if (AudioDeviceInstance < 0)
                {
                    txtAudioDevice.BackgroundColor = Color.Red;
                    dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                    dialogInfo.Header = "Data error";
                    dialogInfo.Text = "AudioDevice must be a valid device!\nPlease correct the name";
                    dialogInfo.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                    dialogInfo.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                    dialogInfo.ShowMsgBox(screen);
                    return;
                }

                // Check for valid subzones
                List<Zone> SubZones = new List<Zone>();
                if (!String.IsNullOrEmpty(txtSubZones.Text))
                {
                    string[] Zones = txtSubZones.Text.Split(new char[] { '|' });
                    foreach (string zone in Zones)
                    {
                        Zone SubZone = Host.ZoneHandler.Zones.Find(x => x.Name == zone);
                        if (SubZone == null)
                        {
                            txtSubZones.BackgroundColor = Color.Red;
                            dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                            dialogInfo.Header = "Data error";
                            dialogInfo.Text = "Sub zones must be valid!\nPlease check sub zone names";
                            dialogInfo.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                            dialogInfo.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                            dialogInfo.ShowMsgBox(screen);
                            return;
                        }
                        else
                        {   // Valid zone, save to subzones list
                            SubZones.Add(SubZone);
                        }
                    }
                }

                // When we reach this level all data should be valid

                if (TmpZone == null)
                {   //let's create the new zone
                    Zone NewZone = new Zone(txtName.Text, txtDescription.Text, ZoneScreen, AudioDeviceInstance);
                    if (!Host.ZoneHandler.Add(NewZone))
                    {
                        dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                        dialogInfo.Header = "ZoneHandler error";
                        dialogInfo.Text = "Unable to create the new zone!\nPlease check zone data";
                        dialogInfo.Icon = OpenMobile.helperFunctions.Forms.icons.Error;
                        dialogInfo.Button = OpenMobile.helperFunctions.Forms.buttons.OK;
                        dialogInfo.ShowMsgBox(screen);
                        return;
                    }

                    // Add any subzones to the new zone
                    foreach (Zone SubZone in SubZones)
                        NewZone.Add(SubZone);
                }
                else
                {   // Update existing zone
                    TmpZone.Name = txtName.Text;
                    TmpZone.Description = txtDescription.Text;
                    TmpZone.Screen = ZoneScreen;
                    TmpZone.AudioDeviceInstance = AudioDeviceInstance;
                    TmpZone.SubZones.Clear();
                    // Add any subzones to the zone
                    foreach (Zone SubZone in SubZones)
                        TmpZone.Add(SubZone);

                    // Raise update event
                    Host.ZoneHandler.Update(TmpZone);
                }
            }

            TmpZone = null;
            Host.execute(eFunction.goBack, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
        }

        static void panelZonesDetail_Leaving(OMPanel sender, int screen)
        {

        }

        static void panelZonesDetail_Entering(OMPanel sender, int screen)
        {
        }

        static void Button_BottomBar_Restore_OnClick(OMControl sender, int screen)
        {
            RestoreZones(screen, true);
        }

        static void Button_BottomBar_Activate_OnClick(OMControl sender, int screen)
        {
            OMList list = sender.Parent[screen, "List_Zones"] as OMList;
            if (list.SelectedItem != null)
                TmpZone = list.SelectedItem.tag as Zone;
            else
                TmpZone = null;
            SetActiveZone(screen, TmpZone);
        }

        static void List_Zones_OnLongClick(OMControl sender, int screen)
        {
            // Get sender object
            OMList list = sender.Parent[screen, "List_Zones"] as OMList;

            // Show menu
            switch (ZoneMenu.ShowMenu(screen) as string)
            {
                case "mnuItemNewZone":
                    TmpZone = null;
                    Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "ZoneDetail");
                    Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
                    break;

                case "mnuItemEditZone":
                    if (list.SelectedItem != null)
                    {
                        TmpZone = list.SelectedItem.tag as Zone;
                        Host.execute(eFunction.TransitionToPanel, screen.ToString(), "OMSettings", "ZoneDetail");
                        Host.execute(eFunction.ExecuteTransition, screen.ToString(), eGlobalTransition.CrossfadeFast.ToString());
                    }
                    break;

                case "mnuItemRemoveZone":
                    if (list.SelectedItem != null)
                    {
                        TmpZone = list.SelectedItem.tag as Zone;
                        DeleteZone(screen, TmpZone, true);
                    }
                    break;

                case "mnuItemSetActive":
                    if (list.SelectedItem != null)
                    {
                        TmpZone = list.SelectedItem.tag as Zone;
                        SetActiveZone(screen, TmpZone);
                    }
                    break;

                default:
                    break;
            }
            
        }

        static private void RestoreZones(int screen, bool confirm)
        {
            // Confirm action
            if (confirm)
            {
                dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                dialogInfo.Header = "Reset zone setup to default?";
                dialogInfo.Text = "Are you sure you want to reset zone setup to default? All user created zones will be removed!";
                dialogInfo.Icon = icons.Question;
                dialogInfo.Button = buttons.Yes | buttons.No;
                if (dialogInfo.ShowMsgBox(screen) == buttons.No)
                    return;
            }

            Host.ZoneHandler.CreateDefaultZones();
        }

        static private void DeleteZone(int screen, Zone zone, bool confirm)
        {
            if (zone == null)
                return;

            // Confirm deletion
            if (confirm)
            {
                dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
                dialogInfo.Header = "Remove zone?";
                dialogInfo.Text = String.Format("Are you sure you want to remove zone\n\"{0}\"", zone.Name);
                dialogInfo.Icon = icons.Question;
                dialogInfo.Button = buttons.Yes | buttons.No;
                if (dialogInfo.ShowMsgBox(screen) == buttons.No)
                    return;
            }

            Host.ZoneHandler.Remove(zone);
        }

        static private void SetActiveZone(int screen, Zone zone)
        {
            if (zone == null)
            {
                dialog dialogWarning = new OpenMobile.helperFunctions.Forms.dialog();
                dialogWarning.Header = "No data available!";
                dialogWarning.Text = "Please select a zone in the zonelist before using this function!";
                dialogWarning.Icon = icons.Exclamation;
                dialogWarning.Button = buttons.OK;
                dialogWarning.ShowMsgBox(screen);
                return;
            }

            dialog dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
            dialogInfo.Header = "Set active zone?";
            dialogInfo.Text = String.Format("In the next dialog, please select the screen you want to activate for the zone \"{0}\"", zone.Name);
            dialogInfo.Icon = icons.Information;
            dialogInfo.Button = buttons.OK;
            dialogInfo.ShowMsgBox(screen);

            // Get the screen to use for activation
            // Get selected item
            int SelectedScreen = -1;
            object o = ZoneDetail_ScreenList.ShowMenu(screen);
            if (o == null)
                return;
            SelectedScreen = (int)o;

            // Confirm operation
            dialogInfo = new OpenMobile.helperFunctions.Forms.dialog();
            dialogInfo.Header = "Set active zone?";
            dialogInfo.Text = String.Format("Do you want to set the zone \"{0}\" as currently active for screen {1}", zone.Name, SelectedScreen);
            dialogInfo.Icon = icons.Question;
            dialogInfo.Button = buttons.Yes | buttons.No;
            if (dialogInfo.ShowMsgBox(screen) == buttons.No)
                return;

            Host.ZoneHandler.SetActiveZone(SelectedScreen, zone);
        }

        static private MediaEvent MediaEvents = new MediaEvent(Host_OnMediaEvent);

        static void panelZones_Entering(OMPanel sender, int screen)
        {
            // Connect host events
            Host.OnMediaEvent += MediaEvents;

            // Update zone list
            ZoneList_Update(screen);
        }

        static void panelZones_Leaving(OMPanel sender, int screen)
        {
            // Disconnect host events
            Host.OnMediaEvent -= MediaEvents;
        }

        static void Host_OnMediaEvent(eFunction function, Zone zone, string arg)
        {
            if ((function == eFunction.ZoneAdded) | (function == eFunction.ZoneRemoved) | (function == eFunction.ZoneSetActive) | (function == eFunction.ZoneUpdated))
            {
                BuiltInComponents.Host.ForEachScreen(delegate(int screen)
                {
                    ZoneList_Update(screen);
                });
                return;
            }
        }

        static private void ZoneList_Update(int screen)
        {
            OMList List = (OMList)Manager[screen, "Zones"][screen, "List_Zones"];
            lock (List)
            {
                List.Clear();

                foreach (Zone z in Host.ZoneHandler.Zones)
                {
                    int[] ActiveScreens = Host.ZoneHandler.GetScreensForActiveZone(z);
                    string sActiveScreens = "";
                    bool first = true;
                    foreach (int i in ActiveScreens)
                    {
                        if (first)
                        {
                            sActiveScreens += String.Format("{0}", i.ToString());
                            first = false;
                        }
                        else
                            sActiveScreens += String.Format(",{0}", i.ToString());
                    }

                    OMListItem item = new OMListItem(String.Format("{0}{1}", (ActiveScreens.Length > 0 ? String.Format("(*{0}) ", sActiveScreens) : ""), z.Name), z.Description, subItemformat);
                    item.tag = z;
                    item.image = Host.getSkinImage("Mixer").image;
                    List.Add(item);
                }
            }
        }

    }
}
