namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class GJKCollide
    {
        private const int MaxIterations = 15;
        private static ResourcePool<VoronoiSimplexSolver> simplexSolverPool = new ResourcePool<VoronoiSimplexSolver>();

        public static bool ClosestPoints(ISupportMappable support1, ISupportMappable support2, ref TSMatrix orientation1, ref TSMatrix orientation2, ref TSVector position1, ref TSVector position2, out TSVector p1, out TSVector p2, out TSVector normal)
        {
            TSVector vector4;
            TSVector vector7;
            VoronoiSimplexSolver solver = simplexSolverPool.GetNew();
            solver.Reset();
            p1 = p2 = TSVector.zero;
            TSVector vector = position1 - position2;
            TSVector direction = TSVector.Negate(vector);
            SupportMapTransformed(support1, ref orientation1, ref position1, ref direction, out vector4);
            SupportMapTransformed(support2, ref orientation2, ref position2, ref vector, out vector7);
            TSVector vector3 = vector4 - vector7;
            normal = TSVector.zero;
            int num = 15;
            FP sqrMagnitude = vector3.sqrMagnitude;
            FP fp2 = FP.EN5;
            while ((sqrMagnitude > fp2) && (num-- > 0))
            {
                TSVector vector6 = TSVector.Negate(vector3);
                SupportMapTransformed(support1, ref orientation1, ref position1, ref vector6, out vector4);
                SupportMapTransformed(support2, ref orientation2, ref position2, ref vector3, out vector7);
                TSVector w = vector4 - vector7;
                if (!solver.InSimplex(w))
                {
                    solver.AddVertex(w, vector4, vector7);
                }
                if (solver.Closest(out vector3))
                {
                    sqrMagnitude = vector3.sqrMagnitude;
                    normal = vector3;
                }
                else
                {
                    sqrMagnitude = FP.Zero;
                }
            }
            solver.ComputePoints(out p1, out p2);
            if (normal.sqrMagnitude > (TSMath.Epsilon * TSMath.Epsilon))
            {
                normal.Normalize();
            }
            simplexSolverPool.GiveBack(solver);
            return true;
        }

        public static bool Pointcast(ISupportMappable support, ref TSMatrix orientation, ref TSVector position, ref TSVector point)
        {
            TSVector vector;
            TSVector vector2;
            TSVector vector6;
            SupportMapTransformed(support, ref orientation, ref position, ref point, out vector);
            TSVector.Subtract(ref point, ref vector, out vector);
            support.SupportCenter(out vector2);
            TSVector.Transform(ref vector2, ref orientation, out vector2);
            TSVector.Add(ref position, ref vector2, out vector2);
            TSVector.Subtract(ref point, ref vector2, out vector2);
            TSVector vector3 = point;
            TSVector.Subtract(ref vector3, ref vector, out vector6);
            FP sqrMagnitude = vector6.sqrMagnitude;
            FP fp3 = FP.EN4;
            int num = 15;
            VoronoiSimplexSolver solver = simplexSolverPool.GetNew();
            solver.Reset();
            while ((sqrMagnitude > fp3) && (num-- > 0))
            {
                TSVector vector4;
                TSVector vector5;
                SupportMapTransformed(support, ref orientation, ref position, ref vector6, out vector5);
                TSVector.Subtract(ref vector3, ref vector5, out vector4);
                if (TSVector.Dot(ref vector6, ref vector4) > FP.Zero)
                {
                    if (TSVector.Dot(ref vector6, ref vector2) >= -(TSMath.Epsilon * TSMath.Epsilon))
                    {
                        simplexSolverPool.GiveBack(solver);
                        return false;
                    }
                    solver.Reset();
                }
                if (!solver.InSimplex(vector4))
                {
                    solver.AddVertex(vector4, vector3, vector5);
                }
                if (solver.Closest(out vector6))
                {
                    sqrMagnitude = vector6.sqrMagnitude;
                }
                else
                {
                    sqrMagnitude = FP.Zero;
                }
            }
            simplexSolverPool.GiveBack(solver);
            return true;
        }

        public static bool Raycast(ISupportMappable support, ref TSMatrix orientation, ref TSMatrix invOrientation, ref TSVector position, ref TSVector origin, ref TSVector direction, out FP fraction, out TSVector normal)
        {
            TSVector vector5;
            TSVector vector6;
            TSVector vector7;
            TSVector vector8;
            VoronoiSimplexSolver solver = simplexSolverPool.GetNew();
            solver.Reset();
            normal = TSVector.zero;
            fraction = FP.MaxValue;
            FP zero = FP.Zero;
            TSVector vector = direction;
            TSVector vector2 = origin;
            SupportMapTransformed(support, ref orientation, ref position, ref vector, out vector6);
            TSVector.Subtract(ref vector2, ref vector6, out vector5);
            int num = 15;
            FP sqrMagnitude = vector5.sqrMagnitude;
            FP fp3 = FP.EN6;
            while ((sqrMagnitude > fp3) && (num-- > 0))
            {
                TSVector vector3;
                TSVector vector4;
                SupportMapTransformed(support, ref orientation, ref position, ref vector5, out vector4);
                TSVector.Subtract(ref vector2, ref vector4, out vector3);
                FP fp5 = TSVector.Dot(ref vector5, ref vector3);
                if (fp5 > FP.Zero)
                {
                    FP fp4 = TSVector.Dot(ref vector5, ref vector);
                    if (fp4 >= -TSMath.Epsilon)
                    {
                        simplexSolverPool.GiveBack(solver);
                        return false;
                    }
                    zero -= fp5 / fp4;
                    TSVector.Multiply(ref vector, zero, out vector2);
                    TSVector.Add(ref origin, ref vector2, out vector2);
                    TSVector.Subtract(ref vector2, ref vector4, out vector3);
                    normal = vector5;
                }
                if (!solver.InSimplex(vector3))
                {
                    solver.AddVertex(vector3, vector2, vector4);
                }
                if (solver.Closest(out vector5))
                {
                    sqrMagnitude = vector5.sqrMagnitude;
                }
                else
                {
                    sqrMagnitude = FP.Zero;
                }
            }
            solver.ComputePoints(out vector7, out vector8);
            vector8 -= origin;
            fraction = vector8.magnitude / direction.magnitude;
            if (normal.sqrMagnitude > (TSMath.Epsilon * TSMath.Epsilon))
            {
                normal.Normalize();
            }
            simplexSolverPool.GiveBack(solver);
            return true;
        }

        private static void SupportMapTransformed(ISupportMappable support, ref TSMatrix orientation, ref TSVector position, ref TSVector direction, out TSVector result)
        {
            result.x = ((direction.x * orientation.M11) + (direction.y * orientation.M12)) + (direction.z * orientation.M13);
            result.y = ((direction.x * orientation.M21) + (direction.y * orientation.M22)) + (direction.z * orientation.M23);
            result.z = ((direction.x * orientation.M31) + (direction.y * orientation.M32)) + (direction.z * orientation.M33);
            support.SupportMapping(ref result, out result);
            FP fp = ((result.x * orientation.M11) + (result.y * orientation.M21)) + (result.z * orientation.M31);
            FP fp2 = ((result.x * orientation.M12) + (result.y * orientation.M22)) + (result.z * orientation.M32);
            FP fp3 = ((result.x * orientation.M13) + (result.y * orientation.M23)) + (result.z * orientation.M33);
            result.x = position.x + fp;
            result.y = position.y + fp2;
            result.z = position.z + fp3;
        }
    }
}

