namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    internal class PointGenerator
    {
        private static readonly TSRandom RNG = TSRandom.New(0);

        public static List<TriangulationPoint> UniformDistribution(int n, FP scale)
        {
            List<TriangulationPoint> list = new List<TriangulationPoint>();
            for (int i = 0; i < n; i++)
            {
                list.Add(new TriangulationPoint(scale * (0.5 - RNG.NextFP()), scale * (0.5 - RNG.NextFP())));
            }
            return list;
        }

        public static List<TriangulationPoint> UniformGrid(int n, FP scale)
        {
            FP x = 0;
            FP fp2 = scale / n;
            FP fp3 = 0.5 * scale;
            List<TriangulationPoint> list = new List<TriangulationPoint>();
            for (int i = 0; i < (n + 1); i++)
            {
                x = fp3 - (i * fp2);
                for (int j = 0; j < (n + 1); j++)
                {
                    list.Add(new TriangulationPoint(x, fp3 - (j * fp2)));
                }
            }
            return list;
        }
    }
}

