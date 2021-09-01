using System.Collections.Generic;
using UnityEngine;
using chess;
using option;
using board;
using rules;
using json;
using check;

namespace controller {
    public class ChessBoardController : MonoBehaviour {
        const float BORD_SIZE = 4;
        const float CELL_SIZE = 0.5f;
        private Resource resources;
        private Option<Piece>[,] board = new Option<Piece>[8, 8];

        private int x;
        private int y;
        private Vector2Int selectedPos;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        public GameObject gameMenu;
        public GameObject changePawn;

        private GameObject canMoveCell;
        private GameObject boardObj;
        private List<GameObject> canMoveCells = new List<GameObject>();

        private GameObject[,] piecesMap = new GameObject[8, 8];
        private List<GameObject> pieceList;

        private List<Vector2Int> canMovePos = new List<Vector2Int>();

        private bool isPaused;

        private Vector2Int? enPassant;
        private bool wLeftCastling;
        private bool bLeftCastling;
        private bool wRightCastling;
        private bool bRightCastling;

        private JsonObject jsonObject;
        private GameStats gameStats;
        private List<PieceInfo> pieceInfoList = new List<PieceInfo>();

        private Dictionary<PieceType, List<Movement>> movement;

        private void Awake() {
           board = Chess.CreateBoard();
        }

        private void Start() {
            var resources = gameObject.GetComponent<Resource>();

            canMoveCell = resources.canMoveCell;
            boardObj = resources.boardObj;
            pieceList = resources.pieceList;
            movement = resources.movement;
            AddPiecesOnBoard();
        }

        public void Save() {
            var whoseMove = this.whoseMove;
            gameStats = GameStats.Mk(whoseMove);

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var board = this.board[i,j];

                    if (this.board[i,j].IsSome()) {
                        pieceInfoList.Add(PieceInfo.Mk(board.Peel(), i, j));
                    }
                }
            }
            jsonObject = JsonObject.Mk(pieceInfoList, gameStats);
            SaveLoad.WriteJson(SaveLoad.GetJsonType<JsonObject>(jsonObject), "json.json");
        }

        public void Load() {
            var gameInfo = SaveLoad.LoadFromJson("json.json", jsonObject);
            board = new Option<Piece>[8,8];

            whoseMove = gameStats.whoseMove;
            foreach (var pieceInfo in gameInfo.pieceInfo) {
                board[pieceInfo.xPos, pieceInfo.yPos] = Option<Piece>.Some(pieceInfo.piece);
            }
            AddPiecesOnBoard();
        }
 
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {
                    x = (int)(hit.point.x - (boardObj.transform.position.x - BORD_SIZE));
                    y = (int)(hit.point.z - (boardObj.transform.position.z - BORD_SIZE));

                    var piece = board[x, y];

                    if (piece.IsSome() && piece.Peel().color == whoseMove && !isPaused) {
                        RemoveCanMoveCells(canMoveCells);
                        canMovePos.Clear();

                        selectedPos = new Vector2Int(x, y);
                        if(piece.Peel().type == PieceType.King) {
                            Chess.CheckCastling();
                        }
                        canMovePos = Chess.GetPossibleMoveCells(movement, selectedPos, board);

                        ShowCanMoveCells(canMovePos);
                    } else {
                        RemoveCanMoveCells(canMoveCells);

                        var end = new Vector2Int(x, y);
                        var moveInfo = move.Move.CheckMove(selectedPos, end, canMovePos, board);
                        moveInfo.enPassant = enPassant;
                        var moveRes = Chess.Move(
                                moveInfo,
                                canMovePos,
                                board,
                                boardObj,
                                piecesMap
                        );
                        if (moveRes.end != null) {
                            enPassant = moveRes.enPassant;
                            if (moveRes.isPawnChange) {
                                selectedPos = new Vector2Int(x, y);
                                isPaused = true;
                                changePawn.SetActive(true);
                            }
                            if (!isPaused) {
                                whoseMove = Chess.ChangeMove(whoseMove);
                                var checkInfo = Chess.Check(
                                    board,
                                    selectedPos,
                                    whoseMove,
                                    movement
                                );
                                if (checkInfo != null) {
                                    Debug.Log(checkInfo);
                                }
                            }
                        }
                        canMovePos.Clear();
                    }
                }
            }
        }

        public void OpenMenu() {
            if (gameMenu.activeSelf == true) {
                gameMenu.SetActive(false);
            } else {
                gameMenu.SetActive(true);
            }
        }

        public void ChangePawn(int type) {
            Chess.ChangePiece(type, boardObj, selectedPos, piecesMap, board, pieceList, whoseMove);
            isPaused = false;
            changePawn.SetActive(false);
            whoseMove = Chess.ChangeMove(whoseMove);
        }

        public void AddPiecesOnBoard() {
            DestroyPieces(piecesMap);
            var boardPos = boardObj.transform.position;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board[i, j].Peel();

                    if (board[i, j].IsSome()) {
                        piecesMap[i, j] = GameObject.Instantiate(
                            pieceList[(int)piece.type * 2 + (int)piece.color],
                            new Vector3(
                                i + boardPos.x - BORD_SIZE + CELL_SIZE,
                                boardPos.y + CELL_SIZE,
                                j + boardPos.z - BORD_SIZE + CELL_SIZE
                            ),
                            Quaternion.identity,
                            boardObj.transform
                        );
                    }
                }
            }
        }

        public void ShowCanMoveCells(List<Vector2Int> canMovePos) {
            var boardPos = boardObj.transform.position;

            foreach (var pos in canMovePos) {
                if (board[pos.x, pos.y].IsSome()) {
                    canMoveCell.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);

                    canMoveCells.Add(GameObject.Instantiate(
                        canMoveCell,
                        new Vector3(
                            pos.x + boardPos.x - BORD_SIZE + CELL_SIZE,
                            boardPos.y + CELL_SIZE,
                            pos.y + boardPos.z - BORD_SIZE + CELL_SIZE),
                        Quaternion.identity)
                    );
                    canMoveCell.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                }
                canMoveCells.Add(GameObject.Instantiate(
                    canMoveCell,
                    new Vector3(
                        pos.x + boardPos.x - BORD_SIZE + CELL_SIZE,
                        boardPos.y + CELL_SIZE,
                        pos.y + boardPos.z - BORD_SIZE + CELL_SIZE
                    ),
                    Quaternion.identity
                ));
            }
        }

        public static void RemoveCanMoveCells(List<GameObject> canMoveCells) {
            foreach (GameObject cell in canMoveCells) {
                GameObject.Destroy(cell);
            }
        }

        private static void DestroyPieces(GameObject[,] piecesMap) {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    GameObject.Destroy(piecesMap[i,j]);
                }
            }
        }
    }
}