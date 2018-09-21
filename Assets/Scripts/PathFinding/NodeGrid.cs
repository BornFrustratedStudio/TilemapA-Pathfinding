using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace BornFrustrated.Pathfinding
{
    public class NodeGrid : MonoBehaviour
    {
        public Dictionary<Vector2, Node> tiles;

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

                    if(!tiles.ContainsKey(new Vector2(checkX, checkY)))
                    {
                        continue;
                    }

                    if ( tiles[new Vector2(checkX, checkY)] != null)
                    {
                        neighbours.Add(tiles[new Vector2(checkX, checkY)]);
                    }
                }
            }

            return neighbours;
        }

        private void GenerateTiles()
        {
            tiles = new Dictionary<Vector2, Node>();

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

        private void Update()
        {
            
            //if (player != null)
            //    CheckPlayer();

        }
        //#region Shitty path
        Action<Vector3[], bool> point;
        public Transform target;

private void Start() {
    PathRequestManager.RequestPath(player.position, target.position, point);
}

        private void OnEnable()
        {
            point += CheckPathBox;
        }
        private void OnDisable()
    {
           point -= CheckPathBox;
        }

        void CheckPathBox(Vector3[] point, bool aasd)
        {
           List<Node> node = new List<Node>();

           for (int i = 0; i < point.Length; i++)
           {
               if (tiles.ContainsKey(point[i]))
               {
                   node.Add(tiles[new Vector3(point[i].x,point[i].y,0)]);
               }

           }

           if (node.Count > 0)
           {
               foreach (Node n in node)
               {
                   UnityEngine.Debug.Log("Pippo");
                   tileMap.SetColor(n.LocalPlace, Color.gray);
                   tileMap.RefreshTile(n.LocalPlace);
               }
           }
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            NodeFromWorldPoint(player.position);
            sw.Stop();
            UnityEngine.Debug.Log("Node Found in : " + sw.ElapsedMilliseconds + " ms");
            foreach (Node n in tiles.Values)
            {
                if (PlayerNode.LocalPlace == n.LocalPlace)
                {
                   UnityEngine. Debug.Log("position " + n.LocalPlace + "Position " + n.WorldLocation);
                    if (!n.Walkable)
                        UnityEngine.Debug.Log("Player On No Walkable Zone");

                }
            }
        }

    }
}
