using chess;
namespace controller {
    public class ChessBoard {
        public Piece [,] board = new Piece[8, 8];
        public ChessBoard() {
            board[0, 0] = new Piece(PieceType.Rook, PieceColor.Black);
            board[0, 1] = new Piece(PieceType.Knight, PieceColor.Black);
            board[0, 2] = new Piece(PieceType.Bishop, PieceColor.Black);
            board[0, 4] = new Piece(PieceType.King, PieceColor.Black);
            board[0, 3] = new Piece(PieceType.Queen, PieceColor.Black);
            board[0, 5] = new Piece(PieceType.Bishop, PieceColor.Black);
            board[0, 6] = new Piece(PieceType.Knight, PieceColor.Black);
            board[0, 7] = new Piece(PieceType.Rook, PieceColor.Black);

            for (int i = 0; i < 8; i++) {
                board[1, i] = new Piece(PieceType.Pawn, PieceColor.Black);
            }

            board[7, 0] = new Piece(PieceType.Rook, PieceColor.White);
            board[7, 1] = new Piece(PieceType.Knight, PieceColor.White);
            board[7, 2] = new Piece(PieceType.Bishop, PieceColor.White);
            board[7, 4] = new Piece(PieceType.King, PieceColor.White);
            board[7, 3] = new Piece(PieceType.Queen, PieceColor.White);
            board[7, 5] = new Piece(PieceType.Bishop, PieceColor.White);
            board[7, 6] = new Piece(PieceType.Knight, PieceColor.White);
            board[7, 7] = new Piece(PieceType.Rook, PieceColor.White);

            for (int i = 0; i < 8; i++) {
                board[6, i] = new Piece(PieceType.Pawn, PieceColor.White);
            }
        }
    }
}

