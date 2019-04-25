namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    public static class ChainHull
    {
        private static PointComparer _pointComparer = new PointComparer();

        public static Vertices GetConvexHull(Vertices vertices)
        {
            Vertices vertices3;
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            Vertices vertices2 = new Vertices(vertices);
            vertices2.Sort(_pointComparer);
            TSVector2[] vectorArray = new TSVector2[vertices2.Count];
            int index = -1;
            FP x = vertices2[0].x;
            int num2 = 1;
            while (num2 < vertices2.Count)
            {
                if (vertices2[num2].x != x)
                {
                    break;
                }
                num2++;
            }
            int num3 = num2 - 1;
            if (num3 == (vertices2.Count - 1))
            {
                vectorArray[++index] = vertices2[0];
                if (vertices2[num3].y != vertices2[0].y)
                {
                    vectorArray[++index] = vertices2[num3];
                }
                vectorArray[++index] = vertices2[0];
                vertices3 = new Vertices(index + 1);
                for (int j = 0; j < (index + 1); j++)
                {
                    vertices3.Add(vectorArray[j]);
                }
                return vertices3;
            }
            index = -1;
            int num4 = vertices2.Count - 1;
            FP fp2 = vertices2[vertices2.Count - 1].x;
            num2 = vertices2.Count - 2;
            while (num2 >= 0)
            {
                if (vertices2[num2].x != fp2)
                {
                    break;
                }
                num2--;
            }
            int num5 = num2 + 1;
            vectorArray[++index] = vertices2[0];
            num2 = num3;
            while (++num2 <= num5)
            {
                if ((MathUtils.Area(vertices2[0], vertices2[num5], vertices2[num2]) >= 0) && (num2 < num5))
                {
                    continue;
                }
                while (index > 0)
                {
                    if (MathUtils.Area(vectorArray[index - 1], vectorArray[index], vertices2[num2]) > 0)
                    {
                        break;
                    }
                    index--;
                }
                vectorArray[++index] = vertices2[num2];
            }
            if (num4 != num5)
            {
                vectorArray[++index] = vertices2[num4];
            }
            int num6 = index;
            num2 = num5;
            while (--num2 >= num3)
            {
                if ((MathUtils.Area(vertices2[num4], vertices2[num3], vertices2[num2]) >= 0) && (num2 > num3))
                {
                    continue;
                }
                while (index > num6)
                {
                    if (MathUtils.Area(vectorArray[index - 1], vectorArray[index], vertices2[num2]) > 0)
                    {
                        break;
                    }
                    index--;
                }
                vectorArray[++index] = vertices2[num2];
            }
            if (num3 > 0)
            {
                vectorArray[++index] = vertices2[0];
            }
            vertices3 = new Vertices(index + 1);
            for (int i = 0; i < (index + 1); i++)
            {
                vertices3.Add(vectorArray[i]);
            }
            return vertices3;
        }

        private class PointComparer : Comparer<TSVector2>
        {
            public override int Compare(TSVector2 a, TSVector2 b)
            {
                int num = a.x.CompareTo(b.x);
                return ((num != 0) ? num : a.y.CompareTo(b.y));
            }
        }
    }
}

