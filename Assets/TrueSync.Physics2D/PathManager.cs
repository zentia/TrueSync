using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public static class PathManager
	{
		public enum LinkType
		{
			Revolute,
			Slider
		}

		public static void ConvertPathToEdges(Path path, Body body, int subdivisions)
		{
			Vertices vertices = path.GetVertices(subdivisions);
			bool closed = path.Closed;
			if (closed)
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
			bool flag = !path.Closed;
			if (flag)
			{
				throw new Exception("The path must be closed to convert to a polygon.");
			}
			List<TSVector2> vertices = path.GetVertices(subdivisions);
			List<Vertices> list = Triangulate.ConvexPartition(new Vertices(vertices), TriangulationAlgorithm.Bayazit, true, FP.EN3);
			foreach (Vertices current in list)
			{
				body.CreateFixture(new PolygonShape(current, density), null);
			}
		}

		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, IEnumerable<Shape> shapes, BodyType type, int copies, object userData = null)
		{
			List<Vector3> list = path.SubdivideEvenly(copies);
			List<Body> list2 = new List<Body>();
			for (int i = 0; i < list.Count; i++)
			{
				Body body = new Body(world, null, 0, null);
				body.BodyType = type;
				body.Position = new TSVector2(list[i].X, list[i].Y);
				body.Rotation = list[i].Z;
				body.UserData = userData;
				foreach (Shape current in shapes)
				{
					body.CreateFixture(current, null);
				}
				list2.Add(body);
			}
			return list2;
		}

		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, Shape shape, BodyType type, int copies, object userData)
		{
			return PathManager.EvenlyDistributeShapesAlongPath(world, path, new List<Shape>(1)
			{
				shape
			}, type, copies, userData);
		}

		public static List<Body> EvenlyDistributeShapesAlongPath(World world, Path path, Shape shape, BodyType type, int copies)
		{
			return PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, type, copies, null);
		}

		public static void MoveBodyOnPath(Path path, Body body, FP time, FP strength, FP timeStep)
		{
			TSVector2 position = path.GetPosition(time);
			TSVector2 value = body.Position - position;
			TSVector2 value2 = value / timeStep * strength;
			body.LinearVelocity = -value2;
		}

		public static List<RevoluteJoint> AttachBodiesWithRevoluteJoint(World world, List<Body> bodies, TSVector2 localAnchorA, TSVector2 localAnchorB, bool connectFirstAndLast, bool collideConnected)
		{
			List<RevoluteJoint> list = new List<RevoluteJoint>(bodies.Count + 1);
			for (int i = 1; i < bodies.Count; i++)
			{
				RevoluteJoint revoluteJoint = new RevoluteJoint(bodies[i], bodies[i - 1], localAnchorA, localAnchorB, false);
				revoluteJoint.CollideConnected = collideConnected;
				world.AddJoint(revoluteJoint);
				list.Add(revoluteJoint);
			}
			if (connectFirstAndLast)
			{
				RevoluteJoint revoluteJoint2 = new RevoluteJoint(bodies[0], bodies[bodies.Count - 1], localAnchorA, localAnchorB, false);
				revoluteJoint2.CollideConnected = collideConnected;
				world.AddJoint(revoluteJoint2);
				list.Add(revoluteJoint2);
			}
			return list;
		}
	}
}
