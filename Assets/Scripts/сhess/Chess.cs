using UnityEngine;
using System.Collections.Generic;
using option;

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

    public class Piece {
        public PieceType type;
        public PieceColor color;
        
        public static Piece mk(PieceType type, PieceColor color) {
            return new Piece { type = type, color = color };
        }
    }

    public struct LineMove {
        public Vector2Int dir;
        public int length;

        public static LineMove mk(Vector2Int dir, int length) {
            return new LineMove {dir = dir, length = length};
        }
    }

    public struct CircleMove {
        public float radius;

        public static CircleMove mk(float radius) {
            return new CircleMove { radius = radius };
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
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, -1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, -1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 0), 2)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 0), 2)));
                    break;
                case PieceType.Bishop:
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, -1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, -1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 1), 8)));
                    break;
                case PieceType.Rook:
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 0), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 0), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(0, -1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(0, 1), 8)));
                    break;
                case PieceType.Queen:
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, -1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, -1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 0), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 0), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(0, -1), 8)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(0, 1), 8)));
                    break;
                case PieceType.King:
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, -1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, -1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(1, 0), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(-1, 0), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(0, -1), 1)));
                    moves.AddRange(CalcLineMove(board, pos, LineMove.mk(new Vector2Int(0, 1), 1)));
                    break;
                case PieceType.Knight:
                    moves.AddRange(CalcCircleMove(board, pos, CircleMove.mk(2f)));
                    break;
            }

            return moves;
        }

        public static List<Vector2Int> CalcLineMove(
            Option<Piece>[,] board,
            Vector2Int piecePosition,
            LineMove lineMove
        ) {
            Piece piece = board[piecePosition.x, piecePosition.y].Peel();
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            for (int i = 1; i <= lineMove.length; i++) {
                int x = piecePosition.x + lineMove.dir.x * i;
                int y = piecePosition.y + lineMove.dir.y * i;

                if (OnChessBoard(x, y) && board[x, y].IsNone()) {
                    canMovePositions.Add(new Vector2Int(x, y));

                } else if (OnChessBoard(x, y) && board[x, y].Peel().color == piece.color) {
                    break;

                } else if (OnChessBoard(x, y) && board[x, y].Peel().color != piece.color) {
                    canMovePositions.Add(new Vector2Int(x, y));
                    break;
                }
            }
            return canMovePositions;
        }

        public static List<Vector2Int> CalcCircleMove(
            Option<Piece>[,] board,
            Vector2Int pos,
            CircleMove circleMove
        ) {
            List<Vector2Int> canMovePositions = new List<Vector2Int>();
            for (int i = 1; i < 16; i += 2) {
                var x = Mathf.Sin(22.5f * i * Mathf.PI / 180) * circleMove.radius + 0.5f + pos.x;
                var y = Mathf.Cos(22.5f * i * Mathf.PI / 180) * circleMove.radius + 0.5f + pos.y;

                if(x < 0) {
                    x -= 1;
                }
                if (y < 0) {
                    y -= 1;
                }

                if(OnChessBoard((int)x, (int)y)) {
                    var possipleCell = board[(int)x, (int)y];

                    if (possipleCell.IsNone()) {
                        canMovePositions.Add(new Vector2Int((int)x, (int)y));
                    }

                    if (possipleCell.IsSome() && possipleCell.Peel().color
                        != board[pos.x, pos.y].Peel().color) {
                        canMovePositions.Add(new Vector2Int((int)x, (int)y));
                    }
                }
            }  
            return canMovePositions;
        }

            public static List<Vector2Int> GetKnightMove(
            Vector2Int piecePosition,
            Option<Piece>[,] board,
            List<Vector2Int> canMovePositions,
            int up,
            int right
        ) {
            Piece piece = board[piecePosition.x, piecePosition.y].Peel();
            int x = piecePosition.x + up;
            int y = piecePosition.y + right;

            if (OnChessBoard(x, y) && board[x, y].IsNone()) {
                canMovePositions.Add(new Vector2Int(x, y));

            } else if (OnChessBoard(x, y) && board[x, y].Peel().color != piece.color) {
                canMovePositions.Add(new Vector2Int(x, y));
            }
            return canMovePositions;
        }

        public static Vector2Int? FindKing(Option<Piece>[,] board, PieceColor whoseMove) {
            for(int i = 0; i < 8; i++) {
                for(int j = 0; j < 8; j++) {
                    if (board[i, j].IsSome() && board[i, j].Peel().type == PieceType.King 
                        && board[i, j].Peel().color == whoseMove) {
                        return new Vector2Int(i, j);
                    }
                }
            }
            return null;
        }

        public static bool OnChessBoard(int i, int j) {
            if (i > 7 || i < 0 || j > 7 || j < 0) {
                return false;
            }
            return true;
        }
    }
}

