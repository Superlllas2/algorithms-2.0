using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int overlap = 1;
    public RectInt initialBounds = new RectInt(0, 0, 100, 60);
    public int minSplitSize = 20;
    public int maxDepth = 4;

    private BSPNode rootNode;
    private List<Room> allRooms = new();
    private List<Door> doors = new();

    void Start()
    {
        rootNode = new BSPNode { Bounds = initialBounds };
        Split(rootNode, maxDepth);
        CreateRooms(rootNode);
        ConnectRooms(rootNode);
    }

    void Update()
    {
        DrawDebugRects(rootNode);
        DrawDebugDoors(doors);
    }

    void Split(BSPNode node, int depth)
    {
        if (depth == 0 || node.Bounds.width < minSplitSize * 2 && node.Bounds.height < minSplitSize * 2)
            return;

        bool splitHorizontally = Random.value > 0.5f;

        if (node.Bounds.width > node.Bounds.height)
            splitHorizontally = false;
        else if (node.Bounds.height > node.Bounds.width)
            splitHorizontally = true;

        if (splitHorizontally)
        {
            // Debug.Log("splitHorizontally");
            int splitY = Random.Range(minSplitSize, node.Bounds.height - minSplitSize);
            node.Left = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y, node.Bounds.width, splitY + overlap)
            };
            node.Right = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y + splitY, node.Bounds.width, node.Bounds.height - splitY)
            };
        }
        else
        {
            // Debug.Log("splitVertically");
            int splitX = Random.Range(minSplitSize, node.Bounds.width - minSplitSize);
            node.Left = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y, splitX + overlap, node.Bounds.height)
            };
            node.Right = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x + splitX, node.Bounds.y, node.Bounds.width - splitX, node.Bounds.height)
            };
        }

        Split(node.Left, depth - 1);
        Split(node.Right, depth - 1);
    }
    
    void ConnectRooms(BSPNode node)
    {
        if (node.Left == null || node.Right == null)
            return;

        var roomA = GetRoomInSubtree(node.Left);
        var roomB = GetRoomInSubtree(node.Right);

        if (roomA != null && roomB != null)
        {
            // Get shared wall
            var overlap = AlgorithmsUtils.Intersect(roomA.Bounds, roomB.Bounds);

            if (overlap.width > 0 || overlap.height > 0)
            {
                Vector2Int doorPos;

                if (overlap.width > 0) // Vertical wall
                {
                    int centerX = overlap.xMin + overlap.width / 2;
                    int centerY = overlap.yMin + overlap.height / 2;
                    doorPos = new Vector2Int(centerX, centerY);
                }
                else // Horizontal wall
                {
                    int centerX = overlap.xMin + overlap.width / 2;
                    int centerY = overlap.yMin + overlap.height / 2;
                    doorPos = new Vector2Int(centerX, centerY);
                }

                var door = new Door(doorPos, roomA, roomB);
                doors.Add(door);
            }
        }

        ConnectRooms(node.Left);
        ConnectRooms(node.Right);
    }

    Room GetRoomInSubtree(BSPNode node)
    {
        if (node == null) return null;

        if (node.Room != null)
            return node.Room;

        Room left = GetRoomInSubtree(node.Left);
        if (left != null) return left;

        return GetRoomInSubtree(node.Right);
    }

    void CreateRooms(BSPNode node)
    {
        if (node.Left != null || node.Right != null)
        {
            if (node.Left != null) CreateRooms(node.Left);
            if (node.Right != null) CreateRooms(node.Right);
            return;
        }

        Room room = new Room { Bounds = node.Bounds };
        node.Room = room;
        allRooms.Add(room);
    }


    void DrawDebugRects(BSPNode node)
    {
        if (node == null) return;

        AlgorithmsUtils.DebugRectInt(node.Bounds, Color.red);

        DrawDebugRects(node.Left);
        DrawDebugRects(node.Right);
    }

    void DrawDebugDoors(List<Door> doorsList)
    {
        foreach (var door in doorsList)
        {
            var pos = door.Position;
            var doorRect = new RectInt(pos.x, pos.y, overlap, overlap);
            AlgorithmsUtils.DebugRectInt(doorRect, Color.cyan);
        }
    }
}
