using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    private RectInt initRoom;
    private List<RectInt> rooms;
    private RectInt room1;
    private RectInt room2;
    private int gap = 10;

    public int roomNum = 2;

    void Start()
    {
        initRoom = new RectInt(0, 0, 100, 50);
        for (var i = 0; i < roomNum; i++)
        {
            room1 = new RectInt(i * initRoom.width / 2, 0, initRoom.width / 2 + gap, 50);
            room2 = new RectInt(i * initRoom.width / 2, 0, initRoom.width / 2 + gap, 50);
        }
    }

    void Update()
    {
        AlgorithmsUtils.DebugRectInt(initRoom, Color.red);
        AlgorithmsUtils.DebugRectInt(room1, Color.red);
        AlgorithmsUtils.DebugRectInt(room2, Color.red);
    }
}