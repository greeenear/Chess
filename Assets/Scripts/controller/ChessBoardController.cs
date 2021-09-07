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

        private bool isPaused;

        private JsonObject jsonObject;

        private void Awake() {
           board = Chess.CreateBoard();
        }

        private void Start() {
            resources = gameObject.GetComponent<Resource>();
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

        public void Load() {
            var gameInfo = SaveLoad.LoadFromJson("json.json", jsonObject);
            board = new Option<Piece>[8,8];

            whoseMove = gameInfo.gameStats.whoseMove;
            foreach (var pieceInfo in gameInfo.pieceInfo) {
                board[pieceInfo.xPos, pieceInfo.yPos] = Option<Piece>.Some(pieceInfo.piece);
            }
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
            Vector2Int selectedPos = new Vector2Int(
                (int)(hit.point.x - (boardPos.x - resources.halfBoardSize)),
                (int)(hit.point.z - (boardPos.z - resources.halfBoardSize))
            );

            var pieceOpt = board[selectedPos.x, selectedPos.y];
            GameState gameState = new GameState();
            if (pieceOpt.IsSome() && pieceOpt.Peel().color == whoseMove && !isPaused) {
                gameState = GameState.PieceNotSelected;
            }

            switch (gameState) {
                case GameState.PieceSelected:
                    int layerMask = 1 << 3;
                    if (!Physics.Raycast(ray, out hit, 100f, layerMask)) {
                        return;
                    }
                    if (!CheckCastling(selectedPiece, selectedPos)) {
                        Move(selectedPiece, selectedPos);
                    }
                    if(!isPaused) {
                        whoseMove = Chess.ChangeMove(whoseMove);
                        Check.CheckMate(board, whoseMove, resources.movement);
                        canMovePos.Clear();
                    }
                    selectedPiece = selectedPos;
                    DestroyHighLightCell();
                    break;
                case GameState.PieceNotSelected:
                    DestroyHighLightCell();
                    canMovePos.Clear();

                    selectedPiece = selectedPos;
                    canMovePos = Chess.GetPossibleMoveCells(
                        resources.movement,
                        selectedPiece,
                        board
                    );
                    HighLightCell(canMovePos);
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

        private void Move(Vector2Int start, Vector2Int end) {
            MoveInfo currentMove = new MoveInfo();
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y] = Option<Piece>.None();
            var piece = board[end.x, end.y].Peel();
            piece.moveCounter++;
            board[end.x, end.y] = Option<Piece>.Some(piece);

            foreach (var move in canMovePos) {
                if (move.end == end) {
                    currentMove = move;
                }
            }
            var boardPos = resources.boardObj.transform.position;
            var deletedPiece = currentMove.deletedPiece;
            if (deletedPiece != null) {
                Destroy(piecesMap[deletedPiece.Value.x, deletedPiece.Value.y]);
            }

            piecesMap[start.x, start.y].transform.position = new Vector3(
                end.x + boardPos.x - resources.halfBoardSize + resources.halfCellSize,
                boardPos.y + resources.halfCellSize,
                end.y + boardPos.z - resources.halfBoardSize + resources.halfCellSize
            );
            piecesMap[end.x, end.y] = piecesMap[start.x, start.y];

            if (board[end.x, end.y].Peel().type == PieceType.Pawn) {
                if (end.x == 0 || end.x == board.GetLength(1)-1) {
                    isPaused = true;
                    resources.changePawn.SetActive(true);
                }
            }

        }

        private bool CheckCastling(Vector2Int start, Vector2Int end) {
            if (board[start.x, start.y].Peel().type == PieceType.King) {
                int castlingDir = end.y - start.y;

                if (Mathf.Abs(castlingDir) == 2) {
                    if(castlingDir < 0) {
                        Move(new Vector2Int(start.x, 0), new Vector2Int(start.x, 3));
                    } else {
                        Move(new Vector2Int(start.x, 7), new Vector2Int(start.x, 5));
                    }
                    Move(start, end);

                    return true;
                }
            }
            return false;
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

        private void HighLightCell(List<MoveInfo> canMovePos) {
            var boardPos = resources.boardObj.transform.position;
            foreach (var pos in canMovePos) {
                Instantiate(
                    resources.canMoveCell,
                    new Vector3(
                        pos.end.x + boardPos.x - resources.halfBoardSize + resources.halfCellSize,
                        boardPos.y + resources.halfCellSize,
                        pos.end.y + boardPos.z - resources.halfBoardSize + resources.halfCellSize
                    ),
                    Quaternion.identity,
                    resources.storageHighlightCells.transform
                );
            }
        }

        private void DestroyHighLightCell() {
            foreach (Transform child in resources.storageHighlightCells.transform) {
                Destroy(child.gameObject);
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