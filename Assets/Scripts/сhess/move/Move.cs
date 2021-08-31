using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;

namespace move {
    public struct MoveRes {
        public Vector2Int? pos;
        public bool isPieceOnPos;
        public bool isPawnChange;

    }

    public static class Move {
        public static MoveRes CheckMove(
            Vector2Int start,
            Vector2Int end,
            List<Vector2Int> movePos,
            Option<Piece>[,] board
        ) {
            MoveRes moveRes = new MoveRes();
            foreach (var pos in movePos) {
                if (Equals(pos, end)) {
                    if (board[start.x, start.y].Peel().type == PieceType.Pawn) {
                        if (end.x == 7 || end.x == 0) {
                            moveRes.isPawnChange = true;
                        }
                    }
                    if (board[end.x, end.y].IsSome()) {
                        moveRes.isPieceOnPos = true;
                        moveRes.pos = new Vector2Int(end.x, end.y);
                    } else {
                        moveRes.pos = new Vector2Int(end.x, end.y);
                    }
                }
            }
            
            return moveRes;
        }

        public static List<Vector2Int> GetPossibleMovePosition(
            List<Movement> moveList,
            Vector2Int pos,
            Option<Piece>[,] board
        ) {
            var possibleMovePositions = new List<Vector2Int>();
            float startAngle;
            foreach (var movment in moveList) {
                if (movment.linear.HasValue) {
                    possibleMovePositions.AddRange(Rules.GetLinearMoves(
                        board,
                        pos,
                        movment.linear.Value
                    ));
                } else {
                    if (board[pos.x, pos.y].Peel().type == PieceType.Knight) {
                        startAngle = 22.5f;
                    } else {
                        startAngle = 20f;
                    }
                    possibleMovePositions = Rules.GetCirclularMoves(
                        board,
                        pos,
                        movment.circular.Value,
                        startAngle
                    );
                }
            }
 
            return possibleMovePositions;
        }

        public static List<Vector2Int> SelectPawnMoves(
            Option<Piece>[,] board,
            Vector2Int position,
            List<Vector2Int> possibleMoves,
            Vector2Int? enPassant
        ) {
            Piece pawn = board[position.x, position.y].Peel();
            int dir;
            List<Vector2Int> newPossibleMoves = new List<Vector2Int>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }

            foreach (var pos in possibleMoves) {
                if (position.x == 1 && dir == 1 || position.x == 6 && dir == -1) {
                    if (Equals(pos, new Vector2Int(position.x + 2 * dir, position.y))
                        && board[pos.x, pos.y].IsNone()) {
                        newPossibleMoves.Add(pos);
                    }
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y))
                    && board[pos.x, pos.y].IsNone()) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y + dir))
                    && board[pos.x, pos.y].IsSome()
                    && board[pos.x, pos.y].Peel().color != pawn.color) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y - dir))
                    && board[pos.x, pos.y].IsSome()
                    && board[pos.x, pos.y].Peel().color != pawn.color) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y - dir))
                    && enPassant != null
                    && Equals(new Vector2Int(position.x, position.y - dir), enPassant)) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y + dir))
                    && enPassant != null
                    && Equals(new Vector2Int(position.x, position.y + dir), enPassant)) {
                    newPossibleMoves.Add(pos);
                }
            }

            return newPossibleMoves;
        } 
    }
}

