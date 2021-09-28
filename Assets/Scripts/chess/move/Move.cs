using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;
using check;

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

        public static List<MoveInfo> GetMoveInfos(
            List<Movement> movementList,
            Vector2Int pos,
            Option<Piece>[,] board,
            PieceTrace? trace
        ) {
            if (board[pos.x, pos.y].IsNone()) {
                return null;
            }

            List<FixedMovement> fixedMovements = new List<FixedMovement>();
            foreach (var movement in movementList) {
                fixedMovements.Add(FixedMovement.Mk(movement, pos));
            }

            var moveInfos = new List<MoveInfo>();
            var possibleMoveCells = GetMovePositions(fixedMovements, board, trace);
            var targetPiece = board[pos.x, pos.y].Peel();
            foreach (var cell in possibleMoveCells) {
                var moveInfo = new MoveInfo {
                    doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, cell))
                };
                if (board[cell.x, cell.y].IsSome()) {
                    moveInfo.sentenced = cell;
                    moveInfos.Add(moveInfo);
                } else {
                    var moveLength = pos.x - cell.x;
                    if (targetPiece.type == PieceType.Pawn) {
                        if (Mathf.Abs(moveLength) == 2) {
                            var tracePos = new Vector2Int(Mathf.Abs((pos + cell).x) / 2, cell.y);
                            var newTrace = new PieceTrace { pawnTrace = tracePos };

                            moveInfo.trace = newTrace;
                        }
                        if (trace.HasValue) {
                            var pawnTrace = trace.Value.pawnTrace;
                            if (pawnTrace.HasValue && cell == pawnTrace.Value) {
                                moveInfo.sentenced = new Vector2Int(pos.x, pawnTrace.Value.y);
                            }
                        }
                    }
                    moveInfos.Add(moveInfo);
                }
            }

            // if (targetPiece.type == PieceType.King) {
            //     CheckCastling(moveInfos, board, pos, 1);
            //     CheckCastling(moveInfos, board, pos, -1);
            // }
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

        public static List<Movement> GetRealMovements(Option<Piece>[,] board, Vector2Int pos) {
            var pieceOpt = board[pos.x, pos.y];
            if (pieceOpt.IsNone()) {
                return null;
            }
            var piece = pieceOpt.Peel();
            var realMovements = new List<Movement>();

            if (piece.type == PieceType.Pawn) {
                foreach (var movement in storage.Storage.movement[piece.type]) {
                    var moveDir = movement.linear.Value.dir;
                    if ((piece.color == PieceColor.White && moveDir.x > 0)
                        || (piece.color == PieceColor.Black && moveDir.x < 0)) {
                        continue;
                    }
                    var movementType = movement.movementType;
                    var newLinear = movement.linear.Value;
                    if (piece.moveCounter == 0 && movementType == MovementType.Move) {
                        newLinear.length = 2;
                    }
                    realMovements.Add(Movement.Linear(newLinear, movementType));
                }
                return realMovements;
            }

            return storage.Storage.movement[piece.type];
        }

        public static List<Vector2Int> GetMovePositions(
            List<FixedMovement> fixedMovements,
            Option<Piece>[,] board,
            PieceTrace? trace
        ) {
            var possibleMoveCells = new List<Vector2Int>();

            foreach (var fixedMovement in fixedMovements) {
                possibleMoveCells.AddRange(Rules.GetMoves(board, fixedMovement, trace));
            }

            return possibleMoveCells;
        }

        // private static void CheckCastling(
        //     List<MoveInfo> newPossibleMoves,
        //     Option<Piece>[,] board,
        //     Vector2Int pos,
        //     int dir
        // ) {
        //     if (board[pos.x, pos.y].IsNone()) {
        //         return;
        //     }

        //     var piece = board[pos.x, pos.y].Peel();
        //     if (Check.IsCheck(Check.GetCheckInfo(board, piece.color, pos))) {
        //         return;
        //     }

        //     var rookPos = new Vector2Int();
        //     if (dir == -1) {
        //         rookPos = new Vector2Int(pos.x, 0);
        //     } else {
        //         rookPos = new Vector2Int(pos.x, board.GetLength(0) - 1);
        //     }

        //     int i = pos.y + dir;
        //     if (piece.moveCounter != 0) {
        //         return;
        //     }

        //     while (i != rookPos.y) {
        //         var currentPos = new Vector2Int(pos.x, i);
        //         if (Check.IsCheck(Check.GetCheckInfo(board, piece.color, currentPos))) {
        //             break;
        //         }

        //         i = i + dir;
        //         if (board[pos.x, i].IsNone()) {
        //             continue;
        //         }

        //         var currentPieceOpt = board[pos.x, i].Peel();
        //         if (currentPieceOpt.type != PieceType.Rook) {
        //             break;
        //         } else if (currentPieceOpt.type == PieceType.Rook){
        //             if (i == rookPos.y && currentPieceOpt.moveCounter == 0) {
        //                 newPossibleMoves.Add(new MoveInfo {
        //                     doubleMove = DoubleMove.MkDoubleMove(
        //                         MoveData.Mk(pos, new Vector2Int(pos.x, pos.y + 2 * dir)),
        //                         MoveData.Mk(rookPos, new Vector2Int(pos.x, pos.y + dir)))
        //                 });
        //             }
        //         }
        //     }
        // }
    }
}