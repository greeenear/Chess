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
            CellInfo[,] board
        ) {
            if (board[targetPos.x, targetPos.y].piece.IsNone()) {
                return null;
            }
            var targetPiece = board[targetPos.x, targetPos.y].piece.Peel();
            var color = targetPiece.color;
            var checkInfos = Check.GetCheckInfo(board, color, Check.FindKing(board, color));

            bool isCheck = Check.IsCheck(checkInfos);

            if (targetPiece.type == PieceType.King) {
                return GetKingPossibleMoves(board, targetPos, color);
            }

            foreach (var checkInfo in checkInfos) {
                if (checkInfo.coveringPiece == null) {
                    return GetСoveringMoves(targetPos, board, checkInfo);
                }
                if (checkInfo.coveringPiece == targetPos && !isCheck) {
                    return GetNotOpeningMoves(targetPos, board, checkInfo);
                }
            }

            var movementList = Move.GetPieceMovements(board, targetPos);

            return move.Move.GetMoveInfos(movementList, targetPos, board);;
        }

        public static List<MoveInfo> GetKingPossibleMoves(
            CellInfo[,] board,
            Vector2Int target,
            PieceColor color
        ) {
            List<MoveInfo> newKingMoves = new List<MoveInfo>();
            if (board[target.x, target.y].piece.IsNone()) {
                return null;
            }

            var movement = Move.GetPieceMovements(board, target);
            var kingMoves = move.Move.GetMoveInfos(movement, target, board);
            foreach (var move in kingMoves) {
                var king = board[target.x, target.y];
                board[target.x, target.y].piece = Option<Piece>.None();
                var moveTo = move.doubleMove.first.to;
                var checkCellInfos = Check.GetCheckInfo(board, color, moveTo);

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
            CellInfo[,] board,
            CheckInfo checkInfo
        ) {
            if (board[target.x, target.y].piece.IsNone()) {
                return null;
            }
            var linearMovement = checkInfo.attackInfo.movement.linear;
            var movementList = new List<PieceMovement>();
            var attakingPos = checkInfo.attackInfo.startPos;

            if (linearMovement.HasValue) {
                var dir = -linearMovement.Value.dir;
                var linearAttack = Linear.Mk(dir, linearMovement.Value.length);
                var linearMove = Linear.Mk(dir, linearMovement.Value.length);
                var attackMovement = Movement.Linear(linearAttack, MovementType.Attack);
                var moveMovement = Movement.Linear(linearMove, MovementType.Move);
                movementList.Add(new PieceMovement {movement = attackMovement});
                movementList.Add(new PieceMovement {movement = moveMovement});
            }

            var possibleAttackPos = move.Move.GetMoveInfos(movementList, attakingPos, board);
            var doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(attakingPos, attakingPos));
            possibleAttackPos.Add(MoveInfo.Mk(doubleMove));

            var possibleDefensePos = move.Move.GetMoveInfos(
                Move.GetPieceMovements(board, target),
                target,
                board
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
            CellInfo[,] board,
            CheckInfo checkInfo
        ) {
            if (board[target.x, target.y].piece.IsNone()) {
                return null;
            }
            var possibleMoves = new List<MoveInfo>();
            var movementList = new List<PieceMovement>();
            var targetPiece = board[target.x, target.y].piece.Peel();
            if (targetPiece.type == PieceType.Knight) {
                return possibleMoves;
            }

            var linear = checkInfo.attackInfo.movement.linear.Value;
            foreach (var pieceMovement in Move.GetPieceMovements(board, target)) {
                if (pieceMovement.movement.linear.Value.dir == linear.dir) {
                    movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                }
                if (pieceMovement.movement.linear.Value.dir == -linear.dir) {
                    movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                }
            }

            return move.Move.GetMoveInfos(movementList, target, board);
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
            CellInfo[,] board,
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
                    if (board[i, j].piece.IsNone()) {
                        continue;
                    }

                    var piece = board[i, j].piece.Peel();
                    if (piece.color == color) {
                        var piecePos = new Vector2Int(i, j);
                        var moves = GetPossibleMoves(piecePos, board);
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
            CellInfo[,] board,
            Vector2Int pos,
            PieceType type,
            PieceColor color
        ) {
            board[pos.x, pos.y].piece = Option<Piece>.None();
            board[pos.x, pos.y].piece = Option<Piece>.Some(Piece.Mk(type, color, 0));
        }

        public static CellInfo[,] CreateBoard() {
            CellInfo[,] board = new CellInfo[8,8];
            var blackColor = PieceColor.Black;
            var whiteColor = PieceColor.White;
            board[0, 0].piece = Option<Piece>.Some(Piece.Mk(PieceType.Rook, blackColor, 0));
            board[0, 1].piece = Option<Piece>.Some(Piece.Mk(PieceType.Knight, blackColor, 0));
            board[0, 2].piece = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, blackColor, 0));
            board[0, 4].piece = Option<Piece>.Some(Piece.Mk(PieceType.King, blackColor, 0));
            board[0, 3].piece = Option<Piece>.Some(Piece.Mk(PieceType.Queen, blackColor, 0));
            board[0, 5].piece = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, blackColor, 0));
            board[0, 6].piece = Option<Piece>.Some(Piece.Mk(PieceType.Knight, blackColor, 0));
            board[0, 7].piece = Option<Piece>.Some(Piece.Mk(PieceType.Rook, blackColor, 0));

            for (int i = 0; i < 8; i++) {
                board[1, i].piece = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, blackColor, 0));
            }

            board[7, 0].piece = Option<Piece>.Some(Piece.Mk(PieceType.Rook, whiteColor, 0));
            board[7, 1].piece = Option<Piece>.Some(Piece.Mk(PieceType.Knight, whiteColor, 0));
            board[7, 2].piece = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, whiteColor, 0));
            board[7, 4].piece = Option<Piece>.Some(Piece.Mk(PieceType.King, whiteColor, 0));
            board[7, 3].piece = Option<Piece>.Some(Piece.Mk(PieceType.Queen, whiteColor, 0));
            board[7, 5].piece = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, whiteColor, 0));
            board[7, 6].piece = Option<Piece>.Some(Piece.Mk(PieceType.Knight, whiteColor, 0));
            board[7, 7].piece = Option<Piece>.Some(Piece.Mk(PieceType.Rook, whiteColor, 0));

            for (int i = 0; i < 8; i++) {
                board[6, i].piece = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, whiteColor, 0));
            }

            return board;
        }
    }
}