using board;

namespace piece{
    public class Piece {

        public PieceType type;
        public PieceColor color;

        public Piece(PieceType type, PieceColor color) {
            this.type = type;
            this.color = color;
        }
    }

}
