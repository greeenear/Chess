using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace board {

    public enum PieceType{
        Bishop,
        King,
        Knight,
        Pawn,
        Queen,
        Rook,
    }

    public enum PieceColor
    {
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

        private int xPossition;
        private int yPossition;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        private bool isCheck;

        private Nullable<SelectedPiece> selectedPiece;

        public GameObject canMoveCell;
        public GameObject check;
        private GameObject chekCell;
        private List<GameObject> canMoveCells = new List<GameObject>();

        public List<GameObject> pieceList = new List<GameObject>();
        private GameObject[,] pieceGameObjects = new GameObject[8, 8];
        private bool[,] canMoveMap = new bool[8, 8];

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

                        ClearCanMoveMap();
                        RemoveCanMoveCells();
                        selectedPiece = SelectPiece(xPossition, yPossition);
                        GetCanMoveMapForPiece((SelectedPiece)selectedPiece);
                        ShowCanMoveCells();
                    }
                    else if (selectedPiece != null) {
                        
                        if(isCheck) {
                            DestroyCheckCell();
                        }
                        RemoveCanMoveCells();
                        if (Move(xPossition, yPossition, (SelectedPiece)selectedPiece)) {
                            ChangeMove();
                            if (Check()) {
                                isCheck = true;
                            }
                        }
                        selectedPiece = null;
                    }

                }
            }
        }
        

        private void AddPiecesOnBoard() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++)
                {
                    if (chessBoard.board[i, j] != null)
                    {
                        if (chessBoard.board[i, j].type == PieceType.Pawn)
                        {
                            if (chessBoard.board[i, j].color == PieceColor.White)
                            {

                                pieceGameObjects[i, j] = Instantiate(pieceList[7],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                            else
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[6],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Bishop)
                        {
                            if (chessBoard.board[i, j].color == PieceColor.White)
                            {

                                pieceGameObjects[i, j] = Instantiate(pieceList[1],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                            else
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[0],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Knight)
                        {
                            if (chessBoard.board[i, j].color == PieceColor.White)
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[5],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                            else
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[4],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.King)
                        {
                            if (chessBoard.board[i, j].color == PieceColor.White)
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[3],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                            else
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[2],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Queen)
                        {
                            if (chessBoard.board[i, j].color == PieceColor.White)
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[9],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                            else
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[8],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                        }

                        if (chessBoard.board[i, j].type == PieceType.Rook)
                        {
                            if (chessBoard.board[i, j].color == PieceColor.White)
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[11],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
                            }
                            else
                            {
                                pieceGameObjects[i, j] = Instantiate(pieceList[10],
                                    new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
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

        private void GetCanMoveMapForPiece(SelectedPiece selectedPiece) {
            switch (selectedPiece.piece.type) {
                case PieceType.Pawn:
                    PawnMove(selectedPiece);
                    break;

                case PieceType.Bishop:
                    DiagonalMove(selectedPiece, 7);
                    break;

                case PieceType.Rook:
                    VerticalMove(selectedPiece, 7);
                    break;

                case PieceType.Queen:
                    DiagonalMove(selectedPiece, 7);
                    VerticalMove(selectedPiece, 7);
                    break;

                case PieceType.King:
                    DiagonalMove(selectedPiece, 1);
                    VerticalMove(selectedPiece, 1);
                    break;

                case PieceType.Knight:
                    KnightMove(selectedPiece, 2,1);
                    KnightMove(selectedPiece, 2, -1);
                    KnightMove(selectedPiece, 1, 2);
                    KnightMove(selectedPiece, -1, 2);
                    KnightMove(selectedPiece, -2, 1);
                    KnightMove(selectedPiece, 1, -2);
                    KnightMove(selectedPiece, -2, -1);
                    KnightMove(selectedPiece, -1, -2);
                    break;
            }
        }

        private void PawnMove(SelectedPiece selectedPiece) {

        if (chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition].color == PieceColor.White) {

                if (OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition) 
                    && chessBoard.board[selectedPiece.xPossition - 1, selectedPiece.yPossition] != null)
                {

                }else if (selectedPiece.xPossition == 6) {

                    if (OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition) 
                        && chessBoard.board[selectedPiece.xPossition - 1, selectedPiece.yPossition] == null) {

                        canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition] = true;
                    }
                        
                    if (OnChessBoard(selectedPiece.xPossition - 2, selectedPiece.yPossition) 
                        && chessBoard.board[selectedPiece.xPossition - 2, selectedPiece.yPossition] == null) {
                        canMoveMap[selectedPiece.xPossition - 2, selectedPiece.yPossition] = true;
                    }  

                } else {

                        canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition] = true;
                }

                if(OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition - 1) 
                    && chessBoard.board[selectedPiece.xPossition - 1, selectedPiece.yPossition - 1]!=null
                    && chessBoard.board[selectedPiece.xPossition - 1, selectedPiece.yPossition - 1].color!=selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition - 1] = true;
                }

                if (OnChessBoard(selectedPiece.xPossition - 1, selectedPiece.yPossition + 1) 
                    && chessBoard.board[selectedPiece.xPossition - 1, selectedPiece.yPossition + 1] != null
                    && chessBoard.board[selectedPiece.xPossition - 1, selectedPiece.yPossition + 1].color != selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition - 1, selectedPiece.yPossition + 1] = true;
                }

            }
            if(chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition].color == PieceColor.Black) {

                if(OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition) 
                    && chessBoard.board[selectedPiece.xPossition + 1, selectedPiece.yPossition]!=null) {

                } else if (selectedPiece.xPossition == 1) {

                    if (OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition) 
                        && chessBoard.board[selectedPiece.xPossition + 1, selectedPiece.yPossition] == null) {

                        canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition] = true;
                    }
                       
                    if (OnChessBoard(selectedPiece.xPossition + 2, selectedPiece.yPossition) 
                        && chessBoard.board[selectedPiece.xPossition + 2, selectedPiece.yPossition] == null) {

                        canMoveMap[selectedPiece.xPossition + 2, selectedPiece.yPossition] = true;
                    }
                        

                } else {

                        canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition] = true;
                }

                if (OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition - 1) 
                    && chessBoard.board[selectedPiece.xPossition + 1, selectedPiece.yPossition - 1] != null
                    && chessBoard.board[selectedPiece.xPossition + 1, selectedPiece.yPossition - 1].color != selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition - 1] = true;
                }

                if (OnChessBoard(selectedPiece.xPossition + 1, selectedPiece.yPossition + 1) 
                    && chessBoard.board[selectedPiece.xPossition + 1, selectedPiece.yPossition + 1] != null
                    && chessBoard.board[selectedPiece.xPossition + 1, selectedPiece.yPossition + 1].color != selectedPiece.piece.color) {

                    canMoveMap[selectedPiece.xPossition + 1, selectedPiece.yPossition + 1] = true;
                }
            }
            
        }

        private void KnightMove(SelectedPiece selectedPiece, int newPossitionX , int newPossitionY) {

            int xPossition = selectedPiece.xPossition + newPossitionX;
            int yPossition = selectedPiece.yPossition + newPossitionY;

            if(OnChessBoard(xPossition,yPossition) && chessBoard.board[xPossition, yPossition ] == null) {

                canMoveMap[xPossition, yPossition] = true;
            } else if(OnChessBoard(xPossition , yPossition) && chessBoard.board[xPossition, yPossition].color
                != selectedPiece.piece.color) {
                canMoveMap[xPossition, yPossition] = true;
            }


        }

        private void DiagonalMove(SelectedPiece selectedPiece , int lenght) {

            for(int i = 1; i <= lenght; i++) {
                int xPossition = selectedPiece.xPossition + i;
                int yPossition = selectedPiece.yPossition + i ;

                if (OnChessBoard(xPossition, yPossition) &&chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;

                } else if(OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;

                } else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }

            for (int i = 1; i <= lenght; i++)
            {
                int xPossition = selectedPiece.xPossition + i ;
                int yPossition = selectedPiece.yPossition - i ;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }

            for (int i = 1; i <= lenght; i++)
            {
                int xPossition = selectedPiece.xPossition - i;
                int yPossition = selectedPiece.yPossition - i;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }

            for (int i = 1; i <= lenght; i++)
            {
                int xPossition = selectedPiece.xPossition - i;
                int yPossition = selectedPiece.yPossition + i;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {

                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }
        }

        private void VerticalMove(SelectedPiece selectedPiece, int lenght) {
            for (int i = 1; i <= lenght; i++)
            {
                int xPossition = selectedPiece.xPossition + i;
                int yPossition = selectedPiece.yPossition;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int xPossition = selectedPiece.xPossition;
                int yPossition = selectedPiece.yPossition + i;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int xPossition = selectedPiece.xPossition;
                int yPossition = selectedPiece.yPossition - i;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }

            for (int i = 1; i <= lenght; i++) {

                int xPossition = selectedPiece.xPossition - i;
                int yPossition = selectedPiece.yPossition;

                if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition] == null) {
                    canMoveMap[xPossition, yPossition] = true;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color == selectedPiece.piece.color) {
                    break;
                }

                else if (OnChessBoard(xPossition, yPossition) && chessBoard.board[xPossition, yPossition].color != selectedPiece.piece.color) {
                    canMoveMap[xPossition, yPossition] = true;
                    break;
                }
            }
        }

        private bool Move(int xPossition, int yPossition , SelectedPiece selectedPiece) {
            if (canMoveMap[xPossition, yPossition] == true) {
                
                if(chessBoard.board[xPossition, yPossition]!=null) {
                    Destroy(pieceGameObjects[xPossition, yPossition]);
                }

                chessBoard.board[xPossition, yPossition] = chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition];
                pieceGameObjects[xPossition, yPossition] = pieceGameObjects[selectedPiece.xPossition, selectedPiece.yPossition];
                pieceGameObjects[xPossition, yPossition].transform.position = new Vector3(xPossition+0.5f,0.5f,yPossition+0.5f);
                chessBoard.board[selectedPiece.xPossition, selectedPiece.yPossition] = null;

                return true;
            }

            return false;
        }


        private bool Check() {
            
            int kingPossitionX = -1;
            int kingPossitionY = -1;
            bool[,] checkArray = new bool[8, 8];
          
            for (int i = 0; i < 8; i++) {
                for(int j = 0; j < 8; j++) {

                    if (chessBoard.board[i, j]!= null && chessBoard.board[i, j].color != whoseMove){

                        SelectedPiece selected = new SelectedPiece();
                        selected.piece = chessBoard.board[i, j];
                        selected.xPossition = i;
                        selected.yPossition = j;
                        GetCanMoveMapForPiece(selected);
                    }

                    if (chessBoard.board[i, j] != null && chessBoard.board[i, j].type == PieceType.King
                        && chessBoard.board[i, j].color == whoseMove) {
                        kingPossitionX = i;
                        kingPossitionY = j;
                    }
                }
            }

            if(kingPossitionX != -1 && canMoveMap[kingPossitionX, kingPossitionY] == true) {

                chekCell = Instantiate(check, new Vector3(kingPossitionX + 0.5f, 0.5f, kingPossitionY + 0.5f), Quaternion.identity);
                return true;

            } else {

                return false;
            }
        }


        private bool OnChessBoard(int i, int j)
        {
            if (i > 7 || i < 0)
            {
                return false;
            }
            if (j > 7 || j < 0)
            {
                return false;
            }
            return true;
        }

        private void ShowCanMoveCells() {
            for (int i = 0; i < 8; i++) { 
                for(int j = 0; j < 8; j++) {
                    if(canMoveMap[i,j] == true) {
                       if(chessBoard.board[i, j] != null) {

                            canMoveCell.transform.localScale = new Vector3(1f, 0.01f, 1f);
                            canMoveCells.Add(Instantiate(canMoveCell, new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity));
                            canMoveCell.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                       }

                       canMoveCells.Add(Instantiate(canMoveCell, new Vector3(i + 0.5f, 0.5f, j + 0.5f ), Quaternion.identity));
                    }
                }
            }
        }

        private void RemoveCanMoveCells(){
            
            foreach(GameObject cell in canMoveCells) {
                Destroy(cell);
            }
        }

        private void ClearCanMoveMap() {
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

        private void DestroyCheckCell() {

            Destroy(chekCell);
        }


    }


    


}

