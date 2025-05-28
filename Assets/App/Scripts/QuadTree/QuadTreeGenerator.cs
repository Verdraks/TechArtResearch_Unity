using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeGenerator : MonoBehaviour
{
    
    [SerializeField] private float minSizeNode = 1f;
    
    private QuadTree _quadTree;
    
    public void Awake()
    {
        _quadTree = new QuadTree( null, minSizeNode);
    }
}


public class QuadTree
{
    public QuadTreeNode Root;

    public QuadTree(List<Bounds> bounds, float minSize)
    {
        Root = new QuadTreeNode
        {
            Bounds = CalculateRootBounds(bounds, minSize)
        };
    }


    private Bounds CalculateRootBounds(List<Bounds> bounds, float minSize)
    {
        if (bounds == null || bounds.Count == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one * minSize);
        }

        Bounds rootBounds = new Bounds();
        
        foreach (var bound in bounds)
        {
            rootBounds.Encapsulate(bound);
        }
        
        Vector3 size = Mathf.Max(minSize, Mathf.Max(rootBounds.size.x, rootBounds.size.z) * 0.5f) * Vector3.one;
        
        rootBounds.SetMinMax(rootBounds.center - size, rootBounds.min + size);
        return rootBounds;
    }
    
}

public class QuadTreeNode
{

    public QuadTreeNode[] Children;
    public Bounds Bounds;
    
    public bool IsLeaf => Children == null;
}