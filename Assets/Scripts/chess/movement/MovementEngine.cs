using System;
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
            int maxLength = Mathf.Max(board.GetLength(1), board.GetLength(0));
            switch (pieceType) {
                case PieceType.Pawn:
                    var moveMovement = new PieceMovement();
                    if (piece.color == PieceColor.White) {
                        movements.Add(PieceMovement.Linear(Direction.downRight, 1, pos, attack));
                        movements.Add(PieceMovement.Linear(Direction.downLeft, 1, pos, attack));
                        moveMovement = PieceMovement.Linear(Direction.down, 1, pos, move);
                    } else if (piece.color == PieceColor.Black) {
                        movements.Add(PieceMovement.Linear(Direction.upRight, 1, pos, attack));
                        movements.Add(PieceMovement.Linear(Direction.upLeft, 1, pos, attack));
                        moveMovement = PieceMovement.Linear(Direction.up, 1, pos, move);
                    }
                    if (piece.moveCounter == 0) {
                        var dir = moveMovement.movement.movement.linear.Value.dir;
                        moveMovement = PieceMovement.Linear(dir, 2, pos, move);
                        moveMovement.traceIndex = Option<int>.Some(1);
                        movements.Add(moveMovement);
                    } else {
                        movements.Add(moveMovement);
                    }
                    break;
                case PieceType.Bishop:
                    InsertMovements(movements, maxLength, pos, (i, j) => i == 0 || j == 0);
                    break;
                case PieceType.Rook:
                    InsertMovements(movements, maxLength, pos, (i, j) => i != 0 && j != 0);
                    break;
                case PieceType.Queen:
                    InsertMovements(movements, maxLength, pos, (i, j) => false);
                    break;
                case PieceType.Knight:
                    movements.Add(PieceMovement.Circular(2f, pos, attack));
                    movements.Add(PieceMovement.Circular(2f, pos, move));
                    break;
                case PieceType.King:
                    movements.Add(PieceMovement.Circular(1f, pos, attack));
                    movements.Add(PieceMovement.Circular(1f, pos, move));
                    if (piece.moveCounter == 0) {
                        var rightLinear = Linear.Mk(Direction.right, maxLength);
                        var rightCell = Rules.GetLastCellOnLine(board, rightLinear, pos);
                        var LeftLinear = Linear.Mk(Direction.left, maxLength);
                        var leftCell = Rules.GetLastCellOnLine(board, LeftLinear, pos);
                        if (board[rightCell.x, rightCell.y].IsSome()) {
                            var lastPiece = board[rightCell.x, rightCell.y].Peel();
                            if (lastPiece.moveCounter == 0 && lastPiece.type == PieceType.Rook) {
                                var movement = PieceMovement.Linear(Direction.right, 2, pos, move);
                                movement.isFragile = true;
                                movement.traceIndex = Option<int>.Some(2);
                                movements.Add(movement);
                            }
                        }
                        if (board[leftCell.x, leftCell.y].IsSome()) {
                            var lastPiece = board[leftCell.x, leftCell.y].Peel();
                            if (lastPiece.moveCounter == 0 && lastPiece.type == PieceType.Rook) {
                                var movement = PieceMovement.Linear(Direction.left, 2, pos, move);
                                movement.isFragile = true;
                                movement.traceIndex = Option<int>.Some(2);
                                movements.Add(movement);
                            }
                        }
                    }
                    break;
            }
            return movements;
        }

        public static void InsertMovements(
            List<PieceMovement> movements,
            int maxLength,
            Vector2Int pos,
            Func<int, int, bool> comparator
        ) {
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i == 0 && j == 0 || comparator(i, j)) {
                        continue;
                    }
                    var dir = new Vector2Int(i,j);
                    movements.Add(PieceMovement.Linear(dir, maxLength, pos, MovementType.Attack));
                    movements.Add(PieceMovement.Linear(dir, maxLength, pos, MovementType.Move));
                }
            }
        }
    }
}