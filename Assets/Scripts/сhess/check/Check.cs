using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using move;

namespace check {
    public struct AttackInfo {
        public Linear? linear;
        public Vector2Int? circleСenter;

        public static AttackInfo Mk(Linear? linear, Vector2Int? circleСenter) {
            return new AttackInfo { linear = linear, circleСenter = circleСenter };
        }
    }
    public struct CheckInfo {
        public Linear? linear;
        public Vector2Int? coveringPiece;
        public Vector2Int? attackingPiecePos;

        public static CheckInfo BlokingInfo(Linear linear, Vector2Int? coveringPiece) {
            return new CheckInfo { linear = linear, coveringPiece = coveringPiece };
        }

        public static CheckInfo CheckingInfo(Linear? linear, Vector2Int? attackingPiecePos) {
            return new CheckInfo { linear = linear, attackingPiecePos = attackingPiecePos };
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

        public static List<AttackInfo> GetAttackInfo(
            PieceColor color,
            Option<Piece>[,] startBoard,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            var attackInfo = new List<AttackInfo>();
            var pos = FindKing(startBoard, color);
            var boardSize = new Vector2Int(startBoard.GetLength(0), startBoard.GetLength(1));
            Option<Piece>[,] board = (Option<Piece>[,])startBoard.Clone();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if(i == pos.x && j == pos.y) {
                        continue;
                    }
                    if (board[i,j].IsSome()
                        && board[i,j].Peel().color == board[pos.x, pos.y].Peel().color) {
                        board[i, j] = Option<Piece>.None();
                    }
                }
            }
            var moveType = movement[PieceType.Queen];
            
            foreach (var dir in moveType) {
                int lineLength = 0;
                foreach (var move in Rules.GetLinearMoves(board, pos, dir.linear.Value, 8)) {
                    lineLength++;

                    if (board[move.x, move.y].IsSome()) {
                        if (movement[board[move.x , move.y].Peel().type].Contains(dir)) {
                            if (board[move.x, move.y].Peel().type == PieceType.Pawn) {
                                if (lineLength == 1 && dir.movementType == MovementType.Attack) {
                                    attackInfo.Add(AttackInfo.Mk(dir.linear.Value, null));
                                    break;
                                } else {
                                    break;
                                }
                            }
                            attackInfo.Add(AttackInfo.Mk(dir.linear.Value, null));
                        }
                    }
                }
            }
            moveType = movement[PieceType.Knight];
            foreach (var circle in moveType) {
                var moves = Rules.GetCirclularMoves(board, pos, circle.circular.Value, 22.5f);
                foreach (var move in moves) {
                    var cell = board[move.x, move.y];
                    if (cell.IsSome() && cell.Peel().type == PieceType.Knight) {
                        attackInfo.Add(AttackInfo.Mk(null, move));
                    }
                }
            }
            return attackInfo;
        }

        public static List<CheckInfo> GetCheckInfo(
            PieceColor color,
            Option<Piece>[,] startBoard,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            var checkInfo = new List<CheckInfo>();
            var attackInfo = GetAttackInfo(color, startBoard, movement);
            var boardSize = new Vector2Int(startBoard.GetLength(0), startBoard.GetLength(1));
            var pos = FindKing(startBoard, color);
            var kingCell = startBoard[pos.x, pos.y];

            foreach (var attack in attackInfo) {
                var piecesCounter = 0;
                if (attack.circleСenter.HasValue) {
                   checkInfo.Add(CheckInfo.CheckingInfo(null, attack.circleСenter));
                   continue;
                }

                for (int i = 1; i < 8; i++) {
                    var next = pos + attack.linear.Value.dir * i;
                    var nextPos = new Vector2Int(next.x, next.y);
                    var isOnBoard = Board.OnBoard(nextPos, boardSize);
                    if (!isOnBoard) {
                        break;
                    }
                    var nextCell = startBoard[next.x, next.y];
                    if (!nextCell.IsSome()) {
                        continue;
                    }
                    if (nextCell.Peel().color == kingCell.Peel().color) {
                        if (piecesCounter == 0) {
                            checkInfo.Add(CheckInfo.BlokingInfo(attack.linear.Value, nextPos));
                            piecesCounter++;
                        } else {
                            checkInfo.RemoveAt(checkInfo.Count - 1);
                            break;
                        }
                    }
                    if (nextCell.Peel().color != kingCell.Peel().color && piecesCounter == 0) {
                        checkInfo.Add(CheckInfo.CheckingInfo(attack.linear.Value, nextPos));
                        break;
                    }
                }
            }

            return checkInfo;
        }
    }
}