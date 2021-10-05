using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;
using movement;
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
            if (targetPiece.type == PieceType.King) {
                var rightCell = GetLastCellOnLine(boardOpt, Linear.Mk(new Vector2Int(0, 1), -1), pos);

                if (boardOpt[rightCell.x, rightCell.y].IsSome()) {
                    var piece = boardOpt[rightCell.x, rightCell.y].Peel();
                    if (piece.type == PieceType.Rook && piece.moveCounter == 0) {
                        var doubleMove = DoubleMove.MkDoubleMove(
                            MoveData.Mk(pos, new Vector2Int(pos.x, pos.y + 2)),
                            MoveData.Mk(rightCell, new Vector2Int(pos.x, pos.y + 1))
                        );
                        var checkInfos = Check.GetCheckInfo(boardOpt, color, doubleMove.first.to);
                        var secondMove = doubleMove.second.Value.to;
                        checkInfos.AddRange(Check.GetCheckInfo(boardOpt, color, secondMove));
                        if (!Check.IsCheck(checkInfos)) {
                            moveInfos.Add(MoveInfo.Mk(doubleMove));
                        }
                    }
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

        public static Vector2Int GetLastCellOnLine(
            Option<Piece>[,] board,
            Linear linear,
            Vector2Int startPos
        ) {
            int length = Board.GetLinearLength(startPos, linear, board);
            var lastPos = startPos + linear.dir * length;
            return lastPos;
        }
    }
}