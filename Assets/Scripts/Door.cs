using UnityEngine;

public class Door
{
    public Vector2Int Position;
    public Room RoomA;
    public Room RoomB;

    public Door(Vector2Int position, Room roomA, Room roomB)
    {
        Position = position;
        RoomA = roomA;
        RoomB = roomB;
    }
}