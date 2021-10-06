using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace math {
    public static class Math {
        public static Vector2Int? GetNormalVector(Vector2Int firstPoint, Vector2Int secondPoint) {
            if (firstPoint == secondPoint) {
                return null;
            }
            var a = secondPoint.y - firstPoint.y;
            var b = secondPoint.x - firstPoint.x;
            return new Vector2Int(a, b);
        }

        public static Vector2Int? GetSegmentsIntersection(
            Vector2Int n1,
            Vector2Int n2,
            Vector2Int p1,
            Vector2Int p2
        ) {
            if (n2.x * n1.y - n1.x * n2.y == 0) {
                return null;
            }
            var y = (n1.x * n2.x * p2.x - n1.x * n2.y * p2.y -
                n2.x * n1.x * p1.x + n2.x * n1.y * p1.y) / (n2.x * n1.y - n1.x * n2.y);
            var x = (n1.x * p1.x + n1.y * y - n1.y * p1.y) / n1.x;
            return new Vector2Int(x, y);
        }

        public static bool IsPointOnSegment(Vector2Int start, Vector2Int end, Vector2Int point) {
            var x = point.x;
            var y = point.y;

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
