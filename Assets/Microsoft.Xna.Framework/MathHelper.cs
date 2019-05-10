using System;
using TrueSync;

namespace Microsoft.Xna.Framework
{
	public static class MathHelper
	{
		public static FP E = 2.7182818284590451;

		public static FP Log10E = 0.4342945f;

		public static FP Log2E = 1.442695f;

		public static FP Pi = FP.Pi;

		public static FP PiOver2 = FP.Pi / 2.0;

		public static FP PiOver4 = FP.Pi / 4.0;

		public static FP TwoPi = FP.Pi * 2.0;

		public static FP Barycentric(FP value1, FP value2, FP value3, FP amount1, FP amount2)
		{
			return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
		}

		public static FP CatmullRom(FP value1, FP value2, FP value3, FP value4, FP amount)
		{
			FP fP = amount * amount;
			FP y = fP * amount;
			return 0.5 * (2.0 * value2 + (value3 - value1) * amount + (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * fP + (3.0 * value2 - value1 - 3.0 * value3 + value4) * y);
		}

		public static FP Clamp(FP value, FP min, FP max)
		{
			value = ((value > max) ? max : value);
			value = ((value < min) ? min : value);
			return value;
		}

		public static FP Distance(FP value1, FP value2)
		{
			return FP.Abs(value1 - value2);
		}

		public static FP Hermite(FP value1, FP tangent1, FP value2, FP tangent2, FP amount)
		{
			FP y = amount * amount * amount;
			FP y2 = amount * amount;
			bool flag = amount == 0f;
			FP result;
			if (flag)
			{
				result = value1;
			}
			else
			{
				bool flag2 = amount == 1f;
				if (flag2)
				{
					result = value2;
				}
				else
				{
					result = (2 * value1 - 2 * value2 + tangent2 + tangent1) * y + (3 * value2 - 3 * value1 - 2 * tangent1 - tangent2) * y2 + tangent1 * amount + value1;
				}
			}
			return result;
		}

		public static FP Lerp(FP value1, FP value2, FP amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static FP Max(FP value1, FP value2)
		{
			return TSMath.Max(value1, value2);
		}

		public static FP Min(FP value1, FP value2)
		{
			return TSMath.Min(value1, value2);
		}

		public static FP SmoothStep(FP value1, FP value2, FP amount)
		{
			FP amount2 = MathHelper.Clamp(amount, 0f, 1f);
			return MathHelper.Hermite(value1, 0f, value2, 0f, amount2);
		}

		public static FP ToDegrees(FP radians)
		{
			return radians * 57.295779513082323;
		}

		public static FP ToRadians(FP degrees)
		{
			return degrees * 0.017453292519943295;
		}

		public static FP WrapAngle(FP angle)
		{
			angle %= 6.2831854820251465;
			bool flag = angle <= -3.141593f;
			FP result;
			if (flag)
			{
				angle += 6.283185f;
				result = angle;
			}
			else
			{
				bool flag2 = angle > 3.141593f;
				if (flag2)
				{
					angle -= 6.283185f;
				}
				result = angle;
			}
			return result;
		}
	}
}
