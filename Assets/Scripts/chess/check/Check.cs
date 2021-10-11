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
            if (board[target.x, target.y].IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var movement = MovementEngine.GetPieceMovements(board, PieceType.Knight, target);
            if (movement.Item2 != Errors.None) {
                Debug.Log(movement.Item2);
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
                    }
                    fixedMovements.AddRange(circularMoves.Item1);
                } else if (type.movement.movement.linear.HasValue) {
                    var linear = type.movement.movement.linear.Value;
                    var linearMoves = GetLinearMoves(board, target, linear);
                    if (linearMoves.Item2 != Errors.None) {
                        Debug.Log(linearMoves.Item2);
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
                var cell = Board.GetCircularPoint(target, circular, angle, board);
                if (cell.Item2 != Errors.None) {
                    Debug.Log(cell.Item2);
                }
                if (!cell.Item1.HasValue) {
                    continue;
                }
                var cellOpt = board[cell.Item1.Value.x, cell.Item1.Value.y];
                if (cellOpt.IsNone()) {
                    continue;
                }
                var piece = cellOpt.Peel();
                var targetPiece = board[target.x, target.y].Peel();
                if (piece.color != targetPiece.color) {
                    var type = piece.type;
                    var attackMovements = MovementEngine.GetPieceMovements(board, type, target);
                    if (attackMovements.Item2 != Errors.None) {
                        Debug.Log(attackMovements.Item2);
                    }
                    foreach (var movement in attackMovements.Item1) {
                        var radius = circular.radius;
                        var circle = Movement.Circular(Circular.Mk(radius));
                        if (movement.movement.movement.circular.HasValue
                            && movement.movement.movement.circular.Value.radius == radius) {
                            movements.Add(FixedMovement.Mk(circle, cell.Item1.Value));
                        }
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
            List<FixedMovement> movements = new List<FixedMovement>();
            var length = Board.GetLinearLength(target, linear, board);
            if (length.Item2 != Errors.None) {
                Debug.Log(Errors.BoardIsNull);
            }

            var lastPos = target + linear.dir * length.Item1;
            if (board[lastPos.x, lastPos.y].IsNone()) {
                return (movements, Errors.None);
            }

            var attackingPiecePos = new Vector2Int(lastPos.x, lastPos.y);
            var type = board[lastPos.x, lastPos.y].Peel().type;
            var attackMovements = MovementEngine.GetPieceMovements(board, type, attackingPiecePos);
            if (attackMovements.Item2 != Errors.None) {
                Debug.Log(attackMovements.Item2);
            }
            bool isMovementContained = false;

            var attackMovement = new PieceMovement();
            foreach (var movement in attackMovements.Item1) {
                if (!movement.movement.movement.linear.HasValue) {
                    return (movements, Errors.None);
                }
                if (-movement.movement.movement.linear.Value.dir == linear.dir) {
                    var fixedMovement = FixedMovement.Mk(movement.movement.movement, target);
                    attackMovement = PieceMovement.Mk(fixedMovement, MovementType.Attack);
                    isMovementContained = true;
                    break;
                }
            }
            if (!isMovementContained) {
                return (movements, Errors.None);
            }

            var lineLength = Math.Abs(attackingPiecePos.x - target.x);
            var attackLength = attackMovement.movement.movement.linear.Value.length;
            var attackDir = Movement.Linear(linear);
            if (attackLength >= lineLength && attackMovement.movementType == MovementType.Attack) {
                movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
            }

            return (movements, Errors.None);
        }

        public static (List<CheckInfo>, Errors) AnalyzeAttackMovements(
            PieceColor color,
            Option<Piece>[,] startBoard,
            List<FixedMovement> attackInfo,
            Vector2Int target
        ) {
            if (startBoard == null) {
                return (null, Errors.BoardIsNull);
            }
            if (startBoard[target.x, target.y].IsNone()) {
                return (null, Errors.PieceIsNone);
            }
            var checkInfo = new List<CheckInfo>();
            var boardSize = new Vector2Int(startBoard.GetLength(0), startBoard.GetLength(1));

            foreach (var info in attackInfo) {
                Vector2Int coveringPos = new Vector2Int();
                if (info.movement.circular.HasValue) {
                    checkInfo.Add(CheckInfo.Mk(info));
                    continue;
                }

                var coveringPiecesCounter = 0;
                for (int i = 1; i < boardSize.x; i++) {
                    var next = target + info.movement.linear.Value.dir * i;
                    var isOnBoard = Board.OnBoard(new Vector2Int(next.x, next.y), boardSize);
                    if (!isOnBoard) {
                        break;
                    }
                    var nextCell = startBoard[next.x, next.y];
                    if (nextCell.IsNone()) {
                        continue;
                    }
                    var pieceColor = nextCell.Peel().color;
                    if (pieceColor == color) {
                        coveringPiecesCounter++;
                        coveringPos = new Vector2Int(next.x, next.y);
                    }
                    if (pieceColor != color && coveringPiecesCounter == 0) {
                        checkInfo.Add(CheckInfo.Mk(info));
                        break;
                    }
                }
                if (coveringPiecesCounter == 1) {
                    checkInfo.Add(new CheckInfo { attackInfo = info, coveringPos = coveringPos });
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