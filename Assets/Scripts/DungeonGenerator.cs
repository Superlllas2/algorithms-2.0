using System.Collections.Generic;
using NUnit.Framework;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private int overlap = 1;
    [SerializeField] private int minDoorLength = 2;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private GameObject heroPrefab;
    private bool showGizmos = true;
    private GameObject heroInstance;

    public RectInt initialBounds = new RectInt(0, 0, 100, 60);
    public int minSplitSize = 20;
    public int maxDepth = 4;

    private BSPNode rootNode;
    private List<Room> allRooms = new();
    private List<Door> doors = new();

    private HashSet<Vector2Int> placedFloors = new();
    private HashSet<Vector2Int> placedWalls = new();

    private Transform floorParent;
    private Transform wallParent;
    private Transform doorParent;

    void Start()
    {
        floorParent = new GameObject("FloorTiles").transform;

        rootNode = new BSPNode { Bounds = initialBounds };
        // TODO: flowchart for these 4 methods
        Split(rootNode, maxDepth);
        CreateRooms(rootNode);
        ConnectRooms(rootNode);
        ConnectAdjacentRooms();
        // ---------
        CreateRoomWallsForAll();
        CreateOuterWalls(initialBounds);
        surface.BuildNavMesh();
        Instantiate(heroPrefab, new Vector3(5, 0.5f, 5), Quaternion.identity);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Regenerate();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            showGizmos = !showGizmos;
        }

        if (!showGizmos) return;
        DrawDebugRects(rootNode);
        DrawDebugDoors(doors);
        DrawRoomGraph();
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
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y + splitY, node.Bounds.width,
                    node.Bounds.height - splitY)
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
                Bounds = new RectInt(node.Bounds.x + splitX, node.Bounds.y, node.Bounds.width - splitX,
                    node.Bounds.height)
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

                if (overlap.width > 1) // Vertical wall
                {
                    var x = Mathf.Clamp(overlap.xMin + Random.Range(1, overlap.width - 1), overlap.xMin + 1,
                        overlap.xMax - 2);
                    var y = overlap.yMin;
                    doorPos = new Vector2Int(x, y);
                }
                else if (overlap.height > 1) // Horizontal wall
                {
                    var x = overlap.xMin;
                    var y = Mathf.Clamp(overlap.yMin + Random.Range(1, overlap.height - 1), overlap.yMin + 1,
                        overlap.yMax - 2);
                    doorPos = new Vector2Int(x, y);
                }
                else
                {
                    // If the wall is too short
                    doorPos = new Vector2Int(overlap.xMin, overlap.yMin);
                }

                // Adding door only if rooms are not already connected
                if (!roomA.ConnectedRooms.Contains(roomB))
                {
                    roomA.ConnectedRooms.Add(roomB);
                    roomB.ConnectedRooms.Add(roomA);
                    doors.Add(new Door(doorPos, roomA, roomB));
                }
            }
        }

        ConnectRooms(node.Left);
        ConnectRooms(node.Right);
        // Debug.Log("Connected rooms A: " + roomA.ConnectedRooms.Count);
        // Debug.Log("Connected rooms B: " + roomB.ConnectedRooms.Count);
    }

    void ConnectAdjacentRooms()
    {
        for (int i = 0; i < allRooms.Count; i++)
        {
            for (int j = i + 1; j < allRooms.Count; j++)
            {
                Room a = allRooms[i];
                Room b = allRooms[j];

                RectInt sharedWall = AlgorithmsUtils.Intersect(a.Bounds, b.Bounds);

                bool isVerticalWall = sharedWall.width == 1 && sharedWall.height >= minDoorLength;
                bool isHorizontalWall = sharedWall.height == 1 && sharedWall.width >= minDoorLength;

                if (isVerticalWall || isHorizontalWall)
                {
                    int centerX = sharedWall.xMin + sharedWall.width / 2;
                    int centerY = sharedWall.yMin + sharedWall.height / 2;
                    Vector2Int doorPos = new Vector2Int(centerX, centerY);

                    // Avoid duplicate connection
                    if (!a.ConnectedRooms.Contains(b))
                    {
                        a.ConnectedRooms.Add(b);
                        b.ConnectedRooms.Add(a);
                        doors.Add(new Door(doorPos, a, b));
                        // CreateDoor(doorPos);
                    }
                }
            }
        }
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

        CreateFloor(room.Bounds);
    }

    void DrawRoomGraph()
    {
        // ROOM CENTER CIRCLE
        foreach (var room in allRooms)
        {
            Vector3 roomCenter = new Vector3(room.Bounds.center.x, 0.5f, room.Bounds.center.y);
            DebugExtension.DebugCircle(roomCenter, Vector3.up, Color.green);
        }

        // DOORS AND ROOM
        foreach (var door in doors)
        {
            Vector3 doorPos = new Vector3(door.Position.x + 0.5f, 0.5f, door.Position.y + 0.5f);

            // Draw door node
            DebugExtension.DebugCircle(doorPos, Vector3.up, Color.cyan);

            // Connect door to its rooms
            foreach (var room in door.ConnectedRooms)
            {
                Vector3 roomCenter = new Vector3(room.Bounds.center.x, 0.5f, room.Bounds.center.y);
                Debug.DrawLine(roomCenter, doorPos, Color.green);
            }
        }
    }

    void CreateWall(RectInt wallRect)
    {
        Vector3 position = new Vector3(wallRect.center.x, wallHeight / 2f, wallRect.center.y);
        Vector3 scale = new Vector3(wallRect.width, wallHeight, wallRect.height);

        var wall = Instantiate(wallPrefab, position, Quaternion.identity);
        wall.transform.localScale = scale;
        wall.isStatic = true;
    }

    void CreateFloor(RectInt area)
    {
        for (int x = area.xMin; x < area.xMax; x++)
        {
            for (int y = area.yMin; y < area.yMax; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (placedFloors.Add(cell))
                {
                    Vector3 pos = new Vector3(x + 0.5f, 0f, y + 0.5f); // Y = 0 (ground level)
                    GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity, floorParent);
                    floor.transform.localScale = new Vector3(1, 0.1f, 1); // Thin tile
                    floor.isStatic = true;
                }
            }
        }
    }

    void CreateDoor(Vector2Int doorPos)
    {
        if (doorPrefab == null) return;

        Vector3 pos = new Vector3(doorPos.x + 0.5f, 0f, doorPos.y + 0.5f);
        GameObject door = Instantiate(doorPrefab, pos, Quaternion.identity, doorParent);
        door.transform.localScale = new Vector3(1, wallHeight, 1);
        door.isStatic = true;
    }

    void CreateRoomWallsForAll()
    {
        foreach (var room in allRooms)
        {
            CreateRoomWalls(room);
        }
    }
    
    void CreateRoomWalls(Room room)
    {
        if (wallParent == null)
            wallParent = new GameObject("WallTiles").transform;

        var bounds = room.Bounds;

        // 4 стороны комнаты
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            Vector2Int posTop = new Vector2Int(x, bounds.yMax - 1);
            Vector2Int posBottom = new Vector2Int(x, bounds.yMin);
            if (!IsDoorPosition(posTop)) CreateWallAt(posTop);
            if (!IsDoorPosition(posBottom)) CreateWallAt(posBottom);
        }

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            Vector2Int posLeft = new Vector2Int(bounds.xMin, y);
            Vector2Int posRight = new Vector2Int(bounds.xMax - 1, y);
            if (!IsDoorPosition(posLeft)) CreateWallAt(posLeft);
            if (!IsDoorPosition(posRight)) CreateWallAt(posRight);
        }
    }

    bool IsDoorPosition(Vector2Int pos)
    {
        foreach (var door in doors)
        {
            if (door.Position == pos)
                return true;
        }
        return false;
    }

    void CreateWallAt(Vector2Int pos)
    {
        if (placedWalls.Add(pos))
        {
            Vector3 position = new Vector3(pos.x + 0.5f, wallHeight / 2f, pos.y + 0.5f);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, wallParent);
            wall.transform.localScale = new Vector3(1, wallHeight, 1);
            wall.isStatic = true;
        }
    }

    void CreateOuterWalls(RectInt bounds)
    {
        int thickness = 1;

        // Creating the left wall
        CreateWall(new RectInt(bounds.xMin - thickness, bounds.yMin, thickness, bounds.height));

        // Creating the right wall
        CreateWall(new RectInt(bounds.xMax, bounds.yMin, thickness, bounds.height));

        // Creating the bottom wall
        CreateWall(new RectInt(bounds.xMin - thickness, bounds.yMin - thickness, bounds.width + 2 * thickness,
            thickness));

        // Creating the top wall
        CreateWall(new RectInt(bounds.xMin - thickness, bounds.yMax, bounds.width + 2 * thickness, thickness));
    }
    
    void ClearDungeon()
    {
        // Deleting floors
        if (floorParent) DestroyImmediate(floorParent.gameObject);

        // Deleting walls
        if (wallParent) DestroyImmediate(wallParent.gameObject);

        // Deleting doors
        if (doorParent) DestroyImmediate(doorParent.gameObject);

        // Deleting old hero
        var oldHero = GameObject.FindWithTag("Player");
        if (oldHero) DestroyImmediate(oldHero);

        // Clearing the lists
        allRooms.Clear();
        doors.Clear();
        placedFloors.Clear();
        placedWalls.Clear();
    }

    void Regenerate()
    {
        ClearDungeon();

        rootNode = new BSPNode { Bounds = initialBounds };
    
        // Full new generation
        Split(rootNode, maxDepth);
        CreateRooms(rootNode);
        ConnectRooms(rootNode);
        ConnectAdjacentRooms();
        CreateRoomWallsForAll();
        CreateOuterWalls(initialBounds);

        // Nav mesh new baking
        surface.BuildNavMesh();

        // New hero
        Instantiate(heroPrefab, new Vector3(5, 0.5f, 5), Quaternion.identity);
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

    public bool IsDungeonFullyConnected()
    {
        return IsGraphConnected(allRooms);
    }

    public static bool IsGraphConnected(List<Room> rooms)
    {
        if (rooms == null || rooms.Count == 0)
            return true;

        var visited = new HashSet<Room>();
        var queue = new Queue<Room>();

        visited.Add(rooms[0]);
        queue.Enqueue(rooms[0]);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in current.ConnectedRooms)
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited.Count == rooms.Count;
    }
}