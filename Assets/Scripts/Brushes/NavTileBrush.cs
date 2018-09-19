using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BornFrustrated.Pathfinding
{
	[CreateAssetMenu(fileName = "Navigation Tile brush", menuName = "Brushes/NavTile brush")]
	[CustomGridBrush(true, false, false, "Navigation Tile Brush")]
	public class NavTileBrush : GridBrushBase
	{
		public bool IsWalkable;
		public Vector2Int GridCellSize = new Vector2Int(32, 32);
		
		private WalkableTile m_walkableTile;
		private WalkableTile m_notWalkableTile;

		private WalkableTile WalkableTile
		{
			get 
			{
				if(m_walkableTile == null)
				{
					m_walkableTile = ScriptableObject.CreateInstance<WalkableTile>();
					m_walkableTile.SpriteSize = GridCellSize;
					m_walkableTile.name = "Walkable Tile";
					m_walkableTile.isWalkable = true;
				}

				return m_walkableTile;
			}
		}

		private WalkableTile NotWalkableTile
		{
			get 
			{
				if(m_notWalkableTile == null)
				{
					m_notWalkableTile = ScriptableObject.CreateInstance<WalkableTile>();
					m_notWalkableTile.SpriteSize = GridCellSize;
					m_notWalkableTile.name = "Not Walkable Tile";
					m_notWalkableTile.isWalkable = false;
				}

				return m_notWalkableTile;
			}
		}  

		public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			var tilemap = brushTarget.GetComponent<Tilemap>();
			if (tilemap == null)
				return;		

			TileBase tile = tilemap.GetTile(position);
			if (tile == null)
			{
		    	tilemap.SetTile(position, (IsWalkable)?WalkableTile:NotWalkableTile);
			}
			else
			{
				tilemap.SetTile(position, (IsWalkable)?WalkableTile:NotWalkableTile);
				tilemap.SetColor(position, (IsWalkable)?WalkableTile.ColorTile:NotWalkableTile.ColorTile);
				EditorUtility.SetDirty(tile);
				tilemap.RefreshTile(position);
			}

		}

		public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			var tilemap = brushTarget.GetComponent<Tilemap>();
			if (tilemap == null)
				return;		

		    TileBase tile = tilemap.GetTile(position);
			if (tile != null)
			{
				tilemap.SetTile(position, null);
			}
		}

		public override void Select(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			base.Select(gridLayout, brushTarget, position);
		}
	}
}
