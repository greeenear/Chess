using System.Collections.Generic;
using UnityEngine;
using chess;
using resource;

namespace controller {  

    public class ChessBoardController : MonoBehaviour {
        private ChessBoard chessBoard;

        public GameObject boardObj;

        private int xPosition;
        private int yPosition;
        Position selectedPosition;
        bool isCheck;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        public GameObject canMoveCell;
        public GameObject checkKingCell;

        private List<GameObject> canMoveCells = new List<GameObject>();

        private GameObject[,] pieceGameObjects = new GameObject[8, 8];

        private List<Position> canMovePositions = new List<Position>();

        private void Start() {
            chessBoard = new ChessBoard();
                AddPiecesOnBoard(
                pieceGameObjects,
                gameObject.GetComponent<Resource>().pieceList);
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
                            selectedPosition,
                            chessBoard.board,
                            canMovePositions);
                        canMovePositions = HiddenCheck(canMovePositions, selectedPosition);
                        ShowCanMoveCells(canMovePositions);

                    } else {
                        RemoveCanMoveCells();

                        if (Move(selectedPosition,
                            new Position(xPosition, yPosition), canMovePositions)) {
                            ChangeMove();
                            if (CheckMate()) {
                                Debug.Log("CheckMate");
                            }

                            if (CheckKing(chessBoard.board)) {
                                Debug.Log("Check");
                            }
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
                                i + boardObj.transform.position.x - 4 + 0.5f,
                                0.5f, 
                                j + boardObj.transform.position.z - 4 + 0.5f
                            ), 
                            Quaternion.identity, boardObj.transform);
                    }
                }
            }
        }

        public bool Move(Position start, Position end, List<Position> canMovePositions) {
            foreach (var pos in canMovePositions) {
                if (pos.x == end.x && pos.y == end.y) {
                    if (chessBoard.board[end.x, end.y] != null) {
                        Destroy(pieceGameObjects[end.x, end.y]);
                    }
                    chessBoard.board[end.x, end.y] = chessBoard.board[start.x, start.y];
                    chessBoard.board[start.x, start.y] = null;
                    pieceGameObjects[end.x, end.y] = pieceGameObjects[start.x, start.y];

                    pieceGameObjects[end.x, end.y].transform.position =
                    new Vector3(
                        xPosition + boardObj.transform.position.x - 4 + 0.5f,
                        0.5f,
                        yPosition + boardObj.transform.position.z - 4 + 0.5f);
                    return true;
                }
            }
            return false;
        }

        private bool CheckKing(Piece[,] board) {
            Position kingPosition = Chess.FindKing(board, whoseMove).Value;

            List<Position> canAttackKing = new List<Position>();
            List<Position> attackPositions = new List<Position>();
            board[kingPosition.x, kingPosition.y].type = PieceType.Queen;
            canAttackKing = Chess.GetCanMoveMapForPiece(
                kingPosition,
                board,
                canAttackKing);
            board[kingPosition.x, kingPosition.y].type = PieceType.Knight;

            canAttackKing = Chess.GetCanMoveMapForPiece(
                kingPosition,
                board,
                canAttackKing);

            foreach (var pos in canAttackKing) {
                if (board[pos.x, pos.y]!=null) {
                    attackPositions = Chess.GetCanMoveMapForPiece(
                        new Position(pos.x, pos.y),
                        board,
                        attackPositions);
                }
            }
            board[kingPosition.x, kingPosition.y].type = PieceType.King;

            foreach (var attackPosition in attackPositions) {
                if (Equals(kingPosition, attackPosition)) {
                    return true;
                }
            }
            return false;
        }

        private bool CheckMate() {
            List<Position> canMovePosition = new List<Position>();
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (chessBoard.board[i, j] != null
                        && chessBoard.board[i, j].color == whoseMove) {

                        canMovePosition = Chess.GetCanMoveMapForPiece(
                            new Position(i, j),
                            chessBoard.board,
                            canMovePosition);
                        canMovePosition = HiddenCheck(canMovePosition, new Position(i, j));
                    }
                }
            }

            if (canMovePosition.Count == 0) {
                return true;
            }
            return false;
        }

        private List<Position> HiddenCheck(
            List<Position> canMovePositions,
            Position piecePos
        ) {
            Piece[,] board = (Piece[,])chessBoard.board.Clone();
            List<Position> newCanMovePositions = new List<Position>();
            foreach (var pos in canMovePositions) {
                board[pos.x, pos.y] = board[piecePos.x, piecePos.y];
                board[piecePos.x, piecePos.y] = null;
                if(!CheckKing(board)) {
                    newCanMovePositions.Add(pos);
                }
                board[piecePos.x, piecePos.y] = board[pos.x, pos.y];
                board[pos.x, pos.y] = null;
            }
            return newCanMovePositions;
        }

        private void RemoveCanMoveCells(){
            foreach (GameObject cell in canMoveCells) {
                Destroy(cell);
            }
        }

        private void ChangeMove() {
            if (whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }
        }

        private void ShowCheckSell() {

        }

        private void ShowCanMoveCells(List<Position> canMovePositions) {
            foreach (var pos in canMovePositions) {
                if (chessBoard.board[pos.x, pos.y] != null) {
                    canMoveCell.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
                    canMoveCells.Add(Instantiate(
                        canMoveCell,
                        new Vector3(
                            pos.x + boardObj.transform.position.x - 4 + 0.5f,
                            0.5f,
                            pos.y + boardObj.transform.position.z - 4 + 0.5f),
                        Quaternion.identity)
                    );
                    canMoveCell.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                }
                canMoveCells.Add(Instantiate(
                    canMoveCell,
                    new Vector3(
                        pos.x + boardObj.transform.position.x - 4 + 0.5f,
                        0.5f,
                        pos.y + boardObj.transform.position.z - 4 + 0.5f),
                    Quaternion.identity)
                );
            }
        }
    }
}

