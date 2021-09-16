using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
using System.Collections.Generic;

namespace chess {
    public struct GameStatus {
        public bool check;
        public bool checkMate;
        public bool draw;
    }
    public static class Chess {
        public static List<MoveInfo> GetPossibleMoves(
            Vector2Int pos,
            Option<Piece>[,] board,
            MoveInfo lastMove
        ) {
            var possibleMoves = new List<MoveInfo>();
            List<Movement> movementList = new List<Movement>(); 
            var color = board[pos.x, pos.y].Peel().color;
            var movement = Storage.movement;
            var checkInfo = CheckKing(board, color);

            foreach (var info in checkInfo) {
                if (info.coveringPiece == null) {
                    movementList.Clear();
                    var linearMovement = info.attackInfo.movement.linear;

                    if (linearMovement.HasValue) {
                        var dir = new Vector2Int(
                            -linearMovement.Value.dir.x,
                            -linearMovement.Value.dir.y
                        );
                        movementList.Add(Movement.Linear(Linear.Mk(dir)));
                    }

                    var possibleAttackPos = move.Move.GetMoveCells(
                        movementList,
                        info.attackInfo.startPos,
                        board,
                        lastMove
                    );
                    var possibleDefensePos = move.Move.GetMoveCells(
                        movement[board[pos.x, pos.y].Peel().type],
                        pos,
                        board,
                        lastMove
                    );

                    foreach (var defense in possibleDefensePos) {
                            if (defense.doubleMove.first.to == info.attackInfo.startPos) {
                                possibleMoves.Add(defense);
                            }
                        foreach (var attack in possibleAttackPos) {
                            if (attack.doubleMove.first.to == defense.doubleMove.first.to) {
                                possibleMoves.Add(defense);
                            }
                        }
                    }
                    return possibleMoves;
                }
                if (info.coveringPiece == pos) {
                    if (board[pos.x , pos.y].Peel().type == PieceType.Knight) {
                        return possibleMoves;
                    }
                    movementList.Add(Movement.Linear(info.attackInfo.movement.linear));

                    return move.Move.GetMoveCells(movementList, pos, board, lastMove);
                }
            }

            movementList = movement[board[pos.x, pos.y].Peel().type];
            possibleMoves = move.Move.GetMoveCells(movementList, pos, board, lastMove);

            return possibleMoves;
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            if (whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }

            return whoseMove;
        }

        public static bool CheckChangePawn(Option<Piece>[,] board, MoveInfo lastMove) {
            var last = board[lastMove.doubleMove.first.to.x, lastMove.doubleMove.first.to.y];
            var end = lastMove.doubleMove.first.to.x;
            if (last.Peel().type == PieceType.Pawn) {
                if (end == 0 || end == board.GetLength(1)-1) {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckDraw(List<MoveInfo> completedMoves, int noTakeMoves) {
            int moveCounter = 0;
            if (completedMoves.Count > 9) {
                var lastMove = completedMoves[completedMoves.Count - 1].doubleMove.first;

                for (int i = completedMoves.Count - 1; i > completedMoves.Count - 10; i = i - 2) {
                    if (completedMoves[i].doubleMove.first.to == lastMove.to 
                        && completedMoves[i].doubleMove.first.from == lastMove.from) {
                        moveCounter++;
                    }
                }
            }
            if (moveCounter == 3) {
                return true;
            }
            if (noTakeMoves == 50) {
                return true;
            }

            return false;
        }

        public static GameStatus GetCheckStatus(
            Option<Piece>[,] board,
            PieceColor color,
            MoveInfo lastMove
        ) {
            var possibleMoves = new List<MoveInfo>();
            var gameStatus = new GameStatus();
            bool isCheck = false;

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }

                    var piece = board[i, j].Peel();
                    if (piece.color == color && piece.type != PieceType.King) {
                        var piecePos = new Vector2Int(i, j);
                        possibleMoves.AddRange(GetPossibleMoves(piecePos, board, lastMove));
                    }
                }
            }
            var checkInfo = CheckKing(board, color);
            foreach (var info in checkInfo) {
                if (info.coveringPiece == null) {
                    isCheck = true;
                }
            }
            if (possibleMoves.Count == 0) {
                if (isCheck) {
                    Debug.Log("CheckMate");
                } else {
                    Debug.Log("Stalemate");
                }
                gameStatus.checkMate = true;
                return gameStatus;
            }
            if (isCheck) {
                gameStatus.check = true;
                Debug.Log("Check");
            }
            return gameStatus;
        }

        public static List<CheckInfo> CheckKing(Option<Piece>[,] board, PieceColor color) {
            var kingPos = Check.FindKing(board, color);
            var movement = Storage.movement;

            var singleColorBoard = DeletePiecesSameColor(color, board);
            var attackInfo = Check.GetPotentialChecks(color, singleColorBoard, movement, kingPos);
            var checkInfo = Check.GetCheckInfo(color, board, attackInfo, kingPos);

            return checkInfo;
        }

        public static Option<Piece>[,] DeletePiecesSameColor(
            PieceColor color,
            Option<Piece>[,] startBoard
        ) {
            Option<Piece>[,] board = (Option<Piece>[,])startBoard.Clone();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var piece = board[i,j].Peel();
                    if (piece.type == PieceType.King) {
                        continue;
                    }
                    if (board[i,j].IsSome() && piece.color == color) {
                        board[i, j] = Option<Piece>.None();
                    }
                }
            }

            return board;
        }

        public static void ChangePiece(
            Option<Piece>[,] board,
            Vector2Int pos,
            PieceType type,
            PieceColor color
        ) {
            board[pos.x, pos.y] = Option<Piece>.None();
            board[pos.x, pos.y] = Option<Piece>.Some(Piece.Mk(type, color, 0));
        }

        public static Option<Piece>[,] CreateBoard() {
            Option<Piece>[,] board = new Option<Piece>[8,8];
            board[0, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.Black, 0));
            board[0, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.Black, 0));
            board[0, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.Black, 0));
            board[0, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, PieceColor.Black, 0));
            board[0, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, PieceColor.Black, 0));
            board[0, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.Black, 0));
            board[0, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.Black, 0));
            board[0, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.Black, 0));

            for (int i = 0; i < 8; i++) {
                board[1, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, PieceColor.Black, 0));
            }

            board[7, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.White, 0));
            board[7, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.White, 0));
            board[7, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.White, 0));
            board[7, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, PieceColor.White, 0));
            board[7, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, PieceColor.White, 0));
            board[7, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, PieceColor.White, 0));
            board[7, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, PieceColor.White, 0));
            board[7, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, PieceColor.White, 0));

            for (int i = 0; i < 8; i++) {
                board[6, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, PieceColor.White, 0));
            }

            return board;
        }
    }
}