using UnityEngine;
using UnityEngine.Tilemaps;

namespace BornFrustrated.Pathfinding
{
    public class Node : IHeapElement<Node>
    {
        public Node parent;

        public Vector3Int LocalPlace { get; set; }

        public Vector3 WorldLocation { get; set; }

        public TileBase TileBase { get; set; }

        public string Name { get; set; }

        public bool Walkable { get; set; }

        public int GCost { get; set; }

        public int HCost { get; set; }

        public int FCost 
        {
            get { return GCost + HCost;  }
            private set { FCost = value; }
        }

        public int MovePenality 
        { 
            get { return movePenality;  }
            set { movePenality = value; }
        }

        private int movePenality;

        private int heapIndex;

        public int HeapIndex 
        {
            get { return heapIndex;  }
            set { heapIndex = value; }
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);
            if (compare == 0)
            {
                compare = HCost.CompareTo(nodeToCompare.HCost);
            }
            return -compare;
        }
    }
}
