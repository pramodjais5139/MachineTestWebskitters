using System.Collections.Generic;
using UnityEngine;

public class Triangulator3D
{
    private List<Vector3> m_points = new List<Vector3>();

    public Triangulator3D(Vector3[] points)
    {
        m_points = new List<Vector3>(points);
    }

    public int[] Triangulate()
    {
        List<int> indices = new List<int>();

        int n = m_points.Count;
        if (n < 3)
            return indices.ToArray();

        int[] V = new int[n];
        if (Volume() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                return indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        indices.Reverse();
        return indices.ToArray();
    }

    private float Volume()
    {
        int n = m_points.Count;
        float volume = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector3 pval = m_points[p];
            Vector3 qval = m_points[q];
            volume += (pval.x * qval.y * qval.z) - (qval.x * pval.y * qval.z) + (qval.x * pval.y * pval.z) - (pval.x * qval.y * pval.z);
        }
        return volume / 6.0f;
    }

    private bool Snip(int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector3 A = m_points[V[u]];
        Vector3 B = m_points[V[v]];
        Vector3 C = m_points[V[w]];
        if (Mathf.Epsilon > (((B.x - A.x) * ((C.y - A.y) * (C.z - A.z))) - ((B.y - A.y) * ((C.x - A.x) * (C.z - A.z))) + ((B.z - A.z) * ((C.x - A.x) * (C.y - A.y)))))
            return false;
        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;
            Vector3 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }
        return true;
    }

    private bool InsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        float ax, ay, az, bx, by, bz, cx, cy, cz, apx, apy, apz, bpx, bpy, bpz, cpx, cpy, cpz;
        float cCROSSapx, cCROSSapy, cCROSSapz, bCROSScpx, bCROSScpy, bCROSScpz, aCROSSbpx, aCROSSbpy, aCROSSbpz;

        ax = C.x - B.x; ay = C.y - B.y; az = C.z - B.z;
        bx = A.x - C.x; by = A.y - C.y; bz = A.z - C.z;
        cx = B.x - A.x; cy = B.y - A.y; cz = B.z - A.z;
        apx = P.x - A.x; apy = P.y - A.y; apz = P.z - A.z;
        bpx = P.x - B.x; bpy = P.y - B.y; bpz = P.z - B.z;
        cpx = P.x - C.x; cpy = P.y - C.y; cpz = P.z - C.z;

        aCROSSbpx = ay * bpz - az * bpy;
        aCROSSbpy = az * bpx - ax * bpz;
        aCROSSbpz = ax * bpy - ay * bpx;
        cCROSSapx = cy * apz - cz * apy;
        cCROSSapy = cz * apx - cx * apz;
        cCROSSapz = cx * apy - cy * apx;
        bCROSScpx = by * cpz - bz * cpy;
        bCROSScpy = bz * cpx - bx * cpz;
        bCROSScpz = bx * cpy - by * cpx;

        return ((aCROSSbpx * cCROSSapx + aCROSSbpy * cCROSSapy + aCROSSbpz * cCROSSapz) >= 0.0f &&
                (bCROSScpx * aCROSSbpx + bCROSScpy * aCROSSbpy + bCROSScpz * aCROSSbpz) >= 0.0f &&
                (cCROSSapx * bCROSScpx + cCROSSapy * bCROSScpy + cCROSSapz * bCROSScpz) >= 0.0f);
    }
}
