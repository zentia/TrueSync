using System;

namespace TrueSync
{
	[Serializable]
	public struct TSQuaternion
	{
		public FP x;

		public FP y;

		public FP z;

		public FP w;

		public static readonly TSQuaternion identity;

		public TSVector eulerAngles
		{
			get
			{
				TSVector value = default(TSVector);
				FP fP = this.y * this.y;
				FP fP2 = -2f * (fP + this.z * this.z) + 1f;
				FP fP3 = 2f * (this.x * this.y - this.w * this.z);
				FP fP4 = -2f * (this.x * this.z + this.w * this.y);
				FP fP5 = 2f * (this.y * this.z - this.w * this.x);
				FP fP6 = -2f * (this.x * this.x + fP) + 1f;
				fP4 = ((fP4 > 1f) ? 1f : fP4);
				fP4 = ((fP4 < -1f) ? -1f : fP4);
				value.x = FP.Atan2(fP5, fP6) * FP.Rad2Deg;
				value.y = FP.Asin(fP4) * FP.Rad2Deg;
				value.z = FP.Atan2(fP3, fP2) * FP.Rad2Deg;
				return value * -1;
			}
		}

		static TSQuaternion()
		{
			TSQuaternion.identity = new TSQuaternion(0, 0, 0, 1);
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
			TSQuaternion tSQuaternion = TSQuaternion.FromToRotation(fromDirection, toDirection);
			this.Set(tSQuaternion.x, tSQuaternion.y, tSQuaternion.z, tSQuaternion.w);
		}

		public static FP Angle(TSQuaternion a, TSQuaternion b)
		{
			TSQuaternion value = TSQuaternion.Inverse(a);
			TSQuaternion tSQuaternion = b * value;
			FP result = FP.Acos(tSQuaternion.w) * 2 * FP.Rad2Deg;
			bool flag = result > 180;
			if (flag)
			{
				result = 360 - result;
			}
			return result;
		}

		public static TSQuaternion Add(TSQuaternion quaternion1, TSQuaternion quaternion2)
		{
			TSQuaternion result;
			TSQuaternion.Add(ref quaternion1, ref quaternion2, out result);
			return result;
		}

		public static TSQuaternion Euler(FP x, FP y, FP z)
		{
			x *= FP.Deg2Rad;
			y *= FP.Deg2Rad;
			z *= FP.Deg2Rad;
			TSQuaternion result;
			TSQuaternion.CreateFromYawPitchRoll(y, x, z, out result);
			return result;
		}

		public static TSQuaternion Euler(TSVector eulerAngles)
		{
			return TSQuaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
		}

		public static TSQuaternion AngleAxis(FP angle, TSVector axis)
		{
			axis *= FP.Deg2Rad;
			axis.Normalize();
			FP fP = angle * FP.Deg2Rad * FP.Half;
			FP fP2 = FP.Sin(fP);
			TSQuaternion result;
			result.x = axis.x * fP2;
			result.y = axis.y * fP2;
			result.z = axis.z * fP2;
			result.w = FP.Cos(fP);
			return result;
		}

		public static void CreateFromYawPitchRoll(FP yaw, FP pitch, FP roll, out TSQuaternion result)
		{
			FP fP = roll * FP.Half;
			FP fP2 = FP.Sin(fP);
			FP fP3 = FP.Cos(fP);
			FP fP4 = pitch * FP.Half;
			FP fP5 = FP.Sin(fP4);
			FP fP6 = FP.Cos(fP4);
			FP fP7 = yaw * FP.Half;
			FP fP8 = FP.Sin(fP7);
			FP fP9 = FP.Cos(fP7);
			result.x = fP9 * fP5 * fP3 + fP8 * fP6 * fP2;
			result.y = fP8 * fP6 * fP3 - fP9 * fP5 * fP2;
			result.z = fP9 * fP6 * fP2 - fP8 * fP5 * fP3;
			result.w = fP9 * fP6 * fP3 + fP8 * fP5 * fP2;
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
			TSQuaternion result;
			result.x = -value.x;
			result.y = -value.y;
			result.z = -value.z;
			result.w = value.w;
			return result;
		}

		public static FP Dot(TSQuaternion a, TSQuaternion b)
		{
			return a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public static TSQuaternion Inverse(TSQuaternion rotation)
		{
			FP scaleFactor = FP.One / (rotation.x * rotation.x + rotation.y * rotation.y + rotation.z * rotation.z + rotation.w * rotation.w);
			return TSQuaternion.Multiply(TSQuaternion.Conjugate(rotation), scaleFactor);
		}

		public static TSQuaternion FromToRotation(TSVector fromVector, TSVector toVector)
		{
			TSVector tSVector = TSVector.Cross(fromVector, toVector);
			TSQuaternion result = new TSQuaternion(tSVector.x, tSVector.y, tSVector.z, TSVector.Dot(fromVector, toVector));
			result.w += FP.Sqrt(fromVector.sqrMagnitude * toVector.sqrMagnitude);
			result.Normalize();
			return result;
		}

		public static TSQuaternion Lerp(TSQuaternion a, TSQuaternion b, FP t)
		{
			t = TSMath.Clamp(t, FP.Zero, FP.One);
			return TSQuaternion.LerpUnclamped(a, b, t);
		}

		public static TSQuaternion LerpUnclamped(TSQuaternion a, TSQuaternion b, FP t)
		{
			TSQuaternion result = TSQuaternion.Multiply(a, 1 - t) + TSQuaternion.Multiply(b, t);
			result.Normalize();
			return result;
		}

		public static TSQuaternion Subtract(TSQuaternion quaternion1, TSQuaternion quaternion2)
		{
			TSQuaternion result;
			TSQuaternion.Subtract(ref quaternion1, ref quaternion2, out result);
			return result;
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
			TSQuaternion result;
			TSQuaternion.Multiply(ref quaternion1, ref quaternion2, out result);
			return result;
		}

		public static void Multiply(ref TSQuaternion quaternion1, ref TSQuaternion quaternion2, out TSQuaternion result)
		{
			FP fP = quaternion1.x;
			FP fP2 = quaternion1.y;
			FP fP3 = quaternion1.z;
			FP fP4 = quaternion1.w;
			FP fP5 = quaternion2.x;
			FP fP6 = quaternion2.y;
			FP fP7 = quaternion2.z;
			FP fP8 = quaternion2.w;
			FP fP9 = fP2 * fP7 - fP3 * fP6;
			FP fP10 = fP3 * fP5 - fP * fP7;
			FP fP11 = fP * fP6 - fP2 * fP5;
			FP fP12 = fP * fP5 + fP2 * fP6 + fP3 * fP7;
			result.x = fP * fP8 + fP5 * fP4 + fP9;
			result.y = fP2 * fP8 + fP6 * fP4 + fP10;
			result.z = fP3 * fP8 + fP7 * fP4 + fP11;
			result.w = fP4 * fP8 - fP12;
		}

		public static TSQuaternion Multiply(TSQuaternion quaternion1, FP scaleFactor)
		{
			TSQuaternion result;
			TSQuaternion.Multiply(ref quaternion1, scaleFactor, out result);
			return result;
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
			FP fP = this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w;
			FP fP2 = 1 / FP.Sqrt(fP);
			this.x *= fP2;
			this.y *= fP2;
			this.z *= fP2;
			this.w *= fP2;
		}

		public static TSQuaternion CreateFromMatrix(TSMatrix matrix)
		{
			TSQuaternion result;
			TSQuaternion.CreateFromMatrix(ref matrix, out result);
			return result;
		}

		public static void CreateFromMatrix(ref TSMatrix matrix, out TSQuaternion result)
		{
			FP fP = matrix.M11 + matrix.M22 + matrix.M33;
			bool flag = fP > FP.Zero;
			if (flag)
			{
				FP fP2 = FP.Sqrt(fP + FP.One);
				result.w = fP2 * FP.Half;
				fP2 = FP.Half / fP2;
				result.x = (matrix.M23 - matrix.M32) * fP2;
				result.y = (matrix.M31 - matrix.M13) * fP2;
				result.z = (matrix.M12 - matrix.M21) * fP2;
			}
			else
			{
				bool flag2 = matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33;
				if (flag2)
				{
					FP fP3 = FP.Sqrt(FP.One + matrix.M11 - matrix.M22 - matrix.M33);
					FP fP4 = FP.Half / fP3;
					result.x = FP.Half * fP3;
					result.y = (matrix.M12 + matrix.M21) * fP4;
					result.z = (matrix.M13 + matrix.M31) * fP4;
					result.w = (matrix.M23 - matrix.M32) * fP4;
				}
				else
				{
					bool flag3 = matrix.M22 > matrix.M33;
					if (flag3)
					{
						FP fP5 = FP.Sqrt(FP.One + matrix.M22 - matrix.M11 - matrix.M33);
						FP fP6 = FP.Half / fP5;
						result.x = (matrix.M21 + matrix.M12) * fP6;
						result.y = FP.Half * fP5;
						result.z = (matrix.M32 + matrix.M23) * fP6;
						result.w = (matrix.M31 - matrix.M13) * fP6;
					}
					else
					{
						FP fP7 = FP.Sqrt(FP.One + matrix.M33 - matrix.M11 - matrix.M22);
						FP fP8 = FP.Half / fP7;
						result.x = (matrix.M31 + matrix.M13) * fP8;
						result.y = (matrix.M32 + matrix.M23) * fP8;
						result.z = FP.Half * fP7;
						result.w = (matrix.M12 - matrix.M21) * fP8;
					}
				}
			}
		}

		public static TSQuaternion operator *(TSQuaternion value1, TSQuaternion value2)
		{
			TSQuaternion result;
			TSQuaternion.Multiply(ref value1, ref value2, out result);
			return result;
		}

		public static TSQuaternion operator +(TSQuaternion value1, TSQuaternion value2)
		{
			TSQuaternion result;
			TSQuaternion.Add(ref value1, ref value2, out result);
			return result;
		}

		public static TSQuaternion operator -(TSQuaternion value1, TSQuaternion value2)
		{
			TSQuaternion result;
			TSQuaternion.Subtract(ref value1, ref value2, out result);
			return result;
		}

		public static TSVector operator *(TSQuaternion quat, TSVector vec)
		{
			FP fP = quat.x * 2f;
			FP fP2 = quat.y * 2f;
			FP fP3 = quat.z * 2f;
			FP fP4 = quat.x * fP;
			FP fP5 = quat.y * fP2;
			FP fP6 = quat.z * fP3;
			FP fP7 = quat.x * fP2;
			FP fP8 = quat.x * fP3;
			FP fP9 = quat.y * fP3;
			FP fP10 = quat.w * fP;
			FP fP11 = quat.w * fP2;
			FP fP12 = quat.w * fP3;
			TSVector result;
			result.x = (1f - (fP5 + fP6)) * vec.x + (fP7 - fP12) * vec.y + (fP8 + fP11) * vec.z;
			result.y = (fP7 + fP12) * vec.x + (1f - (fP4 + fP6)) * vec.y + (fP9 - fP10) * vec.z;
			result.z = (fP8 - fP11) * vec.x + (fP9 + fP10) * vec.y + (1f - (fP4 + fP5)) * vec.z;
			return result;
		}

		public override string ToString()
		{
			return string.Format("({0:f1}, {1:f1}, {2:f1}, {3:f1})", new object[]
			{
				this.x.AsFloat(),
				this.y.AsFloat(),
				this.z.AsFloat(),
				this.w.AsFloat()
			});
		}
	}
}
