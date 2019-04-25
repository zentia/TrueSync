namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public static class Melkman
    {
        public static Vertices GetConvexHull(Vertices vertices)
        {
            if (vertices.Count <= 3)
            {
                return vertices;
            }
            TSVector2[] vectorArray = new TSVector2[vertices.Count + 1];
            int index = 3;
            int num2 = 0;
            int num3 = 3;
            FP fp = MathUtils.Area(vertices[0], vertices[1], vertices[2]);
            if (fp == 0)
            {
                vectorArray[0] = vertices[0];
                vectorArray[1] = vertices[2];
                vectorArray[2] = vertices[0];
                index = 2;
                num3 = 3;
                while (num3 < vertices.Count)
                {
                    TSVector2 c = vertices[num3];
                    if (MathUtils.Area(ref vectorArray[0], ref vectorArray[1], ref c) != 0)
                    {
                        break;
                    }
                    vectorArray[1] = vertices[num3];
                    num3++;
                }
            }
            else
            {
                vectorArray[0] = vectorArray[3] = vertices[2];
                if (fp > 0)
                {
                    vectorArray[1] = vertices[0];
                    vectorArray[2] = vertices[1];
                }
                else
                {
                    vectorArray[1] = vertices[1];
                    vectorArray[2] = vertices[0];
                }
            }
            int num4 = (index == 0) ? (vectorArray.Length - 1) : (index - 1);
            int num5 = (num2 == (vectorArray.Length - 1)) ? 0 : (num2 + 1);
            for (int i = num3; i < vertices.Count; i++)
            {
                TSVector2 vector3 = vertices[i];
                if ((MathUtils.Area(ref vectorArray[num4], ref vectorArray[index], ref vector3) <= 0) || (MathUtils.Area(ref vectorArray[num2], ref vectorArray[num5], ref vector3) <= 0))
                {
                    while (MathUtils.Area(ref vectorArray[num4], ref vectorArray[index], ref vector3) <= 0)
                    {
                        index = num4;
                        num4 = (index == 0) ? (vectorArray.Length - 1) : (index - 1);
                    }
                    index = (index == (vectorArray.Length - 1)) ? 0 : (index + 1);
                    num4 = (index == 0) ? (vectorArray.Length - 1) : (index - 1);
                    vectorArray[index] = vector3;
                    while (MathUtils.Area(ref vectorArray[num2], ref vectorArray[num5], ref vector3) <= 0)
                    {
                        num2 = num5;
                        num5 = (num2 == (vectorArray.Length - 1)) ? 0 : (num2 + 1);
                    }
                    num2 = (num2 == 0) ? (vectorArray.Length - 1) : (num2 - 1);
                    num5 = (num2 == (vectorArray.Length - 1)) ? 0 : (num2 + 1);
                    vectorArray[num2] = vector3;
                }
            }
            if (num2 < index)
            {
                Vertices vertices3 = new Vertices(index);
                for (int m = num2; m < index; m++)
                {
                    vertices3.Add(vectorArray[m]);
                }
                return vertices3;
            }
            Vertices vertices4 = new Vertices(index + vectorArray.Length);
            for (int j = 0; j < index; j++)
            {
                vertices4.Add(vectorArray[j]);
            }
            for (int k = num2; k < vectorArray.Length; k++)
            {
                vertices4.Add(vectorArray[k]);
            }
            return vertices4;
        }
    }
}

