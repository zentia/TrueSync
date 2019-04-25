namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    internal static class SeidelDecomposer
    {
        public static List<Vertices> ConvexPartition(Vertices vertices, FP sheer)
        {
            Debug.Assert(vertices.Count > 3);
            List<Point> polyLine = new List<Point>(vertices.Count);
            foreach (TSVector2 vector in vertices)
            {
                polyLine.Add(new Point(vector.x, vector.y));
            }
            Triangulator triangulator = new Triangulator(polyLine, sheer);
            List<Vertices> list2 = new List<Vertices>();
            foreach (List<Point> list3 in triangulator.Triangles)
            {
                Vertices item = new Vertices(list3.Count);
                foreach (Point point in list3)
                {
                    item.Add(new TSVector2(point.X, point.Y));
                }
                list2.Add(item);
            }
            return list2;
        }

        public static List<Vertices> ConvexPartitionTrapezoid(Vertices vertices, FP sheer)
        {
            List<Point> polyLine = new List<Point>(vertices.Count);
            foreach (TSVector2 vector in vertices)
            {
                polyLine.Add(new Point(vector.x, vector.y));
            }
            Triangulator triangulator = new Triangulator(polyLine, sheer);
            List<Vertices> list2 = new List<Vertices>();
            foreach (Trapezoid trapezoid in triangulator.Trapezoids)
            {
                Vertices item = new Vertices();
                List<Point> list3 = trapezoid.GetVertices();
                foreach (Point point in list3)
                {
                    item.Add(new TSVector2(point.X, point.Y));
                }
                list2.Add(item);
            }
            return list2;
        }
    }
}

