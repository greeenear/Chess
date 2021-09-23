using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public enum MovementType {
        Attack,
        Move,
        Mixed
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

        public static Movement Linear(Linear linear) {
            return new Movement { linear = linear, movementType = MovementType.Mixed };
        }

        public static Movement LinearOnlyMove(Linear linear) {
            return new Movement { linear = linear, movementType = MovementType.Move };
        }

        public static Movement LinearOnlyAttack(Linear linear) {
            return new Movement { linear = linear, movementType = MovementType.Attack };
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
            int maxLength
        ) {
            int length = 0;
            if (maxLength < 0) {
                maxLength = board.GetLength(1);
            }

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

        public static List<Vector2Int> GetAllCircularMoves<T>(
            Vector2Int center,
            Circular circular,
            float startAngle,
            Option<T>[,] board
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            float angle = 0;
            var offset = new Vector2(0.5f + center.x, 0.5f + center.y);

            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = startAngle * i * Mathf.PI / 180;
                var pos = new Vector2(
                    Mathf.Sin(angle) * circular.radius,
                    Mathf.Cos(angle) * circular.radius
                );
                pos = pos + offset;

                if (pos.x < 0) {
                    pos.x--;
                }
                if (pos.y < 0) {
                    pos.y--;
                }
                var movePos = new Vector2Int((int)pos.x, (int)pos.y);
                if (Board.OnBoard(movePos, boardSize)) {
                    canMovePositions.Add(movePos);
                }
            }

            return canMovePositions;
        }
    }
}