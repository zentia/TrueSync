namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    public static class LinkFactory
    {
        public static Path CreateChain(TrueSync.Physics2D.World world, TSVector2 start, TSVector2 end, FP linkWidth, FP linkHeight, int numberOfLinks, FP linkDensity, bool attachRopeJoint)
        {
            Debug.Assert(numberOfLinks >= 2);
            Path path = new Path();
            path.Add(start);
            path.Add(end);
            PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(linkWidth, linkHeight), linkDensity);
            List<Body> bodies = PathManager.EvenlyDistributeShapesAlongPath(world, path, shape, BodyType.Dynamic, numberOfLinks);
            PathManager.AttachBodiesWithRevoluteJoint(world, bodies, new TSVector2(0, -linkHeight), new TSVector2(0, linkHeight), false, false);
            if (attachRopeJoint)
            {
                JointFactory.CreateRopeJoint(world, bodies[0], bodies[bodies.Count - 1], TSVector2.zero, TSVector2.zero, false);
            }
            return path;
        }
    }
}

