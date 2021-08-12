using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace board{
    public class Piece {

        public PieceType type;
        public PieceColor color;

        public Piece(PieceType type, PieceColor color)
        {
            this.type = type;
            this.color = color;
        }
    }

}
