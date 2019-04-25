namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    public static class Triangulate
    {
        public static List<Vertices> ConvexPartition(Vertices vertices, TriangulationAlgorithm algorithm, bool discardAndFixInvalid, FP tolerance)
        {
            List<Vertices> list;
            if (vertices.Count <= 3)
            {
                return new List<Vertices> { vertices };
            }
            switch (algorithm)
            {
                case TriangulationAlgorithm.Earclip:
                    if (vertices.IsCounterClockWise())
                    {
                        Vertices vertices2 = new Vertices(vertices);
                        vertices2.Reverse();
                        list = EarclipDecomposer.ConvexPartition(vertices2, tolerance);
                    }
                    else
                    {
                        list = EarclipDecomposer.ConvexPartition(vertices, tolerance);
                    }
                    break;

                case TriangulationAlgorithm.Bayazit:
                    if (!vertices.IsCounterClockWise())
                    {
                        Vertices vertices3 = new Vertices(vertices);
                        vertices3.Reverse();
                        list = BayazitDecomposer.ConvexPartition(vertices3);
                    }
                    else
                    {
                        list = BayazitDecomposer.ConvexPartition(vertices);
                    }
                    break;

                case TriangulationAlgorithm.Flipcode:
                    if (!vertices.IsCounterClockWise())
                    {
                        Vertices vertices4 = new Vertices(vertices);
                        vertices4.Reverse();
                        list = FlipcodeDecomposer.ConvexPartition(vertices4);
                    }
                    else
                    {
                        list = FlipcodeDecomposer.ConvexPartition(vertices);
                    }
                    break;

                case TriangulationAlgorithm.Seidel:
                    list = SeidelDecomposer.ConvexPartition(vertices, tolerance);
                    break;

                case TriangulationAlgorithm.SeidelTrapezoids:
                    list = SeidelDecomposer.ConvexPartitionTrapezoid(vertices, tolerance);
                    break;

                case TriangulationAlgorithm.Delauny:
                    list = CDTDecomposer.ConvexPartition(vertices);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("algorithm");
            }
            if (discardAndFixInvalid)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Vertices polygon = list[i];
                    if (!ValidatePolygon(polygon))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            return list;
        }

        private static bool ValidatePolygon(Vertices polygon)
        {
            switch (polygon.CheckPolygon())
            {
                case PolygonError.InvalidAmountOfVertices:
                case PolygonError.AreaTooSmall:
                case PolygonError.SideTooSmall:
                case PolygonError.NotSimple:
                    return false;

                case PolygonError.NotCounterClockWise:
                    polygon.Reverse();
                    break;

                case PolygonError.NotConvex:
                    polygon = GiftWrap.GetConvexHull(polygon);
                    return ValidatePolygon(polygon);
            }
            return true;
        }
    }
}

