using System;

namespace TrueSync
{
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

		public TSVector eulerAngles
		{
			get
			{
				return new TSVector
				{
					x = TSMath.Atan2(this.M32, this.M33) * FP.Rad2Deg,
					y = TSMath.Atan2(-this.M31, TSMath.Sqrt(this.M32 * this.M32 + this.M33 * this.M33)) * FP.Rad2Deg,
					z = TSMath.Atan2(this.M21, this.M11) * FP.Rad2Deg
				} * -1;
			}
		}

		static TSMatrix()
		{
			TSMatrix.Zero = default(TSMatrix);
			TSMatrix.Identity = default(TSMatrix);
			TSMatrix.Identity.M11 = FP.One;
			TSMatrix.Identity.M22 = FP.One;
			TSMatrix.Identity.M33 = FP.One;
			TSMatrix.InternalIdentity = TSMatrix.Identity;
		}

		public static TSMatrix CreateFromYawPitchRoll(FP yaw, FP pitch, FP roll)
		{
			TSQuaternion tSQuaternion;
			TSQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out tSQuaternion);
			TSMatrix result;
			TSMatrix.CreateFromQuaternion(ref tSQuaternion, out result);
			return result;
		}

		public static TSMatrix CreateRotationX(FP radians)
		{
			FP fP = FP.Cos(radians);
			FP fP2 = FP.Sin(radians);
			TSMatrix result;
			result.M11 = FP.One;
			result.M12 = FP.Zero;
			result.M13 = FP.Zero;
			result.M21 = FP.Zero;
			result.M22 = fP;
			result.M23 = fP2;
			result.M31 = FP.Zero;
			result.M32 = -fP2;
			result.M33 = fP;
			return result;
		}

		public static void CreateRotationX(FP radians, out TSMatrix result)
		{
			FP fP = FP.Cos(radians);
			FP fP2 = FP.Sin(radians);
			result.M11 = FP.One;
			result.M12 = FP.Zero;
			result.M13 = FP.Zero;
			result.M21 = FP.Zero;
			result.M22 = fP;
			result.M23 = fP2;
			result.M31 = FP.Zero;
			result.M32 = -fP2;
			result.M33 = fP;
		}

		public static TSMatrix CreateRotationY(FP radians)
		{
			FP fP = FP.Cos(radians);
			FP fP2 = FP.Sin(radians);
			TSMatrix result;
			result.M11 = fP;
			result.M12 = FP.Zero;
			result.M13 = -fP2;
			result.M21 = FP.Zero;
			result.M22 = FP.One;
			result.M23 = FP.Zero;
			result.M31 = fP2;
			result.M32 = FP.Zero;
			result.M33 = fP;
			return result;
		}

		public static void CreateRotationY(FP radians, out TSMatrix result)
		{
			FP fP = FP.Cos(radians);
			FP fP2 = FP.Sin(radians);
			result.M11 = fP;
			result.M12 = FP.Zero;
			result.M13 = -fP2;
			result.M21 = FP.Zero;
			result.M22 = FP.One;
			result.M23 = FP.Zero;
			result.M31 = fP2;
			result.M32 = FP.Zero;
			result.M33 = fP;
		}

		public static TSMatrix CreateRotationZ(FP radians)
		{
			FP fP = FP.Cos(radians);
			FP fP2 = FP.Sin(radians);
			TSMatrix result;
			result.M11 = fP;
			result.M12 = fP2;
			result.M13 = FP.Zero;
			result.M21 = -fP2;
			result.M22 = fP;
			result.M23 = FP.Zero;
			result.M31 = FP.Zero;
			result.M32 = FP.Zero;
			result.M33 = FP.One;
			return result;
		}

		public static void CreateRotationZ(FP radians, out TSMatrix result)
		{
			FP fP = FP.Cos(radians);
			FP fP2 = FP.Sin(radians);
			result.M11 = fP;
			result.M12 = fP2;
			result.M13 = FP.Zero;
			result.M21 = -fP2;
			result.M22 = fP;
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
			TSMatrix result;
			TSMatrix.Multiply(ref matrix1, ref matrix2, out result);
			return result;
		}

		public static void Multiply(ref TSMatrix matrix1, ref TSMatrix matrix2, out TSMatrix result)
		{
			FP m = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31;
			FP m2 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32;
			FP m3 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33;
			FP m4 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31;
			FP m5 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32;
			FP m6 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33;
			FP m7 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31;
			FP m8 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32;
			FP m9 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33;
			result.M11 = m;
			result.M12 = m2;
			result.M13 = m3;
			result.M21 = m4;
			result.M22 = m5;
			result.M23 = m6;
			result.M31 = m7;
			result.M32 = m8;
			result.M33 = m9;
		}

		public static TSMatrix Add(TSMatrix matrix1, TSMatrix matrix2)
		{
			TSMatrix result;
			TSMatrix.Add(ref matrix1, ref matrix2, out result);
			return result;
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
			TSMatrix result;
			TSMatrix.Inverse(ref matrix, out result);
			return result;
		}

		public FP Determinant()
		{
			return this.M11 * this.M22 * this.M33 + this.M12 * this.M23 * this.M31 + this.M13 * this.M21 * this.M32 - this.M31 * this.M22 * this.M13 - this.M32 * this.M23 * this.M11 - this.M33 * this.M21 * this.M12;
		}

		public static void Invert(ref TSMatrix matrix, out TSMatrix result)
		{
			FP y = 1 / matrix.Determinant();
			FP m = (matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32) * y;
			FP m2 = (matrix.M13 * matrix.M32 - matrix.M33 * matrix.M12) * y;
			FP m3 = (matrix.M12 * matrix.M23 - matrix.M22 * matrix.M13) * y;
			FP m4 = (matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33) * y;
			FP m5 = (matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31) * y;
			FP m6 = (matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23) * y;
			FP m7 = (matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31) * y;
			FP m8 = (matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32) * y;
			FP m9 = (matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21) * y;
			result.M11 = m;
			result.M12 = m2;
			result.M13 = m3;
			result.M21 = m4;
			result.M22 = m5;
			result.M23 = m6;
			result.M31 = m7;
			result.M32 = m8;
			result.M33 = m9;
		}

		public static void Inverse(ref TSMatrix matrix, out TSMatrix result)
		{
			FP fP = 1024 * matrix.M11 * matrix.M22 * matrix.M33 - 1024 * matrix.M11 * matrix.M23 * matrix.M32 - 1024 * matrix.M12 * matrix.M21 * matrix.M33 + 1024 * matrix.M12 * matrix.M23 * matrix.M31 + 1024 * matrix.M13 * matrix.M21 * matrix.M32 - 1024 * matrix.M13 * matrix.M22 * matrix.M31;
			FP x = 1024 * matrix.M22 * matrix.M33 - 1024 * matrix.M23 * matrix.M32;
			FP x2 = 1024 * matrix.M13 * matrix.M32 - 1024 * matrix.M12 * matrix.M33;
			FP x3 = 1024 * matrix.M12 * matrix.M23 - 1024 * matrix.M22 * matrix.M13;
			FP x4 = 1024 * matrix.M23 * matrix.M31 - 1024 * matrix.M33 * matrix.M21;
			FP x5 = 1024 * matrix.M11 * matrix.M33 - 1024 * matrix.M31 * matrix.M13;
			FP x6 = 1024 * matrix.M13 * matrix.M21 - 1024 * matrix.M23 * matrix.M11;
			FP x7 = 1024 * matrix.M21 * matrix.M32 - 1024 * matrix.M31 * matrix.M22;
			FP x8 = 1024 * matrix.M12 * matrix.M31 - 1024 * matrix.M32 * matrix.M11;
			FP x9 = 1024 * matrix.M11 * matrix.M22 - 1024 * matrix.M21 * matrix.M12;
			bool flag = fP == 0;
			if (flag)
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
				result.M11 = x / fP;
				result.M12 = x2 / fP;
				result.M13 = x3 / fP;
				result.M21 = x4 / fP;
				result.M22 = x5 / fP;
				result.M23 = x6 / fP;
				result.M31 = x7 / fP;
				result.M32 = x8 / fP;
				result.M33 = x9 / fP;
			}
		}

		public static TSMatrix Multiply(TSMatrix matrix1, FP scaleFactor)
		{
			TSMatrix result;
			TSMatrix.Multiply(ref matrix1, scaleFactor, out result);
			return result;
		}

		public static void Multiply(ref TSMatrix matrix1, FP scaleFactor, out TSMatrix result)
		{
			result.M11 = matrix1.M11 * scaleFactor;
			result.M12 = matrix1.M12 * scaleFactor;
			result.M13 = matrix1.M13 * scaleFactor;
			result.M21 = matrix1.M21 * scaleFactor;
			result.M22 = matrix1.M22 * scaleFactor;
			result.M23 = matrix1.M23 * scaleFactor;
			result.M31 = matrix1.M31 * scaleFactor;
			result.M32 = matrix1.M32 * scaleFactor;
			result.M33 = matrix1.M33 * scaleFactor;
		}

		public static TSMatrix CreateFromLookAt(TSVector position, TSVector target)
		{
			TSMatrix result;
			TSMatrix.LookAt(out result, position, target);
			return result;
		}

		public static void LookAt(out TSMatrix result, TSVector position, TSVector target)
		{
			TSVector tSVector = target - position;
			tSVector.Normalize();
			TSVector tSVector2 = TSVector.Cross(TSVector.up, tSVector);
			tSVector2.Normalize();
			TSVector tSVector3 = TSVector.Cross(tSVector, tSVector2);
			result.M11 = tSVector2.x;
			result.M21 = tSVector3.x;
			result.M31 = tSVector.x;
			result.M12 = tSVector2.y;
			result.M22 = tSVector3.y;
			result.M32 = tSVector.y;
			result.M13 = tSVector2.z;
			result.M23 = tSVector3.z;
			result.M33 = tSVector.z;
		}

		public static TSMatrix CreateFromQuaternion(TSQuaternion quaternion)
		{
			TSMatrix result;
			TSMatrix.CreateFromQuaternion(ref quaternion, out result);
			return result;
		}

		public static void CreateFromQuaternion(ref TSQuaternion quaternion, out TSMatrix result)
		{
			FP y = quaternion.x * quaternion.x;
			FP x = quaternion.y * quaternion.y;
			FP fP = quaternion.z * quaternion.z;
			FP x2 = quaternion.x * quaternion.y;
			FP y2 = quaternion.z * quaternion.w;
			FP x3 = quaternion.z * quaternion.x;
			FP y3 = quaternion.y * quaternion.w;
			FP x4 = quaternion.y * quaternion.z;
			FP y4 = quaternion.x * quaternion.w;
			result.M11 = FP.One - 2 * (x + fP);
			result.M12 = 2 * (x2 + y2);
			result.M13 = 2 * (x3 - y3);
			result.M21 = 2 * (x2 - y2);
			result.M22 = FP.One - 2 * (fP + y);
			result.M23 = 2 * (x4 + y4);
			result.M31 = 2 * (x3 + y3);
			result.M32 = 2 * (x4 - y4);
			result.M33 = FP.One - 2 * (x + y);
		}

		public static TSMatrix Transpose(TSMatrix matrix)
		{
			TSMatrix result;
			TSMatrix.Transpose(ref matrix, out result);
			return result;
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
			TSMatrix result;
			TSMatrix.Multiply(ref value1, ref value2, out result);
			return result;
		}

		public FP Trace()
		{
			return this.M11 + this.M22 + this.M33;
		}

		public static TSMatrix operator +(TSMatrix value1, TSMatrix value2)
		{
			TSMatrix result;
			TSMatrix.Add(ref value1, ref value2, out result);
			return result;
		}

		public static TSMatrix operator -(TSMatrix value1, TSMatrix value2)
		{
			TSMatrix.Multiply(ref value2, -FP.One, out value2);
			TSMatrix result;
			TSMatrix.Add(ref value1, ref value2, out result);
			return result;
		}

		public static bool operator ==(TSMatrix value1, TSMatrix value2)
		{
			return value1.M11 == value2.M11 && value1.M12 == value2.M12 && value1.M13 == value2.M13 && value1.M21 == value2.M21 && value1.M22 == value2.M22 && value1.M23 == value2.M23 && value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M33 == value2.M33;
		}

		public static bool operator !=(TSMatrix value1, TSMatrix value2)
		{
			return value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 || value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 || value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33;
		}

		public override bool Equals(object obj)
		{
			bool flag = !(obj is TSMatrix);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				TSMatrix tSMatrix = (TSMatrix)obj;
				result = (this.M11 == tSMatrix.M11 && this.M12 == tSMatrix.M12 && this.M13 == tSMatrix.M13 && this.M21 == tSMatrix.M21 && this.M22 == tSMatrix.M22 && this.M23 == tSMatrix.M23 && this.M31 == tSMatrix.M31 && this.M32 == tSMatrix.M32 && this.M33 == tSMatrix.M33);
			}
			return result;
		}

		public override int GetHashCode()
		{
			return this.M11.GetHashCode() ^ this.M12.GetHashCode() ^ this.M13.GetHashCode() ^ this.M21.GetHashCode() ^ this.M22.GetHashCode() ^ this.M23.GetHashCode() ^ this.M31.GetHashCode() ^ this.M32.GetHashCode() ^ this.M33.GetHashCode();
		}

		public static void CreateFromAxisAngle(ref TSVector axis, FP angle, out TSMatrix result)
		{
			FP x = axis.x;
			FP y = axis.y;
			FP z = axis.z;
			FP x2 = FP.Sin(angle);
			FP x3 = FP.Cos(angle);
			FP fP = x * x;
			FP fP2 = y * y;
			FP fP3 = z * z;
			FP fP4 = x * y;
			FP fP5 = x * z;
			FP fP6 = y * z;
			result.M11 = fP + x3 * (FP.One - fP);
			result.M12 = fP4 - x3 * fP4 + x2 * z;
			result.M13 = fP5 - x3 * fP5 - x2 * y;
			result.M21 = fP4 - x3 * fP4 - x2 * z;
			result.M22 = fP2 + x3 * (FP.One - fP2);
			result.M23 = fP6 - x3 * fP6 + x2 * x;
			result.M31 = fP5 - x3 * fP5 + x2 * y;
			result.M32 = fP6 - x3 * fP6 - x2 * x;
			result.M33 = fP3 + x3 * (FP.One - fP3);
		}

		public static TSMatrix AngleAxis(FP angle, TSVector axis)
		{
			TSMatrix result;
			TSMatrix.CreateFromAxisAngle(ref axis, angle, out result);
			return result;
		}

		public override string ToString()
		{
			return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", new object[]
			{
				this.M11.RawValue,
				this.M12.RawValue,
				this.M13.RawValue,
				this.M21.RawValue,
				this.M22.RawValue,
				this.M23.RawValue,
				this.M31.RawValue,
				this.M32.RawValue,
				this.M33.RawValue
			});
		}

		private void LookAt(TSVector position, TSVector target)
		{
			TSMatrix.LookAt(out this, position, target);
		}
	}
}
