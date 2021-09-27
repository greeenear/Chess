using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
using System.Collections.Generic;
using System;

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
            Vector2Int targetPos,
            Option<Piece>[,] board,
            MoveInfo lastMove
        ) {
            if (board[targetPos.x, targetPos.y].IsNone()) {
                return null;
            }
            var targetPiece = board[targetPos.x, targetPos.y].Peel();
            var color = targetPiece.color;
            var checkInfos = Check.GetCheckInfo(board, color, Check.FindKing(board, color));

            bool isCheck = Check.IsCheck(checkInfos);

            if (targetPiece.type == PieceType.King) {
                return GetKingPossibleMoves(board, targetPos, lastMove, color);
            }

            foreach (var checkInfo in checkInfos) {
                if (checkInfo.coveringPiece == null) {
                    return GetСoveringMoves(targetPos, board, lastMove, checkInfo);
                }
                if (checkInfo.coveringPiece == targetPos && !isCheck) {
                    return GetNotOpeningMoves(targetPos, board, lastMove, checkInfo);
                }
            }

            var movementList = Move.GetRealMovements(board, targetPos);

            return move.Move.GetMoveInfos(movementList, targetPos, board, lastMove);;
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

            var movement = storage.Storage.movement[board[target.x, target.y].Peel().type];
            var kingMoves = move.Move.GetMoveInfos(movement, target, board, lastMove);
            foreach (var move in kingMoves) {
                var king = board[target.x, target.y];
                board[target.x, target.y] = Option<Piece>.None();
                var checkCellInfos = Check.GetCheckInfo(board, color, move.doubleMove.first.to);

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

        public static List<MoveInfo> GetСoveringMoves(
            Vector2Int target,
            Option<Piece>[,] board,
            MoveInfo lastMove,
            CheckInfo checkInfo
        ) {
            if (board[target.x, target.y].IsNone()) {
                return null;
            }
            var linearMovement = checkInfo.attackInfo.movement.linear;
            var movementList = new List<Movement>();
            var attakingPos = checkInfo.attackInfo.startPos;

            if (linearMovement.HasValue) {
                var dir = -linearMovement.Value.dir;
                var linearAttack = Linear.Mk(dir, linearMovement.Value.length);
                var linearMove = Linear.Mk(dir, linearMovement.Value.length);
                movementList.Add(Movement.Linear(linearAttack, MovementType.Attack));
                movementList.Add(Movement.Linear(linearMove, MovementType.Move));
            }

            var possibleAttackPos = move.Move.GetMoveInfos(
                movementList,
                checkInfo.attackInfo.startPos,
                board,
                lastMove
            );
            var doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(attakingPos, attakingPos));
            possibleAttackPos.Add(MoveInfo.Mk(doubleMove));

            var possibleDefensePos = move.Move.GetMoveInfos(
                storage.Storage.movement[board[target.x, target.y].Peel().type],
                target,
                board,
                lastMove
            );

            return GetListsIntersection<MoveInfo>(
                possibleDefensePos,
                possibleAttackPos,
                (first, second) => first.doubleMove.first.to == second.doubleMove.first.to
            );
        }

        public static List<T> GetListsIntersection<T>(
            List<T> firstList,
            List<T> secondList,
            Func<T, T, bool> comparator
        ) {
            var newList = new List<T>();
            foreach (var first in firstList) {
                foreach (var second in secondList) {
                    if (comparator(first, second)) {
                        newList.Add(first);
                    }
                }
            }

            return newList;
        }

        public static List<MoveInfo> GetNotOpeningMoves(
            Vector2Int target,
            Option<Piece>[,] board,
            MoveInfo lastMove,
            CheckInfo checkInfo
        ) {
            if (board[target.x, target.y].IsNone()) {
                return null;
            }
            var possibleMoves = new List<MoveInfo>();
            var movementList = new List<Movement>();
            var targetPiece = board[target.x, target.y].Peel();
            if (targetPiece.type == PieceType.Knight) {
                return possibleMoves;
            }

            var linear = checkInfo.attackInfo.movement.linear.Value;
            if (targetPiece.type == PieceType.Pawn) {
                movementList.Add(Movement.Linear(linear, MovementType.Attack));
            } else {
                movementList.Add(Movement.Linear(linear, MovementType.Attack));
                movementList.Add(Movement.Linear(linear, MovementType.Move));
            }
            Linear reverseDir = Linear.Mk(-linear.dir, linear.length);
            movementList.Add(Movement.Linear(reverseDir, MovementType.Attack));
            movementList.Add(Movement.Linear(reverseDir, MovementType.Move));

            return move.Move.GetMoveInfos(movementList, target, board, lastMove);
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            return (PieceColor)(((int)(whoseMove + 1) % (int)PieceColor.Count));
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
            List<MoveInfo> movesHistory,
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
                        var moves = GetPossibleMoves(
                            piecePos,
                            board,
                            movesHistory[movesHistory.Count -1]
                        );
                        if (moves.Count != 0) {
                            noCheckMate = true;
                            break;
                        }
                    }
                }
            }
            var kingPos = Check.FindKing(board, color);
            var checkInfo = Check.GetCheckInfo(board, color, kingPos);
            if (Check.IsCheck(checkInfo)) {
                gameStatus = GameStatus.Check;
            }

            if (!noCheckMate) {
                if (gameStatus == GameStatus.Check) {
                    gameStatus = GameStatus.CheckMate;
                } else {
                    gameStatus = GameStatus.StaleMate;
                }

                return gameStatus;
            }
            if (CheckDraw(movesHistory, noTakeMoves)) {
                gameStatus = GameStatus.Draw;
            }

            return gameStatus;
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