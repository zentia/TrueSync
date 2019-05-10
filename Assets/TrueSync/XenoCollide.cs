using System;
using UnityEngine;

namespace TrueSync
{
	public sealed class XenoCollide
	{
		private static FP CollideEpsilon = FP.EN4;

		private const int MaximumIterations = 34;

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

		public static bool Detect(ISupportMappable support1, ISupportMappable support2, ref TSMatrix orientation1, ref TSMatrix orientation2, ref TSVector position1, ref TSVector position2, out TSVector point, out TSVector normal, out FP penetration)
		{
			TSVector zero = TSVector.zero;
			TSVector zero2 = TSVector.zero;
			TSVector zero3 = TSVector.zero;
			point = (normal = TSVector.zero);
			penetration = FP.Zero;
			TSVector tSVector;
			support1.SupportCenter(out tSVector);
			TSVector.Transform(ref tSVector, ref orientation1, out tSVector);
			TSVector.Add(ref position1, ref tSVector, out tSVector);
			TSVector tSVector2;
			support2.SupportCenter(out tSVector2);
			TSVector.Transform(ref tSVector2, ref orientation2, out tSVector2);
			TSVector.Add(ref position2, ref tSVector2, out tSVector2);
			TSVector tSVector3;
			TSVector.Subtract(ref tSVector2, ref tSVector, out tSVector3);
			bool flag = tSVector3.IsNearlyZero();
			if (flag)
			{
				tSVector3 = new TSVector(FP.EN4, 0, 0);
			}
			TSVector tSVector4 = tSVector3;
			TSVector.Negate(ref tSVector3, out normal);
			TSVector tSVector5;
			XenoCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref tSVector4, out tSVector5);
			TSVector tSVector6;
			XenoCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out tSVector6);
			TSVector tSVector7;
			TSVector.Subtract(ref tSVector6, ref tSVector5, out tSVector7);
			bool flag2 = TSVector.Dot(ref tSVector7, ref normal) <= FP.Zero;
			bool result;
			if (flag2)
			{
				result = false;
			}
			else
			{
				TSVector.Cross(ref tSVector7, ref tSVector3, out normal);
				bool flag3 = normal.IsNearlyZero();
				if (flag3)
				{
					TSVector.Subtract(ref tSVector7, ref tSVector3, out normal);
					normal.Normalize();
					point = tSVector5;
					TSVector.Add(ref point, ref tSVector6, out point);
					TSVector.Multiply(ref point, FP.Half, out point);
					TSVector tSVector8;
					TSVector.Subtract(ref tSVector6, ref tSVector5, out tSVector8);
					penetration = TSVector.Dot(ref tSVector8, ref normal);
					result = true;
				}
				else
				{
					TSVector.Negate(ref normal, out tSVector4);
					TSVector tSVector9;
					XenoCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref tSVector4, out tSVector9);
					TSVector tSVector10;
					XenoCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out tSVector10);
					TSVector tSVector11;
					TSVector.Subtract(ref tSVector10, ref tSVector9, out tSVector11);
					bool flag4 = TSVector.Dot(ref tSVector11, ref normal) <= FP.Zero;
					if (flag4)
					{
						result = false;
					}
					else
					{
						TSVector tSVector8;
						TSVector.Subtract(ref tSVector7, ref tSVector3, out tSVector8);
						TSVector tSVector12;
						TSVector.Subtract(ref tSVector11, ref tSVector3, out tSVector12);
						TSVector.Cross(ref tSVector8, ref tSVector12, out normal);
						FP x = TSVector.Dot(ref normal, ref tSVector3);
						bool flag5 = x > FP.Zero;
						if (flag5)
						{
							TSVector.Swap(ref tSVector7, ref tSVector11);
							TSVector.Swap(ref tSVector5, ref tSVector9);
							TSVector.Swap(ref tSVector6, ref tSVector10);
							TSVector.Negate(ref normal, out normal);
							Debug.Log("normal: " + normal);
						}
						int num = 0;
						int num2 = 0;
						bool flag6 = false;
						while (true)
						{
							bool flag7 = num2 > 34;
							if (flag7)
							{
								break;
							}
							num2++;
							TSVector.Negate(ref normal, out tSVector4);
							TSVector tSVector13;
							XenoCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref tSVector4, out tSVector13);
							TSVector tSVector14;
							XenoCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out tSVector14);
							TSVector tSVector15;
							TSVector.Subtract(ref tSVector14, ref tSVector13, out tSVector15);
							bool flag8 = TSVector.Dot(ref tSVector15, ref normal) <= FP.Zero;
							if (flag8)
							{
								goto Block_7;
							}
							TSVector.Cross(ref tSVector7, ref tSVector15, out tSVector8);
							bool flag9 = TSVector.Dot(ref tSVector8, ref tSVector3) < FP.Zero;
							if (flag9)
							{
								tSVector11 = tSVector15;
								tSVector9 = tSVector13;
								tSVector10 = tSVector14;
								TSVector.Subtract(ref tSVector7, ref tSVector3, out tSVector8);
								TSVector.Subtract(ref tSVector15, ref tSVector3, out tSVector12);
								TSVector.Cross(ref tSVector8, ref tSVector12, out normal);
							}
							else
							{
								TSVector.Cross(ref tSVector15, ref tSVector11, out tSVector8);
								bool flag10 = TSVector.Dot(ref tSVector8, ref tSVector3) < FP.Zero;
								if (!flag10)
								{
									goto IL_385;
								}
								tSVector7 = tSVector15;
								tSVector5 = tSVector13;
								tSVector6 = tSVector14;
								TSVector.Subtract(ref tSVector15, ref tSVector3, out tSVector8);
								TSVector.Subtract(ref tSVector11, ref tSVector3, out tSVector12);
								TSVector.Cross(ref tSVector8, ref tSVector12, out normal);
							}
						}
						result = false;
						return result;
						Block_7:
						result = false;
						return result;
						IL_385:
						while (true)
						{
							num++;
							TSVector.Subtract(ref tSVector11, ref tSVector7, out tSVector8);
							TSVector tSVector15;
							TSVector.Subtract(ref tSVector15, ref tSVector7, out tSVector12);
							TSVector.Cross(ref tSVector8, ref tSVector12, out normal);
							bool flag11 = normal.IsNearlyZero();
							if (flag11)
							{
								break;
							}
							normal.Normalize();
							FP x2 = TSVector.Dot(ref normal, ref tSVector7);
							bool flag12 = x2 >= 0 && !flag6;
							if (flag12)
							{
								flag6 = true;
							}
							TSVector.Negate(ref normal, out tSVector4);
							XenoCollide.SupportMapTransformed(support1, ref orientation1, ref position1, ref tSVector4, out zero);
							XenoCollide.SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out zero2);
							TSVector.Subtract(ref zero2, ref zero, out zero3);
							TSVector.Subtract(ref zero3, ref tSVector15, out tSVector8);
							FP x3 = TSVector.Dot(ref tSVector8, ref normal);
							penetration = TSVector.Dot(ref zero3, ref normal);
							bool flag13 = x3 <= XenoCollide.CollideEpsilon || penetration <= FP.Zero || num > 34;
							if (flag13)
							{
								goto Block_15;
							}
							TSVector.Cross(ref zero3, ref tSVector3, out tSVector8);
							FP x4 = TSVector.Dot(ref tSVector8, ref tSVector7);
							bool flag14 = x4 >= FP.Zero;
							if (flag14)
							{
								x4 = TSVector.Dot(ref tSVector8, ref tSVector11);
								bool flag15 = x4 >= FP.Zero;
								if (flag15)
								{
									tSVector7 = zero3;
									tSVector5 = zero;
									tSVector6 = zero2;
								}
								else
								{
									tSVector15 = zero3;
									TSVector tSVector13 = zero;
									TSVector tSVector14 = zero2;
								}
							}
							else
							{
								x4 = TSVector.Dot(ref tSVector8, ref tSVector15);
								bool flag16 = x4 >= FP.Zero;
								if (flag16)
								{
									tSVector11 = zero3;
									tSVector9 = zero;
									tSVector10 = zero2;
								}
								else
								{
									tSVector7 = zero3;
									tSVector5 = zero;
									tSVector6 = zero2;
								}
							}
						}
						result = true;
						return result;
						Block_15:
						bool flag17 = flag6;
						if (flag17)
						{
							TSVector.Cross(ref tSVector7, ref tSVector11, out tSVector8);
							TSVector tSVector15;
							FP fP = TSVector.Dot(ref tSVector8, ref tSVector15);
							TSVector.Cross(ref tSVector15, ref tSVector11, out tSVector8);
							FP fP2 = TSVector.Dot(ref tSVector8, ref tSVector3);
							TSVector.Cross(ref tSVector3, ref tSVector7, out tSVector8);
							FP fP3 = TSVector.Dot(ref tSVector8, ref tSVector15);
							TSVector.Cross(ref tSVector11, ref tSVector7, out tSVector8);
							FP fP4 = TSVector.Dot(ref tSVector8, ref tSVector3);
							FP fP5 = fP + fP2 + fP3 + fP4;
							bool flag18 = fP5 <= 0;
							if (flag18)
							{
								fP = 0;
								TSVector.Cross(ref tSVector11, ref tSVector15, out tSVector8);
								fP2 = TSVector.Dot(ref tSVector8, ref normal);
								TSVector.Cross(ref tSVector15, ref tSVector7, out tSVector8);
								fP3 = TSVector.Dot(ref tSVector8, ref normal);
								TSVector.Cross(ref tSVector7, ref tSVector11, out tSVector8);
								fP4 = TSVector.Dot(ref tSVector8, ref normal);
								fP5 = fP2 + fP3 + fP4;
							}
							FP x5 = FP.One / fP5;
							TSVector.Multiply(ref tSVector, fP, out point);
							TSVector.Multiply(ref tSVector5, fP2, out tSVector8);
							TSVector.Add(ref point, ref tSVector8, out point);
							TSVector.Multiply(ref tSVector9, fP3, out tSVector8);
							TSVector.Add(ref point, ref tSVector8, out point);
							TSVector tSVector13;
							TSVector.Multiply(ref tSVector13, fP4, out tSVector8);
							TSVector.Add(ref point, ref tSVector8, out point);
							TSVector.Multiply(ref tSVector2, fP, out tSVector12);
							TSVector.Add(ref tSVector12, ref point, out point);
							TSVector.Multiply(ref tSVector6, fP2, out tSVector8);
							TSVector.Add(ref point, ref tSVector8, out point);
							TSVector.Multiply(ref tSVector10, fP3, out tSVector8);
							TSVector.Add(ref point, ref tSVector8, out point);
							TSVector tSVector14;
							TSVector.Multiply(ref tSVector14, fP4, out tSVector8);
							TSVector.Add(ref point, ref tSVector8, out point);
							TSVector.Multiply(ref point, x5 * FP.Half, out point);
						}
						result = flag6;
					}
				}
			}
			return result;
		}
	}
}
