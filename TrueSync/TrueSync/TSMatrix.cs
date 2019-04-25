namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TSMatrix
    {
        public FP M11;
        public FP M12;
        public FP M13;
        public FP M21;
        public FP M22;
        public FP M23;
        public FP M31;
        public FP M32;
        public FP M33;
        internal static TSMatrix InternalIdentity;
        public static readonly TSMatrix Identity;
        public static readonly TSMatrix Zero;
        static TSMatrix()
        {
            Zero = new TSMatrix();
            Identity = new TSMatrix();
            Identity.M11 = FP.One;
            Identity.M22 = FP.One;
            Identity.M33 = FP.One;
            InternalIdentity = Identity;
        }

        public TSVector eulerAngles
        {
            get
            {
                TSVector vector = new TSVector {
                    x = TSMath.Atan2(this.M32, this.M33) * FP.Rad2Deg,
                    y = TSMath.Atan2(-this.M31, TSMath.Sqrt((this.M32 * this.M32) + (this.M33 * this.M33))) * FP.Rad2Deg,
                    z = TSMath.Atan2(this.M21, this.M11) * FP.Rad2Deg
                };
                return (vector * -1);
            }
        }
        public static TSMatrix CreateFromYawPitchRoll(FP yaw, FP pitch, FP roll)
        {
            TSMatrix matrix;
            TSQuaternion quaternion;
            TSQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
            CreateFromQuaternion(ref quaternion, out matrix);
            return matrix;
        }

        public static TSMatrix CreateRotationX(FP radians)
        {
            TSMatrix matrix;
            FP fp = FP.Cos(radians);
            FP fp2 = FP.Sin(radians);
            matrix.M11 = FP.One;
            matrix.M12 = FP.Zero;
            matrix.M13 = FP.Zero;
            matrix.M21 = FP.Zero;
            matrix.M22 = fp;
            matrix.M23 = fp2;
            matrix.M31 = FP.Zero;
            matrix.M32 = -fp2;
            matrix.M33 = fp;
            return matrix;
        }

        public static void CreateRotationX(FP radians, out TSMatrix result)
        {
            FP fp = FP.Cos(radians);
            FP fp2 = FP.Sin(radians);
            result.M11 = FP.One;
            result.M12 = FP.Zero;
            result.M13 = FP.Zero;
            result.M21 = FP.Zero;
            result.M22 = fp;
            result.M23 = fp2;
            result.M31 = FP.Zero;
            result.M32 = -fp2;
            result.M33 = fp;
        }

        public static TSMatrix CreateRotationY(FP radians)
        {
            TSMatrix matrix;
            FP fp = FP.Cos(radians);
            FP fp2 = FP.Sin(radians);
            matrix.M11 = fp;
            matrix.M12 = FP.Zero;
            matrix.M13 = -fp2;
            matrix.M21 = FP.Zero;
            matrix.M22 = FP.One;
            matrix.M23 = FP.Zero;
            matrix.M31 = fp2;
            matrix.M32 = FP.Zero;
            matrix.M33 = fp;
            return matrix;
        }

        public static void CreateRotationY(FP radians, out TSMatrix result)
        {
            FP fp = FP.Cos(radians);
            FP fp2 = FP.Sin(radians);
            result.M11 = fp;
            result.M12 = FP.Zero;
            result.M13 = -fp2;
            result.M21 = FP.Zero;
            result.M22 = FP.One;
            result.M23 = FP.Zero;
            result.M31 = fp2;
            result.M32 = FP.Zero;
            result.M33 = fp;
        }

        public static TSMatrix CreateRotationZ(FP radians)
        {
            TSMatrix matrix;
            FP fp = FP.Cos(radians);
            FP fp2 = FP.Sin(radians);
            matrix.M11 = fp;
            matrix.M12 = fp2;
            matrix.M13 = FP.Zero;
            matrix.M21 = -fp2;
            matrix.M22 = fp;
            matrix.M23 = FP.Zero;
            matrix.M31 = FP.Zero;
            matrix.M32 = FP.Zero;
            matrix.M33 = FP.One;
            return matrix;
        }

        public static void CreateRotationZ(FP radians, out TSMatrix result)
        {
            FP fp = FP.Cos(radians);
            FP fp2 = FP.Sin(radians);
            result.M11 = fp;
            result.M12 = fp2;
            result.M13 = FP.Zero;
            result.M21 = -fp2;
            result.M22 = fp;
            result.M23 = FP.Zero;
            result.M31 = FP.Zero;
            result.M32 = FP.Zero;
            result.M33 = FP.One;
        }

        public TSMatrix(FP m11, FP m12, FP m13, FP m21, FP m22, FP m23, FP m31, FP m32, FP m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }

        public static TSMatrix Multiply(TSMatrix matrix1, TSMatrix matrix2)
        {
            TSMatrix matrix;
            Multiply(ref matrix1, ref matrix2, out matrix);
            return matrix;
        }

        public static void Multiply(ref TSMatrix matrix1, ref TSMatrix matrix2, out TSMatrix result)
        {
            FP fp = ((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31);
            FP fp2 = ((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32);
            FP fp3 = ((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33);
            FP fp4 = ((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31);
            FP fp5 = ((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32);
            FP fp6 = ((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33);
            FP fp7 = ((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31);
            FP fp8 = ((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32);
            FP fp9 = ((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33);
            result.M11 = fp;
            result.M12 = fp2;
            result.M13 = fp3;
            result.M21 = fp4;
            result.M22 = fp5;
            result.M23 = fp6;
            result.M31 = fp7;
            result.M32 = fp8;
            result.M33 = fp9;
        }

        public static TSMatrix Add(TSMatrix matrix1, TSMatrix matrix2)
        {
            TSMatrix matrix;
            Add(ref matrix1, ref matrix2, out matrix);
            return matrix;
        }

        public static void Add(ref TSMatrix matrix1, ref TSMatrix matrix2, out TSMatrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
        }

        public static TSMatrix Inverse(TSMatrix matrix)
        {
            TSMatrix matrix2;
            Inverse(ref matrix, out matrix2);
            return matrix2;
        }

        public FP Determinant()
        {
            return (((((((this.M11 * this.M22) * this.M33) + ((this.M12 * this.M23) * this.M31)) + ((this.M13 * this.M21) * this.M32)) - ((this.M31 * this.M22) * this.M13)) - ((this.M32 * this.M23) * this.M11)) - ((this.M33 * this.M21) * this.M12));
        }

        public static void Invert(ref TSMatrix matrix, out TSMatrix result)
        {
            FP fp = 1 / matrix.Determinant();
            FP fp2 = ((matrix.M22 * matrix.M33) - (matrix.M23 * matrix.M32)) * fp;
            FP fp3 = ((matrix.M13 * matrix.M32) - (matrix.M33 * matrix.M12)) * fp;
            FP fp4 = ((matrix.M12 * matrix.M23) - (matrix.M22 * matrix.M13)) * fp;
            FP fp5 = ((matrix.M23 * matrix.M31) - (matrix.M21 * matrix.M33)) * fp;
            FP fp6 = ((matrix.M11 * matrix.M33) - (matrix.M13 * matrix.M31)) * fp;
            FP fp7 = ((matrix.M13 * matrix.M21) - (matrix.M11 * matrix.M23)) * fp;
            FP fp8 = ((matrix.M21 * matrix.M32) - (matrix.M22 * matrix.M31)) * fp;
            FP fp9 = ((matrix.M12 * matrix.M31) - (matrix.M11 * matrix.M32)) * fp;
            FP fp10 = ((matrix.M11 * matrix.M22) - (matrix.M12 * matrix.M21)) * fp;
            result.M11 = fp2;
            result.M12 = fp3;
            result.M13 = fp4;
            result.M21 = fp5;
            result.M22 = fp6;
            result.M23 = fp7;
            result.M31 = fp8;
            result.M32 = fp9;
            result.M33 = fp10;
        }

        public static void Inverse(ref TSMatrix matrix, out TSMatrix result)
        {
            FP fp = (((((((0x400 * matrix.M11) * matrix.M22) * matrix.M33) - (((0x400 * matrix.M11) * matrix.M23) * matrix.M32)) - (((0x400 * matrix.M12) * matrix.M21) * matrix.M33)) + (((0x400 * matrix.M12) * matrix.M23) * matrix.M31)) + (((0x400 * matrix.M13) * matrix.M21) * matrix.M32)) - (((0x400 * matrix.M13) * matrix.M22) * matrix.M31);
            FP fp2 = ((0x400 * matrix.M22) * matrix.M33) - ((0x400 * matrix.M23) * matrix.M32);
            FP fp3 = ((0x400 * matrix.M13) * matrix.M32) - ((0x400 * matrix.M12) * matrix.M33);
            FP fp4 = ((0x400 * matrix.M12) * matrix.M23) - ((0x400 * matrix.M22) * matrix.M13);
            FP fp5 = ((0x400 * matrix.M23) * matrix.M31) - ((0x400 * matrix.M33) * matrix.M21);
            FP fp6 = ((0x400 * matrix.M11) * matrix.M33) - ((0x400 * matrix.M31) * matrix.M13);
            FP fp7 = ((0x400 * matrix.M13) * matrix.M21) - ((0x400 * matrix.M23) * matrix.M11);
            FP fp8 = ((0x400 * matrix.M21) * matrix.M32) - ((0x400 * matrix.M31) * matrix.M22);
            FP fp9 = ((0x400 * matrix.M12) * matrix.M31) - ((0x400 * matrix.M32) * matrix.M11);
            FP fp10 = ((0x400 * matrix.M11) * matrix.M22) - ((0x400 * matrix.M21) * matrix.M12);
            if (fp == 0)
            {
                result.M11 = FP.PositiveInfinity;
                result.M12 = FP.PositiveInfinity;
                result.M13 = FP.PositiveInfinity;
                result.M21 = FP.PositiveInfinity;
                result.M22 = FP.PositiveInfinity;
                result.M23 = FP.PositiveInfinity;
                result.M31 = FP.PositiveInfinity;
                result.M32 = FP.PositiveInfinity;
                result.M33 = FP.PositiveInfinity;
            }
            else
            {
                result.M11 = fp2 / fp;
                result.M12 = fp3 / fp;
                result.M13 = fp4 / fp;
                result.M21 = fp5 / fp;
                result.M22 = fp6 / fp;
                result.M23 = fp7 / fp;
                result.M31 = fp8 / fp;
                result.M32 = fp9 / fp;
                result.M33 = fp10 / fp;
            }
        }

        public static TSMatrix Multiply(TSMatrix matrix1, FP scaleFactor)
        {
            TSMatrix matrix;
            Multiply(ref matrix1, scaleFactor, out matrix);
            return matrix;
        }

        public static void Multiply(ref TSMatrix matrix1, FP scaleFactor, out TSMatrix result)
        {
            FP fp = scaleFactor;
            result.M11 = matrix1.M11 * fp;
            result.M12 = matrix1.M12 * fp;
            result.M13 = matrix1.M13 * fp;
            result.M21 = matrix1.M21 * fp;
            result.M22 = matrix1.M22 * fp;
            result.M23 = matrix1.M23 * fp;
            result.M31 = matrix1.M31 * fp;
            result.M32 = matrix1.M32 * fp;
            result.M33 = matrix1.M33 * fp;
        }

        public static TSMatrix CreateFromLookAt(TSVector position, TSVector target)
        {
            TSMatrix matrix;
            LookAt(out matrix, position, target);
            return matrix;
        }

        public static void LookAt(out TSMatrix result, TSVector position, TSVector target)
        {
            TSVector vector = target - position;
            vector.Normalize();
            TSVector vector2 = TSVector.Cross(TSVector.up, vector);
            vector2.Normalize();
            TSVector vector3 = TSVector.Cross(vector, vector2);
            result.M11 = vector2.x;
            result.M21 = vector3.x;
            result.M31 = vector.x;
            result.M12 = vector2.y;
            result.M22 = vector3.y;
            result.M32 = vector.y;
            result.M13 = vector2.z;
            result.M23 = vector3.z;
            result.M33 = vector.z;
        }

        public static TSMatrix CreateFromQuaternion(TSQuaternion quaternion)
        {
            TSMatrix matrix;
            CreateFromQuaternion(ref quaternion, out matrix);
            return matrix;
        }

        public static void CreateFromQuaternion(ref TSQuaternion quaternion, out TSMatrix result)
        {
            FP fp = quaternion.x * quaternion.x;
            FP fp2 = quaternion.y * quaternion.y;
            FP fp3 = quaternion.z * quaternion.z;
            FP fp4 = quaternion.x * quaternion.y;
            FP fp5 = quaternion.z * quaternion.w;
            FP fp6 = quaternion.z * quaternion.x;
            FP fp7 = quaternion.y * quaternion.w;
            FP fp8 = quaternion.y * quaternion.z;
            FP fp9 = quaternion.x * quaternion.w;
            result.M11 = FP.One - (2 * (fp2 + fp3));
            result.M12 = 2 * (fp4 + fp5);
            result.M13 = 2 * (fp6 - fp7);
            result.M21 = 2 * (fp4 - fp5);
            result.M22 = FP.One - (2 * (fp3 + fp));
            result.M23 = 2 * (fp8 + fp9);
            result.M31 = 2 * (fp6 + fp7);
            result.M32 = 2 * (fp8 - fp9);
            result.M33 = FP.One - (2 * (fp2 + fp));
        }

        public static TSMatrix Transpose(TSMatrix matrix)
        {
            TSMatrix matrix2;
            Transpose(ref matrix, out matrix2);
            return matrix2;
        }

        public static void Transpose(ref TSMatrix matrix, out TSMatrix result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
        }

        public static TSMatrix operator *(TSMatrix value1, TSMatrix value2)
        {
            TSMatrix matrix;
            Multiply(ref value1, ref value2, out matrix);
            return matrix;
        }

        public FP Trace()
        {
            return ((this.M11 + this.M22) + this.M33);
        }

        public static TSMatrix operator +(TSMatrix value1, TSMatrix value2)
        {
            TSMatrix matrix;
            Add(ref value1, ref value2, out matrix);
            return matrix;
        }

        public static TSMatrix operator -(TSMatrix value1, TSMatrix value2)
        {
            TSMatrix matrix;
            Multiply(ref value2, -FP.One, out value2);
            Add(ref value1, ref value2, out matrix);
            return matrix;
        }

        public static bool operator ==(TSMatrix value1, TSMatrix value2)
        {
            return (((((value1.M11 == value2.M11) && (value1.M12 == value2.M12)) && ((value1.M13 == value2.M13) && (value1.M21 == value2.M21))) && (((value1.M22 == value2.M22) && (value1.M23 == value2.M23)) && ((value1.M31 == value2.M31) && (value1.M32 == value2.M32)))) && (value1.M33 == value2.M33));
        }

        public static bool operator !=(TSMatrix value1, TSMatrix value2)
        {
            return (((((value1.M11 != value2.M11) || (value1.M12 != value2.M12)) || ((value1.M13 != value2.M13) || (value1.M21 != value2.M21))) || (((value1.M22 != value2.M22) || (value1.M23 != value2.M23)) || ((value1.M31 != value2.M31) || (value1.M32 != value2.M32)))) || (value1.M33 != value2.M33));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TSMatrix))
            {
                return false;
            }
            TSMatrix matrix = (TSMatrix) obj;
            return (((((this.M11 == matrix.M11) && (this.M12 == matrix.M12)) && ((this.M13 == matrix.M13) && (this.M21 == matrix.M21))) && (((this.M22 == matrix.M22) && (this.M23 == matrix.M23)) && ((this.M31 == matrix.M31) && (this.M32 == matrix.M32)))) && (this.M33 == matrix.M33));
        }

        public override int GetHashCode()
        {
            return ((((((((this.M11.GetHashCode() ^ this.M12.GetHashCode()) ^ this.M13.GetHashCode()) ^ this.M21.GetHashCode()) ^ this.M22.GetHashCode()) ^ this.M23.GetHashCode()) ^ this.M31.GetHashCode()) ^ this.M32.GetHashCode()) ^ this.M33.GetHashCode());
        }

        public static void CreateFromAxisAngle(ref TSVector axis, FP angle, out TSMatrix result)
        {
            FP x = axis.x;
            FP y = axis.y;
            FP z = axis.z;
            FP fp4 = FP.Sin(angle);
            FP fp5 = FP.Cos(angle);
            FP fp6 = x * x;
            FP fp7 = y * y;
            FP fp8 = z * z;
            FP fp9 = x * y;
            FP fp10 = x * z;
            FP fp11 = y * z;
            result.M11 = fp6 + (fp5 * (FP.One - fp6));
            result.M12 = (fp9 - (fp5 * fp9)) + (fp4 * z);
            result.M13 = (fp10 - (fp5 * fp10)) - (fp4 * y);
            result.M21 = (fp9 - (fp5 * fp9)) - (fp4 * z);
            result.M22 = fp7 + (fp5 * (FP.One - fp7));
            result.M23 = (fp11 - (fp5 * fp11)) + (fp4 * x);
            result.M31 = (fp10 - (fp5 * fp10)) + (fp4 * y);
            result.M32 = (fp11 - (fp5 * fp11)) - (fp4 * x);
            result.M33 = fp8 + (fp5 * (FP.One - fp8));
        }

        public static TSMatrix AngleAxis(FP angle, TSVector axis)
        {
            TSMatrix matrix;
            CreateFromAxisAngle(ref axis, angle, out matrix);
            return matrix;
        }

        public override string ToString()
        {
            object[] args = new object[] { this.M11.RawValue, this.M12.RawValue, this.M13.RawValue, this.M21.RawValue, this.M22.RawValue, this.M23.RawValue, this.M31.RawValue, this.M32.RawValue, this.M33.RawValue };
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", args);
        }

        private void LookAt(TSVector position, TSVector target)
        {
            LookAt(out this, position, target);
        }
    }
}

