namespace piece {

    public struct OutInfo {
        public bool flag;
        public bool[,] map;
    } 

    public struct SelectedPiece {
        public Piece piece;
        public int xPosition;
        public int yPosition;
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
        public int moveCount;

        public Piece(PieceType type, PieceColor color) {
            this.type = type;
            this.color = color;
        }
    }
}
