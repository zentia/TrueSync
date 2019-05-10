using System;

namespace TrueSync.Physics2D
{
	public static class Melkman
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
				TSVector2[] array = new TSVector2[vertices.Count + 1];
				int num = 3;
				int num2 = 0;
				int i = 3;
				FP x = MathUtils.Area(vertices[0], vertices[1], vertices[2]);
				bool flag2 = x == 0;
				if (flag2)
				{
					array[0] = vertices[0];
					array[1] = vertices[2];
					array[2] = vertices[0];
					num = 2;
					for (i = 3; i < vertices.Count; i++)
					{
						TSVector2 tSVector = vertices[i];
						bool flag3 = MathUtils.Area(ref array[0], ref array[1], ref tSVector) == 0;
						if (!flag3)
						{
							break;
						}
						array[1] = vertices[i];
					}
				}
				else
				{
					array[0] = (array[3] = vertices[2]);
					bool flag4 = x > 0;
					if (flag4)
					{
						array[1] = vertices[0];
						array[2] = vertices[1];
					}
					else
					{
						array[1] = vertices[1];
						array[2] = vertices[0];
					}
				}
				int num3 = (num == 0) ? (array.Length - 1) : (num - 1);
				int num4 = (num2 == array.Length - 1) ? 0 : (num2 + 1);
				for (int j = i; j < vertices.Count; j++)
				{
					TSVector2 tSVector2 = vertices[j];
					bool flag5 = MathUtils.Area(ref array[num3], ref array[num], ref tSVector2) > 0 && MathUtils.Area(ref array[num2], ref array[num4], ref tSVector2) > 0;
					if (!flag5)
					{
						while (!(MathUtils.Area(ref array[num3], ref array[num], ref tSVector2) > 0))
						{
							num = num3;
							num3 = ((num == 0) ? (array.Length - 1) : (num - 1));
						}
						num = ((num == array.Length - 1) ? 0 : (num + 1));
						num3 = ((num == 0) ? (array.Length - 1) : (num - 1));
						array[num] = tSVector2;
						while (!(MathUtils.Area(ref array[num2], ref array[num4], ref tSVector2) > 0))
						{
							num2 = num4;
							num4 = ((num2 == array.Length - 1) ? 0 : (num2 + 1));
						}
						num2 = ((num2 == 0) ? (array.Length - 1) : (num2 - 1));
						num4 = ((num2 == array.Length - 1) ? 0 : (num2 + 1));
						array[num2] = tSVector2;
					}
				}
				bool flag6 = num2 < num;
				if (flag6)
				{
					Vertices vertices2 = new Vertices(num);
					for (int k = num2; k < num; k++)
					{
						vertices2.Add(array[k]);
					}
					result = vertices2;
				}
				else
				{
					Vertices vertices3 = new Vertices(num + array.Length);
					for (int l = 0; l < num; l++)
					{
						vertices3.Add(array[l]);
					}
					for (int m = num2; m < array.Length; m++)
					{
						vertices3.Add(array[m]);
					}
					result = vertices3;
				}
			}
			return result;
		}
	}
}
