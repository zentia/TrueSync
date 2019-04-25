namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class FixtureFactory
    {
        public static Fixture AttachChainShape(Vertices vertices, Body body, object userData = null)
        {
            ChainShape shape = new ChainShape(vertices, false);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture AttachCircle(FP radius, FP density, Body body, object userData = null)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");
            }
            CircleShape shape = new CircleShape(radius, density);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture AttachCircle(FP radius, FP density, Body body, TSVector2 offset, object userData = null)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");
            }
            CircleShape shape = new CircleShape(radius, density) {
                Position = offset
            };
            return body.CreateFixture(shape, userData);
        }

        public static List<Fixture> AttachCompoundPolygon(List<Vertices> list, FP density, Body body, object userData = null)
        {
            List<Fixture> list2 = new List<Fixture>(list.Count);
            foreach (Vertices vertices in list)
            {
                if (vertices.Count == 2)
                {
                    EdgeShape shape = new EdgeShape(vertices[0], vertices[1]);
                    list2.Add(body.CreateFixture(shape, userData));
                }
                else
                {
                    PolygonShape shape2 = new PolygonShape(vertices, density);
                    list2.Add(body.CreateFixture(shape2, userData));
                }
            }
            return list2;
        }

        public static Fixture AttachEdge(TSVector2 start, TSVector2 end, Body body, object userData = null)
        {
            EdgeShape shape = new EdgeShape(start, end);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture AttachEllipse(FP xRadius, FP yRadius, int edges, FP density, Body body, object userData = null)
        {
            if (xRadius <= 0)
            {
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");
            }
            if (yRadius <= 0)
            {
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");
            }
            PolygonShape shape = new PolygonShape(PolygonTools.CreateEllipse(xRadius, yRadius, edges), density);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture AttachLineArc(FP radians, int sides, FP radius, TSVector2 position, FP angle, bool closed, Body body)
        {
            Vertices vertices = PolygonTools.CreateArc(radians, sides, radius);
            vertices.Rotate(((MathHelper.Pi - radians) / 2) + angle);
            vertices.Translate(ref position);
            return (closed ? AttachLoopShape(vertices, body, null) : AttachChainShape(vertices, body, null));
        }

        public static Fixture AttachLoopShape(Vertices vertices, Body body, object userData = null)
        {
            ChainShape shape = new ChainShape(vertices, true);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture AttachPolygon(Vertices vertices, FP density, Body body, object userData = null)
        {
            if (vertices.Count <= 1)
            {
                throw new ArgumentOutOfRangeException("vertices", "Too few points to be a polygon");
            }
            PolygonShape shape = new PolygonShape(vertices, density);
            return body.CreateFixture(shape, userData);
        }

        public static Fixture AttachRectangle(FP width, FP height, FP density, TSVector2 offset, Body body, object userData = null)
        {
            Vertices vertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            vertices.Translate(ref offset);
            PolygonShape shape = new PolygonShape(vertices, density);
            return body.CreateFixture(shape, userData);
        }

        public static List<Fixture> AttachSolidArc(FP density, FP radians, int sides, FP radius, TSVector2 position, FP angle, Body body)
        {
            Vertices vertices = PolygonTools.CreateArc(radians, sides, radius);
            vertices.Rotate(((MathHelper.Pi - radians) / 2) + angle);
            vertices.Translate(ref position);
            vertices.Add(vertices[0]);
            return AttachCompoundPolygon(Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip, true, FP.EN3), density, body, null);
        }
    }
}

