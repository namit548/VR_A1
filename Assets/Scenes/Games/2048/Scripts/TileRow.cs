using UnityEngine;

public class TileRow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TileCell[] cells { get; private set; }      
    private void Awake()
    {
        cells = GetComponentsInChildren<TileCell>();
    }
}
