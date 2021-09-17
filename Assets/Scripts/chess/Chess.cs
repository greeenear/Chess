using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
using System.Collections.Generic;

namespace chess {
    public enum GameStatus {
        None,
        Check,
        CheckMate,
        StaleMate,
        Draw
    }
    public static class Chess {
        public static List<MoveInfo> GetPossibleMoves(
            Vector2Int targetPiece,
            Option<Piece>[,] board,
            MoveInfo lastMove
        ) {
            var possibleMoves = new List<MoveInfo>();
            List<Movement> movementList = new List<Movement>(); 
            var color = board[targetPiece.x, targetPiece.y].Peel().color;
            var movement = Storage.movement;
            var checkInfos = GetCheckInfo(board, color);


            foreach (var checkInfo in checkInfos) {
                if (checkInfo.coveringPiece == null) {
                    InsertPossibleMoves(targetPiece, board, lastMove, possibleMoves, checkInfo);
                    return possibleMoves;
                }
                if (checkInfo.coveringPiece == targetPiece) {
                    if (board[targetPiece.x , targetPiece.y].Peel().type == PieceType.Knight) {
                        return possibleMoves;
                    }
                    movementList.Add(Movement.Linear(checkInfo.attackInfo.movement.linear.Value));

                    return move.Move.GetMoveCells(movementList, targetPiece, board, lastMove);
                }
            }

            movementList = movement[board[targetPiece.x, targetPiece.y].Peel().type];
            possibleMoves = move.Move.GetMoveCells(movementList, targetPiece, board, lastMove);

            return possibleMoves;
        }

        public static void InsertPossibleMoves (
            Vector2Int target,
            Option<Piece>[,] board,
            MoveInfo lastMove,
            List<MoveInfo> possibleMoves,
            CheckInfo checkInfo
        ) {
            var linearMovement = checkInfo.attackInfo.movement.linear;
            var movementList = new List<Movement>();

            if (linearMovement == null) {
                return;
            }
            var dir = -linearMovement.Value.dir;
            movementList.Add(Movement.Linear(Linear.Mk(dir)));

            var possibleAttackPos = move.Move.GetMoveCells(
                movementList,
                checkInfo.attackInfo.startPos,
                board,
                lastMove
            );
            var possibleDefensePos = move.Move.GetMoveCells(
                Storage.movement[board[target.x, target.y].Peel().type],
                target,
                board,
                lastMove
            );

            foreach (var defense in possibleDefensePos) {
                    if (defense.doubleMove.first.to == checkInfo.attackInfo.startPos) {
                        possibleMoves.Add(defense);
                    }
                foreach (var attack in possibleAttackPos) {
                    if (attack.doubleMove.first.to == defense.doubleMove.first.to) {
                        possibleMoves.Add(defense);
                    }
                }
            }
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            return (PieceColor)(((int)(whoseMove + 1) % (int)PieceColor.Count));
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
            bool noCheckMate = false;
            gameStatus = GameStatus.None;

            for (int i = 0; i < board.GetLength(0); i++) {
                if (noCheckMate) {
                    break;
                }
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }

                    var piece = board[i, j].Peel();
                    if (piece.color == color && piece.type != PieceType.King) {
                        var piecePos = new Vector2Int(i, j);
                        var moves = GetPossibleMoves(piecePos, board, lastMove);
                        if (moves.Count != 0) {
                            noCheckMate = true;
                            break;
                        }
                    }
                }
            }
            var checkInfo = GetCheckInfo(board, color);
            foreach (var info in checkInfo) {
                if (info.coveringPiece == null) {
                    gameStatus = GameStatus.Check;
                }
            }
            if (!noCheckMate) {
                if (gameStatus == GameStatus.Check) {
                    gameStatus = GameStatus.CheckMate;
                } else {
                    gameStatus = GameStatus.StaleMate;
                }
                return gameStatus;
            }

            return gameStatus;
        }

        public static List<CheckInfo> GetCheckInfo(Option<Piece>[,] board, PieceColor color) {
            var kingPos = Check.FindKing(board, color);
            var kingMoveCounter = board[kingPos.x, kingPos.y].Peel().moveCounter;
            var movement = Storage.movement;

            var singleColorBoard = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, kingMoveCounter));
            singleColorBoard[kingPos.x, kingPos.y] = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard, movement, kingPos);
            var checkInfo = Check.AnalyzeAttackMovements(color, board, attackInfo, kingPos);

            return checkInfo;
        }

        public static Option<Piece>[,] GetBoardWithOneColor(
            PieceColor color,
            Option<Piece>[,] startBoard
        ) {
            Option<Piece>[,] board = (Option<Piece>[,])startBoard.Clone();

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i,j].IsSome()) {
                        var piece = board[i,j].Peel();
                        if (piece.color == color) {
                            board[i, j] = Option<Piece>.None();
                        }
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