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
            MoveInfo lastMove,
            GameStatus gameStatus
        ) {
            var possibleMoves = new List<MoveInfo>();
            List<Movement> movementList = new List<Movement>(); 
            var color = board[targetPiece.x, targetPiece.y].Peel().color;
            var movement = storage.Storage.movement;
            var kingPos = Check.FindKing(board, color);
            var checkInfos = GetCheckInfo(board, color, kingPos);
            if (board[targetPiece.x, targetPiece.y].IsNone()) {
                return null;
            }

            if (board[targetPiece.x, targetPiece.y].Peel().type == PieceType.King) {
                return GetKingPossibleMoves(board, targetPiece, lastMove, color);
            }

            foreach (var checkInfo in checkInfos) {
                if (checkInfo.coveringPiece == null) {
                    InsertСoveringMoves(targetPiece, board, lastMove, possibleMoves, checkInfo);
                    return possibleMoves;
                }

                if (checkInfo.coveringPiece == targetPiece && gameStatus == GameStatus.None) {
                    if (board[targetPiece.x , targetPiece.y].Peel().type == PieceType.Knight) {
                        return possibleMoves;
                    }
                    movementList.Add(Movement.Linear(checkInfo.attackInfo.movement.linear.Value));
                    Linear reverseDir = Linear.Mk(-checkInfo.attackInfo.movement.linear.Value.dir);
                    movementList.Add(Movement.Linear(reverseDir));
                    return move.Move.GetMoveCells(movementList, targetPiece, board, lastMove);
                }
            }

            movementList = movement[board[targetPiece.x, targetPiece.y].Peel().type];

            return move.Move.GetMoveCells(movementList, targetPiece, board, lastMove);;
        }

        public static List<MoveInfo> GetKingPossibleMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            MoveInfo lastMove,
            PieceColor color
        ) {
            List<MoveInfo> newKingMoves = new List<MoveInfo>();
            if (board[target.x, target.y].IsNone()) {
                return null;
            }
            var kingMoves = move.Move.GetMoveCells(
                storage.Storage.movement[board[target.x, target.y].Peel().type],
                target,
                board,
                lastMove
            );
            foreach (var move in kingMoves) {
                var king = board[target.x, target.y];
                board[target.x, target.y] = Option<Piece>.None();
                var checkCellInfos = GetCheckInfo(board, color, move.doubleMove.first.to);
                board[target.x, target.y] = king;
                if (checkCellInfos.Count == 0) {
                    newKingMoves.Add(move);
                }
                foreach (var info in checkCellInfos) {
                    if (info.coveringPiece != null) {
                        newKingMoves.Add(move);
                    }
                }
            }

            return newKingMoves;
        }

        public static void InsertСoveringMoves (
            Vector2Int target,
            Option<Piece>[,] board,
            MoveInfo lastMove,
            List<MoveInfo> possibleMoves,
            CheckInfo checkInfo
        ) {
            var linearMovement = checkInfo.attackInfo.movement.linear;
            var movementList = new List<Movement>();
            if (board[target.x, target.y].IsNone()) {
                return;
            }

            if (linearMovement.HasValue) {
                var dir = -linearMovement.Value.dir;
                movementList.Add(Movement.Linear(Linear.Mk(dir)));
            }

            var possibleAttackPos = move.Move.GetMoveCells(
                movementList,
                checkInfo.attackInfo.startPos,
                board,
                lastMove
            );
            var possibleDefensePos = move.Move.GetMoveCells(
                storage.Storage.movement[board[target.x, target.y].Peel().type],
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
            if (last.IsNone()) {
                return false;
            }
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

        public static GameStatus GetGameStatus(
            Option<Piece>[,] board,
            PieceColor color,
            MoveInfo lastMove,
            List<MoveInfo> completedMoves,
            int noTakeMoves
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
                    if (piece.color == color) {
                        var piecePos = new Vector2Int(i, j);
                        var moves = GetPossibleMoves(piecePos, board, lastMove, gameStatus);
                        if (moves.Count != 0) {
                            noCheckMate = true;
                            break;
                        }
                    }
                }
            }
            var kingPos = Check.FindKing(board, color);
            var checkInfo = GetCheckInfo(board, color, kingPos);
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
            if(CheckDraw(completedMoves, noTakeMoves)) {
                gameStatus = GameStatus.Draw;
            }

            return gameStatus;
        }

        public static List<CheckInfo> GetCheckInfo(
            Option<Piece>[,] board,
            PieceColor color,
            Vector2Int cellPos
        ) {
            var movement = storage.Storage.movement;

            var singleColorBoard = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard[cellPos.x, cellPos.y] = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard, cellPos);
            var checkInfo = Check.AnalyzeAttackMovements(color, board, attackInfo, cellPos);

            return checkInfo; 
        }

        public static Option<Piece>[,] GetBoardWithOneColor(
            PieceColor color,
            Option<Piece>[,] startBoard
        ) {
            Option<Piece>[,] board = (Option<Piece>[,])startBoard.Clone();

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i,j].IsNone()) {
                        continue;
                    }
                    var piece = board[i,j].Peel();
                    if (piece.color == color) {
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