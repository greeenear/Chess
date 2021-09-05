using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;

namespace move {
    public struct MoveRes {
        public Vector2Int end;
        public Vector2Int? whoDelete;

    }

    public struct StartAngle {
        public float Knight;
        public float King;

        public static StartAngle Mk(float Knight, float King) {
            return new StartAngle { Knight = Knight, King = King };
        }
    }

    public static class Move {
        public static MoveRes PieceMove(Vector2Int start, Vector2Int end, Option<Piece>[,] board) {
            MoveRes moveRes = new MoveRes();

            if (board[end.x, end.y].IsSome()) {
                moveRes.whoDelete = new Vector2Int(end.x, end.y);
            }
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y] = Option<Piece>.None();

            return moveRes;
        }

        public static List<MoveRes> GetMoveCells(
            List<Movement> moveList,
            Vector2Int pos,
            Option<Piece>[,] board
        ) {
            var possibleMoves = new List<Vector2Int>();
            var moveResList = new List<MoveRes>();
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

            foreach (var move in possibleMoves) {
                if(board[move.x, move.y].IsSome()) {
                    moveResList.Add(new MoveRes {end = move, whoDelete = move});
                } else {
                    moveResList.Add(new MoveRes {end = move});
                }
            }

            if(board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                moveResList = SelectPawnMoves(board, pos, moveResList);
            }

            return moveResList;
        }

        public static List<MoveRes> SelectPawnMoves(
            Option<Piece>[,] board,
            Vector2Int pos,
            List<MoveRes> possibleMoves
        ) {
            Piece pawn = board[pos.x, pos.y].Peel();
            int dir;
            var newPossibleMoves = new List<MoveRes>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }

            foreach (var possible in possibleMoves) {
                var cell = board[possible.end.x, possible.end.y];

                if (pos.x == 1 && dir == 1 || pos.x == 6 && dir == -1) {
                    if (possible.end == new Vector2Int(pos.x + 2 * dir, pos.y)
                        && cell.IsNone()) {
                        newPossibleMoves.Add(possible);
                    }
                }
                if (possible.end == new Vector2Int(pos.x + dir, pos.y) && cell.IsNone()) {
                    newPossibleMoves.Add(possible);
                }
                if (possible.end == new Vector2Int(pos.x + dir, pos.y + dir)
                    && cell.IsSome()
                    && cell.Peel().color != pawn.color) {
                    newPossibleMoves.Add(possible);
                }
                if (possible.end == new Vector2Int(pos.x + dir, pos.y - dir)
                    && cell.IsSome()
                    && cell.Peel().color != pawn.color) {
                    newPossibleMoves.Add(possible);
                }
            }

            return newPossibleMoves;
        }
    }
}