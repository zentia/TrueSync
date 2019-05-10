using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class LinkFactory
	{
		public static Path CreateChain(World world, TSVector2 start, TSVector2 end, FP linkWidth, FP linkHeight, int numberOfLinks, FP linkDensity, bool attachRopeJoint)
		{
			Debug.Assert(numberOfLinks >= 2);
			Path path = new Path();
			path.Add(start);
			path.Add(end);
			PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(linkWidth, linkHeight), linkDensity);
			List<Body> list = PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);
			PathManager.AttachBodiesWithRevoluteJoint(world, list, new TSVector2(0, -linkHeight), new TSVector2(0, linkHeight), false, false);
			if (attachRopeJoint)
			{
				JointFactory.CreateRopeJoint(world, list[0], list[list.Count - 1], TSVector2.zero, TSVector2.zero, false);
			}
			return path;
		}
	}
}
