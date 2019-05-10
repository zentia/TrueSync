using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class Distance
	{
		[ThreadStatic]
		public static int GJKCalls;

		[ThreadStatic]
		public static int GJKIters;

		[ThreadStatic]
		public static int GJKMaxIters;

		public static void ComputeDistance(out DistanceOutput output, out SimplexCache cache, DistanceInput input)
		{
			cache = default(SimplexCache);
			Simplex simplex = default(Simplex);
			simplex.ReadCache(ref cache, input.ProxyA, ref input.TransformA, input.ProxyB, ref input.TransformB);
			FixedArray3<int> fixedArray = default(FixedArray3<int>);
			FixedArray3<int> fixedArray2 = default(FixedArray3<int>);
			int i = 0;
			while (i < 20)
			{
				int count = simplex.Count;
				for (int j = 0; j < count; j++)
				{
					fixedArray[j] = simplex.V[j].IndexA;
					fixedArray2[j] = simplex.V[j].IndexB;
				}
				switch (simplex.Count)
				{
				case 1:
					break;
				case 2:
					simplex.Solve2();
					break;
				case 3:
					simplex.Solve3();
					break;
				default:
					Debug.Assert(false);
					break;
				}
				bool flag = simplex.Count == 3;
				if (flag)
				{
					break;
				}
				TSVector2 searchDirection = simplex.GetSearchDirection();
				bool flag2 = searchDirection.LengthSquared() < Settings.EpsilonSqr;
				if (flag2)
				{
					break;
				}
				SimplexVertex simplexVertex = simplex.V[simplex.Count];
				simplexVertex.IndexA = input.ProxyA.GetSupport(MathUtils.MulT(input.TransformA.q, -searchDirection));
				simplexVertex.WA = MathUtils.Mul(ref input.TransformA, input.ProxyA.Vertices[simplexVertex.IndexA]);
				simplexVertex.IndexB = input.ProxyB.GetSupport(MathUtils.MulT(input.TransformB.q, searchDirection));
				simplexVertex.WB = MathUtils.Mul(ref input.TransformB, input.ProxyB.Vertices[simplexVertex.IndexB]);
				simplexVertex.W = simplexVertex.WB - simplexVertex.WA;
				simplex.V[simplex.Count] = simplexVertex;
				i++;
				bool flag3 = false;
				for (int k = 0; k < count; k++)
				{
					bool flag4 = simplexVertex.IndexA == fixedArray[k] && simplexVertex.IndexB == fixedArray2[k];
					if (flag4)
					{
						flag3 = true;
						break;
					}
				}
				bool flag5 = flag3;
				if (flag5)
				{
					break;
				}
				simplex.Count++;
			}
			simplex.GetWitnessPoints(out output.PointA, out output.PointB);
			output.Distance = (output.PointA - output.PointB).magnitude;
			output.Iterations = i;
			simplex.WriteCache(ref cache);
			bool useRadii = input.UseRadii;
			if (useRadii)
			{
				FP radius = input.ProxyA.Radius;
				FP radius2 = input.ProxyB.Radius;
				bool flag6 = output.Distance > radius + radius2 && output.Distance > Settings.Epsilon;
				if (flag6)
				{
					output.Distance -= radius + radius2;
					TSVector2 value = output.PointB - output.PointA;
					value.Normalize();
					output.PointA += radius * value;
					output.PointB -= radius2 * value;
				}
				else
				{
					TSVector2 tSVector = 0.5f * (output.PointA + output.PointB);
					output.PointA = tSVector;
					output.PointB = tSVector;
					output.Distance = 0f;
				}
			}
		}
	}
}
