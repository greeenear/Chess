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
        private Option<Piece>[,] board = new Option<Piece>[8, 8];

        private int x;
        private int y;
        private Vector2Int selectedPos;

        private PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        public GameObject gameMenu;
        public GameObject changePawn;
        public GameObject canMoveCell;
        private List<GameObject> canMoveCells = new List<GameObject>();

        private GameObject[,] piecesMap = new GameObject[8, 8];
        public GameObject boardObj;
        private List<GameObject> pieceList;

        private List<Vector2Int> canMovePos = new List<Vector2Int>();

        private bool isPaused;

        private Vector2Int? enPassant;
        private JsonObject jsonObject;
        private GameStats gameStats;
        private List<PieceInfo> pieceInfoList = new List<PieceInfo>();

        private Dictionary<PieceType, List<Movement>> movement;

        private void Awake() {
           board = Chess.CreateBoard();
        }

        private void Start() {
            var resources = gameObject.GetComponent<Resource>();
            pieceList = resources.pieceList;
            movement = resources.movement;
            Chess.AddPiecesOnBoard(piecesMap, boardObj, pieceList, board);
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
            Chess.AddPiecesOnBoard(piecesMap, boardObj, pieceList, board);
        }
 
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {
                    x = (int)(hit.point.x - (boardObj.transform.position.x - 4));
                    y = (int)(hit.point.z - (boardObj.transform.position.z - 4));

                    var piece = board[x, y];

                    if (piece.IsSome() && piece.Peel().color == whoseMove && !isPaused) {
                        Chess.RemoveCanMoveCells(canMoveCells);
                        canMovePos.Clear();

                        selectedPos = new Vector2Int(x, y);
                        List<Movement> movmentList = movement[piece.Peel().type];

                        canMovePos = move.Move.GetPossibleMovePosition(
                            movmentList,
                            selectedPos,
                            board
                        );

                        if (piece.Peel().type == PieceType.Pawn) {
                            canMovePos = move.Move.SelectPawnMoves(
                                board,
                                selectedPos,
                                canMovePos,
                                enPassant
                            );
                        }
                        canMovePos = Check.HiddenCheck(
                            canMovePos,
                            selectedPos,
                            movement,
                            board,
                            whoseMove,
                            enPassant
                        );
                        Chess.ShowCanMoveCells(
                            canMovePos,
                            boardObj,
                            board,
                            canMoveCell,
                            canMoveCells
                        );
                    } else {
                        Chess.RemoveCanMoveCells(canMoveCells);

                        var moveRes = Chess.Move(
                                selectedPos,
                                new Vector2Int(x, y),
                                enPassant,
                                canMovePos,
                                board,
                                boardObj,
                                piecesMap
                        );
                        if (moveRes.pos != null) {
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
                                    movement,
                                    enPassant
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
    }
}