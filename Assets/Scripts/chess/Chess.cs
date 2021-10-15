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
    public enum ChessErrors {
        None,
        BoardIsNull,
        PieceIsNone,
        CantFindKing,
        CantGetCheckInfo,
        CantGetKingPossibleMoves,
        CantGetMoveInfos,
        CantGetPieceMovements,
        CantGetLinearLength,
        CantGetCircularPoint,
        CantGetPossibleMoves,
        CantGetNotOpeningMoves,
        CantGetСoveringMoves,
        CantCheckDraw,
        ListIsNull,
    }
    public enum GameStatus {
        None,
        Check,
        CheckMate,
        StaleMate,
        Draw
    }

    public static class Chess {
        public static (List<MoveInfo>, ChessErrors) GetPossibleMoves(
            Vector2Int pos,
            FullBoard board
        ) {
            var boardOpt = board.board;
            if (boardOpt == null) {
                return (null, ChessErrors.BoardIsNull);
            }
            if (boardOpt[pos.x, pos.y].IsNone()) {
                return (null, ChessErrors.PieceIsNone);
            }
            var piece = boardOpt[pos.x, pos.y].Peel();

            var (kingPos, findKingErr) = Check.FindKing(boardOpt, piece.color);
            if (findKingErr != CheckErrors.None) {
                return (null, ChessErrors.CantFindKing);
            }
            var (isCheck, isCheckErr) = Check.IsCheck(boardOpt, kingPos, piece.color);
            if (isCheckErr != CheckErrors.None) {
                return (null, ChessErrors.CantGetCheckInfo);
            }
            var movementList = MovementEngine.GetPieceMovements(boardOpt, piece.type, pos);
            
            if (piece.type == PieceType.King) {
                var (kingMoves, a) = move.Move.GetMoveInfos(movementList.Item1, pos, board);
                if (a != MoveErrors.None) {
                    return (null, ChessErrors.CantGetMoveInfos);
                }
                var pieceOpt = board.board[pos.x, pos.y];
                foreach (var move in new List<MoveInfo>(kingMoves)) {
                    var king = board.board[pos.x, pos.y];
                    board.board[pos.x, pos.y] = Option<Piece>.None();
                    var moveTo = move.doubleMove.first.to;
                    board.board[pos.x, pos.y] = king;
                    if (check.Check.IsCheck(boardOpt, moveTo, piece.color).Item1) {
                        kingMoves.Remove(move);
                        continue;
                    }
                }
                return (kingMoves, ChessErrors.None);
            }

            var (checkInfos, getCheckInfoErr) = Check.GetCheckInfo(boardOpt, piece.color, kingPos);
            if (getCheckInfoErr != CheckErrors.None) {
                return (null, ChessErrors.CantGetCheckInfo);
            }
            foreach (var checkInfo in checkInfos) {
                if (!checkInfo.coveringPos.HasValue) {
                    var (coveringMoves, coveringErr) = GetСoveringMoves(pos, board, checkInfo);
                    if (coveringErr != ChessErrors.None) {
                        return (null, ChessErrors.CantGetСoveringMoves);
                    }
                    return (coveringMoves, ChessErrors.None);
                }
                if (checkInfo.coveringPos == pos && !isCheck) {
                    var (notOpenning, err5) = GetNotOpeningMoves(pos, board, checkInfo);
                    if (err5 != ChessErrors.None) {
                        return (null, ChessErrors.CantGetNotOpeningMoves);
                    }
                    return (notOpenning, ChessErrors.None);
                }
            }
            var (moveInfos, err6) = Move.GetMoveInfos(movementList.Item1, pos, board);
            if (err6 != MoveErrors.None) {
                return (null, ChessErrors.CantGetCheckInfo);
            }
            return (moveInfos, ChessErrors.None);
        }

         public static (List<MoveInfo>, ChessErrors) GetСoveringMoves(
            Vector2Int pos,
            FullBoard board,
            CheckInfo checkInfo
        ) {
            var boardOpt = board.board;
            if (boardOpt == null) {
                return (null, ChessErrors.BoardIsNull);
            }
            if (boardOpt[pos.x, pos.y].IsNone()) {
                return (null, ChessErrors.PieceIsNone);
            }
            var piece = boardOpt[pos.x, pos.y].Peel();
            var (movements, err) = MovementEngine.GetPieceMovements(boardOpt, piece.type, pos);
            var startAttackPos = checkInfo.attackInfo.startPos;
            var endAttackPos = startAttackPos;
            var linearAttack = checkInfo.attackInfo.movement.linear;
            if (linearAttack.HasValue) {
                endAttackPos = startAttackPos + linearAttack.Value.dir * linearAttack.Value.length;
            }
            var attackSegment = math.Math.Segment.Mk(startAttackPos, endAttackPos);
            var moveInfos = new List<MoveInfo>();
            foreach (var movement in movements) {
                if (movement.movement.movement.circular.HasValue) {
                    var circular = movement.movement.movement.circular.Value;
                    var angle = 0f;
                    for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                        angle = StartAngle.Knight * i * Mathf.PI / 180;
                        var (cell, err3) = Board.GetCircularPoint(pos, circular, angle, boardOpt);
                        if (err3 != BoardErrors.None) {
                            return (null, ChessErrors.CantGetCircularPoint);
                        }
                        if (!cell.HasValue) {
                            continue;
                        }
                        if (math.Math.IsPointOnSegment(attackSegment, cell.Value)) {
                            var moveData = MoveData.Mk(pos, cell.Value);
                            var doubleMove = DoubleMove.MkSingleMove(moveData);
                            moveInfos.Add(MoveInfo.Mk(doubleMove));
                        }
                    }
                }
                if (movement.movement.movement.linear.HasValue) {
                    var linear = movement.movement.movement.linear;
                    var startDef = movement.movement.startPos;
                    var endDef = startDef + linear.Value.dir * linear.Value.length;
                    var defSegment = math.Math.Segment.Mk(startDef, endDef);

                    var n1 = math.Math.GetNormalVector(attackSegment);
                    var n2 = math.Math.GetNormalVector(defSegment);
                    Vector2Int? point = new Vector2Int();
                    if (!n1.HasValue || !n2.HasValue) {
                        point = startAttackPos;
                    } else {
                        var firstLine = math.Math.GetLineCoefficients(n1.Value, startAttackPos);
                        var secondLine = math.Math.GetLineCoefficients(n2.Value, pos);
                        point = math.Math.GetSegmentsIntersection(firstLine, secondLine);
                    }
                    if (!point.HasValue) {
                        continue;
                    }
                    if (!math.Math.IsPointOnSegment(defSegment, point.Value)
                        || !math.Math.IsPointOnSegment(attackSegment, point.Value)) {
                        continue;
                    }
                    var doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(pos, point.Value));
                    var cellOpt = board.board[point.Value.x, point.Value.y];
                    if (movement.movementType == MovementType.Attack && cellOpt.IsSome()
                    && piece.color != cellOpt.Peel().color) {
                        var pieceOnPoint = cellOpt.Peel();
                        var moveInfo = MoveInfo.Mk(doubleMove);
                        moveInfo.sentenced = point;
                        moveInfos.Add(moveInfo);
                    } else if (movement.movementType == MovementType.Move && cellOpt.IsNone()) {
                        moveInfos.Add(MoveInfo.Mk(doubleMove));
                    }
                }
            }
            return (moveInfos, ChessErrors.None);
        }

        public static (List<MoveInfo>, ChessErrors) GetNotOpeningMoves(
            Vector2Int target,
            FullBoard board,
            CheckInfo checkInfo
        ) {
            if (board.board == null) {
                return (null, ChessErrors.BoardIsNull);
            }
            if (board.board[target.x, target.y].IsNone()) {
                return (null, ChessErrors.PieceIsNone);
            }
            var piece = board.board[target.x, target.y].Peel();
            var pieceMovements = MovementEngine.GetPieceMovements(board.board, piece.type, target);
            if (pieceMovements.Item2 != MovementErrors.None) {
                return (null, ChessErrors.CantGetPieceMovements);
            }
            var linear = checkInfo.attackInfo.movement.linear.Value;
            var movementList = new List<PieceMovement>();
            foreach (var pieceMovement in pieceMovements.Item1) {
                if (pieceMovement.movement.movement.circular.HasValue) {
                    continue;
                }
                if (pieceMovement.movement.movement.linear.HasValue) {
                    if (pieceMovement.movement.movement.linear.Value.dir == linear.dir) {
                        movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                    }
                    if (pieceMovement.movement.movement.linear.Value.dir == -linear.dir) {
                        movementList.Add(new PieceMovement{movement = pieceMovement.movement});
                    }
                }
            }
            var (moveInfos, err) = move.Move.GetMoveInfos(movementList, target, board);
            if (err != MoveErrors.None) {
                return (null, ChessErrors.CantGetMoveInfos);
            }
            return (moveInfos, ChessErrors.None);
        }

        public static PieceColor ChangeMove(PieceColor whoseMove) {
            return (PieceColor)(((int)(whoseMove + 1) % (int)PieceColor.Count));
        }

        public static (bool, ChessErrors) CheckDraw(List<MoveInfo> moveHistory, int noTakeMoves) {
            if (moveHistory == null) {
                return (false, ChessErrors.ListIsNull);
            }
            int moveCounter = 0;
            if (moveHistory.Count > 9) {
                var lastMove = moveHistory[moveHistory.Count - 1].doubleMove.first;

                for (int i = moveHistory.Count - 1; i > moveHistory.Count - 10; i = i - 2) {
                    if (moveHistory[i].doubleMove.first.to == lastMove.to 
                        && moveHistory[i].doubleMove.first.from == lastMove.from) {
                        moveCounter++;
                    }
                }
            }
            if (moveCounter == 3) {
                return (true, ChessErrors.None);
            }
            if (noTakeMoves == 50) {
                return (true, ChessErrors.None);
            }
            return (false, ChessErrors.None);
        }

        public static (GameStatus, ChessErrors) GetGameStatus(
            FullBoard board,
            PieceColor color,
            List<MoveInfo> movesHistory,
            int noTakeMoves
        ) {
            bool noCheckMate = false;
            var gameStatus = GameStatus.None;
            var (kingPos, findKingErr) = Check.FindKing(board.board, color);
            if (findKingErr != CheckErrors.None) {
                return (gameStatus, ChessErrors.CantFindKing);
            }
            if (Check.IsCheck(board.board, kingPos, color).Item1) {
                gameStatus = GameStatus.Check;
            }

            for (int i = 0; i < board.board.GetLength(0); i++) {
                for (int j = 0; j < board.board.GetLength(1); j++) {
                    if (board.board[i, j].IsNone()) {
                        continue;
                    }
                    if (board.board[i, j].Peel().color == color) {
                        var piecePos = new Vector2Int(i, j);
                        var (moves, err3) = GetPossibleMoves(piecePos, board);
                        if (err3 != ChessErrors.None) {
                            return (gameStatus, ChessErrors.CantGetPieceMovements);
                        }
                        if (moves.Count != 0) {
                            noCheckMate = true;
                            break;
                        }
                    }
                }
            }

            if (!noCheckMate) {
                gameStatus = GameStatus.CheckMate;
            }
            var (checkDraw, err4) = CheckDraw(movesHistory, noTakeMoves);
            if (err4 != ChessErrors.None) {
                return (gameStatus, ChessErrors.CantCheckDraw);
            }
            if (checkDraw) {
                gameStatus = GameStatus.Draw;
            }
            return (gameStatus, ChessErrors.None);
        }

        public static void InsertPieceWithOneColor (Option<Piece>[,] board, PieceColor color) {
            board[(int)color * 7, 0] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, color, 0));
            board[(int)color * 7, 1] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, color, 0));
            board[(int)color * 7, 2] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, color, 0));
            board[(int)color * 7, 4] = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            board[(int)color * 7, 3] = Option<Piece>.Some(Piece.Mk(PieceType.Queen, color, 0));
            board[(int)color * 7, 5] = Option<Piece>.Some(Piece.Mk(PieceType.Bishop, color, 0));
            board[(int)color * 7, 6] = Option<Piece>.Some(Piece.Mk(PieceType.Knight, color, 0));
            board[(int)color * 7, 7] = Option<Piece>.Some(Piece.Mk(PieceType.Rook, color, 0));
            
            var pawnPosX = Mathf.Abs((int)color * 7 - 1);
            for (int i = 0; i < 8; i++) {
                board[pawnPosX, i] = Option<Piece>.Some(Piece.Mk(PieceType.Pawn, color, 0));
            }
        }
        public static Option<Piece>[,] CreateBoard() {
            Option<Piece>[,] board = new Option<Piece>[8, 8];
            InsertPieceWithOneColor(board, PieceColor.White);
            InsertPieceWithOneColor(board, PieceColor.Black);
            return board;
        }
    }
}