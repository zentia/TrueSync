using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class RayDataComparer : IComparer<FP>
	{
		int IComparer<FP>.Compare(FP a, FP b)
		{
			FP x = a - b;
			bool flag = x > 0;
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				bool flag2 = x < 0;
				if (flag2)
				{
					result = -1;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}
	}
}
