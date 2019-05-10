using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class TimeOfImpact
	{
		[ThreadStatic]
		public static int TOICalls;

		[ThreadStatic]
		public static int TOIIters;

		[ThreadStatic]
		public static int TOIMaxIters;

		[ThreadStatic]
		public static int TOIRootIters;

		[ThreadStatic]
		public static int TOIMaxRootIters;

		[ThreadStatic]
		private static DistanceInput _distanceInput;

		public static void CalculateTimeOfImpact(out TOIOutput output, TOIInput input)
		{
			output = default(TOIOutput);
			output.State = TOIOutputState.Unknown;
			output.T = input.TMax;
			Sweep sweepA = input.SweepA;
			Sweep sweepB = input.SweepB;
			sweepA.Normalize();
			sweepB.Normalize();
			FP tMax = input.TMax;
			FP x = input.ProxyA.Radius + input.ProxyB.Radius;
			FP fP = TSMath.Max(Settings.LinearSlop, x - 3f * Settings.LinearSlop);
			FP y = 0.25f * Settings.LinearSlop;
			Debug.Assert(fP > y);
			FP fP2 = 0f;
			int num = 0;
			TimeOfImpact._distanceInput = (TimeOfImpact._distanceInput ?? new DistanceInput());
			TimeOfImpact._distanceInput.ProxyA = input.ProxyA;
			TimeOfImpact._distanceInput.ProxyB = input.ProxyB;
			TimeOfImpact._distanceInput.UseRadii = false;
			while (true)
			{
				Transform transformA;
				sweepA.GetTransform(out transformA, fP2);
				Transform transformB;
				sweepB.GetTransform(out transformB, fP2);
				TimeOfImpact._distanceInput.TransformA = transformA;
				TimeOfImpact._distanceInput.TransformB = transformB;
				DistanceOutput distanceOutput;
				SimplexCache simplexCache;
				Distance.ComputeDistance(out distanceOutput, out simplexCache, TimeOfImpact._distanceInput);
				bool flag = distanceOutput.Distance <= 0f;
				if (flag)
				{
					break;
				}
				bool flag2 = distanceOutput.Distance < fP + y;
				if (flag2)
				{
					goto Block_3;
				}
				SeparationFunction.Set(ref simplexCache, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, fP2);
				bool flag3 = false;
				FP fP3 = tMax;
				int num2 = 0;
				while (true)
				{
					int indexA;
					int indexB;
					FP x2 = SeparationFunction.FindMinSeparation(out indexA, out indexB, fP3);
					bool flag4 = x2 > fP + y;
					if (flag4)
					{
						goto Block_4;
					}
					bool flag5 = x2 > fP - y;
					if (flag5)
					{
						goto Block_5;
					}
					FP fP4 = SeparationFunction.Evaluate(indexA, indexB, fP2);
					bool flag6 = fP4 < fP - y;
					if (flag6)
					{
						goto Block_6;
					}
					bool flag7 = fP4 <= fP + y;
					if (flag7)
					{
						goto Block_7;
					}
					int num3 = 0;
					FP fP5 = fP2;
					FP fP6 = fP3;
					FP fP7;
					bool flag11;
					do
					{
						bool flag8 = (num3 & 1) != 0;
						if (flag8)
						{
							fP7 = fP5 + (fP - fP4) * (fP6 - fP5) / (x2 - fP4);
						}
						else
						{
							fP7 = 0.5f * (fP5 + fP6);
						}
						num3++;
						FP fP8 = SeparationFunction.Evaluate(indexA, indexB, fP7);
						bool flag9 = FP.Abs(fP8 - fP) < y;
						if (flag9)
						{
							goto Block_9;
						}
						bool flag10 = fP8 > fP;
						if (flag10)
						{
							fP5 = fP7;
							fP4 = fP8;
						}
						else
						{
							fP6 = fP7;
							x2 = fP8;
						}
						flag11 = (num3 == 50);
					}
					while (!flag11);
					IL_373:
					num2++;
					bool flag12 = num2 == Settings.MaxPolygonVertices;
					if (flag12)
					{
						break;
					}
					continue;
					Block_9:
					fP3 = fP7;
					goto IL_373;
				}
				IL_396:
				num++;
				bool flag13 = flag3;
				if (flag13)
				{
					goto Block_13;
				}
				bool flag14 = num == 20;
				if (flag14)
				{
					goto Block_14;
				}
				continue;
				Block_4:
				output.State = TOIOutputState.Seperated;
				output.T = tMax;
				flag3 = true;
				goto IL_396;
				Block_5:
				fP2 = fP3;
				goto IL_396;
				Block_6:
				output.State = TOIOutputState.Failed;
				output.T = fP2;
				flag3 = true;
				goto IL_396;
				Block_7:
				output.State = TOIOutputState.Touching;
				output.T = fP2;
				flag3 = true;
				goto IL_396;
			}
			output.State = TOIOutputState.Overlapped;
			output.T = 0f;
			return;
			Block_3:
			output.State = TOIOutputState.Touching;
			output.T = fP2;
			Block_13:
			return;
			Block_14:
			output.State = TOIOutputState.Failed;
			output.T = fP2;
		}
	}
}
