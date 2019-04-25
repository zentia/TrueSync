namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class PathManager
    {
        public static List<RevoluteJoint> AttachBodiesWithRevoluteJoint(TrueSync.Physics2D.World world, List<Body> bodies, TSVector2 localAnchorA, TSVector2 localAnchorB, bool connectFirstAndLast, bool collideConnected)
        {
            List<RevoluteJoint> list = new List<RevoluteJoint>(bodies.Count + 1);
            for (int i = 1; i < bodies.Count; i++)
            {
                RevoluteJoint joint = new RevoluteJoint(bodies[i], bodies[i - 1], localAnchorA, localAnchorB, false) {
                    CollideConnected = collideConnected
                };
                world.AddJoint(joint);
                list.Add(joint);
            }
            if (connectFirstAndLast)
            {
                RevoluteJoint joint2 = new RevoluteJoint(bodies[0], bodies[bodies.Count - 1], localAnchorA, localAnchorB, false) {
                    CollideConnected = collideConnected
                };
                world.AddJoint(joint2);
                list.Add(joint2);
            }
            return list;
        }

        public static void ConvertPathToEdges(Path path, Body body, int subdivisions)
        {
            Vertices vertices = path.GetVertices(subdivisions);
            if (path.Closed)
            {
                ChainShape shape = new ChainShape(vertices, true);
                body.CreateFixture(shape, null);
            }
            else
            {
                for (int i = 1; i < vertices.Count; i++)
                {
                    body.CreateFixture(new EdgeShape(vertices[i], vertices[i - 1]), null);
                }
            }
        }

        public static void ConvertPathToPolygon(Path path, Body body, FP density, int subdivisions)
        {
            if (!path.Closed)
            {
                throw new Exception("The path must be closed to convert to a polygon.");
            }
            List<Vertices> list2 = Triangulate.ConvexPartition(new Vertices(path.GetVertices(subdivisions)), TriangulationAlgorithm.Bayazit, true, FP.EN3);
            foreach (Vertices vertices in list2)
            {
                body.CreateFixture(new PolygonShape(vertices, density), null);
            }
        }

        public static List<Body> EvenlyDistributeShapesAlongPath(TrueSync.Physics2D.World world, Path path, TrueSync.Physics2D.Shape shape, BodyType type, int copies)
        {
            return EvenlyDistributeShapesAlongPath(world, path, shape, type, copies, null);
        }

        public static List<Body> EvenlyDistributeShapesAlongPath(TrueSync.Physics2D.World world, Path path, IEnumerable<TrueSync.Physics2D.Shape> shapes, BodyType type, int copies, object userData = null)
        {
            List<Vector3> list = path.SubdivideEvenly(copies);
            List<Body> list2 = new List<Body>();
            for (int i = 0; i < list.Count; i++)
            {
                TSVector2? position = null;
                Body item = new Body(world, position, 0, null) {
                    BodyType = type,
                    Position = new TSVector2(list[i].X, list[i].Y),
                    Rotation = list[i].Z,
                    UserData = userData
                };
                foreach (TrueSync.Physics2D.Shape shape in shapes)
                {
                    item.CreateFixture(shape, null);
                }
                list2.Add(item);
            }
            return list2;
        }

        public static List<Body> EvenlyDistributeShapesAlongPath(TrueSync.Physics2D.World world, Path path, TrueSync.Physics2D.Shape shape, BodyType type, int copies, object userData)
        {
            List<TrueSync.Physics2D.Shape> shapes = new List<TrueSync.Physics2D.Shape>(1) {
                shape
            };
            return EvenlyDistributeShapesAlongPath(world, path, shapes, type, copies, userData);
        }

        public static void MoveBodyOnPath(Path path, Body body, FP time, FP strength, FP timeStep)
        {
            TSVector2 position = path.GetPosition(time);
            TSVector2 vector2 = body.Position - position;
            TSVector2 vector3 = (vector2 / timeStep) * strength;
            body.LinearVelocity = -vector3;
        }

        public enum LinkType
        {
            Revolute,
            Slider
        }
    }
}

