using System.Collections.Generic;
using UnityEngine;
using chess;
using check;
using option;
using rules;
using json;
using move;

namespace controller {
    enum GameState {
        PieceSelected,
        PieceNotSelected
    }

    public class ChessBoardController : MonoBehaviour {
        private Resource resources;
        private Option<Piece>[,] board = new Option<Piece>[8, 8];
        private GameObject[,] piecesMap = new GameObject[8, 8];

        private Vector2Int selectedPiece;
        private PieceColor whoseMove = PieceColor.White;

        private List<MoveInfo> canMovePos = new List<MoveInfo>();
        private List<MoveInfo> completedMoves = new List<MoveInfo>();

        private JsonObject jsonObject;

        private GameState gameState;
        private bool isPaused;

        private void Awake() {
           board = Chess.CreateBoard();
        }

        private void Start() {
            resources = gameObject.GetComponent<Resource>();
            move.Move.changePawn += ShowPieceSelectionMenu;
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
            var halfBoardSize = resources.halfBoardSize;
            var halfSIzeVec = new Vector3(halfBoardSize, halfBoardSize, halfBoardSize);
            var selectedPosFloat = hit.point - (boardPos - halfSIzeVec);
            var selectedPos = new Vector2Int((int)selectedPosFloat.x, (int)selectedPosFloat.z);

            var pieceOpt = board[selectedPos.x, selectedPos.y];
            GameState gameState = GameState.PieceSelected;
            if (pieceOpt.IsSome() && pieceOpt.Peel().color == whoseMove && !isPaused) {
                gameState = GameState.PieceNotSelected;
            }

            switch (gameState) {
                case GameState.PieceSelected:
                    if (!Physics.Raycast(ray, out hit, 100f, resources.highlightMask)) {
                        return;
                    }
                    var currentMove = GetCurrentMove(selectedPos);
                    Move(currentMove.first.from, currentMove.first.to, currentMove);
                    if (currentMove.second.HasValue) {
                        var secondMove = currentMove.second.Value;
                        Move(secondMove.from, secondMove.to, currentMove);
                    }
                    completedMoves.Add(currentMove);
                    var lastMove = completedMoves[completedMoves.Count - 1];
                    if(Chess.CheckChangePawn(board, lastMove)) {
                        resources.changePawn.SetActive(true);
                        isPaused = true;
                    }
                    if (!isPaused) {
                        whoseMove = Chess.ChangeMove(whoseMove);
                        if (Check.CheckMate(board, whoseMove, resources.movement, lastMove)) {
                            resources.gameMenu.SetActive(true);
                        }
                        canMovePos.Clear();
                    }
                    if (Chess.CheckDraw(completedMoves)) {
                        isPaused = true;
                        Debug.Log("draw");
                    }
                    selectedPiece = selectedPos;
                    DestroyHighlightCell();
                    break;
                case GameState.PieceNotSelected:
                    DestroyHighlightCell();
                    canMovePos.Clear();
                    lastMove = completedMoves[completedMoves.Count - 1];
                    selectedPiece = selectedPos;
                    canMovePos = Chess.GetPossibleMoves(
                        resources.movement,
                        selectedPiece,
                        board,
                        lastMove
                    );
                    HighlightCell(canMovePos);
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
            var x = selectedPiece.x;
            var y = selectedPiece.y;
            PieceType pieceType = (PieceType)type;

            Chess.ChangePiece(board, selectedPiece, pieceType, whoseMove);
            Destroy(piecesMap[x, y]);
            var piece = board[x, y];

            piecesMap[x, y] = GameObject.Instantiate(
                resources.pieceList[(int)piece.Peel().type * 2 + (int)piece.Peel().color],
                new Vector3(
                    x + boardPos.x - resources.halfBoardSize + resources.halfCellSize,
                    boardPos.y + resources.halfCellSize,
                    y + boardPos.z - resources.halfBoardSize + resources.halfCellSize
                ),
                Quaternion.identity,
                resources.boardObj.transform
            );
            isPaused = false;
            resources.changePawn.SetActive(false);
            whoseMove = Chess.ChangeMove(whoseMove);
        }

        private void Move(Vector2Int start, Vector2Int end, MoveInfo currentMove) {
            move.Move.MovePiece(start, end, board);

            var boardPos = resources.boardObj.transform.position;
            var sentencedPiece = currentMove.sentenced;
            if (sentencedPiece.HasValue) {
                Destroy(piecesMap[sentencedPiece.Value.x, sentencedPiece.Value.y]);
            }

            piecesMap[start.x, start.y].transform.position = new Vector3(
                end.x + boardPos.x - resources.halfBoardSize + resources.halfCellSize,
                boardPos.y + resources.halfCellSize,
                end.y + boardPos.z - resources.halfBoardSize + resources.halfCellSize
            );
            piecesMap[end.x, end.y] = piecesMap[start.x, start.y];
        }

        public void AddPiecesOnBoard() {
            DestroyPieces(piecesMap);
            var boardPos = resources.boardObj.transform.position;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board[i, j].Peel();

                    if (board[i, j].IsSome()) {
                        piecesMap[i, j] = GameObject.Instantiate(
                            resources.pieceList[(int)piece.type * 2 + (int)piece.color],
                            new Vector3(
                                i + boardPos.x - resources.halfBoardSize + resources.halfCellSize,
                                boardPos.y + resources.halfCellSize,
                                j + boardPos.z - resources.halfBoardSize + resources.halfCellSize
                            ),
                            
                            Quaternion.identity,
                            resources.boardObj.transform
                        );
                    }
                }
            }
        }

        private void HighlightCell(List<MoveInfo> canMovePos) {
            var boardPos = resources.boardObj.transform.position;
            var halfBoardSize = resources.halfBoardSize;
            var halfCellSize = resources.halfCellSize;

            foreach (var pos in canMovePos) {
                if (board[pos.first.to.x, pos.first.to.y].IsSome()) {
                    Instantiate(
                        resources.underAttackCell,
                        new Vector3(
                            pos.first.to.x + boardPos.x - halfBoardSize + halfCellSize,
                            boardPos.y + halfCellSize,
                            pos.first.to.y + boardPos.z - halfBoardSize + halfCellSize
                        ),
                        Quaternion.identity,
                        resources.storageHighlightCells.transform
                    );
                }
                Instantiate(
                    resources.canMoveCell,
                    new Vector3(
                        pos.first.to.x + boardPos.x - halfBoardSize + halfCellSize,
                        boardPos.y + halfCellSize,
                        pos.first.to.y + boardPos.z - halfBoardSize + halfCellSize
                    ),
                    Quaternion.identity,
                    resources.storageHighlightCells.transform
                );
            }
        }

        private void ShowPieceSelectionMenu() {
            resources.changePawn.SetActive(true);
            isPaused = true;
        }

        private void DestroyHighlightCell() {
            foreach (Transform child in resources.storageHighlightCells.transform) {
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
            foreach (var move in canMovePos) {
                if (move.first.to == selectedPos) {
                    return move;
                }
            }

            return new MoveInfo();
        }
    }
}