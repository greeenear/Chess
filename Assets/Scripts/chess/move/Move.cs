using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;

namespace move {
    public struct MoveData {
        public Vector2Int from;
        public Vector2Int to;

        public static MoveData Mk(Vector2Int from, Vector2Int to) {
            return new MoveData { from = from, to = to };
        }
    }

    public struct DoubleMove {
        public MoveData first;
        public MoveData? second;

        public static DoubleMove MkSingleMove(MoveData first) {
            return new DoubleMove { first = first};
        }
        public static DoubleMove MkDoubleMove(MoveData first, MoveData? second) {
            return new DoubleMove {first = first, second = second};
        }
    }

    public struct MoveInfo {
        public DoubleMove doubleMove;
        public Vector2Int? sentenced;
        public bool pawnPromotion;
    }

    public struct StartAngle {
        public const float Knight = 22.5f;
        public const float King = 20f;
    }

    public static class Move {
        public static void MovePiece(Vector2Int start, Vector2Int end, Option<Piece>[,] board) {
            board[end.x, end.y] = board[start.x, start.y];
            board[start.x, start.y] = Option<Piece>.None();
            var piece = board[end.x, end.y].Peel();
            piece.moveCounter++;
            board[end.x, end.y] = Option<Piece>.Some(piece);
        }

        public static List<MoveInfo> GetMoveInfos(
            List<Movement> moveList,
            Vector2Int targetPos,
            Option<Piece>[,] board,
            MoveInfo lastMove
        ) {
            var moveInfos = new List<MoveInfo>();
            var fixedMovements = GetFixedMovements(moveList, targetPos);
            var limitedMovements = GetLimetedMovements(fixedMovements, board);
            var possibleMoveCells = GetAllMoves(limitedMovements, board);

            foreach (var move in possibleMoveCells) {
                if (board[move.x, move.y].IsSome()) {
                    moveInfos.Add(
                        new MoveInfo {
                            doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(targetPos, move)), 
                            sentenced = move
                        }
                    );
                } else {
                    moveInfos.Add(
                        new MoveInfo {
                            doubleMove = DoubleMove.MkSingleMove(MoveData.Mk(targetPos, move))
                        }
                    );
                }
            }

            return moveInfos;
        }

        public static List<FixedMovement> GetFixedMovements (
            List<Movement> movementList,
            Vector2Int startPos
        ) {
            var fixedMovements = new List<FixedMovement>();
            foreach (var movement in movementList) {
                fixedMovements.Add(FixedMovement.Mk(movement, startPos));
            }

            return fixedMovements;
        }

        public static List<LimitedMovement> GetLimetedMovements(
            List<FixedMovement> fixedMovements,
            Option<Piece>[,] board
        ) {
            var limitedMovements = new List<LimitedMovement>();
            foreach (var movement in fixedMovements) {
                if (board[movement.startPos.x, movement.startPos.y].IsNone()) {
                    return limitedMovements;
                }

                var pieceOpt = board[movement.startPos.x, movement.startPos.y].Peel();
                if (pieceOpt.type == PieceType.Pawn) {
                    var moveDir = movement.movement.linear.Value.dir;
                    if ((pieceOpt.color == PieceColor.White && moveDir.x > 0) 
                        || (pieceOpt.color == PieceColor.Black && moveDir.x < 0)) {
                        continue;
                    }

                    var movementType = movement.movement.movementType;
                    if (pieceOpt.moveCounter == 0 && movementType == MovementType.Move) {
                        limitedMovements.Add(LimitedMovement.Mk(movement, 2));
                    } else {
                        limitedMovements.Add(LimitedMovement.Mk(movement, 1));
                    }
                } else {
                    limitedMovements.Add(LimitedMovement.Mk(movement, board.GetLength(0)));
                }
            }

            return limitedMovements;
        }

        public static List<Vector2Int> GetAllMoves(
            List<LimitedMovement> limitedMovements,
            Option<Piece>[,] board
        ) {
            var possibleMoveCells = new List<Vector2Int>();

            foreach (var limMovment in limitedMovements) {
                if (limMovment.fixedMovement.movement.linear.HasValue) {
                    possibleMoveCells.AddRange(Rules.GetLinearMoves(board, limMovment));

                } else if (limMovment.fixedMovement.movement.circular.HasValue) {
                    possibleMoveCells = Rules.GetCirclularMoves(board, limMovment);
                }
            }

            return possibleMoveCells;
        }

        private static List<MoveInfo> CheckEnPassant(
            List<MoveInfo> newPossibleMoves,
            Option<Piece>[,] board,
            Vector2Int pos,
            int colorDir,
            int dir
        ) {
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            Option<Piece> checkedCell = new Option<Piece>();
            if (Board.OnBoard(new Vector2Int(pos.x, pos.y + dir), boardSize)) {
                checkedCell = board[pos.x, pos.y + dir];
            }
            Piece pawn = board[pos.x, pos.y].Peel();

            if (checkedCell.IsSome() && checkedCell.Peel().color != pawn.color
                && checkedCell.Peel().type == PieceType.Pawn
                && checkedCell.Peel().moveCounter == 1) {
                newPossibleMoves.Add(
                    new MoveInfo {
                        doubleMove = DoubleMove.MkSingleMove(
                            MoveData.Mk(
                                pos,
                                new Vector2Int(pos.x + colorDir, pos.y + dir)
                            )
                        ),
                        sentenced = new Vector2Int(pos.x, pos.y + dir)
                    }
                );
            }
            return newPossibleMoves;
        }

        private static void CheckCastling(
            Vector2Int pos,
            Option<Piece>[,] board,
            List<MoveInfo> newPossibleMoves,
            int dir
        ) {
            Vector2Int rookPos = new Vector2Int();
            if (dir == -1) {
                rookPos = new Vector2Int(pos.x, 0);
            } else {
                rookPos = new Vector2Int(pos.x, board.GetLength(0) - 1);
            }
            int i = pos.y + dir;
            if (board[pos.x, pos.y].Peel().moveCounter != 0) {
                return;
            }
            while (i != rookPos.y) {
                i = i + dir;
                if (board[pos.x, i].IsSome() && board[pos.x, i].Peel().type != PieceType.Rook) {
                    break;
                } else if (board[pos.x, i].Peel().type == PieceType.Rook){
                    if (i == rookPos.y && board[pos.x, i].Peel().moveCounter == 0) {
                        newPossibleMoves.Add(new MoveInfo {
                            doubleMove = DoubleMove.MkDoubleMove(
                                MoveData.Mk(pos, new Vector2Int(pos.x, pos.y + 2 * dir)),
                                MoveData.Mk(rookPos, new Vector2Int(pos.x, pos.y + dir)))
                        });
                    }
                }
            }
        }
    }
}