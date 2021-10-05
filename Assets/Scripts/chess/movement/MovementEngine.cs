using System.Collections.Generic;
using UnityEngine;
using rules;
using board;
using option;

namespace movement {
    public struct Direction {
        public static readonly Vector2Int up = new Vector2Int(1, 0);
        public static readonly Vector2Int down = new Vector2Int(-1, 0);
        public static readonly Vector2Int right = new Vector2Int(0, 1);
        public static readonly Vector2Int left = new Vector2Int(0, -1);
        public static readonly Vector2Int upLeft = new Vector2Int(1, -1);
        public static readonly Vector2Int upRight = new Vector2Int(1, 1);
        public static readonly Vector2Int downLeft = new Vector2Int(-1, -1);
        public static readonly Vector2Int downRight = new Vector2Int(-1, 1);
    }

    public static class MovementEngine {
        public static List<PieceMovement> GetPieceMovements(
            Option<Piece>[,] board,
            PieceType pieceType,
            Vector2Int pos
        ) {
            var pieceOpt = board[pos.x, pos.y];
            if (pieceOpt.IsNone()) {
                return null;
            }
            var piece = pieceOpt.Peel();
            var attack = MovementType.Attack;
            var move = MovementType.Move;
            var movements = new List<PieceMovement>();
            int max = Mathf.Max(board.GetLength(1), board.GetLength(0));
            switch (pieceType) {
                case PieceType.Pawn:
                    if (piece.color == PieceColor.White) {
                        movements.Add(PieceMovement.Linear(Direction.downRight, 1, pos, attack));
                        movements.Add(PieceMovement.Linear(Direction.downLeft, 1, pos, attack));
                        if (piece.moveCounter == 0) {
                            var movement = PieceMovement.Linear(Direction.down, 2, pos, move);
                            movement.traceIndex = Option<int>.Some(1);
                            movements.Add(movement);
                        } else {
                            movements.Add(PieceMovement.Linear(Direction.down, 1, pos, move));
                        }
                    } else if (piece.color == PieceColor.Black) {
                        movements.Add(PieceMovement.Linear(Direction.upRight, 1, pos, attack));
                        movements.Add(PieceMovement.Linear(Direction.upLeft, 1, pos, attack));
                        if (piece.moveCounter == 0) {
                            var movement = PieceMovement.Linear(Direction.up, 2, pos, move);
                            movement.traceIndex = Option<int>.Some(1);
                            movements.Add(movement);
                        } else {
                            movements.Add(PieceMovement.Linear(Direction.up, 1, pos, move));
                        }
                    }
                    break;
                case PieceType.Bishop:
                    movements.Add(PieceMovement.Linear(Direction.downRight, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.downLeft, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.upRight, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.upLeft, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.downRight, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.downLeft, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.upRight, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.upLeft, max, pos, move));
                    break;
                case PieceType.Rook:
                    movements.Add(PieceMovement.Linear(Direction.down, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.up, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.right, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.left, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.down, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.up, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.right, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.left, max, pos, move));
                    break;
                case PieceType.Queen:
                    movements.Add(PieceMovement.Linear(Direction.downRight, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.downLeft, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.upRight, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.upLeft, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.downRight, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.downLeft, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.upRight, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.upLeft, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.down, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.up, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.right, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.left, max, pos, attack));
                    movements.Add(PieceMovement.Linear(Direction.down, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.up, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.right, max, pos, move));
                    movements.Add(PieceMovement.Linear(Direction.left, max, pos, move));
                    break;
                case PieceType.Knight:
                    movements.Add(PieceMovement.Circular(2f, pos, attack));
                    movements.Add(PieceMovement.Circular(2f, pos, move));
                    break;
                case PieceType.King:
                    movements.Add(PieceMovement.Circular(1f, pos, attack));
                    movements.Add(PieceMovement.Circular(1f, pos, move));
                    break;
            }
            return movements;
        }
    }
}