using System.Collections.Generic;
using UnityEngine;
using board;
using rules;
using option;
using check;

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

        public static MoveInfo Mk(DoubleMove doubleMove) {
            return new MoveInfo { doubleMove = doubleMove };
        }
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
            List<Movement> movementList,
            Vector2Int targetPos,
            Option<Piece>[,] board,
            MoveInfo lastMove
        ) {
            if (board[targetPos.x, targetPos.y].IsNone()) {
                return null;
            }

            var targetPiece = board[targetPos.x, targetPos.y].Peel();
            var moveInfos = new List<MoveInfo>();
            List<LimitedMovement> limitedMovements = new List<LimitedMovement>();
            foreach (var movement in movementList) {
                var fixedMovement = GetFixedMovement(movement, targetPos);
                limitedMovements.Add(GetLimetedMovement(fixedMovement, board));
            }
            
            var possibleMoveCells = GetMovePositions(limitedMovements, board);
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
            if (targetPiece.type == PieceType.King) {
                CheckCastling(moveInfos, board, targetPos, 1);
                CheckCastling(moveInfos, board, targetPos, -1);
            }
            if (targetPiece.type == PieceType.Pawn) {
                moveInfos = GetPawnPromotion(moveInfos, board);
            }
            if (board[lastMove.doubleMove.first.to.x, lastMove.doubleMove.first.to.y].IsNone()) {
                return moveInfos;
            }

            var piece = board[lastMove.doubleMove.first.to.x, lastMove.doubleMove.first.to.y];
            var moveLength = lastMove.doubleMove.first.from.x - lastMove.doubleMove.first.to.x;
            if (piece.Peel().type == PieceType.Pawn && Mathf.Abs(moveLength) == 2) {
                if (lastMove.doubleMove.first.from.y - targetPos.y == 1) {
                    CheckEnPassant(moveInfos, board, targetPos, 1);
                } else if (lastMove.doubleMove.first.from.y - targetPos.y == -1) {
                    CheckEnPassant(moveInfos, board, targetPos, -1);
                }
            }

            return moveInfos;
        }

        public static FixedMovement GetFixedMovement (
            Movement movement,
            Vector2Int startPos
        ) {
            var fixedMovements = FixedMovement.Mk(movement, startPos);

            return fixedMovements;
        }

        public static LimitedMovement GetLimetedMovement(
            FixedMovement fixedMovement,
            Option<Piece>[,] board
        ) {
            var limitedMovement = new LimitedMovement();
            if (board[fixedMovement.startPos.x, fixedMovement.startPos.y].IsNone()) {
                return limitedMovement;
            }

            var pieceOpt = board[fixedMovement.startPos.x, fixedMovement.startPos.y].Peel();
            if (pieceOpt.type == PieceType.Pawn) {
                var moveDir = fixedMovement.movement.linear.Value.dir;
                if ((pieceOpt.color == PieceColor.White && moveDir.x > 0)
                    || (pieceOpt.color == PieceColor.Black && moveDir.x < 0)) {
                    return limitedMovement;
                }

                var movementType = fixedMovement.movement.movementType;
                if (pieceOpt.moveCounter == 0 && movementType == MovementType.Move) {
                    limitedMovement = LimitedMovement.Mk(fixedMovement, 2);
                } else {
                    limitedMovement = LimitedMovement.Mk(fixedMovement, 1);
                }
            } else {
                limitedMovement = LimitedMovement.Mk(fixedMovement, board.GetLength(0));
            }
            

            return limitedMovement;
        }

        public static List<Vector2Int> GetMovePositions(
            List<LimitedMovement> limitedMovements,
            Option<Piece>[,] board
        ) {
            var possibleMoveCells = new List<Vector2Int>();

            foreach (var limMovment in limitedMovements) {
                if (limMovment.fixedMovement.movement.linear.HasValue) {
                    possibleMoveCells.AddRange(Rules.GetLinearMoves(board, limMovment));

                } else if (limMovment.fixedMovement.movement.circular.HasValue) {
                    possibleMoveCells = Rules.GetCirclularMoves(board, limMovment.fixedMovement);
                }
            }

            return possibleMoveCells;
        }

        private static List<MoveInfo> GetPawnPromotion(
            List<MoveInfo> moveInfos,
            Option<Piece>[,] board
        ) {
            List<MoveInfo> newMoveInfos = new List<MoveInfo>();
            foreach (var info in moveInfos) {
                var moveTo = info.doubleMove.first.to;
                if (moveTo.x == 0 || moveTo.x == board.GetLength(1)) {
                    var promotion = info;
                    promotion.pawnPromotion = true;
                    newMoveInfos.Add(promotion);
                }
                newMoveInfos.Add(info);
            }
            return newMoveInfos;
        }

        private static List<MoveInfo> CheckEnPassant(
            List<MoveInfo> newPossibleMoves,
            Option<Piece>[,] board,
            Vector2Int pos,
            int dir
        ) {
            Option<Piece> checkedCell = new Option<Piece>();
            var boardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            int colorDir = 1;

            if (Board.OnBoard(new Vector2Int(pos.x, pos.y + dir), boardSize)) {
                checkedCell = board[pos.x, pos.y + dir];
            }
            if (board[pos.x, pos.y].IsNone() || checkedCell.IsNone()) {
                return newPossibleMoves;
            }

            var pawnOpt = board[pos.x, pos.y].Peel();
            var checkedCellOpt = checkedCell.Peel();
            if (pawnOpt.color == PieceColor.White) {
                colorDir = -1;
            } else {
                colorDir = 1;
            }

            if (checkedCellOpt.color != pawnOpt.color && checkedCellOpt.type == PieceType.Pawn
                && checkedCellOpt.moveCounter == 1) {
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
            List<MoveInfo> newPossibleMoves,
            Option<Piece>[,] board,
            Vector2Int pos,
            int dir
        ) {
            if (board[pos.x, pos.y].IsNone()) {
                return;
            }
            var king = board[pos.x, pos.y].Peel();
            var rookPos = new Vector2Int();
            if (dir == -1) {
                rookPos = new Vector2Int(pos.x, 0);
            } else {
                rookPos = new Vector2Int(pos.x, board.GetLength(0) - 1);
            }
            int i = pos.y + dir;
            if (king.moveCounter != 0) {
                return;
            }

            while (i != rookPos.y) {
                var currentPos = new Vector2Int(pos.x, i);
                foreach (var info in Check.GetCheckInfo(board, king.color, currentPos)) {
                    if (info.coveringPiece == null) {
                        break;
                    }
                }

                i = i + dir;
                if (board[pos.x, i].IsNone()) {
                    continue;
                }

                var currentPieceOpt = board[pos.x, i].Peel();
                if (currentPieceOpt.type != PieceType.Rook) {
                    break;
                } else if (currentPieceOpt.type == PieceType.Rook){
                    if (i == rookPos.y && currentPieceOpt.moveCounter == 0) {
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