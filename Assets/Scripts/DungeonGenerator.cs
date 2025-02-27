using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private RectInt room;
    
    void Start()
    {
        room = new RectInt(0, 0, 100, 50);
    }

    void Update()
    {
        
    }
}
