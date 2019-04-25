namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    internal class PolygonGenerator
    {
        private static FP PI_2 = (2.0 * FP.Pi);
        private static readonly TSRandom RNG = TSRandom.New(0);

        public static Polygon RandomCircleSweep(FP scale, int vertexCount)
        {
            FP fp = scale / 4;
            PolygonPoint[] points = new PolygonPoint[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                do
                {
                    if ((i % 250) == 0)
                    {
                        fp += (scale / 2) * (0.5 - RNG.NextFP());
                    }
                    else if ((i % 50) == 0)
                    {
                        fp += (scale / 5) * (0.5 - RNG.NextFP());
                    }
                    else
                    {
                        fp += ((0x19 * scale) / vertexCount) * (0.5 - RNG.NextFP());
                    }
                    fp = (fp > (scale / 2)) ? (scale / 2) : fp;
                    fp = (fp < (scale / 10)) ? (scale / 10) : fp;
                }
                while ((fp < (scale / 10)) || (fp > (scale / 2)));
                points[i] = new PolygonPoint(fp * FP.Cos((PI_2 * i) / vertexCount), fp * FP.Sin((PI_2 * i) / vertexCount));
            }
            return new Polygon(points);
        }

        public static Polygon RandomCircleSweep2(FP scale, int vertexCount)
        {
            FP fp = scale / 4;
            PolygonPoint[] points = new PolygonPoint[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                do
                {
                    fp += (scale / 5) * (0.5 - RNG.NextFP());
                    fp = (fp > (scale / 2)) ? (scale / 2) : fp;
                    fp = (fp < (scale / 10)) ? (scale / 10) : fp;
                }
                while ((fp < (scale / 10)) || (fp > (scale / 2)));
                points[i] = new PolygonPoint(fp * FP.Cos((PI_2 * i) / vertexCount), fp * FP.Sin((PI_2 * i) / vertexCount));
            }
            return new Polygon(points);
        }
    }
}

