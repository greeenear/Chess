using System.Collections.Generic;
using System;
using UnityEngine;
using controller;
using chess;
using option;
using jonson;
using jonson.reflect;
using System.IO;

namespace json {

    struct PieceInfo {
        public Piece piece;
        public int xPos;
        public int yPos;

        public static PieceInfo mk(Piece piece, int xPos, int yPos) {
            return new PieceInfo {piece = piece, xPos = xPos, yPos = yPos};
        }
    }

    struct GameStats {
        public PieceColor whoseMove;

        public static GameStats mk(PieceColor whoseMove) {
            return new GameStats{whoseMove = whoseMove};
        }
    }

    public class Json : MonoBehaviour {
        public static Action loadingBoard;

        GameStats gameStats;
        List<PieceInfo> pieceList = new List<PieceInfo>();
        private string outputPiecePos;
        private string outputGameStats;
        
        public void Save() {
            gameStats = GameStats.mk(ChessBoardController.whoseMove);
            for(int i = 0; i < 8; i++) {
                for(int j = 0; j < 8; j++) {
                    if(ChessBoardController.board[i,j].IsSome()) {
                        pieceList.Add(PieceInfo.mk(ChessBoardController.board[i,j].Peel(), i, j));
                    }
                }
            }
            JSONType personType = Reflect.ToJSON(pieceList, true);
            outputPiecePos = Jonson.Generate(personType);

            JSONType jsonMoveType = Reflect.ToJSON(gameStats, true);
            outputGameStats = Jonson.Generate(jsonMoveType);

            File.WriteAllText("json.txt", outputPiecePos);
            File.WriteAllText("GameStats.txt", outputGameStats);
        }

        public void Load() {
            outputPiecePos = File.ReadAllText("json.txt");
            outputGameStats = File.ReadAllText("GameStats.txt");

            Result<JSONType, JSONError> personRes = Jonson.Parse(outputPiecePos, 1024);
            if (personRes.IsErr()) {
                return;
            }
            
            Result<JSONType, JSONError> gameStatsRes = Jonson.Parse(outputGameStats, 1024);
            if (gameStatsRes.IsErr()) {
                return;
            }
            
            var loadGameStats =  Reflect.FromJSON(gameStats, gameStatsRes.AsOk());
            var list  = Reflect.FromJSON(pieceList, personRes.AsOk());

            ChessBoardController.whoseMove = gameStats.whoseMove;
            for(int i = 0; i < 8; i++) {
                for(int j = 0; j < 8; j++) {
                    ChessBoardController.board[i, j] = Option<Piece>.None();
                }
            }
            foreach(var PieceInfo in list) {
                ChessBoardController.board[PieceInfo.xPos, PieceInfo.yPos] 
                    = Option<Piece>.Some(PieceInfo.piece);
            }

            gameObject.GetComponent<ChessBoardController>().AddPiecesOnBoard(
                gameObject.GetComponent<ChessBoardController>().pieceGameObjects,
                 gameObject.GetComponent<Resource>().pieceList
            );
        }
    }
}

