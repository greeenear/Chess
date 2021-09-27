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

    public struct Piece {
        public PieceType type;
        public PieceColor color;

        public int moveCounter;

        public static Piece Mk(PieceType type, PieceColor color, int moveCounter) {
            return new Piece { type = type, color = color, moveCounter = moveCounter };
        }
    }

    public struct StartAngle {
        public const float Knight = 22.5f;
        public const float King = 20f;
    }

    public static class Rules {
        public static List<Vector2Int> GetMoves(Option<Piece>[,] board, FixedMovement movement) {
            if (movement.movement.linear.HasValue) {
                var linear = movement.movement.linear.Value;
                var startPos = movement.startPos;
                int length = Board.GetLinearLength<Piece>(startPos, linear, board, linear.length);
                length = GetFixedLength(board, movement, length);
                return GetLinearMoves(linear, movement.startPos, length);
            } else if (movement.movement.circular.HasValue) {
                var circular = movement.movement.circular.Value;
                return GetCirclularMoves(board, circular, movement.startPos);
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
            float angle;
            if (circlular.radius == 1) {
                angle = StartAngle.King;
            } else {
                angle = StartAngle.Knight;
            }
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            List<Vector2Int> allCanMovePositions = new List<Vector2Int>();
            Vector2Int boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));

            allCanMovePositions = Board.GetAllCircularMoves<Piece>(pos, circlular, angle, board);
            foreach (var movePos in allCanMovePositions) {
                var piece = board[movePos.x, movePos.y];
                if (piece.IsNone()) {
                    canMovePositions.Add(movePos);
                }
                if (piece.IsSome() && piece.Peel().color != board[pos.x, pos.y].Peel().color) {
                    canMovePositions.Add(movePos);
                }
            }

            return canMovePositions;
        }

        private static int GetFixedLength(
            Option<Piece>[,] board,
            FixedMovement linearMovement,
            int maxLength
        ) {
            var piecePos = linearMovement.startPos;
            if (board[piecePos.x, piecePos.y].IsNone()) {
                return 0;
            }
            Piece targetPiece = board[piecePos.x, piecePos.y].Peel();

            var linear = linearMovement.movement.linear.Value;
            var movementType = linearMovement.movement.movementType;
            int moveCounter = 0;

            for (int i = 1; i <= maxLength; i++) {
                Vector2Int pos = piecePos + linear.dir * i;
                var pieceOpt = board[pos.x, pos.y];
                if (movementType == MovementType.Move) {
                    if (pieceOpt.IsNone()) {
                        moveCounter++;
                    }
                } else if (movementType == MovementType.Attack) {
                    if (pieceOpt.IsSome()) {
                        var piece = pieceOpt.Peel();
                        if (piece.color != targetPiece.color) {
                            moveCounter = i;
                            return moveCounter;
                        } else {
                            moveCounter = i - 1;
                            return moveCounter - 1;
                        }
                    }
                }
            }

            return moveCounter;
        }
    }
}

