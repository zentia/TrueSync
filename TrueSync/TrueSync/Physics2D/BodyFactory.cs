namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class BodyFactory
    {
        public static Body CreateBody(TrueSync.Physics2D.World world, object userData = null)
        {
            return new Body(world, null, 0, userData);
        }

        public static Body CreateBody(TrueSync.Physics2D.World world, TSVector2 position, FP rotation, object userData = null)
        {
            return new Body(world, new TSVector2?(position), rotation, userData);
        }

        public static BreakableBody CreateBreakableBody(TrueSync.Physics2D.World world, IEnumerable<TrueSync.Physics2D.Shape> shapes)
        {
            return CreateBreakableBody(world, shapes, TSVector2.zero);
        }

        public static BreakableBody CreateBreakableBody(TrueSync.Physics2D.World world, IEnumerable<TrueSync.Physics2D.Shape> shapes, TSVector2 position)
        {
            BreakableBody breakableBody = new BreakableBody(shapes, world) {
                MainBody = { Position = position }
            };
            world.AddBreakableBody(breakableBody);
            return breakableBody;
        }

        public static BreakableBody CreateBreakableBody(TrueSync.Physics2D.World world, Vertices vertices, FP density)
        {
            return CreateBreakableBody(world, vertices, density, TSVector2.zero);
        }

        public static BreakableBody CreateBreakableBody(TrueSync.Physics2D.World world, Vertices vertices, FP density, TSVector2 position)
        {
            BreakableBody breakableBody = new BreakableBody(Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip, true, FP.EN3), world, density) {
                MainBody = { Position = position }
            };
            world.AddBreakableBody(breakableBody);
            return breakableBody;
        }

        public static Body CreateCapsule(TrueSync.Physics2D.World world, FP height, FP endRadius, FP density, object userData = null)
        {
            Vertices vertices = PolygonTools.CreateRectangle(endRadius, height / 2);
            List<Vertices> list = new List<Vertices> {
                vertices
            };
            Body body = CreateCompoundPolygon(world, list, density, userData);
            body.UserData = userData;
            CircleShape shape = new CircleShape(endRadius, density) {
                Position = new TSVector2(0, height / 2)
            };
            body.CreateFixture(shape, null);
            CircleShape shape2 = new CircleShape(endRadius, density) {
                Position = new TSVector2(0, -(height / 2))
            };
            body.CreateFixture(shape2, null);
            return body;
        }

        public static Body CreateCapsule(TrueSync.Physics2D.World world, FP height, FP topRadius, int topEdges, FP bottomRadius, int bottomEdges, FP density, TSVector2 position, object userData = null)
        {
            Body body;
            Vertices vertices = PolygonTools.CreateCapsule(height, topRadius, topEdges, bottomRadius, bottomEdges);
            if (vertices.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> list = Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip, true, FP.EN3);
                body = CreateCompoundPolygon(world, list, density, userData);
                body.Position = position;
                return body;
            }
            body = CreatePolygon(world, vertices, density, userData);
            body.Position = position;
            return body;
        }

        public static Body CreateChainShape(TrueSync.Physics2D.World world, Vertices vertices, object userData = null)
        {
            return CreateChainShape(world, vertices, TSVector2.zero, userData);
        }

        public static Body CreateChainShape(TrueSync.Physics2D.World world, Vertices vertices, TSVector2 position, object userData = null)
        {
            Body body = CreateBody(world, position);
            FixtureFactory.AttachChainShape(vertices, body, userData);
            return body;
        }

        public static Body CreateCircle(TrueSync.Physics2D.World world, FP radius, FP density, object userData = null)
        {
            return CreateCircle(world, radius, density, TSVector2.zero, userData);
        }

        public static Body CreateCircle(TrueSync.Physics2D.World world, FP radius, FP density, TSVector2 position, object userData = null)
        {
            Body body = CreateBody(world, position);
            FixtureFactory.AttachCircle(radius, density, body, userData);
            return body;
        }

        public static Body CreateCompoundPolygon(TrueSync.Physics2D.World world, List<Vertices> list, FP density, object userData = null)
        {
            return CreateCompoundPolygon(world, list, density, TSVector2.zero, userData);
        }

        public static Body CreateCompoundPolygon(TrueSync.Physics2D.World world, List<Vertices> list, FP density, TSVector2 position, object userData = null)
        {
            Body body = CreateBody(world, position);
            FixtureFactory.AttachCompoundPolygon(list, density, body, userData);
            return body;
        }

        public static Body CreateEdge(TrueSync.Physics2D.World world, TSVector2 start, TSVector2 end, object userData = null)
        {
            Body body = CreateBody(world, null);
            FixtureFactory.AttachEdge(start, end, body, userData);
            return body;
        }

        public static Body CreateEllipse(TrueSync.Physics2D.World world, FP xRadius, FP yRadius, int edges, FP density, object userData = null)
        {
            return CreateEllipse(world, xRadius, yRadius, edges, density, TSVector2.zero, userData);
        }

        public static Body CreateEllipse(TrueSync.Physics2D.World world, FP xRadius, FP yRadius, int edges, FP density, TSVector2 position, object userData = null)
        {
            Body body = CreateBody(world, position);
            FixtureFactory.AttachEllipse(xRadius, yRadius, edges, density, body, userData);
            return body;
        }

        public static Body CreateGear(TrueSync.Physics2D.World world, FP radius, int numberOfTeeth, FP tipPercentage, FP toothHeight, FP density, object userData = null)
        {
            Vertices vertices = PolygonTools.CreateGear(radius, numberOfTeeth, tipPercentage, toothHeight);
            if (!vertices.IsConvex())
            {
                List<Vertices> list = Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip, true, FP.EN3);
                return CreateCompoundPolygon(world, list, density, userData);
            }
            return CreatePolygon(world, vertices, density, userData);
        }

        public static Body CreateLineArc(TrueSync.Physics2D.World world, FP radians, int sides, FP radius, TSVector2 position, FP angle, bool closed)
        {
            Body body = CreateBody(world, null);
            FixtureFactory.AttachLineArc(radians, sides, radius, position, angle, closed, body);
            return body;
        }

        public static Body CreateLoopShape(TrueSync.Physics2D.World world, Vertices vertices, object userData = null)
        {
            return CreateLoopShape(world, vertices, TSVector2.zero, userData);
        }

        public static Body CreateLoopShape(TrueSync.Physics2D.World world, Vertices vertices, TSVector2 position, object userData = null)
        {
            Body body = CreateBody(world, position);
            FixtureFactory.AttachLoopShape(vertices, body, userData);
            return body;
        }

        public static Body CreatePolygon(TrueSync.Physics2D.World world, Vertices vertices, FP density, object userData)
        {
            return CreatePolygon(world, vertices, density, TSVector2.zero, userData);
        }

        public static Body CreatePolygon(TrueSync.Physics2D.World world, Vertices vertices, FP density, TSVector2 position, object userData)
        {
            Body body = CreateBody(world, position);
            FixtureFactory.AttachPolygon(vertices, density, body, userData);
            return body;
        }

        public static Body CreateRectangle(TrueSync.Physics2D.World world, FP width, FP height, FP density, object userData = null)
        {
            return CreateRectangle(world, width, height, density, TSVector2.zero, userData);
        }

        public static Body CreateRectangle(TrueSync.Physics2D.World world, FP width, FP height, FP density, TSVector2 position, object userData = null)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", "Width must be more than 0 meters");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", "Height must be more than 0 meters");
            }
            Body body = CreateBody(world, position);
            body.UserData = userData;
            PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(width / 2, height / 2), density);
            body.CreateFixture(shape, null);
            return body;
        }

        public static Body CreateRoundedRectangle(TrueSync.Physics2D.World world, FP width, FP height, FP xRadius, FP yRadius, int segments, FP density, object userData = null)
        {
            return CreateRoundedRectangle(world, width, height, xRadius, yRadius, segments, density, TSVector2.zero, userData);
        }

        public static Body CreateRoundedRectangle(TrueSync.Physics2D.World world, FP width, FP height, FP xRadius, FP yRadius, int segments, FP density, TSVector2 position, object userData = null)
        {
            Vertices vertices = PolygonTools.CreateRoundedRectangle(width, height, xRadius, yRadius, segments);
            if (vertices.Count >= Settings.MaxPolygonVertices)
            {
                List<Vertices> list = Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Earclip, true, FP.EN3);
                Body body = CreateCompoundPolygon(world, list, density, userData);
                body.Position = position;
                return body;
            }
            return CreatePolygon(world, vertices, density, null);
        }

        public static Body CreateSolidArc(TrueSync.Physics2D.World world, FP density, FP radians, int sides, FP radius, TSVector2 position, FP angle)
        {
            Body body = CreateBody(world, null);
            FixtureFactory.AttachSolidArc(density, radians, sides, radius, position, angle, body);
            return body;
        }
    }
}

