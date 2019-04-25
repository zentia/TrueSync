namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class CuttingTools
    {
        public static bool Cut(TrueSync.Physics2D.World world, TSVector2 start, TSVector2 end)
        {
            List<Fixture> fixtures = new List<Fixture>();
            List<TSVector2> entryPoints = new List<TSVector2>();
            List<TSVector2> exitPoints = new List<TSVector2>();
            if ((world.TestPoint(start) != null) || (world.TestPoint(end) > null))
            {
                return false;
            }
            world.RayCast(delegate (Fixture f, TSVector2 p, TSVector2 n, FP fr) {
                fixtures.Add(f);
                entryPoints.Add(p);
                return 1;
            }, start, end);
            world.RayCast(delegate (Fixture f, TSVector2 p, TSVector2 n, FP fr) {
                exitPoints.Add(p);
                return 1;
            }, end, start);
            if ((entryPoints.Count + exitPoints.Count) < 2)
            {
                return false;
            }
            for (int i = 0; i < fixtures.Count; i++)
            {
                if ((fixtures[i].Shape.ShapeType == ShapeType.Polygon) && (fixtures[i].Body.BodyType > BodyType.Static))
                {
                    Vertices vertices;
                    Vertices vertices2;
                    SplitShape(fixtures[i], entryPoints[i], exitPoints[i], out vertices, out vertices2);
                    if (vertices.CheckPolygon() == PolygonError.NoError)
                    {
                        Body body = BodyFactory.CreatePolygon(world, vertices, fixtures[i].Shape.Density, fixtures[i].Body.Position);
                        body.Rotation = fixtures[i].Body.Rotation;
                        body.LinearVelocity = fixtures[i].Body.LinearVelocity;
                        body.AngularVelocity = fixtures[i].Body.AngularVelocity;
                        body.BodyType = BodyType.Dynamic;
                    }
                    if (vertices2.CheckPolygon() == PolygonError.NoError)
                    {
                        Body body2 = BodyFactory.CreatePolygon(world, vertices2, fixtures[i].Shape.Density, fixtures[i].Body.Position);
                        body2.Rotation = fixtures[i].Body.Rotation;
                        body2.LinearVelocity = fixtures[i].Body.LinearVelocity;
                        body2.AngularVelocity = fixtures[i].Body.AngularVelocity;
                        body2.BodyType = BodyType.Dynamic;
                    }
                    world.RemoveBody(fixtures[i].Body);
                }
            }
            return true;
        }

        public static void SplitShape(Fixture fixture, TSVector2 entryPoint, TSVector2 exitPoint, out Vertices first, out Vertices second)
        {
            TSVector2 localPoint = fixture.Body.GetLocalPoint(ref entryPoint);
            TSVector2 other = fixture.Body.GetLocalPoint(ref exitPoint);
            PolygonShape shape = fixture.Shape as PolygonShape;
            if (shape == null)
            {
                first = new Vertices();
                second = new Vertices();
            }
            else
            {
                foreach (TSVector2 vector3 in shape.Vertices)
                {
                    if (vector3.Equals(localPoint))
                    {
                        localPoint -= new TSVector2(0, Settings.Epsilon);
                    }
                    if (vector3.Equals(other))
                    {
                        other += new TSVector2(0, Settings.Epsilon);
                    }
                }
                Vertices vertices = new Vertices(shape.Vertices);
                Vertices[] verticesArray = new Vertices[2];
                for (int i = 0; i < verticesArray.Length; i++)
                {
                    verticesArray[i] = new Vertices(vertices.Count);
                }
                int[] numArray = new int[] { -1, -1 };
                int index = -1;
                for (int j = 0; j < vertices.Count; j++)
                {
                    int num4;
                    if (TSVector2.Dot(MathUtils.Cross(other - localPoint, 1), vertices[j] - localPoint) > Settings.Epsilon)
                    {
                        num4 = 0;
                    }
                    else
                    {
                        num4 = 1;
                    }
                    if (index != num4)
                    {
                        if (index == 0)
                        {
                            Debug.Assert(numArray[0] == -1);
                            numArray[0] = verticesArray[index].Count;
                            verticesArray[index].Add(other);
                            verticesArray[index].Add(localPoint);
                        }
                        if (index == 1)
                        {
                            Debug.Assert(numArray[index] == -1);
                            numArray[index] = verticesArray[index].Count;
                            verticesArray[index].Add(localPoint);
                            verticesArray[index].Add(other);
                        }
                    }
                    verticesArray[num4].Add(vertices[j]);
                    index = num4;
                }
                if (numArray[0] == -1)
                {
                    numArray[0] = verticesArray[0].Count;
                    verticesArray[0].Add(other);
                    verticesArray[0].Add(localPoint);
                }
                if (numArray[1] == -1)
                {
                    numArray[1] = verticesArray[1].Count;
                    verticesArray[1].Add(localPoint);
                    verticesArray[1].Add(other);
                }
                for (int k = 0; k < 2; k++)
                {
                    TSVector2 one;
                    if (numArray[k] > 0)
                    {
                        one = verticesArray[k][numArray[k] - 1] - verticesArray[k][numArray[k]];
                    }
                    else
                    {
                        one = verticesArray[k][verticesArray[k].Count - 1] - verticesArray[k][0];
                    }
                    one.Normalize();
                    if (!one.IsValid())
                    {
                        one = TSVector2.one;
                    }
                    Vertices vertices2 = verticesArray[k];
                    int num6 = numArray[k];
                    vertices2[num6] += Settings.Epsilon * one;
                    if (numArray[k] < (verticesArray[k].Count - 2))
                    {
                        one = verticesArray[k][numArray[k] + 2] - verticesArray[k][numArray[k] + 1];
                    }
                    else
                    {
                        one = verticesArray[k][0] - verticesArray[k][verticesArray[k].Count - 1];
                    }
                    one.Normalize();
                    if (!one.IsValid())
                    {
                        one = TSVector2.one;
                    }
                    vertices2 = verticesArray[k];
                    num6 = numArray[k] + 1;
                    vertices2[num6] += Settings.Epsilon * one;
                }
                first = verticesArray[0];
                second = verticesArray[1];
            }
        }
    }
}

