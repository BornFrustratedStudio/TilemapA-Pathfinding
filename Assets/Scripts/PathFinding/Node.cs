using UnityEngine;
using UnityEngine.Tilemaps;

namespace BornFrustrated.Pathfinding
{
    public class Node : IHeapElement<Node>
    {
        //the movement  value
        private int movePenalty;

        private int heapIndex;

        /// <summary>
        /// Her parent
        /// </summary>
        public Node parent;

        /// <summary>
        /// TileMap Position
        /// </summary>
        public Vector3Int LocalPlace { get; set; }

        /// <summary>
        /// World Position
        /// </summary>
        public Vector3 WorldLocation { get; set; }

        /// <summary>
        /// Tile Content
        /// </summary>
        public TileBase TileBase { get; set; }

        /// <summary>
        /// Node Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// is Walkable?
        /// </summary>
        public bool Walkable { get; set; }

        /// <summary>
        /// Distance from the starting node
        /// </summary>
        public int GCost { get; set; }

        /// <summary>
        /// Heulistic distance from end node
        /// </summary>
        public int HCost { get; set; }

        /// <summary>
        /// the sum of the Gcost + HCost
        /// </summary>
        public int FCost 
        {
            get { return GCost + HCost;  }
            private set { FCost = value; }
        }

        /// <summary>
        /// Movement penality
        /// </summary>
        public int MovePenalty 
        { 
            get { return movePenalty;  }
            set { movePenalty = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int HeapIndex 
        {
            get { return heapIndex;  }
            set { heapIndex = value; }
        }

        /// <summary>
        /// Comapare the node value
        /// </summary>
        /// <param name="nodeToCompare"></param>
        /// <returns></returns>
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
