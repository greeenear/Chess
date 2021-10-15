using UnityEngine;
using option;
using System;

namespace board {
    public enum BoardErrors {
        None,
        BoardIsNull
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
        public Linear? linear;
        public Circular? circular;

        public static Movement Linear(Linear linear) {
            return new Movement { linear = linear, };
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

        public static (int, BoardErrors) GetLinearLength<T>(
            Vector2Int startPosition,
            Linear linear,
            Option<T>[,] board
        ) {
            if (board == null) {
                return (0, BoardErrors.BoardIsNull);
            }
            int length = 0;
            for (int i = 1; i <= linear.length; i++) {
                Vector2Int pos = startPosition + linear.dir * i;
                if (!Board.OnBoard(pos,new Vector2Int(board.GetLength(0), board.GetLength(1)))) {
                    break;
                }
                length++;
                if (board[pos.x, pos.y].IsSome()) {
                    break;
                }
            }
            return (length, BoardErrors.None);
        }

        public static (Vector2Int?, BoardErrors) GetCircularPoint<T>(
            Vector2Int center,
            Circular circular,
            float angle,
            Option<T>[,] board
        ) {
            if (board == null) {
                return (null, BoardErrors.BoardIsNull);
            }
            Vector2Int movePos = new Vector2Int();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var offset = new Vector2(0.5f + center.x, 0.5f + center.y);
            var radius = circular.radius;
            var pos = new Vector2(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
            pos = pos + offset;
            movePos = new Vector2Int((int)Math.Floor(pos.x), (int)Math.Floor(pos.y));
            if (Board.OnBoard(movePos, boardSize)) {
                return (movePos, BoardErrors.None);
            }
            return (null, BoardErrors.None);
        }
    }
}