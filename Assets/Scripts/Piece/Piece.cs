namespace piece {
    public struct SelectedPiece {
        public Piece piece;
        public int xPossition;
        public int yPossition;
    }

    public enum PieceType {
        Bishop = 0,
        King = 2,
        Knight = 4,
        Pawn = 6,
        Queen = 8,
        Rook = 10,
    }

    public enum PieceColor {
        White,
        Black
    }

    public class Piece {
        public PieceType type;
        public PieceColor color;
        public bool isFirstMove;

        public Piece(PieceType type, PieceColor color) {
            this.type = type;
            this.color = color;
        }
    }
}
