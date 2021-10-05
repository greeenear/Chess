using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
using System.Collections.Generic;
using System;
using movement;

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
            FullBoard board
        ) {
            var boardOpt = board.board;
            if (boardOpt[targetPos.x, targetPos.y].IsNone()) {
                return null;
            }
            var targetPiece = boardOpt[targetPos.x, targetPos.y].Peel();
            var color = targetPiece.color;
            var checkInfos = Check.GetCheckInfo(boardOpt, color, Check.FindKing(boardOpt, color));

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

            var movementList = MovementEngine.GetPieceMovements(boardOpt, targetPos);

            return Move.GetMoveInfos(movementList, targetPos, board);;
        }

        public static List<MoveInfo> GetKingPossibleMoves(
            FullBoard board,
            Vector2Int target,
            PieceColor color
        ) {
            List<MoveInfo> newKingMoves = new List<MoveInfo>();
            if (board.board[target.x, target.y].IsNone()) {
                return null;
            }

            var movement = MovementEngine.GetPieceMovements(board.board, target);
            var kingMoves = move.Move.GetMoveInfos(movement, target, board);
            foreach (var move in kingMoves) {
                var king = board.board[target.x, target.y];
                board.board[target.x, target.y] = Option<Piece>.None();
                var moveTo = move.doubleMove.first.to;
                var checkCellInfos = Check.GetCheckInfo(board.board, color, moveTo);

                board.board[target.x, target.y] = king;
                if (check.Check.IsCheck(checkCellInfos)) {
                    continue;
                }
                newKingMoves.Add(move);

            }

            return newKingMoves;
        }

        public static List<MoveInfo> GetСoveringMoves(
            Vector2Int target,
            FullBoard board,
            CheckInfo checkInfo
        ) {
            if (board.board[target.x, target.y].IsNone()) {
                return null;
            }
            var linearMovement = checkInfo.attackInfo.movement.linear;
            var movementList = new List<MoveInfo>();
            var attackPos = checkInfo.attackInfo.startPos;
            var lastPos = new Vector2Int();

            if (linearMovement.HasValue) {
                var dir = -linearMovement.Value.dir;
                var linear = Linear.Mk(dir, linearMovement.Value.length);
                var length = Board.GetLinearLength(attackPos, linear, board.board);
                lastPos = attackPos + linear.dir * length;
            }

            var defenseMovements = MovementEngine.GetPieceMovements(board.board, target);
            foreach (var defenseMovement in defenseMovements) {
                if (defenseMovement.movement.movement.circular.HasValue) {
                    var angle = 0f;
                    var startAngle = StartAngle.Knight;
                    var circular = defenseMovement.movement.movement.circular.Value;
                    for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                        angle = startAngle * i * Mathf.PI / 180;
                        var cell = Board.GetCircularMove(target, circular, angle, board.board);
                        if (cell.HasValue && IsPointOnSegment(attackPos, lastPos, cell.Value)) {
                            var moveData = MoveData.Mk(target, cell.Value);
                            var doubleMove = DoubleMove.MkSingleMove(moveData);
                            movementList.Add(MoveInfo.Mk(doubleMove));
                        }
                    }
                }
                if (defenseMovement.movement.movement.linear.HasValue) {
                    var linear = defenseMovement.movement.movement.linear.Value;
                    var length = Board.GetLinearLength(target, linear, board.board);
                    var lastDefPos = target + linear.dir * length;
                    var point = GetSegmentsIntersection(attackPos, lastPos, target, lastDefPos);
                    if (point.HasValue) {
                        var doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(target, point.Value));

                        if (defenseMovement.movementType == MovementType.Attack) {
                            var pieceOpt = board.board[point.Value.x, point.Value.y];
                            if (pieceOpt.IsSome()) {
                                var piece = pieceOpt.Peel();
                                if (piece.color != board.board[target.x, target.y].Peel().color) {
                                    var moveInfo = MoveInfo.Mk(doubleMove);
                                    moveInfo.sentenced = point;
                                    movementList.Add(moveInfo);
                                }
                            }
                        } else if (defenseMovement.movementType == MovementType.Move) {
                            if (board.board[point.Value.x, point.Value.y].IsNone()) {
                                movementList.Add(MoveInfo.Mk(doubleMove));
                            }
                        }
                    }
                }
            }
            return movementList;
        }

        public static Vector2Int? GetSegmentsIntersection(
            Vector2Int start1,
            Vector2Int end1,
            Vector2Int start2,
            Vector2Int end2
        ) {
            int x = 0;
            int y = 0;
            int a = 0;
            int b = 0;
            int c = 0;
            int d = 0;
            if (end2.x - start2.x == 0 && end1.x - start1.x == 0) {
                return null;
            } else if (end2.x - start2.x == 0) {
                x = start2.x;
                a = (end1.y - start1.y) / (end1.x - start1.x);
                b = start1.y - (end1.y - start1.y) / (end1.x - start1.x) * start1.x;
                y = a * x + b;
            } else if (end1.x - start1.x == 0) {
                x = start1.x;
                c = (end2.y - start2.y) / (end2.x - start2.x);
                d = start2.y - (end2.y - start2.y) / (end2.x - start2.x) * start2.x;
                y = c * x + d;
            } else {
                a = (end1.y - start1.y) / (end1.x - start1.x);
                b = start1.y - (end1.y - start1.y) / (end1.x - start1.x) * start1.x;
                c = (end2.y - start2.y) / (end2.x - start2.x);
                d = start2.y - (end2.y - start2.y) / (end2.x - start2.x) * start2.x;
                if (a - c == 0) {
                    return null;
                }
                x = (d - b) / (a - c);
                if (x != 1.0*(d - b) / (a - c)) {
                    return null;
                }
                y = a * x + b;
            }
            var point = new Vector2Int(x, y);
            if (IsPointOnSegment(start2, end2, point) && IsPointOnSegment(start1, end1, point)) {
                return point;
            }
            return null;
        }

        public static bool IsPointOnSegment(Vector2Int start, Vector2Int end, Vector2Int point) {
            var x = point.x;
            var y = point.y;

            double a = end.y - start.y;
            double b = start.x - end.x;
            double c = - a * start.x - b * start.y;
            if (Math.Abs(a * point.x + b * point.y + c) > 0) {
                return false;
            }
            if ((x >= start.x && x <= end.x || x <= start.x && x >= end.x) 
                && (y >= start.y && y <= end.y || y <= start.y && y >= end.y)) {
                return true;
            } else {
                return false;
            }
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
            FullBoard board,
            CheckInfo checkInfo
        ) {
            if (board.board[target.x, target.y].IsNone()) {
                return null;
            }
            var possibleMoves = new List<MoveInfo>();
            var movementList = new List<PieceMovement>();
            var targetPiece = board.board[target.x, target.y].Peel();
            if (targetPiece.type == PieceType.Knight) {
                return possibleMoves;
            }

            var linear = checkInfo.attackInfo.movement.linear.Value;
            foreach (var pieceMovement in MovementEngine.GetPieceMovements(board.board, target)) {
                if (pieceMovement.movement.movement.linear.Value.dir == linear.dir) {
                    movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                }
                if (pieceMovement.movement.movement.linear.Value.dir == -linear.dir) {
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
            FullBoard board,
            PieceColor color,
            List<MoveInfo> movesHistory,
            int noTakeMoves
        ) {
            var possibleMoves = new List<MoveInfo>();
            var gameStatus = new GameStatus();
            bool noCheckMate = false;
            gameStatus = GameStatus.None;

            for (int i = 0; i < board.board.GetLength(0); i++) {
                if (noCheckMate) {
                    break;
                }
                for (int j = 0; j < board.board.GetLength(1); j++) {
                    if (board.board[i, j].IsNone()) {
                        continue;
                    }

                    var piece = board.board[i, j].Peel();
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
            var kingPos = Check.FindKing(board.board, color);
            var checkInfo = Check.GetCheckInfo(board.board, color, kingPos);
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
            var blackColor = PieceColor.Black;
            var whiteColor = PieceColor.White;
            board[0, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, blackColor, 0));
            board[0, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, blackColor, 0));
            board[0, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, blackColor, 0));
            board[0, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, blackColor, 0));
            board[0, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, blackColor, 0));
            board[0, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, blackColor, 0));
            board[0, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, blackColor, 0));
            board[0, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, blackColor, 0));

            for (int i = 0; i < 8; i++) {
                board[1, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, blackColor, 0));
            }

            board[7, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, whiteColor, 0));
            board[7, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, whiteColor, 0));
            board[7, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, whiteColor, 0));
            board[7, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, whiteColor, 0));
            board[7, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, whiteColor, 0));
            board[7, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, whiteColor, 0));
            board[7, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, whiteColor, 0));
            board[7, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, whiteColor, 0));

            for (int i = 0; i < 8; i++) {
                board[6, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, whiteColor, 0));
            }

            return board;
        }
    }
}