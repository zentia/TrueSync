namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    internal static class FlipcodeDecomposer
    {
        private static TSVector2 _tmpA;
        private static TSVector2 _tmpB;
        private static TSVector2 _tmpC;

        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            Debug.Assert(vertices.Count > 3);
            Debug.Assert(vertices.IsCounterClockWise());
            int[] v = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                v[i] = i;
            }
            int count = vertices.Count;
            int num2 = 2 * count;
            List<Vertices> list = new List<Vertices>();
            int index = count - 1;
            while (count > 2)
            {
                if (0 >= num2--)
                {
                    return new List<Vertices>();
                }
                int num5 = index;
                if (count <= num5)
                {
                    num5 = 0;
                }
                index = num5 + 1;
                if (count <= index)
                {
                    index = 0;
                }
                int num6 = index + 1;
                if (count <= num6)
                {
                    num6 = 0;
                }
                _tmpA = vertices[v[num5]];
                _tmpB = vertices[v[index]];
                _tmpC = vertices[v[num6]];
                if (Snip(vertices, num5, index, num6, count, v))
                {
                    Vertices item = new Vertices(3) {
                        _tmpA,
                        _tmpB,
                        _tmpC
                    };
                    list.Add(item);
                    int num7 = index;
                    for (int j = index + 1; j < count; j++)
                    {
                        v[num7] = v[j];
                        num7++;
                    }
                    count--;
                    num2 = 2 * count;
                }
            }
            return list;
        }

        private static bool InsideTriangle(ref TSVector2 a, ref TSVector2 b, ref TSVector2 c, ref TSVector2 p)
        {
            FP fp = ((c.x - b.x) * (p.y - b.y)) - ((c.y - b.y) * (p.x - b.x));
            FP fp2 = ((b.x - a.x) * (p.y - a.y)) - ((b.y - a.y) * (p.x - a.x));
            FP fp3 = ((a.x - c.x) * (p.y - c.y)) - ((a.y - c.y) * (p.x - c.x));
            return (((fp >= 0f) && (fp3 >= 0f)) && (fp2 >= 0f));
        }

        private static bool Snip(Vertices contour, int u, int v, int w, int n, int[] V)
        {
            if (Settings.Epsilon > MathUtils.Area(ref _tmpA, ref _tmpB, ref _tmpC))
            {
                return false;
            }
            for (int i = 0; i < n; i++)
            {
                if (((i != u) && (i != v)) && (i != w))
                {
                    TSVector2 p = contour[V[i]];
                    if (InsideTriangle(ref _tmpA, ref _tmpB, ref _tmpC, ref p))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

