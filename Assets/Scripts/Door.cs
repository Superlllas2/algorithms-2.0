using System.Collections.Generic;
using UnityEngine;

public class Door
{
    public Vector2Int Position;
    public List<Room> ConnectedRooms = new();
    public Room RoomA;
    public Room RoomB;

    public Door(Vector2Int position, Room roomA, Room roomB)
    {
        Position = position;
        ConnectedRooms.Add(roomA);
        ConnectedRooms.Add(roomB);
    }
}