using System.Collections.Generic;
using UnityEngine;
using board;
using rules;

namespace chess {
    public class Resource : MonoBehaviour {
        public GameObject boardObj;
        public GameObject canMoveCell;
        public GameObject underAttackCell;
        public GameObject gameMenu;
        public GameObject changePawn;
        public static GameObject gameMenuPanel;
        public static GameObject changePawnPanel;
        public Transform cellSize;
        public GameObject storageHighlightCells;

        public LayerMask highlightMask;
        public List<GameObject> pieceList = new List<GameObject>();

        public Vector3 halfBoardSize;
        public Vector3 halfCellSize;

        private void Awake() {
            halfBoardSize = cellSize.lossyScale * 4;
            halfCellSize = cellSize.lossyScale / 2;
            gameMenuPanel = gameMenu;
            changePawnPanel = changePawn;
        }
    }
}