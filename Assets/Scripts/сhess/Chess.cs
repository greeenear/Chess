using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
using System.Collections.Generic;

namespace chess {
    public struct Castling {
        public Vector2Int kingPos;
        public Vector2Int? rookPos;
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
            var checkInfo = Check.GetCheckInfo(color, board, Storage.movement);

            foreach (var info in checkInfo) {
                if (info.attackingPiecePos.HasValue) {
                    movementList.Clear();
                    if (info.linear.HasValue) {
                        var dir = new Vector2Int(-info.linear.Value.dir.x,-info.linear.Value.dir.y);
                        movementList.Add(Movement.Linear(Linear.Mk(dir)));
                    }

                    var possibleAttackPos = move.Move.GetMoveCells(
                        movementList,
                        info.attackingPiecePos.Value,
                        board,
                        lastMove
                    );
                    var possibleDefensePos = move.Move.GetMoveCells(
                        Storage.movement[board[pos.x, pos.y].Peel().type],
                        pos,
                        board,
                        lastMove
                    );

                    foreach (var defense in possibleDefensePos) {
                            if (defense.doubleMove.first.to == info.attackingPiecePos.Value) {
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
                    movementList.Add(Movement.Linear(info.linear));
                    
                }
            }

            if (movementList.Count == 0) {
                movementList = Storage.movement[board[pos.x, pos.y].Peel().type];
            }
            possibleMoves = move.Move.GetMoveCells(movementList, pos, board, lastMove);

            return possibleMoves;
        }

        public static PieceColor ChangeMove(
            PieceColor whoseMove,
            Option<Piece>[,] board
        ) {
            if (whoseMove == PieceColor.White) {
                whoseMove = PieceColor.Black;
            } else {
                whoseMove = PieceColor.White;
            }
            foreach (var checkInfo in Check.GetCheckInfo(whoseMove, board, Storage.movement)) {
                if (checkInfo.attackingPiecePos.HasValue) {
                    Debug.Log("Check");
                }
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

        public static bool CheckDraw(List<MoveInfo> completedMoves, int movesWithoutTaking) {
            int moveCounter = 0;
            if (completedMoves.Count > 10) {
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
            if(movesWithoutTaking == 50) {
                return true;
            }

            return false;
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