using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class CuttingTools
	{
		public static void SplitShape(Fixture fixture, TSVector2 entryPoint, TSVector2 exitPoint, out Vertices first, out Vertices second)
		{
			TSVector2 tSVector = fixture.Body.GetLocalPoint(ref entryPoint);
			TSVector2 tSVector2 = fixture.Body.GetLocalPoint(ref exitPoint);
			PolygonShape polygonShape = fixture.Shape as PolygonShape;
			bool flag = polygonShape == null;
			if (flag)
			{
				first = new Vertices();
				second = new Vertices();
			}
			else
			{
				foreach (TSVector2 current in polygonShape.Vertices)
				{
					bool flag2 = current.Equals(tSVector);
					if (flag2)
					{
						tSVector -= new TSVector2(0, Settings.Epsilon);
					}
					bool flag3 = current.Equals(tSVector2);
					if (flag3)
					{
						tSVector2 += new TSVector2(0, Settings.Epsilon);
					}
				}
				Vertices vertices = new Vertices(polygonShape.Vertices);
				Vertices[] array = new Vertices[2];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new Vertices(vertices.Count);
				}
				int[] array2 = new int[]
				{
					-1,
					-1
				};
				int num = -1;
				for (int j = 0; j < vertices.Count; j++)
				{
					bool flag4 = TSVector2.Dot(MathUtils.Cross(tSVector2 - tSVector, 1), vertices[j] - tSVector) > Settings.Epsilon;
					int num2;
					if (flag4)
					{
						num2 = 0;
					}
					else
					{
						num2 = 1;
					}
					bool flag5 = num != num2;
					if (flag5)
					{
						bool flag6 = num == 0;
						if (flag6)
						{
							Debug.Assert(array2[0] == -1);
							array2[0] = array[num].Count;
							array[num].Add(tSVector2);
							array[num].Add(tSVector);
						}
						bool flag7 = num == 1;
						if (flag7)
						{
							Debug.Assert(array2[num] == -1);
							array2[num] = array[num].Count;
							array[num].Add(tSVector);
							array[num].Add(tSVector2);
						}
					}
					array[num2].Add(vertices[j]);
					num = num2;
				}
				bool flag8 = array2[0] == -1;
				if (flag8)
				{
					array2[0] = array[0].Count;
					array[0].Add(tSVector2);
					array[0].Add(tSVector);
				}
				bool flag9 = array2[1] == -1;
				if (flag9)
				{
					array2[1] = array[1].Count;
					array[1].Add(tSVector);
					array[1].Add(tSVector2);
				}
				for (int k = 0; k < 2; k++)
				{
					bool flag10 = array2[k] > 0;
					TSVector2 tSVector3;
					if (flag10)
					{
						tSVector3 = array[k][array2[k] - 1] - array[k][array2[k]];
					}
					else
					{
						tSVector3 = array[k][array[k].Count - 1] - array[k][0];
					}
					tSVector3.Normalize();
					bool flag11 = !tSVector3.IsValid();
					if (flag11)
					{
						tSVector3 = TSVector2.one;
					}
					Vertices vertices2 = array[k];
					int index = array2[k];
					vertices2[index] += Settings.Epsilon * tSVector3;
					bool flag12 = array2[k] < array[k].Count - 2;
					if (flag12)
					{
						tSVector3 = array[k][array2[k] + 2] - array[k][array2[k] + 1];
					}
					else
					{
						tSVector3 = array[k][0] - array[k][array[k].Count - 1];
					}
					tSVector3.Normalize();
					bool flag13 = !tSVector3.IsValid();
					if (flag13)
					{
						tSVector3 = TSVector2.one;
					}
					vertices2 = array[k];
					index = array2[k] + 1;
					vertices2[index] += Settings.Epsilon * tSVector3;
				}
				first = array[0];
				second = array[1];
			}
		}

		public static bool Cut(World world, TSVector2 start, TSVector2 end)
		{
			List<Fixture> fixtures = new List<Fixture>();
			List<TSVector2> entryPoints = new List<TSVector2>();
			List<TSVector2> exitPoints = new List<TSVector2>();
			bool flag = world.TestPoint(start) != null || world.TestPoint(end) != null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				world.RayCast(delegate(Fixture f, TSVector2 p, TSVector2 n, FP fr)
				{
					fixtures.Add(f);
					entryPoints.Add(p);
					return 1;
				}, start, end);
				world.RayCast(delegate(Fixture f, TSVector2 p, TSVector2 n, FP fr)
				{
					exitPoints.Add(p);
					return 1;
				}, end, start);
				bool flag2 = entryPoints.Count + exitPoints.Count < 2;
				if (flag2)
				{
					result = false;
				}
				else
				{
					for (int i = 0; i < fixtures.Count; i++)
					{
						bool flag3 = fixtures[i].Shape.ShapeType != ShapeType.Polygon;
						if (!flag3)
						{
							bool flag4 = fixtures[i].Body.BodyType > BodyType.Static;
							if (flag4)
							{
								Vertices vertices;
								Vertices vertices2;
								CuttingTools.SplitShape(fixtures[i], entryPoints[i], exitPoints[i], out vertices, out vertices2);
								bool flag5 = vertices.CheckPolygon() == PolygonError.NoError;
								if (flag5)
								{
									Body body = BodyFactory.CreatePolygon(world, vertices, fixtures[i].Shape.Density, fixtures[i].Body.Position);
									body.Rotation = fixtures[i].Body.Rotation;
									body.LinearVelocity = fixtures[i].Body.LinearVelocity;
									body.AngularVelocity = fixtures[i].Body.AngularVelocity;
									body.BodyType = BodyType.Dynamic;
								}
								bool flag6 = vertices2.CheckPolygon() == PolygonError.NoError;
								if (flag6)
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
					}
					result = true;
				}
			}
			return result;
		}
	}
}
