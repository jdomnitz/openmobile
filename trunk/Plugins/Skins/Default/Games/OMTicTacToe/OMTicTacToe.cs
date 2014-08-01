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
using System.Linq;
using System.Text;
using OpenMobile;
using OpenMobile.Controls;
using OpenMobile.Framework;
using OpenMobile.Plugin;
using OpenMobile.helperFunctions;
using OpenMobile.Graphics;
using OpenMobile.Data;
using System.Collections.ObjectModel;

namespace OMTicTacToe
{
    public class OMTicTacToe : HighLevelCode
    {
        //ScreenManager manager;
        //IPluginHost theHost;
        int[] boardIDs;
        Dictionary<int, int> screensToBoards;
        imageItem imgB;
        imageItem imgX;
        imageItem imgO;
        imageItem imgWonX;
        imageItem imgWonO;

        public OMTicTacToe()
            : base("OMTicTacToe", imageItem.NONE, 0.1f, "Tic Tac Toe", "Tic Tac Toe", "Peter Yeaney", "peter.yeaney@outlook.com")
        {
        }

        public override imageItem pluginIcon
        {
            get
            {
                return OM.Host.getPluginImage(this, "Icon-OMTicTacToe");
            }
        }

        public override eLoadStatus initialize(IPluginHost host)
        {

            //theHost = host;
            //base.PanelManager = new ScreenManager(this);

            boardIDs = new int[OM.Host.ScreenCount];
            screensToBoards = new Dictionary<int, int>();
            imgB = new imageItem((OImage)OM.Host.getPluginImage(this, "Images|Icon-Glass-OMTicTacToe_B").image.Clone());
            imgX = new imageItem((OImage)OM.Host.getPluginImage(this, "Images|Icon-Glass-OMTicTacToe_X").image.Clone());
            imgO = new imageItem((OImage)OM.Host.getPluginImage(this, "Images|Icon-Glass-OMTicTacToe_O").image.Clone());
            imgWonX = new imageItem((OImage)OM.Host.getPluginImage(this, "Images|Icon-Glass-OMTicTacToe_X").image.Copy().Overlay(BuiltInComponents.SystemSettings.SkinFocusColor));
            imgWonO = new imageItem((OImage)OM.Host.getPluginImage(this, "Images|Icon-Glass-OMTicTacToe_O").image.Copy().Overlay(BuiltInComponents.SystemSettings.SkinFocusColor));

            OMPanel TicTacToe = new OMPanel("OMTicTacToe", "TicTacToe", this.pluginIcon);
            OMButton SinglePlayer = OMButton.PreConfigLayout_CleanStyle("SinglePlayer", (OM.Host.ClientArea_Init.Width / 2) - (int)(OM.Host.ClientArea_Init.Width * .1), (OM.Host.ClientArea_Init.Bottom / 2) - 75, (int)(OM.Host.ClientArea_Init.Width * .2), 80, corners:GraphicCorners.Top);
            SinglePlayer.Text = "Single Player";
            SinglePlayer.OnClick += new userInteraction(SinglePlayer_OnClick);
            TicTacToe.addControl(SinglePlayer);
            OMButton MultiPlayer = OMButton.PreConfigLayout_CleanStyle("MultiPlayer", (OM.Host.ClientArea_Init.Width / 2) - (int)(OM.Host.ClientArea_Init.Width * .1), (OM.Host.ClientArea_Init.Bottom / 2) + 5, (int)(OM.Host.ClientArea_Init.Width * .2), 80, corners: GraphicCorners.Bottom);
            MultiPlayer.Text = "Multiplayer";
            MultiPlayer.OnClick += new userInteraction(MultiPlayer_OnClick);
            TicTacToe.addControl(MultiPlayer);

            OMBasicShape multiplayerListBackground = new OMBasicShape("multiplayerListBackground", (OM.Host.ClientArea_Init.Width / 2) - (int)(OM.Host.ClientArea_Init.Width * .25), 80, (int)(OM.Host.ClientArea_Init.Width * .5), 350, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            multiplayerListBackground.Visible = false;
            TicTacToe.addControl(multiplayerListBackground);
            OMList MultiplayerList = new OMList("multiplayerList", multiplayerListBackground.Left + 7, multiplayerListBackground.Top + 7, multiplayerListBackground.Width - 14, multiplayerListBackground.Height - 14);
            MultiplayerList.ListStyle = eListStyle.MultiListText;
            MultiplayerList.ListItemHeight = 40;
            MultiplayerList.SeparatorColor = Color.Silver;
            MultiplayerList.SeparatorSize = 4;
            MultiplayerList.ItemColor1 = Color.Transparent;
            MultiplayerList.HighlightColor = Color.White;
            MultiplayerList.SelectedItemColor1 = BuiltInComponents.SystemSettings.SkinFocusColor;
            MultiplayerList.TextAlignment = Alignment.CenterCenter;
            MultiplayerList.OnClick += new userInteraction(MultiplayerList_OnClick);
            MultiplayerList.Visible = false;
            TicTacToe.addControl(MultiplayerList);
            OMButton multiplayerCancel = OMButton.PreConfigLayout_CleanStyle("multiplayerCancel", (OM.Host.ClientArea_Init.Width / 2) - (int)(OM.Host.ClientArea_Init.Width * .1), MultiplayerList.Top + MultiplayerList.Height + 5, (int)(OM.Host.ClientArea_Init.Width * .2), 80, corners: GraphicCorners.All);
            multiplayerCancel.Text = "Cancel";
            multiplayerCancel.Visible = false;
            multiplayerCancel.OnClick += new userInteraction(multiplayerCancel_OnClick);
            TicTacToe.addControl(multiplayerCancel);

            OMBasicShape challengeBackgroundMain = new OMBasicShape("challengeBackgroundMain", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + (OM.Host.ClientArea_Init.Height / 2) - 100, OM.Host.ClientArea_Init.Width, 200, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            challengeBackgroundMain.Visible = false;
            TicTacToe.addControl(challengeBackgroundMain);
            OMLabel challengeLabelMain = new OMLabel("challengeLabelMain", challengeBackgroundMain.Left + 5, challengeBackgroundMain.Top + 5, challengeBackgroundMain.Width - 10, challengeBackgroundMain.Height - 90);
            challengeLabelMain.Visible = false;
            TicTacToe.addControl(challengeLabelMain);
            OMButton challengeCancel = OMButton.PreConfigLayout_CleanStyle("challengeCancel", (challengeBackgroundMain.Width / 2) - 50, challengeLabelMain.Top + challengeLabelMain.Height + 5, 100, 75, corners: GraphicCorners.All);
            challengeCancel.Text = "Cancel";
            challengeCancel.Visible = false;
            challengeCancel.OnClick += new userInteraction(challengeCancel_OnClick);
            TicTacToe.addControl(challengeCancel);
            OMButton challengeAccept = OMButton.PreConfigLayout_CleanStyle("challengeAccept", (challengeBackgroundMain.Width / 2) - 110, challengeLabelMain.Top + challengeLabelMain.Height + 5, 100, 75, corners: GraphicCorners.All);
            challengeAccept.Text = "Accept";
            challengeAccept.Visible = false;
            challengeAccept.OnClick += new userInteraction(challengeAccept_OnClick);
            TicTacToe.addControl(challengeAccept);
            OMButton challengeDecline = OMButton.PreConfigLayout_CleanStyle("challengeDecline", challengeAccept.Left + challengeAccept.Width + 20, challengeLabelMain.Top + challengeLabelMain.Height + 5, 100, 75, corners: GraphicCorners.All);
            challengeDecline.Text = "Decline";
            challengeDecline.Visible = false;
            challengeDecline.OnClick += new userInteraction(challengeDecline_OnClick);
            TicTacToe.addControl(challengeDecline);

            Size tileSize = new Size(140, 140);
            OMBasicShape shapeBackgroundMain = new OMBasicShape("shapeBackgroundMain", OM.Host.ClientArea_Init.Left, OM.Host.ClientArea_Init.Top + 5, tileSize.Width * 3 + 20, tileSize.Height * 3 + 20 + 40, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(128, Color.Black), Color.Transparent, 0, 10));
            shapeBackgroundMain.Left = OM.Host.ClientArea_Init.Center.X - shapeBackgroundMain.Region.Center.X;
            shapeBackgroundMain.Visible = false;
            TicTacToe.addControl(shapeBackgroundMain);

            OMButton Rematch = OMButton.PreConfigLayout_CleanStyle("Rematch", 0, OM.Host.ClientArea_Init.Bottom - 110, 200, 100, corners:GraphicCorners.All);
            Rematch.Left = (shapeBackgroundMain.Left / 2) - (Rematch.Width / 2);
            Rematch.Text = "Rematch";
            Rematch.Visible = false;
            Rematch.OnClick += new userInteraction(Rematch_OnClick);
            TicTacToe.addControl(Rematch);

            OMButton Quit = OMButton.PreConfigLayout_CleanStyle("Quit", OM.Host.ClientArea_Init.Right - 230, OM.Host.ClientArea_Init.Bottom - 110, 200, 100, corners: GraphicCorners.All);
            Quit.Visible = false;
            Quit.Text = "Quit";
            Quit.OnClick += new userInteraction(Quit_OnClick);
            TicTacToe.addControl(Quit);

            OMLabel statusLabel = new OMLabel("statusLabel", shapeBackgroundMain.Region.Left + 10, shapeBackgroundMain.Region.Top + 5, shapeBackgroundMain.Region.Width - 20, 40);
            statusLabel.Visible = false;
            statusLabel.Color = Color.FromArgb(178, BuiltInComponents.SystemSettings.SkinTextColor);
            statusLabel.ShapeData = new ShapeData(shapes.RoundedRectangle, Color.FromArgb(25, Color.White), Color.Transparent, 0, 5);
            statusLabel.AutoFitTextMode = FitModes.FitFill;
            TicTacToe.addControl(statusLabel);

            for (int i = 0; i < OM.Host.ScreenCount; i++)
            {
                for (int r = 1; r < 4; r++) //rows
                {
                    for (int c = 1; c < 4; c++) //columns
                    {
                        OMButton boardTile = new OMButton("boardTile_" + r.ToString() + "_" + c.ToString(), shapeBackgroundMain.Region.Left + 10 + (tileSize.Width * (c - 1)), shapeBackgroundMain.Region.Top + 50 + (tileSize.Height * (r - 1)), tileSize.Width, tileSize.Height);
                        boardTile.Visible = false;
                        boardTile.OnClick += new userInteraction(boardTile_OnClick);
                        TicTacToe.addControl(boardTile);
                    }
                }
                MultiplayerList.Add("Screen: " + i.ToString());
            }

            TicTacToe.Entering += new PanelEvent(TicTacToe_Entering);
            TicTacToe.Leaving += new PanelEvent(TicTacToe_Leaving);

            base.PanelManager.loadPanel(TicTacToe, true);
            //for(int i=0;i<OM.Host.ScreenCount;i++)
            //    base.PanelManager.loadSinglePanel(TicTacToe, i, true);
            //base.PanelManager.loadPanel(MultiplayerPanel);

            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Multiplayer.List", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Multiplayer.Challenged", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Multiplayer.Spectate", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Multiplayer.ChallengeAccepted", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Multiplayer.ChallengeDeclined", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Multiplayer.ChallengeCancelled", Subscription_Updated);
            OM.Host.DataHandler.SubscribeToDataSource("OMDSTicTacToe;TicTacToe.Notification.Click", Subscription_Updated);

            return eLoadStatus.LoadSuccessful;
        }

        void challengeCancel_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Multiplayer.ChallengeCancel", new object[] { sender.Tag, ((OMLabel)base.PanelManager[Convert.ToInt32(sender.Tag), "OMTicTacToe"]["challengeLabelMain"]).Tag });
        }

        void challengeAccept_OnClick(OMControl sender, int screen)
        {
            object gameBoardID = OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.AddGame", new object[] { screen.ToString(), ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Tag.ToString() });
            if (gameBoardID != null)
            {
                boardIDs[screen] = Convert.ToInt32(gameBoardID);
                boardIDs[Convert.ToInt32(((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Tag)] = Convert.ToInt32(gameBoardID);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardUpdated", gameBoardID.ToString()), Subscription_Updated);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardVisibility", gameBoardID.ToString()), Subscription_Updated);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardMessages", gameBoardID.ToString()), Subscription_Updated);
                OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.StartGame", new object[] { gameBoardID.ToString() });
            }
        }

        void challengeDecline_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Multiplayer.ChallengeDecline", new object[] { sender.Tag, ((OMLabel)base.PanelManager[Convert.ToInt32(sender.Tag), "OMTicTacToe"]["challengeLabelMain"]).Tag });
        }

        private void Subscription_Updated(DataSource sensor)
        {
            if (sensor.Value == null)
                return;
            for (int screen = 0; screen < OM.Host.ScreenCount; screen++)
            {
                if ((sensor.NameLevel2 == "Multiplayer") && (sensor.NameLevel3 == "List"))
                {
                    //update the multiplayer list
                    UpdateMultiplayerList(screen, (ObservableCollection<string>)sensor.Value);
                    //UpdateMultiplayerList(screen, (List<string>)sensor.Value);
                }
                else if ((sensor.NameLevel2 == "Multiplayer") && (sensor.NameLevel3 == "Challenged") && (((List<int>)sensor.Value)[1] == screen))
                {
                    int screenChallenger = ((List<int>)sensor.Value)[0];
                    //show the challenged panel on SCREEN --> background, label, 2 buttons (accept / decline), ((List<int>)sensor.Value)[0] = screen challenged FROM
                    //AllMenuVisibility(false, screenChallenger);
                    //AllMenuVisibility(false, screen);
                    ((OMLabel)base.PanelManager[screenChallenger, "OMTicTacToe"]["challengeLabelMain"]).Text = String.Format("You have challenged Screen: {0}. Awaiting their response...", screen.ToString());
                    ((OMLabel)base.PanelManager[screenChallenger, "OMTicTacToe"]["challengeLabelMain"]).Tag = screen.ToString();
                    ShowChallengeMain(true, screenChallenger, true);
                    ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Text = String.Format("You have been challenged by Screen: {0}. Please choose an action...", screenChallenger.ToString());
                    ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Tag = screenChallenger.ToString();
                    ShowChallengeMain(true, screen, false);
                    //challengeCancel, challengeAccept, challengeDecline



                }
                else if ((sensor.NameLevel2 == "Multiplayer") && (sensor.NameLevel3 == "ChallengeAccepted"))
                {
                    //start game --- this happens in the accept button click event for now

                }
                else if ((sensor.NameLevel2 == "Multiplayer") && (sensor.NameLevel3 == "ChallengeDeclined"))
                {
                    //get rid of the challenge controls
                    ToggleChallengeControlsOff(((List<int>)sensor.Value)[0]);
                    ToggleChallengeControlsOff(((List<int>)sensor.Value)[1]);
                }
                else if ((sensor.NameLevel2 == "Multiplayer") && (sensor.NameLevel3 == "ChallengeCancelled"))
                {
                    //
                    ToggleChallengeControlsOff(((List<int>)sensor.Value)[0]);
                    ToggleChallengeControlsOff(((List<int>)sensor.Value)[1]);
                }
                else if ((sensor.NameLevel2 == "Multiplayer") && (sensor.NameLevel3 == "Spectate") && (((List<int>)sensor.Value)[0] == screen))
                {
                    boardIDs[screen] = ((List<int>)sensor.Value)[1];
                }
                else if (sensor.NameLevel2 == "Notification" && sensor.NameLevel3 == "Click")
                {
                    if (screen == Convert.ToInt32(sensor.Value))
                        base.GotoPanel(screen, "OMTicTacToe");
                }
                else //if (boardIDs[screen] == Convert.ToInt32(sensor.NameLevel2))
                {
                    int gameBoardID;
                    if ((int.TryParse(sensor.NameLevel2, out gameBoardID)) && (gameBoardID > 0) && (boardIDs[screen] == gameBoardID))
                    {
                        //this screen is subscribed to this board
                        if (sensor.NameLevel3 == "BoardUpdated")
                        {
                            UpdateBoard(boardIDs[screen], screen, (string[][])sensor.Value);
                            //OpenMobile.Threading.SafeThread.Asynchronous(delegate() { UpdateBoard(boardIDs[screen], screen, (string[][])sensor.Value); });
                        }
                        else if (sensor.NameLevel3 == "BoardVisibility")
                        {
                            if ((bool)sensor.Value)
                            {
                                ChangeBoardVisibility(true, screen);
                            }
                            else
                            {
                                ChangeBoardVisibility(false, screen);
                            }
                        }
                        else if (sensor.NameLevel3 == "BoardMessages")
                        {
                            //ChangeBoardMessage(((Dictionary<int, string>)sensor.Value)[((Dictionary<int, string>)sensor.Value).Keys.ElementAt(0)], screen);
                            try
                            {
                                ChangeBoardMessage(((Dictionary<int, string>)sensor.Value)[screen], screen);
                            }
                            catch (Exception ex)
                            {
                                //ChangeBoardMessage("Spectating...", screen);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateMultiplayerList(int screen, ObservableCollection<string> multiplayerList)
        //private void UpdateMultiplayerList(int screen, List<string> multiplayerList)
        {
            for (int i = 0; i < multiplayerList.Count; i++)
            {
                ((OMList)base.PanelManager[screen, "OMTicTacToe"]["multiplayerList"])[i].text = multiplayerList[i];
            }
        }

        private void ChangeBoardMessage(string message, int screen)
        {
            if (message == "BackToGame")
            {
                base.GotoPanel(screen, "OMTicTacToe");
                //base.ShowPanel(screen, "OMTicTacToe");
            }
            else
            {
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Text = message;
                //OM.Host.DebugMsg(String.Format("Message - Screen: {0}, Visible: {1}, Text: {2}", screen.ToString(), ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Visible, ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Text));
                if ((message.ToLower().Contains("won")) || (message.ToLower().Contains("lost")) || (message.ToLower().Contains("draw")))
                    ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Rematch"]).Visible = true;
            }
        }

        private void ToggleChallengeControlsOff(int screen)
        {
            ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["SinglePlayer"]).Disabled = false;
            ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["MultiPlayer"]).Disabled = false;
            ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeCancel"]).Visible = false;
            ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeAccept"]).Visible = false;
            ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeDecline"]).Visible = false;
            ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["challengeBackgroundMain"]).Visible = false;
            ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Visible = false;
        }

        private void ShowChallengeMain(bool Visible, int screen, bool Challenger)
        {
            if (Visible)
            {
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["challengeBackgroundMain"]).Visible = true;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Visible = true;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["SinglePlayer"]).Disabled = true;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["MultiPlayer"]).Disabled = true;
            }
            else
            {
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["challengeBackgroundMain"]).Visible = false;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Visible = false;
            }
            if (Challenger)
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeCancel"]).Visible = true;
            else
            {
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeAccept"]).Visible = true;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeDecline"]).Visible = true;
            }
        }

        private void AllMenuVisibility(bool Visible, int screen)
        {
            MainMenuButtonsVisibility(Visible, screen);
            if (Visible)
            {
                ((OMList)base.PanelManager[screen, "OMTicTacToe"]["multiplayerList"]).Visible = true;
            }
            else
            {
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["multiplayerListBackground"]).Visible = false;
                ((OMList)base.PanelManager[screen, "OMTicTacToe"]["multiplayerList"]).Visible = false;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["multiplayerCancel"]).Visible = false;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeCancel"]).Visible = false;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeAccept"]).Visible = false;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeDecline"]).Visible = false;
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["challengeBackgroundMain"]).Visible = false;
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["challengeLabelMain"]).Visible = false;
            }
        }

        private void MainMenuButtonsVisibility(bool Visible, int screen)
        {
            if (Visible)
            {
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["SinglePlayer"]).Visible = true;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["MultiPlayer"]).Visible = true;
            }
            else
            {
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["SinglePlayer"]).Visible = false;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["MultiPlayer"]).Visible = false;
            }
        }

        private void ChangeBoardVisibility(bool Visible, int screen)
        {
            if (Visible)
            {
                AllMenuVisibility(false, screen);
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["shapeBackgroundMain"]).Visible = true;
                //OM.Host.DebugMsg(String.Format("Setting - Screen: {0}, shapeBackgroundMain.Visible: {1}", screen.ToString(), ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["shapeBackgroundMain"]).Visible.ToString()));
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Visible = true;
                //OM.Host.DebugMsg(String.Format("Setting - Screen: {0}, statusLabel.Visible: {1}", screen.ToString(), ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Visible.ToString()));
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Quit"]).Visible = true;
                //OM.Host.DebugMsg(String.Format("Setting - Screen: {0}, Quit.Visible: {1}", screen.ToString(), ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Quit"]).Visible.ToString()));
            }
            else
            {
                MainMenuButtonsVisibility(true, screen);
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["shapeBackgroundMain"]).Visible = false;
                //OM.Host.DebugMsg(String.Format("Setting - Screen: {0}, shapeBackgroundMain.Visible: {1}", screen.ToString(), ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["shapeBackgroundMain"]).Visible.ToString()));
                ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Visible = false;
                //OM.Host.DebugMsg(String.Format("Setting - Screen: {0}, statusLabel.Visible: {1}", screen.ToString(), ((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Visible.ToString()));
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Quit"]).Visible = false;
                //OM.Host.DebugMsg(String.Format("Setting - Screen: {0}, Quit.Visible: {1}", screen.ToString(), ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Quit"]).Visible.ToString()));
            }
            ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Rematch"]).Visible = false;
            for (int r = 1; r < 4; r++)
            {
                for (int c = 1; c < 4; c++)
                {
                    if (Visible)
                    {
                        //base.PanelManager[screen, "OMTicTacToe"].MoveControlToFront((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]);
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Visible = true;
                        //OM.Host.DebugMsg(String.Format("boardTile_{0}_{1}.Visible: true", r.ToString(), c.ToString()));
                    }
                    else
                    {
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Visible = false;
                        //OM.Host.DebugMsg(String.Format("boardTile_{0}_{1}.Visible: false", r.ToString(), c.ToString()));
                    }
                }
            }
        }

        private void UpdateBoard(int gameBoardID, int screen, string[][] Layout)
        {
            ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["Rematch"]).Visible = false;
            for (int r = 1; r < 4; r++)
            {
                for (int c = 1; c < 4; c++)
                {
                    //OM.Host.DebugMsg(String.Format("Updating - Screen: {0}, gameBoardID: {1}, Layout[{2}][{3}]: {4}", screen.ToString(), gameBoardID.ToString(), (r - 1).ToString(), (c - 1).ToString(), Layout[r - 1][c - 1]));
                    ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Image = imageItem.NONE;
                    if (Layout[r - 1][c - 1] == "B")
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Image = imgB;
                    else if (Layout[r - 1][c - 1] == "X")
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Image = imgX;
                    else if (Layout[r - 1][c - 1] == "O")
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Image = imgO;
                    else if (Layout[r - 1][c - 1] == "WX")
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Image = imgWonX;
                    else if (Layout[r - 1][c - 1] == "WO")
                        ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Image = imgWonO;
                }
            }
        }

        private void TicTacToe_Entering(OMPanel sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("OMDSTicTacToe.Notifications.Disable", new object[] { screen });
        }

        private void TicTacToe_Leaving(OMPanel sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("OMDSTicTacToe.Notifications.Enable", new object[] { screen });
        }

        private void Rematch_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.Rematch", new object[] { screen.ToString() });
        }

        private void Quit_OnClick(OMControl sender, int screen)
        {
            //just remove the subscriptions once
            OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.EndGame", new object[] { screen.ToString() });
            OM.Host.DataHandler.UnsubscribeFromDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardUpdated", boardIDs[screen].ToString()), Subscription_Updated);
            OM.Host.DataHandler.UnsubscribeFromDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardVisibility", boardIDs[screen].ToString()), Subscription_Updated);
            OM.Host.DataHandler.UnsubscribeFromDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardMessages", boardIDs[screen].ToString()), Subscription_Updated);
        }

        private void SinglePlayer_OnClick(OMControl sender, int screen)
        {
            object gameBoardID = OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.AddGame", new object[] { screen.ToString(), "-1" });
            if (gameBoardID != null)
            {
                boardIDs[screen] = Convert.ToInt32(gameBoardID);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardUpdated", gameBoardID.ToString()), Subscription_Updated);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardVisibility", gameBoardID.ToString()), Subscription_Updated);
                OM.Host.DataHandler.SubscribeToDataSource(String.Format("OMDSTicTacToe;TicTacToe.{0}.BoardMessages", gameBoardID.ToString()), Subscription_Updated);
                OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.StartGame", new object[] { gameBoardID.ToString() });
            }
        }

        private void MultiPlayerControlsShown(bool Visible, int screen)
        {
            if (Visible)
            {
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["multiplayerListBackground"]).Visible = true;
                ((OMList)base.PanelManager[screen, "OMTicTacToe"]["multiplayerList"]).Visible = true;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["multiplayerCancel"]).Visible = true;
            }
            else
            {
                ((OMBasicShape)base.PanelManager[screen, "OMTicTacToe"]["multiplayerListBackground"]).Visible = false;
                ((OMList)base.PanelManager[screen, "OMTicTacToe"]["multiplayerList"]).Visible = false;
                ((OMButton)base.PanelManager[screen, "OMTicTacToe"]["multiplayerCancel"]).Visible = false;
            }
        }

        private void MultiPlayer_OnClick(OMControl sender, int screen)
        {
            MainMenuButtonsVisibility(false, screen);
            MultiPlayerControlsShown(true, screen);
        }

        private void multiplayerCancel_OnClick(OMControl sender, int screen)
        {
            MultiPlayerControlsShown(false, screen);
            MainMenuButtonsVisibility(true, screen);
        }

        private void MultiplayerList_OnClick(OMControl sender, int screen)
        {
            OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Multiplayer.Challenge", new object[] { screen, ((OMList)sender).SelectedIndex });
        }

        private void boardTile_OnClick(OMControl sender, int screen)
        {
            if (((OMLabel)base.PanelManager[screen, "OMTicTacToe"]["statusLabel"]).Text == "Watching game...") //not playing in this game...
                return;
            string row = (Convert.ToInt32(((OMButton)sender).Name.Remove(0, 10).Substring(0, 1)) - 1).ToString();
            string col = (Convert.ToInt32(((OMButton)sender).Name.Remove(0, 12)) - 1).ToString();
            OM.Host.CommandHandler.ExecuteCommand("TicTacToe.Game.FlipTile", new object[] { screen.ToString(), row, col });
        }

    }
}