using System.Collections.Generic;
using NUnit.Framework;

public class DungeonConnectivityTests
{
    [Test]
    public void GraphConnected_BFS_ChecksConnectivity()
    {
        var roomA = new Room();
        var roomB = new Room();
        var roomC = new Room();

        roomA.ConnectedRooms.Add(roomB);
        roomB.ConnectedRooms.Add(roomA);
        roomB.ConnectedRooms.Add(roomC);
        roomC.ConnectedRooms.Add(roomB);

        var rooms = new List<Room> { roomA, roomB, roomC };
        Assert.IsTrue(DungeonGenerator.IsGraphConnected(rooms));
    }

    [Test]
    public void GraphDisconnected_BFS_FindsDisconnection()
    {
        var roomA = new Room();
        var roomB = new Room();
        var roomC = new Room();

        roomA.ConnectedRooms.Add(roomB);
        roomB.ConnectedRooms.Add(roomA);
        // roomC is isolated

        var rooms = new List<Room> { roomA, roomB, roomC };
        Assert.IsFalse(DungeonGenerator.IsGraphConnected(rooms));
    }
}
