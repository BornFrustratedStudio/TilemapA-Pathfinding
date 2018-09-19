using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
{

    public Vector3Int LocalPlace { get; set; }

    public Vector3 WorldLocation { get; set; }

    public TileBase TileBase { get; set; }

    public string Name { get; set; }

    public bool Walkable { get; set; }

    public int Cost { get; set; }

    public Node()
    {

    }
}
