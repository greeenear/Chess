using UnityEngine;
using rules;
using option;
using check;
using move;
using board;
using System.Collections.Generic;

namespace chess {
    public class Chess : MonoBehaviour {
        public static Vector2Int? CheckEnPassant(Vector2Int endPos, Option<Piece>[,] board) {
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

        public static MoveRes Move(
            Vector2Int start,
            Vector2Int end,
            Vector2Int? enPassant,
            List<Vector2Int> canMovePos,
            Option<Piece>[,] board,
            GameObject boardObj,
            GameObject[,] piecesMap
        ) {
            var offset = boardObj.transform.position;
            var moveRes = move.Move.CheckMove(start, end, canMovePos, board);
            
            if(moveRes.pos != null) {
                var x = moveRes.pos.Value.x;
                var y = moveRes.pos.Value.y;

                if (moveRes.isPieceOnPos) {
                    Destroy(piecesMap[moveRes.pos.Value.x, moveRes.pos.Value.y]);
                }
                board[x, y] = board[start.x, start.y];
                board[start.x, start.y] = Option<Piece>.None();
                piecesMap[x, y] = piecesMap[start.x, start.y];

                piecesMap[x, y].transform.position =
                new Vector3(x + offset.x - 4 + 0.5f, offset.y + 0.5f, y + offset.z - 4 + 0.5f);

                if (board[x, y].Peel().type == PieceType.Pawn) {
                    var possibleEnPassant = new Vector2Int(start.x, y);

                    if (enPassant != null && Equals(enPassant, possibleEnPassant)) {
                        board[start.x, y] = Option<Piece>.None();
                        Destroy(piecesMap[start.x, y]);
                    }
                    if (Mathf.Abs(start.x - x) == 2) {
                        moveRes.enPassant = Chess.CheckEnPassant(end, board);
                        return moveRes;
                    }
                }
                moveRes.enPassant = null;

                return moveRes;
            }

            return moveRes;
        }
        public static string Check(
            Option<Piece>[,] board,
            Vector2Int selectedPos,
            PieceColor whoseMove,
            Dictionary<PieceType,List<Movement>> movement,
            Vector2Int? enPassant
        ) {
            string checkRes = null;

            if (check.Check.CheckKing(board, whoseMove, movement,enPassant)) {
                checkRes = "CheckKing";
            }
            if (check.Check.CheckMate(board, selectedPos, whoseMove, movement, enPassant)) {
                checkRes = "CheckMate";
            }

            return checkRes;
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            if (whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }

            return whoseMove;
        }

        public static void ChangePiece(
            int type,
            GameObject boardObj,
            Vector2Int pos,
            GameObject[,] pieceGameObjects,
            Option<Piece>[,] board,
            List<GameObject> piecesObjList,
            PieceColor whoseMove
        ) {
            var boardPos = boardObj.transform.position;
            var x = pos.x;
            var y = pos.y;

            Destroy(pieceGameObjects[x,y]);
            PieceType pieceType = (PieceType)type;
            board[x, y] = Option<Piece>.Some(Piece.Mk(pieceType, whoseMove));

            var piece = board[x, y].Peel();
            pieceGameObjects[pos.x, pos.y] = Instantiate(
                piecesObjList[(int)piece.type * 2 + (int)piece.color],
                new Vector3(
                    x + boardPos.x - 4 + 0.5f,
                    boardPos.y + 0.5f,
                    y + boardPos.z - 4 + 0.5f
                ),
                Quaternion.identity,
                boardObj.transform
            );
        }

        public static void AddPiecesOnBoard(
            GameObject[,] piecesMap,
            GameObject boardObj,
            List<GameObject> pieceList,
            Option<Piece>[,] board
        ) {
            DestroyPieces(piecesMap);
            var boardPos = boardObj.transform.position;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board[i, j].Peel();

                    if (board[i, j].IsSome()) {
                        piecesMap[i, j] = Instantiate(
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

        private static void DestroyPieces(GameObject[,] piecesMap) {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    Destroy(piecesMap[i,j]);
                }
            }
        }

        public static void ShowCanMoveCells(
            List<Vector2Int> canMovePos,
            GameObject boardObj,
            Option<Piece>[,] board,
            GameObject canMoveCell,
            List<GameObject> canMoveCells
        ) {
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

        public static void RemoveCanMoveCells(List<GameObject> canMoveCells) {
            foreach (GameObject cell in canMoveCells) {
                Destroy(cell);
            }
        }

        public static Option<Piece>[,] CreateBoard() {
            Option<Piece>[,] board = new Option<Piece>[8,8];
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

            return board;
        }
    }
}


