using System.Data.SqlTypes;
using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using move;

namespace check {
    public struct CheckInfo {
        public FixedMovement attackInfo;
        public Vector2Int? coveringPiece;

        public static CheckInfo Mk(FixedMovement attackInfo, Vector2Int? coveringPiece) {
            return new CheckInfo { attackInfo = attackInfo, coveringPiece = coveringPiece };
        }
    }

    public static class Check {
        public static Vector2Int FindKing(Option<Piece>[,] board, PieceColor color) {
            Vector2Int kingPosition = new Vector2Int();

            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    var piece = board[i, j].Peel();
                    if (piece.type == PieceType.King && piece.color == color) {
                        kingPosition.x = i;
                        kingPosition.y = j;
                    }
                }
            }

            return kingPosition;
        }

        public static List<FixedMovement> GetPotentialChecks(
            PieceColor color,
            Option<Piece>[,] board,
            Dictionary<PieceType,List<Movement>> movement,
            Vector2Int pos
        ) {
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var attackInfo = new List<FixedMovement>();
            var movementType = new List<Movement>(movement[PieceType.Queen]);
            movementType.AddRange(movement[PieceType.Knight]);
        
            foreach (var type in movementType) {
                if (type.circular.HasValue) {
                    var moves = Rules.GetCirclularMoves(board, pos, type.circular.Value);
                    foreach (var move in moves) {
                        var cell = board[move.x, move.y];
                        var circle = Movement.Circular(Circular.Mk(2));
                        var attackingPiecePos = new Vector2Int(move.x, move.y);

                        if (cell.IsSome() && cell.Peel().type == PieceType.Knight) {
                            attackInfo.Add(FixedMovement.Mk(circle, attackingPiecePos));
                        }
                    }
                    continue;
                }

                int lineLength = 0;
                var lineMoves = Rules.GetLinearMoves(board, pos, type.linear.Value, boardSize.x);
                foreach (var move in lineMoves) {
                    lineLength++;

                    if (board[move.x, move.y].IsNone()) {
                        continue;
                    }
                    if (!movement[board[move.x , move.y].Peel().type].Contains(type)) {
                        break;
                    }
                    var attackDir = Movement.Linear(type.linear.Value);
                    var attackingPiecePos = new Vector2Int(move.x, move.y);

                    if (board[move.x, move.y].Peel().type == PieceType.Pawn) {
                        if (lineLength == 1 && type.movementType == MovementType.Attack) {
                            attackInfo.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
                            break;
                        } else {
                            break;
                        }
                    }
                    attackInfo.Add(FixedMovement.Mk(attackDir, attackingPiecePos));
                }
            }

            return attackInfo;
        }

        public static List<CheckInfo> GetCheckInfo(
            PieceColor color,
            Option<Piece>[,] startBoard,
            List<FixedMovement> attackInfo,
            Vector2Int pos
        ) {
            var checkInfo = new List<CheckInfo>();
            var boardSize = new Vector2Int(startBoard.GetLength(0), startBoard.GetLength(1));

            foreach (var info in attackInfo) {
                var piecesCounter = 0;
                if (info.movement.circular.HasValue) {
                    checkInfo.Add(CheckInfo.Mk(info, null));
                    continue;
                }

                for (int i = 1; i < boardSize.x; i++) {
                    var next = pos + info.movement.linear.Value.dir * i;
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
                        checkInfo.Add(CheckInfo.Mk(info, null));
                        break;
                    }
                }
            }

            return checkInfo;
        }
    }
}