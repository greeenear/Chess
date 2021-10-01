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
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1), 1), MovementType.AttackTrace),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1), 1), MovementType.AttackTrace),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0), 1), MovementType.Move)
                }
            },
            {
                PieceType.Bishop,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), -1), MovementType.Move)
                }
            },
            {
                PieceType.Rook,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, -1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, 1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, -1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, 1), -1), MovementType.Move)
                }
            },
            {
                PieceType.Queen,
                new List<Movement> {
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, -1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, 1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), -1), MovementType.Attack),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 0), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, -1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 0), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(0, 1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, 1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(1, -1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, -1), -1), MovementType.Move),
                    Movement.Linear(Linear.Mk(new Vector2Int(-1, 1), -1), MovementType.Move)
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