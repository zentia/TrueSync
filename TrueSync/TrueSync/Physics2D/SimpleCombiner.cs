namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class SimpleCombiner
    {
        private static Vertices AddTriangle(Vertices t, Vertices vertices)
        {
            int num = -1;
            int num2 = -1;
            int num3 = -1;
            int num4 = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if ((t[0].x == vertices[i].x) && (t[0].y == vertices[i].y))
                {
                    if (num == -1)
                    {
                        num = i;
                        num2 = 0;
                    }
                    else
                    {
                        num3 = i;
                        num4 = 0;
                    }
                }
                else if ((t[1].x == vertices[i].x) && (t[1].y == vertices[i].y))
                {
                    if (num == -1)
                    {
                        num = i;
                        num2 = 1;
                    }
                    else
                    {
                        num3 = i;
                        num4 = 1;
                    }
                }
                else if ((t[2].x == vertices[i].x) && (t[2].y == vertices[i].y))
                {
                    if (num == -1)
                    {
                        num = i;
                        num2 = 2;
                    }
                    else
                    {
                        num3 = i;
                        num4 = 2;
                    }
                }
            }
            if ((num == 0) && (num3 == (vertices.Count - 1)))
            {
                num = vertices.Count - 1;
                num3 = 0;
            }
            if (num3 == -1)
            {
                return null;
            }
            int num5 = 0;
            if ((num5 == num2) || (num5 == num4))
            {
                num5 = 1;
            }
            if ((num5 == num2) || (num5 == num4))
            {
                num5 = 2;
            }
            Vertices vertices2 = new Vertices(vertices.Count + 1);
            for (int j = 0; j < vertices.Count; j++)
            {
                vertices2.Add(vertices[j]);
                if (j == num)
                {
                    vertices2.Add(t[num5]);
                }
            }
            return vertices2;
        }

        public static List<Vertices> PolygonizeTriangles(List<Vertices> triangles, int maxPolys = 0x7fffffff, float tolerance = 0.001f)
        {
            if (triangles.Count <= 0)
            {
                return triangles;
            }
            List<Vertices> list = new List<Vertices>();
            bool[] flagArray = new bool[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                flagArray[i] = false;
                Vertices vertices = triangles[i];
                TSVector2 vector = vertices[0];
                TSVector2 vector2 = vertices[1];
                TSVector2 vector3 = vertices[2];
                if ((((vector.x == vector2.x) && (vector.y == vector2.y)) || ((vector2.x == vector3.x) && (vector2.y == vector3.y))) || ((vector.x == vector3.x) && (vector.y == vector3.y)))
                {
                    flagArray[i] = true;
                }
            }
            int num = 0;
            bool flag = true;
            while (flag)
            {
                int index = -1;
                for (int k = 0; k < triangles.Count; k++)
                {
                    if (!flagArray[k])
                    {
                        index = k;
                        break;
                    }
                }
                if (index == -1)
                {
                    flag = false;
                }
                else
                {
                    Vertices vertices2 = new Vertices(3);
                    for (int m = 0; m < 3; m++)
                    {
                        vertices2.Add(triangles[index][m]);
                    }
                    flagArray[index] = true;
                    int num5 = 0;
                    int num7 = 0;
                    while (num7 < (2 * triangles.Count))
                    {
                        while (num5 >= triangles.Count)
                        {
                            num5 -= triangles.Count;
                        }
                        if (!flagArray[num5])
                        {
                            Vertices vertices3 = AddTriangle(triangles[num5], vertices2);
                            if (((vertices3 != null) && (vertices3.Count <= Settings.MaxPolygonVertices)) && vertices3.IsConvex())
                            {
                                vertices2 = new Vertices(vertices3);
                                flagArray[num5] = true;
                            }
                        }
                        num7++;
                        num5++;
                    }
                    if (num < maxPolys)
                    {
                        SimplifyTools.MergeParallelEdges(vertices2, tolerance);
                        if (vertices2.Count >= 3)
                        {
                            list.Add(new Vertices(vertices2));
                        }
                        else
                        {
                            Debug.WriteLine("Skipping corrupt poly.");
                        }
                    }
                    if (vertices2.Count >= 3)
                    {
                        num++;
                    }
                }
            }
            for (int j = list.Count - 1; j >= 0; j--)
            {
                if (list[j].Count == 0)
                {
                    list.RemoveAt(j);
                }
            }
            return list;
        }
    }
}

