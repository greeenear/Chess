using piece;
using board;
namespace chess {
    public class Chess {
        public static bool[,] pieceAttakingKing = new bool[8, 8];
        public static SelectedPiece? SelectPiece(
            int xPossition,
            int yPossition,
            Piece[,] chessBoard,
            PieceColor whoseMove
        ) {
            SelectedPiece selectedPiece = new SelectedPiece();
            selectedPiece.xPosition = xPossition;
            selectedPiece.yPosition = yPossition;
            if (chessBoard[xPossition, yPossition] != null
            && chessBoard[xPossition, yPossition].color == whoseMove) {
               selectedPiece.piece = chessBoard[xPossition, yPossition];
               return selectedPiece;
            }

            return null;
        }

        public static bool[,] GetCanMoveMapForPiece(
            SelectedPiece selected,
            bool[,] canMoveMap,
            Piece[,] piecesMap
        ) {
            switch (selected.piece.type) {
                case PieceType.Pawn:
                    canMoveMap = PawnMove(selected, canMoveMap, piecesMap);
                    break;
                case PieceType.Bishop:
                    canMoveMap = DiagonalMove(selected, 7, canMoveMap, false, piecesMap);
                    break;
                case PieceType.Rook:
                    canMoveMap = VerticalMove(selected, 7, canMoveMap, false, piecesMap);
                    break;
                case PieceType.Queen:
                    canMoveMap = DiagonalMove(selected, 7, canMoveMap, false, piecesMap);
                    canMoveMap = VerticalMove(selected, 7, canMoveMap, false, piecesMap);
                    break;
                case PieceType.King:
                    canMoveMap = DiagonalMove(selected, 1, canMoveMap, false, piecesMap);
                    canMoveMap = VerticalMove(selected, 1, canMoveMap, false, piecesMap);
                    break;
                case PieceType.Knight:
                    canMoveMap = KnightMove(selected, 2, 1, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, 2, -1, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, 1, 2, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, -1, 2, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, -2, 1, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, 1, -2, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, -2, -1, canMoveMap, false, piecesMap);
                    canMoveMap = KnightMove(selected, -1, -2, canMoveMap, false, piecesMap);

                    break;
            }
            return canMoveMap;
        }

        public static bool[,] PawnMove(
            SelectedPiece selectedPiece,
            bool[,] canMoveMap,
            Piece[,] piecesMap
        ) {
            var x = selectedPiece.xPosition;
            var y = selectedPiece.yPosition;

            int dir;
            if (selectedPiece.piece.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }
            if (OnChessBoard(x + dir, y)
                    && piecesMap[x + dir, y] != null) {
            } else if (x == 6 && dir == -1 || x == 1 && dir == 1) {
                if (OnChessBoard(x + dir, y)
                    && piecesMap[x + dir, y] == null) {
                    canMoveMap[x + dir, y] = true;
                }
                if (OnChessBoard(x + dir * 2, y)
                    && piecesMap[x + dir * 2, y] == null) {
                    canMoveMap[x + dir * 2, y] = true;
                }
            } else {
                canMoveMap[x + dir, y] = true;
            }
            if (OnChessBoard(x + dir, y - 1)
                && piecesMap[x + dir, y - 1] != null
                && piecesMap[x + dir, y - 1].color != selectedPiece.piece.color) {
                canMoveMap[x + dir, y - 1] = true;
            }
            if (OnChessBoard(x, y - 1)
                && piecesMap[x, y - 1] != null
                && piecesMap[x, y - 1].color != selectedPiece.piece.color 
                && piecesMap[x, y - 1].moveCount == 1) {
                canMoveMap[x + dir, y - 1] = true;
            }
            if (OnChessBoard(x, y + 1)
                && piecesMap[x, y + 1] != null
                && piecesMap[x, y + 1].color != selectedPiece.piece.color
                && piecesMap[x, y + 1].moveCount == 1) {
                canMoveMap[x + dir, y + 1] = true;
            }
            if (OnChessBoard(x + dir, y + 1)
                && piecesMap[x + dir, y + 1] != null
                && piecesMap[x + dir, y + 1].color != selectedPiece.piece.color) {
                canMoveMap[x + dir, y + 1] = true;
            }
            return canMoveMap;
        }

        public static bool[,] KnightMove(
            SelectedPiece selectedPiece,
            int newPossitionX,
            int newPossitionY,
            bool[,] canMoveMap,
            bool isKing,
            Piece[,] piecesMap
        ) {
            int xPossition = selectedPiece.xPosition + newPossitionX;
            int yPossition = selectedPiece.yPosition + newPossitionY;

            if (OnChessBoard(xPossition, yPossition)
                && piecesMap[xPossition, yPossition] == null) {
                canMoveMap[xPossition, yPossition] = true;

            } else if (OnChessBoard(xPossition, yPossition)
                && piecesMap[xPossition, yPossition].color
                != selectedPiece.piece.color) {
                if (isKing) {
                    pieceAttakingKing[xPossition, yPossition] = true;
                } else {
                    canMoveMap[xPossition, yPossition] = true;
                }
            }
            return canMoveMap;
        }

        public static bool[,] DiagonalMove(
            SelectedPiece selectedPiece,
            int lenght,
            bool[,] canMoveMap,
            bool isKing,
            Piece[,] piecesMap
        ) {
            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition + i;
                int y = selectedPiece.yPosition + i;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {
                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                    && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                    && piecesMap[x, y].color != selectedPiece.piece.color) {
                    if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                    } else {
                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition + i;
                int y = selectedPiece.yPosition - i;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {
                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                    && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                    && piecesMap[x, y].color != selectedPiece.piece.color) {

                    if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                    } else if (OnChessBoard(x, y)) {
                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition - i;
                int y = selectedPiece.yPosition - i;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {
                    canMoveMap[x, y] = true;

                } else if (OnChessBoard(x, y)
                    && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color != selectedPiece.piece.color) {
                    if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                    } else {
                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition - i;
                int y = selectedPiece.yPosition + i;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {
                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                    && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color != selectedPiece.piece.color) {
                    if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                    } else {
                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }
            return canMoveMap;
        }

        public static bool[,] VerticalMove(
            SelectedPiece selectedPiece,
            int lenght,
            bool[,] canMoveMap,
            bool isKing,
            Piece[,] piecesMap
        ) {
            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition + i;
                int y = selectedPiece.yPosition;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {

                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color != selectedPiece.piece.color) {
                        if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                    } else {
                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition;
                int y = selectedPiece.yPosition + i;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {
                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color != selectedPiece.piece.color) {
                    if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                    } else {
                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int x = selectedPiece.xPosition;
                int y = selectedPiece.yPosition - i;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {

                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color == selectedPiece.piece.color) {

                    break;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color != selectedPiece.piece.color) {

                    if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;

                    } else {

                        canMoveMap[x, y] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int x = selectedPiece.xPosition - i;
                int y = selectedPiece.yPosition;

                if (OnChessBoard(x, y)
                    && piecesMap[x, y] == null) {
                    canMoveMap[x, y] = true;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color == selectedPiece.piece.color) {
                    break;
                } else if (OnChessBoard(x, y)
                      && piecesMap[x, y].color != selectedPiece.piece.color) {
                        if (isKing) {
                        pieceAttakingKing[x, y] = true;
                        break;
                        } else {
                        canMoveMap[x, y] = true;
                        break;
                        }
                  }
            }
            return canMoveMap;
        }

        public static bool OnChessBoard(int i, int j) {
            if (i > 7 || i < 0 || j > 7 || j < 0) {
                return false;
            }
            return true;
        }


    }


}

