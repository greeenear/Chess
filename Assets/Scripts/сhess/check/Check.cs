using System.Collections.Generic;
using UnityEngine;
using board;
using option;
using rules;
using move;

namespace check {
    public static class Check {
        public static bool CheckKing(
            Option<Piece>[,] board,
            PieceColor whoseMove,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            var kingPosition = FindKing(board, whoseMove);
            var canAttackKing = new List<MoveRes>();
            var attack = new List<MoveRes>();

            List<Movement> movmentList = movement[PieceType.Queen];
            canAttackKing.AddRange(Move.GetMoveCells(movmentList, kingPosition, board));

            movmentList = movement[PieceType.Knight];
            canAttackKing.AddRange(Move.GetMoveCells(movmentList, kingPosition, board));

            foreach (var pos in canAttackKing) {
                if (board[pos.end.x, pos.end.y].IsSome()) {
                    movmentList = movement[board[pos.end.x, pos.end.y].Peel().type];
                    Vector2Int piecePos = new Vector2Int(pos.end.x, pos.end.y);
                    attack.AddRange(Move.GetMoveCells(movmentList, piecePos, board));
                }
            }
            foreach (var attackPos in attack) {
                if (kingPosition == attackPos.end) {
                    return true;
                }
            }

            return false;
        }

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

        public static bool CheckMate(Option<Piece>[,] board,
            PieceColor whoseMove,
            Dictionary<PieceType,List<Movement>> movement
        ) {
            var canMovePosition = new List<MoveRes>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].IsSome()
                    && board[i, j].Peel().color == whoseMove) {
                        List<Movement> movmentList = movement[board[i, j].Peel().type];
                        Vector2Int pos = new Vector2Int(i, j);

                        canMovePosition = Move.GetMoveCells(movmentList, pos, board);
                        if (board[i, j].Peel().type == PieceType.Pawn) {
                            canMovePosition = Move.SelectPawnMoves(
                                board,
                                new Vector2Int(i, j),
                                canMovePosition);
                        }
                        canMovePosition = HiddenCheck(
                            canMovePosition,
                            new Vector2Int(i, j),
                            movement,
                            board
                        );
                        if (canMovePosition.Count != 0) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static List<MoveRes> HiddenCheck(
            List<MoveRes> canMovePos,
            Vector2Int piecePos,
            Dictionary<PieceType,List<Movement>> movement,
            Option<Piece>[,] startBoard
        ) {
            Option<Piece>[,] board;
            List<MoveRes> newCanMovePositions = new List<MoveRes>();
            var color = startBoard[piecePos.x, piecePos.y].Peel().color;

            foreach (var pos in canMovePos) {
                board = (Option<Piece>[,])startBoard.Clone();
                board[pos.end.x, pos.end.y] = board[piecePos.x, piecePos.y];
                board[piecePos.x, piecePos.y] = Option<Piece>.None();

                if (!CheckKing(board, color, movement)) {
                    newCanMovePositions.Add(pos);
                }

                board[piecePos.x, piecePos.y] = board[pos.end.x, pos.end.y];
                board[pos.end.x, pos.end.y] = Option<Piece>.None();
            }

            return newCanMovePositions;
        }
    }
}

