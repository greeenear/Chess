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

            var moveInfos = new List<MoveInfo>();
            foreach (var movement in movementList) { 
                var possibleMoveCells = Rules.GetMoves(board, movement, pos);
                foreach (var cell in possibleMoveCells) {
                    var moveInfo = new MoveInfo {
                        doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, cell))
                    };
                    if (board[cell.x, cell.y].piece.IsSome()) {
                        moveInfo.sentenced = cell;
                    }
                    if (board[cell.x, cell.y].trace.IsSome()){
                        moveInfo.sentenced = new Vector2Int(pos.x, cell.y);
                    }
                    moveInfos.Add(moveInfo);
                }
            }
            var targetPiece = board[pos.x, pos.y].piece.Peel();

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

            foreach (var movement in startMovements) {
                var fixedMovement = new FixedMovement();

                if (piece.type == PieceType.Pawn) {
                    var moveDir = movement.linear.Value.dir;
                    var newLinear = movement.linear.Value;
                    if (moveDir == new Vector2Int(1, 0)) {
                        if (piece.moveCounter == 0) {
                            newLinear.length = 2;
                        }
                        fixedMovement = FixedMovement.Mk(Movement.Linear(newLinear), pos);
                        movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Move));
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
                    fixedMovement = FixedMovement.Mk(Movement.Circular(movement.circular.Value), pos);
                    movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Attack));
                    movements.Add(PieceMovement.Mk(fixedMovement, MovementType.Move));
                }
            }

            return movements;

            // if (piece.type == PieceType.Pawn) {
            //     foreach (var movement in startMovements) {
            //         var moveDir = movement.linear.Value.dir;
            //         var newLinear = movement.linear.Value;
            //         var movementType = movement.movementType;

            //         if (piece.color == PieceColor.White) {
            //             newLinear.dir = -moveDir;
            //         }
            //         if (piece.moveCounter == 0 && movementType == MovementType.Move) {
            //             newLinear.length = 2;
            //         }
            //         var newMovement = Movement.Linear(newLinear, movementType);
            //         var tracePosX = (pos.x + (pos.x + newLinear.dir.x * 2)) / 2;
            //         var tracePos = new Vector2Int(tracePosX, pos.y);
            //         var trace = new PieceTrace { pos = tracePos, whoLeft = PieceType.Pawn};
            //         movements.Add(new PieceMovement { movement = newMovement, trace = trace });
            //     }
            //     return movements;
            // }
            // if (piece.type == PieceType.King) {
            // }
            // foreach (var movement in startMovements) {
            //     movements.Add(new PieceMovement { movement = movement });
            // }
            // return movements;
        }
    }
}