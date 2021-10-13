using System;
using System.Collections.Generic;
using UnityEngine;
using rules;
using board;
using option;

namespace movement {
    public enum MovementErrors {
        None,
        PieceIsNone,
        BoardIsNull,
        CantGetLinearLength
    }
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
        public static (List<PieceMovement>, MovementErrors) GetPieceMovements(
            Option<Piece>[,] board,
            PieceType pieceType,
            Vector2Int pos
        ) {
            if (board == null) {
                return (null, MovementErrors.BoardIsNull);
            }
            var pieceOpt = board[pos.x, pos.y];
            if (pieceOpt.IsNone()) {
                return (null, MovementErrors.PieceIsNone);
            }
            var piece = pieceOpt.Peel();
            var attack = MovementType.Attack;
            var move = MovementType.Move;
            var movements = new List<PieceMovement>();
            int maxLength = Mathf.Max(board.GetLength(1), board.GetLength(0));
            switch (pieceType) {
                case PieceType.Pawn:
                    int dirX = 1;
                    if (piece.color == PieceColor.White) {
                        dirX = -1;
                    }
                    for (int i = -1; i <= 1; i++) {
                        var dir = new Vector2Int(dirX, i);
                        if (i == 0 && piece.moveCounter == 0) {
                            var movement = PieceMovement.Linear(dir, 2, pos, move);
                            movement.traceIndex = Option<int>.Some(1);
                            movements.Add(movement);
                        } else if (i == 0) {
                            movements.Add(PieceMovement.Linear(dir, 1, pos, move));
                        } else {
                            movements.Add(PieceMovement.Linear(dir, 1, pos, attack));
                        }
                    }
                    break;
                case PieceType.Bishop:
                    movements.AddRange(GetMovements(maxLength, pos, (i, j) => i == 0 || j == 0));
                    break;
                case PieceType.Rook:
                    movements.AddRange(GetMovements(maxLength, pos, (i, j) => i != 0 && j != 0));
                    break;
                case PieceType.Queen:
                    movements.AddRange(GetMovements(maxLength, pos, (i, j) => false));
                    break;
                case PieceType.Knight:
                    movements.Add(PieceMovement.Circular(2f, pos, attack));
                    movements.Add(PieceMovement.Circular(2f, pos, move));
                    break;
                case PieceType.King:
                    movements.Add(PieceMovement.Circular(1f, pos, attack));
                    movements.Add(PieceMovement.Circular(1f, pos, move));
                    if (piece.moveCounter == 0) {
                        var (movement, err) = GetFragileMovement(board, pos, Direction.right);
                        if (movement.HasValue) {
                            movements.Add(movement.Value);
                        }
                        (movement, err) = GetFragileMovement(board, pos, Direction.left);
                        if (movement.HasValue) {
                            movements.Add(movement.Value);
                        }
                    }
                    break;
            }
            return (movements, MovementErrors.None);
        }

        public static (PieceMovement?, MovementErrors) GetFragileMovement(
            Option<Piece>[,] board,
            Vector2Int pos,
            Vector2Int dir
        ) {
            if (board == null) {
                return (null, MovementErrors.BoardIsNull);
            }
            int maxLength = Mathf.Max(board.GetLength(1), board.GetLength(0));
            var linear = Linear.Mk(dir, maxLength);
            var (length, err) = Board.GetLinearLength(pos, linear, board);
            if (err != BoardErrors.None) {
                return (null, MovementErrors.CantGetLinearLength);
            }
            var cell = pos + linear.dir * length;
            if (board[cell.x, cell.y].IsSome()) {
                var lastPiece = board[cell.x, cell.y].Peel();
                if (lastPiece.moveCounter == 0 && lastPiece.type == PieceType.Rook) {
                    var movement = PieceMovement.Linear(dir, 2, pos, MovementType.Move);
                    movement.isFragile = true;
                    movement.traceIndex = Option<int>.Some(2);
                    return (movement, MovementErrors.None);
                }
            }
            return (null, MovementErrors.None);
        }

        public static List<PieceMovement> GetMovements(
            int maxLength,
            Vector2Int pos,
            Func<int, int, bool> comparator
        ) {
            var movements = new List<PieceMovement>();
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
            return movements;
        }
    }
}