using System;

namespace TrueSync.Physics2D
{
	internal struct Pair : IComparable<Pair>
	{
		public int ProxyIdA;

		public int ProxyIdB;

		public int CompareTo(Pair other)
		{
			bool flag = this.ProxyIdA < other.ProxyIdA;
			int result;
			if (flag)
			{
				result = -1;
			}
			else
			{
				bool flag2 = this.ProxyIdA == other.ProxyIdA;
				if (flag2)
				{
					bool flag3 = this.ProxyIdB < other.ProxyIdB;
					if (flag3)
					{
						result = -1;
						return result;
					}
					bool flag4 = this.ProxyIdB == other.ProxyIdB;
					if (flag4)
					{
						result = 0;
						return result;
					}
				}
				result = 1;
			}
			return result;
		}
	}
}
