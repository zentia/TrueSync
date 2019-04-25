namespace Microsoft.Xna.Framework
{
    using System;
    using TrueSync;

    public static class MathHelper
    {
        public static FP E = 2.7182818284590451;
        public static FP Log10E = 0.4342945f;
        public static FP Log2E = 1.442695f;
        public static FP Pi = FP.Pi;
        public static FP PiOver2 = (FP.Pi / 2.0);
        public static FP PiOver4 = (FP.Pi / 4.0);
        public static FP TwoPi = (FP.Pi * 2.0);

        public static FP Barycentric(FP value1, FP value2, FP value3, FP amount1, FP amount2)
        {
            return ((value1 + ((value2 - value1) * amount1)) + ((value3 - value1) * amount2));
        }

        public static FP CatmullRom(FP value1, FP value2, FP value3, FP value4, FP amount)
        {
            FP fp = amount * amount;
            FP fp2 = fp * amount;
            return (0.5 * ((((2.0 * value2) + ((value3 - value1) * amount)) + (((((2.0 * value1) - (5.0 * value2)) + (4.0 * value3)) - value4) * fp)) + (((((3.0 * value2) - value1) - (3.0 * value3)) + value4) * fp2)));
        }

        public static FP Clamp(FP value, FP min, FP max)
        {
            value = (value > max) ? max : value;
            value = (value < min) ? min : value;
            return value;
        }

        public static FP Distance(FP value1, FP value2)
        {
            return FP.Abs(value1 - value2);
        }

        public static FP Hermite(FP value1, FP tangent1, FP value2, FP tangent2, FP amount)
        {
            FP fp = value1;
            FP fp2 = value2;
            FP fp3 = tangent1;
            FP fp4 = tangent2;
            FP fp5 = amount;
            FP fp7 = (fp5 * fp5) * fp5;
            FP fp8 = fp5 * fp5;
            if (amount == 0f)
            {
                return value1;
            }
            if (amount == 1f)
            {
                return value2;
            }
            return ((((((((2 * fp) - (2 * fp2)) + fp4) + fp3) * fp7) + (((((3 * fp2) - (3 * fp)) - (2 * fp3)) - fp4) * fp8)) + (fp3 * fp5)) + fp);
        }

        public static FP Lerp(FP value1, FP value2, FP amount)
        {
            return (value1 + ((value2 - value1) * amount));
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
            FP fp = Clamp(amount, 0f, 1f);
            return Hermite(value1, 0f, value2, 0f, fp);
        }

        public static FP ToDegrees(FP radians)
        {
            return (radians * 57.295779513082323);
        }

        public static FP ToRadians(FP degrees)
        {
            return (degrees * 0.017453292519943295);
        }

        public static FP WrapAngle(FP angle)
        {
            angle = angle % 6.2831854820251465;
            if (angle <= -3.141593f)
            {
                angle += 6.283185f;
                return angle;
            }
            if (angle > 3.141593f)
            {
                angle -= 6.283185f;
            }
            return angle;
        }
    }
}

