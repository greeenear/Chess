using System.Collections.Generic;
using UnityEngine;
using chess;
using option;
using board;
using stats;
using type;
using load;
using rules;
using move;
using check;

namespace controller {
    public class ChessBoardController : MonoBehaviour {
        public Option<Piece>[,] board = new Option<Piece>[8, 8];
        public GameObject boardObj;

        public List<GameObject> piecesObjList;
        private int x;
        private int y;
        private Vector2Int selectedPos;

        public PieceColor whoseMove = PieceColor.White;

        private Ray ray;
        private RaycastHit hit;

        public GameObject gameMenu;
        public GameObject changePawn;
        public GameObject canMoveCell;
        private List<GameObject> canMoveCells = new List<GameObject>();

        public GameObject[,] pieceGameObjects = new GameObject[8, 8];

        private List<Vector2Int> canMovePos = new List<Vector2Int>();

        private bool isPaused;

        private Vector2Int? enPassant;

        private JsonObject jsonObject;
        private GameStats gameStats;
        private List<PieceInfo> pieceList = new List<PieceInfo>();

        private Dictionary<PieceType, List<Movement>> movement = 
            new Dictionary<PieceType, List<Movement>>() {
            { PieceType.Pawn, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, 0)))
                }
            },
            { PieceType.Bishop, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 1)))
                }
            },
            { PieceType.Rook, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(0, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(0, 1)))
                }
            },
            { PieceType.Queen, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(0, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(0, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 1)))
                }
            },
            { PieceType.Knight, new List<Movement> { Movement.Circular(Circular.Mk(2f)) } },
            { PieceType.King, new List<Movement> { Movement.Circular(Circular.Mk(1f)) } }
        };

        private void Awake() {
           CreateBoard();
        }

        private void Start() {
            piecesObjList = gameObject.GetComponent<Resource>().pieceList;
            AddPiecesOnBoard(pieceGameObjects, piecesObjList);
        }

        public void Save() {
            var whoseMove = this.whoseMove;
            gameStats = GameStats.Mk(whoseMove);

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var board = this.board[i,j];

                    if (this.board[i,j].IsSome()) {
                        pieceList.Add(PieceInfo.Mk(board.Peel(), i, j));
                    }
                }
            }
            jsonObject = JsonObject.Mk(pieceList, gameStats);
            save.Save.WriteJson(FillJsonType.GetJsonType<JsonObject>(jsonObject), "json.json");
        }

        public void Load() {
            var gameInfo = Load<JsonObject>.LoadFromJson("json.json", jsonObject);
            board = new Option<Piece>[8,8];

            whoseMove = gameInfo.gameStats.whoseMove;
            foreach(var pieceInfo in gameInfo.pieceInfo) {
                board[pieceInfo.xPos, pieceInfo.yPos] = Option<Piece>.Some(pieceInfo.piece);
            }
            AddPiecesOnBoard(pieceGameObjects, piecesObjList);
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {
                    x = (int)(hit.point.x - (boardObj.transform.position.x - 4));
                    y = (int)(hit.point.z - (boardObj.transform.position.z - 4));

                    var piece = board[x, y];

                    if (piece.IsSome() && piece.Peel().color == whoseMove && !isPaused) {
                        RemoveCanMoveCells();
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
                        ShowCanMoveCells(canMovePos);

                    } else {
                        RemoveCanMoveCells();

                        if (Move(selectedPos, new Vector2Int(x, y), canMovePos)) {

                            if(!isPaused) {
                                whoseMove = Chess.ChangeMove(whoseMove);
                                var checkInfo = Chess.Check(
                                    board,
                                    selectedPos,
                                    whoseMove,
                                    movement,
                                    enPassant
                                );
                                if(checkInfo != null) {
                                    Debug.Log(checkInfo);
                                }
                            }
                        }
                        canMovePos.Clear();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (gameMenu.activeSelf == true) {
                    gameMenu.SetActive(false);
                } else {
                    gameMenu.SetActive(true);
                }
            }
        }

        public void AddPiecesOnBoard(GameObject[,] pieceGameObjects, List<GameObject> pieceList) {
            DestroyPieces();
            var boardPos = boardObj.transform.position;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board[i, j].Peel();

                    if (board[i, j].IsSome()) {
                        pieceGameObjects[i, j] = Instantiate(
                            pieceList[(int)piece.type * 2 + (int)piece.color],
                            new Vector3(
                                i + boardPos.x - 4 + 0.5f,
                                boardPos.y + 0.5f,
                                j + boardPos.z - 4 + 0.5f
                            ),
                            Quaternion.identity,
                            boardObj.transform
                        );
                    }
                }
            }
        }

        private bool Move(Vector2Int start, Vector2Int end, List<Vector2Int> canMovePos) {
            var offset = boardObj.transform.position;

            foreach (var pos in canMovePos) {
                if (pos.x == end.x && pos.y == end.y) {
                    if (board[end.x, end.y].IsSome()) {
                        Destroy(pieceGameObjects[end.x, end.y]);
                    }

                    board[end.x, end.y] = board[start.x, start.y];
                    board[start.x, start.y] = Option<Piece>.None();
                    pieceGameObjects[end.x, end.y] = pieceGameObjects[start.x, start.y];

                    pieceGameObjects[end.x, end.y].transform.position =
                    new Vector3(x + offset.x - 4 + 0.5f, offset.y + 0.5f, y + offset.z - 4 + 0.5f);

                    if (board[end.x, end.y].Peel().type == PieceType.Pawn) {
                        var possibleEnPassant = new Vector2Int(start.x, end.y);

                        if (enPassant != null && Equals(enPassant, possibleEnPassant)) {
                            board[start.x, end.y] = Option<Piece>.None();
                            Destroy(pieceGameObjects[start.x, end.y]);
                        }
                        if (Mathf.Abs(start.x - end.x) == 2) {
                            enPassant = Chess.CheckEnPassant(end, board);
                            return true;
                        }
                        if (end.x == 7 || end.x == 0) {
                            selectedPos = new Vector2Int(end.x, end.y);
                            isPaused = true;
                            changePawn.SetActive(true);
                        }
                    }
                    enPassant = null;

                    return true;
                }
             }

            return false;
        }

        public void ChangePawn(int type) {
            var boardPos = boardObj.transform.position;
            var x = selectedPos.x;
            var y = selectedPos.y;

            Destroy(pieceGameObjects[x,y]);
            PieceType pieceType = (PieceType)type;
            board[x, y] = Option<Piece>.Some(Piece.Mk(pieceType, whoseMove));

            var piece = board[x, y].Peel();
            pieceGameObjects[selectedPos.x, selectedPos.y] = Instantiate(
                piecesObjList[(int)piece.type * 2 + (int)piece.color],
                new Vector3(
                    x + boardPos.x - 4 + 0.5f,
                    boardPos.y + 0.5f,
                    y + boardPos.z - 4 + 0.5f
                ),
                Quaternion.identity,
                boardObj.transform
            );

            isPaused = false;
            changePawn.SetActive(false);
            whoseMove = Chess.ChangeMove(whoseMove);
        }

        private void RemoveCanMoveCells() {
            foreach (GameObject cell in canMoveCells) {
                Destroy(cell);
            }
        }

        private void DestroyPieces() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    Destroy(pieceGameObjects[i,j]);
                }
            }
        }

        private void CreateBoard() {
            board[0, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.Black));
            board[0, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.Black));
            board[0, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.Black));
            board[0, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, PieceColor.Black));
            board[0, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, PieceColor.Black));
            board[0, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.Black));
            board[0, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.Black));
            board[0, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.Black));

            for (int i = 0; i < 8; i++) {
                board[1, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, PieceColor.Black));
            }

            board[7, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.White));
            board[7, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.White));
            board[7, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.White));
            board[7, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, PieceColor.White));
            board[7, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, PieceColor.White));
            board[7, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.White));
            board[7, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.White));
            board[7, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.White));

            for (int i = 0; i < 8; i++) {
                board[6, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, PieceColor.White));
            }
        }

        private void ShowCanMoveCells(List<Vector2Int> canMovePos) {
            var boardPos = boardObj.transform.position;

            foreach (var pos in canMovePos) {
                if (board[pos.x, pos.y].IsSome()) {
                    canMoveCell.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);

                    canMoveCells.Add(Instantiate(
                        canMoveCell,
                        new Vector3(
                            pos.x + boardPos.x - 4 + 0.5f,
                            boardPos.y + 0.5f,
                            pos.y + boardPos.z - 4 + 0.5f),
                        Quaternion.identity)
                    );
                    canMoveCell.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
                }
                canMoveCells.Add(Instantiate(
                    canMoveCell,
                    new Vector3(
                        pos.x + boardPos.x - 4 + 0.5f,
                        boardPos.y + 0.5f,
                        pos.y + boardPos.z - 4 + 0.5f
                    ),
                    Quaternion.identity
                ));
            }
        }
    }
}

