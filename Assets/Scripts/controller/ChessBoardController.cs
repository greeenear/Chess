using System.Collections.Generic;
using UnityEngine;
using chess;
using check;
using option;
using rules;
using json;
using move;

namespace controller {
    enum PlayerAction {
        None,
        Select,
        Move
    }

    public class ChessBoardController : MonoBehaviour {
        private Resource resources;
        private Option<Piece>[,] board = new Option<Piece>[8, 8];
        private GameObject[,] piecesMap = new GameObject[8, 8];

        private Vector2Int selectedPiece;
        private PieceColor whoseMove = PieceColor.White;

        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private List<MoveInfo> completedMoves = new List<MoveInfo>();
        private int noTakeMoves;

        private JsonObject jsonObject;

        private PlayerAction playerAction;
        private bool isPaused;

        private void Awake() {
           board = Chess.CreateBoard();
        }

        private void Start() {
            resources = gameObject.GetComponent<Resource>();
            completedMoves.Add(new MoveInfo());
            AddPiecesOnBoard();
        }

        public void Save() {
            GameStats gameStats;
            var whoseMove = this.whoseMove;
            gameStats = GameStats.Mk(whoseMove);
            List<PieceInfo> pieceInfoList = new List<PieceInfo>();

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

        public void Load(string path) {
            var gameInfo = SaveLoad.LoadFromJson(path, jsonObject);
            board = new Option<Piece>[8,8];

            whoseMove = gameInfo.gameStats.whoseMove; 
            foreach (var pieceInfo in gameInfo.pieceInfo) {
                board[pieceInfo.xPos, pieceInfo.yPos] = Option<Piece>.Some(pieceInfo.piece);
            }
            AddPiecesOnBoard();
            resources.gameMenu.SetActive(false);
        }

        private void Update() {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out hit, 100f)) {
                return;
            }

            var boardPos = resources.boardObj.transform.position;
            var selectedPosFloat = hit.point - (boardPos - resources.halfBoardSize);

            var selectedPos = new Vector2Int((int)selectedPosFloat.x, (int)selectedPosFloat.z);
            var currentMove = GetCurrentMove(selectedPos);
            var firstMove = currentMove.doubleMove.first;

            var pieceOpt = board[selectedPos.x, selectedPos.y];
            if (pieceOpt.IsSome() && pieceOpt.Peel().color == whoseMove && !isPaused) {
                playerAction = PlayerAction.Select;
            }
            var lastMove = completedMoves[completedMoves.Count - 1];

            switch (playerAction) {
                case PlayerAction.Move:
                    if (!Physics.Raycast(ray, out hit, 100f, resources.highlightMask)) {
                        return;
                    }

                    CheckMove(currentMove);
                    completedMoves.Add(currentMove);
                    if (!isPaused) {

                        whoseMove = Chess.ChangeMove(whoseMove);
                        CheckGameStatus(board, whoseMove, lastMove, noTakeMoves);
                    }

                    selectedPiece = selectedPos;
                    playerAction = PlayerAction.None;
                    possibleMoves.Clear();
                    DestroyHighlightCell(resources.storageHighlightCells.transform);
                    break;
                case PlayerAction.Select:
                    DestroyHighlightCell(resources.storageHighlightCells.transform);
                    possibleMoves.Clear();

                    possibleMoves = Chess.GetPossibleMoves(selectedPos, board, lastMove);

                    playerAction = PlayerAction.Move;
                    HighlightCell(possibleMoves);
                    break;
            }
        }

        public void OpenMenu() {
            if (resources.gameMenu.activeSelf == true) {
                resources.gameMenu.SetActive(false);
            } else {
                resources.gameMenu.SetActive(true);
            }
        }

        public void ChangePawn(int type) {
            var boardPos = resources.boardObj.transform.position;
            var pawnPos = completedMoves[completedMoves.Count - 1].doubleMove.first.to;
            var x = pawnPos.x;
            var y = pawnPos.y;
            PieceType pieceType = (PieceType)type;

            Chess.ChangePiece(board, pawnPos, pieceType, whoseMove);
            Destroy(piecesMap[x, y]);
            var piece = board[x, y];

            piecesMap[x, y] = GameObject.Instantiate(
                resources.pieceList[(int)piece.Peel().type * 2 + (int)piece.Peel().color],
                new Vector3(
                    x + boardPos.x - resources.halfBoardSize.x + resources.halfCellSize.x,
                    boardPos.y + resources.halfCellSize.x,
                    y + boardPos.z - resources.halfBoardSize.x + resources.halfCellSize.x
                ),
                Quaternion.identity,
                resources.boardObj.transform
            );
            isPaused = false;
            resources.changePawn.SetActive(false);
            var lastMove = completedMoves[completedMoves.Count - 1];
            whoseMove = Chess.ChangeMove(whoseMove);
        }

        private void Move(MoveData moveData, Vector2Int? sentenced) {
            var start = moveData.from;
            var end = moveData.to;
            move.Move.MovePiece(start, end, board);

            var boardPos = resources.boardObj.transform.position;
            if (sentenced.HasValue) {
                noTakeMoves = 0;
                Destroy(piecesMap[sentenced.Value.x, sentenced.Value.y]);
            }

            piecesMap[start.x, start.y].transform.position = new Vector3(
                end.x + boardPos.x - resources.halfBoardSize.x + resources.halfCellSize.x,
                boardPos.y + resources.halfCellSize.x,
                end.y + boardPos.z - resources.halfBoardSize.x + resources.halfCellSize.x
            );
            piecesMap[end.x, end.y] = piecesMap[start.x, start.y];
        }

        private void CheckMove(MoveInfo currentMove) {
            noTakeMoves++;
            
            Move(currentMove.doubleMove.first, currentMove.sentenced);
            if (currentMove.doubleMove.second.HasValue) {
                Move(currentMove.doubleMove.second.Value, currentMove.sentenced);
            }
            if (currentMove.pawnPromotion) {
                resources.changePawn.SetActive(true);
                isPaused = true;
            }
        }

        private void CheckGameStatus(
            Option<Piece>[,] board,
            PieceColor whoseMove,
            MoveInfo lastMove,
            int noTakeMoves
        ) {
            var gameStatus = Chess.GetCheckStatus(board, whoseMove, lastMove);
            if (gameStatus.checkMate) {
                resources.gameMenu.SetActive(true);
            }
            if (Chess.CheckDraw(completedMoves, noTakeMoves)) {
                Debug.Log("Draw");
            }
        }

        public void AddPiecesOnBoard() {
            DestroyPieces(piecesMap);
            var boardPos = resources.boardObj.transform.position;
            var halfBoardSize = resources.halfBoardSize.x;
            var halfCellSize = resources.halfCellSize.x;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board[i, j].Peel();

                    if (board[i, j].IsSome()) {
                        piecesMap[i, j] = GameObject.Instantiate(
                            resources.pieceList[(int)piece.type * 2 + (int)piece.color],
                            new Vector3(
                                i + boardPos.x - halfBoardSize + halfCellSize,
                                boardPos.y + halfCellSize,
                                j + boardPos.z - halfBoardSize + halfCellSize
                            ),

                            Quaternion.identity,
                            resources.boardObj.transform
                        );
                    }
                }
            }
        }

        private void HighlightCell(List<MoveInfo> possibleMoves) {
            var boardPos = resources.boardObj.transform.position;
            var halfBoardSize = resources.halfBoardSize.x;
            var halfCellSize = resources.halfCellSize.x;

            foreach (var pos in possibleMoves) {
                var toX = pos.doubleMove.first.to.x;
                var toY = pos.doubleMove.first.to.y;

                var highlightPos = new Vector3(
                            toX + boardPos.x - halfBoardSize + halfCellSize,
                            boardPos.y + halfCellSize,
                            toY + boardPos.z - halfBoardSize + halfCellSize
                        );
                if (board[toX, toY].IsSome()) {
                    Instantiate(
                        resources.underAttackCell,
                        highlightPos,
                        Quaternion.identity,
                        resources.storageHighlightCells.transform
                    );
                }
                Instantiate(
                    resources.canMoveCell,
                    highlightPos,
                    Quaternion.identity,
                    resources.storageHighlightCells.transform
                );
            }
        }

        private void DestroyHighlightCell(Transform storageHighlightCells) {
            foreach (Transform child in storageHighlightCells) {
                Destroy(child.gameObject);
            }
        }

        private void DestroyPieces(GameObject[,] piecesMap) {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    GameObject.Destroy(piecesMap[i,j]);
                }
            }
        }

        private MoveInfo GetCurrentMove(Vector2Int selectedPos) {
            foreach (var move in possibleMoves) {
                if (move.doubleMove.first.to == selectedPos) {
                    return move;
                }
            }

            return new MoveInfo();
        }
    }
}