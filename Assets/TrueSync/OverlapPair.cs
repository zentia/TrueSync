using System;

namespace TrueSync
{
	public class OverlapPair : IComparable
	{
		public IBroadphaseEntity Entity1;

		public IBroadphaseEntity Entity2;

		public OverlapPair(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			this.Entity1 = entity1;
			this.Entity2 = entity2;
		}

		internal void SetBodies(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			this.Entity1 = entity1;
			this.Entity2 = entity2;
		}

		public override bool Equals(object obj)
		{
			OverlapPair overlapPair = (OverlapPair)obj;
			return (overlapPair.Entity1.Equals(this.Entity1) && overlapPair.Entity2.Equals(this.Entity2)) || (overlapPair.Entity1.Equals(this.Entity2) && overlapPair.Entity2.Equals(this.Entity1));
		}

		public override int GetHashCode()
		{
			return this.Entity1.GetHashCode() + this.Entity2.GetHashCode();
		}

		public int CompareTo(object obj)
		{
			bool flag = obj is OverlapPair;
			int result;
			if (flag)
			{
				long num = (long)((OverlapPair)obj).GetHashCode();
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
