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
        public static List<Vector2Int> GetLinearMoves(
            Option<Piece>[,] board,
            Vector2Int piecePos,
            Linear linear,
            int maxLength
        ) {
            int length = Board.GetLinearLength<Piece>(piecePos, linear, board, maxLength);

            return GetOppositeColorOnLine(board, piecePos, linear, length);
        }

        public static List<Vector2Int> GetCirclularMoves(
            Option<Piece>[,] board,
            Vector2Int pos,
            Circular circlular
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

        private static List<Vector2Int> GetOppositeColorOnLine(
            Option<Piece>[,] board,
            Vector2Int piecePos,
            Linear linear,
            int length
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            if (board[piecePos.x, piecePos.y].IsNone()) {
                return canMovePositions;
            }
            Piece piece = board[piecePos.x, piecePos.y].Peel();
            for (int i = 1; i <= length; i++) {
                Vector2Int pos = piecePos + linear.dir * i;
                if (board[pos.x, pos.y].IsSome()) {
                    if (board[pos.x, pos.y].Peel().color == piece.color) {
                        break;
                    } else {
                        canMovePositions.Add(new Vector2Int(pos.x, pos.y));
                        break;
                    }
                }
                canMovePositions.Add(new Vector2Int(pos.x, pos.y));
            }

            return canMovePositions;
        }
    }
}

