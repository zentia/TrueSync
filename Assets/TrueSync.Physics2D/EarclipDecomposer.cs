using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal static class EarclipDecomposer
	{
		private class Triangle : Vertices
		{
			public Triangle(FP x1, FP y1, FP x2, FP y2, FP x3, FP y3)
			{
				FP x4 = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
				bool flag = x4 > 0;
				if (flag)
				{
					base.Add(new TSVector2(x1, y1));
					base.Add(new TSVector2(x2, y2));
					base.Add(new TSVector2(x3, y3));
				}
				else
				{
					base.Add(new TSVector2(x1, y1));
					base.Add(new TSVector2(x3, y3));
					base.Add(new TSVector2(x2, y2));
				}
			}

			public bool IsInside(FP x, FP y)
			{
				TSVector2 tSVector = base[0];
				TSVector2 tSVector2 = base[1];
				TSVector2 tSVector3 = base[2];
				bool flag = x < tSVector.x && x < tSVector2.x && x < tSVector3.x;
				bool result;
				if (flag)
				{
					result = false;
				}
				else
				{
					bool flag2 = x > tSVector.x && x > tSVector2.x && x > tSVector3.x;
					if (flag2)
					{
						result = false;
					}
					else
					{
						bool flag3 = y < tSVector.y && y < tSVector2.y && y < tSVector3.y;
						if (flag3)
						{
							result = false;
						}
						else
						{
							bool flag4 = y > tSVector.y && y > tSVector2.y && y > tSVector3.y;
							if (flag4)
							{
								result = false;
							}
							else
							{
								FP y2 = x - tSVector.x;
								FP y3 = y - tSVector.y;
								FP fP = tSVector2.x - tSVector.x;
								FP fP2 = tSVector2.y - tSVector.y;
								FP fP3 = tSVector3.x - tSVector.x;
								FP fP4 = tSVector3.y - tSVector.y;
								FP x2 = fP3 * fP3 + fP4 * fP4;
								FP fP5 = fP3 * fP + fP4 * fP2;
								FP y4 = fP3 * y2 + fP4 * y3;
								FP fP6 = fP * fP + fP2 * fP2;
								FP y5 = fP * y2 + fP2 * y3;
								FP y6 = 1f / (x2 * fP6 - fP5 * fP5);
								FP x3 = (fP6 * y4 - fP5 * y5) * y6;
								FP fP7 = (x2 * y5 - fP5 * y4) * y6;
								result = (x3 > 0 && fP7 > 0 && x3 + fP7 < 1);
							}
						}
					}
				}
				return result;
			}
		}

		public static List<Vertices> ConvexPartition(Vertices vertices, FP tolerance)
		{
			Debug.Assert(vertices.Count > 3);
			Debug.Assert(!vertices.IsCounterClockWise());
			return EarclipDecomposer.TriangulatePolygon(vertices, tolerance);
		}

		private static List<Vertices> TriangulatePolygon(Vertices vertices, FP tolerance)
		{
			bool flag = vertices.Count < 3;
			List<Vertices> result;
			if (flag)
			{
				result = new List<Vertices>();
			}
			else
			{
				List<Vertices> list = new List<Vertices>();
				Vertices pin = new Vertices(vertices);
				Vertices vertices2;
				Vertices vertices3;
				bool flag2 = EarclipDecomposer.ResolvePinchPoint(pin, out vertices2, out vertices3, tolerance);
				if (flag2)
				{
					List<Vertices> list2 = EarclipDecomposer.TriangulatePolygon(vertices2, tolerance);
					List<Vertices> list3 = EarclipDecomposer.TriangulatePolygon(vertices3, tolerance);
					bool flag3 = list2.Count == -1 || list3.Count == -1;
					if (flag3)
					{
						throw new Exception("Can't triangulate your polygon.");
					}
					for (int i = 0; i < list2.Count; i++)
					{
						list.Add(new Vertices(list2[i]));
					}
					for (int j = 0; j < list3.Count; j++)
					{
						list.Add(new Vertices(list3[j]));
					}
					result = list;
				}
				else
				{
					Vertices[] array = new Vertices[vertices.Count - 2];
					int num = 0;
					FP[] array2 = new FP[vertices.Count];
					FP[] array3 = new FP[vertices.Count];
					for (int k = 0; k < vertices.Count; k++)
					{
						array2[k] = vertices[k].x;
						array3[k] = vertices[k].y;
					}
					int l = vertices.Count;
					while (l > 3)
					{
						int num2 = -1;
						FP y = -10f;
						for (int m = 0; m < l; m++)
						{
							bool flag4 = EarclipDecomposer.IsEar(m, array2, array3, l);
							if (flag4)
							{
								int num3 = EarclipDecomposer.Remainder(m - 1, l);
								int num4 = EarclipDecomposer.Remainder(m + 1, l);
								TSVector2 tSVector = new TSVector2(array2[num4] - array2[m], array3[num4] - array3[m]);
								TSVector2 tSVector2 = new TSVector2(array2[m] - array2[num3], array3[m] - array3[num3]);
								TSVector2 tSVector3 = new TSVector2(array2[num3] - array2[num4], array3[num3] - array3[num4]);
								tSVector.Normalize();
								tSVector2.Normalize();
								tSVector3.Normalize();
								FP fP;
								MathUtils.Cross(ref tSVector, ref tSVector2, out fP);
								fP = FP.Abs(fP);
								FP fP2;
								MathUtils.Cross(ref tSVector2, ref tSVector3, out fP2);
								fP2 = FP.Abs(fP2);
								FP fP3;
								MathUtils.Cross(ref tSVector3, ref tSVector, out fP3);
								fP3 = FP.Abs(fP3);
								FP fP4 = TSMath.Min(fP, TSMath.Min(fP2, fP3));
								bool flag5 = fP4 > y;
								if (flag5)
								{
									num2 = m;
									y = fP4;
								}
							}
						}
						bool flag6 = num2 == -1;
						if (flag6)
						{
							for (int n = 0; n < num; n++)
							{
								list.Add(array[n]);
							}
							result = list;
							return result;
						}
						l--;
						FP[] array4 = new FP[l];
						FP[] array5 = new FP[l];
						int num5 = 0;
						for (int num6 = 0; num6 < l; num6++)
						{
							bool flag7 = num5 == num2;
							if (flag7)
							{
								num5++;
							}
							array4[num6] = array2[num5];
							array5[num6] = array3[num5];
							num5++;
						}
						int num7 = (num2 == 0) ? l : (num2 - 1);
						int num8 = (num2 == l) ? 0 : (num2 + 1);
						EarclipDecomposer.Triangle triangle = new EarclipDecomposer.Triangle(array2[num2], array3[num2], array2[num8], array3[num8], array2[num7], array3[num7]);
						array[num] = triangle;
						num++;
						array2 = array4;
						array3 = array5;
					}
					EarclipDecomposer.Triangle triangle2 = new EarclipDecomposer.Triangle(array2[1], array3[1], array2[2], array3[2], array2[0], array3[0]);
					array[num] = triangle2;
					num++;
					for (int num9 = 0; num9 < num; num9++)
					{
						list.Add(new Vertices(array[num9]));
					}
					result = list;
				}
			}
			return result;
		}

		private static bool ResolvePinchPoint(Vertices pin, out Vertices poutA, out Vertices poutB, FP tolerance)
		{
			poutA = new Vertices();
			poutB = new Vertices();
			bool flag = pin.Count < 3;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = false;
				int num = -1;
				int num2 = -1;
				for (int i = 0; i < pin.Count; i++)
				{
					for (int j = i + 1; j < pin.Count; j++)
					{
						bool flag3 = FP.Abs(pin[i].x - pin[j].x) < tolerance && FP.Abs(pin[i].y - pin[j].y) < tolerance && j != i + 1;
						if (flag3)
						{
							num = i;
							num2 = j;
							flag2 = true;
							break;
						}
					}
					bool flag4 = flag2;
					if (flag4)
					{
						break;
					}
				}
				bool flag5 = flag2;
				if (flag5)
				{
					int num3 = num2 - num;
					bool flag6 = num3 == pin.Count;
					if (flag6)
					{
						result = false;
						return result;
					}
					for (int k = 0; k < num3; k++)
					{
						int index = EarclipDecomposer.Remainder(num + k, pin.Count);
						poutA.Add(pin[index]);
					}
					int num4 = pin.Count - num3;
					for (int l = 0; l < num4; l++)
					{
						int index2 = EarclipDecomposer.Remainder(num2 + l, pin.Count);
						poutB.Add(pin[index2]);
					}
				}
				result = flag2;
			}
			return result;
		}

		private static int Remainder(int x, int modulus)
		{
			int i;
			for (i = x % modulus; i < 0; i += modulus)
			{
			}
			return i;
		}

		private static bool IsEar(int i, FP[] xv, FP[] yv, int xvLength)
		{
			bool flag = i >= xvLength || i < 0 || xvLength < 3;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				int num = i + 1;
				int num2 = i - 1;
				bool flag2 = i == 0;
				FP x;
				FP y;
				FP x2;
				FP y2;
				if (flag2)
				{
					x = xv[0] - xv[xvLength - 1];
					y = yv[0] - yv[xvLength - 1];
					x2 = xv[1] - xv[0];
					y2 = yv[1] - yv[0];
					num2 = xvLength - 1;
				}
				else
				{
					bool flag3 = i == xvLength - 1;
					if (flag3)
					{
						x = xv[i] - xv[i - 1];
						y = yv[i] - yv[i - 1];
						x2 = xv[0] - xv[i];
						y2 = yv[0] - yv[i];
						num = 0;
					}
					else
					{
						x = xv[i] - xv[i - 1];
						y = yv[i] - yv[i - 1];
						x2 = xv[i + 1] - xv[i];
						y2 = yv[i + 1] - yv[i];
					}
				}
				FP x3 = x * y2 - x2 * y;
				bool flag4 = x3 > 0;
				if (flag4)
				{
					result = false;
				}
				else
				{
					EarclipDecomposer.Triangle triangle = new EarclipDecomposer.Triangle(xv[i], yv[i], xv[num], yv[num], xv[num2], yv[num2]);
					for (int j = 0; j < xvLength; j++)
					{
						bool flag5 = j == i || j == num2 || j == num;
						if (!flag5)
						{
							bool flag6 = triangle.IsInside(xv[j], yv[j]);
							if (flag6)
							{
								result = false;
								return result;
							}
						}
					}
					result = true;
				}
			}
			return result;
		}
	}
}
