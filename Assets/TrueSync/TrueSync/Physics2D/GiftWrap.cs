namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public static class GiftWrap
    {
        public static Vertices GetConvexHull(Vertices vertices)
        {
            int num5;
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            int num = 0;
            FP x = vertices[0].x;
            for (int i = 1; i < vertices.Count; i++)
            {
                FP fp2 = vertices[i].x;
                if ((fp2 > x) || ((fp2 == x) && (vertices[i].y < vertices[num].y)))
                {
                    num = i;
                    x = fp2;
                }
            }
            int[] numArray = new int[vertices.Count];
            int index = 0;
            int num3 = num;
            do
            {
                numArray[index] = num3;
                num5 = 0;
                for (int k = 1; k < vertices.Count; k++)
                {
                    if (num5 == num3)
                    {
                        num5 = k;
                    }
                    else
                    {
                        TSVector2 a = vertices[num5] - vertices[numArray[index]];
                        TSVector2 b = vertices[k] - vertices[numArray[index]];
                        FP fp3 = MathUtils.Cross(ref a, ref b);
                        if (fp3 < 0f)
                        {
                            num5 = k;
                        }
                        if ((fp3 == 0f) && (b.LengthSquared() > a.LengthSquared()))
                        {
                            num5 = k;
                        }
                    }
                }
                index++;
                num3 = num5;
            }
            while (num5 != num);
            Vertices vertices2 = new Vertices(index);
            for (int j = 0; j < index; j++)
            {
                vertices2.Add(vertices[numArray[j]]);
            }
            return vertices2;
        }
    }
}

