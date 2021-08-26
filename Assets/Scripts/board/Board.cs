using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public struct Circular {
        public float radius;

        public static Circular Mk(float radius) {
            return new Circular { radius = radius };
        }
    }

    public struct Linear {
        public Vector2Int dir;

        public static Linear Mk(Vector2Int dir) {
            return new Linear { dir = dir };
        }
    }

    public struct Movment {
        public Linear? linear;
        public Circular? circular;

        public static Movment Mk(Linear? linear, Circular? circular) {
            return new Movment { linear = linear, circular = circular };
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
            Option<T>[,] board
        ) {
            int length = 0;

            for (int i = 1; i <= board.GetLength(0); i++) {
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

        public static List<Vector2Int> GetAllCircularMoves(
            Vector2Int center,
            Circular circular,
            float startAngle
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            float angle = 0;

            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = startAngle * i * Mathf.PI / 180;
                var pos = new UnityEngine.Vector2(
                    Mathf.Sin(angle) * circular.radius + 0.51f + center.x,
                    Mathf.Cos(angle) * circular.radius + 0.51f + center.y
                );

                if (pos.x < 0) {
                    pos.x -= 1;
                }

                if (pos.y < 0) {
                    pos.y -= 1;
                }
                canMovePositions.Add(new Vector2Int((int)pos.x, (int)pos.y));
            }

            return canMovePositions;
        }

        public static Option<T>[,] СleanBoard<T>() {
            return new Option<T>[8,8];
        }

        public static Dictionary<Vector2Int, T> FindAllPieces<T>(Option<T>[,] board) {
            Dictionary<Vector2Int, T> pieces = new Dictionary<Vector2Int, T>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].IsSome()) {
                        pieces.Add(new Vector2Int(i, j), board[i, j].Peel());
                    }
                }
            }

            return pieces;
        }
    }
}

