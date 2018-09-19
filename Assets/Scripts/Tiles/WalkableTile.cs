using System; 
using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using UnityEngine.Tilemaps; 

namespace BornFrustrated.Pathfinding
{
	[Serializable]
	[CreateAssetMenu(fileName = "New Walkable Tile", menuName = "Tiles/Walkable Tile")]
	public class WalkableTile:TileBase 
	{
		public bool isWalkable = true;

		public Vector2Int SpriteSize = new Vector2Int(32, 32);

		private Sprite sprite;

		public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
		{
			if(sprite == null)
			{
				Texture2D left = new Texture2D(SpriteSize.x, SpriteSize.y, TextureFormat.RGBA32, false);
 				left.Apply();

				 sprite = Sprite.Create(left,
        								new Rect(0, 0, left.width, left.height),
        								new Vector2(0.5f,0.5f),
        								SpriteSize.x);
			}
			tileData.flags = TileFlags.None;
			tileData.sprite = sprite;
			tileData.color = (isWalkable) ? Color.blue : Color.red;
		}
	}
}
