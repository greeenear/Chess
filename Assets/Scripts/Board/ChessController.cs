using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace board {

    enum PieceType{
        BishopBlack,
        BishopWhite,
        KingBlack,
        KingWhite,
        KnightBlack,
        KnightWhite,
        PawnBlack,
        PawnWhite,
        QueenBlack,
        QueenWhite,
        RookBlack,
        RookWhite
        }

    struct SelectedPiece{
        public string type;
        public int xPossition;
        public int yPossition;
    }
    public class ChessController : MonoBehaviour {

        int xPossition;
        int yPossition;

        Ray ray;
        RaycastHit hit;

        Nullable<SelectedPiece> selectedPiece;

        public List<GameObject> pieceList = new List<GameObject>();
        private GameObject[,] pieceGameObjects = new GameObject[8, 8];
        private string[,] board = new string[8, 8];
        private bool[,] CanMoveMap = new bool[8, 8];

        void Start() {
            AddPieces();
            AddPiecesOnBoard();
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    xPossition = (int)hit.point.x;
                    yPossition = (int)hit.point.z;

                    if (selectedPiece != null){
                        Debug.Log("+");
                        Move(xPossition, yPossition, (SelectedPiece)selectedPiece);
                        selectedPiece = null;
                    }
                    if (selectedPiece == null && SelectPiece(xPossition, yPossition)!=null) {
                        ClearCanMoveMap();
                        selectedPiece = SelectPiece(xPossition, yPossition);
                        GetCanMoveMapForPiece((SelectedPiece)selectedPiece);
                    } 
                 
                   

                }
            }
        }
        private void AddPieces() {

            board[0, 0] = "BRook";
            board[0, 1] = "BKnight";
            board[0, 2] = "BBishop";
            board[0, 4] = "BQueen";
            board[0, 3] = "BKing";
            board[0, 5] = "BBishop";
            board[0, 6] = "BKnight";
            board[0, 7] = "BRook";

            for (int i = 0; i < 8; i++) {
                board[1, i] = "BPawn";
            }

            board[7, 0] = "WRook";
            board[7, 1] = "WKnight";
            board[7, 2] = "WBishop";
            board[7, 4] = "WQueen";
            board[7, 3] = "WKing";
            board[7, 5] = "WBishop";
            board[7, 6] = "WKnight";
            board[7, 7] = "WRook";

            for (int i = 0; i < 8; i++) {
                board[6, i] = "WPawn";
            }

        }

        private void AddPiecesOnBoard() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    switch (board[i, j]) {
                        case "BPawn":
                            pieceGameObjects[i,j] = Instantiate(pieceList[(int)PieceType.PawnBlack],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "BRook":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.RookBlack],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "BKnight":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.KnightBlack],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "BBishop":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.BishopBlack],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "BQueen":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.QueenBlack],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "BKing":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.KingBlack],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;

                        case "WPawn":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.PawnWhite],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "WRook":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.RookWhite],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "WKnight":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.KnightWhite],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "WBishop":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.BishopWhite],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "WQueen":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.QueenWhite],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                        case "WKing":
                            pieceGameObjects[i, j] = Instantiate(pieceList[(int)PieceType.KingWhite],
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            break;
                    }

                }
            }
        }


        private Nullable<SelectedPiece> SelectPiece(int xPossition, int yPossition) {
            SelectedPiece selectedPiece = new SelectedPiece();
            selectedPiece.xPossition = xPossition;
            selectedPiece.yPossition = yPossition;

            if (board[xPossition, yPossition] == "WPawn") {
                selectedPiece.type = "Pawn";
                return selectedPiece;
            }
            if (board[xPossition, yPossition] == "BPawn")
            {
                selectedPiece.type = "Pawn";
                return selectedPiece;
            }
            return null;
        }

        private void GetCanMoveMapForPiece(SelectedPiece selectedPiece) {
            switch (selectedPiece.type) {
                case "Pawn":
                    GetCanMoveMapForPawn(selectedPiece.xPossition, selectedPiece.yPossition);
                    break;
            }
        }

        private void GetCanMoveMapForPawn(int xPossition, int yPossition){
            if (board[xPossition, yPossition] == "WPawn" && xPossition == 6) {

                CanMoveMap[xPossition - 1, yPossition] = true;
                CanMoveMap[xPossition - 2, yPossition] = true;
            }else if(board[xPossition, yPossition] == "WPawn"){

                CanMoveMap[xPossition - 1, yPossition] = true;
            }

            if (board[xPossition, yPossition] == "BPawn" && xPossition == 1)
            {
                CanMoveMap[xPossition + 1, yPossition] = true;
                CanMoveMap[xPossition + 2, yPossition] = true;
            }else if (board[xPossition, yPossition] == "BPawn"){

                CanMoveMap[xPossition +1, yPossition] = true;
            }

        }


        private void Move(int xPossition, int yPossition , SelectedPiece selectedPiece){
            if (CanMoveMap[xPossition, yPossition] == true){
                board[xPossition, yPossition] = board[selectedPiece.xPossition, selectedPiece.yPossition];
                pieceGameObjects[xPossition, yPossition] = pieceGameObjects[selectedPiece.xPossition, selectedPiece.yPossition];
                pieceGameObjects[xPossition, yPossition].transform.position = new Vector3(xPossition+0.5f,0.5f,yPossition+0.5f);
                board[selectedPiece.xPossition, selectedPiece.yPossition] = null;
                
            }
        }


        private void ClearCanMoveMap()
        {
            for(int i = 0; i < 8; i++){
                for(int j = 0; j < 8; j++){
                    CanMoveMap[i, j] = false;
                }
            }
        }

    }


    


}

