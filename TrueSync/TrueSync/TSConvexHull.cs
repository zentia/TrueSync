namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public static class TSConvexHull
    {
        public static int[] Build(List<TSVector> pointCloud, Approximation factor)
        {
            List<int> list = new List<int>();
            int num = (int) factor;
            for (int i = 0; i < num; i++)
            {
                FP x = (TSMath.Pi / (num - 1)) * i;
                FP fp2 = FP.Sin(x);
                FP y = FP.Cos(x);
                for (int k = 0; k < num; k++)
                {
                    FP fp4 = ((((2 * FP.One) * TSMath.Pi) / num) * k) - TSMath.Pi;
                    FP fp5 = FP.Sin(fp4);
                    FP fp6 = FP.Cos(fp4);
                    TSVector dir = new TSVector(fp2 * fp6, y, fp2 * fp5);
                    int item = FindExtremePoint(pointCloud, ref dir);
                    list.Add(item);
                }
            }
            list.Sort();
            for (int j = 1; j < list.Count; j++)
            {
                if (list[j - 1] == list[j])
                {
                    list.RemoveAt(j - 1);
                    j--;
                }
            }
            return list.ToArray();
        }

        private static int FindExtremePoint(List<TSVector> points, ref TSVector dir)
        {
            int num = 0;
            FP minValue = FP.MinValue;
            for (int i = 1; i < points.Count; i++)
            {
                TSVector vector = points[i];
                FP fp2 = TSVector.Dot(ref vector, ref dir);
                if (fp2 > minValue)
                {
                    minValue = fp2;
                    num = i;
                }
            }
            return num;
        }

        public enum Approximation
        {
            Level1 = 6,
            Level10 = 20,
            Level15 = 0x19,
            Level2 = 7,
            Level20 = 30,
            Level3 = 8,
            Level4 = 9,
            Level5 = 10,
            Level6 = 11,
            Level7 = 12,
            Level8 = 13,
            Level9 = 15
        }
    }
}

