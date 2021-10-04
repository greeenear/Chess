using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;
using movement;

namespace move {
    public static class Move {
        public static void MovePiece(Vector2Int start, Vector2Int end, CellInfo[,] board) {
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y].piece = Option<Piece>.None();
            var piece = board[end.x, end.y].piece.Peel();
            piece.moveCounter++;
            board[end.x, end.y].piece = Option<Piece>.Some(piece);
        }

        public static List<MoveInfo> GetMoveInfos(
            List<PieceMovement> pieceMovements,
            Vector2Int pos,
            CellInfo[,] board
        ) {
            if (board[pos.x, pos.y].piece.IsNone()) {
                return null;
            }

            var moveInfos = new List<MoveInfo>();
            var boardOpt = Rules.GetOptBoard(board);
            foreach (var pieceMovement in pieceMovements) {
                var possibleMoveCells = Rules.GetMoves(board, pieceMovement, pos);
                foreach (var cell in possibleMoveCells) {
                    var moveInfo = new MoveInfo {
                        doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, cell))
                    };
                    if (pieceMovement.traceIndex != 0) {
                        var dir = pieceMovement.movement.movement.linear.Value.dir;
                        var startPos = pieceMovement.movement.startPos;
                        var tracePos = startPos + dir * pieceMovement.traceIndex;
                        if (tracePos != cell) {
                            var pieceType = board[pos.x, pos.y].piece.Peel().type;
                            moveInfo.trace = new PieceTrace { pos = tracePos, whoLeft = pieceType};
                        }
                    }
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
            if (targetPiece.type == PieceType.King) {
                var rightCell = GetLastCellOnLine(board, Linear.Mk(new Vector2Int(0, 1), -1), pos);
                var leftCell = GetLastCellOnLine(board, Linear.Mk(new Vector2Int(0, -1), -1), pos);

                if (boardOpt[rightCell.x, rightCell.y].IsSome()) {
                    var piece = boardOpt[rightCell.x, rightCell.y].Peel();
                    if (piece.type == PieceType.Rook && piece.moveCounter == 0) {
                        var doubleMove = DoubleMove.MkDoubleMove(
                            MoveData.Mk(pos, new Vector2Int(pos.x, pos.y + 2)),
                            MoveData.Mk(rightCell, new Vector2Int(pos.x, pos.y + 1))
                        );
                        moveInfos.Add(MoveInfo.Mk(doubleMove));
                    }
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

        public static Vector2Int GetLastCellOnLine(
            CellInfo[,] board,
            Linear linear,
            Vector2Int startPos
        ) {
            var boardOpt = Rules.GetOptBoard(board);
            int length = Board.GetLinearLength(startPos, linear, boardOpt);
            var lastPos = startPos + linear.dir * length;
            return lastPos;
        }
    }
}