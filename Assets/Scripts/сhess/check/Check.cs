using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using move;

namespace check {
    public struct CheckInfo {
        public Linear linear;
        public Vector2Int coveringPiece;
        public Vector2Int? realCheck;

        public static CheckInfo BlokingInfo(Linear linear, Vector2Int coveringPiece) {
            return new CheckInfo { linear = linear, coveringPiece = coveringPiece };
        }

        public static CheckInfo RealCheckInfo(Linear linear, Vector2Int? realCheck) {
            return new CheckInfo { linear = linear, realCheck = realCheck };
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

        public static List<Linear> GetAttackingDirections(
            PieceColor color,
            Option<Piece>[,] startBoard,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            var linearDirList = new List<Linear>();
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
                                    linearDirList.Add(dir.linear.Value);
                                    break;
                                } else {
                                    break;
                                }
                            }
                            linearDirList.Add(dir.linear.Value);
                        }
                    }
                }
            }

            return linearDirList;
        }

        public static List<CheckInfo> GetCheckInfo(
            PieceColor color,
            Option<Piece>[,] startBoard,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            var checkInfo = new List<CheckInfo>();
            var linearDirList = GetAttackingDirections(color, startBoard, movement);
            var boardSize = new Vector2Int(startBoard.GetLength(0), startBoard.GetLength(1));
            var pos = FindKing(startBoard, color);
            var kingCell = startBoard[pos.x, pos.y];

            foreach (var line in linearDirList) {
                var piecesCounter = 0;
                for (int i = 1; i < 8; i++) {
                    var nextX = pos.x + line.dir.x * i;
                    var newtY = pos.y + line.dir.y * i;
                    var nextPos = new Vector2Int(nextX, newtY);
                    var isOnBoard = Board.OnBoard(nextPos, boardSize);
                    if (!isOnBoard) {
                        break;
                    }
                    var nextCell = startBoard[nextX, newtY];
                    if (!nextCell.IsSome()) {
                        continue;
                    }
                    if (nextCell.Peel().color == kingCell.Peel().color) {
                        if (piecesCounter == 0) {
                            checkInfo.Add(CheckInfo.BlokingInfo(line, nextPos));
                            piecesCounter++;
                        } else {
                            checkInfo.RemoveAt(checkInfo.Count - 1);
                            break;
                        }
                    }
                    if (nextCell.Peel().color != kingCell.Peel().color && piecesCounter == 0) {
                        checkInfo.Add(CheckInfo.RealCheckInfo(line, nextPos));
                        break;
                    }
                }
            }

            return checkInfo;
        }
    }
}