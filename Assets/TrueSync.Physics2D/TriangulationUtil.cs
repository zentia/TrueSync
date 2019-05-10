using System;

namespace TrueSync.Physics2D
{
	internal class TriangulationUtil
	{
		public static FP EPSILON = 1E-12;

		public static bool SmartIncircle(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
		{
			FP x = pd.X;
			FP y = pd.Y;
			FP fP = pa.X - x;
			FP fP2 = pa.Y - y;
			FP fP3 = pb.X - x;
			FP fP4 = pb.Y - y;
			FP x2 = fP * fP4;
			FP y2 = fP3 * fP2;
			FP fP5 = x2 - y2;
			bool flag = fP5 <= 0;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				FP fP6 = pc.X - x;
				FP fP7 = pc.Y - y;
				FP x3 = fP6 * fP2;
				FP y3 = fP * fP7;
				FP fP8 = x3 - y3;
				bool flag2 = fP8 <= 0;
				if (flag2)
				{
					result = false;
				}
				else
				{
					FP x4 = fP3 * fP7;
					FP y4 = fP6 * fP4;
					FP x5 = fP * fP + fP2 * fP2;
					FP x6 = fP3 * fP3 + fP4 * fP4;
					FP x7 = fP6 * fP6 + fP7 * fP7;
					FP x8 = x5 * (x4 - y4) + x6 * fP8 + x7 * fP5;
					result = (x8 > 0);
				}
			}
			return result;
		}

		public static bool InScanArea(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
		{
			FP x = (pa.X - pb.X) * (pd.Y - pb.Y) - (pd.X - pb.X) * (pa.Y - pb.Y);
			bool flag = x >= -TriangulationUtil.EPSILON;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				FP x2 = (pa.X - pc.X) * (pd.Y - pc.Y) - (pd.X - pc.X) * (pa.Y - pc.Y);
				bool flag2 = x2 <= TriangulationUtil.EPSILON;
				result = !flag2;
			}
			return result;
		}

		public static Orientation Orient2d(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc)
		{
			FP x = (pa.X - pc.X) * (pb.Y - pc.Y);
			FP y = (pa.Y - pc.Y) * (pb.X - pc.X);
			FP x2 = x - y;
			bool flag = x2 > -TriangulationUtil.EPSILON && x2 < TriangulationUtil.EPSILON;
			Orientation result;
			if (flag)
			{
				result = Orientation.Collinear;
			}
			else
			{
				bool flag2 = x2 > 0;
				if (flag2)
				{
					result = Orientation.CCW;
				}
				else
				{
					result = Orientation.CW;
				}
			}
			return result;
		}
	}
}
