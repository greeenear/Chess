using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;

namespace move {
    public struct MoveData {
        public Vector2Int from;
        public Vector2Int to;

        public static MoveData Mk(Vector2Int from, Vector2Int to) {
            return new MoveData { from = from, to = to };
        }
    }

    public struct DoubleMove {
        public MoveData first;
        public MoveData? second;

        public static DoubleMove MkSingleMove(MoveData first) {
            return new DoubleMove { first = first};
        }
        public static DoubleMove MkDoubleMove(MoveData first, MoveData? second) {
            return new DoubleMove {first = first, second = second};
        }
    }

    public struct MoveInfo {
        public DoubleMove doubleMove;
        public Vector2Int? sentenced;
        public bool pawnPromotion;
    }

    public struct StartAngle {
        public const float Knight = 22.5f;
        public const float King = 20f;
    }

    public static class Move {
        public static void MovePiece(Vector2Int start, Vector2Int end, Option<Piece>[,] board) {
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y] = Option<Piece>.None();
            var piece = board[end.x, end.y].Peel();
            piece.moveCounter++;
            board[end.x, end.y] = Option<Piece>.Some(piece);
        }

        public static List<MoveInfo> GetMoveCells(
            List<Movement> moveList,
            Vector2Int pos,
            Option<Piece>[,] board,
            MoveInfo lastMove
        ) {
            var possibleMoves = new List<Vector2Int>();
            var moveResList = new List<MoveInfo>();
            int maxLength;

            foreach (var movment in moveList) {
                if (board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                    maxLength = 2;
                } else {
                    maxLength = board.GetLength(0);
                }
                if (movment.linear.HasValue) {
                    possibleMoves.AddRange(Rules.GetLinearMoves(
                        board,
                        pos,
                        movment.linear.Value,
                        maxLength
                    ));
                } else {
                    possibleMoves = Rules.GetCirclularMoves(
                        board,
                        pos,
                        movment.circular.Value
                    );
                }
            }

            foreach (var move in possibleMoves) {
                if (board[move.x, move.y].IsSome()) {
                    moveResList.Add(
                        new MoveInfo {
                            doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, move)), 
                            sentenced = move
                        }
                    );
                } else {
                    moveResList.Add(
                        new MoveInfo {
                            doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, move))
                        }
                    );
                }
            }

            if (board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                moveResList = SelectPawnMoves(board, pos, moveResList, lastMove);
            }
            if (board[pos.x, pos.y].Peel().type == PieceType.King) {
                CheckCastling(pos, board, moveResList, -1);
                CheckCastling(pos, board, moveResList, 1);
            }

            return moveResList;
        }

        public static List<MoveInfo> SelectPawnMoves(
            Option<Piece>[,] board,
            Vector2Int pos,
            List<MoveInfo> possibleMoves,
            MoveInfo lastMove
        ) {
            Piece pawn = board[pos.x, pos.y].Peel();
            int dir;
            var newPossibleMoves = new List<MoveInfo>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }

            foreach (var possible in possibleMoves) {
                MoveInfo promotion = possible;
                var cell = board[possible.doubleMove.first.to.x, possible.doubleMove.first.to.y];
                var possibleMove = possible.doubleMove.first.to;

                if (pos.x == 1 && dir == 1 || pos.x == 6 && dir == -1) {
                    if (possible.doubleMove.first.to == new Vector2Int(pos.x + 2 * dir, pos.y)
                        && cell.IsNone()) {
                        newPossibleMoves.Add(promotion);
                    }
                }
                if (possibleMove.x == 0 || possibleMove.x == 7) {
                    promotion.pawnPromotion = true;
                }
                if (possibleMove == new Vector2Int(pos.x + dir, pos.y) && cell.IsNone()) {
                    newPossibleMoves.Add(promotion);
                }
                if (possibleMove == new Vector2Int(pos.x + dir, pos.y + dir) && cell.IsSome()
                    && cell.Peel().color != pawn.color) {
                    newPossibleMoves.Add(promotion);
                }
                if (possibleMove == new Vector2Int(pos.x + dir, pos.y - dir) && cell.IsSome()
                    && cell.Peel().color != pawn.color) {
                    newPossibleMoves.Add(promotion);
                }
            }
            var piece = board[lastMove.doubleMove.first.to.x, lastMove.doubleMove.first.to.y];
            var moveLength = lastMove.doubleMove.first.from.x - lastMove.doubleMove.first.to.x;
            if (piece.Peel().type == PieceType.Pawn 
                && Mathf.Abs(moveLength) == 2) {
                if (lastMove.doubleMove.first.from.y - pos.y == 1) {
                    CheckEnPassant(newPossibleMoves, board, pos, dir, 1);
                } else if (lastMove.doubleMove.first.from.y - pos.y == -1) {
                    CheckEnPassant(newPossibleMoves, board, pos, dir, -1);
                }
            }

            return newPossibleMoves;
        }

        private static List<MoveInfo> CheckEnPassant(
            List<MoveInfo> newPossibleMoves,
            Option<Piece>[,] board,
            Vector2Int pos,
            int colorDir,
            int dir
        ) {
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            Option<Piece> checkedCell = new Option<Piece>();
            if (Board.OnBoard(new Vector2Int(pos.x, pos.y + dir), boardSize)) {
                checkedCell = board[pos.x, pos.y + dir];
            }
            Piece pawn = board[pos.x, pos.y].Peel();

            if (checkedCell.IsSome() && checkedCell.Peel().color != pawn.color
                && checkedCell.Peel().type == PieceType.Pawn
                && checkedCell.Peel().moveCounter == 1) {
                newPossibleMoves.Add(
                    new MoveInfo {
                        doubleMove = DoubleMove.MkSingleMove(
                            MoveData.Mk(
                                pos,
                                new Vector2Int(pos.x + colorDir, pos.y + dir)
                            )
                        ),
                        sentenced = new Vector2Int(pos.x, pos.y + dir)
                    }
                );
            }
            return newPossibleMoves;
        }

        private static void CheckCastling(
            Vector2Int pos,
            Option<Piece>[,] board,
            List<MoveInfo> newPossibleMoves,
            int dir
        ) {
            Vector2Int rookPos = new Vector2Int();
            if (dir == -1) {
                rookPos = new Vector2Int(pos.x, 0);
            } else {
                rookPos = new Vector2Int(pos.x, board.GetLength(0) - 1);
            }
            int i = pos.y + dir;
            if (board[pos.x, pos.y].Peel().moveCounter != 0) {
                return;
            }
            while (i != rookPos.y) {
                i = i + dir;
                if (board[pos.x, i].IsSome() && board[pos.x, i].Peel().type != PieceType.Rook) {
                    break;
                } else if (board[pos.x, i].Peel().type == PieceType.Rook){
                    if (i == rookPos.y && board[pos.x, i].Peel().moveCounter == 0) {
                        newPossibleMoves.Add(new MoveInfo {
                            doubleMove = DoubleMove.MkDoubleMove(
                                MoveData.Mk(pos, new Vector2Int(pos.x, pos.y + 2 * dir)),
                                MoveData.Mk(rookPos, new Vector2Int(pos.x, pos.y + dir)))
                        });
                    }
                }
            }
        }
    }
}