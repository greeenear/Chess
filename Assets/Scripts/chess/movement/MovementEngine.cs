using System.Collections.Generic;
using UnityEngine;
using rules;
using board;

namespace movement {

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
        public PieceTrace? trace;

        public static MoveInfo Mk(DoubleMove doubleMove) {
            return new MoveInfo { doubleMove = doubleMove };
        }
    }
    public static class MovementEngine {
        public static List<PieceMovement> GetPieceMovements(CellInfo[,] board, Vector2Int pos) {
            var pieceOpt = board[pos.x, pos.y];
            if (pieceOpt.piece.IsNone()) {
                return null;
            }
            var piece = pieceOpt.piece.Peel();
            var movements = new List<PieceMovement>();
            var startMovements = storage.Storage.movement[piece.type];

            foreach (var movement in startMovements) {
                var fixedMovement = new FixedMovement();

                if (piece.type == PieceType.Pawn) {
                    var moveDir = movement.linear.Value.dir;
                    var newLinear = movement.linear.Value;
                    if (piece.color == PieceColor.White) {
                        newLinear.dir = -moveDir;
                    }
                    if (moveDir == new Vector2Int(1, 0)) {
                        if (piece.moveCounter == 0) {
                            newLinear.length = 2;
                        }
                        fixedMovement = FixedMovement.Mk(Movement.Linear(newLinear), pos);
                        var pieceMovement = new PieceMovement {
                            movement = fixedMovement,
                            movementType = MovementType.Move,
                            traceIndex = 1 
                        };
                        movements.Add(pieceMovement);
                    } else {
                        fixedMovement = FixedMovement.Mk(Movement.Linear(newLinear), pos);
                        movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Attack));
                    }
                } else if (movement.linear.HasValue) {
                    var newLinear = movement.linear.Value;
                    var boardOpt = Rules.GetOptBoard(board);

                    newLinear.length = Board.GetMaxLength(boardOpt, newLinear.length);
                    fixedMovement = FixedMovement.Mk(Movement.Linear(newLinear), pos);
                    movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Attack));
                    movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Move));
                } else if (movement.circular.HasValue) {
                    var circularMovement = Movement.Circular(movement.circular.Value);
                    fixedMovement = FixedMovement.Mk(circularMovement, pos);
                    movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Attack));
                    movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Move));
                }
            }
            return movements;
        }
    }
}

