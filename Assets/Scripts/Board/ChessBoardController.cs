using System;
using System.Collections.Generic;
using UnityEngine;
using piece;

namespace board {

    public enum PieceType{
        Bishop,
        King,
        Knight,
        Pawn,
        Queen,
        Rook,
    }

    public enum PieceColor {
        White,
        Black
    }

    public struct SelectedPiece {
        public Piece piece;
        public int xPossition;
        public int yPossition;
    }
    
    public class ChessBoardController : MonoBehaviour {

        private ChessBoard chessBoard;

        private int xKingPossition;
        private int yKingPossition;
        private bool isCheck;

        private int xPossition;
        private int yPossition;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        private Nullable<SelectedPiece> selectedPiece;

        public GameObject canMoveCell;
        public GameObject check;
        private GameObject chekCell;
        private List<GameObject> canMoveCells = new List<GameObject>();

        public List<GameObject> pieceList = new List<GameObject>();
        private GameObject[,] pieceGameObjects = new GameObject[8, 8];
        private bool[,] canMoveMap = new bool[8, 8];
        private bool[,] canMoveMapForKing = new bool[8, 8];
        private bool[,] checkKingMap = new bool[8, 8];
        private bool[,] pieceAttakingKing = new bool[8, 8];

        void Start() {
            chessBoard = new ChessBoard();
            chessBoard.AddPieces();
            AddPiecesOnBoard();
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    xPossition = (int)hit.point.x;
                    yPossition = (int)hit.point.z;

                    if (selectedPiece == null && SelectPiece(xPossition, yPossition) != null) {

                        ClearCanMoveMap(canMoveMap);
                        RemoveCanMoveCells();

                        selectedPiece = SelectPiece(xPossition, yPossition);
                        GetCanMoveMapForPiece((SelectedPiece)selectedPiece, canMoveMap, 
                            chessBoard.board);
                        canMoveMap = HiddenCheck((SelectedPiece)selectedPiece, canMoveMap);
                        ShowCanMoveCells(canMoveMap);

                    } else if (selectedPiece != null) {

                        RemoveCanMoveCells();
                        if (Move(xPossition, yPossition, (SelectedPiece)selectedPiece)) {

                            DestroyCheckCell();
                            ChangeMove();

                            if (CheckKing(whoseMove, chessBoard.board)) {

                                isCheck = true;
                                chekCell = Instantiate(check, new
                                    Vector3(xKingPossition + 0.5f, 0.5f, yKingPossition + 0.5f),
                                    Quaternion.identity);
                            }

                            if (CheckMate()) {
                                if (isCheck) {
                                    Debug.Log("мат");
                                } else {
                                    Debug.Log("пат");
                                }
                                
                            }
                            
                        }
                        selectedPiece = null;
                    }

                }
            }
        }
        private void AddPiecesOnBoard() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (chessBoard.board[i, j] != null) {

                        if (chessBoard.board[i, j].type == PieceType.Pawn) {
                            if (chessBoard.board[i, j].color == PieceColor.White) {

                                pieceGameObjects[i, j] = Instantiate(pieceList[7],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f),
                                    Quaternion.identity);
                            } else {
                                pieceGameObjects[i, j] = Instantiate(pieceList[6],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Bishop) {

                            if (chessBoard.board[i, j].color == PieceColor.White) {

                                pieceGameObjects[i, j] = Instantiate(pieceList[1],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            } else {
                                pieceGameObjects[i, j] = Instantiate(pieceList[0],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Knight) {

                            if (chessBoard.board[i, j].color == PieceColor.White) {

                                pieceGameObjects[i, j] = Instantiate(pieceList[5],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            } else {
                                pieceGameObjects[i, j] = Instantiate(pieceList[4],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.King) {
                            if (chessBoard.board[i, j].color == PieceColor.White) {

                                pieceGameObjects[i, j] = Instantiate(pieceList[3],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            } else {

                                pieceGameObjects[i, j] = Instantiate(pieceList[2],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Queen) {

                            if (chessBoard.board[i, j].color == PieceColor.White) {

                                pieceGameObjects[i, j] = Instantiate(pieceList[9],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            } else {

                                pieceGameObjects[i, j] = Instantiate(pieceList[8],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Rook) {

                            if (chessBoard.board[i, j].color == PieceColor.White) {

                                pieceGameObjects[i, j] = Instantiate(pieceList[11],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            } else {

                                pieceGameObjects[i, j] = Instantiate(pieceList[10],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), 
                                    Quaternion.identity);
                            }
                        }

                    }
                }
            }
        }


        private Nullable<SelectedPiece> SelectPiece(int xPossition, int yPossition) {

            SelectedPiece selectedPiece = new SelectedPiece();
            selectedPiece.xPossition = xPossition;
            selectedPiece.yPossition = yPossition;
            if(chessBoard.board[xPossition, yPossition]!=null 
                && chessBoard.board[xPossition, yPossition].color == whoseMove) {
                selectedPiece.piece = chessBoard.board[xPossition, yPossition];
                return selectedPiece;
            }
            
            return null;
        }

        private void GetCanMoveMapForPiece(SelectedPiece selectedPiece, 
            bool [,] canMoveMap, Piece[,] piecesMap) {

            switch (selectedPiece.piece.type) {
                case PieceType.Pawn:
                    PawnMove(selectedPiece, canMoveMap, piecesMap);
                    break;

                case PieceType.Bishop:
                    DiagonalMove(selectedPiece, 7, canMoveMap, false, piecesMap);
                    break;

                case PieceType.Rook:
                    VerticalMove(selectedPiece, 7, canMoveMap, false, piecesMap);
                    break;

                case PieceType.Queen:
                    DiagonalMove(selectedPiece, 7, canMoveMap, false, piecesMap);
                    VerticalMove(selectedPiece, 7, canMoveMap, false, piecesMap);
                    break;

                case PieceType.King:
                    DiagonalMove(selectedPiece, 1, canMoveMap, false, piecesMap);
                    VerticalMove(selectedPiece, 1, canMoveMap, false, piecesMap);
                    break;

                case PieceType.Knight:
                    KnightMove(selectedPiece, 2, 1 , canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, 2, -1, canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, 1, 2, canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, -1, 2, canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, -2, 1, canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, 1, -2, canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, -2, -1, canMoveMap, false, piecesMap);
                    KnightMove(selectedPiece, -1, -2, canMoveMap, false, piecesMap);
                    break;
            }
        }

        private void PawnMove(SelectedPiece selectedPiece, bool[,] canMoveMap,
            Piece[,] piecesMap) {

        if (piecesMap[selectedPiece.xPossition, selectedPiece.yPossition].color == PieceColor.White) {

            if (OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition) 
                    && piecesMap[selectedPiece.xPossition - 1, selectedPiece.yPossition] != null)
            {

                } else if (selectedPiece.xPossition == 6) {

                    if (OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition) 
                        && piecesMap[selectedPiece.xPossition - 1, selectedPiece.yPossition] == null) {

                        canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition] = true;
                    }
                        
                    if (OnChessBoard(selectedPiece.xPossition - 2, selectedPiece.yPossition) 
                        && piecesMap[selectedPiece.xPossition - 2, selectedPiece.yPossition] == null) {
                        canMoveMap[selectedPiece.xPossition - 2, selectedPiece.yPossition] = true;
                    }  

                } else {

                        canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition] = true;
                }

                if(OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition - 1) 
                    && piecesMap[selectedPiece.xPossition - 1, selectedPiece.yPossition - 1]!=null
                    && piecesMap[selectedPiece.xPossition - 1, selectedPiece.yPossition - 1].color
                    !=selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition - 1] = true;
                }

                if (OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition + 1) 
                    && piecesMap[selectedPiece.xPossition - 1, selectedPiece.yPossition + 1] != null
                    && piecesMap[selectedPiece.xPossition - 1, selectedPiece.yPossition + 1].color 
                        != selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition + 1] = true;
                }
            }

            if(piecesMap[selectedPiece.xPossition, selectedPiece.yPossition].color 
                == PieceColor.Black) {

                if(OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition) 
                    && piecesMap[selectedPiece.xPossition + 1, selectedPiece.yPossition]!=null) {

                } else if (selectedPiece.xPossition == 1) {

                    if (OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition) 
                        && piecesMap[selectedPiece.xPossition + 1, selectedPiece.yPossition] == null) {

                        canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition] = true;
                    }
                       
                    if (OnChessBoard(selectedPiece.xPossition + 2, selectedPiece.yPossition) 
                        && piecesMap[selectedPiece.xPossition + 2, selectedPiece.yPossition] == null) {

                        canMoveMap[selectedPiece.xPossition + 2, selectedPiece.yPossition] = true;
                    }
                        

                } else {

                        canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition] = true;
                }

                if (OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition - 1) 
                    && piecesMap[selectedPiece.xPossition + 1, selectedPiece.yPossition - 1] != null
                    && piecesMap[selectedPiece.xPossition + 1, selectedPiece.yPossition - 1].color 
                        != selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition - 1] = true;
                }

                if (OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition + 1) 
                    && piecesMap[selectedPiece.xPossition + 1, selectedPiece.yPossition + 1] != null
                    && piecesMap[selectedPiece.xPossition + 1, selectedPiece.yPossition + 1].color 
                        != selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition + 1] = true;
                }
            }
            
        }

        private void KnightMove(SelectedPiece selectedPiece, int newPossitionX,
            int newPossitionY, bool[,] canMoveMap, bool isKing, Piece[,] piecesMap) {

            int xPossition = selectedPiece.xPossition + newPossitionX;
            int yPossition = selectedPiece.yPossition + newPossitionY;

            if(OnChessBoard(xPossition,yPossition) 
                && piecesMap[xPossition, yPossition ] == null) {

                canMoveMap[xPossition, yPossition] = true;

            } else if(OnChessBoard(xPossition , yPossition) 
                && piecesMap[xPossition, yPossition].color
                != selectedPiece.piece.color) {

                if(isKing) {
                    pieceAttakingKing[xPossition, yPossition] = true;
                } else {
                    canMoveMap[xPossition, yPossition] = true;
                }
                
            }
        }

        private void DiagonalMove(SelectedPiece selectedPiece , int lenght,
            bool[,] canMoveMap, bool isKing, Piece[,] piecesMap) {

            
            for(int i = 1; i <= lenght; i++) {
                int xPossition = selectedPiece.xPossition + i;
                int yPossition = selectedPiece.yPossition + i ;
                
                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;

                } else if(OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {

                    break;

                } else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {

                    if(isKing){
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;

                    } else {

                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int xPossition = selectedPiece.xPossition + i ;
                int yPossition = selectedPiece.yPossition - i ;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {

                    canMoveMap[xPossition, yPossition] = true;
                } else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {

                    break;

                } else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {

                    if (isKing) {
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;

                    } else if(OnChessBoard(xPossition, yPossition)) {
                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int xPossition = selectedPiece.xPossition - i;
                int yPossition = selectedPiece.yPossition - i;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {

                    canMoveMap[xPossition, yPossition] = true;

                } else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {
                    if (isKing) {

                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;

                    } else {
                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {
                int xPossition = selectedPiece.xPossition - i;
                int yPossition = selectedPiece.yPossition + i;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                } else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {

                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {

                    if (isKing) {
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;
                    } else {

                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }
        }

        private void VerticalMove(SelectedPiece selectedPiece, int lenght, 
            bool[,] canMoveMap, bool isKing, Piece[,] piecesMap) {
            for (int i = 1; i <= lenght; i++)
            {
                int xPossition = selectedPiece.xPossition + i;
                int yPossition = selectedPiece.yPossition;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {

                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {
                    if (isKing) {
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;

                    } else {

                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int xPossition = selectedPiece.xPossition;
                int yPossition = selectedPiece.yPossition + i;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {
                    if (isKing) {
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;
                    } else {

                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int xPossition = selectedPiece.xPossition;
                int yPossition = selectedPiece.yPossition - i;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {

                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {

                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {

                    if (isKing) {
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;

                    } else {

                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int xPossition = selectedPiece.xPossition - i;
                int yPossition = selectedPiece.yPossition;

                if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition] == null) {

                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color == selectedPiece.piece.color) {

                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) 
                    && piecesMap[xPossition, yPossition].color != selectedPiece.piece.color) {

                    if (isKing) {
                        pieceAttakingKing[xPossition, yPossition] = true;
                        break;

                    } else {

                        canMoveMap[xPossition, yPossition] = true;
                        break;
                    }
                }
            }
        }

        private bool Move(int xPossition, int yPossition , SelectedPiece selectedPiece) {
            if (canMoveMap[xPossition, yPossition] == true) {

                if (chessBoard.board[xPossition, yPossition] != null) {

                    Destroy(pieceGameObjects[xPossition, yPossition]);
                }

                chessBoard.board[xPossition, yPossition] = 
                    chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition];

                pieceGameObjects[xPossition, yPossition] = 
                    pieceGameObjects[selectedPiece.xPossition, selectedPiece.yPossition];

                pieceGameObjects[xPossition, yPossition].transform.position =
                    new Vector3(xPossition + 0.5f, 0.5f, yPossition + 0.5f);

                chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition] = null;

                return true;
            }

            return false;
        }

  

        private bool [,]  HiddenCheck(SelectedPiece selectedPiece, bool [,] canMoveMap) {

            PieceColor whoseMove;
            Piece[,] cloneChessBoard = (Piece[,])chessBoard.board.Clone();
            bool[,] cloneCanMoveMap = (bool[,])canMoveMap.Clone();

            if (this.whoseMove == PieceColor.White) {
                whoseMove = PieceColor.White;
            } else {
                whoseMove = PieceColor.Black;
            }

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {

                    if (cloneCanMoveMap[i, j]) {

                        cloneChessBoard[i, j] = 
                            cloneChessBoard[selectedPiece.xPossition, selectedPiece.yPossition];

                        cloneChessBoard[selectedPiece.xPossition, selectedPiece.yPossition] = null;
                        if (CheckKing(whoseMove, cloneChessBoard)) {
                           
                            cloneCanMoveMap[i, j] = false;
                        }
                        cloneChessBoard[selectedPiece.xPossition, selectedPiece.yPossition] =
                            cloneChessBoard[i, j];

                        cloneChessBoard[i, j] = null;
                    }
                }
            }
            return cloneCanMoveMap;
        }

        private bool CheckKing(PieceColor whoseMove, Piece[,] piecesMap) {

            ClearCanMoveMap(checkKingMap);
            for (int i = 0; i < 8; i++){
                for(int j = 0; j < 8; j++){
                    if (piecesMap[i, j] != null && piecesMap[i, j].type == PieceType.King
                        && piecesMap[i, j].color == whoseMove) {

                        xKingPossition = i;
                        yKingPossition = j;
                    }
                }
            }

            SelectedPiece selectedKing = new SelectedPiece();
            selectedKing.piece = piecesMap[xKingPossition, yKingPossition];
            selectedKing.xPossition = xKingPossition;
            selectedKing.yPossition = yKingPossition;

            DiagonalMove(selectedKing, 7, checkKingMap, true, piecesMap);
            VerticalMove(selectedKing, 7, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, 2, 1, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, 2, -1, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, 1, 2, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, -1, 2, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, -2, 1, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, 1, -2, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, -2, -1, checkKingMap, true, piecesMap);
            KnightMove(selectedKing, -1, -2, checkKingMap, true, piecesMap);


            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (pieceAttakingKing[i,j]) {
                        SelectedPiece selected = new SelectedPiece();
                        selected.piece = chessBoard.board[i, j];
                        selected.xPossition = i;
                        selected.yPossition = j;
                        GetCanMoveMapForPiece(selected, canMoveMapForKing,  piecesMap);
                    }
                }
            }

            ClearCanMoveMap(pieceAttakingKing);
            
            if (canMoveMapForKing[xKingPossition, yKingPossition] == true) {
                ClearCanMoveMap(canMoveMapForKing);
                return true;
            } else {
                return false;
            }

        }

        private bool CheckMate() {

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {

                    if (chessBoard.board[i, j] != null 
                        && chessBoard.board[i, j].color == whoseMove) {

                        SelectedPiece piece = new SelectedPiece();
                        piece.piece = chessBoard.board[i, j];
                        piece.xPossition = i;
                        piece.yPossition = j;

                        ClearCanMoveMap(canMoveMap);
                        GetCanMoveMapForPiece(piece, canMoveMap, chessBoard.board);
                        canMoveMap = HiddenCheck(piece, canMoveMap);
                        if (CheckingForFullness(canMoveMap)) {

                            return false;
                        }
                    }
                }
            }
            return true;
        
        }

        private bool OnChessBoard(int i, int j) {
            if (i > 7 || i < 0 || j > 7 || j < 0) {
                return false;
            }

            return true;
        }

        private void ShowCanMoveCells(bool [,] board) {
            for (int i = 0; i < 8; i++) { 
                for(int j = 0; j < 8; j++) {
                    if(canMoveMap[i,j] == true) {
                       if(chessBoard.board[i, j] != null) {

                            canMoveCell.transform.localScale = new Vector3(1f, 0.01f, 1f);
                            canMoveCells.Add(Instantiate(canMoveCell, 
                                new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity));

                            canMoveCell.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                       }

                       canMoveCells.Add(Instantiate(canMoveCell, 
                        new Vector3(i + 0.5f, 0.5f, j + 0.5f ), Quaternion.identity));
                    }
                }
            }
        }

        private void RemoveCanMoveCells(){
            
            foreach(GameObject cell in canMoveCells) {
                Destroy(cell);
            }
        }

        private void ClearCanMoveMap(bool [,] canMoveMap) {

            for(int i = 0; i < 8; i++){
                for(int j = 0; j < 8; j++){
                    canMoveMap[i, j] = false;
                }
            }
        }

        private void ChangeMove() {
            
            if(whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }
        }

        private bool CheckingForFullness(bool[,] array) {

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {

                    if (array[i, j]) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DestroyCheckCell() {

            Destroy(chekCell);
        }


    }

}

