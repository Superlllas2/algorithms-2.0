using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public RectInt initialBounds = new RectInt(0, 0, 100, 60);
    public int minSplitSize = 20;
    public int maxDepth = 4;

    private BSPNode rootNode;

    void Start()
    {
        rootNode = new BSPNode { Bounds = initialBounds };
        Split(rootNode, maxDepth);
    }

    void Update()
    {
        DrawDebugRects(rootNode);
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
            Debug.Log("splitHorizontally");
            int splitY = Random.Range(minSplitSize, node.Bounds.height - minSplitSize);
            node.Left = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y, node.Bounds.width, splitY)
            };
            node.Right = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y + splitY, node.Bounds.width, node.Bounds.height - splitY)
            };
        }
        else
        {
            Debug.Log("splitVertically");
            int splitX = Random.Range(minSplitSize, node.Bounds.width - minSplitSize);
            node.Left = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y, splitX, node.Bounds.height)
            };
            node.Right = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x + splitX, node.Bounds.y, node.Bounds.width - splitX, node.Bounds.height)
            };
        }

        Split(node.Left, depth - 1);
        Split(node.Right, depth - 1);
    }

    void DrawDebugRects(BSPNode node)
    {
        if (node == null) return;

        AlgorithmsUtils.DebugRectInt(node.Bounds, Color.red);

        DrawDebugRects(node.Left);
        DrawDebugRects(node.Right);
    }
}
