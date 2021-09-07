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
    public struct MoveInfo {
        public MoveData first;
        public MoveData? second;
        public Vector2Int? sentenced;
    }

    public struct StartAngle {
        public const float Knight = 22.5f;
        public const float King = 20f;
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
                        startAngle = StartAngle.Knight;
                    } else {
                        startAngle = StartAngle.King;
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
                    moveResList.Add(new MoveInfo {
                        first = MoveData.Mk(pos, move), sentenced = move
                    });
                } else {
                    moveResList.Add(new MoveInfo {first = MoveData.Mk(pos, move)});
                }
            }

            if (board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                moveResList = SelectPawnMoves(board, pos, moveResList);
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
                var cell = board[possible.first.to.x, possible.first.to.y];

                if (pos.x == 1 && dir == 1 || pos.x == 6 && dir == -1) {
                    if (possible.first.to == new Vector2Int(pos.x + 2 * dir, pos.y)
                        && cell.IsNone()) {
                        newPossibleMoves.Add(possible);
                    }
                }
                if (possible.first.to == new Vector2Int(pos.x + dir, pos.y) && cell.IsNone()) {
                    newPossibleMoves.Add(possible);
                }
                if (possible.first.to == new Vector2Int(pos.x + dir, pos.y + dir)
                    && cell.IsSome()
                    && cell.Peel().color != pawn.color) {
                    newPossibleMoves.Add(possible);
                }
                if (possible.first.to == new Vector2Int(pos.x + dir, pos.y - dir)
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
                newPossibleMoves.Add(new MoveInfo {
                    first = MoveData.Mk(pos, new Vector2Int(pos.x + colorDir, pos.y + dir)),
                    sentenced = new Vector2Int(pos.x, pos.y + dir)}
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
            if(board[pos.x, pos.y].Peel().moveCounter != 0) {
                return;
            }
            while (i != rookPos.y) {
                i = i + dir;
                if (board[pos.x, i].IsSome() && board[pos.x, i].Peel().type != PieceType.Rook) {
                    break;
                } else if (board[pos.x, i].Peel().type == PieceType.Rook){
                    if (i == rookPos.y && board[pos.x, i].Peel().moveCounter == 0) {
                        newPossibleMoves.Add(new MoveInfo {
                            first = MoveData.Mk(pos, new Vector2Int(pos.x, pos.y + 2 * dir)),
                            second = MoveData.Mk(rookPos, new Vector2Int(pos.x, pos.y + dir))
                        });
                    }
                }
            }
        }
    }
}