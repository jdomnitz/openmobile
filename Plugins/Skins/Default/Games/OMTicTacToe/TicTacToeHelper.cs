using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMobile.helperFunctions;
using OpenMobile.Plugin;

namespace OMTicTacToe
{
    class TicTacToeHelper
    {
        public delegate void SingleGameEventHandler(string Action, int BoardID, int Screen);
        public static event SingleGameEventHandler SingleGame;

        public static Dictionary<int, TicTacToeBoard> Boards = new Dictionary<int, TicTacToeBoard>();
        static List<int> screensPlayingSingle = new List<int>();
        static List<int> screensPlayingMulti = new List<int>();
        public static int boardID;
        public static IBasePlugin ibp;

        public static void IntializeHelper()
        {
            if (StoredData.Get(ibp, "BoardID") == "")
                boardID = 0;
            else
                boardID = Convert.ToInt32(StoredData.Get(ibp, "BoardID"));
        }

        public static void AddGame(int screen1)
        {
            int currentID = boardID;
            Boards.Add(currentID, new TicTacToeBoard(boardID));
            Boards[currentID].SubscribedScreens.Add(screen1);
            Boards[currentID].player1 = screen1;
            Boards[currentID].player2 = -1;
            Boards[currentID].AI = true;
            Boards[currentID].ResetBoard();
            screensPlayingSingle.Add(screen1);
            Boards[currentID].StartGame();
            if (SingleGame != null) SingleGame("Add", currentID, screen1);
            //if(RefreshList != null) RefreshList("Add", new List<int> {screen1} );
        }

        public static void AddGame(int screen1, int screen2)
        {
            screensPlayingMulti.Add(screen1);
            screensPlayingMulti.Add(screen2);
            //if(RefreshList != null) RefreshList("Add", new List<int> {screen1, screen2} );
        }

        public static void CancelGame(int BoardID)
        {
            //Boards[BoardID].CancelGame();
        }

        public static List<int> ScreensPlayingSingle()
        {
            return screensPlayingSingle;
        }

        public static List<int> ScreensPlayingMulti()
        {
            return screensPlayingMulti;
        }

        public static bool IsPlayingSingle(int screen)
        {
            if (screensPlayingSingle.Contains(screen))
                return true;
            else
                return false;
        }

        public static bool IsPlayingMulti(int screen)
        {
            if (screensPlayingMulti.Contains(screen))
                return true;
            else
                return false;
        }

        //public static List<int> Players(int BoardID)
        //{

        //}

        //public static List<int> Spectators(int BoardID)
        //{

        //}

    }

    public class TicTacToeBoard
    {
        public delegate void FlippedTileEventHandler(int Row, int Col, int Screen);
        public event FlippedTileEventHandler FlippedTile;
        public delegate void SwitchedTurnsEventHandler(int TurnOver, int TurnNow);
        public event SwitchedTurnsEventHandler SwitchedTurns;
        public delegate void GameEndedEventHandler(string Action);
        public event GameEndedEventHandler GameEnded;
        public int ID;
        public int player1;
        public int player2;
        public int Turn;
        public bool AI = false;
        public string[][] Layout = new string[3][] { new string[3], new string[3], new string[3] };
        public List<int> SubscribedScreens = new List<int>();
        Random random;
        public string[] Piece = new string[2];

        public TicTacToeBoard(int BoardID)
        {
            ID = BoardID;
            TicTacToeHelper.boardID += 1;
            StoredData.Set(TicTacToeHelper.ibp, "BoardID", TicTacToeHelper.boardID.ToString());
            //ResetBoard();
            random = new Random();
            //RandomizeTurn();
        }

        public void ResetBoard()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Layout[i][j] = "B";
                    for (int s = 0; s < SubscribedScreens.Count; s++)
                        if (FlippedTile != null)
                            FlippedTile(i, j, s);
                }

        }

        public void StartGame()
        {
            RandomizeTurn();
        }

        private void RandomizeTurn()
        {
            int turn = random.Next(0, 2);
            if (turn == 0)
            {
                Turn = player1;
            }
            else if (turn == 1)
            {
                Turn = player2;
            }
            Piece[0] = "X";
            Piece[1] = "O";
            if (Turn == -1) //ai
                AITurn();
        }

        public void FlipTile(int Row, int Col)
        {
            if (Turn == player1)
                Layout[Row][Col] = Piece[0];
            else if (Turn == player2)
                Layout[Row][Col] = Piece[1];
            for (int i = 0; i < SubscribedScreens.Count; i++)
            {
                if (FlippedTile != null)
                    FlippedTile(Row, Col, i);
            }
            string gameEnd = GameEnd();
            if (gameEnd == "")
                SwitchTurns();
            else
            {
                if (GameEnded != null) GameEnded(gameEnd);
            }
        }

        private void SwitchTurns()
        {
            int oldTurn = Turn;
            if (Turn == player1) //from player1
                Turn = player2;
            else if (Turn == player2) //from player2
                Turn = player1;
            if (SwitchedTurns != null) SwitchedTurns(oldTurn, Turn);
            if (Turn == -1) //ai
                AITurn();
        }

        private void AITurn()
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
    }

}
