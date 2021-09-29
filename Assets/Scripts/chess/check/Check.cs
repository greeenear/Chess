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
            Vector2Int target,
            PieceTrace? trace
        ) {
            var movementType = new List<Movement>(Storage.movement[PieceType.Queen]);
            movementType.AddRange(Storage.movement[PieceType.Knight]);
            movementType.AddRange(Storage.movement[PieceType.King]);

            var movements = new List<FixedMovement>();
            foreach (var type in movementType) {
                if (type.circular.HasValue) {
                    movements.AddRange(GetCircularMoves(board, target, type, trace));
                } else if (type.linear.HasValue) {
                    movements.AddRange(GetLinearMoves(board, target, type, trace));
                }
            }

            return movements;
        }

        public static List<FixedMovement> GetCircularMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Movement circularMovement,
            PieceTrace? trace
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var circular = FixedMovement.Mk(circularMovement, target);
            var moves = Rules.GetMoves(board, circular, trace);
            var radius = circular.movement.circular.Value.radius;

            foreach (var move in moves) {
                var cell = board[move.x, move.y];
                var circle = Movement.Circular(Circular.Mk(radius));
                var attackingPiecePos = new Vector2Int(move.x, move.y);
                if (cell.IsNone()) {
                    continue;
                }

                var attackMovements = storage.Storage.movement[cell.Peel().type];
                foreach (var movement in attackMovements) {
                    if (movement.circular.HasValue && movement.circular.Value.radius == radius) {
                        movements.Add(FixedMovement.Mk(circle, attackingPiecePos));
                    }
                }
            }

            return movements;
        }

        public static List<FixedMovement> GetLinearMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Movement linearMovement,
            PieceTrace? trace
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var fixedLine = FixedMovement.Mk(linearMovement, target);
            var length = Board.GetLinearLength(
                target,
                linearMovement.linear.Value,
                board, Board.GetMaxLength(
                    board,
                    linearMovement.linear.Value.length
                )
            );

            var lastPos = target + linearMovement.linear.Value.dir * length;
            if (board[lastPos.x, lastPos.y].IsNone()) {
                return movements;
            }

            var attackMovement = new Movement();
            var attackingPiecePos = new Vector2Int(lastPos.x, lastPos.y);
            var pieceMovements = Move.GetMovements(board, attackingPiecePos);

            bool isMovementContained = false;
            foreach (var movement in pieceMovements) {
                if (!movement.linear.HasValue) {
                    break;
                }
                if (-movement.linear.Value.dir == linearMovement.linear.Value.dir) {
                    attackMovement = movement;
                    isMovementContained = true;
                    break;
                }
            }
            if (!isMovementContained) {
                return movements;
            }
            var attackDir = Movement.Linear(linearMovement.linear.Value, MovementType.Attack);

            var lineLength = Math.Abs(attackingPiecePos.x - target.x);
            var attackLength = Board.GetMaxLength(board, attackMovement.linear.Value.length);
            var attackMovementType = attackMovement.movementType;
            if (attackLength >= lineLength && attackMovementType == MovementType.Attack) {
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
                    if (nextCell.IsNone()) {
                        continue;
                    }
                    var pieceColor = nextCell.Peel().color;
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
            Option<Piece>[,] board,
            PieceColor color,
            Vector2Int cellPos,
            PieceTrace? trace
        ) {
            var movement = storage.Storage.movement;

            var singleColorBoard = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard[cellPos.x, cellPos.y] = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard, cellPos, trace);
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