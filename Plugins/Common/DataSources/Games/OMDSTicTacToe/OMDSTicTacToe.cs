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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.helperFunctions;
using OpenMobile;
using OpenMobile.Data;
using OpenMobile.Graphics;
using OpenMobile.Controls;

namespace OMDSTicTacToe
{
    public class OMDSTicTacToe : BasePluginCode, IDataSource
    {
        static IPluginHost theHost;
        int boardID;
        Random random;
        static Dictionary<int, TicTacToeBoard> gameBoards;
        ObservableCollection<string> multiplayerList = new ObservableCollection<string>();
        //List<string> multiplayerList;
        public static bool[] displayNotifications;
        public static Notification[] gameNotifications;

        public OMDSTicTacToe()
            : base("OMDSTicTacToe", OM.Host.getPluginImage<OMDSTicTacToe>("Icon-OMTicTacToe"), 0.1f, "Tic Tac Toe Datasource", "Peter Yeaney", "peter.yeaney@outlook.com")
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
            theHost = host;
            random = new Random();
            gameBoards = new Dictionary<int, TicTacToeBoard>();
            //multiplayerList = new List<string>();
            multiplayerList = new ObservableCollection<string>();
            displayNotifications = new bool[theHost.ScreenCount];
            gameNotifications = new Notification[OM.Host.ScreenCount];
            for (int i = 0; i < theHost.ScreenCount; i++)
            {
                multiplayerList.Add("Screen: " + i.ToString());
                displayNotifications[i] = true;
            }

            //get boardID
            if (StoredData.Get(this, "OMDSTicTacToe.BoardID") == "")
                boardID = 0;
            else
                boardID = Convert.ToInt32(StoredData.Get(this, "OMDSTicTacToe.BoardID"));

            //create the commands for games
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "OMDSTicTacToe", "Notifications", "Enable", NotificationsEnable, 0, false, "Enable OMDSTicTacToe Notifications"));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "OMDSTicTacToe", "Notifications", "Disable", NotificationsDisable, 0, false, "Disable OMDSTicTacToe Notifications"));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Game", "AddGame", AddGame, 2, true, ""));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Game", "StartGame", StartGame, 1, false, ""));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Game", "FlipTile", FlipTile, 1, false, ""));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Game", "EndGame", EndGame, 1, false, ""));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Game", "Rematch", Rematch, 1, false, ""));

            //create the datasource for the multiplayerList and push the initial values
            theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Multiplayer", "List", DataSource.DataTypes.raw, "Provides a subscribable DataSource for the multiplayerList: Value = ObservableCollection<string>"), multiplayerList);
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Multiplayer", "Challenge", ChallengeScreen, 2, false, "params = {screenFrom, screenChallenged}"));
            theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Multiplayer", "Challenged", DataSource.DataTypes.raw, "Provides a subscribable DataSource for challenges: Value = List<int> {screenChallanger, screenChallanged}"));
            theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Multiplayer", "Spectate", DataSource.DataTypes.raw, "Provides a subscribable DataSource for spectating = List<int> {SpectatingScreen, ScreenToSpectate}"));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Multiplayer", "ChallengeDecline", ChallengeDeclined, 2, false, "params = {screenFrom, screenChallenger}"));
            theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Multiplayer", "ChallengeDeclined", DataSource.DataTypes.raw, "Provides a subscribable DataSource for when a game challenge is declined"));
            theHost.CommandHandler.AddCommand(new Command(this.pluginName, "TicTacToe", "Multiplayer", "ChallengeCancel", ChallengeCancelled, 2, false, "params = {screenFrom, screenChallenger}"));
            theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Multiplayer", "ChallengeCancelled", DataSource.DataTypes.raw, "Provides a subscribable DataSource for when a game challenge is canceled"));
            theHost.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Notification", "Click", DataSource.DataTypes.raw, "Provides a subscribable DataSource for when a Tic Tac Toe notification is clicked"));

            return eLoadStatus.LoadSuccessful;
        }

        public static void ShowNotification(IBasePlugin ibp, int screen, string message)
        {
            if (OMDSTicTacToe.gameNotifications[screen] == null)
            {
                //gameNotifications[screen] = new Notification(ibp, "OMDSTicTacToe", imageItem.NONE, imageItem.NONE, line1, line2);
                gameNotifications[screen] = new Notification(ibp, "OMDSTicTacToe", ibp.pluginIcon.image, ibp.pluginIcon.image, "Tic Tac Toe", message);
                gameNotifications[screen].ClickAction += new Notification.NotificationAction(TicTacToeBoard_ClickAction);
                gameNotifications[screen].ClearAction += new Notification.NotificationAction(TicTacToeBoard_ClearAction);
                OM.Host.UIHandler.AddNotification(screen, gameNotifications[screen]);
            }
            else
            {
                OMDSTicTacToe.gameNotifications[screen].Text = message;
            }
        }

        private static void TicTacToeBoard_ClickAction(Notification notification, int screen, ref bool cancel)
        {

            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Notification.Click", screen, true);

            //for (int i = 0; i < gameBoards.Count; i++)
            //{
            //    if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == screen) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == screen))
            //    {
            //        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + gameBoards[gameBoards.Keys.ElementAt(i)].ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { screen, "BackToGame" } }, true);
            //        break;
            //    }
            //}
            
            //send the message to go back to the game
            //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { screen, "BackToGame" } }, true);
        }

        private static void TicTacToeBoard_ClearAction(Notification notification, int screen, ref bool cancel)
        {
            cancel = true;
        }
        

        private object ChallengeCancelled(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            ModifyMultiplayerListBack(Convert.ToInt32(param[0]), Convert.ToInt32(param[1]));
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.ChallengeDeclined", new List<int> { Convert.ToInt32(param[0]), Convert.ToInt32(param[1]) }, true);
            return null;
        }

        private object ChallengeDeclined(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            ModifyMultiplayerListBack(Convert.ToInt32(param[0]), Convert.ToInt32(param[1]));
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.ChallengeDeclined", new List<int> { Convert.ToInt32(param[0]), Convert.ToInt32(param[1]) }, true);
            return null;
        }

        private void ModifyMultiplayerListBack(int screen1, int screen2)
        {
            multiplayerList[screen1] = String.Format("Screen: {0}", screen1.ToString());
            multiplayerList[screen2] = String.Format("Screen: {0}", screen2.ToString());
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.List", multiplayerList, true);
        }

        private object ChallengeScreen(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            challengeScreen(Convert.ToInt32(param[0]), Convert.ToInt32(param[1]));
            return null;
        }

        private void challengeScreen(int screen, int challengedScreen)
        {
            if (multiplayerList[challengedScreen].Contains("Challeng"))
                return;
            if (challengedScreen != -1)
            {
                if (multiplayerList[challengedScreen].Contains("***"))
                {
                    //already playing the game, sub_update for the spectator screen (param[0]) the board id of that game
                    for (int i = 0; i < gameBoards.Count; i++)
                    {
                        if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == challengedScreen) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == challengedScreen))
                        {
                            multiplayerList[screen] = "*** Screen: " + screen.ToString() + " (Spectating Screen: " + challengedScreen.ToString() + ")";
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.Spectate", new List<int> {screen, gameBoards[gameBoards.Keys.ElementAt(i)].ID }, true);
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + gameBoards[gameBoards.Keys.ElementAt(i)].ID.ToString() + ".BoardVisibility", true, true);
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + gameBoards[gameBoards.Keys.ElementAt(i)].ID.ToString() + ".BoardUpdated", gameBoards[gameBoards.Keys.ElementAt(i)].Layout, true);
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + gameBoards[gameBoards.Keys.ElementAt(i)].ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { screen, "Spectating..." } }, true);
                            break;
                        }
                    }
                    return;
                }
            }
            if (screen == challengedScreen)
            {
                OM.Host.UIHandler.InfoBanner_Show(screen, new InfoBanner("Cannot challenge yourself!"));
                return;
            }
            multiplayerList[screen] = String.Format("Screen: {0} (Challenging Screen: {1})", screen.ToString(), challengedScreen.ToString());
            multiplayerList[challengedScreen] = String.Format("Screen: {0} (Challenged By Screen: {1})", challengedScreen.ToString(), screen.ToString());
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.List", multiplayerList, true);
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.Challenged", new List<int> {screen, challengedScreen}, true);
            if (displayNotifications[challengedScreen])
            {
                ShowNotification(this, challengedScreen, String.Format("Screen {0} has challenged you to a game of Tic Tac Toe", screen.ToString()));
            }
        }

        private object NotificationsEnable(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            displayNotifications[Convert.ToInt32(param[0])] = true;
            return null;
        }

        private object NotificationsDisable(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            displayNotifications[Convert.ToInt32(param[0])] = false;
            gameNotifications[Convert.ToInt32(param[0])].ClearAction -= TicTacToeBoard_ClearAction;
            gameNotifications[Convert.ToInt32(param[0])] = null;

            ////does this do all the below?
            ////gameNotifications[Convert.ToInt32(param[0])].Dispose();

            ////remove the clear action, so we can clear the notification...
            //for (int i = 0; i < gameBoards.Count; i++)
            //{
            //    if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
            //    {
            //        gameNotifications[Convert.ToInt32(param[0])].ClearAction -= gameBoards[gameBoards.Keys.ElementAt(i)].TicTacToeBoard_ClearAction;
            //    }
            //}
            ////null it out so next time we make a new notification
            //gameNotifications[Convert.ToInt32(param[0])] = null;

            //clear any existing as well
            OM.Host.UIHandler.RemoveAllMyNotifications(this, Convert.ToInt32(param[0]));
            return null;
        }

        private object AddGame(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            if (param == null)
                return null;

            ////can we create a game??? param[0] = screen1, param[1] = screen2
            //if (Convert.ToInt32(param[1]) != -1)
            //{
            //    if (multiplayerList[Convert.ToInt32(param[1])].Contains("***"))
            //    {
            //        //already playing the game, sub_update for the spectator screen (param[0]) the board id of that game
            //        for (int i = 0; i < gameBoards.Count; i++)
            //        {
            //            if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[1])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[1])))
            //            {
            //                //the screen is playing on boardID = i, param[1] is the challenged screen
            //                //return a board message for param[0] for being a spectator
            //                multiplayerList[Convert.ToInt32(param[0])] = "*** Screen: " + param[0].ToString() + " (Spectating Screen: " + param[1].ToString() + ")";
            //                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + gameBoards[i].ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { Convert.ToInt32(param[0]), String.Format("Spectate:{0}", gameBoards[i].ID) } }, true);
            //                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.Spectate", gameBoards[i].ID, true);
            //                break;
            //            }
            //        }
            //        return null;
            //    }
            //}
            //if (Convert.ToInt32(param[0]) == Convert.ToInt32(param[1]))
            //{
            //    OM.Host.UIHandler.InfoBanner_Show(Convert.ToInt32(param[0]), new InfoBanner("Cannot challenge yourself!"));
            //    return null;
            //}



            int currentBoardID = boardID;
            boardID += 1;
            StoredData.Set(this, "OMDSTicTacToe.BoardID", boardID.ToString());
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "BoardUpdated", DataSource.DataTypes.raw, "TicTacToe Game Board Updates"));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "BoardVisibility", DataSource.DataTypes.binary, "TicTacToe Game Board Visibility"));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "BoardMessages", DataSource.DataTypes.raw, "TicTacToe Game Board Messages"));

            if (Convert.ToInt32(param[1]) != -1)
            {
                
                multiplayerList[Convert.ToInt32(param[0])] = "*** Screen: " + param[0].ToString() + " (VS. Screen: " + param[1].ToString() + ")";
                if (!displayNotifications[Convert.ToInt32(param[0])])
                    OM.Host.UIHandler.InfoBanner_Show(Convert.ToInt32(param[0]), new InfoBanner(String.Format("You VS Screen: {0}{1}GOOD LUCK!!!", param[1].ToString(), Environment.NewLine)));
                multiplayerList[Convert.ToInt32(param[1])] = "*** Screen: " + param[1].ToString() + " (VS. Screen: " + param[0].ToString() + ")";
                if (!displayNotifications[Convert.ToInt32(param[1])])
                    OM.Host.UIHandler.InfoBanner_Show(Convert.ToInt32(param[1]), new InfoBanner(String.Format("You VS Screen: {0}{1}GOOD LUCK!!!", param[0].ToString(), Environment.NewLine)));
                OM.Host.DebugMsg(String.Format("Game added ({0}): Screen {1} vs Screen {2}", currentBoardID.ToString(), param[0].ToString(), param[1].ToString()));
            }
            else
            {
                multiplayerList[Convert.ToInt32(param[0])] = "*** Screen: " + param[0].ToString() + " (VS. AI)";
                OM.Host.UIHandler.InfoBanner_Show(Convert.ToInt32(param[0]), new InfoBanner(String.Format("You VS: AI{0}GOOD LUCK!!!", Environment.NewLine)));
                OM.Host.DebugMsg(String.Format("Game added ({0}): Screen {1} vs AI", currentBoardID.ToString(), param[0].ToString() ));
            }

            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.List", multiplayerList, true);
            //create all the dataSources we need
            //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "BoardUpdated", DataSource.DataTypes.raw, "TicTacToe Game Board Updates"));
            //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "BoardVisibility", DataSource.DataTypes.binary, "TicTacToe Game Board Visibility"));
            //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "BoardMessages", DataSource.DataTypes.raw, "TicTacToe Game Board Messages"));

            //create new board
            gameBoards.Add(currentBoardID, new TicTacToeBoard(currentBoardID));
            gameBoards[currentBoardID].ibp = this;
            gameBoards[currentBoardID].theHost = theHost;
            gameBoards[currentBoardID].Player1 = Convert.ToInt32(param[0]);
            gameBoards[currentBoardID].Player2 = Convert.ToInt32(param[1]);

            gameBoards[currentBoardID].RandomizeTurns();
            gameBoards[currentBoardID].RandomizePieces();

            return currentBoardID;
        }

        private object StartGame(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            if (param == null)
                return null;

            //[0] = boardID
            gameBoards[Convert.ToInt32(param[0])].StartGame();
            return null;
        }

        private object FlipTile(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            if (param == null)
                return null;

            for (int i = 0; i < gameBoards.Count; i++)
            {
                if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
                {
                    //the screen is playing on boardID = i
                    if (gameBoards[gameBoards.Keys.ElementAt(i)].Turn != Convert.ToInt32(param[0]))
                        return null;
                    if (gameBoards[gameBoards.Keys.ElementAt(i)].Layout[Convert.ToInt32(param[1])][Convert.ToInt32(param[2])] != "B")
                        return null;
                    gameBoards[gameBoards.Keys.ElementAt(i)].FlipTile(Convert.ToInt32(param[1]), Convert.ToInt32(param[2]));
                    return null;
                }
            }

            //[0] = boardID, [1] = screen clicked from, [2] = row, [3] = col
            if (gameBoards[Convert.ToInt32(param[0])].Turn != Convert.ToInt32(param[1]))
                return null;
            //return gameBoards[Convert.ToInt32(param[0])].FlipTile(Convert.ToInt32(param[2]), Convert.ToInt32(param[3]));
            return null;
        }

        private object EndGame(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            if (param == null)
                return null;
            for (int i = 0; i < gameBoards.Count; i++)
            {
                if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
                {
                    //OM.Host.DebugMsg(String.Format("Removing from gameBoards: {0}", gameBoards.Keys.ElementAt(i).ToString()));
                    //the screen is playing on boardID = i
                    multiplayerList[Convert.ToInt32(param[0])] = String.Format("Screen: {0}", param[0].ToString());
                    if(gameBoards[gameBoards.Keys.ElementAt(i)].Player2 != -1)
                        multiplayerList[gameBoards[gameBoards.Keys.ElementAt(i)].Player2] = String.Format("Screen: {0}", gameBoards[gameBoards.Keys.ElementAt(i)].Player2.ToString());
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe.Multiplayer.List", multiplayerList, true);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + gameBoards[gameBoards.Keys.ElementAt(i)].ID.ToString() + ".BoardVisibility", false, true);
                    gameBoards[gameBoards.Keys.ElementAt(i)].EndGame(Convert.ToInt32(param[0]));
                    gameBoards.Remove(gameBoards.Keys.ElementAt(i));
                    return null;
                }
            }
            //else there is just a screen unsubscribing from spectating
            multiplayerList[Convert.ToInt32(param[0])] = String.Format("Screen: {0}", param[0].ToString());

            return null;
        }

        private object Rematch(OpenMobile.Command command, object[] param, out bool result)
        {
            result = true;
            if (param == null)
                return null;
            for (int i = 0; i < gameBoards.Count; i++)
            {
                if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
                {
                    //the screen is playing on boardID = i
                    gameBoards[gameBoards.Keys.ElementAt(i)].RandomizeTurns();
                    gameBoards[gameBoards.Keys.ElementAt(i)].RandomizePieces();
                    gameBoards[gameBoards.Keys.ElementAt(i)].StartGame();
                    return null;
                }
            }
            return null;
        }

        //not used anymore as of 01/28/2014
        #region DataSourceDelegates
        private object AddGame(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;

            //can we create a game??? param[0] = screen1, param[1] = screen2
            if (Convert.ToInt32(param[1]) != -1)
                if (multiplayerList[Convert.ToInt32(param[1])].Contains("***"))
                    return null;
            if (Convert.ToInt32(param[0]) == Convert.ToInt32(param[1]))
            {
                OM.Host.UIHandler.InfoBanner_Show(Convert.ToInt32(param[0]), new InfoBanner("Cannot challenge yourself!"));
                return null;
            }
            if (Convert.ToInt32(param[1]) != -1)
            {
                multiplayerList[Convert.ToInt32(param[0])] = "*** Screen: " + param[0].ToString() + " (VS. Screen: " + param[1].ToString() + ")";
                multiplayerList[Convert.ToInt32(param[1])] = "*** Screen: " + param[1].ToString() + " (VS. Screen: " + param[0].ToString() + ")";
            }
            else
                multiplayerList[Convert.ToInt32(param[0])] = "*** Screen: " + param[0].ToString() + " (VS. AI)";


            //create all the dataSources we need
            int currentBoardID = boardID;
            boardID += 1;
            StoredData.Set(this, "OMDSTicTacToe.BoardID", boardID.ToString());

            for (int r = 1; r < 4; r++)
                for (int c = 1; c < 4; c++)
                {
                    //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), r.ToString() + "-" + c.ToString(), DataSource.DataTypes.raw, "[" + currentBoardID.ToString() + "] Tile image (row: " + r.ToString() + ", col: " + c.ToString()), theHost.getPluginImage(this, "Images|TicTacToe_B"));
                    //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), r.ToString() + "-" + c.ToString(), DataSource.DataTypes.raw, "[" + currentBoardID.ToString() + "] Tile image (row: " + r.ToString() + ", col: " + c.ToString()), theHost.getPluginImage(this, "Images|TicTacToe_B_Transparent"));
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), r.ToString() + "-" + c.ToString(), DataSource.DataTypes.raw, "[" + currentBoardID.ToString() + "] Tile image (row: " + r.ToString() + ", col: " + c.ToString()), theHost.getPluginImage(this, "Images|Icon-Glass-OMTicTacToe_None"));
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), r.ToString() + "-" + c.ToString() + "-Visible", DataSource.DataTypes.binary, ""), false);
                }
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), param[0].ToString(), DataSource.DataTypes.raw, "[" + currentBoardID.ToString() + "] player " + param[0].ToString() + " game label"), "");
            if (param[1].ToString() != "-1")
                BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), param[1].ToString(), DataSource.DataTypes.raw, "[" + currentBoardID.ToString() + "] player " + param[0].ToString() + " game label"), "");
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "VisibleStatusLabel", DataSource.DataTypes.binary, ""), false);
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "Spectator", DataSource.DataTypes.raw, ""), "[" + currentBoardID.ToString() + "] spectator game label");
            //BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "FlipTile", 0, DataSource.DataTypes.raw, FlipTile, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "VisibleMainMenuButton", DataSource.DataTypes.binary, ""), false);
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "VisibleRematchButton", DataSource.DataTypes.binary, ""), false);
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "VisibleQuitButton", DataSource.DataTypes.binary, ""), false);
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "VisibleSinglePlayerButton", DataSource.DataTypes.binary, ""), false);
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), "VisibleMultiPlayerButton", DataSource.DataTypes.binary, ""), false);

            //create new board
            gameBoards.Add(currentBoardID, new TicTacToeBoard(currentBoardID));
            gameBoards[currentBoardID].ibp = this;
            gameBoards[currentBoardID].theHost = theHost;
            gameBoards[currentBoardID].Player1 = Convert.ToInt32(param[0]);
            gameBoards[currentBoardID].Player2 = Convert.ToInt32(param[1]);

            gameBoards[currentBoardID].RandomizeTurns();
            gameBoards[currentBoardID].RandomizePieces();

            return currentBoardID;
        }

        private object StartGame(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;

            //[0] = boardID
            gameBoards[Convert.ToInt32(param[0])].StartGame();
            return null;
        }

        private object FlipTile(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;

            for (int i = 0; i < gameBoards.Count; i++)
            {
                if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
                {
                    //the screen is playing on boardID = i
                    if (gameBoards[gameBoards.Keys.ElementAt(i)].Turn != Convert.ToInt32(param[0]))
                        return null;
                    if (gameBoards[gameBoards.Keys.ElementAt(i)].Layout[Convert.ToInt32(param[1])][Convert.ToInt32(param[2])] != "B")
                        return null;
                    gameBoards[gameBoards.Keys.ElementAt(i)].FlipTile(Convert.ToInt32(param[1]), Convert.ToInt32(param[2]));
                    return null;
                }
            }

            //[0] = boardID, [1] = screen clicked from, [2] = row, [3] = col
            if (gameBoards[Convert.ToInt32(param[0])].Turn != Convert.ToInt32(param[1]))
                return null;
            //return gameBoards[Convert.ToInt32(param[0])].FlipTile(Convert.ToInt32(param[2]), Convert.ToInt32(param[3]));
            return null;
        }

        private object EndGame(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            for (int i = 0; i < gameBoards.Count; i++)
            {
                if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
                {
                    //the screen is playing on boardID = i
                    gameBoards[gameBoards.Keys.ElementAt(i)].EndGame(Convert.ToInt32(param[0]));
                    gameBoards.Remove(gameBoards.Keys.ElementAt(i));
                    return null;
                }

            }
            return null;
        }

        private object Rematch(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;
            for (int i = 0; i < gameBoards.Count; i++)
            {
                if ((gameBoards[gameBoards.Keys.ElementAt(i)].Player1 == Convert.ToInt32(param[0])) || (gameBoards[gameBoards.Keys.ElementAt(i)].Player2 == Convert.ToInt32(param[0])))
                {
                    //the screen is playing on boardID = i
                    gameBoards[gameBoards.Keys.ElementAt(i)].RandomizeTurns();
                    gameBoards[gameBoards.Keys.ElementAt(i)].StartGame();
                    return null;
                }
            }
            return null;
        }
#endregion

    }

    public class TicTacToeBoard
    {
        public IBasePlugin ibp;
        public IPluginHost theHost;
        public int ID;
        public int Player1;
        public int Player2;
        public int Turn;
        private int oldTurn;
        private bool gameOver = false;
        public string[][] Layout = new string[3][] { new string[3], new string[3], new string[3] };
        public string[] Piece = new string[2];
        Random random;
        Notification[] gameNotifications = new Notification[2]; //0 = Player1, 1 = Player2

        public TicTacToeBoard(int boardID)
        {
            ID = boardID;
            random = new Random();

        }

        public void RandomizeTurns()
        {
            int turn = random.Next(0, 2);
            if (turn == 0)
                Turn = Player1;
            else if (turn == 1)
                Turn = Player2;
        }

        public void RandomizePieces()
        {
            int turn = random.Next(0, 2);
            int piece = random.Next(0, 2);
            if (((turn == 0) && (piece == 0)) || ((turn == 1) && (piece == 1)))
            {
                Piece[0] = "X";
                Piece[1] = "O";
            }
            else if (((turn == 0) && (piece == 1)) || ((turn == 1) && (piece == 0)))
            {
                Piece[0] = "O";
                Piece[1] = "X";
            }
        }

        public void StartGame()
        {
            ResetBoard();
            string messageP1 = "";
            string messageP2 = "";
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".Spectator", "Watching Game...");
            if (Turn == Player1) //player 1 turn
            {
                messageP1 = "It is your turn, please make a move!";
                if (Player2 != -1) //not ai
                    messageP2 = "Opponent's turn, please wait!";
            }
            else if ((Player2 != -1) && (Turn == Player2)) //player 2 turn
            {
                messageP1 = "Opponent's turn, please wait!";
                messageP2 = "It is your turn, please make a move!";

            }
            else if (Turn == -1) //ai turn
            {
                messageP1 = "Computer's turn, please wait!";
                OpenMobile.Threading.SafeThread.Asynchronous(AITurn);
            }
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { Player1, messageP1 }, { Player2, messageP2 } }, true);
        }

        public void ResetBoard(bool quit = false, int screenQuit = -1)
        {
            //flip all images "blank"
            for (int r = 1; r < 4; r++)
            {
                for (int c = 1; c < 4; c++)
                {
                    Layout[r - 1][c - 1] = "B";
                 }
            }
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardUpdated", Layout, true);
            if (quit)
            {   
                if (screenQuit != -1)
                {
                    if (!gameOver)
                    {
                        if (Player1 == screenQuit)
                        {
                            if (Player2 != -1)
                                OM.Host.UIHandler.InfoBanner_Show(Player2, new InfoBanner("Your opponent has quit!"));
                        }
                        else if (Player2 == screenQuit)
                        {
                            OM.Host.UIHandler.InfoBanner_Show(Player1, new InfoBanner("Your opponent has quit!"));
                        }
                    }
                }
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardVisibility", false);
            }
            else
            {
                gameOver = false;
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardVisibility", true);
             }
        }

        public void EndGame(int screenQuit = -1)
        {
            ResetBoard(true, screenQuit);
        }

        public void FlipTile(int row, int col)
        {
            int oldTurn = Turn;
            Turn = -2;
            if (oldTurn == Player1)
            {

                Layout[row][col] = Piece[0];
            }
            else if (oldTurn == Player2)
            {
                Layout[row][col] = Piece[1];
            }
            
            SwitchTurns(oldTurn);
        }

        public void SwitchTurns(int oldTurn)
        {
            string gameEnd = GameEnd();
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardUpdated", Layout, true);
            string messageP1 = "";
            string messageP2 = "";
            if (gameEnd == "")
            {
                if (oldTurn == Player1)
                {
                    if (Player2 == -1)
                    {
                        messageP1 = "Computer's turn, please wait!";
                        Turn = Player2;
                        OpenMobile.Threading.SafeThread.Asynchronous(AITurn);
                    }
                    else
                    {
                        messageP1 = "Opponent's turn, please wait!";
                        messageP2 = "It is your turn, please make a move!";
                        Turn = Player2;
                        if (OMDSTicTacToe.displayNotifications[Player2])
                        {
                            //CreateNotification(Player2, messageP2);
                            ShowNotification(Player2, messageP2);
                        }
                    }
                }
                else if (oldTurn == Player2)
                {
                    messageP1 = "It is your turn, please make a move!";
                    if (Player2 != -1) //not ai
                        messageP2 = "Opponent's turn, please wait!";
                    Turn = Player1;
                    if (OMDSTicTacToe.displayNotifications[Player1])
                    {
                        //CreateNotification(Player1, messageP1);
                        ShowNotification(Player1, messageP1);
                    }
                }
            }
            else
            {
                gameOver = true;
                //someone won or it's a draw....
                if (gameEnd == "Win")
                {
                    if (oldTurn == Player1)
                    {
                        messageP1 = "Congraulations, you've won!";
                        if (Player2 != -1)
                        {
                            messageP2 = "Unfortunate, you've lost!";
                            if (OMDSTicTacToe.displayNotifications[Player2])
                            {
                                //CreateNotification(Player2, messageP2);
                                ShowNotification(Player2, messageP2);
                            }
                        }
                    }
                    else if (oldTurn == Player2)
                    {
                        if (Player2 != -1)
                            messageP2 = "Congratulations, you've won!";
                        messageP1 = "Unfortunate, you've lost!";
                        if (OMDSTicTacToe.displayNotifications[Player1])
                        {
                            //CreateNotification(Player1, messageP1);
                            ShowNotification(Player1, messageP1);
                        }
                    }
                }
                else
                {
                    messageP1 = "Draw!";
                    if (OMDSTicTacToe.displayNotifications[Player1])
                    {
                        //CreateNotification(Player1, messageP1);
                        ShowNotification(Player1, messageP1);
                    }
                    if (Player2 != -1)
                    {
                        messageP2 = "Draw!";
                        if (OMDSTicTacToe.displayNotifications[Player2])
                        {
                            //CreateNotification(Player2, messageP2);
                            ShowNotification(Player2, messageP2);
                        }
                    }
                }
            }
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { Player1, messageP1 }, { Player2, messageP2 } }, true);
        }

        private string GameEnd()
        {
            if ((Layout[0][0] != "B") && (Layout[0][0] == Layout[0][1]) && (Layout[0][1] == Layout[0][2]))
            {
                Layout[0][0] = Layout[0][1] = Layout[0][2] = String.Format("W{0}", Layout[0][0]);
                return "Win";
            }
            else if ((Layout[1][0] != "B") && (Layout[1][0] == Layout[1][1]) && (Layout[1][1] == Layout[1][2]))
            {
                Layout[1][0] = Layout[1][1] = Layout[1][2] = String.Format("W{0}", Layout[1][0]);
                return "Win";
            }
            else if ((Layout[2][0] != "B") && (Layout[2][0] == Layout[2][1]) && (Layout[2][1] == Layout[2][2]))
            {
                Layout[2][0] = Layout[2][1] = Layout[2][2] = String.Format("W{0}", Layout[2][0]);
                return "Win";
            }
            else if ((Layout[0][0] != "B") && (Layout[0][0] == Layout[1][0]) && (Layout[1][0] == Layout[2][0]))
            {
                Layout[0][0] = Layout[1][0] = Layout[2][0] = String.Format("W{0}", Layout[0][0]);
                return "Win";
            }
            else if ((Layout[0][1] != "B") && (Layout[0][1] == Layout[1][1]) && (Layout[0][1] == Layout[2][1]))
            {
                Layout[0][1] = Layout[1][1] = Layout[2][1] = String.Format("W{0}", Layout[0][1]);
                return "Win";
            }
            else if ((Layout[0][2] != "B") && (Layout[0][2] == Layout[1][2]) && (Layout[1][2] == Layout[2][2]))
            {
                Layout[0][2] = Layout[1][2] = Layout[2][2] = String.Format("W{0}", Layout[0][2]);
                return "Win";
            }
            else if ((Layout[0][0] != "B") && (Layout[0][0] == Layout[1][1]) && (Layout[1][1] == Layout[2][2]))
            {
                Layout[0][0] = Layout[1][1] = Layout[2][2] = String.Format("W{0}", Layout[0][0]);
                return "Win";
            }
            else if ((Layout[0][2] != "B") && (Layout[0][2] == Layout[1][1]) && (Layout[1][1] == Layout[2][0]))
            {
                Layout[0][2] = Layout[1][1] = Layout[2][0] = String.Format("W{0}", Layout[0][2]);
                return "Win";
            }
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (Layout[i][j] == "B") return ""; //if ANY piece is still blank, return blank and keep playing
            //if here it's a draw
            return "Draw";
        }

        private void CreateNotification(int screen, string message)
        {
            Notification notification = new Notification(ibp, String.Format("notification.{0}", screen.ToString()), ibp.pluginIcon.image, "Tic Tac Toe", message, TicTacToeNotification_Action, TicTacToeNotification_Clear);
            OM.Host.UIHandler.AddNotification(screen, notification);
        }

        void TicTacToeNotification_Action(Notification notification, int screen, ref bool cancel)
        {
            //send the message to go back to the game
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { screen, "BackToGame" } }, true);
        }

        void TicTacToeNotification_Clear(Notification notification, int screen, ref bool cancel)
        {
            // Cancel the clear request on this notification
            //we aren't changing anything so the notification stays
            //it will become cleared when it is clicked
            cancel = true;
        }

        /// <summary>
        /// Check if the source tile matches the target tile
        /// </summary>
        /// <param name="row1"></param>
        /// <param name="col1"></param>
        /// <param name="row2"></param>
        /// <param name="col2"></param>
        /// <returns></returns>
        private bool TileMatches(int row1, int col1, int row2, int col2, string priorityTile)
        {
            return Layout[row1][col1] == Layout[row2][col2] && Layout[row1][col1] == priorityTile && Layout[row1][col1] != "B";
        }

        /// <summary>
        /// Generate a random move
        /// </summary>
        /// <returns></returns>
        private Point GetRandomMove()
        {
            Point nextMove = new Point(random.Next(0, 3), random.Next(0, 3));
            while (Layout[nextMove.Y][nextMove.X] != "B")
            {
                nextMove.X = random.Next(0, 3);
                nextMove.Y = random.Next(0, 3);
            }
            return nextMove;
        }

        private bool ValidMove(Point nextMove)
        {
            return nextMove.X > -1 && nextMove.Y > -1 && Layout[nextMove.Y][nextMove.X] == "B";
        }

        private Point GetNextAIMove(string priorityTile)
        {
            Point nextMove = new Point(-1, -1);

            // Find first matching two in a row (horizontal)
            for (int i = 0; i < 3; i++)
            {
                if (TileMatches(i, 0, i, 1, priorityTile))
                {   // XXO
                    nextMove = new Point(2, i);
                    if (ValidMove(nextMove))
                        break;
                }
                if (TileMatches(i, 0, i, 2, priorityTile))
                {   // XOX
                    nextMove = new Point(1, i);
                    if (ValidMove(nextMove))
                        break;
                }
                if (TileMatches(i, 1, i, 2, priorityTile))
                {   // OXX
                    nextMove = new Point(0, i);
                    if (ValidMove(nextMove))
                        break;
                }
            }

            if (nextMove.X == -1)
            {
                // Find first matching two in a row (vertical)
                for (int i = 0; i < 3; i++)
                {
                    if (TileMatches(0, i, 1, i, priorityTile))
                    {   // XXO
                        nextMove = new Point(i, 2);
                        if (ValidMove(nextMove))
                            break;
                    }
                    if (TileMatches(0, i, 2, i, priorityTile))
                    {   // XOX
                        nextMove = new Point(i, 1);
                        if (ValidMove(nextMove))
                            break;
                    }
                    if (TileMatches(1, i, 2, i, priorityTile))
                    {   // OXX
                        nextMove = new Point(i, 0);
                        if (ValidMove(nextMove))
                            break;
                    }
                }
            }

            // Find first matching two in a row (diagonal)
            if (TileMatches(0, 0, 1, 1, priorityTile) && nextMove.X == -1 && !ValidMove(nextMove))
                nextMove = new Point(2, 2);
            if (TileMatches(0, 0, 2, 2, priorityTile) && nextMove.X == -1 && !ValidMove(nextMove))
                nextMove = new Point(1, 1);
            if (TileMatches(1, 1, 2, 2, priorityTile) && nextMove.X == -1 && !ValidMove(nextMove))
                nextMove = new Point(0, 0);
            if (TileMatches(0, 2, 1, 1, priorityTile) && nextMove.X == -1 && !ValidMove(nextMove))
                nextMove = new Point(0, 0);
            if (TileMatches(0, 2, 2, 0, priorityTile) && nextMove.X == -1 && !ValidMove(nextMove))
                nextMove = new Point(1, 1);
            if (TileMatches(1, 1, 2, 0, priorityTile) && nextMove.X == -1 && !ValidMove(nextMove))
                nextMove = new Point(0, 2);

            return nextMove;
        }

        public void AITurn()
        {
            lock (Layout)
            {
                Thread.Sleep(750);

                // Try winning moves
                Point nextMove = GetNextAIMove(Piece[1]);

                // Try blocking moves
                if (nextMove.X == -1)
                    nextMove = GetNextAIMove(Piece[0]);

                // Invalid move, make a random one instead
                if (!ValidMove(nextMove))
                    nextMove = GetRandomMove();
 

                FlipTile(nextMove.Y, nextMove.X);
            }
        }


        private void ShowNotification(int screen, string message)
        {
            OMDSTicTacToe.ShowNotification(ibp, screen, message);
            //if (OMDSTicTacToe.gameNotifications[screen] == null)
            //{
            //    //gameNotifications[screen] = new Notification(ibp, "OMDSTicTacToe", imageItem.NONE, imageItem.NONE, line1, line2);
            //    gameNotifications[screen] = new Notification(ibp, "OMDSTicTacToe", ibp.pluginIcon.image, "Tic Tac Toe", message);
            //    gameNotifications[screen].ClickAction += new Notification.NotificationAction(TicTacToeBoard_ClickAction);
            //    gameNotifications[screen].ClearAction += new Notification.NotificationAction(TicTacToeBoard_ClearAction);
            //    OM.Host.UIHandler.AddNotification(screen, gameNotifications[screen]);
            //}
            //else
            //{
            //    OMDSTicTacToe.gameNotifications[screen].Text = message;

            //}
        }

        private void TicTacToeBoard_ClickAction(Notification notification, int screen, ref bool cancel)
        {
            //send the message to go back to the game
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".BoardMessages", new Dictionary<int, string>() { { screen, "BackToGame" } }, true);
        }

        public void TicTacToeBoard_ClearAction(Notification notification, int screen, ref bool cancel)
        {
            cancel = true;
        }

    }

}