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
            Dictionary<PieceType,List<Movement>> movement,
            MoveInfo lastMove
        ) {
            var kingPosition = FindKing(board, whoseMove);
            var canAttackKing = new List<MoveInfo>();
            var attack = new List<MoveInfo>();

            List<Movement> movmentList = movement[PieceType.Queen];
            canAttackKing.AddRange(Move.GetMoveCells(movmentList, kingPosition, board, lastMove));

            movmentList = movement[PieceType.Knight];
            canAttackKing.AddRange(Move.GetMoveCells(movmentList, kingPosition, board, lastMove));

            foreach (var pos in canAttackKing) {
                if (board[pos.first.to.x, pos.first.to.y].IsSome()) {
                    movmentList = movement[board[pos.first.to.x, pos.first.to.y].Peel().type];
                    Vector2Int piecePos = new Vector2Int(pos.first.to.x, pos.first.to.y);
                    attack.AddRange(Move.GetMoveCells(movmentList, piecePos, board, lastMove));
                }
            }
            foreach (var attackPos in attack) {
                if (kingPosition == attackPos.first.to) {
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

        public static bool CheckMate(
            Option<Piece>[,] board,
            PieceColor whoseMove,
            Dictionary<PieceType,List<Movement>> movement,
            MoveInfo lastMove
        ) {
            var canMovePosition = new List<MoveInfo>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].IsSome()
                    && board[i, j].Peel().color == whoseMove) {
                        List<Movement> movmentList = movement[board[i, j].Peel().type];
                        Vector2Int pos = new Vector2Int(i, j);

                        canMovePosition = Move.GetMoveCells(movmentList, pos, board,lastMove);
                        if (board[i, j].Peel().type == PieceType.Pawn) {
                            canMovePosition = Move.SelectPawnMoves(
                                board,
                                new Vector2Int(i, j),
                                canMovePosition,
                                lastMove
                            );
                        }
                        canMovePosition = HiddenCheck(
                            canMovePosition,
                            new Vector2Int(i, j),
                            movement,
                            board,
                            lastMove
                        );
                        if (canMovePosition.Count != 0) {
                            if (CheckKing(board, whoseMove, movement, lastMove)) {
                                Debug.Log("Check");
                            }

                            return false;
                        }
                    }
                }
            }
            Debug.Log("CheckMate");

            return true;
        }

        public static List<MoveInfo> HiddenCheck(
            List<MoveInfo> canMovePos,
            Vector2Int piecePos,
            Dictionary<PieceType,List<Movement>> movement,
            Option<Piece>[,] startBoard,
            MoveInfo lastMove
        ) {
            Option<Piece>[,] board;
            List<MoveInfo> newCanMovePositions = new List<MoveInfo>();
            var color = startBoard[piecePos.x, piecePos.y].Peel().color;

            foreach (var pos in canMovePos) {
                board = (Option<Piece>[,])startBoard.Clone();
                board[pos.first.to.x, pos.first.to.y] = board[piecePos.x, piecePos.y];
                board[piecePos.x, piecePos.y] = Option<Piece>.None();

                if (!CheckKing(board, color, movement, lastMove)) {
                    newCanMovePositions.Add(pos);
                }

                board[piecePos.x, piecePos.y] = board[pos.first.to.x, pos.first.to.y];
                board[pos.first.to.x, pos.first.to.y] = Option<Piece>.None();
            }

            return newCanMovePositions;
        }
    }
}

