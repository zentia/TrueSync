using System;

namespace TrueSync
{
	public struct ArbiterKey : IComparable
	{
		internal RigidBody body1;

		internal RigidBody body2;

		public ArbiterKey(RigidBody body1, RigidBody body2)
		{
			this.body1 = body1;
			this.body2 = body2;
		}

		internal void SetBodies(RigidBody body1, RigidBody body2)
		{
			this.body1 = body1;
			this.body2 = body2;
		}

		public override bool Equals(object obj)
		{
			ArbiterKey arbiterKey = (ArbiterKey)obj;
			return (arbiterKey.body1.Equals(this.body1) && arbiterKey.body2.Equals(this.body2)) || (arbiterKey.body1.Equals(this.body2) && arbiterKey.body2.Equals(this.body1));
		}

		public override int GetHashCode()
		{
			return this.body1.GetHashCode() + this.body2.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			bool flag = obj is ArbiterKey;
			int result;
			if (flag)
			{
				long num = (long)((ArbiterKey)obj).GetHashCode();
				long num2 = (long)this.GetHashCode();
				long num3 = num - num2;
				bool flag2 = num3 < 0L;
				if (flag2)
				{
					result = 1;
					return result;
				}
				bool flag3 = num3 > 0L;
				if (flag3)
				{
					result = -1;
					return result;
				}
			}
			result = 0;
			return result;
		}
	}
}
