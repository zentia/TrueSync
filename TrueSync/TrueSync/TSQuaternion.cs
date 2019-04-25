namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct TSQuaternion
    {
        public FP x;
        public FP y;
        public FP z;
        public FP w;
        public static readonly TSQuaternion identity;
        static TSQuaternion()
        {
            identity = new TSQuaternion(0, 0, 0, 1);
        }

        public TSQuaternion(FP x, FP y, FP z, FP w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public void Set(FP new_x, FP new_y, FP new_z, FP new_w)
        {
            this.x = new_x;
            this.y = new_y;
            this.z = new_z;
            this.w = new_w;
        }

        public void SetFromToRotation(TSVector fromDirection, TSVector toDirection)
        {
            TSQuaternion quaternion = FromToRotation(fromDirection, toDirection);
            this.Set(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public TSVector eulerAngles
        {
            get
            {
                TSVector vector = new TSVector();
                FP fp = this.y * this.y;
                FP x = (-2f * (fp + (this.z * this.z))) + 1f;
                FP y = 2f * ((this.x * this.y) - (this.w * this.z));
                FP fp4 = -2f * ((this.x * this.z) + (this.w * this.y));
                FP fp5 = 2f * ((this.y * this.z) - (this.w * this.x));
                FP fp6 = (-2f * ((this.x * this.x) + fp)) + 1f;
                fp4 = (fp4 > 1f) ? 1f : fp4;
                fp4 = (fp4 < -1f) ? -1f : fp4;
                vector.x = FP.Atan2(fp5, fp6) * FP.Rad2Deg;
                vector.y = FP.Asin(fp4) * FP.Rad2Deg;
                vector.z = FP.Atan2(y, x) * FP.Rad2Deg;
                return (vector * -1);
            }
        }
        public static FP Angle(TSQuaternion a, TSQuaternion b)
        {
            TSQuaternion quaternion = Inverse(a);
            TSQuaternion quaternion2 = b * quaternion;
            FP fp = (FP.Acos(quaternion2.w) * 2) * FP.Rad2Deg;
            if (fp > 180)
            {
                fp = 360 - fp;
            }
            return fp;
        }

        public static TSQuaternion Add(TSQuaternion quaternion1, TSQuaternion quaternion2)
        {
            TSQuaternion quaternion;
            Add(ref quaternion1, ref quaternion2, out quaternion);
            return quaternion;
        }

        public static TSQuaternion Euler(FP x, FP y, FP z)
        {
            TSQuaternion quaternion;
            x *= FP.Deg2Rad;
            y *= FP.Deg2Rad;
            z *= FP.Deg2Rad;
            CreateFromYawPitchRoll(y, x, z, out quaternion);
            return quaternion;
        }

        public static TSQuaternion Euler(TSVector eulerAngles)
        {
            return Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        }

        public static TSQuaternion AngleAxis(FP angle, TSVector axis)
        {
            TSQuaternion quaternion;
            axis *= FP.Deg2Rad;
            axis.Normalize();
            FP x = (angle * FP.Deg2Rad) * FP.Half;
            FP fp2 = FP.Sin(x);
            quaternion.x = axis.x * fp2;
            quaternion.y = axis.y * fp2;
            quaternion.z = axis.z * fp2;
            quaternion.w = FP.Cos(x);
            return quaternion;
        }

        public static void CreateFromYawPitchRoll(FP yaw, FP pitch, FP roll, out TSQuaternion result)
        {
            FP x = roll * FP.Half;
            FP fp2 = FP.Sin(x);
            FP fp3 = FP.Cos(x);
            FP fp4 = pitch * FP.Half;
            FP fp5 = FP.Sin(fp4);
            FP fp6 = FP.Cos(fp4);
            FP fp7 = yaw * FP.Half;
            FP fp8 = FP.Sin(fp7);
            FP fp9 = FP.Cos(fp7);
            result.x = ((fp9 * fp5) * fp3) + ((fp8 * fp6) * fp2);
            result.y = ((fp8 * fp6) * fp3) - ((fp9 * fp5) * fp2);
            result.z = ((fp9 * fp6) * fp2) - ((fp8 * fp5) * fp3);
            result.w = ((fp9 * fp6) * fp3) + ((fp8 * fp5) * fp2);
        }

        public static void Add(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
        {
            result.x = quaternion1.x + quaternion2.x;
            result.y = quaternion1.y + quaternion2.y;
            result.z = quaternion1.z + quaternion2.z;
            result.w = quaternion1.w + quaternion2.w;
        }

        public static TSQuaternion Conjugate(TSQuaternion value)
        {
            TSQuaternion quaternion;
            quaternion.x = -value.x;
            quaternion.y = -value.y;
            quaternion.z = -value.z;
            quaternion.w = value.w;
            return quaternion;
        }

        public static FP Dot(TSQuaternion a, TSQuaternion b)
        {
            return ((((a.w * b.w) + (a.x * b.x)) + (a.y * b.y)) + (a.z * b.z));
        }

        public static TSQuaternion Inverse(TSQuaternion rotation)
        {
            FP scaleFactor = FP.One / ((((rotation.x * rotation.x) + (rotation.y * rotation.y)) + (rotation.z * rotation.z)) + (rotation.w * rotation.w));
            return Multiply(Conjugate(rotation), scaleFactor);
        }

        public static TSQuaternion FromToRotation(TSVector fromVector, TSVector toVector)
        {
            TSQuaternion quaternion;
            TSVector vector = TSVector.Cross(fromVector, toVector);
            quaternion = new TSQuaternion(vector.x, vector.y, vector.z, TSVector.Dot(fromVector, toVector)) {
                w = quaternion.w + FP.Sqrt(fromVector.sqrMagnitude * toVector.sqrMagnitude)
            };
            quaternion.Normalize();
            return quaternion;
        }

        public static TSQuaternion Lerp(TSQuaternion a, TSQuaternion b, FP t)
        {
            t = TSMath.Clamp(t, FP.Zero, FP.One);
            return LerpUnclamped(a, b, t);
        }

        public static TSQuaternion LerpUnclamped(TSQuaternion a, TSQuaternion b, FP t)
        {
            TSQuaternion quaternion = Multiply(a, 1 - t) + Multiply(b, t);
            quaternion.Normalize();
            return quaternion;
        }

        public static TSQuaternion Subtract(TSQuaternion quaternion1, TSQuaternion quaternion2)
        {
            TSQuaternion quaternion;
            Subtract(ref quaternion1, ref quaternion2, out quaternion);
            return quaternion;
        }

        public static void Subtract(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
        {
            result.x = quaternion1.x - quaternion2.x;
            result.y = quaternion1.y - quaternion2.y;
            result.z = quaternion1.z - quaternion2.z;
            result.w = quaternion1.w - quaternion2.w;
        }

        public static TSQuaternion Multiply(TSQuaternion quaternion1, TSQuaternion quaternion2)
        {
            TSQuaternion quaternion;
            Multiply(ref quaternion1, ref quaternion2, out quaternion);
            return quaternion;
        }

        public static void Multiply(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
        {
            FP x = quaternion1.x;
            FP y = quaternion1.y;
            FP z = quaternion1.z;
            FP w = quaternion1.w;
            FP fp5 = quaternion2.x;
            FP fp6 = quaternion2.y;
            FP fp7 = quaternion2.z;
            FP fp8 = quaternion2.w;
            FP fp9 = (y * fp7) - (z * fp6);
            FP fp10 = (z * fp5) - (x * fp7);
            FP fp11 = (x * fp6) - (y * fp5);
            FP fp12 = ((x * fp5) + (y * fp6)) + (z * fp7);
            result.x = ((x * fp8) + (fp5 * w)) + fp9;
            result.y = ((y * fp8) + (fp6 * w)) + fp10;
            result.z = ((z * fp8) + (fp7 * w)) + fp11;
            result.w = (w * fp8) - fp12;
        }

        public static TSQuaternion Multiply(TSQuaternion quaternion1, FP scaleFactor)
        {
            TSQuaternion quaternion;
            Multiply(ref quaternion1, scaleFactor, out quaternion);
            return quaternion;
        }

        public static void Multiply(ref TSQuaternion quaternion1, FP scaleFactor, out TSQuaternion result)
        {
            result.x = quaternion1.x * scaleFactor;
            result.y = quaternion1.y * scaleFactor;
            result.z = quaternion1.z * scaleFactor;
            result.w = quaternion1.w * scaleFactor;
        }

        public void Normalize()
        {
            FP x = (((this.x * this.x) + (this.y * this.y)) + (this.z * this.z)) + (this.w * this.w);
            FP fp2 = 1 / FP.Sqrt(x);
            this.x *= fp2;
            this.y *= fp2;
            this.z *= fp2;
            this.w *= fp2;
        }

        public static TSQuaternion CreateFromMatrix(TSMatrix matrix)
        {
            TSQuaternion quaternion;
            CreateFromMatrix(ref matrix, out quaternion);
            return quaternion;
        }

        public static void CreateFromMatrix(ref TSMatrix matrix, out TSQuaternion result)
        {
            FP fp = (matrix.M11 + matrix.M22) + matrix.M33;
            if (fp > FP.Zero)
            {
                FP fp2 = FP.Sqrt(fp + FP.One);
                result.w = fp2 * FP.Half;
                fp2 = FP.Half / fp2;
                result.x = (matrix.M23 - matrix.M32) * fp2;
                result.y = (matrix.M31 - matrix.M13) * fp2;
                result.z = (matrix.M12 - matrix.M21) * fp2;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                FP fp3 = FP.Sqrt(((FP.One + matrix.M11) - matrix.M22) - matrix.M33);
                FP fp4 = FP.Half / fp3;
                result.x = FP.Half * fp3;
                result.y = (matrix.M12 + matrix.M21) * fp4;
                result.z = (matrix.M13 + matrix.M31) * fp4;
                result.w = (matrix.M23 - matrix.M32) * fp4;
            }
            else if (matrix.M22 > matrix.M33)
            {
                FP fp5 = FP.Sqrt(((FP.One + matrix.M22) - matrix.M11) - matrix.M33);
                FP fp6 = FP.Half / fp5;
                result.x = (matrix.M21 + matrix.M12) * fp6;
                result.y = FP.Half * fp5;
                result.z = (matrix.M32 + matrix.M23) * fp6;
                result.w = (matrix.M31 - matrix.M13) * fp6;
            }
            else
            {
                FP fp7 = FP.Sqrt(((FP.One + matrix.M33) - matrix.M11) - matrix.M22);
                FP fp8 = FP.Half / fp7;
                result.x = (matrix.M31 + matrix.M13) * fp8;
                result.y = (matrix.M32 + matrix.M23) * fp8;
                result.z = FP.Half * fp7;
                result.w = (matrix.M12 - matrix.M21) * fp8;
            }
        }

        public static TSQuaternion operator *(TSQuaternion value1, TSQuaternion value2)
        {
            TSQuaternion quaternion;
            Multiply(ref value1, ref value2, out quaternion);
            return quaternion;
        }

        public static TSQuaternion operator +(TSQuaternion value1, TSQuaternion value2)
        {
            TSQuaternion quaternion;
            Add(ref value1, ref value2, out quaternion);
            return quaternion;
        }

        public static TSQuaternion operator -(TSQuaternion value1, TSQuaternion value2)
        {
            TSQuaternion quaternion;
            Subtract(ref value1, ref value2, out quaternion);
            return quaternion;
        }

        public static TSVector operator *(TSQuaternion quat, TSVector vec)
        {
            TSVector vector;
            FP fp = quat.x * 2f;
            FP fp2 = quat.y * 2f;
            FP fp3 = quat.z * 2f;
            FP fp4 = quat.x * fp;
            FP fp5 = quat.y * fp2;
            FP fp6 = quat.z * fp3;
            FP fp7 = quat.x * fp2;
            FP fp8 = quat.x * fp3;
            FP fp9 = quat.y * fp3;
            FP fp10 = quat.w * fp;
            FP fp11 = quat.w * fp2;
            FP fp12 = quat.w * fp3;
            vector.x = (((1f - (fp5 + fp6)) * vec.x) + ((fp7 - fp12) * vec.y)) + ((fp8 + fp11) * vec.z);
            vector.y = (((fp7 + fp12) * vec.x) + ((1f - (fp4 + fp6)) * vec.y)) + ((fp9 - fp10) * vec.z);
            vector.z = (((fp8 - fp11) * vec.x) + ((fp9 + fp10) * vec.y)) + ((1f - (fp4 + fp5)) * vec.z);
            return vector;
        }

        public override string ToString()
        {
            object[] args = new object[] { this.x.AsFloat(), this.y.AsFloat(), this.z.AsFloat(), this.w.AsFloat() };
            return string.Format("({0:f1}, {1:f1}, {2:f1}, {3:f1})", args);
        }
    }
}

