using System.Collections.Generic;
using UnityEngine;

public class ConvexHull
{
    private static Vector3 Cross(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Vector3.Cross(p1 - p0, p2 - p0);
    }

    // Sort points by polar angle with respect to the lowest point
    private static List<Vector3> SortByPolarAngle(List<Vector3> points)
    {
        Vector3 lowestPoint = FindLowestPoint(points);
        points.Sort((p1, p2) =>
        {
            if (p1 == lowestPoint) return -1; // Lowest point comes first
            if (p2 == lowestPoint) return 1; // Lowest point comes first

            Vector3 dir1 = (p1 - lowestPoint).normalized;
            Vector3 dir2 = (p2 - lowestPoint).normalized;

            // Compute azimuth angles
            float angle1 = Mathf.Atan2(dir1.z, dir1.x);
            float angle2 = Mathf.Atan2(dir2.z, dir2.x);

            // Compute elevation angles
            float elevation1 = Mathf.Asin(dir1.y);
            float elevation2 = Mathf.Asin(dir2.y);

            if (angle1 < angle2) return -1;
            if (angle1 > angle2) return 1;
            return elevation1.CompareTo(elevation2);
        });
        return points;
    }

    private static Vector3 FindLowestPoint(List<Vector3> points)
    {
        Vector3 lowestPoint = points[0];
        foreach (Vector3 point in points)
        {
            if (point.y < lowestPoint.y || (point.y == lowestPoint.y && point.z < lowestPoint.z))
            {
                lowestPoint = point;
            }
        }
        return lowestPoint;
    }

    // Compute the convex hull using Graham Scan algorithm
    public static List<Vector3> ComputeConvexHull(List<Vector3> points)
    {
        if (points.Count < 3)
            return points;

        List<Vector3> sortedPoints = SortByPolarAngle(points);
        List<Vector3> convexHull = new List<Vector3>();

        convexHull.Add(sortedPoints[0]);
        convexHull.Add(sortedPoints[1]);

        for (int i = 2; i < sortedPoints.Count; i++)
        {
            while (convexHull.Count >= 2 && Cross(convexHull[convexHull.Count - 2], convexHull[convexHull.Count - 1], sortedPoints[i]).y <= 0)
            {
                convexHull.RemoveAt(convexHull.Count - 1);
            }
            convexHull.Add(sortedPoints[i]);
        }

        return convexHull;
    }

}