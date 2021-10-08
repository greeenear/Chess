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
        public static Vector2Int FindKing(Option<Piece>[,] board, PieceColor color) {
            Vector2Int kingPosition = new Vector2Int();
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
            return kingPosition;
        }

        public static List<FixedMovement> GetAttackMovements(
            PieceColor color,
            Option<Piece>[,] board,
            Vector2Int target
        ) {
            var movementType = MovementEngine.GetPieceMovements(board, PieceType.Knight, target);
            movementType.AddRange(MovementEngine.GetPieceMovements(board, PieceType.Queen, target));

            var movements = new List<FixedMovement>();
            foreach (var type in movementType) {
                if (type.movement.movement.circular.HasValue) {
                    var circular = type.movement.movement.circular.Value;
                    movements.AddRange(GetCircularMoves(board, target, circular));
                } else if (type.movement.movement.linear.HasValue) {
                    var linear = type.movement.movement.linear.Value;
                    movements.AddRange(GetLinearMoves(board, target, linear));
                }
            }
            return movements;
        }

        public static List<FixedMovement> GetCircularMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Circular circular
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var angle = 0f;

            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = StartAngle.Knight * i * Mathf.PI / 180;
                var cell = Board.GetCircularPoint(target, circular, angle, board);
                if (!cell.HasValue) {
                    continue;
                }
                var cellOpt = board[cell.Value.x, cell.Value.y];
                if (cellOpt.IsNone()) {
                    continue;
                }
                var piece = cellOpt.Peel();
                var targetPiece = board[target.x, target.y].Peel();
                if (piece.color != targetPiece.color) {
                    var type = piece.type;
                    var attackMovements = MovementEngine.GetPieceMovements(board, type, target);
                    foreach (var movement in attackMovements) {
                        var radius = circular.radius;
                        var circle = Movement.Circular(Circular.Mk(radius));
                        if (movement.movement.movement.circular.HasValue
                            && movement.movement.movement.circular.Value.radius == radius) {
                            movements.Add(FixedMovement.Mk(circle, cell.Value));
                        }
                    }
                }
            }
            return movements;
        }

        public static List<FixedMovement> GetLinearMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Linear linear
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var length = Board.GetLinearLength(target, linear, board);

            var lastPos = target + linear.dir * length;
            if (board[lastPos.x, lastPos.y].IsNone()) {
                return movements;
            }

            var attackingPiecePos = new Vector2Int(lastPos.x, lastPos.y);
            var type = board[lastPos.x, lastPos.y].Peel().type;
            var attackMovements = MovementEngine.GetPieceMovements(board, type, attackingPiecePos);

            bool isMovementContained = false;

            var attackMovement = new PieceMovement();
            foreach (var movement in attackMovements) {
                if (!movement.movement.movement.linear.HasValue) {
                    return movements;
                }
                if (-movement.movement.movement.linear.Value.dir == linear.dir) {
                    var fixedMovement = FixedMovement.Mk(movement.movement.movement, target);
                    attackMovement = PieceMovement.Mk(fixedMovement, MovementType.Attack);
                    isMovementContained = true;
                    break;
                }
            }
            if (!isMovementContained) {
                return movements;
            }

            var lineLength = Math.Abs(attackingPiecePos.x - target.x);
            var attackLength = attackMovement.movement.movement.linear.Value.length;
            var attackDir = Movement.Linear(linear);
            if (attackLength >= lineLength && attackMovement.movementType == MovementType.Attack) {
                movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
            }

            return movements;
        }

        public static List<CheckInfo> AnalyzeAttackMovements(
            PieceColor color,
            Option<Piece>[,] startBoard,
            List<FixedMovement> attackInfo,
            Vector2Int target
        ) {
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
            return checkInfo;
        }

        public static List<CheckInfo> GetCheckInfo(
            Option<Piece>[,] board,
            PieceColor color,
            Vector2Int cellPos
        ) {
            var singleColorBoard = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard[cellPos.x, cellPos.y] = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard, cellPos);
            var checkInfo = Check.AnalyzeAttackMovements(color, board, attackInfo, cellPos);

            return checkInfo; 
        }

        public static bool IsCheck(List<CheckInfo> checkInfos) {
            foreach (var info in checkInfos) {
                if (!info.coveringPos.HasValue) {
                    return true;
                }
            }
            return false;
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
    }
}