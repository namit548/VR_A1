using UnityEngine;

public class TilesCell : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Vector2Int coordinates { get; set; }
    public Tile tile { get; set; }
    public bool empty => tile == null;



}
