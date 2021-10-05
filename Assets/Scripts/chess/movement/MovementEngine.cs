using System.Collections.Generic;
using UnityEngine;
using rules;
using board;
using option;

namespace movement {
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
            switch (pieceType) {
                case PieceType.Pawn:
                    if (piece.color == PieceColor.White) {
                        movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), 1)), pos), attack));
                        movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), 1)), pos), attack));
                        if (piece.moveCounter == 0) {
                            var movement = PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), 2)), pos), move);
                            movement.traceIndex = Option<int>.Some(1);
                            movements.Add(movement);
                        } else {
                            movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), 1)), pos), move));
                        }
                    } else if (piece.color == PieceColor.Black) {
                        movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 1)), pos), attack));
                        movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 1)), pos), attack));
                        if (piece.moveCounter == 0) {
                            var movement = PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 2)), pos), move);
                            movement.traceIndex = Option<int>.Some(1);
                            movements.Add(movement);
                        } else {
                            movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 1)), pos), move));
                        }
                    }
                    break;
                case PieceType.Bishop:
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 8)), pos), move));
                    break;
                case PieceType.Rook:
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, 1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, -1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, 1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, -1), 8)), pos), move));
                    break;
                case PieceType.Queen:
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, 1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, -1), 8)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, 1), 8)), pos), move));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Linear(Linear.Mk(new Vector2Int(0, -1), 8)), pos), move));
                    break;
                case PieceType.Knight:
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Circular(Circular.Mk(2f)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Circular(Circular.Mk(2f)), pos), move));
                    break;
                case PieceType.King:
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Circular(Circular.Mk(1f)), pos), attack));
                    movements.Add(PieceMovement.Mk(FixedMovement.Mk(Movement.Circular(Circular.Mk(1f)), pos), move));
                    break;
            }
            return movements;
        }
    }
}