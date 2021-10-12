using System;
using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using movement;

namespace check {
    public struct CheckInfo {
        public FixedMovement attackInfo;
        public Vector2Int? coveringPos;

        public static CheckInfo Mk(FixedMovement attackInfo) {
            return new CheckInfo { attackInfo = attackInfo };
        }
    }

    public static class Check {
        public static (Vector2Int, Errors) FindKing(Option<Piece>[,] board, PieceColor color) {
            Vector2Int kingPosition = new Vector2Int();
            if (board == null) {
                return (kingPosition, Errors.BoardIsNull);
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
            return (kingPosition, Errors.None);
        }

        public static (List<FixedMovement>, Errors) GetAttackMovements(
            PieceColor color,
            Option<Piece>[,] board,
            Vector2Int target
        ) {
            if (board == null) {
                return (null, Errors.BoardIsNull);
            }
            var movement = MovementEngine.GetPieceMovements(board, PieceType.Knight, target);
            if (movement.Item2 != Errors.None) {
                Debug.Log(movement.Item2);
                return (null, movement.Item2);
            }
            var move = movement.Item1;
            move.AddRange(MovementEngine.GetPieceMovements(board, PieceType.Queen, target).Item1);

            var fixedMovements = new List<FixedMovement>();
            foreach (var type in move) {
                if (type.movement.movement.circular.HasValue) {
                    var circular = type.movement.movement.circular.Value;
                    var circularMoves = GetCircularMoves(board, target, circular);
                    if (circularMoves.Item2 != Errors.None) {
                        Debug.Log(circularMoves.Item2);
                        return (null, circularMoves.Item2);
                    }
                    fixedMovements.AddRange(circularMoves.Item1);
                } else if (type.movement.movement.linear.HasValue) {
                    var linear = type.movement.movement.linear.Value;
                    var linearMoves = GetLinearMoves(board, target, linear);
                    if (linearMoves.Item2 != Errors.None) {
                        Debug.Log(linearMoves.Item2);
                        return (null, linearMoves.Item2);
                    }
                    fixedMovements.AddRange(linearMoves.Item1);
                } else {
                    return (fixedMovements, Errors.ImpossibleMovement);
                }
            }
            return (fixedMovements, Errors.None);
        }

        public static (List<FixedMovement>, Errors) GetCircularMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Circular circular
        ) {
            if (board == null) {
                return (null, Errors.BoardIsNull);
            }
            if (board[target.x, target.y].IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var angle = 0f;
            List<FixedMovement> movements = new List<FixedMovement>();
            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = StartAngle.Knight * i * Mathf.PI / 180;
                var possibleCell = Board.GetCircularPoint(target, circular, angle, board);
                if (!possibleCell.Item1.HasValue) {
                    continue;
                }
                var radius = circular.radius;
                var cell = possibleCell.Item1.Value;
                if (board[cell.x, cell.y].IsSome()) {
                    var type = board[cell.x, cell.y].Peel().type;
                    var movement = MovementEngine.GetPieceMovements(board, type, cell);
                    var pieceMovement = PieceMovement.Circular(radius, cell, MovementType.Attack);
                    if (movement.Item1.Contains(pieceMovement)) {
                        movements.Add(FixedMovement.Mk(pieceMovement.movement.movement, cell));
                    }
                }
            }
            return (movements, Errors.None);
        }

        public static (List<FixedMovement>, Errors) GetLinearMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Linear linear
        ) {
            if (board == null) {
                return (null, Errors.BoardIsNull);
            }
            var length = Board.GetLinearLength(target, linear, board);
            if (length.Item2 != Errors.None) {
                Debug.Log(Errors.BoardIsNull);
                return (null, length.Item2);
            }
            List<FixedMovement> movements = new List<FixedMovement>();
            var cell = target + linear.dir * length.Item1;
            if (board[cell.x, cell.y].IsSome()) {
                var type = board[cell.x, cell.y].Peel().type;
                var movement = MovementEngine.GetPieceMovements(board, type, cell);
                foreach (var move in movement.Item1) {
                    if (!move.movement.movement.linear.HasValue) {
                        continue;
                    }
                    if (-move.movement.movement.linear.Value.dir == linear.dir) {
                        var lineLength = Math.Abs(cell.x - target.x);
                        var attackLen = move.movement.movement.linear.Value.length;
                        if (attackLen >= lineLength && move.movementType == MovementType.Attack) {
                            movements.Add(FixedMovement.Mk(Movement.Linear(linear), cell));
                        }
                        break;
                    }
                }
            }
            return (movements, Errors.None);
        }

        public static (List<CheckInfo>, Errors) AnalyzeAttackMovements(
            PieceColor color,
            Option<Piece>[,] board,
            List<FixedMovement> attackInfo,
            Vector2Int target
        ) {
            if (board == null) {
                return (null, Errors.BoardIsNull);
            }
            var checkInfo = new List<CheckInfo>();
            foreach (var info in attackInfo) {
                if (info.movement.circular.HasValue) {
                    checkInfo.Add(CheckInfo.Mk(info));
                    continue;
                }
                if (info.movement.linear.HasValue) {
                    var length = Board.GetLinearLength(target, info.movement.linear.Value, board);
                    if (length.Item2 != Errors.None) {
                        Debug.Log(length.Item2);
                        return (null, length.Item2);
                    }
                    var cell = target + info.movement.linear.Value.dir * length.Item1;
                    if (board[cell.x, cell.y].IsNone()) {
                        continue;
                    }
                    if (board[cell.x, cell.y].Peel().color != color) {
                        checkInfo.Add(CheckInfo.Mk(info));
                        continue;
                    } else {
                        length = Board.GetLinearLength(cell, info.movement.linear.Value, board);
                        var secondCell = cell + info.movement.linear.Value.dir * length.Item1;
                        if (board[secondCell.x, secondCell.y].Peel().color != color) {
                            checkInfo.Add(new CheckInfo { attackInfo = info, coveringPos = cell });
                        }
                    }
                    
                }
            }
            return (checkInfo, Errors.None);
        }

        public static (List<CheckInfo>, Errors) GetCheckInfo(
            Option<Piece>[,] board,
            PieceColor color,
            Vector2Int cellPos
        ) {
            if (board == null) {
                return (null, Errors.BoardIsNull);
            }
            var singleColorBoard = GetBoardWithOneColor(color, board);
            if (singleColorBoard.Item2 != Errors.None) {
                Debug.Log(singleColorBoard.Item2);
            }
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard.Item1[cellPos.x, cellPos.y] = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard.Item1, cellPos);
            if (attackInfo.Item2 != Errors.None) {
                Debug.Log(attackInfo.Item2);
            }
            var checkInfo = Check.AnalyzeAttackMovements(color, board, attackInfo.Item1, cellPos);
            if (checkInfo.Item2 != Errors.None) {
                Debug.Log(checkInfo.Item2);
            }

            return (checkInfo.Item1, Errors.None);
        }

        public static (bool, Errors) IsCheck(List<CheckInfo> checkInfos) {
            if (checkInfos == null) {
                return (false, Errors.ListIsNull);
            }
            foreach (var info in checkInfos) {
                if (!info.coveringPos.HasValue) {
                    return (true, Errors.None);
                }
            }
            return (false, Errors.None);
        }

        public static (Option<Piece>[,], Errors) GetBoardWithOneColor(
            PieceColor color,
            Option<Piece>[,] startBoard
        ) {
            if (startBoard == null) {
                return (null, Errors.BoardIsNull);
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

            return (board, Errors.None);
        }
    }
}