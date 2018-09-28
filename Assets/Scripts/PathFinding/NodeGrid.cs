using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;

namespace BornFrustrated.Pathfinding
{
    public class NodeGrid : MonoBehaviour
    {
        public Dictionary<Vector2, Node> tiles;
        
        public Transform debugPlayer;

        public Tilemap tileMap;

        [SerializeField]
	    private int obstacleProximityPenalty = 10;
        [SerializeField]
        private bool DebugNodeValue = true;

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
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);

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


                if (!NodeIsWalkable(new Vector3Int(localPlace.x + 1, localPlace.y, localPlace.z)) || !NodeIsWalkable(new Vector3Int(localPlace.x - 1, localPlace.y, localPlace.z))|| !NodeIsWalkable(new Vector3Int(localPlace.x, localPlace.y+1, localPlace.z)) || !NodeIsWalkable(new Vector3Int(localPlace.x, localPlace.y-1, localPlace.z)))
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
            Vector3Int localPlace = new Vector3Int(position.x, position.y, position.z);

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

        private void OnDrawGizmos()
        {
            if(Application.isPlaying && DebugNodeValue)
                DebugFCost();
        }

        public Node FindTheMostNearNode(Vector3 worldPosition)
        {
            Vector3 reachPosition = Vector3.zero;

            foreach(Vector3 p in tiles.Keys)
            {
                if (!tiles[p].Walkable)
                    continue;

                if (Vector3.Distance(worldPosition, reachPosition) > Vector3.Distance(p, worldPosition))
                    reachPosition = p;
            }
            return tiles[reachPosition];
        }

        void DebugFCost()
        {
            foreach (Node item in tiles.Values)
            {
                DrawString(item.FCost.ToString(), new Vector3(item.WorldLocation.x + 0.5f, item.WorldLocation.y + 1.5f), Color.white);
            }
        }

        public static void DrawString(string text, Vector3 worldPos, Color? textColor = null, Color? backColor = null)
        {
            UnityEditor.Handles.BeginGUI();
            var restoreTextColor = GUI.color;
            var restoreBackColor = GUI.backgroundColor;

            GUI.color = textColor ?? Color.white;
            GUI.backgroundColor = backColor ?? Color.black;

            var view = UnityEditor.SceneView.currentDrawingSceneView;
            if (view != null && view.camera != null)
            {
                Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
                {
                    GUI.color = restoreTextColor;
                    UnityEditor.Handles.EndGUI();
                    return;
                }
                Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
                var r = new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y);
                GUI.Box(r, text, EditorStyles.numberField);
                GUI.Label(r, text);
                GUI.color = restoreTextColor;
                GUI.backgroundColor = restoreBackColor;
            }
            UnityEditor.Handles.EndGUI();
        }


        public bool Reachable(Node node)
        {
            List<Node> nodes = GetNeighbours(node);
            int notReachable = 0;
            foreach(Node n in nodes)
            {
                if (n.FCost == 0)
                    notReachable++;
            }
            return notReachable < nodes.Count;
        }
    }
}
