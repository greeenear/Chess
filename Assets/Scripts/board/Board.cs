using System.Collections.Generic;
using UnityEngine;

namespace board {
    public struct Circle {
        public float radius;
        public float startingAngle;

        public static Circle mk(float radius, float startingAngle) {
            return new Circle { radius = radius, startingAngle = startingAngle };
        }
    }

    public struct Linear {
        public Vector2Int dir;
        public int length;

        public static Linear mk(Vector2Int dir, int length) {
            return new Linear {dir = dir, length = length};
        }
    }

    public static class Board {
        public static bool OnChessBoard(int i, int j) {
            if (i > 7 || i < 0 || j > 7 || j < 0) {

                return false;
            }

            return true;
        }

        public static List<Vector2Int> CalcAllLinearMove(
            Vector2Int piecePosition,
            Linear Linear
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            for (int i = 1; i <= Linear.length; i++) {
                int x = piecePosition.x + Linear.dir.x * i;
                int y = piecePosition.y + Linear.dir.y * i;

                if (Board.OnChessBoard(x, y)) {
                    canMovePositions.Add(new Vector2Int(x, y));
                }
            }

            return canMovePositions;
        }

        public static List<Vector2Int> CalcAllCircleMove(
            Vector2Int pos,
            Circle circleMove
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            for (int i = 1; i < 16; i += 2) {
                var angle = circleMove.startingAngle * i * Mathf.PI / 180;
                int x = (int)(Mathf.Sin(angle) * circleMove.radius + 0.51f + pos.x);
                int y = (int)(Mathf.Cos(angle) * circleMove.radius + 0.51f + pos.y);

                if (x < 0) {
                    x -= 1;
                }
                if (y < 0) {
                    y -= 1;
                }

                if (OnChessBoard(x, y)) {
                    canMovePositions.Add(new Vector2Int(x, y));
                }
            }

            return canMovePositions;
        }
    }
}
