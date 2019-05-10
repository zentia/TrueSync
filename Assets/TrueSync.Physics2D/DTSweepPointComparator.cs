using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class DTSweepPointComparator : IComparer<TriangulationPoint>
	{
		public int Compare(TriangulationPoint p1, TriangulationPoint p2)
		{
			bool flag = p1.Y < p2.Y;
			int result;
			if (flag)
			{
				result = -1;
			}
			else
			{
				bool flag2 = p1.Y > p2.Y;
				if (flag2)
				{
					result = 1;
				}
				else
				{
					bool flag3 = p1.X < p2.X;
					if (flag3)
					{
						result = -1;
					}
					else
					{
						bool flag4 = p1.X > p2.X;
						if (flag4)
						{
							result = 1;
						}
						else
						{
							result = 0;
						}
					}
				}
			}
			return result;
		}
	}
}
