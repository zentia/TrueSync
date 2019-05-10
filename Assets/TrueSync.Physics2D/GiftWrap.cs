using System;

namespace TrueSync.Physics2D
{
	public static class GiftWrap
	{
		public static Vertices GetConvexHull(Vertices vertices)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				int num = 0;
				FP y = vertices[0].x;
				for (int i = 1; i < vertices.Count; i++)
				{
					FP x = vertices[i].x;
					bool flag2 = x > y || (x == y && vertices[i].y < vertices[num].y);
					if (flag2)
					{
						num = i;
						y = x;
					}
				}
				int[] array = new int[vertices.Count];
				int num2 = 0;
				int num3 = num;
				bool flag6;
				do
				{
					array[num2] = num3;
					int num4 = 0;
					for (int j = 1; j < vertices.Count; j++)
					{
						bool flag3 = num4 == num3;
						if (flag3)
						{
							num4 = j;
						}
						else
						{
							TSVector2 tSVector = vertices[num4] - vertices[array[num2]];
							TSVector2 tSVector2 = vertices[j] - vertices[array[num2]];
							FP x2 = MathUtils.Cross(ref tSVector, ref tSVector2);
							bool flag4 = x2 < 0f;
							if (flag4)
							{
								num4 = j;
							}
							bool flag5 = x2 == 0f && tSVector2.LengthSquared() > tSVector.LengthSquared();
							if (flag5)
							{
								num4 = j;
							}
						}
					}
					num2++;
					num3 = num4;
					flag6 = (num4 == num);
				}
				while (!flag6);
				Vertices vertices2 = new Vertices(num2);
				for (int k = 0; k < num2; k++)
				{
					vertices2.Add(vertices[array[k]]);
				}
				result = vertices2;
			}
			return result;
		}
	}
}
