namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class Distance
    {
        [ThreadStatic]
        public static int GJKCalls;
        [ThreadStatic]
        public static int GJKIters;
        [ThreadStatic]
        public static int GJKMaxIters;

        public static void ComputeDistance(out DistanceOutput output, out SimplexCache cache, DistanceInput input)
        {
            cache = new SimplexCache();
            Simplex simplex = new Simplex();
            simplex.ReadCache(ref cache, input.ProxyA, ref input.TransformA, input.ProxyB, ref input.TransformB);
            FixedArray3<int> array = new FixedArray3<int>();
            FixedArray3<int> array2 = new FixedArray3<int>();
            int num = 0;
            while (num < 20)
            {
                int count = simplex.Count;
                for (int i = 0; i < count; i++)
                {
                    array[i] = simplex.V[i].IndexA;
                    array2[i] = simplex.V[i].IndexB;
                }
                switch (simplex.Count)
                {
                    case 1:
                        break;

                    case 2:
                        simplex.Solve2();
                        break;

                    case 3:
                        simplex.Solve3();
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
                if (simplex.Count == 3)
                {
                    break;
                }
                TSVector2 searchDirection = simplex.GetSearchDirection();
                if (searchDirection.LengthSquared() < Settings.EpsilonSqr)
                {
                    break;
                }
                SimplexVertex vertex = simplex.V[simplex.Count];
                vertex.IndexA = input.ProxyA.GetSupport(MathUtils.MulT(input.TransformA.q, -searchDirection));
                vertex.WA = MathUtils.Mul(ref input.TransformA, input.ProxyA.Vertices[vertex.IndexA]);
                vertex.IndexB = input.ProxyB.GetSupport(MathUtils.MulT(input.TransformB.q, searchDirection));
                vertex.WB = MathUtils.Mul(ref input.TransformB, input.ProxyB.Vertices[vertex.IndexB]);
                vertex.W = vertex.WB - vertex.WA;
                simplex.V[simplex.Count] = vertex;
                num++;
                bool flag2 = false;
                for (int j = 0; j < count; j++)
                {
                    if ((vertex.IndexA == array[j]) && (vertex.IndexB == array2[j]))
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (flag2)
                {
                    break;
                }
                simplex.Count++;
            }
            simplex.GetWitnessPoints(out output.PointA, out output.PointB);
            TSVector2 vector2 = output.PointA - output.PointB;
            output.Distance = vector2.magnitude;
            output.Iterations = num;
            simplex.WriteCache(ref cache);
            if (input.UseRadii)
            {
                FP radius = input.ProxyA.Radius;
                FP fp2 = input.ProxyB.Radius;
                if ((output.Distance > (radius + fp2)) && (output.Distance > Settings.Epsilon))
                {
                    output.Distance -= radius + fp2;
                    TSVector2 vector3 = output.PointB - output.PointA;
                    vector3.Normalize();
                    output.PointA += radius * vector3;
                    output.PointB -= fp2 * vector3;
                }
                else
                {
                    TSVector2 vector4 = (TSVector2) (0.5f * (output.PointA + output.PointB));
                    output.PointA = vector4;
                    output.PointB = vector4;
                    output.Distance = 0f;
                }
            }
        }
    }
}

