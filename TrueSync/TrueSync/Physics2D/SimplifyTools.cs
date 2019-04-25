namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    public static class SimplifyTools
    {
        public static Vertices CollinearSimplify(Vertices vertices, FP collinearityTolerance)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            Vertices vertices2 = new Vertices(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                TSVector2 a = vertices.PreviousVertex(i);
                TSVector2 b = vertices[i];
                TSVector2 c = vertices.NextVertex(i);
                if (!MathUtils.IsCollinear(ref a, ref b, ref c, collinearityTolerance))
                {
                    vertices2.Add(b);
                }
            }
            return vertices2;
        }

        public static Vertices DouglasPeuckerSimplify(Vertices vertices, FP distanceTolerance)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            bool[] usePoint = new bool[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                usePoint[i] = true;
            }
            SimplifySection(vertices, 0, vertices.Count - 1, usePoint, distanceTolerance);
            Vertices vertices2 = new Vertices(vertices.Count);
            for (int j = 0; j < vertices.Count; j++)
            {
                if (usePoint[j])
                {
                    vertices2.Add(vertices[j]);
                }
            }
            return vertices2;
        }

        public static Vertices MergeIdenticalPoints(Vertices vertices)
        {
            HashSet<TSVector2> set = new HashSet<TSVector2>();
            foreach (TSVector2 vector in vertices)
            {
                set.Add(vector);
            }
            return new Vertices(set);
        }

        public static Vertices MergeParallelEdges(Vertices vertices, FP tolerance)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            bool[] flagArray = new bool[vertices.Count];
            int count = vertices.Count;
            for (int i = 0; i < vertices.Count; i++)
            {
                int num4 = (i == 0) ? (vertices.Count - 1) : (i - 1);
                int num5 = i;
                int num6 = (i == (vertices.Count - 1)) ? 0 : (i + 1);
                FP fp = vertices[num5].x - vertices[num4].x;
                FP fp2 = vertices[num5].y - vertices[num4].y;
                FP fp3 = vertices[num6].y - vertices[num5].x;
                FP fp4 = vertices[num6].y - vertices[num5].y;
                FP fp5 = FP.Sqrt((fp * fp) + (fp2 * fp2));
                FP fp6 = FP.Sqrt((fp3 * fp3) + (fp4 * fp4));
                if (((fp5 <= 0f) || (fp6 <= 0f)) && (count > 3))
                {
                    flagArray[i] = true;
                    count--;
                }
                fp /= fp5;
                fp2 /= fp5;
                fp3 /= fp6;
                fp4 /= fp6;
                FP fp7 = (fp * fp4) - (fp3 * fp2);
                FP fp8 = (fp * fp3) + (fp2 * fp4);
                if (((FP.Abs(fp7) < tolerance) && (fp8 > 0)) && (count > 3))
                {
                    flagArray[i] = true;
                    count--;
                }
                else
                {
                    flagArray[i] = false;
                }
            }
            if ((count == vertices.Count) || (count == 0))
            {
                return vertices;
            }
            int num2 = 0;
            Vertices vertices2 = new Vertices(count);
            for (int j = 0; j < vertices.Count; j++)
            {
                if ((!flagArray[j] && (count != 0)) && (num2 != count))
                {
                    Debug.Assert(num2 < count);
                    vertices2.Add(vertices[j]);
                    num2++;
                }
            }
            return vertices2;
        }

        public static Vertices ReduceByArea(Vertices vertices, FP areaTolerance)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            if (areaTolerance < 0)
            {
                throw new ArgumentOutOfRangeException("areaTolerance", "must be equal to or greater than zero.");
            }
            Vertices vertices2 = new Vertices(vertices.Count);
            TSVector2 a = vertices[vertices.Count - 2];
            TSVector2 b = vertices[vertices.Count - 1];
            areaTolerance *= 2;
            int num = 0;
            while (num < vertices.Count)
            {
                FP fp;
                FP fp2;
                FP fp3;
                TSVector2 vector = (num == (vertices.Count - 1)) ? vertices2[0] : vertices[num];
                MathUtils.Cross(ref a, ref b, out fp);
                MathUtils.Cross(ref b, ref vector, out fp2);
                MathUtils.Cross(ref a, ref vector, out fp3);
                if (FP.Abs(fp3 - (fp + fp2)) > areaTolerance)
                {
                    vertices2.Add(b);
                    a = b;
                }
                num++;
                b = vector;
            }
            return vertices2;
        }

        public static Vertices ReduceByDistance(Vertices vertices, FP distance)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            FP fp = distance * distance;
            Vertices vertices2 = new Vertices(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                TSVector2 item = vertices[i];
                TSVector2 vector3 = vertices.NextVertex(i) - item;
                if (vector3.LengthSquared() > fp)
                {
                    vertices2.Add(item);
                }
            }
            return vertices2;
        }

        public static Vertices ReduceByNth(Vertices vertices, int nth)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            if (nth == 0)
            {
                return vertices;
            }
            Vertices vertices2 = new Vertices(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                if ((i % nth) != 0)
                {
                    vertices2.Add(vertices[i]);
                }
            }
            return vertices2;
        }

        private static void SimplifySection(Vertices vertices, int i, int j, bool[] usePoint, FP distanceTolerance)
        {
            if ((i + 1) != j)
            {
                TSVector2 start = vertices[i];
                TSVector2 end = vertices[j];
                FP fp = -1.0;
                int num = i;
                for (int k = i + 1; k < j; k++)
                {
                    TSVector2 point = vertices[k];
                    FP fp2 = LineTools.DistanceBetweenPointAndLineSegment(ref point, ref start, ref end);
                    if (fp2 > fp)
                    {
                        fp = fp2;
                        num = k;
                    }
                }
                if (fp <= distanceTolerance)
                {
                    for (int m = i + 1; m < j; m++)
                    {
                        usePoint[m] = false;
                    }
                }
                else
                {
                    SimplifySection(vertices, i, num, usePoint, distanceTolerance);
                    SimplifySection(vertices, num, j, usePoint, distanceTolerance);
                }
            }
        }
    }
}

