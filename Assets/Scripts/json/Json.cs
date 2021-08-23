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

    public class Json : MonoBehaviour {
        public static Action loadingBoard;
        List<PieceInfo> pieceList = new List<PieceInfo>();
        private string outputPiecePos;
        private string outputGameStats;
        
        public void Save() {
            for(int i = 0; i < 8; i++) {
                for(int j = 0; j < 8; j++) {
                    if(ChessBoardController.board[i,j].IsSome()) {
                        pieceList.Add(PieceInfo.mk(ChessBoardController.board[i,j].Peel(), i, j));
                    }
                }
            }
            JSONType personType = Reflect.ToJSON(pieceList, true);
            outputPiecePos = Jonson.Generate(personType);

            File.WriteAllText("json.txt", outputPiecePos);
        }

        public void Load() {
            outputPiecePos = File.ReadAllText("json.txt");

            Result<JSONType, JSONError> personRes = Jonson.Parse(outputPiecePos, 1024);
            if (personRes.IsErr()) {
                return;
            }
            
            var list  = Reflect.FromJSON(pieceList, personRes.AsOk());

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

