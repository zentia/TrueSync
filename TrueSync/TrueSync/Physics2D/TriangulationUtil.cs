namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    internal class TriangulationUtil
    {
        public static FP EPSILON = 1E-12;

        public static bool InScanArea(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
        {
            FP fp = ((pa.X - pb.X) * (pd.Y - pb.Y)) - ((pd.X - pb.X) * (pa.Y - pb.Y));
            if (fp >= -EPSILON)
            {
                return false;
            }
            FP fp2 = ((pa.X - pc.X) * (pd.Y - pc.Y)) - ((pd.X - pc.X) * (pa.Y - pc.Y));
            if (fp2 <= EPSILON)
            {
                return false;
            }
            return true;
        }

        public static Orientation Orient2d(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc)
        {
            FP fp = (pa.X - pc.X) * (pb.Y - pc.Y);
            FP fp2 = (pa.Y - pc.Y) * (pb.X - pc.X);
            FP fp3 = fp - fp2;
            if ((fp3 > -EPSILON) && (fp3 < EPSILON))
            {
                return Orientation.Collinear;
            }
            if (fp3 > 0)
            {
                return Orientation.CCW;
            }
            return Orientation.CW;
        }

        public static bool SmartIncircle(TriangulationPoint pa, TriangulationPoint pb, TriangulationPoint pc, TriangulationPoint pd)
        {
            FP x = pd.X;
            FP y = pd.Y;
            FP fp3 = pa.X - x;
            FP fp4 = pa.Y - y;
            FP fp5 = pb.X - x;
            FP fp6 = pb.Y - y;
            FP fp7 = fp3 * fp6;
            FP fp8 = fp5 * fp4;
            FP fp9 = fp7 - fp8;
            if (fp9 <= 0)
            {
                return false;
            }
            FP fp10 = pc.X - x;
            FP fp11 = pc.Y - y;
            FP fp12 = fp10 * fp4;
            FP fp13 = fp3 * fp11;
            FP fp14 = fp12 - fp13;
            if (fp14 <= 0)
            {
                return false;
            }
            FP fp15 = fp5 * fp11;
            FP fp16 = fp10 * fp6;
            FP fp17 = (fp3 * fp3) + (fp4 * fp4);
            FP fp18 = (fp5 * fp5) + (fp6 * fp6);
            FP fp19 = (fp10 * fp10) + (fp11 * fp11);
            FP fp20 = ((fp17 * (fp15 - fp16)) + (fp18 * fp14)) + (fp19 * fp9);
            return (fp20 > 0);
        }
    }
}

