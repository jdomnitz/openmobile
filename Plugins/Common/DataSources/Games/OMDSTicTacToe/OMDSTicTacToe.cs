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
using System.Threading;
using System.Net;
using OpenMobile.Plugin;
using OpenMobile.Framework;
using OpenMobile.helperFunctions;
using OpenMobile;
using OpenMobile.Data;

namespace OMDSTicTacToe
{
    public class Class1 : IBasePlugin, IDataSource
    {
        static IPluginHost theHost;
        int boardID;
        Random random;
        Dictionary<int, TicTacToeBoard> gameBoards;

        public eLoadStatus initialize(IPluginHost host)
        {
            theHost = host;
            random = new Random();
            gameBoards = new Dictionary<int, TicTacToeBoard>();

            //get boardID
            if (StoredData.Get(this, "OMDSTicTacToe.BoardID") == "")
                boardID = 0;
            else
                boardID = Convert.ToInt32(StoredData.Get(this, "OMDSTicTacToe.BoardID"));

            //create the manual dataSource for starting games
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Game", "AddGame", 0, DataSource.DataTypes.raw, AddGame, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Game", "StartGame", 0, DataSource.DataTypes.raw, StartGame, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Game", "FlipTile", 0, DataSource.DataTypes.raw, FlipTile, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Game", "EndGame", 0, DataSource.DataTypes.raw, EndGame, ""));
            BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", "Game" , "Rematch", 0, DataSource.DataTypes.raw, Rematch, ""));
            return eLoadStatus.LoadSuccessful;
        }

        private object AddGame(OpenMobile.Data.DataSource dataSource, out bool result, object[] param)
        {
            result = true;
            if (param == null)
                return null;

            //create all the dataSources we need
            int currentBoardID = boardID;
            boardID += 1;
            StoredData.Set(this, "OMDSTicTacToe.BoardID", boardID.ToString());

            for (int r = 1; r < 4; r++)
                for (int c = 1; c < 4; c++)
                {
                    BuiltInComponents.Host.DataHandler.AddDataSource(new DataSource(this.pluginName, "TicTacToe", currentBoardID.ToString(), r.ToString() + "-" + c.ToString(), DataSource.DataTypes.raw, "[" + currentBoardID.ToString() + "] Tile image (row: " + r.ToString() + ", col: " + c.ToString()), theHost.getPluginImage(this, "Images|TicTacToe_B_Transparent"));
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
                        gameBoards[gameBoards.Keys.ElementAt(i)].EndGame();
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
            get { return "OMDSTicTacToe"; }
        }
        public string displayName
        {
            get { return "TicTacToe Data"; }
        }
        public float pluginVersion
        {
            get { return 0.1F; }
        }

        public string pluginDescription
        {
            get { return "TicTacToe Data"; }
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
            get { return OM.Host.getSkinImage("Icons|Icon-OMTicTacToe"); }
        }

        public void Dispose()
        {
            //
        }

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
        public string[][] Layout = new string[3][] { new string[3], new string[3], new string[3] };
        public string[] Piece = new string[2];
        Random random;

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
            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".Spectator", "Watching Game...");
            if (Turn == Player1) //player 1 turn
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "It is your turn, please make a move!");
                if (Player2 != -1) //not ai
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "Opponent's turn, please wait!");

            }
            else if ((Player2 != -1) && (Turn == Player2)) //player 2 turn
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Opponent's turn, please wait!");
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "It is your turn, please make a move!");
            }
            else if (Turn == -1) //ai turn
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Computer's turn, please wait!");
                AITurn();
            }
        }

        public void ResetBoard(bool quit = false)
        {
            //flip all images "blank"
            for (int r = 1; r < 4; r++)
                for (int c = 1; c < 4; c++)
                {
                    Layout[r - 1][c - 1] = "B";
                    if (quit)
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + r.ToString() + "-" + c.ToString() + "-Visible", false);
                    else
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + r.ToString() + "-" + c.ToString() + "-Visible", true);
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + r.ToString() + "-" + c.ToString(), theHost.getPluginImage(ibp, "Images|TicTacToe_B_Transparent"));
                }
            if (quit)
            {
                //mainmenu, quit, rematch, single player, multiplayer
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleStatusLabel", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "");
                if (Player2 != -1) //not ai
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "");
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleMainMenuButton", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleRematchButton", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleQuitButton", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleSinglePlayerButton", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleMultiPlayerButton", true);
            }
            else
            {
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleStatusLabel", true);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleMainMenuButton", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleRematchButton", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleQuitButton", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleSinglePlayerButton", false);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleMultiPlayerButton", false);
            }
        }

        public void EndGame()
        {
            ResetBoard(true);
        }

        public void FlipTile(int row, int col)
        {
            int oldTurn = Turn;
            Turn = -2;
            if (oldTurn == Player1)
            {

                Layout[row][col] = Piece[0];
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + (row + 1).ToString() + "-" + (col + 1).ToString(), theHost.getPluginImage(ibp, "Images|TicTacToe_" + Piece[0] + "_Transparent"));
            }
            else if (oldTurn == Player2)
            {
                Layout[row][col] = Piece[1];
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + (row + 1).ToString() + "-" + (col + 1).ToString(), theHost.getPluginImage(ibp, "Images|TicTacToe_" + Piece[1] + "_Transparent"));
                // Turn = Player1;
            }
            SwitchTurns(oldTurn);
        }

        public void SwitchTurns(int oldTurn)
        {
            string gameEnd = GameEnd();
            if (gameEnd == "")
            {
                if (oldTurn == Player1)
                {
                    if (Player2 == -1)
                    {
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Computer's turn, please wait!");
                        Turn = Player2;
                        AITurn();
                    }
                    else
                    {
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Opponent's turn, please wait!");
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "It is your turn, please make a move!");
                        Turn = Player2;
                    }
                }
                else if (oldTurn == Player2)
                {
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "It is your turn, please make a move!");
                    if (Player2 != -1) //not ai
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "Opponent's turn, please wait!");
                    Turn = Player1;
                }
                //return "";
            }
            else
            {
                //someone won or it's a draw....
                if (gameEnd == "Win")
                {
                    if (oldTurn == Player1)
                    {
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Congratulations, you've won!");
                        if (Player2 != -1)
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "Unfortunate, you've lost!");
                    }
                    else if (oldTurn == Player2)
                    {
                        if (Player2 != -1)
                            BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "Congratulations, you've won!");
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Unfortunate, you've lost!");
                    }
                    //return Player2.ToString();
                }
                else
                {
                    BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player1.ToString(), "Draw!");
                    if (Player2 != -1)
                        BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + "." + Player2.ToString(), "Draw!");
                    //return Player2.ToString();
                }
                //if here, we need to change visible = true for "return to main menu","rematch" buttons, visible = false for "quit" button
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleMainMenuButton", true);
                BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleRematchButton", true);
                //BuiltInComponents.Host.DataHandler.PushDataSourceValue("OMDSTicTacToe;TicTacToe." + ID.ToString() + ".VisibleQuitButton", true);
            }
        }

        private string GameEnd()
        {
            if ((Layout[0][0] != "B") && (Layout[0][0] == Layout[0][1]) && (Layout[0][1] == Layout[0][2]))
                return "Win";
            else if ((Layout[1][0] != "B") && (Layout[1][0] == Layout[1][1]) && (Layout[1][1] == Layout[1][2]))
                return "Win";
            else if ((Layout[2][0] != "B") && (Layout[2][0] == Layout[2][1]) && (Layout[2][1] == Layout[2][2]))
                return "Win";
            else if ((Layout[0][0] != "B") && (Layout[0][0] == Layout[1][0]) && (Layout[1][0] == Layout[2][0]))
                return "Win";
            else if ((Layout[0][1] != "B") && (Layout[0][1] == Layout[1][1]) && (Layout[0][1] == Layout[2][1]))
                return "Win";
            else if ((Layout[0][2] != "B") && (Layout[0][2] == Layout[1][2]) && (Layout[1][2] == Layout[2][2]))
                return "Win";
            else if ((Layout[0][0] != "B") && (Layout[0][0] == Layout[1][1]) && (Layout[1][1] == Layout[2][2]))
                return "Win";
            else if ((Layout[0][2] != "B") && (Layout[0][2] == Layout[1][1]) && (Layout[1][1] == Layout[2][0]))
                return "Win";
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (Layout[i][j] == "B") return ""; //if ANY piece is still blank, return blank and keep playing
            //if here it's a draw
            return "Draw";
        }

        public void AITurn()
        {
            int row = random.Next(0, 3);
            int col = random.Next(0, 3);
            while (Layout[row][col] != "B")
            {
                row = random.Next(0, 3);
                col = random.Next(0, 3);
            }
            FlipTile(row, col);
        }

    }

}