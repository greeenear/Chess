using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;

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
                    if ( board[i, j].IsSome()) {
                        var piece = board[i, j].Peel();
                        if (piece.type == PieceType.King && piece.color == color) {
                            kingPosition.x = i;
                            kingPosition.y = j;
                        }
                    }
                }
            }

            return kingPosition;
        }

        public static List<FixedMovement> GetAttackMovements(
            PieceColor color,
            Option<Piece>[,] board,
            Dictionary<PieceType,List<Movement>> movementStorage,
            Vector2Int target
        ) {
            var movements = new List<FixedMovement>();
            var movementType = new List<Movement>(movementStorage[PieceType.Queen]);
            movementType.AddRange(movementStorage[PieceType.Knight]);
        
            foreach (var type in movementType) {
                if (type.circular.HasValue) {
                    InsertCircularMovements(board, target, type, movements);
                } else if (type.linear.HasValue) {
                    InsertLinearMoves(board, target, type, movements);
                }
            }

            return movements;
        }

        public static void InsertCircularMovements(
            Option<Piece>[,] board,
            Vector2Int target,
            Movement circularMovement,
            List<FixedMovement> movements
        ) {
            var moves = Rules.GetCirclularMoves(board, target, circularMovement.circular.Value);

            foreach (var move in moves) {
                var cell = board[move.x, move.y];
                var circle = Movement.Circular(Circular.Mk(2));
                var attackingPiecePos = new Vector2Int(move.x, move.y);
                if (cell.IsSome() && cell.Peel().type == PieceType.Knight) {
                    movements.Add(FixedMovement.Mk(circle, attackingPiecePos));
                }
            }
        }

        public static void InsertLinearMoves(
            Option<Piece>[,] board,
            Vector2Int target,
            Movement linearMovement,
            List<FixedMovement> movements
        ) {
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            int lineLength = 0;
            var lineMoves = Rules.GetLinearMoves(
                board,
                target,
                linearMovement.linear.Value,
                boardSize.x
            );
            
            foreach (var move in lineMoves) {
                lineLength++;

                if (board[move.x, move.y].IsNone()) {
                    continue;
                }
                var piece = board[move.x , move.y].Peel();
                if (!storage.Storage.movement[piece.type].Contains(linearMovement)) {
                    break;
                }
                var attackDir = Movement.Linear(linearMovement.linear.Value);
                var attackingPiecePos = new Vector2Int(move.x, move.y);

                if (piece.type == PieceType.Pawn) {
                    if (lineLength == 1 && linearMovement.movementType == MovementType.Attack) {
                        movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
                        break;
                    } else {
                        break;
                    }
                }
                movements.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
            }
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
                var piecesCounter = 0;
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
                    if (nextCell.Peel().color == color) {
                        if (piecesCounter == 0) {
                            checkInfo.Add(CheckInfo.Mk(info, nextPos));
                            piecesCounter++;
                        } else {
                            checkInfo.RemoveAt(checkInfo.Count - 1);
                            break;
                        }
                    }
                    if (nextCell.Peel().color != color && piecesCounter == 0) {
                        checkInfo.Add(CheckInfo.Mk(info));
                        break;
                    }
                }
            }

            return checkInfo;
        }
    }
}