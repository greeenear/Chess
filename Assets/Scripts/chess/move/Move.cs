using System.Collections.Generic;
using UnityEngine;
using rules;
using option;
using check;

namespace move {
    public enum MoveErrors {
        None,
        BoardIsNull,
        PieceIsNone
    }
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
        public Trace? trace;

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

        public static (List<MoveInfo>, MoveErrors) GetMoveInfos(
            List<PieceMovement> pieceMovements,
            Vector2Int pos,
            FullBoard board
        ) {
            var boardOpt = board.board;
            if (boardOpt == null) {
                return (null, MoveErrors.BoardIsNull);
            }
            if (boardOpt[pos.x, pos.y].IsNone()) {
                return (null, MoveErrors.PieceIsNone);
            }
            var piece = boardOpt[pos.x, pos.y].Peel();
            var moveInfos = new List<MoveInfo>();
            var color = piece.color;
            foreach (var pieceMovement in pieceMovements) {
                var (possibleMoveCells, err) = Rules.GetMoves(board, pieceMovement, pos);
                if (err != RulesErrors.None) {
                    continue;
                }
                foreach (var cell in possibleMoveCells) {
                    var moveInfo = new MoveInfo {
                        doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, cell))
                    };
                    if (pieceMovement.traceIndex.IsSome()) {
                        var dir = pieceMovement.movement.movement.linear.Value.dir;
                        var tracePos = pos + dir * pieceMovement.traceIndex.Peel();
                        moveInfo.trace = new Trace { pos = tracePos, whoLeft = piece.type };
                        if (pieceMovement.isFragile) {
                            if(!CheckFragileMovement(pos, tracePos, boardOpt, piece.color)) {
                                continue;
                            }
                            var lastPos = new Vector2Int(pos.x, 0);
                            if (tracePos.y - pos.y > 0) {
                                lastPos = new Vector2Int(pos.x, boardOpt.GetLength(0) - 1);
                            }
                            var doubleMove = DoubleMove.MkDoubleMove(
                                MoveData.Mk(pos, new Vector2Int(pos.x, tracePos.y)),
                                MoveData.Mk(lastPos, (pos + tracePos) / 2)
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
            if (piece.type == PieceType.Pawn) {
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
            return (moveInfos, MoveErrors.None);
        }
        private static bool CheckFragileMovement(
            Vector2Int pos,
            Vector2Int tracePos,
            Option<Piece>[,] boardOpt,
            PieceColor color
        ) {
            var (check1, err1) = Check.IsCheck(boardOpt, pos, color);
            if (err1 != CheckErrors.None) {
                return false;
            }
            var (check2, err2) = Check.IsCheck(boardOpt, tracePos, color);
            var (check3, err3) = Check.IsCheck(boardOpt, (pos + tracePos) / 2, color);
            if(check1 || check2 || check3) {
                return false;
            } else {
                return true;
            }
        }
    }
}