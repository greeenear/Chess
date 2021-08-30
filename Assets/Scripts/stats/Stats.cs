using System.Collections.Generic;
using rules;

namespace stats {
    public struct PieceInfo {
        public Piece piece;
        public int xPos;
        public int yPos;

        public static PieceInfo Mk(Piece piece, int xPos, int yPos) {
            return new PieceInfo {piece = piece, xPos = xPos, yPos = yPos};
        }
    }

    public struct GameStats {
        public PieceColor whoseMove;

        public static GameStats Mk(PieceColor whoseMove) {
            return new GameStats{whoseMove = whoseMove};
        }
    }

    public struct JsonObject {
        public List<PieceInfo> pieceInfo;
        public GameStats gameStats;

        public static JsonObject Mk(List<PieceInfo> pieceInfo, GameStats gameStats) {
            return new JsonObject { pieceInfo = pieceInfo, gameStats = gameStats};
        }
    }
}


