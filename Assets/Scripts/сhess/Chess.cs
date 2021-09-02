using UnityEngine;
using rules;
using option;
using move;
using board;
using System.Collections.Generic;

namespace chess {
    public static class Chess {
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

        public static List<Vector2Int> GetPossibleMoveCells(
            Dictionary<PieceType,List<Movement>> movement,
            Vector2Int pos,
            Option<Piece>[,] board
        ) {
            var possibleMoveCells = new List<Vector2Int>();
            var movementList = movement[board[pos.x, pos.y].Peel().type];

            possibleMoveCells = move.Move.GetMoveCells(movementList, pos, board);
            possibleMoveCells = check.Check.HiddenCheck(possibleMoveCells, pos, movement, board);

            return possibleMoveCells;
        }

        public static void CheckCastling() {
            Debug.Log("CheckCastling");
        }

        public static MoveRes Move(
            MoveRes res,
            List<Vector2Int> canMovePos,
            Option<Piece>[,] board,
            GameObject boardObj,
            GameObject[,] piecesMap
        ) {
            var offset = boardObj.transform.position;
            var start = res.start.Value;

            if(res.toMove != null) {
                var end = res.toMove.Value;
                var x = end.x;
                var y = end.y;

                if (res.isPieceOnPos) {
                    GameObject.Destroy(piecesMap[x, y]);
                }
                piecesMap[x, y] = piecesMap[start.x, start.y];

                piecesMap[x, y].transform.position =
                new Vector3(
                    x + offset.x - Resource.BORD_SIZE + Resource.CELL_SIZE,
                    offset.y + Resource.CELL_SIZE,
                    y + offset.z - Resource.BORD_SIZE + Resource.CELL_SIZE
                );

                return res;
            }

            return res;
        }

        public static string Check(
            Option<Piece>[,] board,
            Vector2Int selectedPos,
            PieceColor whoseMove,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            string checkRes = null;

            if (check.Check.CheckKing(board, whoseMove, movement)) {
                checkRes = "CheckKing";
            }
            if (check.Check.CheckMate(board, selectedPos, whoseMove, movement)) {
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

        public static void ChangePiece(
            Option<Piece>[,] board,
            Vector2Int pos,
            PieceType type,
            PieceColor color
        ) {
            board[pos.x, pos.y] = Option<Piece>.None();
            board[pos.x, pos.y] = Option<Piece>.Some(Piece.Mk(type, color));
        }

        public static Option<Piece>[,] CreateBoard() {
            Option<Piece>[,] board = new Option<Piece>[8,8];
            board[0, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.Black));
            board[0, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.Black));
            board[0, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.Black));
            board[0, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, PieceColor.Black));
            board[0, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, PieceColor.Black));
            board[0, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.Black));
            board[0, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.Black));
            board[0, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.Black));

            for (int i = 0; i < 8; i++) {
                board[1, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, PieceColor.Black));
            }

            board[7, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.White));
            board[7, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.White));
            board[7, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.White));
            board[7, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, PieceColor.White));
            board[7, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, PieceColor.White));
            board[7, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.White));
            board[7, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.White));
            board[7, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.White));

            for (int i = 0; i < 8; i++) {
                board[6, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, PieceColor.White));
            }

            return board;
        }
    }
}


