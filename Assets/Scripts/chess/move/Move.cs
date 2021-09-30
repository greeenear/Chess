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
        public PieceTrace? trace;

        public static MoveInfo Mk(DoubleMove doubleMove) {
            return new MoveInfo { doubleMove = doubleMove };
        }
    }

    public static class Move {
        public static void MovePiece(Vector2Int start, Vector2Int end, CellInfo[,] board) {
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y].piece = Option<Piece>.None();
            var piece = board[end.x, end.y].piece.Peel();
            piece.moveCounter++;
            board[end.x, end.y].piece = Option<Piece>.Some(piece);
        }

        public static List<MoveInfo> GetMoveInfos(
            List<PieceMovement> movementList,
            Vector2Int pos,
            CellInfo[,] board
        ) {
            if (board[pos.x, pos.y].piece.IsNone()) {
                return null;
            }

            List<FixedMovement> fixedMovements = new List<FixedMovement>();
            // foreach (var movement in movementList) {
            //     fixedMovements.Add(FixedMovement.Mk(movement.movement, pos));
            // }

            var moveInfos = new List<MoveInfo>();
            var possibleMoveCells = new List<Vector2Int>();
            foreach (var movement in movementList) {
                possibleMoveCells.AddRange(Rules.GetMoves(board, movement, pos));
            }
            var targetPiece = board[pos.x, pos.y].piece.Peel();
            foreach (var cell in possibleMoveCells) {
                var moveInfo = new MoveInfo {
                    doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, cell))
                };
                if (board[cell.x, cell.y].piece.IsSome()) {
                    moveInfo.sentenced = cell;
                    moveInfos.Add(moveInfo);
                } else {
                    var moveLength = pos.x - cell.x;
                    // if (targetPiece.type == PieceType.Pawn) {
                    //     if (Mathf.Abs(moveLength) == 2) {
                    //         var tracePos = new Vector2Int(Mathf.Abs((pos + cell).x) / 2, cell.y);
                    //         var newTrace = new PieceTrace { pawnTrace = tracePos };

                    //         moveInfo.trace = newTrace;
                    //     }
                    //     if (trace.HasValue) {
                    //         var pawnTrace = trace.Value.pawnTrace;
                    //         if (pawnTrace.HasValue && cell == pawnTrace.Value) {
                    //             moveInfo.sentenced = new Vector2Int(pos.x, pawnTrace.Value.y);
                    //         }
                    //     }
                    // }
                    moveInfos.Add(moveInfo);
                }
            }

            if (targetPiece.type == PieceType.Pawn) {
                foreach (var info in new List<MoveInfo>(moveInfos)) {
                    var moveTo = info.doubleMove.first.to;
                    if (moveTo.x == 0 || moveTo.x == board.GetLength(1) - 1) {
                        var promotion = info;
                        promotion.pawnPromotion = true;
                        moveInfos.Remove(info);
                        moveInfos.Add(promotion);
                    }
                }
            }

            return moveInfos;
        }

        public static List<PieceMovement> GetPieceMovements(CellInfo[,] board, Vector2Int pos) {
            var pieceOpt = board[pos.x, pos.y];
            if (pieceOpt.piece.IsNone()) {
                return null;
            }
            var piece = pieceOpt.piece.Peel();
            var movements = new List<PieceMovement>();
            var startMovements = storage.Storage.movement[piece.type];

            if (piece.type == PieceType.Pawn) {
                foreach (var movement in startMovements) {
                    var moveDir = movement.linear.Value.dir;
                    var newLinear = movement.linear.Value;
                    var movementType = movement.movementType;

                    if (piece.color == PieceColor.White) {
                        newLinear.dir = -moveDir;
                    }
                    if (piece.moveCounter == 0 && movementType == MovementType.Move) {
                        newLinear.length = 2;
                    }
                    var newMovement = Movement.Linear(newLinear, movementType);
                    var tracePosX = (pos.x + (pos.x + newLinear.dir.x * 2)) / 2;
                    var tracePos = new Vector2Int(tracePosX, pos.y);
                    var trace = new PieceTrace { tracePos = tracePos, isCanTake = true };
                    movements.Add(new PieceMovement {movement = newMovement, trace = trace});
                }
                return movements;
            }
            foreach (var movement in startMovements) {
                movements.Add(new PieceMovement { movement = movement });
            }
            return movements;
        }
    }
}