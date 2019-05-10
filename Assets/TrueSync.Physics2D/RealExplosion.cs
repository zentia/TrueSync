using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrueSync.Physics2D
{
	public sealed class RealExplosion : PhysicsLogic
	{
		private static readonly FP MaxEdgeOffset = MathHelper.Pi / 90;

		public FP EdgeRatio = 0.025f;

		public bool IgnoreWhenInsideShape = false;

		public FP MaxAngle = MathHelper.Pi / 15;

		public int MaxShapes = 100;

		public int MinRays = 5;

		private List<ShapeData> _data = new List<ShapeData>();

		private RayDataComparer _rdc;

		public RealExplosion(World world) : base(world, PhysicsLogicType.Explosion)
		{
			this._rdc = new RayDataComparer();
			this._data = new List<ShapeData>();
		}

		public Dictionary<Fixture, TSVector2> Activate(TSVector2 pos, FP radius, FP maxForce)
		{
			AABB aABB;
			aABB.LowerBound = pos + new TSVector2(-radius, -radius);
			aABB.UpperBound = pos + new TSVector2(radius, radius);
			Fixture[] shapes = new Fixture[this.MaxShapes];
			Fixture[] containedShapes = new Fixture[5];
			bool exit = false;
			int shapeCount = 0;
			int containedShapeCount = 0;
			this.World.QueryAABB(delegate(Fixture fixture)
			{
				bool flag22 = fixture.TestPoint(ref pos);
				bool result2;
				if (flag22)
				{
					bool ignoreWhenInsideShape = this.IgnoreWhenInsideShape;
					if (ignoreWhenInsideShape)
					{
						exit = true;
						result2 = false;
						return result2;
					}
					Fixture[] arg_45_0 = containedShapes;
					int num4 = containedShapeCount;
					containedShapeCount = num4 + 1;
					arg_45_0[num4] = fixture;
				}
				else
				{
					Fixture[] arg_62_0 = shapes;
					int num4 = shapeCount;
					shapeCount = num4 + 1;
					arg_62_0[num4] = fixture;
				}
				result2 = true;
				return result2;
			}, ref aABB);
			bool exit2 = exit;
			Dictionary<Fixture, TSVector2> result;
			if (exit2)
			{
				result = new Dictionary<Fixture, TSVector2>();
			}
			else
			{
				Dictionary<Fixture, TSVector2> dictionary = new Dictionary<Fixture, TSVector2>(shapeCount + containedShapeCount);
				FP[] array = new FP[shapeCount * 2];
				int num = 0;
				for (int i = 0; i < shapeCount; i++)
				{
					CircleShape circleShape = shapes[i].Shape as CircleShape;
					bool flag = circleShape != null;
					PolygonShape polygonShape;
					if (flag)
					{
						Vertices vertices = new Vertices();
						TSVector2 item = TSVector2.zero + new TSVector2(circleShape.Radius, 0);
						vertices.Add(item);
						item = TSVector2.zero + new TSVector2(0, circleShape.Radius);
						vertices.Add(item);
						item = TSVector2.zero + new TSVector2(-circleShape.Radius, circleShape.Radius);
						vertices.Add(item);
						item = TSVector2.zero + new TSVector2(0, -circleShape.Radius);
						vertices.Add(item);
						polygonShape = new PolygonShape(vertices, 0);
					}
					else
					{
						polygonShape = (shapes[i].Shape as PolygonShape);
					}
					bool flag2 = shapes[i].Body.BodyType == BodyType.Dynamic && polygonShape != null;
					if (flag2)
					{
						TSVector2 tSVector = shapes[i].Body.GetWorldPoint(polygonShape.MassData.Centroid) - pos;
						FP y = FP.Atan2(tSVector.y, tSVector.x);
						FP y2 = FP.MaxValue;
						FP y3 = FP.MinValue;
						FP fP = 0f;
						FP fP2 = 0f;
						for (int j = 0; j < polygonShape.Vertices.Count; j++)
						{
							TSVector2 tSVector2 = shapes[i].Body.GetWorldPoint(polygonShape.Vertices[j]) - pos;
							FP fP3 = FP.Atan2(tSVector2.y, tSVector2.x);
							FP fP4 = fP3 - y;
							fP4 = (fP4 - MathHelper.Pi) % (2 * MathHelper.Pi);
							bool flag3 = fP4 < 0f;
							if (flag3)
							{
								fP4 += 2 * MathHelper.Pi;
							}
							fP4 -= MathHelper.Pi;
							bool flag4 = FP.Abs(fP4) > MathHelper.Pi;
							if (!flag4)
							{
								bool flag5 = fP4 > y3;
								if (flag5)
								{
									y3 = fP4;
									fP2 = fP3;
								}
								bool flag6 = fP4 < y2;
								if (flag6)
								{
									y2 = fP4;
									fP = fP3;
								}
							}
						}
						array[num] = fP;
						num++;
						array[num] = fP2;
						num++;
					}
				}
				Array.Sort<FP>(array, 0, num, this._rdc);
				this._data.Clear();
				bool flag7 = true;
				for (int k = 0; k < num; k++)
				{
					Fixture fixture = null;
					int num2 = (k == num - 1) ? 0 : (k + 1);
					bool flag8 = array[k] == array[num2];
					if (!flag8)
					{
						bool flag9 = k == num - 1;
						FP x;
						if (flag9)
						{
							x = array[0] + MathHelper.Pi * 2 + array[k];
						}
						else
						{
							x = array[k + 1] + array[k];
						}
						x /= 2;
						TSVector2 pos2 = pos;
						TSVector2 point = radius * new TSVector2(FP.Cos(x), FP.Sin(x)) + pos;
						bool hitClosest = false;
						this.World.RayCast(delegate(Fixture f, TSVector2 p, TSVector2 n, FP fr)
						{
							Body body = f.Body;
							bool flag22 = !this.IsActiveOn(body);
							FP result2;
							if (flag22)
							{
								result2 = 0;
							}
							else
							{
								hitClosest = true;
								fixture = f;
								result2 = fr;
							}
							return result2;
						}, pos2, point);
						bool flag10 = hitClosest && fixture.Body.BodyType == BodyType.Dynamic;
						if (flag10)
						{
							bool flag11 = this._data.Any<ShapeData>() && this._data.Last<ShapeData>().Body == fixture.Body && !flag7;
							if (flag11)
							{
								int index = this._data.Count - 1;
								ShapeData value = this._data[index];
								value.Max = array[num2];
								this._data[index] = value;
							}
							else
							{
								ShapeData item2;
								item2.Body = fixture.Body;
								item2.Min = array[k];
								item2.Max = array[num2];
								this._data.Add(item2);
							}
							bool flag12 = this._data.Count > 1 && k == num - 1 && this._data.Last<ShapeData>().Body == this._data.First<ShapeData>().Body && this._data.Last<ShapeData>().Max == this._data.First<ShapeData>().Min;
							if (flag12)
							{
								ShapeData value2 = this._data[0];
								value2.Min = this._data.Last<ShapeData>().Min;
								this._data.RemoveAt(this._data.Count - 1);
								this._data[0] = value2;
								while (this._data.First<ShapeData>().Min >= this._data.First<ShapeData>().Max)
								{
									value2.Min -= MathHelper.Pi * 2;
									this._data[0] = value2;
								}
							}
							int index2 = this._data.Count - 1;
							ShapeData value3 = this._data[index2];
							while (this._data.Count > 0 && this._data.Last<ShapeData>().Min >= this._data.Last<ShapeData>().Max)
							{
								value3.Min = this._data.Last<ShapeData>().Min - 2 * MathHelper.Pi;
								this._data[index2] = value3;
							}
							flag7 = false;
						}
						else
						{
							flag7 = true;
						}
					}
				}
				for (int l = 0; l < this._data.Count; l++)
				{
					bool flag13 = !this.IsActiveOn(this._data[l].Body);
					if (!flag13)
					{
						FP fP5 = this._data[l].Max - this._data[l].Min;
						FP fP6 = MathHelper.Min(RealExplosion.MaxEdgeOffset, this.EdgeRatio * fP5);
						int num3 = FP.Ceiling((fP5 - 2f * fP6 - (this.MinRays - 1) * this.MaxAngle) / this.MaxAngle).AsInt();
						bool flag14 = num3 < 0;
						if (flag14)
						{
							num3 = 0;
						}
						FP y4 = (fP5 - fP6 * 2f) / (this.MinRays + num3 - 1);
						FP fP7 = this._data[l].Min + fP6;
						while (fP7 < this._data[l].Max || MathUtils.FPEquals(fP7, this._data[l].Max, 0.0001f))
						{
							TSVector2 pos3 = pos;
							TSVector2 tSVector3 = pos + radius * new TSVector2(FP.Cos(fP7), FP.Sin(fP7));
							TSVector2 tSVector4 = TSVector2.zero;
							FP fP8 = FP.MaxValue;
							List<Fixture> fixtureList = this._data[l].Body.FixtureList;
							for (int m = 0; m < fixtureList.Count; m++)
							{
								Fixture fixture3 = fixtureList[m];
								RayCastInput rayCastInput;
								rayCastInput.Point1 = pos3;
								rayCastInput.Point2 = tSVector3;
								rayCastInput.MaxFraction = 50f;
								RayCastOutput rayCastOutput;
								bool flag15 = fixture3.RayCast(out rayCastOutput, ref rayCastInput, 0);
								if (flag15)
								{
									bool flag16 = fP8 > rayCastOutput.Fraction;
									if (flag16)
									{
										fP8 = rayCastOutput.Fraction;
										tSVector4 = rayCastOutput.Fraction * tSVector3 + (1 - rayCastOutput.Fraction) * pos3;
									}
								}
								FP scaleFactor = fP5 / (this.MinRays + num3) * maxForce * 180f / MathHelper.Pi * (1f - TSMath.Min(FP.One, fP8));
								TSVector2 tSVector5 = TSVector2.Dot(scaleFactor * new TSVector2(FP.Cos(fP7), FP.Sin(fP7)), -rayCastOutput.Normal) * new TSVector2(FP.Cos(fP7), FP.Sin(fP7));
								this._data[l].Body.ApplyLinearImpulse(ref tSVector5, ref tSVector4);
								bool flag17 = dictionary.ContainsKey(fixture3);
								if (flag17)
								{
									Dictionary<Fixture, TSVector2> dictionary2 = dictionary;
									Fixture key = fixture3;
									dictionary2[key] += tSVector5;
								}
								else
								{
									dictionary.Add(fixture3, tSVector5);
								}
								bool flag18 = fP8 > 1f;
								if (flag18)
								{
									tSVector4 = tSVector3;
								}
							}
							fP7 += y4;
						}
					}
				}
				for (int n2 = 0; n2 < containedShapeCount; n2++)
				{
					Fixture fixture2 = containedShapes[n2];
					bool flag19 = !this.IsActiveOn(fixture2.Body);
					if (!flag19)
					{
						FP scaleFactor2 = this.MinRays * maxForce * 180f / MathHelper.Pi;
						CircleShape circleShape2 = fixture2.Shape as CircleShape;
						bool flag20 = circleShape2 != null;
						TSVector2 worldPoint;
						if (flag20)
						{
							worldPoint = fixture2.Body.GetWorldPoint(circleShape2.Position);
						}
						else
						{
							PolygonShape polygonShape2 = fixture2.Shape as PolygonShape;
							worldPoint = fixture2.Body.GetWorldPoint(polygonShape2.MassData.Centroid);
						}
						TSVector2 value4 = scaleFactor2 * (worldPoint - pos);
						fixture2.Body.ApplyLinearImpulse(ref value4, ref worldPoint);
						bool flag21 = !dictionary.ContainsKey(fixture2);
						if (flag21)
						{
							dictionary.Add(fixture2, value4);
						}
					}
				}
				result = dictionary;
			}
			return result;
		}
	}
}
