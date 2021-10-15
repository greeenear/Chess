using System;
using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using movement;

namespace check {
    public enum CheckErrors {
        None,
        ListIsNull,
        BoardIsNull,
        PieceIsNone,
        CantGetPieceMovements,
        CantGetCircularMoves,
        CantGetLinearMoves,
        CantGetLinearLength,
        CantGetAttackMovements
    }
    public struct CheckInfo {
        public FixedMovement attackInfo;
        public Vector2Int? coveringPos;

        public static CheckInfo Mk(FixedMovement attackInfo) {
            return new CheckInfo { attackInfo = attackInfo };
        }
    }

    public static class Check {
        public static (Vector2Int, CheckErrors) FindKing(Option<Piece>[,] board, PieceColor color) {
            Vector2Int kingPosition = new Vector2Int();
            if (board == null) {
                return (kingPosition, CheckErrors.BoardIsNull);
            }
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].IsNone()) {
                        continue;
                    }
                    var piece = board[i, j].Peel();
                    if (piece.type == PieceType.King && piece.color == color) {
                        kingPosition.x = i;
                        kingPosition.y = j;
                    }
                }
            }
            return (kingPosition, CheckErrors.None);
        }

        public static (List<FixedMovement>, CheckErrors) GetAttackMovements(
            PieceColor color,
            Option<Piece>[,] board,
            Vector2Int pos
        ) {
            if (board == null) {
                return (null, CheckErrors.BoardIsNull);
            }
            var (movement, err) = MovementEngine.GetPieceMovements(board, PieceType.Knight, pos);
            if (err != MovementErrors.None) {
                return (null, CheckErrors.CantGetPieceMovements);
            }
            movement.AddRange(MovementEngine.GetPieceMovements(board, PieceType.Queen, pos).Item1);

            var fixedMovements = new List<FixedMovement>();
            foreach (var type in movement) {
                if (type.movement.movement.circular.HasValue) {
                    var circle = type.movement.movement.circular.Value;
                    var (circularMovement, error) = GetCircilarAttackMovement(board, pos, circle);
                    if (error != CheckErrors.None) {
                        return (null, CheckErrors.CantGetCircularMoves);
                    }
                    if (circularMovement.HasValue) {
                        fixedMovements.Add(circularMovement.Value);
                    }
                } else if (type.movement.movement.linear.HasValue) {
                    var linear = type.movement.movement.linear.Value;
                    var (linearMovement, error) = GetLinearAttackMovement(board, pos, linear);
                    if (error != CheckErrors.None) {
                        return (null, CheckErrors.CantGetLinearMoves);
                    }
                    if (linearMovement.HasValue) {
                        fixedMovements.Add(linearMovement.Value);
                    }
                }
            }
            return (fixedMovements, CheckErrors.None);
        }

        public static (FixedMovement?, CheckErrors) GetCircilarAttackMovement(
            Option<Piece>[,] board,
            Vector2Int target,
            Circular circular
        ) {
            if (board == null) {
                return (null, CheckErrors.BoardIsNull);
            }
            if (board[target.x, target.y].IsNone()) {
                return (null, CheckErrors.PieceIsNone);
            }
            var angle = 0f;
            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = StartAngle.Knight * i * Mathf.PI / 180;
                var (possibleCell, err) = Board.GetCircularPoint(target, circular, angle, board);
                if (!possibleCell.HasValue) {
                    continue;
                }
                var radius = circular.radius;
                var cell = possibleCell.Value;
                if (board[cell.x, cell.y].IsSome()) {
                    var type = board[cell.x, cell.y].Peel().type;
                    var (movement, err2) = MovementEngine.GetPieceMovements(board, type, cell);
                    if (err2 != MovementErrors.None) {
                        return (null, CheckErrors.CantGetPieceMovements);
                    }
                    var pieceMovement = PieceMovement.Circular(radius, cell, MovementType.Attack);
                    if (movement.Contains(pieceMovement)) {
                        return (pieceMovement.movement, CheckErrors.None);
                    }
                }
            }

            return (null, CheckErrors.None);
        }

        public static (FixedMovement?, CheckErrors) GetLinearAttackMovement(
            Option<Piece>[,] board,
            Vector2Int target,
            Linear linear
        ) {
            if (board == null) {
                return (null, CheckErrors.BoardIsNull);
            }
            var (length, err) = Board.GetLinearLength(target, linear, board);
            if (err != BoardErrors.None) {
                return (null, CheckErrors.CantGetLinearLength);
            }
            var cell = target + linear.dir * length;
            if (board[cell.x, cell.y].IsSome()) {
                var type = board[cell.x, cell.y].Peel().type;
                var (movement, err2) = MovementEngine.GetPieceMovements(board, type, cell);
                if (err2 != MovementErrors.None) {
                    return (null, CheckErrors.CantGetPieceMovements);
                }

                foreach (var move in movement) {
                    if (!move.movement.movement.linear.HasValue) {
                        continue;
                    }
                    if (-move.movement.movement.linear.Value.dir == linear.dir) {
                        int lineLength = (int)(cell - target).magnitude;
                        var line = Linear.Mk(move.movement.movement.linear.Value.dir, lineLength);
                        var attackLen = move.movement.movement.linear.Value.length;
                        if (attackLen >= lineLength && move.movementType == MovementType.Attack) {
                            var fixedMovement = FixedMovement.Mk(Movement.Linear(line), cell);
                            return (fixedMovement, CheckErrors.None);
                        }
                        break;
                    }
                }
            }

            return (null, CheckErrors.None);
        }

        public static (List<CheckInfo>, CheckErrors) AnalyzeAttackMovements(
            PieceColor color,
            Option<Piece>[,] board,
            List<FixedMovement> attackInfo,
            Vector2Int target
        ) {
            if (board == null) {
                return (null, CheckErrors.BoardIsNull);
            }
            var checkInfo = new List<CheckInfo>();
            foreach (var info in attackInfo) {
                if (info.movement.circular.HasValue) {
                    checkInfo.Add(CheckInfo.Mk(info));
                    continue;
                }
                if (info.movement.linear.HasValue) {
                    var linearInfo = info.movement.linear.Value;
                    var linear = Linear.Mk(-linearInfo.dir, linearInfo.length);
                    var (length, err) = Board.GetLinearLength(target, linear, board);
                    if (err != BoardErrors.None) {
                        return (null, CheckErrors.CantGetLinearLength);
                    }
                    var cell = target + linear.dir * length;
                    if (board[cell.x, cell.y].IsNone()) {
                        continue;
                    }
                    if (board[cell.x, cell.y].Peel().color != color) {
                        checkInfo.Add(CheckInfo.Mk(info));
                        continue;
                    } else {
                        (length, err) = Board.GetLinearLength(cell, linear, board);
                        if (err != BoardErrors.None) {
                            return (null, CheckErrors.CantGetLinearLength);
                        }
                        var secondCell = cell + linear.dir * length;
                        if (board[secondCell.x, secondCell.y].Peel().color != color) {
                            checkInfo.Add(new CheckInfo { attackInfo = info, coveringPos = cell });
                        }
                    }
                }
            }

            return (checkInfo, CheckErrors.None);
        }

        public static (List<CheckInfo>, CheckErrors) GetCheckInfo(
            Option<Piece>[,] board,
            PieceColor color,
            Vector2Int cellPos
        ) {
            if (board == null) {
                return (null, CheckErrors.BoardIsNull);
            }
            var (singleColorBoard, err1) = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard[cellPos.x, cellPos.y] = king;

            var (attackInfo, err) = Check.GetAttackMovements(color, singleColorBoard, cellPos);
            if (err != CheckErrors.None) {
                return (null, CheckErrors.CantGetAttackMovements);
            }
            var (checkInfo, err2) = Check.AnalyzeAttackMovements(color, board, attackInfo, cellPos);
            if (err2 != CheckErrors.None) {
                return (null, CheckErrors.CantGetAttackMovements);
            }

            return (checkInfo, CheckErrors.None);
        }

        public static (bool, CheckErrors) IsCheck(Option<Piece>[,] board, Vector2Int pos, PieceColor color) {
            var (infos, err) = GetCheckInfo(board, color, pos);
            if (err != CheckErrors.None) {
                return (false, CheckErrors.CantCheckKing);
            }
            foreach (var info in infos) {
                if (!info.coveringPos.HasValue) {
                    return (true, CheckErrors.None);
                }
            }

            return (false, CheckErrors.None);
        }

        public static (Option<Piece>[,], CheckErrors) GetBoardWithOneColor(
            PieceColor color,
            Option<Piece>[,] startBoard
        ) {
            if (startBoard == null) {
                return (null, CheckErrors.BoardIsNull);
            }
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

            return (board, CheckErrors.None);
        }
    }
}