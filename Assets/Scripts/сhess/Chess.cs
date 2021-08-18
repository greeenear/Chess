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
            Piece piece,
            Position piecePos,
            Piece[,] board,
            List<Position> canMovePositions
        ) {
            switch (piece.type) {
                case PieceType.Pawn:
                    GetPawnMove(piece, piecePos, board, canMovePositions);
                    break;

                case PieceType.Bishop:
                    GetDiagonalMove(piece, piecePos, board, canMovePositions, 1, 1, 8);
                    GetDiagonalMove(piece, piecePos, board, canMovePositions, 1, -1, 8);
                    GetDiagonalMove(piece, piecePos, board, canMovePositions, -1, -1, 8);
                    GetDiagonalMove(piece, piecePos, board, canMovePositions, -1, 1, 8);
                    break;
            }
            return canMovePositions;
        }

        public static List<Position> GetPawnMove(
            Piece pawn,
            Position pawnPosition,
            Piece[,] board,
            List<Position> canMovePositions
        ) {
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
            Piece piece,
            Position piecePosition,
            Piece[,] board,
            List<Position> canMovePositions,
            int up,
            int right,
            int length
        ) {
            Debug.Log(length);
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

        //public static bool[,] PawnMove(
        //    SelectedPiece selectedPiece,
        //    bool[,] canMoveMap,
        //    Piece[,] piecesMap
        //) {
        //    var x = selectedPiece.xPosition;
        //    var y = selectedPiece.yPosition;

        //    int dir;
        //    if (selectedPiece.piece.color == PieceColor.White) {
        //        dir = -1;
        //    } else {
        //        dir = 1;
        //    }
        //    if (OnChessBoard(x + dir, y)
        //            && piecesMap[x + dir, y] != null) {
        //    } else if (x == 6 && dir == -1 || x == 1 && dir == 1) {
        //        if (OnChessBoard(x + dir, y)
        //            && piecesMap[x + dir, y] == null) {
        //            canMoveMap[x + dir, y] = true;
        //        }
        //        if (OnChessBoard(x + dir * 2, y)
        //            && piecesMap[x + dir * 2, y] == null) {
        //            canMoveMap[x + dir * 2, y] = true;
        //        }
        //    } else if(OnChessBoard(x + dir, y)) {
        //        canMoveMap[x + dir, y] = true;
        //    }
        //    if (OnChessBoard(x + dir, y - 1)
        //        && piecesMap[x + dir, y - 1] != null
        //        && piecesMap[x + dir, y - 1].color != selectedPiece.piece.color) {
        //        canMoveMap[x + dir, y - 1] = true;
        //    }
        //    if (OnChessBoard(x, y - 1)
        //        && piecesMap[x, y - 1] != null
        //        && piecesMap[x, y - 1].color != selectedPiece.piece.color 
        //        && piecesMap[x, y - 1].moveCount == 1) {
        //        canMoveMap[x + dir, y - 1] = true;
        //    }
        //    if (OnChessBoard(x, y + 1)
        //        && piecesMap[x, y + 1] != null
        //        && piecesMap[x, y + 1].color != selectedPiece.piece.color
        //        && piecesMap[x, y + 1].moveCount == 1) {
        //        canMoveMap[x + dir, y + 1] = true;
        //    }
        //    if (OnChessBoard(x + dir, y + 1)
        //        && piecesMap[x + dir, y + 1] != null
        //        && piecesMap[x + dir, y + 1].color != selectedPiece.piece.color) {
        //        canMoveMap[x + dir, y + 1] = true;
        //    }
        //    ñhangePawn?.Invoke();
        //    return canMoveMap;
        //}

        //public static bool[,] KnightMove(
        //    SelectedPiece selectedPiece,
        //    int newPossitionX,
        //    int newPossitionY,
        //    bool[,] canMoveMap,
        //    bool isKing,
        //    Piece[,] piecesMap
        //) {
        //    int xPossition = selectedPiece.xPosition + newPossitionX;
        //    int yPossition = selectedPiece.yPosition + newPossitionY;

        //    if (OnChessBoard(xPossition, yPossition)
        //        && piecesMap[xPossition, yPossition] == null) {
        //        canMoveMap[xPossition, yPossition] = true;

        //    } else if (OnChessBoard(xPossition, yPossition)
        //        && piecesMap[xPossition, yPossition].color
        //        != selectedPiece.piece.color) {
        //        if (isKing) {
        //            pieceAttakingKing[xPossition, yPossition] = true;
        //        } else {
        //            canMoveMap[xPossition, yPossition] = true;
        //        }
        //    }
        //    return canMoveMap;
        //}

        //public static bool[,] DiagonalMove(
        //    SelectedPiece selectedPiece,
        //    int length,
        //    bool[,] canMoveMap,
        //    bool isKing,
        //    Piece[,] piecesMap
        //) {
        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition + i;
        //        int y = selectedPiece.yPosition + i;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {
        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //            && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //            && piecesMap[x, y].color != selectedPiece.piece.color) {
        //            if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //            } else {
        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition + i;
        //        int y = selectedPiece.yPosition - i;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {
        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //            && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //            && piecesMap[x, y].color != selectedPiece.piece.color) {

        //            if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //            } else if (OnChessBoard(x, y)) {
        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition - i;
        //        int y = selectedPiece.yPosition - i;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {
        //            canMoveMap[x, y] = true;

        //        } else if (OnChessBoard(x, y)
        //            && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color != selectedPiece.piece.color) {
        //            if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //            } else {
        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition - i;
        //        int y = selectedPiece.yPosition + i;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {
        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //            && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color != selectedPiece.piece.color) {
        //            if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //            } else {
        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }
        //    return canMoveMap;
        //}

        //public static bool[,] VerticalMove(
        //    SelectedPiece selectedPiece,
        //    int length,
        //    bool[,] canMoveMap,
        //    bool isKing,
        //    Piece[,] piecesMap
        //) {
        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition + i;
        //        int y = selectedPiece.yPosition;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {

        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color != selectedPiece.piece.color) {
        //                if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //            } else {
        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition;
        //        int y = selectedPiece.yPosition + i;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {
        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color != selectedPiece.piece.color) {
        //            if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //            } else {
        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 1; i <= length; i++) {
        //        int x = selectedPiece.xPosition;
        //        int y = selectedPiece.yPosition - i;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {

        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color == selectedPiece.piece.color) {

        //            break;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color != selectedPiece.piece.color) {

        //            if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;

        //            } else {

        //                canMoveMap[x, y] = true;
        //                break;
        //            }
        //        }
        //    }

        //    for (int i = 1; i <= length; i++) {

        //        int x = selectedPiece.xPosition - i;
        //        int y = selectedPiece.yPosition;

        //        if (OnChessBoard(x, y)
        //            && piecesMap[x, y] == null) {
        //            canMoveMap[x, y] = true;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color == selectedPiece.piece.color) {
        //            break;
        //        } else if (OnChessBoard(x, y)
        //              && piecesMap[x, y].color != selectedPiece.piece.color) {
        //                if (isKing) {
        //                pieceAttakingKing[x, y] = true;
        //                break;
        //                } else {
        //                canMoveMap[x, y] = true;
        //                break;
        //                }
        //          }
        //    }
        //    return canMoveMap;
        //}

        public static bool OnChessBoard(int i, int j) {
            if (i > 7 || i < 0 || j > 7 || j < 0) {
                return false;
            }
            return true;
        }
    }
}

