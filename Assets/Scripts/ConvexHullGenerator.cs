using UnityEngine;
using System.Collections.Generic;

public class ConvexHullGenerator : MonoBehaviour
{
    public Mesh GenerateConvexHullMesh(Vector3[] points)
    {

        
        List<Vector3> convexHullVertices = CalculateConvexHull(points);
        List<int> convexHullTriangles = TriangulateConvexHull(convexHullVertices);

        // Create a new mesh
        Mesh mesh = new Mesh();
        mesh.vertices = convexHullVertices.ToArray();
        mesh.triangles = convexHullTriangles.ToArray();

        // Recalculate normals and other necessary properties
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Debug.Log(mesh.triangles.Length);
        return mesh;
    }

    private List<Vector3> CalculateConvexHull(Vector3[] points)
    {
        List<Vector3> convexHull = new List<Vector3>();

        // Find the point with the lowest y-coordinate
        int minPointIndex = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].y < points[minPointIndex].y)
                minPointIndex = i;
        }

        int startPointIndex = minPointIndex;
        int currentPointIndex = startPointIndex;

        // Start the gift wrapping algorithm
        do
        {
            convexHull.Add(points[currentPointIndex]);
            int nextPointIndex = (currentPointIndex + 1) % points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                if (Orientation(points[currentPointIndex], points[i], points[nextPointIndex]) == 2)
                    nextPointIndex = i;
            }
            currentPointIndex = nextPointIndex;
        } while (currentPointIndex != startPointIndex);

        return convexHull;

    }

    private int Orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        float val = (q.z - p.z) * (r.x - q.x) - (q.x - p.x) * (r.z - q.z);
        if (val == 0) return 0; // Collinear
        return (val > 0) ? 1 : 2; // Clockwise or counterclockwise
    }

    private List<int> TriangulateConvexHull(List<Vector3> convexHullVertices)
    {
        List<int> triangles = new List<int>();

        int n = convexHullVertices.Count;
        if (n < 3)
        {
            Debug.LogWarning("Convex hull has less than 3 vertices.");
            return triangles;
        }

        // Indices array to keep track of remaining vertices
        int[] indices = new int[n];
        for (int i = 0; i < n; i++)
        {
            indices[i] = i;
        }

        int i0, i1, i2;
        int remainingVertices = n;
        int safety = 0;
        while (remainingVertices > 3)
        {
            if (++safety > 1000)
            {
                Debug.LogError("Triangulation failed due to safety limit.");
                break;
            }

            bool earFound = false;
            for (int i = 0; i < remainingVertices; i++)
            {
                i0 = indices[(i - 1 + remainingVertices) % remainingVertices];
                i1 = indices[i];
                i2 = indices[(i + 1) % remainingVertices];

                Vector2 v0 = new Vector2(convexHullVertices[i0].x, convexHullVertices[i0].z);
                Vector2 v1 = new Vector2(convexHullVertices[i1].x, convexHullVertices[i1].z);
                Vector2 v2 = new Vector2(convexHullVertices[i2].x, convexHullVertices[i2].z);

                if (IsEar(i0, i1, i2, convexHullVertices))
                {
                    earFound = true;
                    triangles.Add(i0);
                    triangles.Add(i1);
                    triangles.Add(i2);

                    // Remove vertex i1
                    List<int> updatedIndices = new List<int>();
                    for (int j = 0; j < remainingVertices; j++)
                    {
                        if (indices[j] != i1)
                        {
                            updatedIndices.Add(indices[j]);
                        }
                    }
                    indices = updatedIndices.ToArray();
                    remainingVertices--;

                    break;
                }
            }

            if (!earFound)
            {
                Debug.LogError("No ear found. Triangulation failed.");
                Debug.Log("Remaining vertices: " + remainingVertices);
                foreach (int index in indices)
                {
                    Debug.Log("Index: " + index + ", Vertex: " + convexHullVertices[index]);
                }
                break;
            }
        }

        // Add the last remaining triangle
        triangles.Add(indices[0]);
        triangles.Add(indices[1]);
        triangles.Add(indices[2]);

        return triangles;
    }

    private bool IsEar(int i0, int i1, int i2, List<Vector3> vertices)
    {
        // Check if the angle is concave
        if (Cross(vertices[i0], vertices[i1], vertices[i2]) >= 0f)
        {
            return false;
        }

        // Check if any other vertex is inside the triangle
        for (int i = 0; i < vertices.Count; i++)
        {
            if (i == i0 || i == i1 || i == i2)
            {
                continue;
            }

            Vector2 v0 = new Vector2(vertices[i0].x, vertices[i0].z);
            Vector2 v1 = new Vector2(vertices[i1].x, vertices[i1].z);
            Vector2 v2 = new Vector2(vertices[i2].x, vertices[i2].z);
            Vector2 vi = new Vector2(vertices[i].x, vertices[i].z);

            if (PointInTriangle(v0, v1, v2, vi))
            {
                return false;
            }
        }

        return true;
    }

    private float Cross(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p2.x - p1.x) * (p3.z - p1.z) - (p2.z - p1.z) * (p3.x - p1.x);
    }

    private bool PointInTriangle(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 p)
    {
        bool b1, b2, b3;

        b1 = Cross(p, v0, v1) < 0.0f;
        b2 = Cross(p, v1, v2) < 0.0f;
        b3 = Cross(p, v2, v0) < 0.0f;

        return ((b1 == b2) && (b2 == b3));
    }

    private float Cross(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
    }
}
