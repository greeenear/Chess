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

    public struct PieceMovement {
        public Movement movement;
        public PieceTrace? trace;

        public static PieceMovement Mk(Movement movement) {
            return new PieceMovement { movement = movement };
        }
    }

    public struct CellInfo {
        public Option<Piece> piece;
        public PieceTrace? trace;
    }

    public struct PieceTrace {
        public Vector2Int? tracePos;
        public bool isCanTake;
        public bool isCheckUnderAttack;

    }

    public struct StartAngle {
        public const float Knight = 22.5f;
        public const float King = 20f;
    }

    public static class Rules {
        public static List<Vector2Int> GetMoves(
            CellInfo[,] board,
            FixedMovement movement,
            PieceTrace? trace
        ) {
            if (movement.movement.linear.HasValue) {
                var linear = movement.movement.linear.Value;
                var startPos = movement.startPos;
                var boardOpt = GetOptBoard(board);
                int length = Board.GetLinearLength<Piece>(startPos, linear, boardOpt, linear.length);
                length = GetFixedLength(boardOpt, movement, length, trace);
                return GetLinearMoves(linear, movement.startPos, length);
            } else if (movement.movement.circular.HasValue) {
                var circular = movement.movement.circular.Value;
                return GetCirclularMoves(board, circular, movement.startPos);
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
            Option<Piece>[,] board,
            FixedMovement linearMovement,
            int maxLength,
            PieceTrace? trace
        ) {
            var targetPieceOpt = board[linearMovement.startPos.x, linearMovement.startPos.y];
            if (targetPieceOpt.IsNone()) {
                return 0;
            }
            Piece targetPiece = targetPieceOpt.Peel();

            var linear = linearMovement.movement.linear.Value;
            var movementType = linearMovement.movement.movementType;
            var lastPos = linearMovement.startPos + linear.dir * maxLength;

            var pieceOpt = board[lastPos.x, lastPos.y];
            if (movementType == MovementType.Move) {
                if (pieceOpt.IsSome()) {
                    return maxLength - 1;
                } else {
                    return maxLength;
                }
            } else if (movementType == MovementType.Attack) {
                // if (pieceOpt.IsNone() && trace.HasValue && trace.Value.pawnTrace.HasValue) {
                //     if (targetPiece.type == PieceType.Pawn && lastPos == trace.Value.pawnTrace) {
                //         return maxLength;
                //     }
                // }
                if (pieceOpt.IsSome() && pieceOpt.Peel().color != targetPiece.color) {
                    return maxLength;
                } else {
                    return maxLength - 1;
                }
            } else if (movementType == MovementType.AttackTrace) {

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