namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class MathUtils
    {
        public static TSVector2 Abs(TSVector2 v)
        {
            return new TSVector2(FP.Abs(v.x), FP.Abs(v.y));
        }

        public static FP Area(TSVector2 a, TSVector2 b, TSVector2 c)
        {
            return Area(ref a, ref b, ref c);
        }

        public static FP Area(ref TSVector2 a, ref TSVector2 b, ref TSVector2 c)
        {
            return (((a.x * (b.y - c.y)) + (b.x * (c.y - a.y))) + (c.x * (a.y - b.y)));
        }

        public static int Clamp(int a, int low, int high)
        {
            return Math.Max(low, Math.Min(a, high));
        }

        public static FP Clamp(FP a, FP low, FP high)
        {
            return TSMath.Max(low, TSMath.Min(a, high));
        }

        public static TSVector2 Clamp(TSVector2 a, TSVector2 low, TSVector2 high)
        {
            return TSVector2.Max(low, TSVector2.Min(a, high));
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));
        }

        public static FP Cross(ref TSVector2 a, ref TSVector2 b)
        {
            return ((a.x * b.y) - (a.y * b.x));
        }

        public static TSVector2 Cross(FP s, TSVector2 a)
        {
            return new TSVector2(-s * a.y, s * a.x);
        }

        public static TSVector2 Cross(TSVector2 a, FP s)
        {
            return new TSVector2(s * a.y, -s * a.x);
        }

        public static FP Cross(TSVector2 a, TSVector2 b)
        {
            return Cross(ref a, ref b);
        }

        public static void Cross(ref TSVector2 a, ref TSVector2 b, out FP c)
        {
            c = (a.x * b.y) - (a.y * b.x);
        }

        public static void Cross(FP s, ref TSVector2 a, out TSVector2 b)
        {
            b = new TSVector2(-s * a.y, s * a.x);
        }

        public static FP Dot(Vector3 a, Vector3 b)
        {
            return (((a.X * b.X) + (a.Y * b.Y)) + (a.Z * b.Z));
        }

        public static bool FPEquals(FP value1, FP value2)
        {
            return (FP.Abs(value1 - value2) <= Settings.Epsilon);
        }

        public static bool FPEquals(FP value1, FP value2, FP delta)
        {
            return FPInRange(value1, value2 - delta, value2 + delta);
        }

        public static bool FPInRange(FP value, FP min, FP max)
        {
            return ((value >= min) && (value <= max));
        }

        public static FP InvSqrt(FP x)
        {
            FPConverter converter = new FPConverter {
                x = x
            };
            FP fp = 0.5f * x;
            converter.i = 0x5f3759df - (converter.i >> 1);
            x = converter.x;
            x *= 1.5f - ((fp * x) * x);
            return x;
        }

        public static bool IsCollinear(ref TSVector2 a, ref TSVector2 b, ref TSVector2 c, FP tolerance)
        {
            return FPInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        }

        public static bool IsValid(FP x)
        {
            if (FP.IsNaN(x))
            {
                return false;
            }
            return !FP.IsInfinity(x);
        }

        public static bool IsValid(this TSVector2 x)
        {
            return (IsValid(x.x) && IsValid(x.y));
        }

        public static Vector3 Mul(Mat33 A, Vector3 v)
        {
            return (Vector3) (((v.X * A.ex) + (v.Y * A.ey)) + (v.Z * A.ez));
        }

        public static Rot Mul(Rot q, Rot r)
        {
            Rot rot;
            rot.s = (q.s * r.c) + (q.c * r.s);
            rot.c = (q.c * r.c) - (q.s * r.s);
            return rot;
        }

        public static Transform Mul(Transform A, Transform B)
        {
            return new Transform { q = Mul(A.q, B.q), p = Mul(A.q, B.p) + A.p };
        }

        public static TSVector2 Mul(ref Mat22 A, TSVector2 v)
        {
            return Mul(ref A, ref v);
        }

        public static TSVector2 Mul(ref Mat22 A, ref TSVector2 v)
        {
            return new TSVector2((A.ex.x * v.x) + (A.ey.x * v.y), (A.ex.y * v.x) + (A.ey.y * v.y));
        }

        public static TSVector2 Mul(ref Rot rot, TSVector2 axis)
        {
            return Mul((Rot) rot, axis);
        }

        public static TSVector2 Mul(ref Transform T, TSVector2 v)
        {
            return Mul(ref T, ref v);
        }

        public static TSVector2 Mul(ref Transform T, ref TSVector2 v)
        {
            FP x = ((T.q.c * v.x) - (T.q.s * v.y)) + T.p.x;
            return new TSVector2(x, ((T.q.s * v.x) + (T.q.c * v.y)) + T.p.y);
        }

        public static TSVector2 Mul(Rot q, TSVector2 v)
        {
            return new TSVector2((q.c * v.x) - (q.s * v.y), (q.s * v.x) + (q.c * v.y));
        }

        public static TSVector2 Mul22(Mat33 A, TSVector2 v)
        {
            return new TSVector2((A.ex.X * v.x) + (A.ey.X * v.y), (A.ex.Y * v.x) + (A.ey.Y * v.y));
        }

        public static Rot MulT(Rot q, Rot r)
        {
            Rot rot;
            rot.s = (q.c * r.s) - (q.s * r.c);
            rot.c = (q.c * r.c) + (q.s * r.s);
            return rot;
        }

        public static Transform MulT(Transform A, Transform B)
        {
            return new Transform { q = MulT(A.q, B.q), p = MulT(A.q, B.p - A.p) };
        }

        public static TSVector2 MulT(ref Mat22 A, TSVector2 v)
        {
            return MulT(ref A, ref v);
        }

        public static TSVector2 MulT(ref Mat22 A, ref TSVector2 v)
        {
            return new TSVector2((v.x * A.ex.x) + (v.y * A.ex.y), (v.x * A.ey.x) + (v.y * A.ey.y));
        }

        public static TSVector2 MulT(Rot q, TSVector2 v)
        {
            return new TSVector2((q.c * v.x) + (q.s * v.y), (-q.s * v.x) + (q.c * v.y));
        }

        public static TSVector2 MulT(Transform T, TSVector2 v)
        {
            FP fp = v.x - T.p.x;
            FP fp2 = v.y - T.p.y;
            FP x = (T.q.c * fp) + (T.q.s * fp2);
            return new TSVector2(x, (-T.q.s * fp) + (T.q.c * fp2));
        }

        public static TSVector2 MulT(ref Rot rot, TSVector2 axis)
        {
            return MulT((Rot) rot, axis);
        }

        public static TSVector2 MulT(ref Transform T, TSVector2 v)
        {
            return MulT(ref T, ref v);
        }

        public static TSVector2 MulT(ref Transform T, ref TSVector2 v)
        {
            FP fp = v.x - T.p.x;
            FP fp2 = v.y - T.p.y;
            FP x = (T.q.c * fp) + (T.q.s * fp2);
            return new TSVector2(x, (-T.q.s * fp) + (T.q.c * fp2));
        }

        public static void MulT(ref Mat22 A, ref Mat22 B, out Mat22 C)
        {
            C = new Mat22();
            C.ex.x = (A.ex.x * B.ex.x) + (A.ex.y * B.ex.y);
            C.ex.y = (A.ey.x * B.ex.x) + (A.ey.y * B.ex.y);
            C.ey.x = (A.ex.x * B.ey.x) + (A.ex.y * B.ey.y);
            C.ey.y = (A.ey.x * B.ey.x) + (A.ey.y * B.ey.y);
        }

        public static void MulT(ref Transform A, ref Transform B, out Transform C)
        {
            C = new Transform();
            C.q = MulT(A.q, B.q);
            C.p = MulT(A.q, B.p - A.p);
        }

        public static TSVector2 Skew(TSVector2 input)
        {
            return new TSVector2(-input.y, input.x);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T local = a;
            a = b;
            b = local;
        }

        public static FP VectorAngle(ref TSVector2 p1, ref TSVector2 p2)
        {
            FP fp = FP.Atan2(p1.y, p1.x);
            FP fp3 = FP.Atan2(p2.y, p2.x) - fp;
            while (fp3 > FP.Pi)
            {
                fp3 -= 2 * FP.Pi;
            }
            while (fp3 < -FP.Pi)
            {
                fp3 += 2 * FP.Pi;
            }
            return fp3;
        }

        public static FP VectorAngle(TSVector2 p1, TSVector2 p2)
        {
            return VectorAngle(ref p1, ref p2);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FPConverter
        {
            [FieldOffset(0)]
            public int i;
            [FieldOffset(0)]
            public FP x;
        }
    }
}

