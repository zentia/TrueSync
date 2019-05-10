using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class SeparationFunction
	{
		[ThreadStatic]
		private static TSVector2 _axis;

		[ThreadStatic]
		private static TSVector2 _localPoint;

		[ThreadStatic]
		private static DistanceProxy _proxyA;

		[ThreadStatic]
		private static DistanceProxy _proxyB;

		[ThreadStatic]
		private static Sweep _sweepA;

		[ThreadStatic]
		private static Sweep _sweepB;

		[ThreadStatic]
		private static SeparationFunctionType _type;

		public static void Set(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, FP t1)
		{
			SeparationFunction._localPoint = TSVector2.zero;
			SeparationFunction._proxyA = proxyA;
			SeparationFunction._proxyB = proxyB;
			int count = (int)cache.Count;
			Debug.Assert(0 < count && count < 3);
			SeparationFunction._sweepA = sweepA;
			SeparationFunction._sweepB = sweepB;
			Transform transform;
			SeparationFunction._sweepA.GetTransform(out transform, t1);
			Transform transform2;
			SeparationFunction._sweepB.GetTransform(out transform2, t1);
			bool flag = count == 1;
			if (flag)
			{
				SeparationFunction._type = SeparationFunctionType.Points;
				TSVector2 v = SeparationFunction._proxyA.Vertices[(int)cache.IndexA[0]];
				TSVector2 v2 = SeparationFunction._proxyB.Vertices[(int)cache.IndexB[0]];
				TSVector2 value = MathUtils.Mul(ref transform, v);
				TSVector2 value2 = MathUtils.Mul(ref transform2, v2);
				SeparationFunction._axis = value2 - value;
				SeparationFunction._axis.Normalize();
			}
			else
			{
				bool flag2 = cache.IndexA[0] == cache.IndexA[1];
				if (flag2)
				{
					SeparationFunction._type = SeparationFunctionType.FaceB;
					TSVector2 tSVector = proxyB.Vertices[(int)cache.IndexB[0]];
					TSVector2 tSVector2 = proxyB.Vertices[(int)cache.IndexB[1]];
					TSVector2 tSVector3 = tSVector2 - tSVector;
					SeparationFunction._axis = new TSVector2(tSVector3.y, -tSVector3.x);
					SeparationFunction._axis.Normalize();
					TSVector2 value3 = MathUtils.Mul(ref transform2.q, SeparationFunction._axis);
					SeparationFunction._localPoint = 0.5f * (tSVector + tSVector2);
					TSVector2 value4 = MathUtils.Mul(ref transform2, SeparationFunction._localPoint);
					TSVector2 v3 = proxyA.Vertices[(int)cache.IndexA[0]];
					TSVector2 value5 = MathUtils.Mul(ref transform, v3);
					FP x = TSVector2.Dot(value5 - value4, value3);
					bool flag3 = x < 0f;
					if (flag3)
					{
						SeparationFunction._axis = -SeparationFunction._axis;
					}
				}
				else
				{
					SeparationFunction._type = SeparationFunctionType.FaceA;
					TSVector2 tSVector4 = SeparationFunction._proxyA.Vertices[(int)cache.IndexA[0]];
					TSVector2 tSVector5 = SeparationFunction._proxyA.Vertices[(int)cache.IndexA[1]];
					TSVector2 tSVector6 = tSVector5 - tSVector4;
					SeparationFunction._axis = new TSVector2(tSVector6.y, -tSVector6.x);
					SeparationFunction._axis.Normalize();
					TSVector2 value6 = MathUtils.Mul(ref transform.q, SeparationFunction._axis);
					SeparationFunction._localPoint = 0.5f * (tSVector4 + tSVector5);
					TSVector2 value7 = MathUtils.Mul(ref transform, SeparationFunction._localPoint);
					TSVector2 v4 = SeparationFunction._proxyB.Vertices[(int)cache.IndexB[0]];
					TSVector2 value8 = MathUtils.Mul(ref transform2, v4);
					FP x2 = TSVector2.Dot(value8 - value7, value6);
					bool flag4 = x2 < 0f;
					if (flag4)
					{
						SeparationFunction._axis = -SeparationFunction._axis;
					}
				}
			}
		}

		public static FP FindMinSeparation(out int indexA, out int indexB, FP t)
		{
			Transform transform;
			SeparationFunction._sweepA.GetTransform(out transform, t);
			Transform transform2;
			SeparationFunction._sweepB.GetTransform(out transform2, t);
			FP result;
			switch (SeparationFunction._type)
			{
			case SeparationFunctionType.Points:
			{
				TSVector2 direction = MathUtils.MulT(ref transform.q, SeparationFunction._axis);
				TSVector2 direction2 = MathUtils.MulT(ref transform2.q, -SeparationFunction._axis);
				indexA = SeparationFunction._proxyA.GetSupport(direction);
				indexB = SeparationFunction._proxyB.GetSupport(direction2);
				TSVector2 v = SeparationFunction._proxyA.Vertices[indexA];
				TSVector2 v2 = SeparationFunction._proxyB.Vertices[indexB];
				TSVector2 value = MathUtils.Mul(ref transform, v);
				TSVector2 value2 = MathUtils.Mul(ref transform2, v2);
				FP fP = TSVector2.Dot(value2 - value, SeparationFunction._axis);
				result = fP;
				break;
			}
			case SeparationFunctionType.FaceA:
			{
				TSVector2 tSVector = MathUtils.Mul(ref transform.q, SeparationFunction._axis);
				TSVector2 value3 = MathUtils.Mul(ref transform, SeparationFunction._localPoint);
				TSVector2 direction3 = MathUtils.MulT(ref transform2.q, -tSVector);
				indexA = -1;
				indexB = SeparationFunction._proxyB.GetSupport(direction3);
				TSVector2 v3 = SeparationFunction._proxyB.Vertices[indexB];
				TSVector2 value4 = MathUtils.Mul(ref transform2, v3);
				FP fP2 = TSVector2.Dot(value4 - value3, tSVector);
				result = fP2;
				break;
			}
			case SeparationFunctionType.FaceB:
			{
				TSVector2 tSVector2 = MathUtils.Mul(ref transform2.q, SeparationFunction._axis);
				TSVector2 value5 = MathUtils.Mul(ref transform2, SeparationFunction._localPoint);
				TSVector2 direction4 = MathUtils.MulT(ref transform.q, -tSVector2);
				indexB = -1;
				indexA = SeparationFunction._proxyA.GetSupport(direction4);
				TSVector2 v4 = SeparationFunction._proxyA.Vertices[indexA];
				TSVector2 value6 = MathUtils.Mul(ref transform, v4);
				FP fP3 = TSVector2.Dot(value6 - value5, tSVector2);
				result = fP3;
				break;
			}
			default:
				Debug.Assert(false);
				indexA = -1;
				indexB = -1;
				result = 0f;
				break;
			}
			return result;
		}

		public static FP Evaluate(int indexA, int indexB, FP t)
		{
			Transform transform;
			SeparationFunction._sweepA.GetTransform(out transform, t);
			Transform transform2;
			SeparationFunction._sweepB.GetTransform(out transform2, t);
			FP result;
			switch (SeparationFunction._type)
			{
			case SeparationFunctionType.Points:
			{
				TSVector2 v = SeparationFunction._proxyA.Vertices[indexA];
				TSVector2 v2 = SeparationFunction._proxyB.Vertices[indexB];
				TSVector2 value = MathUtils.Mul(ref transform, v);
				TSVector2 value2 = MathUtils.Mul(ref transform2, v2);
				FP fP = TSVector2.Dot(value2 - value, SeparationFunction._axis);
				result = fP;
				break;
			}
			case SeparationFunctionType.FaceA:
			{
				TSVector2 value3 = MathUtils.Mul(ref transform.q, SeparationFunction._axis);
				TSVector2 value4 = MathUtils.Mul(ref transform, SeparationFunction._localPoint);
				TSVector2 v3 = SeparationFunction._proxyB.Vertices[indexB];
				TSVector2 value5 = MathUtils.Mul(ref transform2, v3);
				FP fP2 = TSVector2.Dot(value5 - value4, value3);
				result = fP2;
				break;
			}
			case SeparationFunctionType.FaceB:
			{
				TSVector2 value6 = MathUtils.Mul(ref transform2.q, SeparationFunction._axis);
				TSVector2 value7 = MathUtils.Mul(ref transform2, SeparationFunction._localPoint);
				TSVector2 v4 = SeparationFunction._proxyA.Vertices[indexA];
				TSVector2 value8 = MathUtils.Mul(ref transform, v4);
				FP fP3 = TSVector2.Dot(value8 - value7, value6);
				result = fP3;
				break;
			}
			default:
				Debug.Assert(false);
				result = 0f;
				break;
			}
			return result;
		}
	}
}
