using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public enum MovementType {
        Attack,
        Move,
    }

    public struct Circular {
        public float radius;

        public static Circular Mk(float radius) {
            return new Circular { radius = radius };
        }
    }

    public struct Linear {
        public Vector2Int dir;
        public int length;

        public static Linear Mk(Vector2Int dir, int length ) {
            return new Linear { dir = dir, length = length };
        }
    }

    public struct Movement {
        public MovementType movementType;
        public Linear? linear;
        public Circular? circular;

        public static Movement Linear(Linear linear, MovementType type) {
            return new Movement { linear = linear, movementType = type };
        }

        public static Movement Circular(Circular circular) {
            return new Movement { circular = circular };
        }
    }

    public struct FixedMovement {
        public Movement movement;
        public Vector2Int startPos;

        public static FixedMovement Mk(Movement movement, Vector2Int startPos) {
            return new FixedMovement { movement = movement, startPos = startPos };
        }
    }

    public static class Board {
        public static bool OnBoard(Vector2Int pos, Vector2Int boardSize) {
            if (pos.x < 0 || pos.x > boardSize.x - 1 || pos.y < 0 || pos.y > boardSize.y - 1) {
                return false;
            }

            return true;
        }

        public static int GetLinearLength<T>(
            Vector2Int startPosition,
            Linear linear,
            Option<T>[,] board,
            int linearLength
        ) {
            var maxLength = GetMaxLength(board, linearLength);

            int length = 0;
            for (int i = 1; i <= maxLength; i++) {
                Vector2Int pos = startPosition + linear.dir * i;

                if (!Board.OnBoard(pos,new Vector2Int(board.GetLength(0), board.GetLength(1)))) {
                    break;
                }
                if (board[pos.x, pos.y].IsSome()) {
                    length++;
                    break;
                }
                if (board[pos.x, pos.y].IsNone()) {
                    length++;
                }
            }

            return length;
        }

        public static Vector2Int? GetCircularMove<T>(
            Vector2Int center,
            Circular circular,
            float startAngle,
            Option<T>[,] board
        ) {
            Vector2Int movePos = new Vector2Int();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var offset = new Vector2(0.5f + center.x, 0.5f + center.y);

            var pos = new Vector2(
                Mathf.Sin(startAngle) * circular.radius,
                Mathf.Cos(startAngle) * circular.radius
            );
            pos = pos + offset;

            if (pos.x < 0) {
                pos.x--;
            }
            if (pos.y < 0) {
                pos.y--;
            }
            movePos = new Vector2Int((int)pos.x, (int)pos.y);
            if (Board.OnBoard(movePos, boardSize)) {
                return movePos;
            }

            return null;
        }

        public static int GetMaxLength<T>(Option<T>[,] board, int length) {
            int maxBoardSize = Mathf.Max(board.GetLength(1), board.GetLength(0));
            if (length < 0) {
                length = maxBoardSize;
            } else if (length > maxBoardSize) {
                length = maxBoardSize;
            }

            return length;
        }
    }
}