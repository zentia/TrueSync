namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class ConvertUnits
    {
        private static FP _displayUnitsToSimUnitsRatio = 100f;
        private static FP _simUnitsToDisplayUnitsRatio = (1 / _displayUnitsToSimUnitsRatio);

        public static void SetDisplayUnitToSimUnitRatio(FP displayUnitsPerSimUnit)
        {
            _displayUnitsToSimUnitsRatio = displayUnitsPerSimUnit;
            _simUnitsToDisplayUnitsRatio = 1 / displayUnitsPerSimUnit;
        }

        public static Vector3 ToDisplayUnits(Vector3 simUnits)
        {
            return (simUnits * _displayUnitsToSimUnitsRatio);
        }

        public static FP ToDisplayUnits(int simUnits)
        {
            return (simUnits * _displayUnitsToSimUnitsRatio);
        }

        public static FP ToDisplayUnits(FP simUnits)
        {
            return (simUnits * _displayUnitsToSimUnitsRatio);
        }

        public static TSVector2 ToDisplayUnits(TSVector2 simUnits)
        {
            return (simUnits * _displayUnitsToSimUnitsRatio);
        }

        public static void ToDisplayUnits(ref TSVector2 simUnits, out TSVector2 displayUnits)
        {
            TSVector2.Multiply(ref simUnits, _displayUnitsToSimUnitsRatio, out displayUnits);
        }

        public static TSVector2 ToDisplayUnits(FP x, FP y)
        {
            return (new TSVector2(x, y) * _displayUnitsToSimUnitsRatio);
        }

        public static void ToDisplayUnits(FP x, FP y, out TSVector2 displayUnits)
        {
            displayUnits = TSVector2.zero;
            displayUnits.x = x * _displayUnitsToSimUnitsRatio;
            displayUnits.y = y * _displayUnitsToSimUnitsRatio;
        }

        public static Vector3 ToSimUnits(Vector3 displayUnits)
        {
            return (displayUnits * _simUnitsToDisplayUnitsRatio);
        }

        public static FP ToSimUnits(int displayUnits)
        {
            return (displayUnits * _simUnitsToDisplayUnitsRatio);
        }

        public static FP ToSimUnits(FP displayUnits)
        {
            return (displayUnits * _simUnitsToDisplayUnitsRatio);
        }

        public static TSVector2 ToSimUnits(TSVector2 displayUnits)
        {
            return (displayUnits * _simUnitsToDisplayUnitsRatio);
        }

        public static void ToSimUnits(ref TSVector2 displayUnits, out TSVector2 simUnits)
        {
            TSVector2.Multiply(ref displayUnits, _simUnitsToDisplayUnitsRatio, out simUnits);
        }

        public static TSVector2 ToSimUnits(FP x, FP y)
        {
            return (new TSVector2(x, y) * _simUnitsToDisplayUnitsRatio);
        }

        public static void ToSimUnits(FP x, FP y, out TSVector2 simUnits)
        {
            simUnits = TSVector2.zero;
            simUnits.x = x * _simUnitsToDisplayUnitsRatio;
            simUnits.y = y * _simUnitsToDisplayUnitsRatio;
        }
    }
}

