using System.Collections.Generic;
using rules;

namespace json {
    public struct PieceInfo {
        public Piece piece;
        public int x;
        public int y;

        public static PieceInfo Mk(Piece piece, int xPos, int yPos) {
            return new PieceInfo {piece = piece, x = xPos, y = yPos};
        }
    }

    public struct TraceInfo {
        public Trace trace;
        public int x;
        public int y;

        public static TraceInfo Mk(Trace trace, int xPos, int yPos) {
            return new TraceInfo {trace = trace, x = xPos, y = yPos};
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

        public static JsonObject Mk(List<PieceInfo> info, List<TraceInfo> trace, GameStats stats) {
            return new JsonObject { pieceInfos = info, traceInfos = trace, gameStats = stats};
        }
    }
    
}

