namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct TSVector
    {
        private static FP ZeroEpsilonSq;
        internal static TSVector InternalZero;
        internal static TSVector Arbitrary;
        public FP x;
        public FP y;
        public FP z;
        public static readonly TSVector zero;
        public static readonly TSVector left;
        public static readonly TSVector right;
        public static readonly TSVector up;
        public static readonly TSVector down;
        public static readonly TSVector back;
        public static readonly TSVector forward;
        public static readonly TSVector one;
        public static readonly TSVector MinValue;
        public static readonly TSVector MaxValue;
        static TSVector()
        {
            ZeroEpsilonSq = TSMath.Epsilon;
            one = new TSVector(1, 1, 1);
            zero = new TSVector(0, 0, 0);
            left = new TSVector(-1, 0, 0);
            right = new TSVector(1, 0, 0);
            up = new TSVector(0, 1, 0);
            down = new TSVector(0, -1, 0);
            back = new TSVector(0, 0, -1);
            forward = new TSVector(0, 0, 1);
            MinValue = new TSVector(FP.MinValue);
            MaxValue = new TSVector(FP.MaxValue);
            Arbitrary = new TSVector(1, 1, 1);
            InternalZero = zero;
        }

        public static TSVector Abs(TSVector other)
        {
            return new TSVector(FP.Abs(other.x), FP.Abs(other.y), FP.Abs(other.z));
        }

        public FP sqrMagnitude
        {
            get
            {
                return (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z));
            }
        }
        public FP magnitude
        {
            get
            {
                FP x = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z);
                return FP.Sqrt(x);
            }
        }
        public TSVector normalized
        {
            get
            {
                TSVector vector = new TSVector(this.x, this.y, this.z);
                vector.Normalize();
                return vector;
            }
        }
        public TSVector(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public TSVector(FP x, FP y, FP z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Scale(TSVector other)
        {
            this.x *= other.x;
            this.y *= other.y;
            this.z *= other.z;
        }

        public void Set(FP x, FP y, FP z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public TSVector(FP xyz)
        {
            this.x = xyz;
            this.y = xyz;
            this.z = xyz;
        }

        public TSVector(FP xyz, object p, FP fP) : this(xyz)
        {
        }

        public static TSVector Lerp(TSVector from, TSVector to, FP percent)
        {
            return (from + ((to - from) * percent));
        }

        public override string ToString()
        {
            return string.Format("({0:f1}, {1:f1}, {2:f1})", this.x.AsFloat(), this.y.AsFloat(), this.z.AsFloat());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TSVector))
            {
                return false;
            }
            TSVector vector = (TSVector) obj;
            return (((this.x == vector.x) && (this.y == vector.y)) && (this.z == vector.z));
        }

        public static TSVector Scale(TSVector vecA, TSVector vecB)
        {
            TSVector vector;
            vector.x = vecA.x * vecB.x;
            vector.y = vecA.y * vecB.y;
            vector.z = vecA.z * vecB.z;
            return vector;
        }

        public static bool operator ==(TSVector value1, TSVector value2)
        {
            return (((value1.x == value2.x) && (value1.y == value2.y)) && (value1.z == value2.z));
        }

        public static bool operator !=(TSVector value1, TSVector value2)
        {
            if ((value1.x == value2.x) && (value1.y == value2.y))
            {
                return (value1.z != value2.z);
            }
            return true;
        }

        public static TSVector Min(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Min(ref value1, ref value2, out vector);
            return vector;
        }

        public static void Min(ref TSVector value1, ref TSVector value2, out TSVector result)
        {
            result.x = (value1.x < value2.x) ? value1.x : value2.x;
            result.y = (value1.y < value2.y) ? value1.y : value2.y;
            result.z = (value1.z < value2.z) ? value1.z : value2.z;
        }

        public static TSVector Max(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Max(ref value1, ref value2, out vector);
            return vector;
        }

        public static FP Distance(TSVector v1, TSVector v2)
        {
            return FP.Sqrt((((v1.x - v2.x) * (v1.x - v2.x)) + ((v1.y - v2.y) * (v1.y - v2.y))) + ((v1.z - v2.z) * (v1.z - v2.z)));
        }

        public static void Max(ref TSVector value1, ref TSVector value2, out TSVector result)
        {
            result.x = (value1.x > value2.x) ? value1.x : value2.x;
            result.y = (value1.y > value2.y) ? value1.y : value2.y;
            result.z = (value1.z > value2.z) ? value1.z : value2.z;
        }

        public void MakeZero()
        {
            this.x = FP.Zero;
            this.y = FP.Zero;
            this.z = FP.Zero;
        }

        public bool IsZero()
        {
            return (this.sqrMagnitude == FP.Zero);
        }

        public bool IsNearlyZero()
        {
            return (this.sqrMagnitude < ZeroEpsilonSq);
        }

        public static TSVector Transform(TSVector position, TSMatrix matrix)
        {
            TSVector vector;
            Transform(ref position, ref matrix, out vector);
            return vector;
        }

        public static void Transform(ref TSVector position, ref TSMatrix matrix, out TSVector result)
        {
            FP fp = ((position.x * matrix.M11) + (position.y * matrix.M21)) + (position.z * matrix.M31);
            FP fp2 = ((position.x * matrix.M12) + (position.y * matrix.M22)) + (position.z * matrix.M32);
            FP fp3 = ((position.x * matrix.M13) + (position.y * matrix.M23)) + (position.z * matrix.M33);
            result.x = fp;
            result.y = fp2;
            result.z = fp3;
        }

        public static void TransposedTransform(ref TSVector position, ref TSMatrix matrix, out TSVector result)
        {
            FP fp = ((position.x * matrix.M11) + (position.y * matrix.M12)) + (position.z * matrix.M13);
            FP fp2 = ((position.x * matrix.M21) + (position.y * matrix.M22)) + (position.z * matrix.M23);
            FP fp3 = ((position.x * matrix.M31) + (position.y * matrix.M32)) + (position.z * matrix.M33);
            result.x = fp;
            result.y = fp2;
            result.z = fp3;
        }

        public static FP Dot(TSVector vector1, TSVector vector2)
        {
            return Dot(ref vector1, ref vector2);
        }

        public static FP Dot(ref TSVector vector1, ref TSVector vector2)
        {
            return (((vector1.x * vector2.x) + (vector1.y * vector2.y)) + (vector1.z * vector2.z));
        }

        public static TSVector Add(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Add(ref value1, ref value2, out vector);
            return vector;
        }

        public static void Add(ref TSVector value1, ref TSVector value2, out TSVector result)
        {
            FP fp = value1.x + value2.x;
            FP fp2 = value1.y + value2.y;
            FP fp3 = value1.z + value2.z;
            result.x = fp;
            result.y = fp2;
            result.z = fp3;
        }

        public static TSVector Divide(TSVector value1, FP scaleFactor)
        {
            TSVector vector;
            Divide(ref value1, scaleFactor, out vector);
            return vector;
        }

        public static void Divide(ref TSVector value1, FP scaleFactor, out TSVector result)
        {
            result.x = value1.x / scaleFactor;
            result.y = value1.y / scaleFactor;
            result.z = value1.z / scaleFactor;
        }

        public static TSVector Subtract(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Subtract(ref value1, ref value2, out vector);
            return vector;
        }

        public static void Subtract(ref TSVector value1, ref TSVector value2, out TSVector result)
        {
            FP fp = value1.x - value2.x;
            FP fp2 = value1.y - value2.y;
            FP fp3 = value1.z - value2.z;
            result.x = fp;
            result.y = fp2;
            result.z = fp3;
        }

        public static TSVector Cross(TSVector vector1, TSVector vector2)
        {
            TSVector vector;
            Cross(ref vector1, ref vector2, out vector);
            return vector;
        }

        public static void Cross(ref TSVector vector1, ref TSVector vector2, out TSVector result)
        {
            FP fp = (vector1.y * vector2.z) - (vector1.z * vector2.y);
            FP fp2 = (vector1.z * vector2.x) - (vector1.x * vector2.z);
            FP fp3 = (vector1.x * vector2.y) - (vector1.y * vector2.x);
            result.x = fp;
            result.y = fp2;
            result.z = fp3;
        }

        public override int GetHashCode()
        {
            return ((this.x.GetHashCode() ^ this.y.GetHashCode()) ^ this.z.GetHashCode());
        }

        public void Negate()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
        }

        public static TSVector Negate(TSVector value)
        {
            TSVector vector;
            Negate(ref value, out vector);
            return vector;
        }

        public static void Negate(ref TSVector value, out TSVector result)
        {
            FP fp = -value.x;
            FP fp2 = -value.y;
            FP fp3 = -value.z;
            result.x = fp;
            result.y = fp2;
            result.z = fp3;
        }

        public static TSVector Normalize(TSVector value)
        {
            TSVector vector;
            Normalize(ref value, out vector);
            return vector;
        }

        public void Normalize()
        {
            FP x = ((this.x * this.x) + (this.y * this.y)) + (this.z * this.z);
            FP fp2 = FP.One / FP.Sqrt(x);
            this.x *= fp2;
            this.y *= fp2;
            this.z *= fp2;
        }

        public static void Normalize(ref TSVector value, out TSVector result)
        {
            FP x = ((value.x * value.x) + (value.y * value.y)) + (value.z * value.z);
            FP fp2 = FP.One / FP.Sqrt(x);
            result.x = value.x * fp2;
            result.y = value.y * fp2;
            result.z = value.z * fp2;
        }

        public static void Swap(ref TSVector vector1, ref TSVector vector2)
        {
            FP x = vector1.x;
            vector1.x = vector2.x;
            vector2.x = x;
            x = vector1.y;
            vector1.y = vector2.y;
            vector2.y = x;
            x = vector1.z;
            vector1.z = vector2.z;
            vector2.z = x;
        }

        public static TSVector Multiply(TSVector value1, FP scaleFactor)
        {
            TSVector vector;
            Multiply(ref value1, scaleFactor, out vector);
            return vector;
        }

        public static void Multiply(ref TSVector value1, FP scaleFactor, out TSVector result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
            result.z = value1.z * scaleFactor;
        }

        public static TSVector operator %(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Cross(ref value1, ref value2, out vector);
            return vector;
        }

        public static FP operator *(TSVector value1, TSVector value2)
        {
            return Dot(ref value1, ref value2);
        }

        public static TSVector operator *(TSVector value1, FP value2)
        {
            TSVector vector;
            Multiply(ref value1, value2, out vector);
            return vector;
        }

        public static TSVector operator *(FP value1, TSVector value2)
        {
            TSVector vector;
            Multiply(ref value2, value1, out vector);
            return vector;
        }

        public static TSVector operator -(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Subtract(ref value1, ref value2, out vector);
            return vector;
        }

        public static TSVector operator +(TSVector value1, TSVector value2)
        {
            TSVector vector;
            Add(ref value1, ref value2, out vector);
            return vector;
        }

        public static TSVector operator /(TSVector value1, FP value2)
        {
            TSVector vector;
            Divide(ref value1, value2, out vector);
            return vector;
        }

        public static FP Angle(TSVector a, TSVector b)
        {
            return (FP.Acos((FP) (a.normalized * b.normalized)) * FP.Rad2Deg);
        }
    }
}

