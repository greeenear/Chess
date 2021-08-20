using UnityEngine;
using System.Collections.Generic;

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

        public Piece(PieceType type, PieceColor color) {
            this.type = type;
            this.color = color;
        }
    }

    public struct Position {
        public int x;
        public int y;
        public Position(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }

    public static class Chess {
        public static List<Position> GetCanMoveMapForPiece(
            Position piecePos,
            Piece[,] board,
            List<Position> canMovePositions
        ) {
            Piece piece = board[piecePos.x, piecePos.y];
            switch (piece.type) {
                case PieceType.Pawn:
                    GetPawnMove(piecePos, board, canMovePositions);
                    break;

                case PieceType.Bishop:
                    GetDiagonalMove(piecePos, board, canMovePositions, 1, 1, 8);
                    GetDiagonalMove(piecePos, board, canMovePositions, 1, -1, 8);
                    GetDiagonalMove(piecePos, board, canMovePositions, -1, -1, 8);
                    GetDiagonalMove(piecePos, board, canMovePositions, -1, 1, 8);
                    break;

                case PieceType.Rook:
                    GetVerticalMove(piecePos, board, canMovePositions, 1, 0, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, -1, 0, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, 0, -1, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, 0, 1, 8);
                    break;

                case PieceType.Queen:
                    GetDiagonalMove(piecePos, board, canMovePositions, 1, 1, 8);
                    GetDiagonalMove(piecePos, board, canMovePositions, 1, -1, 8);
                    GetDiagonalMove(piecePos, board, canMovePositions, -1, -1, 8);
                    GetDiagonalMove(piecePos, board, canMovePositions, -1, 1, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, 1, 0, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, -1, 0, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, 0, -1, 8);
                    GetVerticalMove(piecePos, board, canMovePositions, 0, 1, 8);
                    break;

                case PieceType.King:
                    GetDiagonalMove(piecePos, board, canMovePositions, 1, 1, 1);
                    GetDiagonalMove(piecePos, board, canMovePositions, 1, -1, 1);
                    GetDiagonalMove(piecePos, board, canMovePositions, -1, -1, 1);
                    GetDiagonalMove(piecePos, board, canMovePositions, -1, 1, 1);
                    GetVerticalMove(piecePos, board, canMovePositions, 1, 0, 1);
                    GetVerticalMove(piecePos, board, canMovePositions, -1, 0, 1);
                    GetVerticalMove(piecePos, board, canMovePositions, 0, -1, 1);
                    GetVerticalMove(piecePos, board, canMovePositions, 0, 1, 1);
                    break;

                case PieceType.Knight:
                    GetKnightMove(piecePos, board, canMovePositions, 2, 1);
                    GetKnightMove(piecePos, board, canMovePositions, 2, -1);
                    GetKnightMove(piecePos, board, canMovePositions, 1, 2);
                    GetKnightMove(piecePos, board, canMovePositions, -1, 2);
                    GetKnightMove(piecePos, board, canMovePositions, -2, 1);
                    GetKnightMove(piecePos, board, canMovePositions, -2, -1);
                    GetKnightMove(piecePos, board, canMovePositions, 1, -2);
                    GetKnightMove(piecePos, board, canMovePositions, -1, -2);
                    break;
            }
            return canMovePositions;
        }

        public static List<Position> GetPawnMove(
            Position pawnPosition,
            Piece[,] board,
            List<Position> canMovePositions
        ) {
            Piece pawn = board[pawnPosition.x, pawnPosition.y];
            int dir;
            int x = pawnPosition.x;
            int y = pawnPosition.y;

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }

            if (OnChessBoard(x + dir, y)
                    && board[x + dir, y] != null) {
            } else if (x == 6 && dir == -1 || x == 1 && dir == 1) {
                if (OnChessBoard(x + dir, y)
                    && board[x + dir, y] == null) {
                    canMovePositions.Add(new Position(x + dir, y));
                }
                if (OnChessBoard(x + dir * 2, y)
                    && board[x + dir * 2, y] == null) {
                    canMovePositions.Add(new Position(x + dir * 2, y));
                }
            } else if (OnChessBoard(x + dir, y)) {
                canMovePositions.Add(new Position(x + dir, y));
            }

            if (OnChessBoard(x + dir, y - 1)
                && board[x + dir, y - 1] != null
                && board[x + dir, y - 1].color != pawn.color) {
                canMovePositions.Add(new Position(x + dir, y - 1));
            }
            
            if (OnChessBoard(x + dir, y + 1)
                && board[x + dir, y + 1] != null
                && board[x + dir, y + 1].color != pawn.color) {
                canMovePositions.Add(new Position(x + dir, y + 1));
            }
            return canMovePositions;
        }

        public static List<Position> GetDiagonalMove(
            Position piecePosition,
            Piece[,] board,
            List<Position> canMovePositions,
            int up,
            int right,
            int length
        ) {
            Piece piece = board[piecePosition.x, piecePosition.y];
            for (int i = 1; i <= length; i++) {
                int x = piecePosition.x + up * i;
                int y = piecePosition.y + right * i;

                if (OnChessBoard(x, y) && board[x, y] == null) {
                    canMovePositions.Add(new Position(x, y));

                } else if (OnChessBoard(x, y) && board[x, y].color == piece.color) {
                    break;

                } else if (OnChessBoard(x, y) && board[x, y].color != piece.color) {
                    canMovePositions.Add(new Position(x, y));
                    break;
                }
            }
            return canMovePositions;
        }

        public static List<Position> GetVerticalMove(
            Position piecePosition,
            Piece[,] board,
            List<Position> canMovePositions,
            int up,
            int right,
            int length
        ){
            Piece piece = board[piecePosition.x, piecePosition.y];
            for (int i = 1; i <= length; i++) {
                int x = piecePosition.x + i * up;
                int y = piecePosition.y + i * right;

                if (OnChessBoard(x, y) && board[x, y] == null) {
                    canMovePositions.Add(new Position(x, y));

                } else if (OnChessBoard(x, y) && board[x, y].color == piece.color) {
                    break;

                } else if (OnChessBoard(x, y) && board[x, y].color != piece.color) {
                    canMovePositions.Add(new Position(x, y));
                    break;
                    
                }
            }
            return canMovePositions;
        }

        public static List<Position> GetKnightMove(
            Position piecePosition,
            Piece[,] board,
            List<Position> canMovePositions,
            int up,
            int right
        ) {
            Piece piece = board[piecePosition.x, piecePosition.y];
            int x = piecePosition.x + up;
            int y = piecePosition.y + right;

            if (OnChessBoard(x, y) && board[x, y] == null) {
                canMovePositions.Add(new Position(x, y));

            } else if (OnChessBoard(x, y) && board[x, y].color != piece.color) {
                canMovePositions.Add(new Position(x, y));
            }
            return canMovePositions;
        }

        public static Position? FindKing(Piece[,] board, PieceColor whoseMove) {
            for(int i = 0; i < 8; i++) {
                for(int j = 0; j < 8; j++) {
                    if (board[i, j] != null && board[i, j].type == PieceType.King 
                        && board[i, j].color == whoseMove) {
                        return new Position(i, j);
                    }
                }
            }
            return null;
        }

        public static bool CheckKing(Piece[,] board, PieceColor whoseMove) {
            Position kingPosition = Chess.FindKing(board, whoseMove).Value;

            List<Position> canAttackKing = new List<Position>();
            List<Position> attackPositions = new List<Position>();
            board[kingPosition.x, kingPosition.y].type = PieceType.Queen;
            canAttackKing = Chess.GetCanMoveMapForPiece(
                kingPosition,
                board,
                canAttackKing);
            board[kingPosition.x, kingPosition.y].type = PieceType.Knight;

            canAttackKing = Chess.GetCanMoveMapForPiece(
                kingPosition,
                board,
                canAttackKing);

            foreach (var pos in canAttackKing) {
                if (board[pos.x, pos.y] != null) {
                    attackPositions = Chess.GetCanMoveMapForPiece(
                        new Position(pos.x, pos.y),
                        board,
                        attackPositions);
                }
            }
            board[kingPosition.x, kingPosition.y].type = PieceType.King;

            foreach (var attackPosition in attackPositions) {
                if (Equals(kingPosition, attackPosition)) {
                    return true;
                }
            }
            return false;
        }

        public static bool OnChessBoard(int i, int j) {
            if (i > 7 || i < 0 || j > 7 || j < 0) {
                return false;
            }
            return true;
        }
    }
}

