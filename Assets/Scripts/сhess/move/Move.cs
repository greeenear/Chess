using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;

namespace move {
    public struct MoveRes {
        public Vector2Int? start;
        public Vector2Int? moveTo;
        public Vector2Int? enPassant;
        public bool isPieceOnPos;
        public bool isPawnChange;
    }

    public struct StartAngle {
        public float Knight;
        public float King;

        public static StartAngle Mk(float Knight, float King) {
            return new StartAngle { Knight = Knight, King = King };
        }
    }

    public static class Move {
        public static MoveRes CheckMove(
            Vector2Int start,
            Vector2Int end,
            List<Vector2Int> movePos,
            Option<Piece>[,] board
        ) {
            MoveRes moveRes = new MoveRes();
            moveRes.start = start;
            foreach (var pos in movePos) {
                if (pos == end) {
                    board[end.x, end.y] = board[start.x, start.y];
                    board[start.x, start.y] = Option<Piece>.None();
                    moveRes.moveTo = new Vector2Int(end.x, end.y);

                    if (board[end.x, end.y].IsSome()) {
                        moveRes.isPieceOnPos = true;
                        moveRes.moveTo = new Vector2Int(end.x, end.y);
                    }
                    if (board[end.x, end.y].Peel().type == PieceType.Pawn) {
                        if (Mathf.Abs(end.x - start.x) == 2) {
                            var rightCell = board[end.x, end.y + 1];
                            var leftCell = board[end.x, end.y - 1];
                            var pieceColor = board[end.x, end.y].Peel().color;

                            if (rightCell.IsSome() && rightCell.Peel().color != pieceColor) {
                                moveRes.enPassant = new Vector2Int(end.x, end.y);
                            }
                            if (leftCell.IsSome() && leftCell.Peel().color != pieceColor) {
                                moveRes.enPassant = new Vector2Int(end.x, end.y);
                            }
                        }
                        if (end.x == 7 || end.x == 0) {
                            moveRes.isPawnChange = true;
                        }
                    }
                }
            }

            return moveRes;
        }

        public static List<Vector2Int> GetMoveCells(
            List<Movement> moveList,
            Vector2Int pos,
            Option<Piece>[,] board
        ) {
            var possibleMoves = new List<Vector2Int>();
            int maxLength;
            float startAngle;
            var angle = StartAngle.Mk(22.5f, 20f);

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
                    if (board[pos.x, pos.y].Peel().type == PieceType.Knight) {
                        startAngle = angle.Knight;
                    } else {
                        startAngle = angle.King;
                    }
                    possibleMoves = Rules.GetCirclularMoves(
                        board,
                        pos,
                        movment.circular.Value,
                        startAngle
                    );
                }
            }
            if(board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                possibleMoves = SelectPawnMoves(board, pos, possibleMoves);
            }

            return possibleMoves;
        }

        public static List<Vector2Int> SelectPawnMoves(
            Option<Piece>[,] board,
            Vector2Int pos,
            List<Vector2Int> possibleMoves
        ) {
            Piece pawn = board[pos.x, pos.y].Peel();
            int dir;
            List<Vector2Int> newPossibleMoves = new List<Vector2Int>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }

            foreach (var possible in possibleMoves) {
                if (pos.x == 1 && dir == 1 || pos.x == 6 && dir == -1) {
                    if (possible == new Vector2Int(pos.x + 2 * dir, pos.y)
                        && board[possible.x, possible.y].IsNone()) {
                        newPossibleMoves.Add(possible);
                    }
                }

                if (possible == new Vector2Int(pos.x + dir, pos.y)
                    && board[possible.x, possible.y].IsNone()) {
                    newPossibleMoves.Add(possible);
                }

                if (possible == new Vector2Int(pos.x + dir, pos.y + dir)
                    && board[possible.x, possible.y].IsSome()
                    && board[possible.x, possible.y].Peel().color != pawn.color) {
                    newPossibleMoves.Add(possible);
                }

                if (possible == new Vector2Int(pos.x + dir, pos.y - dir)
                    && board[possible.x, possible.y].IsSome()
                    && board[possible.x, possible.y].Peel().color != pawn.color) {
                    newPossibleMoves.Add(possible);
                }
            }

            return newPossibleMoves;
        }
    }
}