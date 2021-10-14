using UnityEngine;
using System.Collections.Generic;
using option;
using board;

namespace rules {
    public enum RulesErrors {
        None,
        BoardIsNull,
        ImpossibleMovement,
        PieceIsNone,
        CantGetLinearLength

    }

    public enum PieceType {
        Bishop,
        King,
        Knight,
        Pawn,
        Queen,
        Rook
    }

    public enum PieceColor {
        Black,
        White,
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
        public static (List<Vector2Int>, RulesErrors) GetMoves(
            FullBoard board,
            PieceMovement pieceMovement,
            Vector2Int startPos
        ) {
            if (pieceMovement.movement.movement.linear.HasValue) {
                var linear = pieceMovement.movement.movement.linear.Value;
                var movementType = pieceMovement.movementType;
                var fixedLength = GetFixedLength(board, linear, startPos, movementType);
                return (GetLinearMoves(linear, startPos, fixedLength.Item1), RulesErrors.None);
            } else if (pieceMovement.movement.movement.circular.HasValue) {
                var circular = pieceMovement.movement.movement.circular.Value;
                var (circularMoves, err) = GetCirclularMoves(board.board, circular, startPos);
                if (err != RulesErrors.None) {
                    return (null, err);
                }
                return (circularMoves, RulesErrors.None);
            }

            return (null, RulesErrors.ImpossibleMovement);
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

        public static (List<Vector2Int>, RulesErrors) GetCirclularMoves(
            Option<Piece>[,] board,
            Circular circlular,
            Vector2Int pos
        ) {
            if (board == null) {
                return (null, RulesErrors.BoardIsNull);
            }
            if (board[pos.x, pos.y].IsNone()) {
                return (null, RulesErrors.PieceIsNone);
            }
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
                if (cell.Item2 != BoardErrors.None) {
                    Debug.Log(cell.Item2);
                }
                if (!cell.Item1.HasValue) {
                    continue;
                }
                var cellOpt = board[cell.Item1.Value.x, cell.Item1.Value.y];
                if (cellOpt.IsSome() && cellOpt.Peel().color == board[pos.x, pos.y].Peel().color) {
                    continue;
                }
                canMovePositions.Add(cell.Item1.Value);
            }
            return (canMovePositions, RulesErrors.None);
        }

        private static (int, RulesErrors) GetFixedLength(
            FullBoard board,
            Linear linearMovement,
            Vector2Int startPos,
            MovementType movementType
        ) {
            var (maxLength, err) = Board.GetLinearLength(startPos, linearMovement, board.board);
            if (err != BoardErrors.None) {
                return (0, RulesErrors.CantGetLinearLength);
            }
            if (board.board == null) {
                return (0, RulesErrors.BoardIsNull);
            }
            var targetPieceOpt = board.board[startPos.x, startPos.y];
            if (targetPieceOpt.IsNone()) {
                return (0, RulesErrors.PieceIsNone);
            }
            var targetPiece = targetPieceOpt.Peel();
            var lastPos = startPos + linearMovement.dir * maxLength;
            var pieceOpt = board.board[lastPos.x, lastPos.y];
            if (movementType == MovementType.Move) {
                if (pieceOpt.IsSome()) {
                    return (maxLength - 1, RulesErrors.None);
                } else {
                    return (maxLength, RulesErrors.None);
                }
            } else if (movementType == MovementType.Attack) {
                if (pieceOpt.IsSome() && pieceOpt.Peel().color != targetPiece.color) {
                    return (maxLength, RulesErrors.None);
                } else if (pieceOpt.IsNone() && board.traceBoard[lastPos.x, lastPos.y].IsSome()) {
                    var lastPiece = board.traceBoard[lastPos.x, lastPos.y].Peel();
                    if (lastPiece.whoLeft == targetPiece.type) {
                        return (maxLength, RulesErrors.None);
                    }
                }
            }
            return (maxLength - 1, RulesErrors.None);
        }
    }
}