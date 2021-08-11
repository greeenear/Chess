using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace board {
    public class ChessBoard
    {
        public string[,] board = new string[8, 8];
        public void AddPieces(){

            board[0, 0] = "BRook";
            board[0, 1] = "BKnight";
            board[0, 2] = "BBishop";
            board[0, 4] = "BQueen";
            board[0, 3] = "BKing";
            board[0, 5] = "BBishop";
            board[0, 6] = "BKnight";
            board[0, 7] = "BRook";

            for (int i = 0; i < 8; i++){
                board[1, i] = "BPawn";
            }

            board[7, 0] = "WRook";
            board[7, 1] = "WKnight";
            board[7, 2] = "WBishop";
            board[7, 4] = "WQueen";
            board[7, 3] = "WKing";
            board[7, 5] = "WBishop";
            board[7, 6] = "WKnight";
            board[7, 7] = "WRook";

            for (int i = 0; i < 8; i++){
                board[6, i] = "WPawn";
            }

        }
    }
}

