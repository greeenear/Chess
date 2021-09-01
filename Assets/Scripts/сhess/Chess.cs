using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
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
            
            if(res.end != null) {
                var end = res.end.Value;
                var x = end.x;
                var y = end.y;

                if (res.isPieceOnPos) {
                    GameObject.Destroy(piecesMap[x, y]);
                }
                board[x, y] = board[start.x, start.y];
                board[start.x, start.y] = Option<Piece>.None();
                piecesMap[x, y] = piecesMap[start.x, start.y];

                piecesMap[x, y].transform.position =
                new Vector3(x + offset.x - 4 + 0.5f, offset.y + 0.5f, y + offset.z - 4 + 0.5f);

                if (board[x, y].Peel().type == PieceType.Pawn) {
                    var possibleEnPassant = new Vector2Int(start.x, y);

                    if (res.enPassant != null && Equals(res.enPassant, possibleEnPassant)) {
                        board[start.x, y] = Option<Piece>.None();
                        GameObject.Destroy(piecesMap[start.x, y]);
                    }
                    if (Mathf.Abs(start.x - x) == 2) {
                        res.enPassant = Chess.CheckEnPassant(end, board);
                        return res;
                    }
                }
                res.enPassant = null;

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
            int type,
            GameObject boardObj,
            Vector2Int pos,
            GameObject[,] pieceGameObjects,
            Option<Piece>[,] board,
            List<GameObject> piecesObjList,
            PieceColor whoseMove
        ) {
            var boardPos = boardObj.transform.position;
            var x = pos.x;
            var y = pos.y;

            GameObject.Destroy(pieceGameObjects[x,y]);
            PieceType pieceType = (PieceType)type;
            board[x, y] = Option<Piece>.Some(Piece.Mk(pieceType, whoseMove));

            var piece = board[x, y].Peel();
            pieceGameObjects[pos.x, pos.y] = GameObject.Instantiate(
                piecesObjList[(int)piece.type * 2 + (int)piece.color],
                new Vector3(
                    x + boardPos.x - 4 + 0.5f,
                    boardPos.y + 0.5f,
                    y + boardPos.z - 4 + 0.5f
                ),
                Quaternion.identity,
                boardObj.transform
            );
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


