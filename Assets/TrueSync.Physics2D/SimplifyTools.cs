using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class SimplifyTools
	{
		public static Vertices CollinearSimplify(Vertices vertices, FP collinearityTolerance)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				Vertices vertices2 = new Vertices(vertices.Count);
				for (int i = 0; i < vertices.Count; i++)
				{
					TSVector2 tSVector = vertices.PreviousVertex(i);
					TSVector2 item = vertices[i];
					TSVector2 tSVector2 = vertices.NextVertex(i);
					bool flag2 = MathUtils.IsCollinear(ref tSVector, ref item, ref tSVector2, collinearityTolerance);
					if (!flag2)
					{
						vertices2.Add(item);
					}
				}
				result = vertices2;
			}
			return result;
		}

		public static Vertices DouglasPeuckerSimplify(Vertices vertices, FP distanceTolerance)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				bool[] array = new bool[vertices.Count];
				for (int i = 0; i < vertices.Count; i++)
				{
					array[i] = true;
				}
				SimplifyTools.SimplifySection(vertices, 0, vertices.Count - 1, array, distanceTolerance);
				Vertices vertices2 = new Vertices(vertices.Count);
				for (int j = 0; j < vertices.Count; j++)
				{
					bool flag2 = array[j];
					if (flag2)
					{
						vertices2.Add(vertices[j]);
					}
				}
				result = vertices2;
			}
			return result;
		}

		private static void SimplifySection(Vertices vertices, int i, int j, bool[] usePoint, FP distanceTolerance)
		{
			bool flag = i + 1 == j;
			if (!flag)
			{
				TSVector2 tSVector = vertices[i];
				TSVector2 tSVector2 = vertices[j];
				FP fP = -1.0;
				int num = i;
				for (int k = i + 1; k < j; k++)
				{
					TSVector2 tSVector3 = vertices[k];
					FP fP2 = LineTools.DistanceBetweenPointAndLineSegment(ref tSVector3, ref tSVector, ref tSVector2);
					bool flag2 = fP2 > fP;
					if (flag2)
					{
						fP = fP2;
						num = k;
					}
				}
				bool flag3 = fP <= distanceTolerance;
				if (flag3)
				{
					for (int l = i + 1; l < j; l++)
					{
						usePoint[l] = false;
					}
				}
				else
				{
					SimplifyTools.SimplifySection(vertices, i, num, usePoint, distanceTolerance);
					SimplifyTools.SimplifySection(vertices, num, j, usePoint, distanceTolerance);
				}
			}
		}

		public static Vertices MergeParallelEdges(Vertices vertices, FP tolerance)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				bool[] array = new bool[vertices.Count];
				int num = vertices.Count;
				for (int i = 0; i < vertices.Count; i++)
				{
					int index = (i == 0) ? (vertices.Count - 1) : (i - 1);
					int index2 = i;
					int index3 = (i == vertices.Count - 1) ? 0 : (i + 1);
					FP fP = vertices[index2].x - vertices[index].x;
					FP fP2 = vertices[index2].y - vertices[index].y;
					FP fP3 = vertices[index3].y - vertices[index2].x;
					FP fP4 = vertices[index3].y - vertices[index2].y;
					FP fP5 = FP.Sqrt(fP * fP + fP2 * fP2);
					FP fP6 = FP.Sqrt(fP3 * fP3 + fP4 * fP4);
					bool flag2 = (!(fP5 > 0f) || !(fP6 > 0f)) && num > 3;
					if (flag2)
					{
						array[i] = true;
						num--;
					}
					fP /= fP5;
					fP2 /= fP5;
					fP3 /= fP6;
					fP4 /= fP6;
					FP value = fP * fP4 - fP3 * fP2;
					FP x = fP * fP3 + fP2 * fP4;
					bool flag3 = FP.Abs(value) < tolerance && x > 0 && num > 3;
					if (flag3)
					{
						array[i] = true;
						num--;
					}
					else
					{
						array[i] = false;
					}
				}
				bool flag4 = num == vertices.Count || num == 0;
				if (flag4)
				{
					result = vertices;
				}
				else
				{
					int num2 = 0;
					Vertices vertices2 = new Vertices(num);
					for (int j = 0; j < vertices.Count; j++)
					{
						bool flag5 = array[j] || num == 0 || num2 == num;
						if (!flag5)
						{
							Debug.Assert(num2 < num);
							vertices2.Add(vertices[j]);
							num2++;
						}
					}
					result = vertices2;
				}
			}
			return result;
		}

		public static Vertices MergeIdenticalPoints(Vertices vertices)
		{
			HashSet<TSVector2> hashSet = new HashSet<TSVector2>();
			foreach (TSVector2 current in vertices)
			{
				hashSet.Add(current);
			}
			return new Vertices(hashSet);
		}

		public static Vertices ReduceByDistance(Vertices vertices, FP distance)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				FP y = distance * distance;
				Vertices vertices2 = new Vertices(vertices.Count);
				for (int i = 0; i < vertices.Count; i++)
				{
					TSVector2 tSVector = vertices[i];
					TSVector2 value = vertices.NextVertex(i);
					bool flag2 = (value - tSVector).LengthSquared() <= y;
					if (!flag2)
					{
						vertices2.Add(tSVector);
					}
				}
				result = vertices2;
			}
			return result;
		}

		public static Vertices ReduceByNth(Vertices vertices, int nth)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				bool flag2 = nth == 0;
				if (flag2)
				{
					result = vertices;
				}
				else
				{
					Vertices vertices2 = new Vertices(vertices.Count);
					for (int i = 0; i < vertices.Count; i++)
					{
						bool flag3 = i % nth == 0;
						if (!flag3)
						{
							vertices2.Add(vertices[i]);
						}
					}
					result = vertices2;
				}
			}
			return result;
		}

		public static Vertices ReduceByArea(Vertices vertices, FP areaTolerance)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				bool flag2 = areaTolerance < 0;
				if (flag2)
				{
					throw new ArgumentOutOfRangeException("areaTolerance", "must be equal to or greater than zero.");
				}
				Vertices vertices2 = new Vertices(vertices.Count);
				TSVector2 tSVector = vertices[vertices.Count - 2];
				TSVector2 tSVector2 = vertices[vertices.Count - 1];
				areaTolerance *= 2;
				int i = 0;
				while (i < vertices.Count)
				{
					TSVector2 tSVector3 = (i == vertices.Count - 1) ? vertices2[0] : vertices[i];
					FP x;
					MathUtils.Cross(ref tSVector, ref tSVector2, out x);
					FP y;
					MathUtils.Cross(ref tSVector2, ref tSVector3, out y);
					FP x2;
					MathUtils.Cross(ref tSVector, ref tSVector3, out x2);
					bool flag3 = FP.Abs(x2 - (x + y)) > areaTolerance;
					if (flag3)
					{
						vertices2.Add(tSVector2);
						tSVector = tSVector2;
					}
					i++;
					tSVector2 = tSVector3;
				}
				result = vertices2;
			}
			return result;
		}
	}
}
