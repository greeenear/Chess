using System.Collections.Generic;
using UnityEngine;
using piece;
using chess;

namespace board {  
    public class ChessBoardController : MonoBehaviour {
        private ChessBoard chessBoard;

        public GameObject boardObj;
        private float boardOffsetX;
        private float boardOffsetY;

        private int xKingPosition;
        private int yKingPosition;
        private bool isCheck;

        private bool isCastling;

        private int xPosition;
        private int yPosition;

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
        private bool[,] KingsAttackMap = new bool[8, 8];

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
                    if ( Chess.SelectPiece(xPosition, yPosition,
                        chessBoard.board, whoseMove) != null) {
                        ClearCanMoveMap(canMoveMap);
                        RemoveCanMoveCells();

                        selectedPiece = Chess.SelectPiece(xPosition, yPosition,
                            chessBoard.board, whoseMove);
                        canMoveMap = Chess.GetCanMoveMapForPiece(
                            (SelectedPiece)selectedPiece,
                            canMoveMap, 
                            chessBoard.board
                        );
                        canMoveMap = HiddenCheck((SelectedPiece)selectedPiece, canMoveMap);

                        var cast = CheckCastling((SelectedPiece)selectedPiece, canMoveMap);
                        if(cast.flag){
                            canMoveMap = cast.map;
                            isCastling = cast.flag;
                        }
                        ShowCanMoveCells(canMoveMap);
                    } else if (selectedPiece != null) {
                        RemoveCanMoveCells();
                        if (Move(xPosition, yPosition, (SelectedPiece)selectedPiece)) {
                            DestroyCheckCell();
                            ChangeMove();
                            if (CheckKing(whoseMove, chessBoard.board).flag) {
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
                        isCastling = false;
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

 
        private bool Move(int xPosition, int yPosition , SelectedPiece selectedPiece) {
            var x = selectedPiece.xPosition;
            var y = selectedPiece.yPosition;

            if (canMoveMap[xPosition, yPosition] == true) {
                if (isCastling && selectedPiece.piece.type == PieceType.King) {
                    if (Castling(xPosition, yPosition, selectedPiece)) {
                        Debug.Log("+");
                        return true;
                    }
                }
                if (selectedPiece.piece.type == PieceType.Pawn) {
                    if (chessBoard.board[xPosition, yPosition] == null 
                    && y != yPosition) {
                        chessBoard.board[x, yPosition] = null;
                        Destroy(pieceGameObjects[x, yPosition]);
                    }
                }
                if (chessBoard.board[xPosition, yPosition] != null) {
                    Destroy(pieceGameObjects[xPosition, yPosition]);
                }
                chessBoard.board[xPosition, yPosition] = chessBoard.board[x, y];
                chessBoard.board[x, y].moveCount++;
                pieceGameObjects[xPosition, yPosition] = pieceGameObjects[x, y];
                pieceGameObjects[xPosition, yPosition].transform.position =
                    new Vector3(xPosition + boardOffsetX + 0.5f, 0.5f,
                        yPosition + boardOffsetY + 0.5f);

                chessBoard.board[x, y] = null;
                return true;
            }
            return false;
        }

        private OutInfo CheckCastling(SelectedPiece king, bool[,] canMoveMap) {
            var checkInfo = CheckKing(whoseMove, chessBoard.board);
          
  
            var x = king.xPosition;
            var y = king.yPosition;

            OutInfo outInfo = new OutInfo();

            if(king.piece.type == PieceType.King && king.piece.moveCount == 0) {
                if (chessBoard.board[x, y + 1] == null && !checkInfo.map[x, y + 1] &&
                chessBoard.board[x, y + 2] == null && !checkInfo.map[x, y + 2]
                && chessBoard.board[x, y + 3] != null 
                && chessBoard.board[x, y + 3].type == PieceType.Rook
                && chessBoard.board[x, y + 3].moveCount == 0) {
                    canMoveMap[king.xPosition, king.yPosition + 2] = true;
                    outInfo.flag = true;
                }
                if (chessBoard.board[x, y - 1] == null && !checkInfo.map[x, y - 1] &&
                chessBoard.board[x, y - 2] == null && !checkInfo.map[x, y - 2]
                && chessBoard.board[x, y - 3] == null 
                && !checkInfo.map[x, y - 3]
                && chessBoard.board[x, y - 4] != null 
                && chessBoard.board[x, y - 4].type == PieceType.Rook
                && chessBoard.board[x, y - 4].moveCount == 0) {
                    canMoveMap[king.xPosition, king.yPosition - 2] = true;
                    outInfo.flag = true;
                }
            }
            canMoveMap = HiddenCheck(king, canMoveMap);
            outInfo.map = canMoveMap;
            return outInfo;
        }

        private bool Castling(int x, int y, SelectedPiece selectedPiece) {
            var oldPosX = selectedPiece.xPosition;
            var oldPosY = selectedPiece.yPosition;

            int startRookPos;
            int endRookPos;
            if (y - selectedPiece.yPosition > 0) {
                startRookPos = 7;
                endRookPos = 5;
            } else {
                startRookPos = 0;
                endRookPos = 3;
            }
            if (Mathf.Abs(y - selectedPiece.yPosition) == 2) {
                chessBoard.board[oldPosX, oldPosY].moveCount++;
                chessBoard.board[x, y] = chessBoard.board[oldPosX, oldPosY];
                pieceGameObjects[x, y] = pieceGameObjects[oldPosX, oldPosY];
                pieceGameObjects[oldPosX, oldPosY].transform.position =
                    new Vector3(x + boardOffsetX + 0.5f, 0.5f,
                        y + boardOffsetY + 0.5f);
                
                chessBoard.board[x, endRookPos] = chessBoard.board[x, startRookPos];
                pieceGameObjects[x, endRookPos] = pieceGameObjects[x, startRookPos];
                pieceGameObjects[x, startRookPos].transform.position =
                    new Vector3(x + boardOffsetX + 0.5f, 0.5f,
                        endRookPos + boardOffsetY + 0.5f);
                chessBoard.board[x, startRookPos] = null;
                chessBoard.board[oldPosX, oldPosY] = null;
                return true;
            }
            return false;
        }

        private bool [,]  HiddenCheck(SelectedPiece selectedPiece, bool [,] canMoveMap) {
            PieceColor whoseMove;
            Piece[,] cloneChessBoard = (Piece[,])chessBoard.board.Clone();
            bool[,] cloneCanMoveMap = (bool[,])canMoveMap.Clone();

            int x = selectedPiece.xPosition;
            int y = selectedPiece.yPosition;

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
                        if (CheckKing(whoseMove, cloneChessBoard).flag) {
                            cloneCanMoveMap[i, j] = false;
                        }
                        cloneChessBoard[x, y] = cloneChessBoard[i, j];
                        cloneChessBoard[i, j] = null;
                    }
                }
            }
            return cloneCanMoveMap;
        }

        private OutInfo CheckKing(PieceColor whoseMove, Piece[,] piecesMap) {
            ClearCanMoveMap(KingsAttackMap);
            OutInfo checkInfo = new OutInfo();
            bool[,] checkKingMap = new bool[8, 8];
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
            selectedKing.xPosition = xKingPosition;
            selectedKing.yPosition = yKingPosition;

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
                    if (Chess.pieceAttakingKing[i,j]) {
                        SelectedPiece selected = new SelectedPiece();
                        selected.piece = chessBoard.board[i, j];
                        selected.xPosition = i;
                        selected.yPosition = j;
                        Chess.GetCanMoveMapForPiece(selected, KingsAttackMap,  piecesMap);
                    }
                }
            }
            ClearCanMoveMap(Chess.pieceAttakingKing);
            if (KingsAttackMap[xKingPosition, yKingPosition] == true) {
                checkInfo.flag = true;
                checkInfo.map = KingsAttackMap;
                return checkInfo;
            } else {
                checkInfo.flag = false;
                checkInfo.map = KingsAttackMap;
                return checkInfo;
            }

        }

        private bool CheckMate() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (chessBoard.board[i, j] != null 
                        && chessBoard.board[i, j].color == whoseMove) {
                        SelectedPiece piece = new SelectedPiece();
                        piece.piece = chessBoard.board[i, j];
                        piece.xPosition = i;
                        piece.yPosition = j;

                        ClearCanMoveMap(canMoveMap);
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

