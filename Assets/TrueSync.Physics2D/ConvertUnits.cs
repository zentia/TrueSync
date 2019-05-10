using Microsoft.Xna.Framework;
using System;

namespace TrueSync.Physics2D
{
	public static class ConvertUnits
	{
		private static FP _displayUnitsToSimUnitsRatio = 100f;

		private static FP _simUnitsToDisplayUnitsRatio = 1 / ConvertUnits._displayUnitsToSimUnitsRatio;

		public static void SetDisplayUnitToSimUnitRatio(FP displayUnitsPerSimUnit)
		{
			ConvertUnits._displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
			ConvertUnits._simUnitsToDisplayUnitsRatio = 1 / displayUnitsPerSimUnit;
		}

		public static FP ToDisplayUnits(FP simUnits)
		{
			return simUnits * ConvertUnits._displayUnitsToSimUnitsRatio;
		}

		public static FP ToDisplayUnits(int simUnits)
		{
			return simUnits * ConvertUnits._displayUnitsToSimUnitsRatio;
		}

		public static TSVector2 ToDisplayUnits(TSVector2 simUnits)
		{
			return simUnits * ConvertUnits._displayUnitsToSimUnitsRatio;
		}

		public static void ToDisplayUnits(ref TSVector2 simUnits, out TSVector2 displayUnits)
		{
			TSVector2.Multiply(ref simUnits, ConvertUnits._displayUnitsToSimUnitsRatio, out displayUnits);
		}

		public static Vector3 ToDisplayUnits(Vector3 simUnits)
		{
			return simUnits * ConvertUnits._displayUnitsToSimUnitsRatio;
		}

		public static TSVector2 ToDisplayUnits(FP x, FP y)
		{
			return new TSVector2(x, y) * ConvertUnits._displayUnitsToSimUnitsRatio;
		}

		public static void ToDisplayUnits(FP x, FP y, out TSVector2 displayUnits)
		{
			displayUnits = TSVector2.zero;
			displayUnits.x = x * ConvertUnits._displayUnitsToSimUnitsRatio;
			displayUnits.y = y * ConvertUnits._displayUnitsToSimUnitsRatio;
		}

		public static FP ToSimUnits(FP displayUnits)
		{
			return displayUnits * ConvertUnits._simUnitsToDisplayUnitsRatio;
		}

		public static FP ToSimUnits(int displayUnits)
		{
			return displayUnits * ConvertUnits._simUnitsToDisplayUnitsRatio;
		}

		public static TSVector2 ToSimUnits(TSVector2 displayUnits)
		{
			return displayUnits * ConvertUnits._simUnitsToDisplayUnitsRatio;
		}

		public static Vector3 ToSimUnits(Vector3 displayUnits)
		{
			return displayUnits * ConvertUnits._simUnitsToDisplayUnitsRatio;
		}

		public static void ToSimUnits(ref TSVector2 displayUnits, out TSVector2 simUnits)
		{
			TSVector2.Multiply(ref displayUnits, ConvertUnits._simUnitsToDisplayUnitsRatio, out simUnits);
		}

		public static TSVector2 ToSimUnits(FP x, FP y)
		{
			return new TSVector2(x, y) * ConvertUnits._simUnitsToDisplayUnitsRatio;
		}

		public static void ToSimUnits(FP x, FP y, out TSVector2 simUnits)
		{
			simUnits = TSVector2.zero;
			simUnits.x = x * ConvertUnits._simUnitsToDisplayUnitsRatio;
			simUnits.y = y * ConvertUnits._simUnitsToDisplayUnitsRatio;
		}
	}
}
