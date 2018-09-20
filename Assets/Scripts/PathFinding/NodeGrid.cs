using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace BornFrustrated.Pathfinding
{
    public class NodeGrid : MonoBehaviour
    {
        public Dictionary<Vector3, Node> tiles;

        public Transform player;

        public Tilemap tileMap;

        private void Awake()
        {
            GenerateTiles();
        }

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

                    Node curNaighboard;
                    if (tiles.TryGetValue(new Vector2(checkX, checkY), out curNaighboard))
                    {
                        neighbours.Add(curNaighboard);
                    }
                }
            }

            return neighbours;
        }

        private void GenerateTiles()
        {
            tiles = new Dictionary<Vector3, Node>();

            foreach (Vector3Int pos in tileMap.cellBounds.allPositionsWithin)
            {
                var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

                if (!tileMap.HasTile(localPlace) || tileMap.GetTile<WalkableTile>(localPlace) == null) continue; // if this tile map has not tile inside skip to the nextone

                var tile = new Node
                {
                    LocalPlace = localPlace,
                    WorldLocation = tileMap.CellToWorld(localPlace),
                    TileBase = tileMap.GetTile(localPlace),
                    Walkable = (tileMap.GetTile<WalkableTile>(localPlace).isWalkable), // Check if the current tilemap if equal to the target basetile, if is equal to true this tile map is not walkable
                    Name = localPlace.x + " , " + localPlace.y,
                };
                tiles.Add(tile.WorldLocation, tile);
            }
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            var localPlace = tileMap.WorldToCell(worldPosition);
            Node node;
            foreach(Node n in tiles.Values)
            {
                if (tiles.TryGetValue(localPlace, out node))
                {
                    Debug.Log("Found gameobject on node");
                    return node;
                }
            }
            Debug.LogError("Porca troia");
            return null;
        }

        private void Update()
        {
            //if (player != null)
            //    CheckPlayer();

        }

        private void CheckPlayer()
        {
            var localPlace = tileMap.WorldToCell(player.position);

            var PlayerNode = new Node
            {
                LocalPlace = localPlace,
                WorldLocation = tileMap.CellToWorld(localPlace),
                TileBase = tileMap.GetTile(localPlace),
                Name = localPlace.x + " , " + localPlace.y,
            };

            List<Node> cell = GetNeighbours(PlayerNode);
            if(cell.Count > 0)
            {
                foreach (Node n in cell)
                {
                    tileMap.SetColor(n.LocalPlace, Color.black);
                    tileMap.RefreshTile(n.LocalPlace);
                }
            }

            foreach (Node n in tiles.Values)
            {
                if (PlayerNode.LocalPlace == n.LocalPlace)
                {
                    Debug.Log("position " + n.LocalPlace + "Position " + n.WorldLocation);
                    if (!n.Walkable)
                        Debug.Log("Player On No Walkable Zone");

                }
            }
        }

    }
}


/*	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}
*/