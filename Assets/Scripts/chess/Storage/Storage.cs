using System.Collections.Generic;
using UnityEngine;
using board;
using rules;

namespace storage {
    public static class Storage {
         public static readonly Dictionary<PieceType, List<Movement>> movement =
                new Dictionary<PieceType, List<Movement>>() {
            {
                PieceType.Pawn,
                new List<Movement> {
                    Movement.LinearOnlyAttack(Linear.Mk(new Vector2Int(1, 1))),
                    Movement.LinearOnlyAttack(Linear.Mk(new Vector2Int(1, -1))),
                    Movement.LinearOnlyAttack(Linear.Mk(new Vector2Int(-1, -1))),
                    Movement.LinearOnlyAttack(Linear.Mk(new Vector2Int(-1, 1))),
                    Movement.LinearOnlyMove(Linear.Mk(new Vector2Int(-1, 0))),
                    Movement.LinearOnlyMove(Linear.Mk(new Vector2Int(1, 0)))
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

