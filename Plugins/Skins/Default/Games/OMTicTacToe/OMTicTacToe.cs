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

namespace OMTicTacToe
{
    public class Class1 : IHighLevel
    {

        ScreenManager manager;
        IPluginHost theHost;
        int[] boardIDs;
        Dictionary<int, int> screensToBoards;

        public eLoadStatus initialize(IPluginHost host)
        {

            theHost = host;
            manager = new ScreenManager(this);

            boardIDs = new int[theHost.ScreenCount];
            screensToBoards = new Dictionary<int, int>();

            //Dim text1btn = OpenMobile.helperFunctions.Controls.DefaultControls.GetButton("_Text1Btn", x + 420, y + 350, 150, 80, "", "OK")
            //OMButton location_button = OMButton.PreConfigLayout_BasicStyle("locationbutton", 351, 41, 298, 48, GraphicCorners.All);
            OMPanel TicTacToe = new OMPanel("OMTicTacToe");
            //OMButton SinglePlayer = new OMButton("SinglePlayer", (theHost.ClientArea[0].Width / 2) - 100, (theHost.ClientArea[0].Height / 2) - 75, 200, 80);
            OMButton SinglePlayer = OMButton.PreConfigLayout_BasicStyle("SinglePlayer", (theHost.ClientArea[0].Width / 2) - 100, (theHost.ClientArea[0].Height / 2) - 75, 200, 80, GraphicCorners.All);
            SinglePlayer.Text = "Single Player";
            //SinglePlayer.BorderColor = Color.White;
            //SinglePlayer.BorderSize = 1;
            SinglePlayer.OnClick += new userInteraction(SinglePlayer_OnClick);
            //OMButton MultiPlayer = new OMButton("MultiPlayer", (theHost.ClientArea[0].Width / 2) - 100, (theHost.ClientArea[0].Height / 2) + 5, 200, 80);
            OMButton MultiPlayer = OMButton.PreConfigLayout_BasicStyle("MultiPlayer", (theHost.ClientArea[0].Width / 2) - 100, (theHost.ClientArea[0].Height / 2) + 5, 200, 80, GraphicCorners.All);
            MultiPlayer.Text = "Multiplayer";
            //MultiPlayer.BorderColor = Color.White;
            //MultiPlayer.BorderSize = 1;
            MultiPlayer.OnClick += new userInteraction(MultiPlayer_OnClick);
            //OMButton MainMenu = new OMButton("MainMenu", ((theHost.ClientArea[0].Width / 2) / 2) - 100, 400, 200, 100);
            OMButton MainMenu = OMButton.PreConfigLayout_BasicStyle("MainMenu", ((theHost.ClientArea[0].Width / 2) / 2) - 100, 400, 200, 100, GraphicCorners.All);
            MainMenu.Text = "Return To Main Menu";
            MainMenu.Visible = false;
            //MainMenu.Visible_DataSource = "TicTacToe.Game.VisibleButtons";
            MainMenu.OnClick += new userInteraction(MainMenu_OnClick);
            //OMButton Rematch = new OMButton("Rematch", (theHost.ClientArea[0].Width / 2) - 100, 400, 200, 100);
            //OMButton Rematch = OMButton.PreConfigLayout_BasicStyle("Rematch", (theHost.ClientArea[0].Width / 2) - 100, 400, 200, 100, GraphicCorners.All);
            //Rematch.Text = "Rematch";
            //Rematch.Visible = false;
            //Rematch.Visible_DataSource = "TicTacToe.Game.VisibleButtons";
            //Rematch.OnClick += new userInteraction(Rematch_OnClick);
            TicTacToe.addControl(SinglePlayer);
            TicTacToe.addControl(MultiPlayer);
            TicTacToe.addControl(MainMenu);
            //TicTacToe.addControl(Rematch);
            //TicTacToe.Entering += new PanelEvent(TicTacToe_Entering);

            OMPanel MultiplayerPanel = new OMPanel("MultiplayerPanel");
            OMList MultiplayerList = new OMList("MultiplayerList", 100, 80, 390, 440);
            MultiplayerList.OnClick += new userInteraction(MultiplayerList_OnClick);
            OMList MultiplayerListPlaying = new OMList("MultiplayerListPlaying", 610, 80, 380, 440);
            MultiplayerPanel.addControl(MultiplayerList);
            MultiplayerPanel.addControl(MultiplayerListPlaying);

            OMPanel GamePanel = new OMPanel("GamePanel");
            //OMLabel statusLabel = new OMLabel("statusLabel", 0, 35, 1000, 65);
            //statusLabel.Visible = false;
            //OMButton Quit = new OMButton("Quit", (theHost.ClientArea[0].Width / 2) + ((theHost.ClientArea[0].Width / 2) / 2) - 100, 400, 200, 100);
            //OMButton Quit = OMButton.PreConfigLayout_BasicStyle("Quit", (theHost.ClientArea[0].Width / 2) + ((theHost.ClientArea[0].Width / 2) / 2) - 100, 400, 200, 100, GraphicCorners.All);
            //Quit.Visible = false;
            //Quit.Text = "Quit";
            //Quit.Visible_DataSource = "TicTacToe.Game.VisibleButtons";
            //Quit.OnClick += new userInteraction(Quit_OnClick);
            //GamePanel.addControl(statusLabel);
            //GamePanel.addControl(Quit);
            //TicTacToe.addControl(statusLabel);
            //TicTacToe.addControl(Quit);

           
            //shpBackground.Visible = false;
            //TicTacToe.addControl(shpBackground);
            
            //shpBackground1.Visible = false;
            //TicTacToe.addControl(shpBackground1);
            
            //shpBackground2.Visible = false;
            //TicTacToe.addControl(shpBackground2);
            
            //shpBackground3.Visible = false;
            //TicTacToe.addControl(shpBackground3);
            
            //shpBackground4.Visible = false;
            //TicTacToe.addControl(shpBackground4);

            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                for (int r = 1; r < 4; r++) //rows
                {
                    for (int c = 1; c < 4; c++) //columns
                    {
                        OMButton boardTile = new OMButton("boardTile_" + r.ToString() + "_" + c.ToString(), 50 + (150 * c), 100 * r, 150, 100);
                        boardTile.Visible = false;
                        //boardTile.Image = theHost.getSkinImage("TicTacToe_B");
                        //boardTile.Image = theHost.getPluginImage(this, "Images|TicTacToe_B");
                        boardTile.OnClick += new userInteraction(boardTile_OnClick);
                        //GamePanel.addControl(boardTile);
                        //TicTacToe.addControl(boardTile);
                        
                    }
                }
                MultiplayerList.Add("Screen: " + i.ToString());
            }

            manager.loadPanel(TicTacToe, true);
            manager.loadPanel(MultiplayerPanel);
            //manager.loadPanel(GamePanel);

            return eLoadStatus.LoadSuccessful;
        }

        private void MainMenu_OnClick(OMControl sender, int screen)
        {

        }

        private void Rematch_OnClick(OMControl sender, int screen)
        {
            object value;
            theHost.DataHandler.GetDataSourceValue("TicTacToe.Game.Rematch", new object[] { screen.ToString() }, out value);
        }

        private void Quit_OnClick(OMControl sender, int screen)
        {
            object value;
            theHost.DataHandler.GetDataSourceValue("TicTacToe.Game.EndGame", new object[] { screen.ToString() }, out value);
        }

        private void SinglePlayer_OnClick(OMControl sender, int screen)
        {
            object value;
            theHost.DataHandler.GetDataSourceValue("TicTacToe.Game.AddGame", new object[] { screen.ToString(), "-1" }, out value);
            if (value != null)
            {
                OMBasicShape shpBackground = new OMBasicShape("shpBackground", 190, 90, 470, 320, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(175, Color.Black), Color.Transparent, 0, 5));
                shpBackground.Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                //((OMBasicShape)manager[screen, "OMTicTacToe"]["shpBackground"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                OMBasicShape shpBackground1 = new OMBasicShape("shpBackground1", 190, 198, 470, 4, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(125, Color.Black), Color.Transparent, 0, 5));
                shpBackground1.Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                //((OMBasicShape)manager[screen, "OMTicTacToe"]["shpBackground1"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                OMBasicShape shpBackground2 = new OMBasicShape("shpBackground2", 190, 298, 470, 4, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(125, Color.Black), Color.Transparent, 0, 5));
                shpBackground2.Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                //((OMBasicShape)manager[screen, "OMTicTacToe"]["shpBackground2"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                OMBasicShape shpBackground3 = new OMBasicShape("shpBackground3", 348, 90, 4, 320, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(125, Color.Black), Color.Transparent, 0, 5));
                shpBackground3.Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                //((OMBasicShape)manager[screen, "OMTicTacToe"]["shpBackground3"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                OMBasicShape shpBackground4 = new OMBasicShape("shpBackground4", 498, 90, 4, 320, new ShapeData(shapes.RoundedRectangle, Color.FromArgb(125, Color.Black), Color.Transparent, 0, 5));
                shpBackground4.Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                //((OMBasicShape)manager[screen, "OMTicTacToe"]["shpBackground4"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                OMPanel p = manager[screen, "OMTicTacToe"];
                p.addControl(shpBackground);
                p.addControl(shpBackground1);
                p.addControl(shpBackground2);
                p.addControl(shpBackground3);
                p.addControl(shpBackground4);
                //p.MoveControlToBack(shpBackground1);
                //p.MoveControlToBack(shpBackground2);
                //p.MoveControlToBack(shpBackground3);
                //p.MoveControlToBack(shpBackground4);
                //p.MoveControlToBack(shpBackground);

                //set the button datasources
                for (int r = 1; r < 4; r++)
                {
                    for (int c = 1; c < 4; c++)
                    {
                        OMButton boardTile = new OMButton("boardTile_" + r.ToString() + "_" + c.ToString(), 50 + (150 * c), 100 * r, 150, 100);
                        boardTile.OnClick += new userInteraction(boardTile_OnClick);
                        boardTile.Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + "." + r.ToString() + "-" + c.ToString() + "-Visible";
                        boardTile.DataSource_Image = "TicTacToe." + Convert.ToInt32(value).ToString() + "." + r.ToString() + "-" + c.ToString();
                        p.addControl(boardTile);
                        //((OMButton)manager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + "." + r.ToString() + "-" + c.ToString() + "-Visible";
                        //((OMButton)manager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]).DataSource_Image = "TicTacToe." + Convert.ToInt32(value).ToString() + "." + r.ToString() + "-" + c.ToString();
                        //p.MoveControlToFront((OMButton)manager[screen, "OMTicTacToe"]["boardTile_" + r.ToString() + "_" + c.ToString()]);
                    }
                }
                OMLabel statusLabel = new OMLabel("statusLabel", 0, 35, 1000, 65);
                p.addControl(statusLabel);
                ((OMLabel)manager[screen, "OMTicTacToe"]["statusLabel"]).DataSource = String.Format("TicTacToe." + Convert.ToInt32(value).ToString() + ".{0}", screen.ToString());
                ((OMLabel)manager[screen, "OMTicTacToe"]["statusLabel"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleStatusLabel";
                OMButton Rematch = OMButton.PreConfigLayout_BasicStyle("Rematch", (theHost.ClientArea[0].Width / 2) - 100, 400, 200, 100, GraphicCorners.All);
                Rematch.Text = "Rematch";
                p.addControl(Rematch);
                Rematch.OnClick += new userInteraction(Rematch_OnClick);
                p.addControl(Rematch);
                OMButton Quit = OMButton.PreConfigLayout_BasicStyle("Quit", (theHost.ClientArea[0].Width / 2) + ((theHost.ClientArea[0].Width / 2) / 2) - 100, 400, 200, 100, GraphicCorners.All);
                Quit.Text = "Quit";
                Quit.OnClick += new userInteraction(Quit_OnClick);
                p.addControl(Quit);
                //((OMButton)manager[screen, "OMTicTacToe"]["MainMenu"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleMainMenuButton";
                ((OMButton)manager[screen, "OMTicTacToe"]["Rematch"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleRematchButton";
                ((OMButton)manager[screen, "OMTicTacToe"]["Quit"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleQuitButton";
                ((OMButton)manager[screen, "OMTicTacToe"]["SinglePlayer"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleSinglePlayerButton";
                ((OMButton)manager[screen, "OMTicTacToe"]["MultiPlayer"]).Visible_DataSource = "TicTacToe." + Convert.ToInt32(value).ToString() + ".VisibleMultiPlayerButton";
                theHost.DataHandler.GetDataSourceValue("TicTacToe.Game.StartGame", new object[] { Convert.ToInt32(value).ToString() }, out value);
            }
        }

        private void MultiPlayer_OnClick(OMControl sender, int screen)
        {
            //theHost.execute(eFunction.TransitionFromAny, screen.ToString());
            //theHost.execute(eFunction.TransitionToPanel, screen.ToString(), "OMTicTacToe", "MultiplayerPanel");
            //theHost.execute(eFunction.ExecuteTransition, screen.ToString());
        }

        private void MultiplayerList_OnClick(OMControl sender, int screen)
        {

        }

        private void boardTile_OnClick(OMControl sender, int screen)
        {
            if (((OMLabel)manager[screen, "OMTicTacToe"]["statusLabel"]).Text == "Watching game...") //not playing in this game...
                return;
            string row = (Convert.ToInt32(((OMButton)sender).Name.Remove(0, 10).Substring(0, 1)) - 1).ToString();
            string col = (Convert.ToInt32(((OMButton)sender).Name.Remove(0, 12)) - 1).ToString();
            object value;
            theHost.DataHandler.GetDataSourceValue("TicTacToe.Game.FlipTile", new object[] { screen.ToString(), row, col }, out value);
        }

        public OMPanel loadPanel(string name, int screen)
        {
            return manager[screen, name];
        }
        public Settings loadSettings()
        {
            return null;
        }
        public string authorName
        {
            get { return "Peter Yeaney"; }
        }
        public string authorEmail
        {
            get { return "peter.yeaney@outlook.com"; }
        }
        public string pluginName
        {
            get { return "OMTicTacToe"; }
        }
        public string displayName
        {
            get { return "TicTacToe"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }
        public string pluginDescription
        {
            get { return "Tic Tac Toe Game"; }
        }
        public bool incomingMessage(string message, string source)
        {
            return false;
        }
        public bool incomingMessage<T>(string message, string source, ref T data)
        {
            return false;
        }
        public imageItem pluginIcon
        {
            get { return OM.Host.getSkinImage("Icons|Icon-TicTacToe"); }
        }
        public void Dispose()
        {
            //
        }
    }
}