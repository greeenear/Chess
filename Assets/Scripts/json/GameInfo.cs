using System.Collections.Generic;
using rules;

namespace json {
    public struct PieceInfo {
        public Piece piece;
        public int xPos;
        public int yPos;

        public static PieceInfo Mk(Piece piece, int xPos, int yPos) {
            return new PieceInfo {piece = piece, xPos = xPos, yPos = yPos};
        }
    }

    public struct GameStats<T> {
        public PieceColor whoseMove;
        public List<T> movesHistory;

        public static GameStats<T> Mk(PieceColor whoseMove, List<T> movesHistory) {
            return new GameStats<T>{whoseMove = whoseMove, movesHistory = movesHistory};
        }
    }

    public struct JsonObject<T> {
        public List<PieceInfo> pieceInfo;
        public GameStats<T> gameStats;

        public static JsonObject<T> Mk(List<PieceInfo> pieceInfo, GameStats<T> gameStats) {
            return new JsonObject<T> { pieceInfo = pieceInfo, gameStats = gameStats};
        }
    }
    
}

