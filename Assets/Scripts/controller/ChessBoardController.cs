using System.Collections.Generic;
using UnityEngine;
using chess;
using resource;

namespace controller {  
    public class ChessBoardController : MonoBehaviour {
        private ChessBoard chessBoard;

        public GameObject boardObj;
        private float boardOffsetX;
        private float boardOffsetY;

        private int xPosition;
        private int yPosition;
        Position selectedPosition;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        public GameObject canMoveCell;

        private List<GameObject> canMoveCells = new List<GameObject>();

        private GameObject[,] pieceGameObjects = new GameObject[8, 8];

        private List<Position> canMovePositions = new List<Position>();

        private void Start() {
            boardOffsetX = boardObj.transform.position.x - 4;
            boardOffsetY = boardObj.transform.position.z - 4;

            chessBoard = new ChessBoard();
            AddPiecesOnBoard(pieceGameObjects, gameObject.GetComponent<Resource>().pieceList);
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    xPosition = (int)(hit.point.x - (boardObj.transform.position.x - 4));
                    yPosition = (int)(hit.point.z - (boardObj.transform.position.z - 4));

                    var piece = chessBoard.board[xPosition, yPosition];

                    if (piece != null && piece.color == whoseMove) {
                        RemoveCanMoveCells();
                        canMovePositions.Clear();

                        selectedPosition = new Position(xPosition, yPosition);
                        canMovePositions = Chess.GetCanMoveMapForPiece(
                            piece,
                            selectedPosition,
                            chessBoard.board,
                            canMovePositions);
                        ShowCanMoveCells(canMovePositions);

                    } else {
                        RemoveCanMoveCells();

                        if(Move(selectedPosition,
                            new Position(xPosition, yPosition), canMovePositions)) {
                            ChangeMove();
                        }
                        canMovePositions.Clear();
                    }
                }
            }
        }

        private void AddPiecesOnBoard(
            GameObject[,] pieceGameObjects,
            List<GameObject> pieceList
        ) {
            var board = chessBoard.board;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j] != null) {
                        pieceGameObjects[i, j] = Instantiate(
                            pieceList[(int)board[i, j].type * 2 + (int)board[i, j].color],
                            new Vector3(
                                i + boardOffsetX + 0.5f,
                                0.5f, 
                                j + boardOffsetY + 0.5f
                            ), 
                            Quaternion.identity, boardObj.transform);
                    }
                }
            }
        }

        public bool Move(Position start, Position end, List<Position> canMovePositions) {
            foreach(var pos in canMovePositions) {
                if(pos.x == end.x && pos.y == end.y) {
                    if(chessBoard.board[end.x, end.y] != null) {
                        Destroy(pieceGameObjects[end.x, end.y]);
                    }
                    chessBoard.board[end.x, end.y] = chessBoard.board[start.x, start.y];
                    chessBoard.board[start.x, start.y] = null;
                    pieceGameObjects[end.x, end.y] = pieceGameObjects[start.x, start.y];

                    pieceGameObjects[end.x, end.y].transform.position =
                    new Vector3(
                        xPosition + boardOffsetX + 0.5f,
                        0.5f,
                        yPosition + boardOffsetY + 0.5f);
                    return true;
                }
            }
            return false;
        }
        private void RemoveCanMoveCells(){
            foreach(GameObject cell in canMoveCells) {
                Destroy(cell);
            }
        }

        private void ChangeMove() {     
            if(whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }
        }

        private void ShowCanMoveCells(List<Position> canMovePositions) {
            foreach (var pos in canMovePositions) {
                canMoveCells.Add(Instantiate(
                    canMoveCell,
                    new Vector3(pos.x + boardOffsetX + 0.5f, 0.5f,
                    pos.y + boardOffsetY + 0.5f), Quaternion.identity)
                );
            }
        }
    }
}

