using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangulation
{
    public static List<int> TriangulateConvexPolygon(List<Vector3> convexPolygon)
    {
        List<int> triangles = new List<int>();

        // Select the pivot vertex as the first vertex of the convex polygon
        Vector3 pivot = convexPolygon[0];

        // Triangulate the convex polygon using a fan triangulation
        for (int i = 1; i < convexPolygon.Count - 1; i++)
        {
            triangles.Add(0); // Pivot vertex
            triangles.Add(i + 1); // Reverse order
            triangles.Add(i);
        }

        return triangles;
    }
}

