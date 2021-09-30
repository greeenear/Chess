using System;
using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using storage;
using move;

namespace check {
    public struct CheckInfo {
        public FixedMovement attackInfo;
        public Vector2Int? coveringPiece;

        public static CheckInfo Mk(FixedMovement attackInfo) {
            return new CheckInfo { attackInfo = attackInfo };
        }
        public static CheckInfo Mk(FixedMovement attackInfo, Vector2Int coveringPiece) {
            return new CheckInfo { attackInfo = attackInfo, coveringPiece = coveringPiece };
        }
    }

    public static class Check {
        public static Vector2Int FindKing(CellInfo[,] board, PieceColor color) {
            Vector2Int kingPosition = new Vector2Int();

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].piece.IsNone()) {
                        continue;
                    }
                    var piece = board[i, j].piece.Peel();
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
            CellInfo[,] board,
            Vector2Int target
        ) {
            var movementType = new List<Movement>(Storage.movement[PieceType.Queen]);
            movementType.AddRange(Storage.movement[PieceType.Knight]);
            movementType.AddRange(Storage.movement[PieceType.King]);

            var movements = new List<FixedMovement>();
            foreach (var type in movementType) {
                if (type.circular.HasValue) {
                    var circular = type.circular.Value;
                    movements.AddRange(GetCircularMoves(board, target, circular));
                } else if (type.linear.HasValue) {
                    var linear = type.linear.Value;
                    movements.AddRange(GetLinearMoves(board, target, linear));
                }
            }

            return movements;
        }

        public static List<FixedMovement> GetCircularMoves(
            CellInfo[,] board,
            Vector2Int target,
            Circular circular
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var radius = circular.radius;
            var angle = 0f;
            var boardOpt = Rules.GetOptBoard(board);

            for (int i = 1; angle < Mathf.PI * 2; i += 2) {
                angle = StartAngle.Knight * i * Mathf.PI / 180;
                var cell = Board.GetCircularMove<Piece>(target, circular, angle, boardOpt);
                if (!cell.HasValue) {
                    continue;
                }
                var cellOpt = board[cell.Value.x, cell.Value.y];
                if (cellOpt.piece.IsNone()) {
                    continue;
                }
                var piece = cellOpt.piece.Peel();
                if (piece.color != board[target.x, target.y].piece.Peel().color) {
                    var attackMovements = storage.Storage.movement[piece.type];
                    foreach (var movement in attackMovements) {
                        var circle = Movement.Circular(Circular.Mk(radius));

                        if (movement.circular.HasValue 
                            && movement.circular.Value.radius == radius) {
                            movements.Add(FixedMovement.Mk(circle, cell.Value));
                        }
                    }
                }
            }

            return movements;
        }

        public static List<FixedMovement> GetLinearMoves(
            CellInfo[,] board,
            Vector2Int target,
            Linear linear
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var boardOpt = Rules.GetOptBoard(board);
            var length = Board.GetLinearLength(target, linear, boardOpt, linear.length);

            var lastPos = target + linear.dir * length;
            if (board[lastPos.x, lastPos.y].piece.IsNone()) {
                return movements;
            }

            var attackMovement = new Movement();
            var attackingPiecePos = new Vector2Int(lastPos.x, lastPos.y);
            var pieceMovements = Move.GetPieceMovements(board, attackingPiecePos);

            bool isMovementContained = false;
            foreach (var pieceMovement in pieceMovements) {
                if (!pieceMovement.movement.linear.HasValue) {
                    break;
                }
                if (-pieceMovement.movement.linear.Value.dir == linear.dir) {
                    attackMovement = pieceMovement.movement;
                    isMovementContained = true;
                    break;
                }
            }
            if (!isMovementContained) {
                return movements;
            }
            var attackDir = Movement.Linear(linear, MovementType.Attack);

            var lineLength = Math.Abs(attackingPiecePos.x - target.x);
            var attackLength = Board.GetMaxLength(boardOpt, attackMovement.linear.Value.length);
            var attackMovementType = attackMovement.movementType;
            if (attackLength >= lineLength && attackMovementType == MovementType.Attack) {
                movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
            }

            return movements;
        }

        public static List<CheckInfo> AnalyzeAttackMovements(
            PieceColor color,
            CellInfo[,] startBoard,
            List<FixedMovement> attackInfo,
            Vector2Int target
        ) {
            var checkInfo = new List<CheckInfo>();
            var boardSize = new Vector2Int(startBoard.GetLength(0), startBoard.GetLength(1));

            foreach (var info in attackInfo) {
                var coveringPiecesCounter = 0;
                Vector2Int coveringPos = new Vector2Int();
                if (info.movement.circular.HasValue) {
                    checkInfo.Add(CheckInfo.Mk(info));
                    continue;
                }

                for (int i = 1; i < boardSize.x; i++) {
                    var next = target + info.movement.linear.Value.dir * i;
                    var nextPos = new Vector2Int(next.x, next.y);
                    var isOnBoard = Board.OnBoard(nextPos, boardSize);
                    if (!isOnBoard) {
                        break;
                    }
                    var nextCell = startBoard[next.x, next.y];
                    if (nextCell.piece.IsNone()) {
                        continue;
                    }
                    var pieceColor = nextCell.piece.Peel().color;
                    if (pieceColor == color) {
                        coveringPiecesCounter++;
                        coveringPos = nextPos;
                    }
                    if (pieceColor != color && coveringPiecesCounter == 0) {
                        checkInfo.Add(CheckInfo.Mk(info));
                        break;
                    }
                }
                if (coveringPiecesCounter == 1) {
                    checkInfo.Add(CheckInfo.Mk(info, coveringPos));
                }
            }

            return checkInfo;
        }

        public static List<CheckInfo> GetCheckInfo(
            CellInfo[,] board,
            PieceColor color,
            Vector2Int cellPos
        ) {
            var movement = storage.Storage.movement;

            var singleColorBoard = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard[cellPos.x, cellPos.y].piece = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard, cellPos);
            var checkInfo = Check.AnalyzeAttackMovements(color, board, attackInfo, cellPos);

            return checkInfo; 
        }

        public static bool IsCheck(List<CheckInfo> checkInfos) {
            foreach (var info in checkInfos) {
                if (info.coveringPiece == null) {
                    return true;
                }
            }

            return false;
        }

        public static CellInfo[,] GetBoardWithOneColor(
            PieceColor color,
            CellInfo[,] startBoard
        ) {
            CellInfo[,] board = (CellInfo[,])startBoard.Clone();

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i,j].piece.IsNone()) {
                        continue;
                    }
                    var piece = board[i,j].piece.Peel();
                    if (piece.color == color) {
                        board[i, j].piece = Option<Piece>.None();
                    }
                }
            }
            return board;
        }
    }
}