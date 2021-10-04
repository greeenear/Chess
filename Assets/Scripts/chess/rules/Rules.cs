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
        public int traceIndex;

        public static PieceMovement Mk(FixedMovement movement, MovementType movementType) {
            return new PieceMovement { movement = movement, movementType = movementType };
        }
    }

    public struct CellInfo {
        public Option<Piece> piece;
        public Option<PieceTrace> trace;
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
        public static List<Vector2Int> GetMoves(
            CellInfo[,] board,
            PieceMovement pieceMovement,
            Vector2Int startPos
        ) {
            if (pieceMovement.movement.movement.linear.HasValue) {
                var linear = pieceMovement.movement.movement.linear.Value;
                var boardOpt = GetOptBoard(board);

                int length = Board.GetLinearLength(startPos, linear, boardOpt, linear.length);
                var movementType = pieceMovement.movementType;
                length = GetFixedLength(board, linear, length, startPos, movementType);
                return GetLinearMoves(linear, startPos, length);
            } else if (pieceMovement.movement.movement.circular.HasValue) {
                var circular = pieceMovement.movement.movement.circular.Value;
                return GetCirclularMoves(board, circular, startPos);
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
            CellInfo[,] board,
            Circular circlular,
            Vector2Int pos
        ) {
            float startAngle;
            if (circlular.radius == 1) {
                startAngle = StartAngle.King;
            } else {
                startAngle = StartAngle.Knight;
            }
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            Vector2Int boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            float angle = 0;
            var boardOpt = GetOptBoard(board);
            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = startAngle * i * Mathf.PI / 180;
                var cell = Board.GetCircularMove<Piece>(pos, circlular, angle, boardOpt);
                if (!cell.HasValue) {
                    continue;
                }
                var cellOpt = board[cell.Value.x, cell.Value.y];
                if (cellOpt.piece.IsNone()) {
                    canMovePositions.Add(cell.Value);
                } else if (cellOpt.piece.Peel().color != board[pos.x, pos.y].piece.Peel().color) {
                    canMovePositions.Add(cell.Value);
                }
            }
            return canMovePositions;
        }

        private static int GetFixedLength(
            CellInfo[,] board,
            Linear linearMovement,
            int maxLength,
            Vector2Int startPos,
            MovementType movementType
        ) {
            var targetPieceOpt = board[startPos.x, startPos.y];
            if (targetPieceOpt.piece.IsNone()) {
                return 0;
            }
            var targetPiece = targetPieceOpt.piece.Peel();
            var lastPos = startPos + linearMovement.dir * maxLength;
            var pieceOpt = board[lastPos.x, lastPos.y];
            if (movementType == MovementType.Move) {
                if (pieceOpt.piece.IsSome()) {
                    return maxLength - 1;
                } else {
                    return maxLength;
                }
            } else if (movementType == MovementType.Attack) {
                if (pieceOpt.piece.IsSome() && pieceOpt.piece.Peel().color != targetPiece.color) {
                    return maxLength;
                } else if (pieceOpt.piece.IsNone() && board[lastPos.x, lastPos.y].trace.IsSome()) {
                    if (board[lastPos.x, lastPos.y].trace.Peel().whoLeft == targetPiece.type) {
                        return maxLength;
                    }
                } else if (pieceOpt.piece.IsNone()) {
                    return maxLength - 1;
                } else {
                    return maxLength - 1;
                }
            }
            return maxLength;
        }

        public static Option<Piece>[,] GetOptBoard(CellInfo[,] cellInfoBoard) {
            var boardSize = new Vector2Int(cellInfoBoard.GetLength(0), cellInfoBoard.GetLength(1));
            Option<Piece>[,] board = new Option<Piece>[boardSize.x,boardSize.y];
            for (int i = 0; i < boardSize.x; i++) {
                for (int j = 0; j < boardSize.y; j++) {
                    if (cellInfoBoard[i,j].piece.IsSome()) {
                        board[i,j] = Option<Piece>.Some(cellInfoBoard[i,j].piece.Peel());
                    }
                }
            }

            return board;
        }
    }
}