using System.Collections.Generic;
using UnityEngine;
using board;
using rules;

namespace chess {
    public class Resource : MonoBehaviour {
        public GameObject boardObj;
        public GameObject canMoveCell;
        public GameObject gameMenu;
        public GameObject changePawn;
        public Transform cellSize;
        public GameObject storageHighlightCells;
        public List<GameObject> pieceList = new List<GameObject>();

        public float halfBoardSize;
        public float halfCellSize;

        public Dictionary<PieceType, List<Movement>> movement =
                new Dictionary<PieceType, List<Movement>>() {
            { PieceType.Pawn, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, 0)))
                }
            },
            { PieceType.Bishop, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 1))),
                Movement.Linear(Linear.Mk(new Vector2Int(1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 1)))
                }
            },
            { PieceType.Rook, new List<Movement> {
                Movement.Linear(Linear.Mk(new Vector2Int(1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(0, -1))),
                Movement.Linear(Linear.Mk(new Vector2Int(-1, 0))),
                Movement.Linear(Linear.Mk(new Vector2Int(0, 1)))
                }
            },
            { PieceType.Queen, new List<Movement> {
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
            { PieceType.Knight, new List<Movement> {
                Movement.Circular(Circular.Mk(2f))
                }
            },
            { PieceType.King, new List<Movement> {
                Movement.Circular(Circular.Mk(1f))
                }
            }
        };

        private void Awake() {
            halfBoardSize = cellSize.lossyScale.x * 4;
            halfCellSize = cellSize.lossyScale.x / 2;
        }
    }
}

