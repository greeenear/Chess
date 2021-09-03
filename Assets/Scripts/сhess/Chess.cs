using UnityEngine;
using rules;
using option;
using move;
using board;
using System.Collections.Generic;

namespace chess {
    public struct Castling {
        public Vector2Int kingPos;
        public Vector2Int? rookPos;
    }
    public static class Chess {
        public static List<Vector2Int> GetPossibleMoveCells(
            Dictionary<PieceType,List<Movement>> movement,
            Vector2Int pos,
            Option<Piece>[,] board,
            Vector2Int? enPassant
        ) {
            var possibleMoveCells = new List<Vector2Int>();
            var movementList = movement[board[pos.x, pos.y].Peel().type];

            possibleMoveCells = move.Move.GetMoveCells(movementList, pos, board);
            possibleMoveCells = check.Check.HiddenCheck(possibleMoveCells, pos, movement, board);

            if (board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                if (enPassant != null && enPassant.Value.x == pos.x) {
                        var x = enPassant.Value.x;
                        var y = enPassant.Value.y;

                    if (board[pos.x, pos.y].Peel().color == PieceColor.White) {
                        possibleMoveCells.Add(new Vector2Int(pos.x - 1, y));
                    } else {
                        possibleMoveCells.Add(new Vector2Int(pos.x + 1, y));
                    }
                }
            }
            if(board[pos.x, pos.y].Peel().type == PieceType.King) {
                CheckCastling(pos, board);
            }

            return possibleMoveCells;
        }

        public static MoveRes Move(
            MoveRes res,
            List<Vector2Int> canMovePos,
            Option<Piece>[,] board,
            GameObject boardObj,
            GameObject[,] piecesMap
        ) {
            var boardOffset = boardObj.transform.position;
            var start = res.start.Value;

            if (res.moveTo != null) {
                var end = res.moveTo.Value;
                board[end.x, end.y] = board[start.x, start.y];
                board[start.x, start.y] = Option<Piece>.None();

                if (res.isPieceOnPos) {
                    GameObject.Destroy(piecesMap[end.x, end.y]);
                }

                piecesMap[end.x, end.y] = piecesMap[start.x, start.y];
                piecesMap[end.x, end.y].transform.position =
                new Vector3(
                    end.x + boardOffset.x - Resource.BORD_SIZE + Resource.CELL_SIZE,
                    boardOffset.y + Resource.CELL_SIZE,
                    end.y + boardOffset.z - Resource.BORD_SIZE + Resource.CELL_SIZE
                );

                if (board[end.x, end.y].Peel().type == PieceType.Pawn && res.enPassant != null) {
                    if (res.enPassant.Value == new Vector2Int(start.x, end.y)) {
                        GameObject.Destroy(piecesMap[start.x, end.y]);
                        board[start.x, end.y] = Option<Piece>.None();
                    }
                }

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
            if (check.Check.CheckMate(board, whoseMove, movement)) {
                checkRes = "CheckMate";
            }

            return checkRes;
        }

        public static Castling CheckCastling(Vector2Int kingPos, Option<Piece>[,] board) {
            List<Vector2Int> castlingMove = new List<Vector2Int>();
            Castling castlingInfo = new Castling();
            List<Movement> checkLeft = new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(0, -1)))
            };
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));

            castlingMove = move.Move.GetMoveCells(checkLeft, kingPos, board);
            foreach (var move in castlingMove) {
                if (move.y - 1 == 0
                    && board[kingPos.x, move.y - 1].Peel().type == PieceType.Rook) {
                    castlingInfo.kingPos = kingPos;
                    castlingInfo.rookPos = new Vector2Int(kingPos.x, move.y - 1);
               }
            }

            checkLeft = new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(0, 1)))
            };
            castlingMove = move.Move.GetMoveCells(checkLeft, kingPos, board);
            foreach (var move in castlingMove) {
                if (move.y + 1 == boardSize.y - 1
                    && board[kingPos.x, move.y + 1].Peel().type == PieceType.Rook) {
                    castlingInfo.kingPos = kingPos;
                    castlingInfo.rookPos = new Vector2Int(kingPos.x, move.y + 1);
               }
            }

            return castlingInfo;
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