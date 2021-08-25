using UnityEngine;
using System.Collections.Generic;
using option;
using board;

namespace chess {
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
        Black
    }

    public struct Piece {
        public PieceType type;
        public PieceColor color;

        public static Piece mk(PieceType type, PieceColor color) {
            return new Piece { type = type, color = color };
        }
    }

    public static class Chess {
        public static List<Vector2Int> CalcPossibleMoves(
            Vector2Int pos,
            Option<Piece>[,] board
        ) {
            List<Vector2Int> moves = new List<Vector2Int>();
            Piece piece = board[pos.x, pos.y].Peel();

            switch (piece.type) {
                case PieceType.Pawn:
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, 1), 1)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, -1), 1)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, -1), 1)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, 1), 1)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, 0), 2)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, 0), 2)));
                    break;
                case PieceType.Bishop:
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, 1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, -1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, -1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, 1), 8)));
                    break;
                case PieceType.Rook:
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, 0), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, 0), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(0, -1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(0, 1), 8)));
                    break;
                case PieceType.Queen:
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, 1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, -1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, -1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, 1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(1, 0), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(-1, 0), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(0, -1), 8)));
                    moves.AddRange(CalcLinear(board, pos, Linear.mk(new Vector2Int(0, 1), 8)));
                    break;
                case PieceType.King:
                    moves.AddRange(CalcCircle(board, pos, Circle.mk(1f, 20f)));
                    break;
                case PieceType.Knight:
                    moves.AddRange(CalcCircle(board, pos, Circle.mk(2f, 22.5f)));
                    break;
            }

            return moves;
        }

        public static List<Vector2Int> CalcLinear(
            Option<Piece>[,] board,
            Vector2Int pos,
            Linear linear
        ) {
            Piece piece = board[pos.x, pos.y].Peel();
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            List<Vector2Int> allCanMovePositions = new List<Vector2Int>();

            allCanMovePositions = Board.CalcAllLinearMove(pos, linear);
            foreach(var movePos in allCanMovePositions) {
                if (board[movePos.x, movePos.y].IsNone()) {
                    canMovePositions.Add(movePos);

                } else if (board[movePos.x, movePos.y].Peel().color == piece.color) {
                    break;

                } else if (board[movePos.x, movePos.y].Peel().color != piece.color) {
                    canMovePositions.Add(movePos);
                    break;
                }
            }

            return canMovePositions;
        }

        public static List<Vector2Int> CalcCircle(
            Option<Piece>[,] board,
            Vector2Int pos,
            Circle circleMove
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            List<Vector2Int> allCanMovePositions = new List<Vector2Int>();

            allCanMovePositions = Board.CalcAllCircleMove(pos,circleMove);
            foreach (var movePos in allCanMovePositions) {
                var piece = board[movePos.x, movePos.y];

                if (piece.IsNone()) {
                    canMovePositions.Add(movePos);
                }

                if (piece.IsSome() && piece.Peel().color != board[pos.x, pos.y].Peel().color) {
                    canMovePositions.Add(movePos);
                }
            }

            return canMovePositions;
        }

        public static Vector2Int? FindKing(Option<Piece>[,] board, PieceColor whoseMove) {
            Dictionary<Vector2Int, Piece> allPieces = new Dictionary<Vector2Int, Piece>();
            allPieces = Board.FindAllPieces(board);

            foreach (var piece in allPieces) {
                if (piece.Value.type == PieceType.King && piece.Value.color == whoseMove) {

                    return new Vector2Int(piece.Key.x, piece.Key.y);
                }
            }

            return null;
        }
    }
}

