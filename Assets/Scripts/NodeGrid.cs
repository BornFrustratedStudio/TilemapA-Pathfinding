using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

namespace BornFrustrated
{
    public class NodeGrid : MonoBehaviour
    {
        public Dictionary<Vector3, Node> tiles;

        public Transform player;

        public TileBase targetTile;

        public Tilemap tileMap;

        private void Awake()
        {
            GenerateTiles();
        }

        private void GenerateTiles()
        {
            tiles = new Dictionary<Vector3, Node>();

            foreach (Vector3Int pos in tileMap.cellBounds.allPositionsWithin)
            {
                var localPlace = new Vector3Int(pos.x, pos.y, pos.z);

                if (!tileMap.HasTile(localPlace)) continue; // if this tile map has not tile inside skip to the nextone

                var tile = new Node
                {
                    LocalPlace = localPlace,
                    WorldLocation = tileMap.CellToWorld(localPlace),
                    TileBase = tileMap.GetTile(localPlace),
                    Walkable = !(tileMap.GetTile(localPlace) == targetTile), // Check if the current tilemap if equal to the target basetile, if is equal to true this tile map is not walkable
                    Name = localPlace.x + " , " + localPlace.y,
                    Cost = 1,
                };
                tiles.Add(tile.WorldLocation, tile);
            }
        }

        private void Update()
        {
            var localPlace = tileMap.WorldToCell(player.position);

            var PlayerNode = new Node
            {
                LocalPlace = localPlace,
                WorldLocation = tileMap.CellToWorld(localPlace),
                TileBase = tileMap.GetTile(localPlace),
                Walkable = !(tileMap.GetTile(localPlace) == targetTile), // Check if the current tilemap if equal to the target basetile, if is equal to true this tile map is not walkable
                Name = localPlace.x + " , " + localPlace.y,
            };

            foreach (Node n in tiles.Values)
            {
                if (PlayerNode.LocalPlace == n.LocalPlace)
                {
                    Debug.Log("position " + n.LocalPlace);
                    if (!n.Walkable)
                        Debug.Log("Player On No Walkable Zone");

                }
            }

        }
    }
}


