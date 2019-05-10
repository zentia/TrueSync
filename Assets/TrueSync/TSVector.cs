using System;

namespace TrueSync
{
	[Serializable]
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

		public FP sqrMagnitude
		{
			get
			{
				return this.x * this.x + this.y * this.y + this.z * this.z;
			}
		}

		public FP magnitude
		{
			get
			{
				FP fP = this.x * this.x + this.y * this.y + this.z * this.z;
				return FP.Sqrt(fP);
			}
		}

		public TSVector normalized
		{
			get
			{
				TSVector result = new TSVector(this.x, this.y, this.z);
				result.Normalize();
				return result;
			}
		}

		static TSVector()
		{
			TSVector.ZeroEpsilonSq = TSMath.Epsilon;
			TSVector.one = new TSVector(1, 1, 1);
			TSVector.zero = new TSVector(0, 0, 0);
			TSVector.left = new TSVector(-1, 0, 0);
			TSVector.right = new TSVector(1, 0, 0);
			TSVector.up = new TSVector(0, 1, 0);
			TSVector.down = new TSVector(0, -1, 0);
			TSVector.back = new TSVector(0, 0, -1);
			TSVector.forward = new TSVector(0, 0, 1);
			TSVector.MinValue = new TSVector(FP.MinValue);
			TSVector.MaxValue = new TSVector(FP.MaxValue);
			TSVector.Arbitrary = new TSVector(1, 1, 1);
			TSVector.InternalZero = TSVector.zero;
		}

		public static TSVector Abs(TSVector other)
		{
			return new TSVector(FP.Abs(other.x), FP.Abs(other.y), FP.Abs(other.z));
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

		public static TSVector Lerp(TSVector from, TSVector to, FP percent)
		{
			return from + (to - from) * percent;
		}

		public override string ToString()
		{
			return string.Format("({0:f1}, {1:f1}, {2:f1})", this.x.AsFloat(), this.y.AsFloat(), this.z.AsFloat());
		}

		public override bool Equals(object obj)
		{
			bool flag = !(obj is TSVector);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				TSVector tSVector = (TSVector)obj;
				result = (this.x == tSVector.x && this.y == tSVector.y && this.z == tSVector.z);
			}
			return result;
		}

		public static TSVector Scale(TSVector vecA, TSVector vecB)
		{
			TSVector result;
			result.x = vecA.x * vecB.x;
			result.y = vecA.y * vecB.y;
			result.z = vecA.z * vecB.z;
			return result;
		}

		public static bool operator ==(TSVector value1, TSVector value2)
		{
			return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z;
		}

		public static bool operator !=(TSVector value1, TSVector value2)
		{
			bool flag = value1.x == value2.x && value1.y == value2.y;
			return !flag || value1.z != value2.z;
		}

		public static TSVector Min(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Min(ref value1, ref value2, out result);
			return result;
		}

		public static void Min(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			result.x = ((value1.x < value2.x) ? value1.x : value2.x);
			result.y = ((value1.y < value2.y) ? value1.y : value2.y);
			result.z = ((value1.z < value2.z) ? value1.z : value2.z);
		}

		public static TSVector Max(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Max(ref value1, ref value2, out result);
			return result;
		}

		public static FP Distance(TSVector v1, TSVector v2)
		{
			return FP.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y) + (v1.z - v2.z) * (v1.z - v2.z));
		}

		public static void Max(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			result.x = ((value1.x > value2.x) ? value1.x : value2.x);
			result.y = ((value1.y > value2.y) ? value1.y : value2.y);
			result.z = ((value1.z > value2.z) ? value1.z : value2.z);
		}

		public void MakeZero()
		{
			this.x = FP.Zero;
			this.y = FP.Zero;
			this.z = FP.Zero;
		}

		public bool IsZero()
		{
			return this.sqrMagnitude == FP.Zero;
		}

		public bool IsNearlyZero()
		{
			return this.sqrMagnitude < TSVector.ZeroEpsilonSq;
		}

		public static TSVector Transform(TSVector position, TSMatrix matrix)
		{
			TSVector result;
			TSVector.Transform(ref position, ref matrix, out result);
			return result;
		}

		public static void Transform(ref TSVector position, ref TSMatrix matrix, out TSVector result)
		{
			FP fP = position.x * matrix.M11 + position.y * matrix.M21 + position.z * matrix.M31;
			FP fP2 = position.x * matrix.M12 + position.y * matrix.M22 + position.z * matrix.M32;
			FP fP3 = position.x * matrix.M13 + position.y * matrix.M23 + position.z * matrix.M33;
			result.x = fP;
			result.y = fP2;
			result.z = fP3;
		}

		public static void TransposedTransform(ref TSVector position, ref TSMatrix matrix, out TSVector result)
		{
			FP fP = position.x * matrix.M11 + position.y * matrix.M12 + position.z * matrix.M13;
			FP fP2 = position.x * matrix.M21 + position.y * matrix.M22 + position.z * matrix.M23;
			FP fP3 = position.x * matrix.M31 + position.y * matrix.M32 + position.z * matrix.M33;
			result.x = fP;
			result.y = fP2;
			result.z = fP3;
		}

		public static FP Dot(TSVector vector1, TSVector vector2)
		{
			return TSVector.Dot(ref vector1, ref vector2);
		}

		public static FP Dot(ref TSVector vector1, ref TSVector vector2)
		{
			return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
		}

		public static TSVector Add(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Add(ref value1, ref value2, out result);
			return result;
		}

		public static void Add(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			FP fP = value1.x + value2.x;
			FP fP2 = value1.y + value2.y;
			FP fP3 = value1.z + value2.z;
			result.x = fP;
			result.y = fP2;
			result.z = fP3;
		}

		public static TSVector Divide(TSVector value1, FP scaleFactor)
		{
			TSVector result;
			TSVector.Divide(ref value1, scaleFactor, out result);
			return result;
		}

		public static void Divide(ref TSVector value1, FP scaleFactor, out TSVector result)
		{
			result.x = value1.x / scaleFactor;
			result.y = value1.y / scaleFactor;
			result.z = value1.z / scaleFactor;
		}

		public static TSVector Subtract(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Subtract(ref value1, ref value2, out result);
			return result;
		}

		public static void Subtract(ref TSVector value1, ref TSVector value2, out TSVector result)
		{
			FP fP = value1.x - value2.x;
			FP fP2 = value1.y - value2.y;
			FP fP3 = value1.z - value2.z;
			result.x = fP;
			result.y = fP2;
			result.z = fP3;
		}

		public static TSVector Cross(TSVector vector1, TSVector vector2)
		{
			TSVector result;
			TSVector.Cross(ref vector1, ref vector2, out result);
			return result;
		}

		public static void Cross(ref TSVector vector1, ref TSVector vector2, out TSVector result)
		{
			FP fP = vector1.y * vector2.z - vector1.z * vector2.y;
			FP fP2 = vector1.z * vector2.x - vector1.x * vector2.z;
			FP fP3 = vector1.x * vector2.y - vector1.y * vector2.x;
			result.x = fP;
			result.y = fP2;
			result.z = fP3;
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode();
		}

		public void Negate()
		{
			this.x = -this.x;
			this.y = -this.y;
			this.z = -this.z;
		}

		public static TSVector Negate(TSVector value)
		{
			TSVector result;
			TSVector.Negate(ref value, out result);
			return result;
		}

		public static void Negate(ref TSVector value, out TSVector result)
		{
			FP fP = -value.x;
			FP fP2 = -value.y;
			FP fP3 = -value.z;
			result.x = fP;
			result.y = fP2;
			result.z = fP3;
		}

		public static TSVector Normalize(TSVector value)
		{
			TSVector result;
			TSVector.Normalize(ref value, out result);
			return result;
		}

		public void Normalize()
		{
			FP fP = this.x * this.x + this.y * this.y + this.z * this.z;
			FP fP2 = FP.One / FP.Sqrt(fP);
			this.x *= fP2;
			this.y *= fP2;
			this.z *= fP2;
		}

		public static void Normalize(ref TSVector value, out TSVector result)
		{
			FP fP = value.x * value.x + value.y * value.y + value.z * value.z;
			FP fP2 = FP.One / FP.Sqrt(fP);
			result.x = value.x * fP2;
			result.y = value.y * fP2;
			result.z = value.z * fP2;
		}

		public static void Swap(ref TSVector vector1, ref TSVector vector2)
		{
			FP fP = vector1.x;
			vector1.x = vector2.x;
			vector2.x = fP;
			fP = vector1.y;
			vector1.y = vector2.y;
			vector2.y = fP;
			fP = vector1.z;
			vector1.z = vector2.z;
			vector2.z = fP;
		}

		public static TSVector Multiply(TSVector value1, FP scaleFactor)
		{
			TSVector result;
			TSVector.Multiply(ref value1, scaleFactor, out result);
			return result;
		}

		public static void Multiply(ref TSVector value1, FP scaleFactor, out TSVector result)
		{
			result.x = value1.x * scaleFactor;
			result.y = value1.y * scaleFactor;
			result.z = value1.z * scaleFactor;
		}

		public static TSVector operator %(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Cross(ref value1, ref value2, out result);
			return result;
		}

		public static FP operator *(TSVector value1, TSVector value2)
		{
			return TSVector.Dot(ref value1, ref value2);
		}

		public static TSVector operator *(TSVector value1, FP value2)
		{
			TSVector result;
			TSVector.Multiply(ref value1, value2, out result);
			return result;
		}

		public static TSVector operator *(FP value1, TSVector value2)
		{
			TSVector result;
			TSVector.Multiply(ref value2, value1, out result);
			return result;
		}

		public static TSVector operator -(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Subtract(ref value1, ref value2, out result);
			return result;
		}

		public static TSVector operator +(TSVector value1, TSVector value2)
		{
			TSVector result;
			TSVector.Add(ref value1, ref value2, out result);
			return result;
		}

		public static TSVector operator /(TSVector value1, FP value2)
		{
			TSVector result;
			TSVector.Divide(ref value1, value2, out result);
			return result;
		}

		public static FP Angle(TSVector a, TSVector b)
		{
			return FP.Acos(a.normalized * b.normalized) * FP.Rad2Deg;
		}
	}
}
