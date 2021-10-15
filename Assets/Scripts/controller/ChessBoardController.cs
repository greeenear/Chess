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
            board.traceBoard = new Option<Trace>[8,8];
            board.board = Chess.CreateBoard();
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
                    Array.Clear(board.traceBoard, 0 , board.traceBoard.Length);
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
                    var (newPossibleMoves, err) = Chess.GetPossibleMoves(selectedPos, board);
                    if (err != ChessErrors.None) {
                        return;
                    }
                    possibleMoves = newPossibleMoves;

                    playerAction = PlayerAction.Move;
                    HighlightCells(possibleMoves);
                    break;
            }
        }

        public void Save() {
            var gameStats = GameStats.Mk(whoseMove);
            jsonObject = JsonObject.Mk(new List<PieceInfo>(), new List<TraceInfo>(), gameStats);
            for (int i = 0; i < board.board.GetLength(0); i++) {
                for (int j = 0; j < board.board.GetLength(1); j++) {
                    var board = this.board.board[i,j];
                    var trace = this.board.traceBoard[i,j];
                    if (board.IsSome()) {
                        jsonObject.pieceInfos.Add(PieceInfo.Mk(board.Peel(), i, j));
                    }
                    if (trace.IsSome()) {
                        jsonObject.traceInfos.Add(TraceInfo.Mk(trace.Peel(), i, j));
                    }
                }
            }
            SaveLoad.WriteJson(SaveLoad.GetJsonType<JsonObject>(jsonObject), "json.json");
        }

        public void Load(string path) {
            board.board = new Option<Piece>[8,8];
            DestroyHighlightCell(resources.storageHighlightCells.transform);
            var gameInfo = SaveLoad.ReadJson(path, jsonObject);
            whoseMove = gameInfo.gameStats.whoseMove; 
            foreach (var pieceInfo in gameInfo.pieceInfos) {
                board.board[pieceInfo.x, pieceInfo.y] = Option<Piece>.Some(pieceInfo.piece);
            }
            foreach (var pieceInfo in gameInfo.traceInfos) {
                var trace = Option<Trace>.Some(pieceInfo.trace);
                board.traceBoard[pieceInfo.x, pieceInfo.y] = Option<Trace>.Some(pieceInfo.trace);
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
            var pos = movesHistory[movesHistory.Count - 1].doubleMove.first.to;
            board.board[pos.x, pos.y] = Option<Piece>.Some(Piece.Mk(pieceType, whoseMove, 0));
            Destroy(piecesMap[pos.x, pos.y]);
            if (board.board[pos.x, pos.y].IsNone()) {
                return;
            }
            var piece = board.board[pos.x, pos.y].Peel();
            var Obj = resources.pieceList[(int)piece.type * 2 + (int)piece.color];
            piecesMap[pos.x, pos.y] = ObjectSpawner(Obj, pos, resources.boardObj.transform);
            this.enabled = true;
            resources.changePawn.SetActive(false);

            whoseMove = Chess.ChangeMove(whoseMove);
            CheckGameStatus(board, whoseMove);
        }

        private void Move(MoveData moveData, Vector2Int? sentenced) {
            var boardPos = resources.boardObj.transform.position;
            if (sentenced.HasValue) {
                noTakeMoves = 0;
                Destroy(piecesMap[sentenced.Value.x, sentenced.Value.y]);
                board.board[sentenced.Value.x, sentenced.Value.y] = Option<Piece>.None();
            }
            move.Move.MovePiece(moveData.from, moveData.to, board.board);

            piecesMap[moveData.from.x, moveData.from.y].transform.position = new Vector3(
                moveData.to.x + boardPos.x - resources.halfBoardSize.x + resources.halfCellSize.x,
                boardPos.y + resources.halfCellSize.x,
                moveData.to.y + boardPos.z - resources.halfBoardSize.x + resources.halfCellSize.x
            );
            piecesMap[moveData.to.x, moveData.to.y] = piecesMap[moveData.from.x, moveData.from.y];
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
                board.traceBoard[trace.pos.x, trace.pos.y] = Option<Trace>.Some(trace);
            }
        }

        private void CheckGameStatus(
            FullBoard board,
            PieceColor whoseMove
        ) {
            var (status, err) = Chess.GetGameStatus(board, whoseMove, movesHistory, noTakeMoves);
            if (err != ChessErrors.None) {
                Debug.Log("gameStatusError");
            }
            if (status == GameStatus.None) {
                return;
            }
            if (status == GameStatus.Check) {
                var (kingPos, err2) = Check.FindKing(board.board, whoseMove);
                if (err2 != CheckErrors.None) {
                    Debug.Log("FindKingError");
                }
                var highlightCheckCell = resources.storageHighlightCheckCell.transform;
                ObjectSpawner(resources.checkCell, kingPos, highlightCheckCell);
            } else {
                resources.gameMenu.SetActive(true);
                this.enabled = false;
            }
        }

        public void AddPiecesOnBoard() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    GameObject.Destroy(piecesMap[i,j]);
                    if (board.board[i, j].IsSome()) {
                        var piece = board.board[i, j].Peel();
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

            var spawnWorldPos = new Vector3(
                spawnPos.x + boardPos.x - resources.halfBoardSize.x + resources.halfCellSize.x,
                boardPos.y + resources.halfCellSize.x,
                spawnPos.y + boardPos.z - resources.halfBoardSize.x + resources.halfCellSize.x
            );
            return Instantiate(gameObject, spawnWorldPos, Quaternion.identity, parentTransform);
        }

        private void DestroyHighlightCell(Transform highlightCells) {
            foreach (Transform child in highlightCells) {
                Destroy(child.gameObject);
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