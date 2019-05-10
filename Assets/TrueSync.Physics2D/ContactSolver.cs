using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class ContactSolver
	{
		public static class WorldManifold
		{
			public static void Initialize(ref Manifold manifold, ref Transform xfA, FP radiusA, ref Transform xfB, FP radiusB, out TSVector2 normal, out FixedArray2<TSVector2> points)
			{
				normal = TSVector2.zero;
				points = default(FixedArray2<TSVector2>);
				bool flag = manifold.PointCount == 0;
				if (!flag)
				{
					switch (manifold.Type)
					{
					case ManifoldType.Circles:
					{
						normal = new TSVector2(1f, 0f);
						TSVector2 tSVector = MathUtils.Mul(ref xfA, manifold.LocalPoint);
						TSVector2 tSVector2 = MathUtils.Mul(ref xfB, manifold.Points[0].LocalPoint);
						bool flag2 = TSVector2.DistanceSquared(tSVector, tSVector2) > Settings.EpsilonSqr;
						if (flag2)
						{
							normal = tSVector2 - tSVector;
							normal.Normalize();
						}
						TSVector2 value = tSVector + radiusA * normal;
						TSVector2 value2 = tSVector2 - radiusB * normal;
						points[0] = 0.5f * (value + value2);
						break;
					}
					case ManifoldType.FaceA:
					{
						normal = MathUtils.Mul(xfA.q, manifold.LocalNormal);
						TSVector2 value3 = MathUtils.Mul(ref xfA, manifold.LocalPoint);
						for (int i = 0; i < manifold.PointCount; i++)
						{
							TSVector2 value4 = MathUtils.Mul(ref xfB, manifold.Points[i].LocalPoint);
							TSVector2 value5 = value4 + (radiusA - TSVector2.Dot(value4 - value3, normal)) * normal;
							TSVector2 value6 = value4 - radiusB * normal;
							points[i] = 0.5f * (value5 + value6);
						}
						break;
					}
					case ManifoldType.FaceB:
					{
						normal = MathUtils.Mul(xfB.q, manifold.LocalNormal);
						TSVector2 value7 = MathUtils.Mul(ref xfB, manifold.LocalPoint);
						for (int j = 0; j < manifold.PointCount; j++)
						{
							TSVector2 value8 = MathUtils.Mul(ref xfA, manifold.Points[j].LocalPoint);
							TSVector2 value9 = value8 + (radiusB - TSVector2.Dot(value8 - value7, normal)) * normal;
							TSVector2 value10 = value8 - radiusA * normal;
							points[j] = 0.5f * (value10 + value9);
						}
						normal = -normal;
						break;
					}
					}
				}
			}
		}

		private static class PositionSolverManifold
		{
			public static void Initialize(ContactPositionConstraint pc, Transform xfA, Transform xfB, int index, out TSVector2 normal, out TSVector2 point, out FP separation)
			{
				Debug.Assert(pc.pointCount > 0);
				switch (pc.type)
				{
				case ManifoldType.Circles:
				{
					TSVector2 tSVector = MathUtils.Mul(ref xfA, pc.localPoint);
					TSVector2 tSVector2 = MathUtils.Mul(ref xfB, pc.localPoints[0]);
					normal = tSVector2 - tSVector;
					normal.Normalize();
					point = 0.5f * (tSVector + tSVector2);
					separation = TSVector2.Dot(tSVector2 - tSVector, normal) - pc.radiusA - pc.radiusB;
					break;
				}
				case ManifoldType.FaceA:
				{
					normal = MathUtils.Mul(xfA.q, pc.localNormal);
					TSVector2 value = MathUtils.Mul(ref xfA, pc.localPoint);
					TSVector2 tSVector3 = MathUtils.Mul(ref xfB, pc.localPoints[index]);
					separation = TSVector2.Dot(tSVector3 - value, normal) - pc.radiusA - pc.radiusB;
					point = tSVector3;
					break;
				}
				case ManifoldType.FaceB:
				{
					normal = MathUtils.Mul(xfB.q, pc.localNormal);
					TSVector2 value2 = MathUtils.Mul(ref xfB, pc.localPoint);
					TSVector2 tSVector4 = MathUtils.Mul(ref xfA, pc.localPoints[index]);
					separation = TSVector2.Dot(tSVector4 - value2, normal) - pc.radiusA - pc.radiusB;
					point = tSVector4;
					normal = -normal;
					break;
				}
				default:
					normal = TSVector2.zero;
					point = TSVector2.zero;
					separation = 0;
					break;
				}
			}
		}

		public TimeStep _step;

		public Position[] _positions;

		public Velocity[] _velocities;

		public ContactPositionConstraint[] _positionConstraints;

		public ContactVelocityConstraint[] _velocityConstraints;

		public Contact[] _contacts;

		public int _count;

		public void Reset(TimeStep step, int count, Contact[] contacts, Position[] positions, Velocity[] velocities, bool warmstarting = true)
		{
			this._step = step;
			this._count = count;
			this._positions = positions;
			this._velocities = velocities;
			this._contacts = contacts;
			bool flag = this._velocityConstraints == null || this._velocityConstraints.Length < count;
			if (flag)
			{
				this._velocityConstraints = new ContactVelocityConstraint[count * 2];
				this._positionConstraints = new ContactPositionConstraint[count * 2];
				for (int i = 0; i < this._velocityConstraints.Length; i++)
				{
					this._velocityConstraints[i] = new ContactVelocityConstraint();
				}
				for (int j = 0; j < this._positionConstraints.Length; j++)
				{
					this._positionConstraints[j] = new ContactPositionConstraint();
				}
			}
			for (int k = 0; k < this._count; k++)
			{
				Contact contact = contacts[k];
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				Shape shape = fixtureA.Shape;
				Shape shape2 = fixtureB.Shape;
				FP radius = shape.Radius;
				FP radius2 = shape2.Radius;
				Body body = fixtureA.Body;
				Body body2 = fixtureB.Body;
				Manifold manifold = contact.Manifold;
				int pointCount = manifold.PointCount;
				Debug.Assert(pointCount > 0);
				ContactVelocityConstraint contactVelocityConstraint = this._velocityConstraints[k];
				contactVelocityConstraint.friction = contact.Friction;
				contactVelocityConstraint.restitution = contact.Restitution;
				contactVelocityConstraint.tangentSpeed = contact.TangentSpeed;
				contactVelocityConstraint.indexA = body.IslandIndex;
				contactVelocityConstraint.indexB = body2.IslandIndex;
				contactVelocityConstraint.invMassA = body._invMass;
				contactVelocityConstraint.invMassB = body2._invMass;
				contactVelocityConstraint.invIA = body._invI;
				contactVelocityConstraint.invIB = body2._invI;
				contactVelocityConstraint.contactIndex = k;
				contactVelocityConstraint.pointCount = pointCount;
				contactVelocityConstraint.K.SetZero();
				contactVelocityConstraint.normalMass.SetZero();
				ContactPositionConstraint contactPositionConstraint = this._positionConstraints[k];
				contactPositionConstraint.indexA = body.IslandIndex;
				contactPositionConstraint.indexB = body2.IslandIndex;
				contactPositionConstraint.invMassA = body._invMass;
				contactPositionConstraint.invMassB = body2._invMass;
				contactPositionConstraint.localCenterA = body._sweep.LocalCenter;
				contactPositionConstraint.localCenterB = body2._sweep.LocalCenter;
				contactPositionConstraint.invIA = body._invI;
				contactPositionConstraint.invIB = body2._invI;
				contactPositionConstraint.localNormal = manifold.LocalNormal;
				contactPositionConstraint.localPoint = manifold.LocalPoint;
				contactPositionConstraint.pointCount = pointCount;
				contactPositionConstraint.radiusA = radius;
				contactPositionConstraint.radiusB = radius2;
				contactPositionConstraint.type = manifold.Type;
				for (int l = 0; l < pointCount; l++)
				{
					ManifoldPoint manifoldPoint = manifold.Points[l];
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[l];
					velocityConstraintPoint.normalImpulse = this._step.dtRatio * manifoldPoint.NormalImpulse;
					velocityConstraintPoint.tangentImpulse = this._step.dtRatio * manifoldPoint.TangentImpulse;
					velocityConstraintPoint.rA = TSVector2.zero;
					velocityConstraintPoint.rB = TSVector2.zero;
					velocityConstraintPoint.normalMass = 0f;
					velocityConstraintPoint.tangentMass = 0f;
					velocityConstraintPoint.velocityBias = 0f;
					contactPositionConstraint.localPoints[l] = manifoldPoint.LocalPoint;
				}
			}
		}

		public void InitializeVelocityConstraints()
		{
			for (int i = 0; i < this._count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = this._velocityConstraints[i];
				ContactPositionConstraint contactPositionConstraint = this._positionConstraints[i];
				FP radiusA = contactPositionConstraint.radiusA;
				FP radiusB = contactPositionConstraint.radiusB;
				Manifold manifold = this._contacts[contactVelocityConstraint.contactIndex].Manifold;
				int indexA = contactVelocityConstraint.indexA;
				int indexB = contactVelocityConstraint.indexB;
				FP invMassA = contactVelocityConstraint.invMassA;
				FP invMassB = contactVelocityConstraint.invMassB;
				FP invIA = contactVelocityConstraint.invIA;
				FP invIB = contactVelocityConstraint.invIB;
				TSVector2 localCenterA = contactPositionConstraint.localCenterA;
				TSVector2 localCenterB = contactPositionConstraint.localCenterB;
				TSVector2 c = this._positions[indexA].c;
				FP a = this._positions[indexA].a;
				TSVector2 v = this._velocities[indexA].v;
				FP w = this._velocities[indexA].w;
				TSVector2 c2 = this._positions[indexB].c;
				FP a2 = this._positions[indexB].a;
				TSVector2 v2 = this._velocities[indexB].v;
				FP w2 = this._velocities[indexB].w;
				Debug.Assert(manifold.PointCount > 0);
				Transform transform = default(Transform);
				Transform transform2 = default(Transform);
				transform.q.Set(a);
				transform2.q.Set(a2);
				transform.p = c - MathUtils.Mul(transform.q, localCenterA);
				transform2.p = c2 - MathUtils.Mul(transform2.q, localCenterB);
				TSVector2 normal;
				FixedArray2<TSVector2> fixedArray;
				ContactSolver.WorldManifold.Initialize(ref manifold, ref transform, radiusA, ref transform2, radiusB, out normal, out fixedArray);
				contactVelocityConstraint.normal = normal;
				int pointCount = contactVelocityConstraint.pointCount;
				for (int j = 0; j < pointCount; j++)
				{
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[j];
					velocityConstraintPoint.rA = fixedArray[j] - c;
					velocityConstraintPoint.rB = fixedArray[j] - c2;
					FP y = MathUtils.Cross(velocityConstraintPoint.rA, contactVelocityConstraint.normal);
					FP y2 = MathUtils.Cross(velocityConstraintPoint.rB, contactVelocityConstraint.normal);
					FP fP = invMassA + invMassB + invIA * y * y + invIB * y2 * y2;
					velocityConstraintPoint.normalMass = ((fP > 0f) ? (1f / fP) : 0f);
					TSVector2 b = MathUtils.Cross(contactVelocityConstraint.normal, 1f);
					FP y3 = MathUtils.Cross(velocityConstraintPoint.rA, b);
					FP y4 = MathUtils.Cross(velocityConstraintPoint.rB, b);
					FP fP2 = invMassA + invMassB + invIA * y3 * y3 + invIB * y4 * y4;
					velocityConstraintPoint.tangentMass = ((fP2 > 0f) ? (1f / fP2) : 0f);
					velocityConstraintPoint.velocityBias = 0f;
					FP fP3 = TSVector2.Dot(contactVelocityConstraint.normal, v2 + MathUtils.Cross(w2, velocityConstraintPoint.rB) - v - MathUtils.Cross(w, velocityConstraintPoint.rA));
					bool flag = fP3 < -Settings.VelocityThreshold;
					if (flag)
					{
						velocityConstraintPoint.velocityBias = -contactVelocityConstraint.restitution * fP3;
					}
				}
				bool flag2 = contactVelocityConstraint.pointCount == 2;
				if (flag2)
				{
					VelocityConstraintPoint velocityConstraintPoint2 = contactVelocityConstraint.points[0];
					VelocityConstraintPoint velocityConstraintPoint3 = contactVelocityConstraint.points[1];
					FP y5 = MathUtils.Cross(velocityConstraintPoint2.rA, contactVelocityConstraint.normal);
					FP y6 = MathUtils.Cross(velocityConstraintPoint2.rB, contactVelocityConstraint.normal);
					FP y7 = MathUtils.Cross(velocityConstraintPoint3.rA, contactVelocityConstraint.normal);
					FP y8 = MathUtils.Cross(velocityConstraintPoint3.rB, contactVelocityConstraint.normal);
					FP fP4 = invMassA + invMassB + invIA * y5 * y5 + invIB * y6 * y6;
					FP y9 = invMassA + invMassB + invIA * y7 * y7 + invIB * y8 * y8;
					FP fP5 = invMassA + invMassB + invIA * y5 * y7 + invIB * y6 * y8;
					FP x = 1000f;
					bool flag3 = fP4 * fP4 < x * (fP4 * y9 - fP5 * fP5);
					if (flag3)
					{
						contactVelocityConstraint.K.ex = new TSVector2(fP4, fP5);
						contactVelocityConstraint.K.ey = new TSVector2(fP5, y9);
						contactVelocityConstraint.normalMass = contactVelocityConstraint.K.Inverse;
					}
					else
					{
						contactVelocityConstraint.pointCount = 1;
					}
				}
			}
		}

		public void WarmStart()
		{
			for (int i = 0; i < this._count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = this._velocityConstraints[i];
				int indexA = contactVelocityConstraint.indexA;
				int indexB = contactVelocityConstraint.indexB;
				FP invMassA = contactVelocityConstraint.invMassA;
				FP invIA = contactVelocityConstraint.invIA;
				FP invMassB = contactVelocityConstraint.invMassB;
				FP invIB = contactVelocityConstraint.invIB;
				int pointCount = contactVelocityConstraint.pointCount;
				TSVector2 tSVector = this._velocities[indexA].v;
				FP fP = this._velocities[indexA].w;
				TSVector2 tSVector2 = this._velocities[indexB].v;
				FP fP2 = this._velocities[indexB].w;
				TSVector2 normal = contactVelocityConstraint.normal;
				TSVector2 value = MathUtils.Cross(normal, 1f);
				for (int j = 0; j < pointCount; j++)
				{
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[j];
					TSVector2 tSVector3 = velocityConstraintPoint.normalImpulse * normal + velocityConstraintPoint.tangentImpulse * value;
					fP -= invIA * MathUtils.Cross(velocityConstraintPoint.rA, tSVector3);
					tSVector -= invMassA * tSVector3;
					fP2 += invIB * MathUtils.Cross(velocityConstraintPoint.rB, tSVector3);
					tSVector2 += invMassB * tSVector3;
				}
				this._velocities[indexA].v = tSVector;
				this._velocities[indexA].w = fP;
				this._velocities[indexB].v = tSVector2;
				this._velocities[indexB].w = fP2;
			}
		}

		public void SolveVelocityConstraints()
		{
			for (int i = 0; i < this._count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = this._velocityConstraints[i];
				int indexA = contactVelocityConstraint.indexA;
				int indexB = contactVelocityConstraint.indexB;
				FP invMassA = contactVelocityConstraint.invMassA;
				FP invIA = contactVelocityConstraint.invIA;
				FP invMassB = contactVelocityConstraint.invMassB;
				FP invIB = contactVelocityConstraint.invIB;
				int pointCount = contactVelocityConstraint.pointCount;
				TSVector2 tSVector = this._velocities[indexA].v;
				FP fP = this._velocities[indexA].w;
				TSVector2 tSVector2 = this._velocities[indexB].v;
				FP fP2 = this._velocities[indexB].w;
				TSVector2 normal = contactVelocityConstraint.normal;
				TSVector2 tSVector3 = MathUtils.Cross(normal, 1f);
				FP friction = contactVelocityConstraint.friction;
				Debug.Assert(pointCount == 1 || pointCount == 2);
				for (int j = 0; j < pointCount; j++)
				{
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[j];
					TSVector2 value = tSVector2 + MathUtils.Cross(fP2, velocityConstraintPoint.rB) - tSVector - MathUtils.Cross(fP, velocityConstraintPoint.rA);
					FP x = TSVector2.Dot(value, tSVector3) - contactVelocityConstraint.tangentSpeed;
					FP fP3 = velocityConstraintPoint.tangentMass * -x;
					FP fP4 = friction * velocityConstraintPoint.normalImpulse;
					FP fP5 = MathUtils.Clamp(velocityConstraintPoint.tangentImpulse + fP3, -fP4, fP4);
					fP3 = fP5 - velocityConstraintPoint.tangentImpulse;
					velocityConstraintPoint.tangentImpulse = fP5;
					TSVector2 tSVector4 = fP3 * tSVector3;
					tSVector -= invMassA * tSVector4;
					fP -= invIA * MathUtils.Cross(velocityConstraintPoint.rA, tSVector4);
					tSVector2 += invMassB * tSVector4;
					fP2 += invIB * MathUtils.Cross(velocityConstraintPoint.rB, tSVector4);
				}
				bool flag = contactVelocityConstraint.pointCount == 1;
				if (flag)
				{
					VelocityConstraintPoint velocityConstraintPoint2 = contactVelocityConstraint.points[0];
					TSVector2 value2 = tSVector2 + MathUtils.Cross(fP2, velocityConstraintPoint2.rB) - tSVector - MathUtils.Cross(fP, velocityConstraintPoint2.rA);
					FP x2 = TSVector2.Dot(value2, normal);
					FP fP6 = -velocityConstraintPoint2.normalMass * (x2 - velocityConstraintPoint2.velocityBias);
					FP fP7 = TSMath.Max(velocityConstraintPoint2.normalImpulse + fP6, 0f);
					fP6 = fP7 - velocityConstraintPoint2.normalImpulse;
					velocityConstraintPoint2.normalImpulse = fP7;
					TSVector2 tSVector5 = fP6 * normal;
					tSVector -= invMassA * tSVector5;
					fP -= invIA * MathUtils.Cross(velocityConstraintPoint2.rA, tSVector5);
					tSVector2 += invMassB * tSVector5;
					fP2 += invIB * MathUtils.Cross(velocityConstraintPoint2.rB, tSVector5);
				}
				else
				{
					VelocityConstraintPoint velocityConstraintPoint3 = contactVelocityConstraint.points[0];
					VelocityConstraintPoint velocityConstraintPoint4 = contactVelocityConstraint.points[1];
					TSVector2 tSVector6 = new TSVector2(velocityConstraintPoint3.normalImpulse, velocityConstraintPoint4.normalImpulse);
					Debug.Assert(tSVector6.x >= 0f && tSVector6.y >= 0f);
					TSVector2 value3 = tSVector2 + MathUtils.Cross(fP2, velocityConstraintPoint3.rB) - tSVector - MathUtils.Cross(fP, velocityConstraintPoint3.rA);
					TSVector2 value4 = tSVector2 + MathUtils.Cross(fP2, velocityConstraintPoint4.rB) - tSVector - MathUtils.Cross(fP, velocityConstraintPoint4.rA);
					FP x3 = TSVector2.Dot(value3, normal);
					FP x4 = TSVector2.Dot(value4, normal);
					TSVector2 tSVector7 = new TSVector2
					{
						x = x3 - velocityConstraintPoint3.velocityBias,
						y = x4 - velocityConstraintPoint4.velocityBias
					} - MathUtils.Mul(ref contactVelocityConstraint.K, tSVector6);
					TSVector2 tSVector8 = -MathUtils.Mul(ref contactVelocityConstraint.normalMass, tSVector7);
					bool flag2 = tSVector8.x >= 0f && tSVector8.y >= 0f;
					if (flag2)
					{
						TSVector2 tSVector9 = tSVector8 - tSVector6;
						TSVector2 tSVector10 = tSVector9.x * normal;
						TSVector2 tSVector11 = tSVector9.y * normal;
						tSVector -= invMassA * (tSVector10 + tSVector11);
						fP -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, tSVector10) + MathUtils.Cross(velocityConstraintPoint4.rA, tSVector11));
						tSVector2 += invMassB * (tSVector10 + tSVector11);
						fP2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, tSVector10) + MathUtils.Cross(velocityConstraintPoint4.rB, tSVector11));
						velocityConstraintPoint3.normalImpulse = tSVector8.x;
						velocityConstraintPoint4.normalImpulse = tSVector8.y;
					}
					else
					{
						tSVector8.x = -velocityConstraintPoint3.normalMass * tSVector7.x;
						tSVector8.y = 0f;
						x3 = 0f;
						x4 = contactVelocityConstraint.K.ex.y * tSVector8.x + tSVector7.y;
						bool flag3 = tSVector8.x >= 0f && x4 >= 0f;
						if (flag3)
						{
							TSVector2 tSVector12 = tSVector8 - tSVector6;
							TSVector2 tSVector13 = tSVector12.x * normal;
							TSVector2 tSVector14 = tSVector12.y * normal;
							tSVector -= invMassA * (tSVector13 + tSVector14);
							fP -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, tSVector13) + MathUtils.Cross(velocityConstraintPoint4.rA, tSVector14));
							tSVector2 += invMassB * (tSVector13 + tSVector14);
							fP2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, tSVector13) + MathUtils.Cross(velocityConstraintPoint4.rB, tSVector14));
							velocityConstraintPoint3.normalImpulse = tSVector8.x;
							velocityConstraintPoint4.normalImpulse = tSVector8.y;
						}
						else
						{
							tSVector8.x = 0f;
							tSVector8.y = -velocityConstraintPoint4.normalMass * tSVector7.y;
							x3 = contactVelocityConstraint.K.ey.x * tSVector8.y + tSVector7.x;
							x4 = 0f;
							bool flag4 = tSVector8.y >= 0f && x3 >= 0f;
							if (flag4)
							{
								TSVector2 tSVector15 = tSVector8 - tSVector6;
								TSVector2 tSVector16 = tSVector15.x * normal;
								TSVector2 tSVector17 = tSVector15.y * normal;
								tSVector -= invMassA * (tSVector16 + tSVector17);
								fP -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, tSVector16) + MathUtils.Cross(velocityConstraintPoint4.rA, tSVector17));
								tSVector2 += invMassB * (tSVector16 + tSVector17);
								fP2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, tSVector16) + MathUtils.Cross(velocityConstraintPoint4.rB, tSVector17));
								velocityConstraintPoint3.normalImpulse = tSVector8.x;
								velocityConstraintPoint4.normalImpulse = tSVector8.y;
							}
							else
							{
								tSVector8.x = 0f;
								tSVector8.y = 0f;
								x3 = tSVector7.x;
								x4 = tSVector7.y;
								bool flag5 = x3 >= 0f && x4 >= 0f;
								if (flag5)
								{
									TSVector2 tSVector18 = tSVector8 - tSVector6;
									TSVector2 tSVector19 = tSVector18.x * normal;
									TSVector2 tSVector20 = tSVector18.y * normal;
									tSVector -= invMassA * (tSVector19 + tSVector20);
									fP -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, tSVector19) + MathUtils.Cross(velocityConstraintPoint4.rA, tSVector20));
									tSVector2 += invMassB * (tSVector19 + tSVector20);
									fP2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, tSVector19) + MathUtils.Cross(velocityConstraintPoint4.rB, tSVector20));
									velocityConstraintPoint3.normalImpulse = tSVector8.x;
									velocityConstraintPoint4.normalImpulse = tSVector8.y;
								}
							}
						}
					}
				}
				this._velocities[indexA].v = tSVector;
				this._velocities[indexA].w = fP;
				this._velocities[indexB].v = tSVector2;
				this._velocities[indexB].w = fP2;
			}
		}

		public void StoreImpulses()
		{
			for (int i = 0; i < this._count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = this._velocityConstraints[i];
				Manifold manifold = this._contacts[contactVelocityConstraint.contactIndex].Manifold;
				for (int j = 0; j < contactVelocityConstraint.pointCount; j++)
				{
					ManifoldPoint value = manifold.Points[j];
					value.NormalImpulse = contactVelocityConstraint.points[j].normalImpulse;
					value.TangentImpulse = contactVelocityConstraint.points[j].tangentImpulse;
					manifold.Points[j] = value;
				}
				this._contacts[contactVelocityConstraint.contactIndex].Manifold = manifold;
			}
		}

		public bool SolvePositionConstraints()
		{
			FP fP = 0f;
			for (int i = 0; i < this._count; i++)
			{
				ContactPositionConstraint contactPositionConstraint = this._positionConstraints[i];
				int indexA = contactPositionConstraint.indexA;
				int indexB = contactPositionConstraint.indexB;
				TSVector2 localCenterA = contactPositionConstraint.localCenterA;
				FP invMassA = contactPositionConstraint.invMassA;
				FP invIA = contactPositionConstraint.invIA;
				TSVector2 localCenterB = contactPositionConstraint.localCenterB;
				FP invMassB = contactPositionConstraint.invMassB;
				FP invIB = contactPositionConstraint.invIB;
				int pointCount = contactPositionConstraint.pointCount;
				TSVector2 tSVector = this._positions[indexA].c;
				FP fP2 = this._positions[indexA].a;
				TSVector2 tSVector2 = this._positions[indexB].c;
				FP fP3 = this._positions[indexB].a;
				for (int j = 0; j < pointCount; j++)
				{
					Transform transform = default(Transform);
					Transform transform2 = default(Transform);
					transform.q.Set(fP2);
					transform2.q.Set(fP3);
					transform.p = tSVector - MathUtils.Mul(transform.q, localCenterA);
					transform2.p = tSVector2 - MathUtils.Mul(transform2.q, localCenterB);
					TSVector2 tSVector3;
					TSVector2 value;
					FP fP4;
					ContactSolver.PositionSolverManifold.Initialize(contactPositionConstraint, transform, transform2, j, out tSVector3, out value, out fP4);
					TSVector2 a = value - tSVector;
					TSVector2 a2 = value - tSVector2;
					fP = TSMath.Min(fP, fP4);
					FP x = MathUtils.Clamp(Settings.Baumgarte * (fP4 + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0f);
					FP y = MathUtils.Cross(a, tSVector3);
					FP y2 = MathUtils.Cross(a2, tSVector3);
					FP fP5 = invMassA + invMassB + invIA * y * y + invIB * y2 * y2;
					FP scaleFactor = (fP5 > 0f) ? (-x / fP5) : 0f;
					TSVector2 tSVector4 = scaleFactor * tSVector3;
					tSVector -= invMassA * tSVector4;
					fP2 -= invIA * MathUtils.Cross(a, tSVector4);
					tSVector2 += invMassB * tSVector4;
					fP3 += invIB * MathUtils.Cross(a2, tSVector4);
				}
				this._positions[indexA].c = tSVector;
				this._positions[indexA].a = fP2;
				this._positions[indexB].c = tSVector2;
				this._positions[indexB].a = fP3;
			}
			return fP >= -3f * Settings.LinearSlop;
		}

		public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
		{
			FP fP = 0f;
			for (int i = 0; i < this._count; i++)
			{
				ContactPositionConstraint contactPositionConstraint = this._positionConstraints[i];
				int indexA = contactPositionConstraint.indexA;
				int indexB = contactPositionConstraint.indexB;
				TSVector2 localCenterA = contactPositionConstraint.localCenterA;
				TSVector2 localCenterB = contactPositionConstraint.localCenterB;
				int pointCount = contactPositionConstraint.pointCount;
				FP fP2 = 0f;
				FP x = 0f;
				bool flag = indexA == toiIndexA || indexA == toiIndexB;
				if (flag)
				{
					fP2 = contactPositionConstraint.invMassA;
					x = contactPositionConstraint.invIA;
				}
				FP fP3 = 0f;
				FP x2 = 0f;
				bool flag2 = indexB == toiIndexA || indexB == toiIndexB;
				if (flag2)
				{
					fP3 = contactPositionConstraint.invMassB;
					x2 = contactPositionConstraint.invIB;
				}
				TSVector2 tSVector = this._positions[indexA].c;
				FP fP4 = this._positions[indexA].a;
				TSVector2 tSVector2 = this._positions[indexB].c;
				FP fP5 = this._positions[indexB].a;
				for (int j = 0; j < pointCount; j++)
				{
					Transform transform = default(Transform);
					Transform transform2 = default(Transform);
					transform.q.Set(fP4);
					transform2.q.Set(fP5);
					transform.p = tSVector - MathUtils.Mul(transform.q, localCenterA);
					transform2.p = tSVector2 - MathUtils.Mul(transform2.q, localCenterB);
					TSVector2 tSVector3;
					TSVector2 value;
					FP fP6;
					ContactSolver.PositionSolverManifold.Initialize(contactPositionConstraint, transform, transform2, j, out tSVector3, out value, out fP6);
					TSVector2 a = value - tSVector;
					TSVector2 a2 = value - tSVector2;
					fP = TSMath.Min(fP, fP6);
					FP x3 = MathUtils.Clamp(Settings.Baumgarte * (fP6 + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0f);
					FP y = MathUtils.Cross(a, tSVector3);
					FP y2 = MathUtils.Cross(a2, tSVector3);
					FP fP7 = fP2 + fP3 + x * y * y + x2 * y2 * y2;
					FP scaleFactor = (fP7 > 0f) ? (-x3 / fP7) : 0f;
					TSVector2 tSVector4 = scaleFactor * tSVector3;
					tSVector -= fP2 * tSVector4;
					fP4 -= x * MathUtils.Cross(a, tSVector4);
					tSVector2 += fP3 * tSVector4;
					fP5 += x2 * MathUtils.Cross(a2, tSVector4);
				}
				this._positions[indexA].c = tSVector;
				this._positions[indexA].a = fP4;
				this._positions[indexB].c = tSVector2;
				this._positions[indexB].a = fP5;
			}
			return fP >= -1.5f * Settings.LinearSlop;
		}
	}
}
