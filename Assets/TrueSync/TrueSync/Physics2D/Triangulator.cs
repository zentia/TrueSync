// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Triangulate
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
    public static class Triangulate
    {
        public static List<Vertices> ConvexPartition(Vertices vertices, TriangulationAlgorithm algorithm, bool discardAndFixInvalid, FP tolerance)
        {
            if (vertices.Count <= 3)
                return new List<Vertices>() { vertices };
            List<Vertices> verticesList;
            switch (algorithm)
            {
                case TriangulationAlgorithm.Earclip:
                    if (vertices.IsCounterClockWise())
                    {
                        Vertices vertices1 = new Vertices((IEnumerable<TSVector2>)vertices);
                        vertices1.Reverse();
                        verticesList = EarclipDecomposer.ConvexPartition(vertices1, tolerance);
                        break;
                    }
                    verticesList = EarclipDecomposer.ConvexPartition(vertices, tolerance);
                    break;
                case TriangulationAlgorithm.Bayazit:
                    if (!vertices.IsCounterClockWise())
                    {
                        Vertices vertices1 = new Vertices((IEnumerable<TSVector2>)vertices);
                        vertices1.Reverse();
                        verticesList = BayazitDecomposer.ConvexPartition(vertices1);
                        break;
                    }
                    verticesList = BayazitDecomposer.ConvexPartition(vertices);
                    break;
                case TriangulationAlgorithm.Flipcode:
                    if (!vertices.IsCounterClockWise())
                    {
                        Vertices vertices1 = new Vertices((IEnumerable<TSVector2>)vertices);
                        vertices1.Reverse();
                        verticesList = FlipcodeDecomposer.ConvexPartition(vertices1);
                        break;
                    }
                    verticesList = FlipcodeDecomposer.ConvexPartition(vertices);
                    break;
                case TriangulationAlgorithm.Seidel:
                    verticesList = SeidelDecomposer.ConvexPartition(vertices, tolerance);
                    break;
                case TriangulationAlgorithm.SeidelTrapezoids:
                    verticesList = SeidelDecomposer.ConvexPartitionTrapezoid(vertices, tolerance);
                    break;
                case TriangulationAlgorithm.Delauny:
                    verticesList = CDTDecomposer.ConvexPartition(vertices);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm));
            }
            if (discardAndFixInvalid)
            {
                for (int index = verticesList.Count - 1; index >= 0; --index)
                {
                    if (!Triangulate.ValidatePolygon(verticesList[index]))
                        verticesList.RemoveAt(index);
                }
            }
            return verticesList;
        }

        private static bool ValidatePolygon(Vertices polygon)
        {
            PolygonError polygonError = polygon.CheckPolygon();
            int num;
            switch (polygonError)
            {
                case PolygonError.InvalidAmountOfVertices:
                case PolygonError.AreaTooSmall:
                case PolygonError.SideTooSmall:
                    num = 1;
                    break;
                default:
                    num = polygonError == PolygonError.NotSimple ? 1 : 0;
                    break;
            }
            if (num != 0)
                return false;
            if (polygonError == PolygonError.NotCounterClockWise)
                polygon.Reverse();
            if (polygonError != PolygonError.NotConvex)
                return true;
            polygon = GiftWrap.GetConvexHull(polygon);
            return Triangulate.ValidatePolygon(polygon);
        }
    }
}
