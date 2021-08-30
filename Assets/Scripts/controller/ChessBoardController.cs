using System.Collections.Generic;
using UnityEngine;
using chess;
using option;
using board;
using stats;
using type;
using load;

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

        private Dictionary<PieceType, List<Movment>> movment = 
            new Dictionary<PieceType, List<Movment>>() {
            { PieceType.Pawn, new List<Movment> {
                Movment.Mk(Linear.Mk(new Vector2Int(1, 1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(1, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, 1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, 0)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(1, 0)), null)
                }
            },
            { PieceType.Bishop, new List<Movment> {
                Movment.Mk(Linear.Mk(new Vector2Int(1, 1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(1, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, 1)), null)
                }
            },
            { PieceType.Rook, new List<Movment> {
                Movment.Mk(Linear.Mk(new Vector2Int(1, 0)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(0, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, 0)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(0, 1)), null)
                }
            },
            { PieceType.Queen, new List<Movment> {
                Movment.Mk(Linear.Mk(new Vector2Int(1, 0)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(0, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, 0)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(0, 1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(1, 1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(1, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, -1)), null),
                Movment.Mk(Linear.Mk(new Vector2Int(-1, 1)), null)
                }
            },
            { PieceType.Knight, new List<Movment> { Movment.Mk(null, Circular.Mk(2f)) } },
            { PieceType.King, new List<Movment> { Movment.Mk(null,  Circular.Mk(1f)) } }
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
                        List<Movment> movmentList = movment[piece.Peel().type];

                        canMovePos = GetPossibleMovePosition(movmentList, selectedPos, board);

                        if (piece.Peel().type == PieceType.Pawn) {
                            canMovePos = SelectPawnMoves(board, selectedPos, canMovePos);
                        }
                        canMovePos = HiddenCheck(canMovePos, selectedPos);
                        ShowCanMoveCells(canMovePos);

                    } else {
                        RemoveCanMoveCells();

                        if (Move(selectedPos, new Vector2Int(x, y), canMovePos)) {

                            if(!isPaused) {
                                ChangeMove();
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

        public List<Vector2Int> SelectPawnMoves(
            Option<Piece>[,] board,
            Vector2Int position,
            List<Vector2Int> possibleMoves
        ) {
            Piece pawn = board[position.x, position.y].Peel();
            int dir;
            List<Vector2Int> newPossibleMoves = new List<Vector2Int>();

            if (pawn.color == PieceColor.White) {
                dir = -1;
            } else {
                dir = 1;
            }

            foreach (var pos in possibleMoves) {
                if (position.x == 1 && dir == 1 || position.x == 6 && dir == -1) {
                    if (Equals(pos, new Vector2Int(position.x + 2 * dir, position.y))
                        && board[pos.x, pos.y].IsNone()) {
                        newPossibleMoves.Add(pos);
                    }
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y))
                    && board[pos.x, pos.y].IsNone()) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y + dir))
                    && board[pos.x, pos.y].IsSome()
                    && board[pos.x, pos.y].Peel().color != pawn.color) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y - dir))
                    && board[pos.x, pos.y].IsSome()
                    && board[pos.x, pos.y].Peel().color != pawn.color) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y - dir))
                    && enPassant != null
                    && Equals(new Vector2Int(position.x, position.y - dir), enPassant)) {
                    newPossibleMoves.Add(pos);
                }

                if (Equals(pos, new Vector2Int(position.x + dir, position.y + dir))
                    && enPassant != null
                    && Equals(new Vector2Int(position.x, position.y + dir), enPassant)) {
                    newPossibleMoves.Add(pos);
                }
            }

            return newPossibleMoves;
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
                            enPassant = CheckEnPassant(end, board);
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

        private Vector2Int? CheckEnPassant(Vector2Int endPos, Option<Piece>[,] board) {
            var leftPiece = board[endPos.x, endPos.y - 1];
            var rigthPiece = board[endPos.x, endPos.y + 1];

            if (leftPiece.IsSome() && leftPiece.Peel().type == PieceType.Pawn) {
                return new Vector2Int(endPos.x, endPos.y);
            }
            if (rigthPiece.IsSome() && rigthPiece.Peel().type == PieceType.Pawn) {
                return new Vector2Int(endPos.x, endPos.y);
            }

            return null;
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
            ChangeMove();
        }

        public bool CheckKing(Option<Piece>[,] board, PieceColor whoseMove) {
            Vector2Int kingPosition = FindKing(board, whoseMove).Value;

            List<Vector2Int> canAttackKing = new List<Vector2Int>();
            List<Vector2Int> attack = new List<Vector2Int>();

            var king = board[kingPosition.x, kingPosition.y].Peel();
            king.type = PieceType.Queen;
            List<Movment> movmentList = movment[king.type];

            canAttackKing.AddRange(GetPossibleMovePosition(movmentList, kingPosition, board));

            king.type = PieceType.Knight;
            movmentList = movment[king.type];
            canAttackKing.AddRange(GetPossibleMovePosition(movmentList, kingPosition, board));

            foreach (var pos in canAttackKing) {
                if (board[pos.x, pos.y].IsSome()) {
                    movmentList = movment[board[pos.x, pos.y].Peel().type];
                    Vector2Int piecePos = new Vector2Int(pos.x, pos.y);

                    if (board[pos.x, pos.y].Peel().type == PieceType.Pawn) {
                        attack.AddRange(SelectPawnMoves(
                            board, 
                            pos, 
                            GetPossibleMovePosition(movmentList, piecePos, board))
                        );
                        continue;
                    }
                    attack.AddRange(GetPossibleMovePosition(movmentList, piecePos, board));
                }
            }

            foreach (var attackition in attack) {
                if (Equals(kingPosition, attackition)) {

                    return true;
                }
            }

            return false;
        }

        private bool CheckMate() {
            List<Vector2Int> canMovePosition = new List<Vector2Int>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].IsSome()
                    && board[i, j].Peel().color == whoseMove) {
                        List<Movment> movmentList = movment[board[i, j].Peel().type];
                        Vector2Int pos = new Vector2Int(i, j);

                        canMovePosition = GetPossibleMovePosition(movmentList, pos, board);
                        if (board[i, j].Peel().type == PieceType.Pawn) {
                            canMovePosition = SelectPawnMoves(board, selectedPos, canMovePosition);
                        }
                        canMovePosition = HiddenCheck(canMovePosition, new Vector2Int(i, j));
                        if (canMovePosition.Count != 0) {

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private List<Vector2Int> HiddenCheck(List<Vector2Int> canMovePos, Vector2Int piecePos) {
            Option<Piece>[,] board;
            List<Vector2Int> newCanMovePositions = new List<Vector2Int>();

            foreach (var pos in canMovePos) {
                board = (Option<Piece>[,])this.board.Clone();
                board[pos.x, pos.y] = board[piecePos.x, piecePos.y];
                board[piecePos.x, piecePos.y] = Option<Piece>.None();

                if (!CheckKing(board, whoseMove)) {
                    newCanMovePositions.Add(pos);
                }

                board[piecePos.x, piecePos.y] = board[pos.x, pos.y];
                board[pos.x, pos.y] = Option<Piece>.None();
            }

            return newCanMovePositions;
        }

        private List<Vector2Int> GetPossibleMovePosition(
                List<Movment> moveList,
                Vector2Int pos,
                Option<Piece>[,] board
            ) {
            var possibleMovePositions = new List<Vector2Int>();
            float startAngle;
            foreach (var movment in moveList) {
                if (movment.linear.HasValue) {
                    possibleMovePositions.AddRange(Chess.GetLinearMoves(
                        board,
                        pos,
                        movment.linear.Value
                    ));
                } else {
                    if (board[pos.x, pos.y].Peel().type == PieceType.Knight) {
                        startAngle = 22.5f;
                    } else {
                        startAngle = 20f;
                    }
                    possibleMovePositions = Chess.GetCirclularMoves(
                        board,
                        pos,
                        movment.circular.Value,
                        startAngle
                    );
                }
            }
 
            return possibleMovePositions;
        }

        private void RemoveCanMoveCells() {
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

            if (CheckMate()) {
                Debug.Log("CheckMate");
            }

            if (CheckKing(board, whoseMove)) {
                Debug.Log("Check");
            }
        }

        public static Vector2Int? FindKing(Option<Piece>[,] board, PieceColor color) {
            Vector2Int kingPosition = new Vector2Int();

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    var piece = board[i, j].Peel();
                    if (piece.type == PieceType.King && piece.color == color) {
                        kingPosition.x = i;
                        kingPosition.y = j;
                    }
                }
            }
            return kingPosition;
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

