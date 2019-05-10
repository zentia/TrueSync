using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class SimpleCombiner
	{
		public static List<Vertices> PolygonizeTriangles(List<Vertices> triangles, int maxPolys = 2147483647, float tolerance = 0.001f)
		{
			bool flag = triangles.Count <= 0;
			List<Vertices> result;
			if (flag)
			{
				result = triangles;
			}
			else
			{
				List<Vertices> list = new List<Vertices>();
				bool[] array = new bool[triangles.Count];
				for (int i = 0; i < triangles.Count; i++)
				{
					array[i] = false;
					Vertices vertices = triangles[i];
					TSVector2 tSVector = vertices[0];
					TSVector2 tSVector2 = vertices[1];
					TSVector2 tSVector3 = vertices[2];
					bool flag2 = (tSVector.x == tSVector2.x && tSVector.y == tSVector2.y) || (tSVector2.x == tSVector3.x && tSVector2.y == tSVector3.y) || (tSVector.x == tSVector3.x && tSVector.y == tSVector3.y);
					if (flag2)
					{
						array[i] = true;
					}
				}
				int num = 0;
				bool flag3 = true;
				while (flag3)
				{
					int num2 = -1;
					for (int j = 0; j < triangles.Count; j++)
					{
						bool flag4 = array[j];
						if (!flag4)
						{
							num2 = j;
							break;
						}
					}
					bool flag5 = num2 == -1;
					if (flag5)
					{
						flag3 = false;
					}
					else
					{
						Vertices vertices2 = new Vertices(3);
						for (int k = 0; k < 3; k++)
						{
							vertices2.Add(triangles[num2][k]);
						}
						array[num2] = true;
						int l = 0;
						int m = 0;
						while (m < 2 * triangles.Count)
						{
							while (l >= triangles.Count)
							{
								l -= triangles.Count;
							}
							bool flag6 = array[l];
							if (!flag6)
							{
								Vertices vertices3 = SimpleCombiner.AddTriangle(triangles[l], vertices2);
								bool flag7 = vertices3 == null;
								if (!flag7)
								{
									bool flag8 = vertices3.Count > Settings.MaxPolygonVertices;
									if (!flag8)
									{
										bool flag9 = vertices3.IsConvex();
										if (flag9)
										{
											vertices2 = new Vertices(vertices3);
											array[l] = true;
										}
									}
								}
							}
							m++;
							l++;
						}
						bool flag10 = num < maxPolys;
						if (flag10)
						{
							SimplifyTools.MergeParallelEdges(vertices2, tolerance);
							bool flag11 = vertices2.Count >= 3;
							if (flag11)
							{
								list.Add(new Vertices(vertices2));
							}
							else
							{
								Debug.WriteLine("Skipping corrupt poly.");
							}
						}
						bool flag12 = vertices2.Count >= 3;
						if (flag12)
						{
							num++;
						}
					}
				}
				for (int n = list.Count - 1; n >= 0; n--)
				{
					bool flag13 = list[n].Count == 0;
					if (flag13)
					{
						list.RemoveAt(n);
					}
				}
				result = list;
			}
			return result;
		}

		private static Vertices AddTriangle(Vertices t, Vertices vertices)
		{
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			for (int i = 0; i < vertices.Count; i++)
			{
				bool flag = t[0].x == vertices[i].x && t[0].y == vertices[i].y;
				if (flag)
				{
					bool flag2 = num == -1;
					if (flag2)
					{
						num = i;
						num2 = 0;
					}
					else
					{
						num3 = i;
						num4 = 0;
					}
				}
				else
				{
					bool flag3 = t[1].x == vertices[i].x && t[1].y == vertices[i].y;
					if (flag3)
					{
						bool flag4 = num == -1;
						if (flag4)
						{
							num = i;
							num2 = 1;
						}
						else
						{
							num3 = i;
							num4 = 1;
						}
					}
					else
					{
						bool flag5 = t[2].x == vertices[i].x && t[2].y == vertices[i].y;
						if (flag5)
						{
							bool flag6 = num == -1;
							if (flag6)
							{
								num = i;
								num2 = 2;
							}
							else
							{
								num3 = i;
								num4 = 2;
							}
						}
					}
				}
			}
			bool flag7 = num == 0 && num3 == vertices.Count - 1;
			if (flag7)
			{
				num = vertices.Count - 1;
				num3 = 0;
			}
			bool flag8 = num3 == -1;
			Vertices result;
			if (flag8)
			{
				result = null;
			}
			else
			{
				int num5 = 0;
				bool flag9 = num5 == num2 || num5 == num4;
				if (flag9)
				{
					num5 = 1;
				}
				bool flag10 = num5 == num2 || num5 == num4;
				if (flag10)
				{
					num5 = 2;
				}
				Vertices vertices2 = new Vertices(vertices.Count + 1);
				for (int j = 0; j < vertices.Count; j++)
				{
					vertices2.Add(vertices[j]);
					bool flag11 = j == num;
					if (flag11)
					{
						vertices2.Add(t[num5]);
					}
				}
				result = vertices2;
			}
			return result;
		}
	}
}
