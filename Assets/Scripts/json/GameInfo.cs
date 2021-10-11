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

    public struct TraceInfo {
        public PieceTrace trace;
        public int xPos;
        public int yPos;

        public static TraceInfo Mk(PieceTrace trace, int xPos, int yPos) {
            return new TraceInfo {trace = trace, xPos = xPos, yPos = yPos};
        }
    }

    public struct GameStats {
        public PieceColor whoseMove;

        public static GameStats Mk(PieceColor whoseMove) {
            return new GameStats{whoseMove = whoseMove};
        }
    }

    public struct JsonObject {
        public List<PieceInfo> pieceInfos;
        public List<TraceInfo> traceInfos;
        public GameStats gameStats;

        public static JsonObject Mk(List<PieceInfo> pieceInfo, List<TraceInfo> traceInfos, GameStats gameStats) {
            return new JsonObject { pieceInfos = pieceInfo, traceInfos = traceInfos, gameStats = gameStats};
        }
    }
    
}

