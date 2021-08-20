using System.Collections.Generic;
using UnityEngine;
using chess;

namespace controller {  

    public class ChessBoardController : MonoBehaviour {
        public Piece[,] board = new Piece[8, 8];

        public GameObject boardObj;

        private int xPosition;
        private int yPosition;
        Vector2Int selectedPosition;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        public GameObject canMoveCell;
        public GameObject checkKingCell;

        private List<GameObject> canMoveCells = new List<GameObject>();

        private GameObject[,] pieceGameObjects = new GameObject[8, 8];

        private List<Vector2Int> canMovePositions = new List<Vector2Int>();

        private void Awake() {
            board[0, 0] = Piece.mk(PieceType.Rook, PieceColor.Black);
            board[0, 1] = Piece.mk(PieceType.Knight, PieceColor.Black);
            board[0, 2] = Piece.mk(PieceType.Bishop, PieceColor.Black);
            board[0, 4] = Piece.mk(PieceType.King, PieceColor.Black);
            board[0, 3] = Piece.mk(PieceType.Queen, PieceColor.Black);
            board[0, 5] = Piece.mk(PieceType.Bishop, PieceColor.Black);
            board[0, 6] = Piece.mk(PieceType.Knight, PieceColor.Black);
            board[0, 7] = Piece.mk(PieceType.Rook, PieceColor.Black);

            for (int i = 0; i < 8; i++) {
                board[1, i] = Piece.mk(PieceType.Pawn, PieceColor.Black);
            }

            board[7, 0] = Piece.mk(PieceType.Rook, PieceColor.White);
            board[7, 1] = Piece.mk(PieceType.Knight, PieceColor.White);
            board[7, 2] = Piece.mk(PieceType.Bishop, PieceColor.White);
            board[7, 4] = Piece.mk(PieceType.King, PieceColor.White);
            board[7, 3] = Piece.mk(PieceType.Queen, PieceColor.White);
            board[7, 5] = Piece.mk(PieceType.Bishop, PieceColor.White);
            board[7, 6] = Piece.mk(PieceType.Knight, PieceColor.White);
            board[7, 7] = Piece.mk(PieceType.Rook, PieceColor.White);

            for (int i = 0; i < 8; i++) {
                board[6, i] = Piece.mk(PieceType.Pawn, PieceColor.White);
            }
        }
        private void Start() {
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

                    var piece = board[xPosition, yPosition];

                    if (piece != null && piece.color == whoseMove) {
                        RemoveCanMoveCells();
                        canMovePositions.Clear();

                        selectedPosition = new Vector2Int(xPosition, yPosition);
                        canMovePositions = Chess.CalcPossibleMoves(
                            selectedPosition,
                            board);
                        if (piece.type == PieceType.Pawn) {
                            canMovePositions = SelectPawnMoves(
                                board,
                                selectedPosition,
                                canMovePositions
                            );
                        }
                        canMovePositions = HiddenCheck(canMovePositions, selectedPosition);
                        ShowCanMoveCells(canMovePositions);

                    } else {
                        RemoveCanMoveCells();

                        if (Move(selectedPosition,
                            new Vector2Int(xPosition, yPosition), canMovePositions)) {
                            ChangeMove();
                            if (CheckMate()) {
                                Debug.Log("CheckMate");
                            }

                            if (CheckKing(board, whoseMove)) {
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

        public static List<Vector2Int> SelectPawnMoves(
            Piece[,] board,
            Vector2Int position,
            List<Vector2Int> possibleMoves
        ) {
            Piece pawn = board[position.x, position.y];
            int dir;
            List<Vector2Int> newPossibleMoves = new List<Vector2Int>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }
            
            foreach(var pos in possibleMoves) {        
                if(position.x == 1 && dir == 1 || position.x == 6 && dir == -1) {
                    if (Equals(pos, new Vector2Int(position.x + 2 * dir, position.y)) 
                        && board[pos.x, pos.y] == null) {
                        newPossibleMoves.Add(pos);
                    }
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y)) 
                    &&  board[pos.x, pos.y] == null) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y + dir)) 
                    && board[pos.x, pos.y] != null && board[pos.x, pos.y].color != pawn.color) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y - dir)) 
                    && board[pos.x, pos.y] != null && board[pos.x, pos.y].color != pawn.color) {
                    newPossibleMoves.Add(pos);
                }
            }
            return newPossibleMoves;
        }

        public bool Move(Vector2Int start, Vector2Int end, List<Vector2Int> canMovePositions) {
            foreach (var pos in canMovePositions) {
                if (pos.x == end.x && pos.y == end.y) {
                    if (board[end.x, end.y] != null) {
                        Destroy(pieceGameObjects[end.x, end.y]);
                    }
                    board[end.x, end.y] = board[start.x, start.y];
                    board[start.x, start.y] = null;
                    pieceGameObjects[end.x, end.y] = pieceGameObjects[start.x, start.y];

                    pieceGameObjects[end.x, end.y].transform.position =
                    new Vector3(
                        xPosition + boardObj.transform.position.x - 4 + 0.5f,
                        boardObj.transform.position.y + 0.5f,
                        yPosition + boardObj.transform.position.z - 4 + 0.5f);
                    return true;
                }
            }
            return false;
        }

        public static bool CheckKing(Piece[,] board, PieceColor whoseMove) {
            Vector2Int kingPosition = Chess.FindKing(board, whoseMove).Value;

            List<Vector2Int> canAttackKing = new List<Vector2Int>();
            List<Vector2Int> attackPositions = new List<Vector2Int>();
            board[kingPosition.x, kingPosition.y].type = PieceType.Queen;
            canAttackKing.AddRange(Chess.CalcPossibleMoves(
                kingPosition,
                board));
            board[kingPosition.x, kingPosition.y].type = PieceType.Knight;

            canAttackKing.AddRange(Chess.CalcPossibleMoves(
                kingPosition,
                board));

            foreach (var pos in canAttackKing) {
                if (board[pos.x, pos.y] != null) {
                    if(board[pos.x, pos.y].type == PieceType.Pawn) {
                        attackPositions.AddRange(SelectPawnMoves(board, pos, Chess.CalcPossibleMoves(
                           new Vector2Int(pos.x, pos.y),
                           board)));
                        continue;
                    }
                    attackPositions.AddRange(Chess.CalcPossibleMoves(
                        new Vector2Int(pos.x, pos.y),
                        board));
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
            List<Vector2Int> canMovePosition = new List<Vector2Int>();
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j] != null
                        && board[i, j].color == whoseMove) {
                        canMovePosition = Chess.CalcPossibleMoves(
                            new Vector2Int(i, j),
                            board
                        );
                        canMovePosition = HiddenCheck(canMovePosition, new Vector2Int(i, j));
                        if (canMovePosition.Count != 0) {
                            return false;
                        }

                    }
                }
            }
            return true ;
        }

        private List<Vector2Int> HiddenCheck(
            List<Vector2Int> canMovePositions,
            Vector2Int piecePos
        ) {
            Piece[,] board = (Piece[,])this.board.Clone();
            List<Vector2Int> newCanMovePositions = new List<Vector2Int>();
            foreach (var pos in canMovePositions) {
                board[pos.x, pos.y] = board[piecePos.x, piecePos.y];
                board[piecePos.x, piecePos.y] = null;
                if(!CheckKing(board, whoseMove)) {
                    newCanMovePositions.Add(pos);
                }
                board[piecePos.x, piecePos.y] = board[pos.x, pos.y];
                board[pos.x, pos.y] = null;
            }
            foreach (var a in newCanMovePositions) {
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

        private void ShowCanMoveCells(List<Vector2Int> canMovePositions) {
            foreach (var pos in canMovePositions) {
                if (board[pos.x, pos.y] != null) {
                    canMoveCell.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
                    canMoveCells.Add(Instantiate(
                        canMoveCell,
                        new Vector3(
                            pos.x + boardObj.transform.position.x - 4 + 0.5f,
                            boardObj.transform.position.y +0.5f,
                            pos.y + boardObj.transform.position.z - 4 + 0.5f),
                        Quaternion.identity)
                    );
                    canMoveCell.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                }
                canMoveCells.Add(Instantiate(
                    canMoveCell,
                    new Vector3(
                        pos.x + boardObj.transform.position.x - 4 + 0.5f,
                        boardObj.transform.position.y + 0.5f,
                        pos.y + boardObj.transform.position.z - 4 + 0.5f),
                    Quaternion.identity)
                );
            }
        }
    }
}

