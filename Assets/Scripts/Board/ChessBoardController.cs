using System;
using System.Collections.Generic;
using UnityEngine;
using piece;
using chess;

namespace board {  
    public class ChessBoardController : MonoBehaviour {
        private ChessBoard chessBoard;

        public GameObject boardObj;

        private int xKingPosition;
        private int yKingPosition;
        private bool isCheck;

        private int xPosition;
        private int yPosition;
        private float boardOffsetX;
        private float boardOffsetY;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        private SelectedPiece? selectedPiece;

        public GameObject canMoveCell;
        public GameObject check;
        private GameObject checkCell;
        private List<GameObject> canMoveCells = new List<GameObject>();

        public List<GameObject> pieceList = new List<GameObject>();
        private GameObject[,] pieceGameObjects = new GameObject[8, 8];
        private bool[,] canMoveMap = new bool[8, 8];
        private bool[,] checkKingMap = new bool[8, 8];
        private bool[,] pieceAttakingKing = new bool[8, 8];
        private bool[,] test = new bool[8, 8];

        private void Start() {
            boardOffsetX = boardObj.transform.position.x - 4;
            boardOffsetY = boardObj.transform.position.z - 4;

            chessBoard = new ChessBoard();
            pieceGameObjects =  AddPiecesOnBoard(pieceGameObjects);
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    xPosition = (int)(hit.point.x - (boardObj.transform.position.x - 4));
                    yPosition = (int)(hit.point.z - (boardObj.transform.position.z - 4));

                    if (selectedPiece == null && Chess.SelectPiece(xPosition, yPosition,
                        chessBoard.board, whoseMove) != null) {
                        ClearCanMoveMap(canMoveMap);
                        RemoveCanMoveCells();

                        selectedPiece = Chess.SelectPiece(xPosition, yPosition,
                            chessBoard.board, whoseMove);
                        Debug.Log(chessBoard.board[xPosition, yPosition].type);
                        canMoveMap = Chess.GetCanMoveMapForPiece(
                            (SelectedPiece)selectedPiece,
                            canMoveMap, 
                            chessBoard.board
                        );
                        canMoveMap = HiddenCheck((SelectedPiece)selectedPiece, canMoveMap);
                        ShowCanMoveCells(canMoveMap);

                    } else if (selectedPiece != null) {
                        RemoveCanMoveCells();
                        if (Move(xPosition, yPosition, (SelectedPiece)selectedPiece)) {
                            DestroyCheckCell();
                            ChangeMove();
                            if (CheckKing(whoseMove, chessBoard.board)) {
                                isCheck = true;
                                checkCell = Instantiate(check, new
                                    Vector3(xKingPosition + 0.5f, 0.5f,
                                    yKingPosition + 0.5f), Quaternion.identity);
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

        private GameObject[,] AddPiecesOnBoard(GameObject[,] pieceGameObjects) {
            var board = chessBoard.board;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j] != null) {
                        pieceGameObjects[i, j] = Instantiate(
                            pieceList[(int)board[i, j].type + (int)board[i, j].color],
                            new Vector3(
                                i + boardOffsetX + 0.5f, 
                                0.5f, 
                                j + boardOffsetY + 0.5f
                            ), 
                            Quaternion.identity, boardObj.transform);
                    }
                }
            }
            return pieceGameObjects;
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
                    new Vector3(xPossition + boardOffsetX + 0.5f, 0.5f,
                        yPossition + boardOffsetY + 0.5f);

                chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition] = null;

                return true;
            }
            return false;
        }

  

        private bool [,]  HiddenCheck(SelectedPiece selectedPiece, bool [,] canMoveMap) {
            PieceColor whoseMove;
            Piece[,] cloneChessBoard = (Piece[,])chessBoard.board.Clone();
            bool[,] cloneCanMoveMap = (bool[,])canMoveMap.Clone();

            int x = selectedPiece.xPossition;
            int y = selectedPiece.yPossition;

            if (this.whoseMove == PieceColor.White) {
                whoseMove = PieceColor.White;
            } else {
                whoseMove = PieceColor.Black;
            }
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (cloneCanMoveMap[i, j]) {
                        cloneChessBoard[i, j] = cloneChessBoard[x, y];
                        cloneChessBoard[x, y] = null;
                        if (CheckKing(whoseMove, cloneChessBoard)) {
                            cloneCanMoveMap[i, j] = false;
                        }
                        cloneChessBoard[x, y] = cloneChessBoard[i, j];
                        cloneChessBoard[i, j] = null;
                    }
                }
            }
            return cloneCanMoveMap;
        }

        private bool CheckKing(PieceColor whoseMove, Piece[,] piecesMap) {
            pieceAttakingKing = Chess.pieceAttakingKing;
            ClearCanMoveMap(checkKingMap);
            for (int i = 0; i < 8; i++){
                for(int j = 0; j < 8; j++){
                    if (piecesMap[i, j] != null && piecesMap[i, j].type == PieceType.King
                        && piecesMap[i, j].color == whoseMove) {

                        xKingPosition = i;
                        yKingPosition = j;
                    }
                }
            }

            SelectedPiece selectedKing = new SelectedPiece();
            selectedKing.piece = piecesMap[xKingPosition, yKingPosition];
            selectedKing.xPossition = xKingPosition;
            selectedKing.yPossition = yKingPosition;

            Chess.DiagonalMove(selectedKing, 7, checkKingMap, true, piecesMap);
            Chess.VerticalMove(selectedKing, 7, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, 2, 1, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, 2, -1, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, 1, 2, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, -1, 2, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, -2, 1, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, 1, -2, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, -2, -1, checkKingMap, true, piecesMap);
            Chess.KnightMove(selectedKing, -1, -2, checkKingMap, true, piecesMap);

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (pieceAttakingKing[i,j]) {
                        SelectedPiece selected = new SelectedPiece();
                        selected.piece = chessBoard.board[i, j];
                        selected.xPossition = i;
                        selected.yPossition = j;
                        Chess.GetCanMoveMapForPiece(selected, test,  piecesMap);
                    }
                }
            }

            ClearCanMoveMap(pieceAttakingKing);
            
            if (test[xKingPosition, yKingPosition] == true) {
                ClearCanMoveMap(test);
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

                        //ClearCanMoveMap(canMoveMap);
                        Chess.GetCanMoveMapForPiece(piece, canMoveMap, chessBoard.board);
                        canMoveMap = HiddenCheck(piece, canMoveMap);
                        if (CheckingForFullness(canMoveMap)) {

                            return false;
                        }
                    }
                }
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
                                new Vector3(i + boardOffsetX + 0.5f, 0.5f,
                                j + boardOffsetY + 0.5f), Quaternion.identity));

                            canMoveCell.transform.localScale = 
                                new Vector3(0.2f, 0.01f, 0.2f);
                       }
                       canMoveCells.Add(Instantiate(canMoveCell, 
                       new Vector3(i + boardOffsetX + 0.5f, 0.5f,
                       j + boardOffsetY + 0.5f ),
                       Quaternion.identity));
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
            Destroy(checkCell);
        }


    }

}

