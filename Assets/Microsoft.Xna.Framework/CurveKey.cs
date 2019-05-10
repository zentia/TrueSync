using System;
using TrueSync;

namespace Microsoft.Xna.Framework
{
	public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
	{
		private CurveContinuity continuity;

		private FP position;

		private FP tangentIn;

		private FP tangentOut;

		private FP value;

		public CurveContinuity Continuity
		{
			get
			{
				return this.continuity;
			}
			set
			{
				this.continuity = value;
			}
		}

		public FP Position
		{
			get
			{
				return this.position;
			}
		}

		public FP TangentIn
		{
			get
			{
				return this.tangentIn;
			}
			set
			{
				this.tangentIn = value;
			}
		}

		public FP TangentOut
		{
			get
			{
				return this.tangentOut;
			}
			set
			{
				this.tangentOut = value;
			}
		}

		public FP Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}

		public CurveKey(FP position, FP value) : this(position, value, 0, 0, CurveContinuity.Smooth)
		{
		}

		public CurveKey(FP position, FP value, FP tangentIn, FP tangentOut) : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
		{
		}

		public CurveKey(FP position, FP value, FP tangentIn, FP tangentOut, CurveContinuity continuity)
		{
			this.position = position;
			this.value = value;
			this.tangentIn = tangentIn;
			this.tangentOut = tangentOut;
			this.continuity = continuity;
		}

		public int CompareTo(CurveKey other)
		{
			return this.position.CompareTo(other.position);
		}

		public bool Equals(CurveKey other)
		{
			return this == other;
		}

		public static bool operator !=(CurveKey a, CurveKey b)
		{
			return !(a == b);
		}

		public static bool operator ==(CurveKey a, CurveKey b)
		{
			bool flag = object.Equals(a, null);
			bool result;
			if (flag)
			{
				result = object.Equals(b, null);
			}
			else
			{
				bool flag2 = object.Equals(b, null);
				if (flag2)
				{
					result = object.Equals(a, null);
				}
				else
				{
					result = (a.position == b.position && a.value == b.value && a.tangentIn == b.tangentIn && a.tangentOut == b.tangentOut && a.continuity == b.continuity);
				}
			}
			return result;
		}

		public CurveKey Clone()
		{
			return new CurveKey(this.position, this.value, this.tangentIn, this.tangentOut, this.continuity);
		}

		public override bool Equals(object obj)
		{
			return obj is CurveKey && (CurveKey)obj == this;
		}

		public override int GetHashCode()
		{
			return this.position.GetHashCode() ^ this.value.GetHashCode() ^ this.tangentIn.GetHashCode() ^ this.tangentOut.GetHashCode() ^ this.continuity.GetHashCode();
		}
	}
}
