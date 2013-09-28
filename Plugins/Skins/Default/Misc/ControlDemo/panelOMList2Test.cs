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
using OpenMobile.helperFunctions.Graphics;
using OpenMobile.Media;


namespace ControlDemo
{
    public static class panelOMList2Test
    {
        static IPluginHost Host;
        static ScreenManager Manager;
        static string PluginName;
        static OMPanel pOMList2Test = null;
        static OMObjectList.ListItem ListItem_MessageType1 = null;
        static OMObjectList.ListItem ListItem_MessageType2 = null;
        static imageItem imgPanel_Background_Highlighted;
        static imageItem imgPanel_Background;

        public static void Initialize(string pluginName, ScreenManager manager, IPluginHost host)
        {
            // Save reference to host objects
            Host = host;
            Manager = manager;
            PluginName = pluginName;

            pOMList2Test = new OMPanel("OMList2Test");

            OMLabel lblHeader = new OMLabel("lblHeader", 0, 100, 1000, 35);
            lblHeader.TextAlignment = Alignment.CenterCenter;
            lblHeader.Text = pOMList2Test.Name;
            pOMList2Test.addControl(lblHeader);

            OMButton btnAddItem = DefaultControls.GetButton("btnAddItem", 0, 100, 180, 90, "", "Add item");
            btnAddItem.OnClick += new userInteraction(btnAddItem_OnClick);
            pOMList2Test.addControl(btnAddItem);
            OMButton btnAddItem100 = DefaultControls.GetButton("btnAddItem100", 0, 200, 180, 90, "", "Add 100 items");
            btnAddItem100.OnClick += new userInteraction(btnAddItem100_OnClick);
            pOMList2Test.addControl(btnAddItem100);
            OMButton btnClear = DefaultControls.GetButton("btnClear", 0, 300, 180, 90, "", "Clear items");
            btnClear.OnClick += new userInteraction(btnClear_OnClick);
            pOMList2Test.addControl(btnClear);

            OMSlider Slider_RotationY = new OMSlider("Slider_RotationY", 200, 100, 400, 25, 12, 40);
            Slider_RotationY.Slider = Host.getSkinImage("Slider");
            Slider_RotationY.SliderBar = Host.getSkinImage("Slider.Bar");
            Slider_RotationY.Minimum = -180;
            Slider_RotationY.Maximum = 180;
            Slider_RotationY.Value = 0;
            Slider_RotationY.OnSliderMoved += new OMSlider.slidermoved(Slider_RotationY_OnSliderMoved);
            pOMList2Test.addControl(Slider_RotationY);

            // List control
            OMObjectList lstListControl = new OMObjectList("lstListControl", 200, 150, 400, 400);
            lstListControl._3D_CameraData = new _3D_Control(new OpenMobile.Math.Vector3d(0, -35, 0));
            //lstListControl.SkinDebug = true;
            //lstListControl.BackgroundColor = Color.Black;
            //lstListControl.SoftEdges = FadingEdge.GraphicSides.Bottom | FadingEdge.GraphicSides.Left | FadingEdge.GraphicSides.Right | FadingEdge.GraphicSides.Top;
            pOMList2Test.addControl(lstListControl);

            #region Configure list item

            OMObjectList.ListItem ItemBase = new OMObjectList.ListItem();
            OMBasicShape shpListItemBackground = new OMBasicShape("shpListItemBackground", 0, 0, lstListControl.Width, 70,
                new ShapeData(shapes.Rectangle, lstListControl.BackgroundColor, Color.Empty, 1));
            ItemBase.Add(shpListItemBackground);
            OMLabel lblListItemHeader = new OMLabel("lblListItemHeader", 0, 0, lstListControl.Width, 45, "lblListItemHeader");
            lblListItemHeader.Font = new Font(Font.GenericSansSerif, 30F);
            lblListItemHeader.TextAlignment = Alignment.CenterLeft;
            ItemBase.Add(lblListItemHeader);
            OMLabel lblListItemDescription = new OMLabel("lblListItemDescription", 0, 45, lstListControl.Width, 25, "lblListItemDescription");
            lblListItemDescription.TextAlignment = Alignment.CenterLeft;
            lblListItemDescription.Color = Color.FromArgb(128, lblListItemHeader.Color);
            lblListItemDescription.Font = new Font(Font.GenericSansSerif, 21F);
            ItemBase.Add(lblListItemDescription);
            OMCheckbox chkListItemCheckBox = new OMCheckbox("chkListItemCheckBox", lstListControl.Width - 40, 5, 30, 30);
            chkListItemCheckBox.OutlineColor = lblListItemHeader.Color;
            chkListItemCheckBox.CheckedColor = lblListItemDescription.Color;
            chkListItemCheckBox.OnClick += new userInteraction(chkListItemCheckBox_OnClick);
            ItemBase.Add(chkListItemCheckBox);
            OMBasicShape shpListItemSeparator = new OMBasicShape("shpListItemSeparator", 0, 70, lstListControl.Width, 1,
                new ShapeData(shapes.Rectangle, lblListItemDescription.Color, Color.Empty, 1));
            ItemBase.Add(shpListItemSeparator);
            // Method for adding list items
            ItemBase.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
            {
                // Header text
                OMLabel lblItemHeader = item["lblListItemHeader"] as OMLabel;
                lblItemHeader.Text = values[0] as string;

                // Description text
                OMLabel lblItemDescription = item["lblListItemDescription"] as OMLabel;
                lblItemDescription.Text = values[1] as string;

            };
            // Method for selecting list items
            ItemBase.Action_Select = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                ShapeData shpData = shpItemBackground.ShapeData;
                shpData.FillColor = BuiltInComponents.SystemSettings.SkinFocusColor;
                shpItemBackground.ShapeData = shpData;
            };
            // Method for deselecting list items
            ItemBase.Action_Deselect = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                ShapeData shpData = shpItemBackground.ShapeData;
                shpData.FillColor = Color.Black;
                shpItemBackground.ShapeData = shpData;
            };
            // Method for highlighting list items
            ItemBase.Action_Highlight = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                ShapeData shpData = shpItemBackground.ShapeData;
                shpData.BorderColor = BuiltInComponents.SystemSettings.SkinFocusColor;
                shpData.BorderSize = 1;
                shpItemBackground.ShapeData = shpData;
            };
            // Method for dehighlighting list items
            ItemBase.Action_Unhighlight = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                ShapeData shpData = shpItemBackground.ShapeData;
                shpData.BorderColor = BuiltInComponents.SystemSettings.SkinFocusColor;
                shpData.BorderSize = 0;
                shpItemBackground.ShapeData = shpData;
            };
            lstListControl.ItemBase = ItemBase;

            #endregion

            OMButton btnList1_Scroll1 = DefaultControls.GetButton("btnList1_Scroll1", 200, 130, 100, 30, "", "Top");
            btnList1_Scroll1.OnClick += new userInteraction(btnList1_Scroll1_OnClick);
            pOMList2Test.addControl(btnList1_Scroll1);
            OMButton btnList1_Scroll2 = DefaultControls.GetButton("btnList1_Scroll2", 300, 130, 100, 30, "", "Btm");
            btnList1_Scroll2.OnClick += new userInteraction(btnList1_Scroll2_OnClick);
            pOMList2Test.addControl(btnList1_Scroll2);


            OMButton btnList2_AddItem_Type2 = DefaultControls.GetButton("btnList2_AddItem", 0, 500, 180, 90, "", "Add msg to list2");
            btnList2_AddItem_Type2.OnClick += new userInteraction(btnList2_AddItem_Type2_OnClick);
            pOMList2Test.addControl(btnList2_AddItem_Type2);

            // List control 2
            OMObjectList lstListControl2 = new OMObjectList("lstListControl2", 600, 150, 400, 400);
            //lstListControl.SkinDebug = true;
            //lstListControl2.BackgroundColor = Color.Black;
            //lstListControl2.SoftEdges = FadingEdge.GraphicSides.Bottom | FadingEdge.GraphicSides.Left | FadingEdge.GraphicSides.Right | FadingEdge.GraphicSides.Top;
            pOMList2Test.addControl(lstListControl2);
            ListItem_MessageType1 = CreateMessageListItem(lstListControl2, 0, 70, 300, BuiltInComponents.SystemSettings.SkinFocusColor, BuiltInComponents.SystemSettings.SkinTextColor, lstListControl2.BackgroundColor);
            ListItem_MessageType2 = CreateMessageListItem(lstListControl2, 1, 20, 300, Color.Turquoise, BuiltInComponents.SystemSettings.SkinTextColor, lstListControl2.BackgroundColor);

            #region SlideUpPanel

            OMButton btnList3_AddItem = DefaultControls.GetButton("btnList3_AddItem", 0, 400, 180, 90, "", "Add item to list 3");
            btnList3_AddItem.OnClick += new userInteraction(btnList3_AddItem_OnClick);
            pOMList2Test.addControl(btnList3_AddItem);

            OMButton Button_PanelSlideIn = DefaultControls.GetHorisontalEdgeButton("Button_SlideIn", 420, 540, 160, 70, "5", "");
            Button_PanelSlideIn.OnClick += new userInteraction(Button_PanelSlideIn_OnClick);
            pOMList2Test.addControl(Button_PanelSlideIn);

            PanelOutlineGraphic.GraphicData gd = new PanelOutlineGraphic.GraphicData();
            gd.Width = 1100;
            gd.Height = 300;
            gd.ShadowType = PanelOutlineGraphic.ShadowTypes.None;
            gd.GraphicPath = OpenMobile.Graphics.GDI.GraphicsPathHelpers.GetPath_RoundedRectangleArchedTop(new Rectangle(0, 15, gd.Width, gd.Height - 15), 30, 50);
            imgPanel_Background = new imageItem(PanelOutlineGraphic.GetImage(ref gd));

            gd = new PanelOutlineGraphic.GraphicData();
            gd.Width = 1100;
            gd.Height = 300;
            gd.GraphicPath = OpenMobile.Graphics.GDI.GraphicsPathHelpers.GetPath_RoundedRectangleArchedTop(new Rectangle(0, 15, gd.Width, gd.Height - 15), 30, 50);
            imgPanel_Background_Highlighted = new imageItem(PanelOutlineGraphic.GetImage(ref gd));

            OMImage Image_PanelSlideIn_Background = new OMImage("Image_PanelSlideIn_Background", -50, 580, imgPanel_Background);
            pOMList2Test.addControl(Image_PanelSlideIn_Background);

            #region List control 3

            OMObjectList lstListControl3 = new OMObjectList("lst_PanelSlideIn_ListControl3", 0, 600, 1000, 150);
            //lstListControl3.SkinDebug = true;
            //lstListControl3.BackgroundColor = Color.Black;
            lstListControl3.ItemBase = ListControl3_CreateMessageListItem();
            lstListControl3.ScrollBar_ColorNormal = Color.Transparent;
            pOMList2Test.addControl(lstListControl3);

            // Calculate scroll points that's placed along a curve to match the curved top of the panel it belongs in
            List<Point> Points = new List<Point>();
            double r = 3500;
            double h = 30;
            // Calculate one point for each pixel on screen + additional motions 
            for (int i = -100; i < lstListControl3.Width + 100; i++)
                Points.Add(new Point(i, (int)-((h - r) + r * Math.Sin(Math.Acos((i - (lstListControl3.Width / 2)) / r)) - h) + 60));

            lstListControl3.ScrollPoints = Points;
            lstListControl3.MainScrollDirection = OMContainer.ScrollDirections.X;

            // Method for controling the rendering order of the list items
            // This calculation finds the card that's located in the center and renderes this on top of all the rest
            lstListControl3.ListItems_RenderOrderCalc = delegate(List<ControlGroup> controls, Rectangle Offset, ref List<int> renderOrder)
            {   
                // Object in center on screen should be rendered on top of all others
                int CenterObjectIndex = renderOrder.Count / 2;

                //for (int i = 0; i < renderOrder.Count; i++)
                //{
                //    for (int i2 = 0; i2 < controls[renderOrder[i]].Count; i2++)
                //    {
                //        if (i == CenterObjectIndex)
                //        {
                //            if (controls[renderOrder[i]][i2].Width != 92)
                //            {
                //                controls[renderOrder[i]][i2].Left -= 10;
                //                controls[renderOrder[i]][i2].Width = 92;
                //                controls[renderOrder[i]][i2].Height = 125;
                //            }
                //        }
                //        else
                //        {
                //            if (controls[renderOrder[i]][i2].Width != 72)
                //            {
                //                controls[renderOrder[i]][i2].Left += 10;
                //                controls[renderOrder[i]][i2].Width = 72;
                //                controls[renderOrder[i]][i2].Height = 96;
                //            }
                //        }
                //    }
                //}

                // Swap rendering order to ensure center object is drawn first
                renderOrder.Reverse(CenterObjectIndex, renderOrder.Count - CenterObjectIndex);

                //// Set rendering properties for first half
                //for (int i = 0; i < CenterObjectIndex; i++)
                //{
                //    for (int i2 = 0; i2 < controls[renderOrder[i]].Count; i2++)
                //    {
                //        //OMControl control = controls[renderOrder[i]][i2];
                //        OMControl control = controls[renderOrder[i]]["shpOverlay"];
                //        control.Opacity = 125;
                //    }
                //}
                //// Set rendering properties for last half
                //for (int i = CenterObjectIndex; i < renderOrder.Count; i++)
                //{
                //    for (int i2 = 0; i2 < controls[renderOrder[i]].Count; i2++)
                //    {
                //        OMControl control = controls[renderOrder[i]]["shpOverlay"];                         
                //        control.Opacity = 125;
                //    }
                //}
                //// Set rendering properties for center object
                //for (int i2 = 0; i2 < controls[renderOrder[renderOrder.Count-1]].Count; i2++)
                //{
                //    //OMControl control = controls[renderOrder[renderOrder.Count - 1]][i2];
                //    OMControl control = controls[renderOrder[renderOrder.Count - 1]]["shpOverlay"];
                //    control.Opacity = 0;
                //}
               
            };

            #endregion

            lstListControl3.OnSelectedIndexChanged += new OMObjectList.IndexChangedDelegate(lstListControl3_OnSelectedIndexChanged);

            OMLabel lbl_PanelSlideIn_ListControl3Info = new OMLabel("lbl_PanelSlideIn_ListControl3Info", 400, 725, 200, 30);
            lbl_PanelSlideIn_ListControl3Info.Font = new Font(Font.Arial, 18);
            pOMList2Test.addControl(lbl_PanelSlideIn_ListControl3Info);

            #endregion

            pOMList2Test.Entering += new PanelEvent(pOMList2Test_Entering);
            pOMList2Test.Leaving += new PanelEvent(pOMList2Test_Leaving);

            // Load panel
            manager.loadPanel(pOMList2Test);
        }

        static void Slider_RotationY_OnSliderMoved(OMSlider sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl"] as OMObjectList;
            OMSlider sldrY = sender.Parent[screen, "Slider_RotationY"] as OMSlider;
            if (lst != null && sldrY != null)
            {
                lst._3D_CameraData = new _3D_Control(new OpenMobile.Math.Vector3d(sldrY.Value, 0, 0),
                    new OpenMobile.Math.Vector3d(0, 0, 0),
                    new OpenMobile.Math.Vector3d(0, 0, 0),
                    new OpenMobile.Math.Vector3d(0, 0, 0),
                    0);
            }
        }

        static void pOMList2Test_Leaving(OMPanel sender, int screen)
        {
            OM.Host.UIHandler.Bars_Show(screen, false, OpenMobile.UI.UIHandler.Bars.Bottom);
        }

        static void pOMList2Test_Entering(OMPanel sender, int screen)
        {
            OM.Host.UIHandler.Bars_Hide(screen, false, OpenMobile.UI.UIHandler.Bars.Bottom);
        }

        static void lstListControl3_OnSelectedIndexChanged(OMObjectList sender, int screen)
        {
            OMLabel lbl = sender.Parent[screen, "lbl_PanelSlideIn_ListControl3Info"] as OMLabel;
            OMButton btn = sender.SelectedItem["btnCard"] as OMButton;
            if (btn != null)
                lbl.Text = btn.Image.name;
        }

        static void btnList1_Scroll1_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl"] as OMObjectList;
            if (lst == null) return;
            lst.ScrollToControl("lblListItemHeader:1", false);
        }

        static void btnList1_Scroll2_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl"] as OMObjectList;
            if (lst == null) return;
            lst.ScrollToControl("lblListItemHeader:99", false);
        }

        static string[] CardNames = new string[52]
        {
            "2C","2D","2H","2S", "3C","3D","3H","3S", "4C","4D","4H","4S", "5C","5D","5H","5S", "6C","6D","6H","6S", "7C","7D","7H","7S", "8C","8D","8H","8S", "9C","9D","9H","9S", "10C","10D","10H","10S", "JC","JD","JH","JS", "QC","QD","QH","QS", "KC","KD","KH","KS", "AC","AD","AH","AS"
        };
        static void btnList3_AddItem_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lst_PanelSlideIn_ListControl3"] as OMObjectList;
            if (lst == null) return;

            // Add list items
            for (int i = 0; i < CardNames.Length; i++)
                lst.AddItemFromItemBase(new object[1] { String.Format("Cards|{0}", CardNames[i]) }, ControlDirections.Right);
        }

        static private OMObjectList.ListItem ListControl3_CreateMessageListItem()
        {
            OMObjectList.ListItem ItemBase1 = new OMObjectList.ListItem();

            ItemBase1.ItemSize = new Size(50, 96);
            OMButton btnCard = new OMButton("btnCard", 0, 0, 72, 96);
            ItemBase1.Add(btnCard);
            OMLabel lblCard = new OMLabel("lblCard", 0, 100, 72, 15);
            lblCard.Font = new Font(Font.Arial, 12);
            ItemBase1.Add(lblCard);
            OMBasicShape shpOverlay = new OMBasicShape("shpOverlay", 0, 0, 72, 115,
                new ShapeData(shapes.Rectangle, Color.Black, Color.Empty, 0));
            shpOverlay.Opacity = 125;
            shpOverlay.NoUserInteraction = true;
            ItemBase1.Add(shpOverlay);
            //OMImage imgCard = new OMImage("imgCard", 0, 0);

            // Method for setting list items 
            ItemBase1.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
            {
                // Item image
                //OMImage imgItemCard = item["imgCard"] as OMImage;
                //imgItemCard.Image = Host.getSkinImage(values[0] as string);

                OMLabel lblItemCard = item["lblCard"] as OMLabel;
                string text = values[0] as string;
                lblItemCard.Text = text.Substring(text.LastIndexOf('|')+1);

                OMButton btnItemCard = item["btnCard"] as OMButton;
                btnItemCard.Image = Host.getSkinImage(values[0] as string);
                btnItemCard.OnClick += new userInteraction(btnItemCard_OnClick);
            };

            // Method for highlighting list items
            ItemBase1.Action_Highlight = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpOverlay"] as OMBasicShape;
                if (shpItemBackground.Opacity > 0)  // Dont change opacity of selected item
                    shpItemBackground.Opacity = 50;
            };
            // Method for unhighlighting list items
            ItemBase1.Action_Unhighlight = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpOverlay"] as OMBasicShape;
                if (shpItemBackground.Opacity > 0)  // Dont change opacity of selected item
                    shpItemBackground.Opacity = 125;
            };
            // Method for selecting list items
            ItemBase1.Action_Select = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpOverlay"] as OMBasicShape;
                shpItemBackground.Opacity = 0;
            };
            // Method for deselecting list items
            ItemBase1.Action_Deselect = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                OMBasicShape shpItemBackground = item["shpOverlay"] as OMBasicShape;
                shpItemBackground.Opacity = 125;
            };


            return ItemBase1;
        }

        static void btnItemCard_OnClick(OMControl sender, int screen)
        {
            Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("{0}", sender.Name)));

            OMObjectList lst = sender.Parent[screen, "lst_PanelSlideIn_ListControl3"] as OMObjectList;
            if (lst == null) return;

            lst.ScrollToControl(sender, true);
        }

        static private OMObjectList.ListItem CreateMessageListItem(OMObjectList lst, int frameStyle, int left, int width, Color BaseColor, Color HighlightForegroundColor, Color HighlightBackgroundColor)
        {
            OMObjectList.ListItem ItemBase1 = new OMObjectList.ListItem();
            OMBasicShape shpListItemBackground = new OMBasicShape("shpListItemBackground", 0, 0, lst.Width, 300,
                new ShapeData(shapes.Rectangle, lst.BackgroundColor));
            ItemBase1.Add(shpListItemBackground);

            OMLabel lblListItemHeader = new OMLabel("lblListItemHeader", 0, 0, lst.Width, 15, "lblListItemHeader");
            lblListItemHeader.Font = new Font(Font.GenericSansSerif, 12F);
            lblListItemHeader.TextAlignment = Alignment.CenterCenter;
            lblListItemHeader.Color = BaseColor;
            ItemBase1.Add(lblListItemHeader);
            OMBasicShape shpItemFrame = new OMBasicShape("shpItemFrame", left + 5, 15, width, 270,
                new ShapeData(shapes.Polygon, Color.Transparent, lblListItemHeader.Color, 2));
            ItemBase1.Add(shpItemFrame);
            OMLabel lblListItemText = new OMLabel("lblListItemText", left + 10, 20, shpItemFrame.Width - 10, 260, "lblListItemText");
            lblListItemText.Font = new Font(Font.GenericSansSerif, 16F);
            lblListItemText.TextAlignment = Alignment.WordWrap | Alignment.TopLeft;
            lblListItemText.Color = BaseColor;
            ItemBase1.Add(lblListItemText);

            // Method for adding list items
            ItemBase1.Action_SetItemInfo = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item, object[] values)
            {
                // Item header
                OMLabel lblItemHeader = item["lblListItemHeader"] as OMLabel;
                lblItemHeader.Text = values[0] as string;

                // Item text
                OMLabel lblItemText = item["lblListItemText"] as OMLabel;
                lblItemText.Text = values[1] as string;
                Size textSize = lblItemText.GetSizeOfContainedText();

                // Adjust size of control to match text
                lblItemText.Height = textSize.Height;
                lblItemText.Width = textSize.Width;
                switch (frameStyle)
                {
                    case 0:
                        lblItemText.Left = lst.Region.Width - lblItemText.Width - (lst.Region.Width - width - left);
                        break;
                    case 1:
                        //lblItemText.Left = lst.Region.Right - lblItemText.Width - (lst.Region.Left - width);
                        break;
                }

                // Adjust size and placement of header text
                lblItemHeader.Left = lblItemText.Left;
                lblItemHeader.Width = lblItemText.Width;

                // Item frame
                OMBasicShape shpFrame = item["shpItemFrame"] as OMBasicShape;
                shpFrame.Height = lblItemText.Height + 10;
                shpFrame.Width = lblItemText.Width + 10;
                shpFrame.Left = lblItemText.Left - 5;


                ShapeData shpData = shpFrame.ShapeData;

                // Set graphic points to create the frame around the object at a proper size
                switch (frameStyle)
                {
                    case 0:
                        {
                            shpData.GraphicPoints = new Point[8]
                            {
                                new Point(0,0),
                                new Point(shpFrame.Width,0),
                                new Point(shpFrame.Width,(shpFrame.Height/2) - 10),
                                new Point(shpFrame.Width + 10, (shpFrame.Height/2)),
                                new Point(shpFrame.Width,(shpFrame.Height/2) + 10),
                                new Point(shpFrame.Width,shpFrame.Height),
                                new Point(0,shpFrame.Height),
                                new Point(0,0)
                            };
                        }
                        break;
                    case 1:
                        {
                            shpData.GraphicPoints = new Point[8]
                            {
                                new Point(0,0),
                                new Point(shpFrame.Width,0),
                                new Point(shpFrame.Width,shpFrame.Height),
                                new Point(0,shpFrame.Height),
                                new Point(0,(shpFrame.Height/2) + 10),
                                new Point(-10, (shpFrame.Height/2)),
                                new Point(0,(shpFrame.Height/2) - 10),
                                new Point(0,0)
                            };
                        }
                        break;
                }

                shpFrame.ShapeData = shpData;

                // Item background
                OMBasicShape shpItemBackground = item["shpListItemBackground"] as OMBasicShape;
                shpItemBackground.Height = shpFrame.Height + 30;

            };
            // Method for selecting list items
            ItemBase1.Action_Select = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                // Item text
                OMLabel lblItemText = item["lblListItemText"] as OMLabel;
                lblItemText.Color = HighlightForegroundColor;

                // Item header
                OMLabel lblItemHeader = item["lblListItemHeader"] as OMLabel;
                lblItemHeader.Color = lblItemText.Color;

                // Item frame
                OMBasicShape shpFrame = item["shpItemFrame"] as OMBasicShape;
                ShapeData shpData = shpFrame.ShapeData;
                shpData.BorderColor = HighlightForegroundColor;
                shpData.FillColor = HighlightBackgroundColor;
                shpFrame.ShapeData = shpData;
            };
            // Method for deselecting list items
            ItemBase1.Action_Deselect = delegate(OMObjectList sender, int screen, OMObjectList.ListItem item)
            {
                // Item text
                OMLabel lblItemText = item["lblListItemText"] as OMLabel;
                lblItemText.Color = BaseColor;

                // Item header
                OMLabel lblItemHeader = item["lblListItemHeader"] as OMLabel;
                lblItemHeader.Color = lblItemText.Color;

                // Item frame
                OMBasicShape shpFrame = item["shpItemFrame"] as OMBasicShape;
                ShapeData shpData = shpFrame.ShapeData;
                shpData.BorderColor = BaseColor;
                shpData.FillColor = Color.Transparent;
                shpFrame.ShapeData = shpData;
            };
            return ItemBase1;
        }

        static int ConversationState = -1;
        static private string GetConversationText(out int msgType)
        {
            ConversationState++;
            switch (ConversationState)
            {
                case 0:
                    msgType = 1;
                    return "Hi how are you?";
                case 1:
                    msgType = 2;
                    return "I'm fine. Thanks!";
                case 2:
                    msgType = 1;
                    return "What do you think of this cool frontend called OM?";
                case 3:
                    msgType = 2;
                    return "I like it! Seems really feature packed and cool!";
                case 4:
                    msgType = 1;
                    return "Yeah and it uses OpenGl for graphics so it's really fast as well.";
                case 5:
                    msgType = 1;
                    return "Oh and it's also packed full of animations and advanced support for custom effects!";
                case 6:
                    msgType = 2;
                    return "Cool!";
                case 7:
                    msgType = 1;
                    return "It also has a lot of advanced controls available to skinners like this highly flexible list control that's used for this message list.";
                case 8:
                    msgType = 2;
                    return "Sweet! Sounds like something I gotta test.\nWhere can I get it?";
                case 9:
                    msgType = 1;
                    return "Go to http://sourceforge.net/projects/openmobile/ to grab the source or check out http://www.mp3car.com/openmobile/ for forums and additional info.";
                case 10:
                    msgType = 2;
                    ConversationState = -1;
                    return "Cool, I'll do that!";
                default:
                    break;
            }
            msgType = 1;
            ConversationState = -1;
            return "";
        }

        static void btnList2_AddItem_Type2_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl2"] as OMObjectList;
            if (lst == null) return;

            // Add list items
            int msgType = 1;
            string text = GetConversationText(out msgType);
            if (msgType == 1)
                lst.AddItem(ListItem_MessageType1, new object[2] { DateTime.Now.ToShortTimeString(), text }, ControlDirections.Down);
            else
                lst.AddItem(ListItem_MessageType2, new object[2] { DateTime.Now.ToShortTimeString(), text }, ControlDirections.Down);
        }

        static void btnList2_AddItem_Type1_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl2"] as OMObjectList;
            if (lst == null) return;

            // Add list items
            lst.AddItem(ListItem_MessageType1, new object[2] { DateTime.Now.ToShortTimeString(), "Text from me" }, ControlDirections.Down);
        }

        static void btnClear_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl"] as OMObjectList;
            if (lst == null) return;
            lst.Clear();
            lst = sender.Parent[screen, "lstListControl2"] as OMObjectList;
            if (lst == null) return;
            lst.Clear();
            lst = sender.Parent[screen, "lst_PanelSlideIn_ListControl3"] as OMObjectList;
            if (lst == null) return;
            lst.Clear();
            ConversationState = -1;            
        }

        static void btnAddItem100_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl"] as OMObjectList;
            if (lst == null) return;

            // Add list items
            for (int i = 0; i < 100; i++)
                lst.AddItemFromItemBase(new object[2] { String.Format("New item {0}", lst.Items.Count), String.Format("Description for item {0}", lst.Items.Count) }, ControlDirections.Down);
        }

        static void chkListItemCheckBox_OnClick(OMControl sender, int screen)
        {
            Host.UIHandler.InfoBanner_Show(screen, new InfoBanner(String.Format("{0}", sender.Name)));
        }

        static void btnAddItem_OnClick(OMControl sender, int screen)
        {
            OMObjectList lst = sender.Parent[screen, "lstListControl"] as OMObjectList;
            if (lst == null) return;

            // Add list items
            lst.AddItemFromItemBase(new object[2] { String.Format("New item {0}", lst.Items.Count), String.Format("Description for item {0}", lst.Items.Count) }, ControlDirections.Down);
        }

        private static bool[] MediaBarVisible = null;

        static void Button_PanelSlideIn_OnClick(OMControl sender, int screen)
        {
            // Initialize variable 
            if (MediaBarVisible == null)
                MediaBarVisible = new bool[Host.ScreenCount];

            List<OMControl> Controls = sender.Parent.getPanelAtScreen(screen).Controls.FindAll(x => x.Name.Contains("_PanelSlideIn_"));
            OMControl MainControl = sender.Parent[screen, "Button_SlideIn"];
            OMImage img = (OMImage)sender.Parent[screen, "Image_PanelSlideIn_Background"];

            if (!MediaBarVisible[screen])
            {   // Show media bar
                if (img != null)
                    img.Image = imgPanel_Background_Highlighted;
                AnimateAndMoveControls(true, screen, 380, 540, 1.8f, MainControl, Controls);
                if (MainControl != null)
                    DefaultControls.UpdateHorisontalEdgeButton((OMButton)MainControl, 420, 540, 160, 70, "6", "");
            }
            else
            {   // Hide media bar
                AnimateAndMoveControls(false, screen, 380, 540, 1.8f, MainControl, Controls);
                if (img != null)
                    img.Image = imgPanel_Background;
                if (MainControl != null)
                    DefaultControls.UpdateHorisontalEdgeButton((OMButton)MainControl, 420, 540, 160, 70, "5", "");
            }
        }

        private static void AnimateAndMoveControls(bool up, int screen, int TopPos, int BottomPos, float AnimationSpeed, OMControl MainControl, List<OMControl> ControlsToMove)
        {
            // Calculate relative placements of media controls
            int[] RelativePlacements = new int[ControlsToMove.Count];
            for (int i = 0; i < RelativePlacements.Length; i++)
                RelativePlacements[i] = ControlsToMove[i].Top - MainControl.Top;

            if (up)
            {   // Move media bar up                
                int EndPos = TopPos;
                int Top = MainControl.Top;

                if (AnimationSpeed == 0)
                    AnimationSpeed = 0.9f;

                SmoothAnimator Animation = new SmoothAnimator(AnimationSpeed);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    Top -= AnimationStep;
                    if (Top <= EndPos)
                    {   // Animation has completed
                        MainControl.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        MainControl.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = true;
            }
            else
            {   // Move media bar down
                int EndPos = BottomPos;
                int Top = MainControl.Top;

                SmoothAnimator Animation = new SmoothAnimator(0.9f);
                Animation.Animate(delegate(int AnimationStep, float AnimationStepF, double AnimationDurationMS)
                {
                    Top += AnimationStep;
                    if (Top >= EndPos)
                    {   // Animation has completed
                        MainControl.Top = EndPos;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                        return false;
                    }
                    else
                    {   // Move object down
                        MainControl.Top = Top;
                        for (int i = 0; i < RelativePlacements.Length; i++)
                            ControlsToMove[i].Top = MainControl.Top + RelativePlacements[i];
                    }
                    return true;
                });
                MediaBarVisible[screen] = false;
            }
        }
    }
}
