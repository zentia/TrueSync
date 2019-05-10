using System;

namespace TrueSync.Physics2D
{
	public static class LineTools
	{
		public static FP DistanceBetweenPointAndLineSegment(ref TSVector2 point, ref TSVector2 start, ref TSVector2 end)
		{
			bool flag = start == end;
			FP result;
			if (flag)
			{
				result = TSVector2.Distance(point, start);
			}
			else
			{
				TSVector2 tSVector = TSVector2.Subtract(end, start);
				TSVector2 value = TSVector2.Subtract(point, start);
				FP fP = TSVector2.Dot(value, tSVector);
				bool flag2 = fP <= 0;
				if (flag2)
				{
					result = TSVector2.Distance(point, start);
				}
				else
				{
					FP fP2 = TSVector2.Dot(tSVector, tSVector);
					bool flag3 = fP2 <= fP;
					if (flag3)
					{
						result = TSVector2.Distance(point, end);
					}
					else
					{
						FP scaleFactor = fP / fP2;
						TSVector2 value2 = TSVector2.Add(start, TSVector2.Multiply(tSVector, scaleFactor));
						result = TSVector2.Distance(point, value2);
					}
				}
			}
			return result;
		}

		public static bool LineIntersect2(ref TSVector2 a0, ref TSVector2 a1, ref TSVector2 b0, ref TSVector2 b1, out TSVector2 intersectionPoint)
		{
			intersectionPoint = TSVector2.zero;
			bool flag = a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				FP x = a0.x;
				FP y = a0.y;
				FP x2 = a1.x;
				FP y2 = a1.y;
				FP x3 = b0.x;
				FP y3 = b0.y;
				FP x4 = b1.x;
				FP y4 = b1.y;
				bool flag2 = TSMath.Max(x, x2) < TSMath.Min(x3, x4) || TSMath.Max(x3, x4) < TSMath.Min(x, x2);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = TSMath.Max(y, y2) < TSMath.Min(y3, y4) || TSMath.Max(y3, y4) < TSMath.Min(y, y2);
					if (flag3)
					{
						result = false;
					}
					else
					{
						FP fP = (x4 - x3) * (y - y3) - (y4 - y3) * (x - x3);
						FP fP2 = (x2 - x) * (y - y3) - (y2 - y) * (x - x3);
						FP fP3 = (y4 - y3) * (x2 - x) - (x4 - x3) * (y2 - y);
						bool flag4 = FP.Abs(fP3) < Settings.Epsilon;
						if (flag4)
						{
							result = false;
						}
						else
						{
							fP /= fP3;
							fP2 /= fP3;
							bool flag5 = 0 < fP && fP < 1 && 0 < fP2 && fP2 < 1;
							if (flag5)
							{
								intersectionPoint.x = x + fP * (x2 - x);
								intersectionPoint.y = y + fP * (y2 - y);
								result = true;
							}
							else
							{
								result = false;
							}
						}
					}
				}
			}
			return result;
		}

		public static TSVector2 LineIntersect(TSVector2 p1, TSVector2 p2, TSVector2 q1, TSVector2 q2)
		{
			TSVector2 zero = TSVector2.zero;
			FP x = p2.y - p1.y;
			FP fP = p1.x - p2.x;
			FP y = x * p1.x + fP * p1.y;
			FP x2 = q2.y - q1.y;
			FP fP2 = q1.x - q2.x;
			FP y2 = x2 * q1.x + fP2 * q1.y;
			FP fP3 = x * fP2 - x2 * fP;
			bool flag = !MathUtils.FPEquals(fP3, 0);
			if (flag)
			{
				zero.x = (fP2 * y - fP * y2) / fP3;
				zero.y = (x * y2 - x2 * y) / fP3;
			}
			return zero;
		}

		public static bool LineIntersect(ref TSVector2 point1, ref TSVector2 point2, ref TSVector2 point3, ref TSVector2 point4, bool firstIsSegment, bool secondIsSegment, out TSVector2 point)
		{
			point = default(TSVector2);
			FP x = point4.y - point3.y;
			FP fP = point2.x - point1.x;
			FP x2 = point4.x - point3.x;
			FP fP2 = point2.y - point1.y;
			FP fP3 = x * fP - x2 * fP2;
			bool flag = !(fP3 >= -Settings.Epsilon) || !(fP3 <= Settings.Epsilon);
			bool result;
			if (flag)
			{
				FP y = point1.y - point3.y;
				FP y2 = point1.x - point3.x;
				FP y3 = 1f / fP3;
				FP x3 = x2 * y - x * y2;
				x3 *= y3;
				bool flag2 = !firstIsSegment || (x3 >= 0f && x3 <= 1f);
				if (flag2)
				{
					FP x4 = fP * y - fP2 * y2;
					x4 *= y3;
					bool flag3 = !secondIsSegment || (x4 >= 0f && x4 <= 1f);
					if (flag3)
					{
						bool flag4 = x3 != 0f || x4 != 0f;
						if (flag4)
						{
							point.x = point1.x + x3 * fP;
							point.y = point1.y + x3 * fP2;
							result = true;
							return result;
						}
					}
				}
			}
			result = false;
			return result;
		}

		public static bool LineIntersect(TSVector2 point1, TSVector2 point2, TSVector2 point3, TSVector2 point4, bool firstIsSegment, bool secondIsSegment, out TSVector2 intersectionPoint)
		{
			return LineTools.LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
		}

		public static bool LineIntersect(ref TSVector2 point1, ref TSVector2 point2, ref TSVector2 point3, ref TSVector2 point4, out TSVector2 intersectionPoint)
		{
			return LineTools.LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
		}

		public static bool LineIntersect(TSVector2 point1, TSVector2 point2, TSVector2 point3, TSVector2 point4, out TSVector2 intersectionPoint)
		{
			return LineTools.LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
		}

		public static Vertices LineSegmentVerticesIntersect(ref TSVector2 point1, ref TSVector2 point2, Vertices vertices)
		{
			Vertices vertices2 = new Vertices();
			for (int i = 0; i < vertices.Count; i++)
			{
				TSVector2 item;
				bool flag = LineTools.LineIntersect(vertices[i], vertices[vertices.NextIndex(i)], point1, point2, true, true, out item);
				if (flag)
				{
					vertices2.Add(item);
				}
			}
			return vertices2;
		}

		public static Vertices LineSegmentAABBIntersect(ref TSVector2 point1, ref TSVector2 point2, AABB aabb)
		{
			return LineTools.LineSegmentVerticesIntersect(ref point1, ref point2, aabb.Vertices);
		}
	}
}
