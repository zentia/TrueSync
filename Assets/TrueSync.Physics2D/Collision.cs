using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class Collision
	{
		private class EPCollider
		{
			private TempPolygon _polygonB = new TempPolygon();

			private Transform _xf;

			private TSVector2 _centroidB;

			private TSVector2 _v0;

			private TSVector2 _v1;

			private TSVector2 _v2;

			private TSVector2 _v3;

			private TSVector2 _normal0;

			private TSVector2 _normal1;

			private TSVector2 _normal2;

			private TSVector2 _normal;

			private TSVector2 _lowerLimit;

			private TSVector2 _upperLimit;

			private FP _radius;

			private bool _front;

			public void Collide(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
			{
				this._xf = MathUtils.MulT(xfA, xfB);
				this._centroidB = MathUtils.Mul(ref this._xf, polygonB.MassData.Centroid);
				this._v0 = edgeA.Vertex0;
				this._v1 = edgeA._vertex1;
				this._v2 = edgeA._vertex2;
				this._v3 = edgeA.Vertex3;
				bool hasVertex = edgeA.HasVertex0;
				bool hasVertex2 = edgeA.HasVertex3;
				TSVector2 tSVector = this._v2 - this._v1;
				tSVector.Normalize();
				this._normal1 = new TSVector2(tSVector.y, -tSVector.x);
				FP x = TSVector2.Dot(this._normal1, this._centroidB - this._v1);
				FP x2 = 0f;
				FP x3 = 0f;
				bool flag = false;
				bool flag2 = false;
				bool flag3 = hasVertex;
				if (flag3)
				{
					TSVector2 tSVector2 = this._v1 - this._v0;
					tSVector2.Normalize();
					this._normal0 = new TSVector2(tSVector2.y, -tSVector2.x);
					flag = (MathUtils.Cross(tSVector2, tSVector) >= 0f);
					x2 = TSVector2.Dot(this._normal0, this._centroidB - this._v0);
				}
				bool flag4 = hasVertex2;
				if (flag4)
				{
					TSVector2 tSVector3 = this._v3 - this._v2;
					tSVector3.Normalize();
					this._normal2 = new TSVector2(tSVector3.y, -tSVector3.x);
					flag2 = (MathUtils.Cross(tSVector, tSVector3) > 0f);
					x3 = TSVector2.Dot(this._normal2, this._centroidB - this._v2);
				}
				bool flag5 = hasVertex & hasVertex2;
				if (flag5)
				{
					bool flag6 = flag & flag2;
					if (flag6)
					{
						this._front = (x2 >= 0f || x >= 0f || x3 >= 0f);
						bool front = this._front;
						if (front)
						{
							this._normal = this._normal1;
							this._lowerLimit = this._normal0;
							this._upperLimit = this._normal2;
						}
						else
						{
							this._normal = -this._normal1;
							this._lowerLimit = -this._normal1;
							this._upperLimit = -this._normal1;
						}
					}
					else
					{
						bool flag7 = flag;
						if (flag7)
						{
							this._front = (x2 >= 0f || (x >= 0f && x3 >= 0f));
							bool front2 = this._front;
							if (front2)
							{
								this._normal = this._normal1;
								this._lowerLimit = this._normal0;
								this._upperLimit = this._normal1;
							}
							else
							{
								this._normal = -this._normal1;
								this._lowerLimit = -this._normal2;
								this._upperLimit = -this._normal1;
							}
						}
						else
						{
							bool flag8 = flag2;
							if (flag8)
							{
								this._front = (x3 >= 0f || (x2 >= 0f && x >= 0f));
								bool front3 = this._front;
								if (front3)
								{
									this._normal = this._normal1;
									this._lowerLimit = this._normal1;
									this._upperLimit = this._normal2;
								}
								else
								{
									this._normal = -this._normal1;
									this._lowerLimit = -this._normal1;
									this._upperLimit = -this._normal0;
								}
							}
							else
							{
								this._front = (x2 >= 0f && x >= 0f && x3 >= 0f);
								bool front4 = this._front;
								if (front4)
								{
									this._normal = this._normal1;
									this._lowerLimit = this._normal1;
									this._upperLimit = this._normal1;
								}
								else
								{
									this._normal = -this._normal1;
									this._lowerLimit = -this._normal2;
									this._upperLimit = -this._normal0;
								}
							}
						}
					}
				}
				else
				{
					bool flag9 = hasVertex;
					if (flag9)
					{
						bool flag10 = flag;
						if (flag10)
						{
							this._front = (x2 >= 0f || x >= 0f);
							bool front5 = this._front;
							if (front5)
							{
								this._normal = this._normal1;
								this._lowerLimit = this._normal0;
								this._upperLimit = -this._normal1;
							}
							else
							{
								this._normal = -this._normal1;
								this._lowerLimit = this._normal1;
								this._upperLimit = -this._normal1;
							}
						}
						else
						{
							this._front = (x2 >= 0f && x >= 0f);
							bool front6 = this._front;
							if (front6)
							{
								this._normal = this._normal1;
								this._lowerLimit = this._normal1;
								this._upperLimit = -this._normal1;
							}
							else
							{
								this._normal = -this._normal1;
								this._lowerLimit = this._normal1;
								this._upperLimit = -this._normal0;
							}
						}
					}
					else
					{
						bool flag11 = hasVertex2;
						if (flag11)
						{
							bool flag12 = flag2;
							if (flag12)
							{
								this._front = (x >= 0f || x3 >= 0f);
								bool front7 = this._front;
								if (front7)
								{
									this._normal = this._normal1;
									this._lowerLimit = -this._normal1;
									this._upperLimit = this._normal2;
								}
								else
								{
									this._normal = -this._normal1;
									this._lowerLimit = -this._normal1;
									this._upperLimit = this._normal1;
								}
							}
							else
							{
								this._front = (x >= 0f && x3 >= 0f);
								bool front8 = this._front;
								if (front8)
								{
									this._normal = this._normal1;
									this._lowerLimit = -this._normal1;
									this._upperLimit = this._normal1;
								}
								else
								{
									this._normal = -this._normal1;
									this._lowerLimit = -this._normal2;
									this._upperLimit = this._normal1;
								}
							}
						}
						else
						{
							this._front = (x >= 0f);
							bool front9 = this._front;
							if (front9)
							{
								this._normal = this._normal1;
								this._lowerLimit = -this._normal1;
								this._upperLimit = -this._normal1;
							}
							else
							{
								this._normal = -this._normal1;
								this._lowerLimit = this._normal1;
								this._upperLimit = this._normal1;
							}
						}
					}
				}
				this._polygonB.Count = polygonB.Vertices.Count;
				for (int i = 0; i < polygonB.Vertices.Count; i++)
				{
					this._polygonB.Vertices[i] = MathUtils.Mul(ref this._xf, polygonB.Vertices[i]);
					this._polygonB.Normals[i] = MathUtils.Mul(this._xf.q, polygonB.Normals[i]);
				}
				this._radius = 2f * Settings.PolygonRadius;
				manifold.PointCount = 0;
				EPAxis ePAxis = this.ComputeEdgeSeparation();
				bool flag13 = ePAxis.Type == EPAxisType.Unknown;
				if (!flag13)
				{
					bool flag14 = ePAxis.Separation > this._radius;
					if (!flag14)
					{
						EPAxis ePAxis2 = this.ComputePolygonSeparation();
						bool flag15 = ePAxis2.Type != EPAxisType.Unknown && ePAxis2.Separation > this._radius;
						if (!flag15)
						{
							FP x4 = 0.98f;
							FP y = 0.001f;
							bool flag16 = ePAxis2.Type == EPAxisType.Unknown;
							EPAxis ePAxis3;
							if (flag16)
							{
								ePAxis3 = ePAxis;
							}
							else
							{
								bool flag17 = ePAxis2.Separation > x4 * ePAxis.Separation + y;
								if (flag17)
								{
									ePAxis3 = ePAxis2;
								}
								else
								{
									ePAxis3 = ePAxis;
								}
							}
							FixedArray2<ClipVertex> fixedArray = default(FixedArray2<ClipVertex>);
							bool flag18 = ePAxis3.Type == EPAxisType.EdgeA;
							ReferenceFace referenceFace;
							if (flag18)
							{
								manifold.Type = ManifoldType.FaceA;
								int num = 0;
								FP y2 = TSVector2.Dot(this._normal, this._polygonB.Normals[0]);
								for (int j = 1; j < this._polygonB.Count; j++)
								{
									FP fP = TSVector2.Dot(this._normal, this._polygonB.Normals[j]);
									bool flag19 = fP < y2;
									if (flag19)
									{
										y2 = fP;
										num = j;
									}
								}
								int num2 = num;
								int num3 = (num2 + 1 < this._polygonB.Count) ? (num2 + 1) : 0;
								ClipVertex value = fixedArray[0];
								value.V = this._polygonB.Vertices[num2];
								value.ID.Features.IndexA = 0;
								value.ID.Features.IndexB = (byte)num2;
								value.ID.Features.TypeA = 1;
								value.ID.Features.TypeB = 0;
								fixedArray[0] = value;
								ClipVertex value2 = fixedArray[1];
								value2.V = this._polygonB.Vertices[num3];
								value2.ID.Features.IndexA = 0;
								value2.ID.Features.IndexB = (byte)num3;
								value2.ID.Features.TypeA = 1;
								value2.ID.Features.TypeB = 0;
								fixedArray[1] = value2;
								bool front10 = this._front;
								if (front10)
								{
									referenceFace.i1 = 0;
									referenceFace.i2 = 1;
									referenceFace.v1 = this._v1;
									referenceFace.v2 = this._v2;
									referenceFace.normal = this._normal1;
								}
								else
								{
									referenceFace.i1 = 1;
									referenceFace.i2 = 0;
									referenceFace.v1 = this._v2;
									referenceFace.v2 = this._v1;
									referenceFace.normal = -this._normal1;
								}
							}
							else
							{
								manifold.Type = ManifoldType.FaceB;
								ClipVertex value3 = fixedArray[0];
								value3.V = this._v1;
								value3.ID.Features.IndexA = 0;
								value3.ID.Features.IndexB = (byte)ePAxis3.Index;
								value3.ID.Features.TypeA = 0;
								value3.ID.Features.TypeB = 1;
								fixedArray[0] = value3;
								ClipVertex value4 = fixedArray[1];
								value4.V = this._v2;
								value4.ID.Features.IndexA = 0;
								value4.ID.Features.IndexB = (byte)ePAxis3.Index;
								value4.ID.Features.TypeA = 0;
								value4.ID.Features.TypeB = 1;
								fixedArray[1] = value4;
								referenceFace.i1 = ePAxis3.Index;
								referenceFace.i2 = ((referenceFace.i1 + 1 < this._polygonB.Count) ? (referenceFace.i1 + 1) : 0);
								referenceFace.v1 = this._polygonB.Vertices[referenceFace.i1];
								referenceFace.v2 = this._polygonB.Vertices[referenceFace.i2];
								referenceFace.normal = this._polygonB.Normals[referenceFace.i1];
							}
							referenceFace.sideNormal1 = new TSVector2(referenceFace.normal.y, -referenceFace.normal.x);
							referenceFace.sideNormal2 = -referenceFace.sideNormal1;
							referenceFace.sideOffset1 = TSVector2.Dot(referenceFace.sideNormal1, referenceFace.v1);
							referenceFace.sideOffset2 = TSVector2.Dot(referenceFace.sideNormal2, referenceFace.v2);
							FixedArray2<ClipVertex> fixedArray2;
							int num4 = Collision.ClipSegmentToLine(out fixedArray2, ref fixedArray, referenceFace.sideNormal1, referenceFace.sideOffset1, referenceFace.i1);
							bool flag20 = num4 < 2;
							if (!flag20)
							{
								FixedArray2<ClipVertex> fixedArray3;
								num4 = Collision.ClipSegmentToLine(out fixedArray3, ref fixedArray2, referenceFace.sideNormal2, referenceFace.sideOffset2, referenceFace.i2);
								bool flag21 = num4 < 2;
								if (!flag21)
								{
									bool flag22 = ePAxis3.Type == EPAxisType.EdgeA;
									if (flag22)
									{
										manifold.LocalNormal = referenceFace.normal;
										manifold.LocalPoint = referenceFace.v1;
									}
									else
									{
										manifold.LocalNormal = polygonB.Normals[referenceFace.i1];
										manifold.LocalPoint = polygonB.Vertices[referenceFace.i1];
									}
									int num5 = 0;
									for (int k = 0; k < 2; k++)
									{
										FP x5 = TSVector2.Dot(referenceFace.normal, fixedArray3[k].V - referenceFace.v1);
										bool flag23 = x5 <= this._radius;
										if (flag23)
										{
											ManifoldPoint value5 = manifold.Points[num5];
											bool flag24 = ePAxis3.Type == EPAxisType.EdgeA;
											if (flag24)
											{
												value5.LocalPoint = MathUtils.MulT(ref this._xf, fixedArray3[k].V);
												value5.Id = fixedArray3[k].ID;
											}
											else
											{
												value5.LocalPoint = fixedArray3[k].V;
												value5.Id.Features.TypeA = fixedArray3[k].ID.Features.TypeB;
												value5.Id.Features.TypeB = fixedArray3[k].ID.Features.TypeA;
												value5.Id.Features.IndexA = fixedArray3[k].ID.Features.IndexB;
												value5.Id.Features.IndexB = fixedArray3[k].ID.Features.IndexA;
											}
											manifold.Points[num5] = value5;
											num5++;
										}
									}
									manifold.PointCount = num5;
								}
							}
						}
					}
				}
			}

			private EPAxis ComputeEdgeSeparation()
			{
				EPAxis ePAxis;
				ePAxis.Type = EPAxisType.EdgeA;
				ePAxis.Index = (this._front ? 0 : 1);
				ePAxis.Separation = Settings.MaxFP;
				for (int i = 0; i < this._polygonB.Count; i++)
				{
					FP fP = TSVector2.Dot(this._normal, this._polygonB.Vertices[i] - this._v1);
					bool flag = fP < ePAxis.Separation;
					if (flag)
					{
						ePAxis.Separation = fP;
					}
				}
				return ePAxis;
			}

			private EPAxis ComputePolygonSeparation()
			{
				EPAxis ePAxis;
				ePAxis.Type = EPAxisType.Unknown;
				ePAxis.Index = -1;
				ePAxis.Separation = -Settings.MaxFP;
				TSVector2 value = new TSVector2(-this._normal.y, this._normal.x);
				int i = 0;
				EPAxis result;
				while (i < this._polygonB.Count)
				{
					TSVector2 value2 = -this._polygonB.Normals[i];
					FP val = TSVector2.Dot(value2, this._polygonB.Vertices[i] - this._v1);
					FP val2 = TSVector2.Dot(value2, this._polygonB.Vertices[i] - this._v2);
					FP fP = TSMath.Min(val, val2);
					bool flag = fP > this._radius;
					if (flag)
					{
						ePAxis.Type = EPAxisType.EdgeB;
						ePAxis.Index = i;
						ePAxis.Separation = fP;
						result = ePAxis;
						return result;
					}
					bool flag2 = TSVector2.Dot(value2, value) >= 0f;
					if (flag2)
					{
						bool flag3 = TSVector2.Dot(value2 - this._upperLimit, this._normal) < -Settings.AngularSlop;
						if (!flag3)
						{
							goto IL_16B;
						}
					}
					else
					{
						bool flag4 = TSVector2.Dot(value2 - this._lowerLimit, this._normal) < -Settings.AngularSlop;
						if (!flag4)
						{
							goto IL_16B;
						}
					}
					IL_19A:
					i++;
					continue;
					IL_16B:
					bool flag5 = fP > ePAxis.Separation;
					if (flag5)
					{
						ePAxis.Type = EPAxisType.EdgeB;
						ePAxis.Index = i;
						ePAxis.Separation = fP;
					}
					goto IL_19A;
				}
				result = ePAxis;
				return result;
			}
		}

		[ThreadStatic]
		private static DistanceInput _input;

		public static bool TestOverlap(Shape shapeA, int indexA, Shape shapeB, int indexB, ref Transform xfA, ref Transform xfB)
		{
			Collision._input = (Collision._input ?? new DistanceInput());
			Collision._input.ProxyA.Set(shapeA, indexA);
			Collision._input.ProxyB.Set(shapeB, indexB);
			Collision._input.TransformA = xfA;
			Collision._input.TransformB = xfB;
			Collision._input.UseRadii = true;
			DistanceOutput distanceOutput;
			SimplexCache simplexCache;
			Distance.ComputeDistance(out distanceOutput, out simplexCache, Collision._input);
			return distanceOutput.Distance < 10f * Settings.Epsilon;
		}

		public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2, ref Manifold manifold1, ref Manifold manifold2)
		{
			state1 = default(FixedArray2<PointState>);
			state2 = default(FixedArray2<PointState>);
			for (int i = 0; i < manifold1.PointCount; i++)
			{
				ContactID id = manifold1.Points[i].Id;
				state1[i] = PointState.Remove;
				for (int j = 0; j < manifold2.PointCount; j++)
				{
					bool flag = manifold2.Points[j].Id.Key == id.Key;
					if (flag)
					{
						state1[i] = PointState.Persist;
						break;
					}
				}
			}
			for (int k = 0; k < manifold2.PointCount; k++)
			{
				ContactID id2 = manifold2.Points[k].Id;
				state2[k] = PointState.Add;
				for (int l = 0; l < manifold1.PointCount; l++)
				{
					bool flag2 = manifold1.Points[l].Id.Key == id2.Key;
					if (flag2)
					{
						state2[k] = PointState.Persist;
						break;
					}
				}
			}
		}

		public static void CollideCircles(ref Manifold manifold, CircleShape circleA, ref Transform xfA, CircleShape circleB, ref Transform xfB)
		{
			manifold.PointCount = 0;
			TSVector2 value = MathUtils.Mul(ref xfA, circleA.Position);
			TSVector2 value2 = MathUtils.Mul(ref xfB, circleB.Position);
			TSVector2 tSVector = value2 - value;
			FP x = TSVector2.Dot(tSVector, tSVector);
			FP fP = circleA.Radius + circleB.Radius;
			bool flag = x > fP * fP;
			if (!flag)
			{
				manifold.Type = ManifoldType.Circles;
				manifold.LocalPoint = circleA.Position;
				manifold.LocalNormal = TSVector2.zero;
				manifold.PointCount = 1;
				ManifoldPoint value3 = manifold.Points[0];
				value3.LocalPoint = circleB.Position;
				value3.Id.Key = 0u;
				manifold.Points[0] = value3;
			}
		}

		public static void CollidePolygonAndCircle(ref Manifold manifold, PolygonShape polygonA, ref Transform xfA, CircleShape circleB, ref Transform xfB)
		{
			manifold.PointCount = 0;
			TSVector2 v = MathUtils.Mul(ref xfB, circleB.Position);
			TSVector2 tSVector = MathUtils.MulT(ref xfA, v);
			int num = 0;
			FP fP = -Settings.MaxFP;
			FP fP2 = polygonA.Radius + circleB.Radius;
			int count = polygonA.Vertices.Count;
			for (int i = 0; i < count; i++)
			{
				TSVector2 tSVector2 = polygonA.Normals[i];
				TSVector2 tSVector3 = tSVector - polygonA.Vertices[i];
				FP fP3 = tSVector2.x * tSVector3.x + tSVector2.y * tSVector3.y;
				bool flag = fP3 > fP2;
				if (flag)
				{
					return;
				}
				bool flag2 = fP3 > fP;
				if (flag2)
				{
					fP = fP3;
					num = i;
				}
			}
			int num2 = num;
			int index = (num2 + 1 < count) ? (num2 + 1) : 0;
			TSVector2 tSVector4 = polygonA.Vertices[num2];
			TSVector2 tSVector5 = polygonA.Vertices[index];
			bool flag3 = fP < Settings.Epsilon;
			if (flag3)
			{
				manifold.PointCount = 1;
				manifold.Type = ManifoldType.FaceA;
				manifold.LocalNormal = polygonA.Normals[num];
				manifold.LocalPoint = 0.5f * (tSVector4 + tSVector5);
				ManifoldPoint value = manifold.Points[0];
				value.LocalPoint = circleB.Position;
				value.Id.Key = 0u;
				manifold.Points[0] = value;
				return;
			}
			FP x = (tSVector.x - tSVector4.x) * (tSVector5.x - tSVector4.x) + (tSVector.y - tSVector4.y) * (tSVector5.y - tSVector4.y);
			FP x2 = (tSVector.x - tSVector5.x) * (tSVector4.x - tSVector5.x) + (tSVector.y - tSVector5.y) * (tSVector4.y - tSVector5.y);
			bool flag4 = x <= 0f;
			if (flag4)
			{
				FP x3 = (tSVector.x - tSVector4.x) * (tSVector.x - tSVector4.x) + (tSVector.y - tSVector4.y) * (tSVector.y - tSVector4.y);
				bool flag5 = x3 > fP2 * fP2;
				if (flag5)
				{
					return;
				}
				manifold.PointCount = 1;
				manifold.Type = ManifoldType.FaceA;
				manifold.LocalNormal = tSVector - tSVector4;
				FP y = 1f / FP.Sqrt(manifold.LocalNormal.x * manifold.LocalNormal.x + manifold.LocalNormal.y * manifold.LocalNormal.y);
				manifold.LocalNormal.x = manifold.LocalNormal.x * y;
				manifold.LocalNormal.y = manifold.LocalNormal.y * y;
				manifold.LocalPoint = tSVector4;
				ManifoldPoint value2 = manifold.Points[0];
				value2.LocalPoint = circleB.Position;
				value2.Id.Key = 0u;
				manifold.Points[0] = value2;
				return;
			}
			else
			{
				bool flag6 = x2 <= 0f;
				if (flag6)
				{
					FP x4 = (tSVector.x - tSVector5.x) * (tSVector.x - tSVector5.x) + (tSVector.y - tSVector5.y) * (tSVector.y - tSVector5.y);
					bool flag7 = x4 > fP2 * fP2;
					if (flag7)
					{
						return;
					}
					manifold.PointCount = 1;
					manifold.Type = ManifoldType.FaceA;
					manifold.LocalNormal = tSVector - tSVector5;
					FP y2 = 1f / FP.Sqrt(manifold.LocalNormal.x * manifold.LocalNormal.x + manifold.LocalNormal.y * manifold.LocalNormal.y);
					manifold.LocalNormal.x = manifold.LocalNormal.x * y2;
					manifold.LocalNormal.y = manifold.LocalNormal.y * y2;
					manifold.LocalPoint = tSVector5;
					ManifoldPoint value3 = manifold.Points[0];
					value3.LocalPoint = circleB.Position;
					value3.Id.Key = 0u;
					manifold.Points[0] = value3;
					return;
				}
				else
				{
					TSVector2 tSVector6 = 0.5f * (tSVector4 + tSVector5);
					TSVector2 tSVector7 = tSVector - tSVector6;
					TSVector2 tSVector8 = polygonA.Normals[num2];
					FP x5 = tSVector7.x * tSVector8.x + tSVector7.y * tSVector8.y;
					bool flag8 = x5 > fP2;
					if (flag8)
					{
						return;
					}
					manifold.PointCount = 1;
					manifold.Type = ManifoldType.FaceA;
					manifold.LocalNormal = polygonA.Normals[num2];
					manifold.LocalPoint = tSVector6;
					ManifoldPoint value4 = manifold.Points[0];
					value4.LocalPoint = circleB.Position;
					value4.Id.Key = 0u;
					manifold.Points[0] = value4;
					return;
				}
			}
		}

		public static void CollidePolygons(ref Manifold manifold, PolygonShape polyA, ref Transform transformA, PolygonShape polyB, ref Transform transformB)
		{
			manifold.PointCount = 0;
			FP y = polyA.Radius + polyB.Radius;
			int num = 0;
			FP fP = Collision.FindMaxSeparation(out num, polyA, ref transformA, polyB, ref transformB);
			bool flag = fP > y;
			if (!flag)
			{
				int num2 = 0;
				FP x = Collision.FindMaxSeparation(out num2, polyB, ref transformB, polyA, ref transformA);
				bool flag2 = x > y;
				if (!flag2)
				{
					FP x2 = 0.98f;
					FP y2 = 0.001f;
					bool flag3 = x > x2 * fP + y2;
					PolygonShape polygonShape;
					PolygonShape poly;
					Transform transform;
					Transform transform2;
					int num3;
					bool flag4;
					if (flag3)
					{
						polygonShape = polyB;
						poly = polyA;
						transform = transformB;
						transform2 = transformA;
						num3 = num2;
						manifold.Type = ManifoldType.FaceB;
						flag4 = true;
					}
					else
					{
						polygonShape = polyA;
						poly = polyB;
						transform = transformA;
						transform2 = transformB;
						num3 = num;
						manifold.Type = ManifoldType.FaceA;
						flag4 = false;
					}
					FixedArray2<ClipVertex> fixedArray;
					Collision.FindIncidentEdge(out fixedArray, polygonShape, ref transform, num3, poly, ref transform2);
					int count = polygonShape.Vertices.Count;
					int num4 = num3;
					int num5 = (num3 + 1 < count) ? (num3 + 1) : 0;
					TSVector2 tSVector = polygonShape.Vertices[num4];
					TSVector2 tSVector2 = polygonShape.Vertices[num5];
					TSVector2 tSVector3 = tSVector2 - tSVector;
					tSVector3.Normalize();
					TSVector2 localNormal = new TSVector2(tSVector3.y, -tSVector3.x);
					TSVector2 localPoint = 0.5f * (tSVector + tSVector2);
					TSVector2 tSVector4 = MathUtils.Mul(transform.q, tSVector3);
					FP y3 = tSVector4.y;
					FP x3 = -tSVector4.x;
					tSVector = MathUtils.Mul(ref transform, tSVector);
					tSVector2 = MathUtils.Mul(ref transform, tSVector2);
					FP y4 = y3 * tSVector.x + x3 * tSVector.y;
					FP offset = -(tSVector4.x * tSVector.x + tSVector4.y * tSVector.y) + y;
					FP offset2 = tSVector4.x * tSVector2.x + tSVector4.y * tSVector2.y + y;
					FixedArray2<ClipVertex> fixedArray2;
					int num6 = Collision.ClipSegmentToLine(out fixedArray2, ref fixedArray, -tSVector4, offset, num4);
					bool flag5 = num6 < 2;
					if (!flag5)
					{
						FixedArray2<ClipVertex> fixedArray3;
						num6 = Collision.ClipSegmentToLine(out fixedArray3, ref fixedArray2, tSVector4, offset2, num5);
						bool flag6 = num6 < 2;
						if (!flag6)
						{
							manifold.LocalNormal = localNormal;
							manifold.LocalPoint = localPoint;
							int num7 = 0;
							for (int i = 0; i < 2; i++)
							{
								TSVector2 v = fixedArray3[i].V;
								FP x4 = y3 * v.x + x3 * v.y - y4;
								bool flag7 = x4 <= y;
								if (flag7)
								{
									ManifoldPoint manifoldPoint = manifold.Points[num7];
									manifoldPoint.LocalPoint = MathUtils.MulT(ref transform2, fixedArray3[i].V);
									manifoldPoint.Id = fixedArray3[i].ID;
									bool flag8 = flag4;
									if (flag8)
									{
										ContactFeature features = manifoldPoint.Id.Features;
										manifoldPoint.Id.Features.IndexA = features.IndexB;
										manifoldPoint.Id.Features.IndexB = features.IndexA;
										manifoldPoint.Id.Features.TypeA = features.TypeB;
										manifoldPoint.Id.Features.TypeB = features.TypeA;
									}
									manifold.Points[num7] = manifoldPoint;
									num7++;
								}
							}
							manifold.PointCount = num7;
						}
					}
				}
			}
		}

		public static void CollideEdgeAndCircle(ref Manifold manifold, EdgeShape edgeA, ref Transform transformA, CircleShape circleB, ref Transform transformB)
		{
			manifold.PointCount = 0;
			TSVector2 tSVector = MathUtils.MulT(ref transformA, MathUtils.Mul(ref transformB, ref circleB._position));
			TSVector2 vertex = edgeA.Vertex1;
			TSVector2 vertex2 = edgeA.Vertex2;
			TSVector2 tSVector2 = vertex2 - vertex;
			FP fP = TSVector2.Dot(tSVector2, vertex2 - tSVector);
			FP fP2 = TSVector2.Dot(tSVector2, tSVector - vertex);
			FP fP3 = edgeA.Radius + circleB.Radius;
			ContactFeature features;
			features.IndexB = 0;
			features.TypeB = 0;
			bool flag = fP2 <= 0f;
			if (flag)
			{
				TSVector2 tSVector3 = vertex;
				TSVector2 tSVector4 = tSVector - tSVector3;
				FP x;
				TSVector2.Dot(ref tSVector4, ref tSVector4, out x);
				bool flag2 = x > fP3 * fP3;
				if (!flag2)
				{
					bool hasVertex = edgeA.HasVertex0;
					if (hasVertex)
					{
						TSVector2 vertex3 = edgeA.Vertex0;
						TSVector2 value = vertex;
						TSVector2 value2 = value - vertex3;
						FP x2 = TSVector2.Dot(value2, value - tSVector);
						bool flag3 = x2 > 0f;
						if (flag3)
						{
							return;
						}
					}
					features.IndexA = 0;
					features.TypeA = 0;
					manifold.PointCount = 1;
					manifold.Type = ManifoldType.Circles;
					manifold.LocalNormal = TSVector2.zero;
					manifold.LocalPoint = tSVector3;
					ManifoldPoint value3 = default(ManifoldPoint);
					value3.Id.Key = 0u;
					value3.Id.Features = features;
					value3.LocalPoint = circleB.Position;
					manifold.Points[0] = value3;
				}
			}
			else
			{
				bool flag4 = fP <= 0f;
				if (flag4)
				{
					TSVector2 tSVector3 = vertex2;
					TSVector2 tSVector4 = tSVector - tSVector3;
					FP x3;
					TSVector2.Dot(ref tSVector4, ref tSVector4, out x3);
					bool flag5 = x3 > fP3 * fP3;
					if (!flag5)
					{
						bool hasVertex2 = edgeA.HasVertex3;
						if (hasVertex2)
						{
							TSVector2 vertex4 = edgeA.Vertex3;
							TSVector2 value4 = vertex2;
							TSVector2 value5 = vertex4 - value4;
							FP x4 = TSVector2.Dot(value5, tSVector - value4);
							bool flag6 = x4 > 0f;
							if (flag6)
							{
								return;
							}
						}
						features.IndexA = 1;
						features.TypeA = 0;
						manifold.PointCount = 1;
						manifold.Type = ManifoldType.Circles;
						manifold.LocalNormal = TSVector2.zero;
						manifold.LocalPoint = tSVector3;
						ManifoldPoint value6 = default(ManifoldPoint);
						value6.Id.Key = 0u;
						value6.Id.Features = features;
						value6.LocalPoint = circleB.Position;
						manifold.Points[0] = value6;
					}
				}
				else
				{
					FP fP4;
					TSVector2.Dot(ref tSVector2, ref tSVector2, out fP4);
					Debug.Assert(fP4 > 0f);
					TSVector2 tSVector3 = 1f / fP4 * (fP * vertex + fP2 * vertex2);
					TSVector2 tSVector4 = tSVector - tSVector3;
					FP x5;
					TSVector2.Dot(ref tSVector4, ref tSVector4, out x5);
					bool flag7 = x5 > fP3 * fP3;
					if (!flag7)
					{
						TSVector2 tSVector5 = new TSVector2(-tSVector2.y, tSVector2.x);
						bool flag8 = TSVector2.Dot(tSVector5, tSVector - vertex) < 0f;
						if (flag8)
						{
							tSVector5 = new TSVector2(-tSVector5.x, -tSVector5.y);
						}
						tSVector5.Normalize();
						features.IndexA = 0;
						features.TypeA = 1;
						manifold.PointCount = 1;
						manifold.Type = ManifoldType.FaceA;
						manifold.LocalNormal = tSVector5;
						manifold.LocalPoint = vertex;
						ManifoldPoint value7 = default(ManifoldPoint);
						value7.Id.Key = 0u;
						value7.Id.Features = features;
						value7.LocalPoint = circleB.Position;
						manifold.Points[0] = value7;
					}
				}
			}
		}

		public static void CollideEdgeAndPolygon(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
		{
			Collision.EPCollider ePCollider = new Collision.EPCollider();
			ePCollider.Collide(ref manifold, edgeA, ref xfA, polygonB, ref xfB);
		}

		private static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn, TSVector2 normal, FP offset, int vertexIndexA)
		{
			vOut = default(FixedArray2<ClipVertex>);
			ClipVertex clipVertex = vIn[0];
			ClipVertex clipVertex2 = vIn[1];
			int num = 0;
			FP x = normal.x * clipVertex.V.x + normal.y * clipVertex.V.y - offset;
			FP fP = normal.x * clipVertex2.V.x + normal.y * clipVertex2.V.y - offset;
			bool flag = x <= 0f;
			if (flag)
			{
				vOut[num++] = clipVertex;
			}
			bool flag2 = fP <= 0f;
			if (flag2)
			{
				vOut[num++] = clipVertex2;
			}
			bool flag3 = x * fP < 0f;
			if (flag3)
			{
				FP x2 = x / (x - fP);
				ClipVertex value = vOut[num];
				value.V.x = clipVertex.V.x + x2 * (clipVertex2.V.x - clipVertex.V.x);
				value.V.y = clipVertex.V.y + x2 * (clipVertex2.V.y - clipVertex.V.y);
				value.ID.Features.IndexA = (byte)vertexIndexA;
				value.ID.Features.IndexB = clipVertex.ID.Features.IndexB;
				value.ID.Features.TypeA = 0;
				value.ID.Features.TypeB = 1;
				vOut[num] = value;
				num++;
			}
			return num;
		}

		private static FP EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
		{
			List<TSVector2> vertices = poly1.Vertices;
			List<TSVector2> normals = poly1.Normals;
			int count = poly2.Vertices.Count;
			List<TSVector2> vertices2 = poly2.Vertices;
			Debug.Assert(0 <= edge1 && edge1 < poly1.Vertices.Count);
			TSVector2 tSVector = MathUtils.Mul(xf1.q, normals[edge1]);
			TSVector2 value = MathUtils.MulT(xf2.q, tSVector);
			int index = 0;
			FP y = Settings.MaxFP;
			for (int i = 0; i < count; i++)
			{
				FP fP = TSVector2.Dot(vertices2[i], value);
				bool flag = fP < y;
				if (flag)
				{
					y = fP;
					index = i;
				}
			}
			TSVector2 value2 = MathUtils.Mul(ref xf1, vertices[edge1]);
			TSVector2 value3 = MathUtils.Mul(ref xf2, vertices2[index]);
			return TSVector2.Dot(value3 - value2, tSVector);
		}

		private static FP FindMaxSeparation(out int edgeIndex, PolygonShape poly1, ref Transform xf1, PolygonShape poly2, ref Transform xf2)
		{
			int count = poly1.Vertices.Count;
			List<TSVector2> normals = poly1.Normals;
			TSVector2 v = MathUtils.Mul(ref xf2, poly2.MassData.Centroid) - MathUtils.Mul(ref xf1, poly1.MassData.Centroid);
			TSVector2 value = MathUtils.MulT(xf1.q, v);
			int num = 0;
			FP y = -Settings.MaxFP;
			for (int i = 0; i < count; i++)
			{
				FP fP = TSVector2.Dot(normals[i], value);
				bool flag = fP > y;
				if (flag)
				{
					y = fP;
					num = i;
				}
			}
			FP fP2 = Collision.EdgeSeparation(poly1, ref xf1, num, poly2, ref xf2);
			int num2 = (num - 1 >= 0) ? (num - 1) : (count - 1);
			FP fP3 = Collision.EdgeSeparation(poly1, ref xf1, num2, poly2, ref xf2);
			int num3 = (num + 1 < count) ? (num + 1) : 0;
			FP fP4 = Collision.EdgeSeparation(poly1, ref xf1, num3, poly2, ref xf2);
			bool flag2 = fP3 > fP2 && fP3 > fP4;
			int num4;
			int num5;
			FP fP5;
			FP result;
			if (flag2)
			{
				num4 = -1;
				num5 = num2;
				fP5 = fP3;
			}
			else
			{
				bool flag3 = fP4 > fP2;
				if (!flag3)
				{
					edgeIndex = num;
					result = fP2;
					return result;
				}
				num4 = 1;
				num5 = num3;
				fP5 = fP4;
			}
			while (true)
			{
				bool flag4 = num4 == -1;
				if (flag4)
				{
					num = ((num5 - 1 >= 0) ? (num5 - 1) : (count - 1));
				}
				else
				{
					num = ((num5 + 1 < count) ? (num5 + 1) : 0);
				}
				fP2 = Collision.EdgeSeparation(poly1, ref xf1, num, poly2, ref xf2);
				bool flag5 = fP2 > fP5;
				if (!flag5)
				{
					break;
				}
				num5 = num;
				fP5 = fP2;
			}
			edgeIndex = num5;
			result = fP5;
			return result;
		}

		private static void FindIncidentEdge(out FixedArray2<ClipVertex> c, PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
		{
			c = default(FixedArray2<ClipVertex>);
			Vertices normals = poly1.Normals;
			int count = poly2.Vertices.Count;
			Vertices vertices = poly2.Vertices;
			Vertices normals2 = poly2.Normals;
			Debug.Assert(0 <= edge1 && edge1 < poly1.Vertices.Count);
			TSVector2 value = MathUtils.MulT(xf2.q, MathUtils.Mul(xf1.q, normals[edge1]));
			int num = 0;
			FP y = Settings.MaxFP;
			for (int i = 0; i < count; i++)
			{
				FP fP = TSVector2.Dot(value, normals2[i]);
				bool flag = fP < y;
				if (flag)
				{
					y = fP;
					num = i;
				}
			}
			int num2 = num;
			int num3 = (num2 + 1 < count) ? (num2 + 1) : 0;
			ClipVertex value2 = c[0];
			value2.V = MathUtils.Mul(ref xf2, vertices[num2]);
			value2.ID.Features.IndexA = (byte)edge1;
			value2.ID.Features.IndexB = (byte)num2;
			value2.ID.Features.TypeA = 1;
			value2.ID.Features.TypeB = 0;
			c[0] = value2;
			ClipVertex value3 = c[1];
			value3.V = MathUtils.Mul(ref xf2, vertices[num3]);
			value3.ID.Features.IndexA = (byte)edge1;
			value3.ID.Features.IndexB = (byte)num3;
			value3.ID.Features.TypeA = 1;
			value3.ID.Features.TypeB = 0;
			c[1] = value3;
		}
	}
}
