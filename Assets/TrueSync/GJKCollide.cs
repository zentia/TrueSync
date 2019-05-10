using System;

namespace TrueSync
{
	public sealed class GJKCollide
	{
		private const int MaxIterations = 15;

		private static ResourcePool<VoronoiSimplexSolver> simplexSolverPool = new ResourcePool<VoronoiSimplexSolver>();

		private static void SupportMapTransformed(ISupportMappable support, ref TSMatrix orientation, ref TSVector position, ref TSVector direction, out TSVector result)
		{
			result.x = direction.x * orientation.M11 + direction.y * orientation.M12 + direction.z * orientation.M13;
			result.y = direction.x * orientation.M21 + direction.y * orientation.M22 + direction.z * orientation.M23;
			result.z = direction.x * orientation.M31 + direction.y * orientation.M32 + direction.z * orientation.M33;
			support.SupportMapping(ref result, out result);
			FP y = result.x * orientation.M11 + result.y * orientation.M21 + result.z * orientation.M31;
			FP y2 = result.x * orientation.M12 + result.y * orientation.M22 + result.z * orientation.M32;
			FP y3 = result.x * orientation.M13 + result.y * orientation.M23 + result.z * orientation.M33;
			result.x = position.x + y;
			result.y = position.y + y2;
			result.z = position.z + y3;
		}

		public static bool Pointcast(ISupportMappable support, ref TSMatrix orientation, ref TSVector position, ref TSVector point)
		{
			TSVector tSVector;
			GJKCollide.SupportMapTransformed(support, ref orientation, ref position, ref point, out tSVector);
			TSVector.Subtract(ref point, ref tSVector, out tSVector);
			TSVector tSVector2;
			support.SupportCenter(out tSVector2);
			TSVector.Transform(ref tSVector2, ref orientation, out tSVector2);
			TSVector.Add(ref position, ref tSVector2, out tSVector2);
			TSVector.Subtract(ref point, ref tSVector2, out tSVector2);
			TSVector p = point;
			TSVector tSVector3;
			TSVector.Subtract(ref p, ref tSVector, out tSVector3);
			FP x = tSVector3.sqrMagnitude;
			FP eN = FP.EN4;
			int num = 15;
			VoronoiSimplexSolver @new = GJKCollide.simplexSolverPool.GetNew();
			@new.Reset();
			bool result;
			while (x > eN && num-- != 0)
			{
				TSVector q;
				GJKCollide.SupportMapTransformed(support, ref orientation, ref position, ref tSVector3, out q);
				TSVector w;
				TSVector.Subtract(ref p, ref q, out w);
				FP x2 = TSVector.Dot(ref tSVector3, ref w);
				bool flag = x2 > FP.Zero;
				if (flag)
				{
					FP x3 = TSVector.Dot(ref tSVector3, ref tSVector2);
					bool flag2 = x3 >= -(TSMath.Epsilon * TSMath.Epsilon);
					if (flag2)
					{
						GJKCollide.simplexSolverPool.GiveBack(@new);
						result = false;
						return result;
					}
					@new.Reset();
				}
				bool flag3 = !@new.InSimplex(w);
				if (flag3)
				{
					@new.AddVertex(w, p, q);
				}
				bool flag4 = @new.Closest(out tSVector3);
				if (flag4)
				{
					x = tSVector3.sqrMagnitude;
				}
				else
				{
					x = FP.Zero;
				}
			}
			GJKCollide.simplexSolverPool.GiveBack(@new);
			result = true;
			return result;
		}

		public static bool ClosestPoints(ISupportMappable support1, ISupportMappable support2, ref TSMatrix orientation1, ref TSMatrix orientation2, ref TSVector position1, ref TSVector position2, out TSVector p1, out TSVector p2, out TSVector normal)
		{
			VoronoiSimplexSolver @new = GJKCollide.simplexSolverPool.GetNew();
			@new.Reset();
			p1 = (p2 = TSVector.zero);
			TSVector value = position1 - position2;
			TSVector tSVector = TSVector.Negate(value);
			TSVector tSVector2;
			GJKCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref tSVector, out tSVector2);
			TSVector tSVector3;
			GJKCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref value, out tSVector3);
			TSVector tSVector4 = tSVector2 - tSVector3;
			normal = TSVector.zero;
			int num = 15;
			FP x = tSVector4.sqrMagnitude;
			FP eN = FP.EN5;
			while (x > eN && num-- != 0)
			{
				TSVector tSVector5 = TSVector.Negate(tSVector4);
				GJKCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref tSVector5, out tSVector2);
				GJKCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref tSVector4, out tSVector3);
				TSVector w = tSVector2 - tSVector3;
				bool flag = !@new.InSimplex(w);
				if (flag)
				{
					@new.AddVertex(w, tSVector2, tSVector3);
				}
				bool flag2 = @new.Closest(out tSVector4);
				if (flag2)
				{
					x = tSVector4.sqrMagnitude;
					normal = tSVector4;
				}
				else
				{
					x = FP.Zero;
				}
			}
			@new.ComputePoints(out p1, out p2);
			bool flag3 = normal.sqrMagnitude > TSMath.Epsilon * TSMath.Epsilon;
			if (flag3)
			{
				normal.Normalize();
			}
			GJKCollide.simplexSolverPool.GiveBack(@new);
			return true;
		}

		public static bool Raycast(ISupportMappable support, ref TSMatrix orientation, ref TSMatrix invOrientation, ref TSVector position, ref TSVector origin, ref TSVector direction, out FP fraction, out TSVector normal)
		{
			VoronoiSimplexSolver @new = GJKCollide.simplexSolverPool.GetNew();
			@new.Reset();
			normal = TSVector.zero;
			fraction = FP.MaxValue;
			FP fP = FP.Zero;
			TSVector tSVector = direction;
			TSVector p = origin;
			TSVector tSVector2;
			GJKCollide.SupportMapTransformed(support, ref orientation, ref position, ref tSVector, out tSVector2);
			TSVector tSVector3;
			TSVector.Subtract(ref p, ref tSVector2, out tSVector3);
			int num = 15;
			FP x = tSVector3.sqrMagnitude;
			FP eN = FP.EN6;
			bool result;
			while (x > eN && num-- != 0)
			{
				TSVector q;
				GJKCollide.SupportMapTransformed(support, ref orientation, ref position, ref tSVector3, out q);
				TSVector w;
				TSVector.Subtract(ref p, ref q, out w);
				FP x2 = TSVector.Dot(ref tSVector3, ref w);
				bool flag = x2 > FP.Zero;
				if (flag)
				{
					FP fP2 = TSVector.Dot(ref tSVector3, ref tSVector);
					bool flag2 = fP2 >= -TSMath.Epsilon;
					if (flag2)
					{
						GJKCollide.simplexSolverPool.GiveBack(@new);
						result = false;
						return result;
					}
					fP -= x2 / fP2;
					TSVector.Multiply(ref tSVector, fP, out p);
					TSVector.Add(ref origin, ref p, out p);
					TSVector.Subtract(ref p, ref q, out w);
					normal = tSVector3;
				}
				bool flag3 = !@new.InSimplex(w);
				if (flag3)
				{
					@new.AddVertex(w, p, q);
				}
				bool flag4 = @new.Closest(out tSVector3);
				if (flag4)
				{
					x = tSVector3.sqrMagnitude;
				}
				else
				{
					x = FP.Zero;
				}
			}
			TSVector tSVector4;
			TSVector value;
			@new.ComputePoints(out tSVector4, out value);
			value -= origin;
			fraction = value.magnitude / direction.magnitude;
			bool flag5 = normal.sqrMagnitude > TSMath.Epsilon * TSMath.Epsilon;
			if (flag5)
			{
				normal.Normalize();
			}
			GJKCollide.simplexSolverPool.GiveBack(@new);
			result = true;
			return result;
		}
	}
}
