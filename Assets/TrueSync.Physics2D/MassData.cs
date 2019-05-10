using System;

namespace TrueSync.Physics2D
{
	public struct MassData : IEquatable<MassData>
	{
		public FP Area
		{
			get;
			internal set;
		}

		public TSVector2 Centroid
		{
			get;
			internal set;
		}

		public FP Inertia
		{
			get;
			internal set;
		}

		public FP Mass
		{
			get;
			internal set;
		}

		public static bool operator ==(MassData left, MassData right)
		{
			return left.Area == right.Area && left.Mass == right.Mass && left.Centroid == right.Centroid && left.Inertia == right.Inertia;
		}

		public static bool operator !=(MassData left, MassData right)
		{
			return !(left == right);
		}

		public bool Equals(MassData other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			bool flag = obj == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = obj.GetType() != typeof(MassData);
				result = (!flag2 && this.Equals((MassData)obj));
			}
			return result;
		}

		public override int GetHashCode()
		{
			int num = this.Area.GetHashCode();
			num = (num * 397 ^ this.Centroid.GetHashCode());
			num = (num * 397 ^ this.Inertia.GetHashCode());
			return num * 397 ^ this.Mass.GetHashCode();
		}
	}
}
