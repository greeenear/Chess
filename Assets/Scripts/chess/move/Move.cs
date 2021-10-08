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
    public static class Move {
        public static void MovePiece(Vector2Int start, Vector2Int end, Option<Piece>[,] board) {
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y] = Option<Piece>.None();
            var piece = board[end.x, end.y].Peel();
            piece.moveCounter++;
            board[end.x, end.y] = Option<Piece>.Some(piece);
        }

        public static List<MoveInfo> GetMoveInfos(
            List<PieceMovement> pieceMovements,
            Vector2Int pos,
            FullBoard board
        ) {
            var boardOpt = board.board;
            if (boardOpt[pos.x, pos.y].IsNone()) {
                return null;
            }
            var moveInfos = new List<MoveInfo>();
            var targetPiece = boardOpt[pos.x, pos.y].Peel();
            var color = targetPiece.color;
            foreach (var pieceMovement in pieceMovements) {
                var possibleMoveCells = Rules.GetMoves(board, pieceMovement, pos);
                foreach (var cell in possibleMoveCells) {
                    var moveInfo = new MoveInfo {
                        doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, cell))
                    };
                    if (pieceMovement.traceIndex.IsSome()) {
                        var dir = pieceMovement.movement.movement.linear.Value.dir;
                        var startPos = pieceMovement.movement.startPos;
                        var tracePos = startPos + dir * pieceMovement.traceIndex.Peel();
                        if (tracePos != cell) {
                            var pieceType = targetPiece.type;
                            moveInfo.trace = new PieceTrace { pos = tracePos, whoLeft = pieceType};
                        }
                        if (pieceMovement.isFragile) {
                            var fragileCell = (pos + tracePos) / 2;
                            var checkInfos = Check.GetCheckInfo(boardOpt, color, pos);
                            Vector2Int lastPos;
                            if (tracePos.y - pos.y > 0) {
                                lastPos = new Vector2Int(pos.x, boardOpt.GetLength(0) - 1);
                            } else {
                                lastPos = new Vector2Int(pos.x, 0);
                            }
                            checkInfos.AddRange(Check.GetCheckInfo(boardOpt, color, tracePos));
                            checkInfos.AddRange(Check.GetCheckInfo(boardOpt, color, fragileCell));
                            if(Check.IsCheck(checkInfos)) {
                                continue;
                            }
                            var doubleMove = DoubleMove.MkDoubleMove(
                                MoveData.Mk(pos, new Vector2Int(pos.x, tracePos.y)),
                                MoveData.Mk(lastPos, fragileCell)
                            );
                            moveInfo = MoveInfo.Mk(doubleMove);
                        }
                    }
                    if (boardOpt[cell.x, cell.y].IsSome()) {
                        moveInfo.sentenced = cell;
                    }
                    if (board.traceBoard[cell.x, cell.y].IsSome()){
                        moveInfo.sentenced = new Vector2Int(pos.x, cell.y);
                    }
                    moveInfos.Add(moveInfo);
                }
            }

            if (targetPiece.type == PieceType.Pawn) {
                foreach (var info in new List<MoveInfo>(moveInfos)) {
                    var moveTo = info.doubleMove.first.to;
                    if (moveTo.x == 0 || moveTo.x == board.board.GetLength(1) - 1) {
                        var promotion = info;
                        promotion.pawnPromotion = true;
                        moveInfos.Remove(info);
                        moveInfos.Add(promotion);
                    }
                }
            }

            return moveInfos;
        }
    }
}