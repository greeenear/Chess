using UnityEngine;
using rules;
using option;
using check;
using board;
using System.Collections.Generic;

namespace chess {
    public class Chess : MonoBehaviour {
        public static Vector2Int? CheckEnPassant(Vector2Int endPos, Option<Piece>[,] board) {
            var leftPiece = board[endPos.x, endPos.y - 1];
            var rigthPiece = board[endPos.x, endPos.y + 1];

            if (leftPiece.IsSome() && leftPiece.Peel().type == PieceType.Pawn) {
                return new Vector2Int(endPos.x, endPos.y);
            }
            if (rigthPiece.IsSome() && rigthPiece.Peel().type == PieceType.Pawn) {
                return new Vector2Int(endPos.x, endPos.y);
            }

            return null;
        }

        public static string Check(
            Option<Piece>[,] board,
            Vector2Int selectedPos,
            PieceColor whoseMove,
            Dictionary<PieceType,List<Movement>> movement,
            Vector2Int? enPassant
            ) {
            string checkRes = null;

            if(check.Check.CheckKing(board, whoseMove, movement,enPassant)) {
                checkRes = "CheckKing";
            }
            if(check.Check.CheckMate(board, selectedPos, whoseMove, movement, enPassant)) {
                checkRes = "CheckMate";
            }

            return checkRes;
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            if (whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }

            return whoseMove;
        }
    }
}


