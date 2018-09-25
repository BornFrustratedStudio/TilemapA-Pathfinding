using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace BornFrustrated.Pathfinding
{
    public class NodeGrid : MonoBehaviour
    {
        public Dictionary<Vector2, Node> tiles;
        
        public Transform player;

        public Tilemap tileMap;

	    public int obstacleProximityPenalty = 10;

        private void Awake()
        {
            GenerateTiles();
        }

        /// <summary>
        /// Generate node from the tile map
        /// </summary>
        private void GenerateTiles()
        {
            tiles = new Dictionary<Vector2, Node>();

            foreach (Vector3Int pos in tileMap.cellBounds.allPositionsWithin)
            {
                UnityEngine.Debug.Log("CULO");
                var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

                if (!tileMap.HasTile(localPlace) || tileMap.GetTile<WalkableTile>(localPlace) == null) continue; // if this tile map has not tile inside skip to the nextone

                int movementPenalty = 0;

                Node tile = new Node
                {
                    LocalPlace = localPlace,
                    WorldLocation = tileMap.CellToWorld(localPlace),
                    TileBase = tileMap.GetTile(localPlace),
                    Walkable = (tileMap.GetTile<WalkableTile>(localPlace).isWalkable), // Check if the current tilemap if equal to the target basetile, if is equal to true this tile map is not walkable
                    Name = localPlace.x + " , " + localPlace.y,
                };


                if (!NodeIsWalkable(new Vector3Int(localPlace.x + 1, localPlace.y, localPlace.z)) || !NodeIsWalkable(new Vector3Int(localPlace.x - 1, localPlace.y, localPlace.z)))
                    movementPenalty += obstacleProximityPenalty;


                if (!tile.Walkable)
                {
                    movementPenalty += obstacleProximityPenalty * 2;
                }

                tile.MovePenalty = movementPenalty;

                tiles.Add(tile.WorldLocation, tile);
            }
        }

        /// <summary>
        /// Get the neighbours from a node
        /// </summary>
        /// <param name="node">current node</param>
        /// <returns>list of neighbours</returns>
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    float checkX = node.WorldLocation.x + x;
                    float checkY = node.WorldLocation.y + y;

                    if (!tiles.ContainsKey(new Vector2(checkX, checkY)))
                    {
                        continue;
                    }

                    if (tiles[new Vector2(checkX, checkY)] != null)
                    {
                        neighbours.Add(tiles[new Vector2(checkX, checkY)]);
                    }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Check if the node is walkable
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>if is walkable</returns>
        public bool NodeIsWalkable(Vector3Int position)
        {
            var localPlace = new Vector3Int(position.x, position.y, position.z);

            if (!tileMap.HasTile(localPlace) || tileMap.GetTile<WalkableTile>(localPlace) == null)
                return false;

            if (!tileMap.GetTile<WalkableTile>(localPlace).isWalkable)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get the node from a world position
        /// </summary>
        /// <param name="worldPosition">world position</param>
        /// <returns>node</returns>
        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
 
            Vector3 localPlace = tileMap.WorldToCell(worldPosition);

            if (tiles.ContainsKey(localPlace))
            {
                if (tiles[localPlace] != null)
                {
                    return tiles[localPlace];
                }
            }
            return null;
        }
    }
}
