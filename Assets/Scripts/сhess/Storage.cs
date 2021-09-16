using System.Collections.Generic;
using UnityEngine;
using board;
using rules;

namespace chess {
    public static class Storage {
        public const int knightRadius = 2;
         public static readonly Dictionary<PieceType, List<Movement>> movement =
                new Dictionary<PieceType, List<Movement>>() {
            {
                PieceType.Pawn,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1))),
                    Movement.LinearOnlyMove(Linear.Mk(new Vector2Int(-1, 0)), MovementType.Move),
                    Movement.LinearOnlyMove(Linear.Mk(new Vector2Int(1, 0)), MovementType.Move)
                }
            },
            {
                PieceType.Bishop,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1)))
                }
            },
            {
                PieceType.Rook,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0))),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, 1)))
                }
            },
            {
                PieceType.Queen,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0))),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, 1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1)))
                }
            },
            {
                PieceType.Knight,
                new List<Movement> {
                    Movement.Circular(Circular.Mk(2f))
                }
            },
            {
                PieceType.King,
                new List<Movement> {
                    Movement.Circular(Circular.Mk(1f))
                }
            }
        };
    }
}

