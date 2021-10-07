using UnityEngine;
using System.Collections.Generic;
using option;
using board;

namespace rules {
    public enum PieceType {
        Bishop,
        King,
        Knight,
        Pawn,
        Queen,
        Rook
    }

    public enum PieceColor {
        White,
        Black,
        Count
    }

    public enum MovementType {
        Attack,
        Move
    }

    public struct Piece {
        public PieceType type;
        public PieceColor color;
        public int moveCounter;

        public static Piece Mk(PieceType type, PieceColor color, int moveCounter) {
            return new Piece { type = type, color = color, moveCounter = moveCounter };
        }
    }

    public struct PieceMovement {
        public FixedMovement movement;
        public MovementType movementType;
        public Option<int> traceIndex;
        public bool isFragile;

        public static PieceMovement Mk(FixedMovement movement, MovementType movementType) {
            return new PieceMovement { movement = movement, movementType = movementType };
        }

        public static PieceMovement Linear(
            Vector2Int dir,
            int length,
            Vector2Int pos,
            MovementType type
        ) {
            var linear = board.Linear.Mk(dir, length);
            var fixedMovement = FixedMovement.Mk(Movement.Linear(linear), pos);
            return new PieceMovement { movement = fixedMovement, movementType = type };
        }

        public static PieceMovement Circular(float radius, Vector2Int pos, MovementType type) {
            var circular = board.Circular.Mk(radius);
            var fixedMovement = FixedMovement.Mk(Movement.Circular(circular), pos);
            return new PieceMovement { movement = fixedMovement, movementType = type };
        }

    }

    public struct FullBoard {
        public Option<Piece>[,] board;
        public Option<PieceTrace>[,] traceBoard;
    }

    public struct PieceTrace {
        public Vector2Int pos;
        public PieceType whoLeft;
    }

    public struct StartAngle {
        public const float Knight = 22.5f;
        public const float King = 20f;
    }

    public static class Rules {
        public static List<Vector2Int> GetMoves(
            FullBoard board,
            PieceMovement pieceMovement,
            Vector2Int startPos
        ) {
            if (pieceMovement.movement.movement.linear.HasValue) {
                var linear = pieceMovement.movement.movement.linear.Value;

                int length = Board.GetLinearLength(startPos, linear, board.board);
                var movementType = pieceMovement.movementType;
                length = GetFixedLength(board, linear, length, startPos, movementType);
                return GetLinearMoves(linear, startPos, length);
            } else if (pieceMovement.movement.movement.circular.HasValue) {
                var circular = pieceMovement.movement.movement.circular.Value;
                return GetCirclularMoves(board.board, circular, startPos);
            }

            return null;
        }

        public static List<Vector2Int> GetLinearMoves(
            Linear linear,
            Vector2Int piecePos,
            int length
        ) {
            var moves = new List<Vector2Int>();
            for (int i = 1; i <= length; i++) {
                moves.Add(piecePos + linear.dir * i);
            }

            return moves;
        }

        public static List<Vector2Int> GetCirclularMoves(
            Option<Piece>[,] board,
            Circular circlular,
            Vector2Int pos
        ) {
            float startAngle;
            if (circlular.radius == 1) {
                startAngle = StartAngle.King;
            } else {
                startAngle = StartAngle.Knight;
            }
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            Vector2Int boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            float angle = 0;
            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = startAngle * i * Mathf.PI / 180;
                var cell = Board.GetCircularPoint(pos, circlular, angle, board);
                if (!cell.HasValue) {
                    continue;
                }
                var cellOpt = board[cell.Value.x, cell.Value.y];
                if (cellOpt.IsNone()) {
                    canMovePositions.Add(cell.Value);
                } else if (cellOpt.Peel().color != board[pos.x, pos.y].Peel().color) {
                    canMovePositions.Add(cell.Value);
                }
            }
            return canMovePositions;
        }

        private static int GetFixedLength(
            FullBoard board,
            Linear linearMovement,
            int maxLength,
            Vector2Int startPos,
            MovementType movementType
        ) {
            var targetPieceOpt = board.board[startPos.x, startPos.y];
            if (targetPieceOpt.IsNone()) {
                return 0;
            }
            var targetPiece = targetPieceOpt.Peel();
            var lastPos = startPos + linearMovement.dir * maxLength;
            var pieceOpt = board.board[lastPos.x, lastPos.y];
            if (movementType == MovementType.Move) {
                if (pieceOpt.IsSome()) {
                    return maxLength - 1;
                } else {
                    return maxLength;
                }
            } else if (movementType == MovementType.Attack) {
                if (pieceOpt.IsSome() && pieceOpt.Peel().color != targetPiece.color) {
                    return maxLength;
                } else if (pieceOpt.IsNone() && board.traceBoard[lastPos.x, lastPos.y].IsSome()) {
                    var lastPiece = board.traceBoard[lastPos.x, lastPos.y].Peel();
                    if (lastPiece.whoLeft == targetPiece.type) {
                        return maxLength;
                    }
                } else if (pieceOpt.IsNone()) {
                    return maxLength - 1;
                } else {
                    return maxLength - 1;
                }
            }
            return maxLength;
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