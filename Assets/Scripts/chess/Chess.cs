using System.ComponentModel;
using System.IO;
using UnityEngine;
using rules;
using option;
using move;
using board;
using check;
using System.Collections.Generic;
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
        public static (List<MoveInfo>, Errors) GetPossibleMoves(
            Vector2Int targetPos,
            FullBoard board
        ) {
            var boardOpt = board.board;
            if (boardOpt == null) {
                return (null, Errors.BoardIsNull);
            }
            if (boardOpt[targetPos.x, targetPos.y].IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var targetPiece = boardOpt[targetPos.x, targetPos.y].Peel();
            var color = targetPiece.color;
            var kingPos = Check.FindKing(boardOpt, color);
            if (kingPos.Item2 != Errors.None) {
                Debug.Log(kingPos.Item2);
            }
            var checkInfos = Check.GetCheckInfo(boardOpt, color, kingPos.Item1);
            if (checkInfos.Item2 != Errors.None) {
                Debug.Log(checkInfos.Item2);
            }
            bool isCheck = Check.IsCheck(checkInfos.Item1).Item1;
            var pieceType = targetPiece.type;
            if (pieceType == PieceType.King) {
                var kingPossibleMoves = GetKingPossibleMoves(board, targetPos, color);
                if (kingPossibleMoves.Item2 != Errors.None) {
                    Debug.Log(kingPossibleMoves.Item2);
                }
                return (kingPossibleMoves.Item1, Errors.None);
            }

            foreach (var checkInfo in checkInfos.Item1) {
                if (!checkInfo.coveringPos.HasValue) {
                    var coveringMoves = GetСoveringMoves(targetPos, board, checkInfo);
                    if (coveringMoves.Item2 != Errors.None) {
                        Debug.Log(coveringMoves.Item2);
                    }
                    return (coveringMoves.Item1, Errors.None);
                }
                if (checkInfo.coveringPos == targetPos && !isCheck) {
                    var notOpenning = GetNotOpeningMoves(targetPos, board, checkInfo);
                    if (notOpenning.Item2 != Errors.None) {
                        Debug.Log(notOpenning.Item2);
                    }
                    return (notOpenning.Item1, Errors.None);
                }
            }
            var movementList = MovementEngine.GetPieceMovements(boardOpt, pieceType, targetPos);
            if (movementList.Item2 != Errors.None) {
                Debug.Log(movementList.Item2);
            }
            var moveInfos = Move.GetMoveInfos(movementList.Item1, targetPos, board);
            if (moveInfos.Item2 != Errors.None) {
                Debug.Log(moveInfos.Item2);
            }
            return (Move.GetMoveInfos(movementList.Item1, targetPos, board).Item1, Errors.None);
        }

        public static (List<MoveInfo>, Errors) GetKingPossibleMoves(
            FullBoard board,
            Vector2Int target,
            PieceColor color
        ) {
            List<MoveInfo> newKingMoves = new List<MoveInfo>();
            if (board.board == null) {
                return (null, Errors.BoardIsNull);
            }
            var pieceOpt = board.board[target.x, target.y];
            if (pieceOpt.IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var piece = pieceOpt.Peel();
            var movement = MovementEngine.GetPieceMovements(board.board, piece.type, target);
            if (movement.Item2 != Errors.None) {
                Debug.Log(movement.Item2);
                return (null, movement.Item2);
            }
            var kingMoves = move.Move.GetMoveInfos(movement.Item1, target, board);
            if (kingMoves.Item2 != Errors.None) {
                Debug.Log(kingMoves.Item2);
            }
            foreach (var move in kingMoves.Item1) {
                var king = pieceOpt;
                pieceOpt = Option<Piece>.None();
                var moveTo = move.doubleMove.first.to;
                var checkCellInfos = Check.GetCheckInfo(board.board, color, moveTo);
                if (checkCellInfos.Item2 != Errors.None) {
                    Debug.Log(checkCellInfos.Item2);
                }

                pieceOpt = king;
                if (check.Check.IsCheck(checkCellInfos.Item1).Item1) {
                    continue;
                }
                newKingMoves.Add(move);
            }
            return (newKingMoves, Errors.None);
        }

        public static (List<MoveInfo>, Errors) GetСoveringMoves(
            Vector2Int target,
            FullBoard board,
            CheckInfo checkInfo
        ) {
            var boardOpt = board.board;
            if (boardOpt == null) {
                return (null, Errors.BoardIsNull);
            }
            if (boardOpt[target.x, target.y].IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var linearMovement = checkInfo.attackInfo.movement.linear;
            var movementList = new List<MoveInfo>();
            var attackPos = checkInfo.attackInfo.startPos;
            var lastAttackPos = new Vector2Int();

            if (linearMovement.HasValue) {
                var dir = -linearMovement.Value.dir;
                var linear = Linear.Mk(dir, linearMovement.Value.length);
                var length = Board.GetLinearLength(attackPos, linear, boardOpt);
                if (length.Item2 != Errors.None) {
                        Debug.Log(length.Item2);
                    }
                lastAttackPos = attackPos + linear.dir * length.Item1;
            }
            var pieceType = boardOpt[target.x, target.y].Peel().type;
            var defenseMovements = MovementEngine.GetPieceMovements(boardOpt, pieceType, target);
            if (defenseMovements.Item2 != Errors.None) {
                Debug.Log(defenseMovements.Item2);
            }
            foreach (var defenseMovement in defenseMovements.Item1) {
                if (defenseMovement.movement.movement.circular.HasValue) {
                    var angle = 0f;
                    var startAngle = StartAngle.Knight;
                    var circular = defenseMovement.movement.movement.circular.Value;
                    for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                        angle = startAngle * i * Mathf.PI / 180;
                        var cell = Board.GetCircularPoint(target, circular, angle, boardOpt);
                        if (cell.Item2 != Errors.None) {
                            Debug.Log(cell.Item2);
                        }
                        if (!cell.Item1.HasValue) {
                            continue;
                        }
                        var segment = math.Math.Segment.Mk(attackPos, lastAttackPos);
                        if (math.Math.IsPointOnSegment(segment, cell.Item1.Value)) {
                            var moveData = MoveData.Mk(target, cell.Item1.Value);
                            var doubleMove = DoubleMove.MkSingleMove(moveData);
                            movementList.Add(MoveInfo.Mk(doubleMove));
                        }
                    }
                }
                if (defenseMovement.movement.movement.linear.HasValue) {
                    var linear = defenseMovement.movement.movement.linear.Value;
                    var length = Board.GetLinearLength(target, linear, board.board);
                    if (length.Item2 != Errors.None) {
                        Debug.Log(length.Item2);
                    }
                    var lastDefPos = target + linear.dir * length.Item1;
                    var firstSegment = math.Math.Segment.Mk(attackPos, lastAttackPos);
                    var secondSegment = math.Math.Segment.Mk(target, lastDefPos);
                    var n1 = math.Math.GetNormalVector(firstSegment);
                    var n2 = math.Math.GetNormalVector(secondSegment);
                    if (!n1.HasValue || !n2.HasValue) {
                        continue;
                    }
                    var firstLineCoefficients = math.Math.GetLineCoefficients(n1.Value, attackPos);
                    var secondLineCoefficients = math.Math.GetLineCoefficients(n2.Value, target);
                    var point = math.Math.GetSegmentsIntersection(
                        firstLineCoefficients,
                        secondLineCoefficients
                    );
                    if (!point.HasValue) {
                        continue;
                    }
                    if (!math.Math.IsPointOnSegment(firstSegment, point.Value)) {
                        continue;
                    }
                    if (!math.Math.IsPointOnSegment(secondSegment, point.Value)) {
                        continue;
                    }
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
            return (movementList, Errors.None);
        }

        public static (List<MoveInfo>, Errors) GetNotOpeningMoves(
            Vector2Int target,
            FullBoard board,
            CheckInfo checkInfo
        ) {
            if (board.board == null) {
                return (null, Errors.BoardIsNull);
            }
            if (board.board[target.x, target.y].IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var possibleMoves = new List<MoveInfo>();
            var movementList = new List<PieceMovement>();
            var targetPiece = board.board[target.x, target.y].Peel();
            if (targetPiece.type == PieceType.Knight) {
                return (possibleMoves, Errors.None);
            }
            var type = targetPiece.type;
            var linear = checkInfo.attackInfo.movement.linear.Value;
            var pieceMovements = MovementEngine.GetPieceMovements(board.board, type, target);
            if (pieceMovements.Item2 != Errors.None) {
                Debug.Log(pieceMovements.Item2);
            }
            foreach (var pieceMovement in pieceMovements.Item1) {
                if (pieceMovement.movement.movement.linear.Value.dir == linear.dir) {
                    movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                }
                if (pieceMovement.movement.movement.linear.Value.dir == -linear.dir) {
                    movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                }
            }
            var moveInfos = move.Move.GetMoveInfos(movementList, target, board);
            if (moveInfos.Item2 != Errors.None) {
                Debug.Log(moveInfos.Item2);
            }
            return (moveInfos.Item1, Errors.None);
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            return (PieceColor)(((int)(whoseMove + 1) % (int)PieceColor.Count));
        }

        public static (bool, Errors) CheckDraw(List<MoveInfo> completedMoves, int noTakeMoves) {
            if (completedMoves == null) {
                return (false, Errors.ListIsNull);
            }
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
                return (true, Errors.None);
            }
            if (noTakeMoves == 50) {
                return (true, Errors.None);
            }
            return (false, Errors.None);
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
                        if (moves.Item2 != Errors.None) {
                            Debug.Log(moves.Item2);
                        }
                        if (moves.Item1.Count != 0) {
                            noCheckMate = true;
                            break;
                        }
                    }
                }
            }
            var kingPos = Check.FindKing(board.board, color);
            if (kingPos.Item2 != Errors.None) {
                Debug.Log(kingPos.Item2);
            }
            var checkInfo = Check.GetCheckInfo(board.board, color, kingPos.Item1);
            if (checkInfo.Item2 != Errors.None) {
                Debug.Log(checkInfo.Item2);
            }
            if (Check.IsCheck(checkInfo.Item1).Item1) {
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
            var checkDraw = CheckDraw(movesHistory, noTakeMoves);
            if (checkDraw.Item2 != Errors.None) {
                Debug.Log(checkDraw.Item2);
            }
            if (checkDraw.Item1) {
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