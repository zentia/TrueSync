namespace TrueSync
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct TSVector2 : IEquatable<TSVector2>
    {
        private static TSVector2 zeroVector;
        private static TSVector2 oneVector;
        private static TSVector2 rightVector;
        private static TSVector2 leftVector;
        private static TSVector2 upVector;
        private static TSVector2 downVector;
        public FP x;
        public FP y;
        public static TSVector2 zero
        {
            get
            {
                return zeroVector;
            }
        }
        public static TSVector2 one
        {
            get
            {
                return oneVector;
            }
        }
        public static TSVector2 right
        {
            get
            {
                return rightVector;
            }
        }
        public static TSVector2 left
        {
            get
            {
                return leftVector;
            }
        }
        public static TSVector2 up
        {
            get
            {
                return upVector;
            }
        }
        public static TSVector2 down
        {
            get
            {
                return downVector;
            }
        }
        public TSVector2(FP x, FP y)
        {
            this.x = x;
            this.y = y;
        }

        public TSVector2(FP value)
        {
            this.x = value;
            this.y = value;
        }

        public void Set(FP x, FP y)
        {
            this.x = x;
            this.y = y;
        }

        public static void Reflect(ref TSVector2 vector, ref TSVector2 normal, out TSVector2 result)
        {
            FP fp = Dot(vector, normal);
            result.x = vector.x - ((2f * fp) * normal.x);
            result.y = vector.y - ((2f * fp) * normal.y);
        }

        public static TSVector2 Reflect(TSVector2 vector, TSVector2 normal)
        {
            TSVector2 vector2;
            Reflect(ref vector, ref normal, out vector2);
            return vector2;
        }

        public static TSVector2 Add(TSVector2 value1, TSVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        public static void Add(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x + value2.x;
            result.y = value1.y + value2.y;
        }

        public static TSVector2 Barycentric(TSVector2 value1, TSVector2 value2, TSVector2 value3, FP amount1, FP amount2)
        {
            return new TSVector2(MathHelper.Barycentric(value1.x, value2.x, value3.x, amount1, amount2), MathHelper.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

        public static void Barycentric(ref TSVector2 value1, ref TSVector2 value2, ref TSVector2 value3, FP amount1, FP amount2, out TSVector2 result)
        {
            result = new TSVector2(MathHelper.Barycentric(value1.x, value2.x, value3.x, amount1, amount2), MathHelper.Barycentric(value1.y, value2.y, value3.y, amount1, amount2));
        }

        public static TSVector2 CatmullRom(TSVector2 value1, TSVector2 value2, TSVector2 value3, TSVector2 value4, FP amount)
        {
            return new TSVector2(MathHelper.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount), MathHelper.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        public static void CatmullRom(ref TSVector2 value1, ref TSVector2 value2, ref TSVector2 value3, ref TSVector2 value4, FP amount, out TSVector2 result)
        {
            result = new TSVector2(MathHelper.CatmullRom(value1.x, value2.x, value3.x, value4.x, amount), MathHelper.CatmullRom(value1.y, value2.y, value3.y, value4.y, amount));
        }

        public static TSVector2 Clamp(TSVector2 value1, TSVector2 min, TSVector2 max)
        {
            return new TSVector2(MathHelper.Clamp(value1.x, min.x, max.x), MathHelper.Clamp(value1.y, min.y, max.y));
        }

        public static void Clamp(ref TSVector2 value1, ref TSVector2 min, ref TSVector2 max, out TSVector2 result)
        {
            result = new TSVector2(MathHelper.Clamp(value1.x, min.x, max.x), MathHelper.Clamp(value1.y, min.y, max.y));
        }

        public static FP Distance(TSVector2 value1, TSVector2 value2)
        {
            FP fp;
            DistanceSquared(ref value1, ref value2, out fp);
            return FP.Sqrt(fp);
        }

        public static void Distance(ref TSVector2 value1, ref TSVector2 value2, out FP result)
        {
            DistanceSquared(ref value1, ref value2, out result);
            result = FP.Sqrt(result);
        }

        public static FP DistanceSquared(TSVector2 value1, TSVector2 value2)
        {
            FP fp;
            DistanceSquared(ref value1, ref value2, out fp);
            return fp;
        }

        public static void DistanceSquared(ref TSVector2 value1, ref TSVector2 value2, out FP result)
        {
            result = ((value1.x - value2.x) * (value1.x - value2.x)) + ((value1.y - value2.y) * (value1.y - value2.y));
        }

        public static TSVector2 Divide(TSVector2 value1, TSVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        public static void Divide(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x / value2.x;
            result.y = value1.y / value2.y;
        }

        public static TSVector2 Divide(TSVector2 value1, FP divider)
        {
            FP fp = 1 / divider;
            value1.x *= fp;
            value1.y *= fp;
            return value1;
        }

        public static void Divide(ref TSVector2 value1, FP divider, out TSVector2 result)
        {
            FP fp = 1 / divider;
            result.x = value1.x * fp;
            result.y = value1.y * fp;
        }

        public static FP Dot(TSVector2 value1, TSVector2 value2)
        {
            return ((value1.x * value2.x) + (value1.y * value2.y));
        }

        public static void Dot(ref TSVector2 value1, ref TSVector2 value2, out FP result)
        {
            result = (value1.x * value2.x) + (value1.y * value2.y);
        }

        public override bool Equals(object obj)
        {
            return ((obj is TSVector2) ? (this == ((TSVector2) obj)) : false);
        }

        public bool Equals(TSVector2 other)
        {
            return (this == other);
        }

        public override int GetHashCode()
        {
            return (int) ((long) (this.x + this.y));
        }

        public static TSVector2 Hermite(TSVector2 value1, TSVector2 tangent1, TSVector2 value2, TSVector2 tangent2, FP amount)
        {
            TSVector2 result = new TSVector2();
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        public static void Hermite(ref TSVector2 value1, ref TSVector2 tangent1, ref TSVector2 value2, ref TSVector2 tangent2, FP amount, out TSVector2 result)
        {
            result.x = MathHelper.Hermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
            result.y = MathHelper.Hermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
        }

        public FP magnitude
        {
            get
            {
                FP fp;
                DistanceSquared(ref this, ref zeroVector, out fp);
                return FP.Sqrt(fp);
            }
        }
        public FP LengthSquared()
        {
            FP fp;
            DistanceSquared(ref this, ref zeroVector, out fp);
            return fp;
        }

        public static TSVector2 Lerp(TSVector2 value1, TSVector2 value2, FP amount)
        {
            amount = MathHelper.Clamp(amount, 0, 1);
            return new TSVector2(MathHelper.Lerp(value1.x, value2.x, amount), MathHelper.Lerp(value1.y, value2.y, amount));
        }

        public static TSVector2 LerpUnclamped(TSVector2 value1, TSVector2 value2, FP amount)
        {
            return new TSVector2(MathHelper.Lerp(value1.x, value2.x, amount), MathHelper.Lerp(value1.y, value2.y, amount));
        }

        public static void LerpUnclamped(ref TSVector2 value1, ref TSVector2 value2, FP amount, out TSVector2 result)
        {
            result = new TSVector2(MathHelper.Lerp(value1.x, value2.x, amount), MathHelper.Lerp(value1.y, value2.y, amount));
        }

        public static TSVector2 Max(TSVector2 value1, TSVector2 value2)
        {
            return new TSVector2(TSMath.Max(value1.x, value2.x), TSMath.Max(value1.y, value2.y));
        }

        public static void Max(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = TSMath.Max(value1.x, value2.x);
            result.y = TSMath.Max(value1.y, value2.y);
        }

        public static TSVector2 Min(TSVector2 value1, TSVector2 value2)
        {
            return new TSVector2(TSMath.Min(value1.x, value2.x), TSMath.Min(value1.y, value2.y));
        }

        public static void Min(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = TSMath.Min(value1.x, value2.x);
            result.y = TSMath.Min(value1.y, value2.y);
        }

        public void Scale(TSVector2 other)
        {
            this.x *= other.x;
            this.y *= other.y;
        }

        public static TSVector2 Scale(TSVector2 value1, TSVector2 value2)
        {
            TSVector2 vector;
            vector.x = value1.x * value2.x;
            vector.y = value1.y * value2.y;
            return vector;
        }

        public static TSVector2 Multiply(TSVector2 value1, TSVector2 value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            return value1;
        }

        public static TSVector2 Multiply(TSVector2 value1, FP scaleFactor)
        {
            value1.x *= scaleFactor;
            value1.y *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref TSVector2 value1, FP scaleFactor, out TSVector2 result)
        {
            result.x = value1.x * scaleFactor;
            result.y = value1.y * scaleFactor;
        }

        public static void Multiply(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x * value2.x;
            result.y = value1.y * value2.y;
        }

        public static TSVector2 Negate(TSVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        public static void Negate(ref TSVector2 value, out TSVector2 result)
        {
            result.x = -value.x;
            result.y = -value.y;
        }

        public void Normalize()
        {
            Normalize(ref this, out this);
        }

        public static TSVector2 Normalize(TSVector2 value)
        {
            Normalize(ref value, out value);
            return value;
        }

        public TSVector2 normalized
        {
            get
            {
                TSVector2 vector;
                Normalize(ref this, out vector);
                return vector;
            }
        }
        public static void Normalize(ref TSVector2 value, out TSVector2 result)
        {
            FP fp;
            DistanceSquared(ref value, ref zeroVector, out fp);
            fp = 1f / FP.Sqrt(fp);
            result.x = value.x * fp;
            result.y = value.y * fp;
        }

        public static TSVector2 SmoothStep(TSVector2 value1, TSVector2 value2, FP amount)
        {
            return new TSVector2(MathHelper.SmoothStep(value1.x, value2.x, amount), MathHelper.SmoothStep(value1.y, value2.y, amount));
        }

        public static void SmoothStep(ref TSVector2 value1, ref TSVector2 value2, FP amount, out TSVector2 result)
        {
            result = new TSVector2(MathHelper.SmoothStep(value1.x, value2.x, amount), MathHelper.SmoothStep(value1.y, value2.y, amount));
        }

        public static TSVector2 Subtract(TSVector2 value1, TSVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        public static void Subtract(ref TSVector2 value1, ref TSVector2 value2, out TSVector2 result)
        {
            result.x = value1.x - value2.x;
            result.y = value1.y - value2.y;
        }

        public static TSVector2 Transform(TSVector2 position, Matrix matrix)
        {
            Transform(ref position, ref matrix, out position);
            return position;
        }

        public static void Transform(ref TSVector2 position, ref Matrix matrix, out TSVector2 result)
        {
            result = new TSVector2(((position.x * matrix.M11) + (position.y * matrix.M21)) + matrix.M41, ((position.x * matrix.M12) + (position.y * matrix.M22)) + matrix.M42);
        }

        public static TSVector2 TransformNormal(TSVector2 normal, Matrix matrix)
        {
            TransformNormal(ref normal, ref matrix, out normal);
            return normal;
        }

        public static void TransformNormal(ref TSVector2 normal, ref Matrix matrix, out TSVector2 result)
        {
            result = new TSVector2((normal.x * matrix.M11) + (normal.y * matrix.M21), (normal.x * matrix.M12) + (normal.y * matrix.M22));
        }

        public static FP Angle(TSVector2 a, TSVector2 b)
        {
            return (FP.Acos((FP) (a.normalized * b.normalized)) * FP.Rad2Deg);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x18);
            builder.Append("{X:");
            builder.Append(this.x);
            builder.Append(" Y:");
            builder.Append(this.y);
            builder.Append("}");
            return builder.ToString();
        }

        public static TSVector2 operator -(TSVector2 value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        public static bool operator ==(TSVector2 value1, TSVector2 value2)
        {
            return ((value1.x == value2.x) && (value1.y == value2.y));
        }

        public static bool operator !=(TSVector2 value1, TSVector2 value2)
        {
            return ((value1.x != value2.x) || (value1.y != value2.y));
        }

        public static TSVector2 operator +(TSVector2 value1, TSVector2 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        public static TSVector2 operator -(TSVector2 value1, TSVector2 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        public static FP operator *(TSVector2 value1, TSVector2 value2)
        {
            return Dot(value1, value2);
        }

        public static TSVector2 operator *(TSVector2 value, FP scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        public static TSVector2 operator *(FP scaleFactor, TSVector2 value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        public static TSVector2 operator /(TSVector2 value1, TSVector2 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        public static TSVector2 operator /(TSVector2 value1, FP divider)
        {
            FP fp = 1 / divider;
            value1.x *= fp;
            value1.y *= fp;
            return value1;
        }

        static TSVector2()
        {
            zeroVector = new TSVector2(0, 0);
            oneVector = new TSVector2(1, 1);
            rightVector = new TSVector2(1, 0);
            leftVector = new TSVector2(-1, 0);
            upVector = new TSVector2(0, 1);
            downVector = new TSVector2(0, -1);
        }
    }
}

