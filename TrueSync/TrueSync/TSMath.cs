namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class TSMath
    {
        public static FP Deg2Rad = FP.Deg2Rad;
        public static FP Epsilon = FP.Epsilon;
        public static FP Pi = FP.Pi;
        public static FP PiOver2 = FP.PiOver2;
        public static FP Rad2Deg = FP.Rad2Deg;

        public static FP Abs(FP value)
        {
            return FP.Abs(value);
        }

        public static void Absolute(ref TSMatrix matrix, out TSMatrix result)
        {
            result.M11 = FP.Abs(matrix.M11);
            result.M12 = FP.Abs(matrix.M12);
            result.M13 = FP.Abs(matrix.M13);
            result.M21 = FP.Abs(matrix.M21);
            result.M22 = FP.Abs(matrix.M22);
            result.M23 = FP.Abs(matrix.M23);
            result.M31 = FP.Abs(matrix.M31);
            result.M32 = FP.Abs(matrix.M32);
            result.M33 = FP.Abs(matrix.M33);
        }

        public static FP Acos(FP value)
        {
            return FP.Acos(value);
        }

        public static FP Asin(FP value)
        {
            return FP.Asin(value);
        }

        public static FP Atan(FP value)
        {
            return FP.Atan(value);
        }

        public static FP Atan2(FP y, FP x)
        {
            return FP.Atan2(y, x);
        }

        public static FP Ceiling(FP value)
        {
            return value;
        }

        public static FP Clamp(FP value, FP min, FP max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static FP Cos(FP value)
        {
            return FP.Cos(value);
        }

        public static FP Floor(FP value)
        {
            return FP.Floor(value);
        }

        public static FP Max(FP val1, FP val2)
        {
            return ((val1 > val2) ? val1 : val2);
        }

        public static FP Max(FP val1, FP val2, FP val3)
        {
            FP fp = (val1 > val2) ? val1 : val2;
            return ((fp > val3) ? fp : val3);
        }

        public static FP Min(FP val1, FP val2)
        {
            return ((val1 < val2) ? val1 : val2);
        }

        public static FP Round(FP value)
        {
            return FP.Round(value);
        }

        public static int Sign(FP value)
        {
            return FP.Sign(value);
        }

        public static FP Sin(FP value)
        {
            return FP.Sin(value);
        }

        public static FP Sqrt(FP number)
        {
            return FP.Sqrt(number);
        }

        public static FP Tan(FP value)
        {
            return FP.Tan(value);
        }
    }
}

