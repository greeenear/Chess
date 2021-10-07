
using System;
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
        private FullBoard board;
        private GameObject[,] piecesMap = new GameObject[8, 8];

        private Vector2Int selectedPiece;
        private PieceColor whoseMove = PieceColor.White;

        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private List<MoveInfo> movesHistory = new List<MoveInfo>();
        private int noTakeMoves;

        private JsonObject jsonObject;

        private PlayerAction playerAction;

        private void Awake() {
            board.board = new Option<Piece>[8,8];
            board.traceBoard = new Option<PieceTrace>[8,8];
            board.board = Chess.CreateBoard();
        }

        private void Start() {
            resources = gameObject.GetComponent<Resource>();
            movesHistory.Add(new MoveInfo());
            AddPiecesOnBoard();
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

            var pieceOpt = board.board[selectedPos.x, selectedPos.y];
            if (pieceOpt.IsSome() && pieceOpt.Peel().color == whoseMove) {
                playerAction = PlayerAction.Select;
            }
            var lastMove = movesHistory[movesHistory.Count - 1];

            switch (playerAction) {
                case PlayerAction.Move:
                    DestroyHighlightCell(resources.storageHighlightCells.transform);
                    DestroyHighlightCell(resources.storageHighlightCheckCell.transform);
                    TraceCleaner(board.traceBoard);
                    if (!Physics.Raycast(ray, out hit, 100f, resources.highlightMask)) {
                        return;
                    }

                    CheckMoveInfo(currentMove);
                    movesHistory.Add(currentMove);
                    whoseMove = Chess.ChangeMove(whoseMove);

                    CheckGameStatus(board, whoseMove);
                    possibleMoves.Clear();

                    selectedPiece = selectedPos;
                    playerAction = PlayerAction.None;
                    break;
                case PlayerAction.Select:
                    DestroyHighlightCell(resources.storageHighlightCells.transform);
                    possibleMoves.Clear();

                    possibleMoves = Chess.GetPossibleMoves(selectedPos, board);

                    playerAction = PlayerAction.Move;
                    HighlightCells(possibleMoves);
                    break;
            }
        }

        public void Save() {
            GameStats gameStats;
            var whoseMove = this.whoseMove;
            gameStats = GameStats.Mk(whoseMove);
            List<PieceInfo> pieceInfoList = new List<PieceInfo>();

            for (int i = 0; i < board.board.GetLength(0); i++) {
                for (int j = 0; j < board.board.GetLength(1); j++) {
                    var board = this.board.board[i,j];

                    if (this.board.board[i,j].IsSome()) {
                        pieceInfoList.Add(PieceInfo.Mk(board.Peel(), i, j));
                    }
                }
            }
            jsonObject = JsonObject.Mk(pieceInfoList, gameStats);
            SaveLoad.WriteJson(SaveLoad.GetJsonType<JsonObject>(jsonObject), "json.json");
        }

        public void Load(string path) {
            board.board = new Option<Piece>[8,8];
            possibleMoves.Clear();
            DestroyHighlightCell(resources.storageHighlightCells.transform);
            var gameInfo = SaveLoad.ReadJson(path, jsonObject);
            whoseMove = gameInfo.gameStats.whoseMove; 
            foreach (var pieceInfo in gameInfo.pieceInfo) {
                board.board[pieceInfo.xPos, pieceInfo.yPos] = Option<Piece>.Some(pieceInfo.piece);
            }
            AddPiecesOnBoard();
            resources.gameMenu.SetActive(false);
            this.enabled = true;
        }

        public void OpenMenu() {
            if (resources.gameMenu.activeSelf == true) {
                resources.gameMenu.SetActive(false);
                this.enabled = true;
            } else {
                resources.gameMenu.SetActive(true);
                this.enabled = false;
            }
        }

        public void ChangePawn(int type) {
            whoseMove = Chess.ChangeMove(whoseMove);
            PieceType pieceType = (PieceType)type;

            var pawnPos = movesHistory[movesHistory.Count - 1].doubleMove.first.to;
            var x = pawnPos.x;
            var y = pawnPos.y;
            Chess.ChangePiece(board.board, pawnPos, pieceType, whoseMove);
            Destroy(piecesMap[x, y]);
            if (board.board[x, y].IsNone()) {
                return;
            }
            var piece = board.board[x, y].Peel();

            var Obj = resources.pieceList[(int)piece.type * 2 + (int)piece.color];
            piecesMap[x, y] = ObjectSpawner(Obj, pawnPos, resources.boardObj.transform);
            this.enabled = true;
            resources.changePawn.SetActive(false);
            var lastMove = movesHistory[movesHistory.Count - 1];

            whoseMove = Chess.ChangeMove(whoseMove);
            CheckGameStatus(board, whoseMove);
        }

        private void Move(MoveData moveData, Vector2Int? sentenced) {
            var start = moveData.from;
            var end = moveData.to;

            var boardPos = resources.boardObj.transform.position;
            if (sentenced.HasValue) {
                noTakeMoves = 0;
                Destroy(piecesMap[sentenced.Value.x, sentenced.Value.y]);
                board.board[sentenced.Value.x, sentenced.Value.y] = Option<Piece>.None();
            }
            move.Move.MovePiece(start, end, board.board);

            piecesMap[start.x, start.y].transform.position = new Vector3(
                end.x + boardPos.x - resources.halfBoardSize.x + resources.halfCellSize.x,
                boardPos.y + resources.halfCellSize.x,
                end.y + boardPos.z - resources.halfBoardSize.x + resources.halfCellSize.x
            );
            piecesMap[end.x, end.y] = piecesMap[start.x, start.y];
        }

        private void CheckMoveInfo(MoveInfo currentMove) {
            noTakeMoves++;
            Move(currentMove.doubleMove.first, currentMove.sentenced);
            if (currentMove.doubleMove.second.HasValue) {
                Move(currentMove.doubleMove.second.Value, currentMove.sentenced);
            }
            if (currentMove.pawnPromotion) {
                resources.changePawn.SetActive(true);
                this.enabled = false;
            }
            if (currentMove.trace.HasValue) {
                var trace = currentMove.trace.Value;
                board.traceBoard[trace.pos.x, trace.pos.y] = Option<PieceTrace>.Some(trace);
            }
        }

        private void CheckGameStatus(
            FullBoard board,
            PieceColor whoseMove
        ) {
            var gameStatus = Chess.GetGameStatus(
                board,
                whoseMove,
                movesHistory,
                noTakeMoves
            );
            if (gameStatus == GameStatus.None) {
                return;
            }

            if (gameStatus == GameStatus.CheckMate) {
                resources.gameMenu.SetActive(true);
                this.enabled = false;
            } else if (gameStatus == GameStatus.StaleMate) {
                resources.gameMenu.SetActive(true);
                this.enabled = false;
            } else if (gameStatus == GameStatus.Check) {
                var kingPos = Check.FindKing(board.board, whoseMove);
                var checkCell = resources.checkCell;
                ObjectSpawner(checkCell, kingPos, resources.storageHighlightCheckCell.transform);
            }

            if (gameStatus == GameStatus.Draw) {
                this.enabled = false;
                resources.gameMenu.SetActive(true);
            }
        }

        public void AddPiecesOnBoard() {
            DestroyPieces(piecesMap);

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board.board[i, j].Peel();
                    if (board.board[i, j].IsSome()) {
                        var obj = resources.pieceList[(int)piece.type * 2 + (int)piece.color];
                        var pos = new Vector2Int(i, j);
                        piecesMap[i, j] = ObjectSpawner(obj, pos, resources.boardObj.transform);
                    }
                }
            }
        }

        private void HighlightCells(List<MoveInfo> possibleMoves) {
            foreach (var pos in possibleMoves) {
                var cellPos = pos.doubleMove.first.to;
                var parentTransfrom =resources.storageHighlightCells.transform;

                if (board.board[cellPos.x, cellPos.y].IsSome()) {
                    ObjectSpawner(resources.underAttackCell, cellPos, parentTransfrom);
                }
                ObjectSpawner(resources.canMoveCell, cellPos, parentTransfrom);
            }
        }

        private GameObject ObjectSpawner(
            GameObject gameObject,
            Vector2Int spawnPos,
            Transform parentTransform
        ) {
            var boardPos = resources.boardObj.transform.position;
            var halfBoardSize = resources.halfBoardSize.x;
            var halfCellSize = resources.halfCellSize.x;

            var spawnWorldPos = new Vector3(
                spawnPos.x + boardPos.x - halfBoardSize + halfCellSize,
                boardPos.y + halfCellSize,
                spawnPos.y + boardPos.z - halfBoardSize + halfCellSize
            );

            return Instantiate(
                gameObject,
                spawnWorldPos,
                Quaternion.identity,
                parentTransform
            );
        }

        public static void TraceCleaner(Option<PieceTrace>[,] board) {
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    board[i,j] = Option<PieceTrace>.None();
                }
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