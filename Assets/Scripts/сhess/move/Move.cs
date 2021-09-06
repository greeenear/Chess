using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;

namespace move {
    public struct MoveInfo {
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
        public static List<MoveInfo> GetMoveCells(
            List<Movement> moveList,
            Vector2Int pos,
            Option<Piece>[,] board
        ) {
            var possibleMoves = new List<Vector2Int>();
            var moveResList = new List<MoveInfo>();
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
                if (board[move.x, move.y].IsSome()) {
                    moveResList.Add(new MoveInfo {end = move, whoDelete = move});
                } else {
                    moveResList.Add(new MoveInfo {end = move});
                }
            }

            if (board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                moveResList = SelectPawnMoves(board, pos, moveResList);
            }

            return moveResList;
        }

        public static List<MoveInfo> SelectPawnMoves(
            Option<Piece>[,] board,
            Vector2Int pos,
            List<MoveInfo> possibleMoves
        ) {
            Piece pawn = board[pos.x, pos.y].Peel();
            int dir;
            int enPassantX;
            var newPossibleMoves = new List<MoveInfo>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
                enPassantX = 3;
            } else {
                dir = 1;
                enPassantX = board.GetLength(1) - 4;
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
            if (pos.x == enPassantX) {
                CheckEnPassant(newPossibleMoves, board, pos, dir, 1);
                CheckEnPassant(newPossibleMoves, board, pos, dir, -1);
            }

            return newPossibleMoves;
        }

        private static List<MoveInfo> CheckEnPassant(
            List<MoveInfo> newPossibleMoves,
            Option<Piece>[,] board,
            Vector2Int pos,
            int colorDir,
            int horizontalDir
        ) {
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            Option<Piece> checkedCell = new Option<Piece>();
            if(Board.OnBoard(new Vector2Int(pos.x, pos.y + horizontalDir), boardSize)) {
                checkedCell = board[pos.x, pos.y + horizontalDir];
            }
            Piece pawn = board[pos.x, pos.y].Peel();

            if (checkedCell.IsSome() && checkedCell.Peel().color != pawn.color
                && checkedCell.Peel().type == PieceType.Pawn
                && checkedCell.Peel().moveCounter == 1) {
                newPossibleMoves.Add(new MoveInfo {
                    end = new Vector2Int(pos.x + colorDir, pos.y + horizontalDir),
                    whoDelete = new Vector2Int(pos.x, pos.y + horizontalDir)}
                );
            }
            return newPossibleMoves;
        }
    }
}