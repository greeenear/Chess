using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace math {
    public static class Math {
        public struct Segment {
            public Vector2Int start;
            public Vector2Int end;
            
            public static Segment Mk(Vector2Int start, Vector2Int end) {
                return new Segment { start = start, end = end };
            }
        }

        public struct Coefficients {
            public int a;
            public int b;
            public int c;
            public static Coefficients Mk(int a, int b, int c) {
                return new Coefficients { a = a, b = b, c = c };
            }
        }

        public static Vector2Int? GetNormalVector(Segment segment) {
            if (segment.start == segment.end) {
                return null;
            }
            var normal = segment.end - segment.start;
            return new Vector2Int(normal.y, -normal.x);
        }

        public static Vector3Int GetLineCoefficients(Vector2Int normal, Vector2Int point) {
            var c = -normal.x * point.x - normal.y * point.y;
            return new Vector3Int(normal.x, normal.y, c);
        }

        public static Vector2Int? GetSegmentsIntersection(
            Vector3Int l1,
            Vector3Int l2
        ) {
            if (l2.x * l1.y - l1.x * l2.y == 0) {
                return null;
            }
            var y = (l1.x * l2.z - l2.x * l1.z) / (l2.x * l1.y - l1.x * l2.y);
            var x = 0;
            if (l1.x == 0) {
                x = -l1.z / l1.y;
            } else {
                x = (-l1.y * y - l1.z) / l1.x;
            }
            return new Vector2Int(x, y);
        }

        public static bool IsPointOnSegment(Segment segment, Vector2Int point) {
            var x = point.x;
            var y = point.y;

            var start = segment.start;
            var end = segment.end;
            double a = end.y - start.y;
            double b = start.x - end.x;
            double c = - a * start.x - b * start.y;
            if (System.Math.Abs(a * point.x + b * point.y + c) > 0) {
                return false;
            }
            if ((x >= start.x && x <= end.x || x <= start.x && x >= end.x)
                && (y >= start.y && y <= end.y || y <= start.y && y >= end.y)) {
                return true;
            } else {
                return false;
            }
        }
    }
}
