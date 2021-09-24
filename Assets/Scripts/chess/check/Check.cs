using System;
using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using storage;

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
            Vector2Int target
        ) {
            var movements = new List<FixedMovement>();
            var movementType = new List<Movement>(Storage.movement[PieceType.Queen]);
            movementType.AddRange(Storage.movement[PieceType.Knight]);
        
            foreach (var type in movementType) {
                if (type.circular.HasValue) {
                    movements.AddRange(GetCircularMoves(board, target, type));
                } else if (type.linear.HasValue) {
                    movements.AddRange(GetLinearMoves(board, target, type));
                }
            }

            return movements;
        }

        public static List<FixedMovement> GetCircularMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Movement circularMovement

        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var circular = FixedMovement.Mk(circularMovement, target);
            var moves = Rules.GetCirclularMoves(board, circular);

            foreach (var move in moves) {
                var cell = board[move.x, move.y];
                var circle = Movement.Circular(Circular.Mk(2));
                var attackingPiecePos = new Vector2Int(move.x, move.y);
                if (cell.IsSome() && cell.Peel().type == PieceType.Knight) {
                    movements.Add(FixedMovement.Mk(circle, attackingPiecePos));
                }
            }

            return movements;
        }

        public static List<FixedMovement> GetLinearMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Movement linearMovement
        ) {
            List<FixedMovement> movements = new List<FixedMovement>();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var fixedLine = FixedMovement.Mk(linearMovement, target);
            var lineMoves = Rules.GetLinearMoves(board, fixedLine);

            foreach (var move in lineMoves) {
                if (board[move.x, move.y].IsNone()) {
                    continue;
                }

                var piece = board[move.x , move.y].Peel();
                var attackMovement = new Movement();
                bool isMovementContained = false;
                foreach (var movement in storage.Storage.movement[piece.type]) {
                    if (!movement.linear.HasValue) {
                        break;
                    }
                    if (movement.linear.Value.dir == linearMovement.linear.Value.dir) {
                        attackMovement = movement;
                        isMovementContained = true;
                    }
                }
                if (!isMovementContained) {
                    return movements;
                }
                var attackingPiecePos = new Vector2Int(move.x, move.y);
                var attackDir = Movement.Linear(linearMovement.linear.Value, MovementType.Attack);

                if (piece.type == PieceType.Pawn) {
                    var lineLength = Math.Abs(attackingPiecePos.x - target.x);
                    if (lineLength == 1 && attackMovement.movementType == MovementType.Attack) {
                        if (piece.color == PieceColor.White && attackDir.linear.Value.dir.x > 0) {
                            movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
                        }
                        if (piece.color == PieceColor.Black && attackDir.linear.Value.dir.x < 0) {
                            movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
                        }
                    }
                    return movements;
                }
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
            Vector2Int cellPos
        ) {
            var movement = storage.Storage.movement;

            var singleColorBoard = GetBoardWithOneColor(color, board);
            var king = Option<Piece>.Some(Piece.Mk(PieceType.King, color, 0));
            singleColorBoard[cellPos.x, cellPos.y] = king;

            var attackInfo = Check.GetAttackMovements(color, singleColorBoard, cellPos);
            var checkInfo = Check.AnalyzeAttackMovements(color, board, attackInfo, cellPos);

            return checkInfo; 
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