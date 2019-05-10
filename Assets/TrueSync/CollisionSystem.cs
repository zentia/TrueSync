using System;
using System.Collections.Generic;

namespace TrueSync
{
	public abstract class CollisionSystem
	{
		protected class BroadphasePair
		{
			public IBroadphaseEntity Entity1;

			public IBroadphaseEntity Entity2;

			public static ResourcePool<BroadphasePair> Pool = new ResourcePool<BroadphasePair>();
		}

		public World world;

		private CollisionDetectedHandler collisionDetected;

		protected ThreadManager threadManager = ThreadManager.Instance;

		private bool speculativeContacts = false;

		internal bool useTerrainNormal = true;

		internal bool useTriangleMeshNormal = true;

		private ResourcePool<List<int>> potentialTriangleLists = new ResourcePool<List<int>>();

		public event PassedBroadphaseHandler PassedBroadphase;

		public event CollisionDetectedHandler CollisionDetected
		{
			add
			{
				this.collisionDetected = (CollisionDetectedHandler)Delegate.Combine(this.collisionDetected, value);
			}
			remove
			{
				this.collisionDetected = (CollisionDetectedHandler)Delegate.Remove(this.collisionDetected, value);
			}
		}

		public bool EnableSpeculativeContacts
		{
			get
			{
				return this.speculativeContacts;
			}
			set
			{
				this.speculativeContacts = value;
			}
		}

		public bool UseTriangleMeshNormal
		{
			get
			{
				return this.useTriangleMeshNormal;
			}
			set
			{
				this.useTriangleMeshNormal = value;
			}
		}

		public bool UseTerrainNormal
		{
			get
			{
				return this.useTerrainNormal;
			}
			set
			{
				this.useTerrainNormal = value;
			}
		}

		public abstract bool RemoveEntity(IBroadphaseEntity body);

		public abstract void AddEntity(IBroadphaseEntity body);

		public CollisionSystem()
		{
		}

		public virtual void Detect(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			RigidBody rigidBody = entity1 as RigidBody;
			RigidBody rigidBody2 = entity2 as RigidBody;
			bool flag = rigidBody != null;
			if (flag)
			{
				bool flag2 = rigidBody2 != null;
				if (flag2)
				{
					this.DetectRigidRigid(rigidBody, rigidBody2);
				}
				else
				{
					SoftBody softBody = entity2 as SoftBody;
					bool flag3 = softBody != null;
					if (flag3)
					{
						this.DetectSoftRigid(rigidBody, softBody);
					}
				}
			}
			else
			{
				SoftBody softBody2 = entity1 as SoftBody;
				bool flag4 = rigidBody2 != null;
				if (flag4)
				{
					bool flag5 = softBody2 != null;
					if (flag5)
					{
						this.DetectSoftRigid(rigidBody2, softBody2);
					}
				}
				else
				{
					SoftBody softBody3 = entity2 as SoftBody;
					bool flag6 = softBody2 != null && softBody3 != null;
					if (flag6)
					{
						this.DetectSoftSoft(softBody2, softBody3);
					}
				}
			}
		}

		private void DetectSoftSoft(SoftBody body1, SoftBody body2)
		{
			List<int> @new = this.potentialTriangleLists.GetNew();
			List<int> new2 = this.potentialTriangleLists.GetNew();
			body1.dynamicTree.Query(new2, @new, body2.dynamicTree);
			for (int i = 0; i < new2.Count; i++)
			{
				SoftBody.Triangle userData = body1.dynamicTree.GetUserData(@new[i]);
				SoftBody.Triangle userData2 = body2.dynamicTree.GetUserData(new2[i]);
				TSVector tSVector;
				TSVector tSVector2;
				FP penetration;
				bool flag = XenoCollide.Detect(userData, userData2, ref TSMatrix.InternalIdentity, ref TSMatrix.InternalIdentity, ref TSVector.InternalZero, ref TSVector.InternalZero, out tSVector, out tSVector2, out penetration);
				bool flag2 = flag;
				if (flag2)
				{
					int index = CollisionSystem.FindNearestTrianglePoint(body1, @new[i], ref tSVector);
					int index2 = CollisionSystem.FindNearestTrianglePoint(body2, new2[i], ref tSVector);
					this.RaiseCollisionDetected(body1.VertexBodies[index], body2.VertexBodies[index2], ref tSVector, ref tSVector, ref tSVector2, penetration);
				}
			}
			@new.Clear();
			new2.Clear();
			this.potentialTriangleLists.GiveBack(@new);
			this.potentialTriangleLists.GiveBack(new2);
		}

		private void DetectRigidRigid(RigidBody body1, RigidBody body2)
		{
			bool flag = body1.Shape is Multishape;
			bool flag2 = body2.Shape is Multishape;
			bool flag3 = this.speculativeContacts || body1.EnableSpeculativeContacts || body2.EnableSpeculativeContacts;
			bool flag4 = !flag && !flag2;
			if (flag4)
			{
				TSVector tSVector;
				TSVector value;
				FP fP;
				bool flag5 = XenoCollide.Detect(body1.Shape, body2.Shape, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out tSVector, out value, out fP);
				if (flag5)
				{
					TSVector tSVector2;
					TSVector tSVector3;
					this.FindSupportPoints(body1, body2, body1.Shape, body2.Shape, ref tSVector, ref value, out tSVector2, out tSVector3);
					this.RaiseCollisionDetected(body1, body2, ref tSVector2, ref tSVector3, ref value, fP);
				}
				else
				{
					bool flag6 = flag3;
					if (flag6)
					{
						TSVector value2;
						TSVector value3;
						bool flag7 = GJKCollide.ClosestPoints(body1.Shape, body2.Shape, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out value2, out value3, out value);
						if (flag7)
						{
							TSVector value4 = value3 - value2;
							bool flag8 = value4.sqrMagnitude < (body1.sweptDirection - body2.sweptDirection).sqrMagnitude;
							if (flag8)
							{
								fP = value4 * value;
								bool flag9 = fP < FP.Zero;
								if (flag9)
								{
									this.RaiseCollisionDetected(body1, body2, ref value2, ref value3, ref value, fP);
								}
							}
						}
					}
				}
			}
			else
			{
				bool flag10 = flag & flag2;
				if (flag10)
				{
					Multishape multishape = body1.Shape as Multishape;
					Multishape multishape2 = body2.Shape as Multishape;
					multishape = multishape.RequestWorkingClone();
					multishape2 = multishape2.RequestWorkingClone();
					TSBBox boundingBox = body2.boundingBox;
					boundingBox.InverseTransform(ref body1.position, ref body1.orientation);
					int num = multishape.Prepare(ref boundingBox);
					boundingBox = body1.boundingBox;
					boundingBox.InverseTransform(ref body2.position, ref body2.orientation);
					int num2 = multishape2.Prepare(ref boundingBox);
					bool flag11 = num == 0 || num2 == 0;
					if (flag11)
					{
						multishape.ReturnWorkingClone();
						multishape2.ReturnWorkingClone();
					}
					else
					{
						for (int i = 0; i < num; i++)
						{
							multishape.SetCurrentShape(i);
							for (int j = 0; j < num2; j++)
							{
								multishape2.SetCurrentShape(j);
								TSVector tSVector;
								TSVector value;
								FP fP;
								bool flag12 = XenoCollide.Detect(multishape, multishape2, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out tSVector, out value, out fP);
								if (flag12)
								{
									TSVector tSVector4;
									TSVector tSVector5;
									this.FindSupportPoints(body1, body2, multishape, multishape2, ref tSVector, ref value, out tSVector4, out tSVector5);
									this.RaiseCollisionDetected(body1, body2, ref tSVector4, ref tSVector5, ref value, fP);
								}
								else
								{
									bool flag13 = flag3;
									if (flag13)
									{
										TSVector value5;
										TSVector value6;
										bool flag14 = GJKCollide.ClosestPoints(multishape, multishape2, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out value5, out value6, out value);
										if (flag14)
										{
											TSVector value7 = value6 - value5;
											bool flag15 = value7.sqrMagnitude < (body1.sweptDirection - body2.sweptDirection).sqrMagnitude;
											if (flag15)
											{
												fP = value7 * value;
												bool flag16 = fP < FP.Zero;
												if (flag16)
												{
													this.RaiseCollisionDetected(body1, body2, ref value5, ref value6, ref value, fP);
												}
											}
										}
									}
								}
							}
						}
						multishape.ReturnWorkingClone();
						multishape2.ReturnWorkingClone();
					}
				}
				else
				{
					bool flag17 = body2.Shape is Multishape;
					RigidBody rigidBody;
					RigidBody rigidBody2;
					if (flag17)
					{
						rigidBody = body2;
						rigidBody2 = body1;
					}
					else
					{
						rigidBody2 = body2;
						rigidBody = body1;
					}
					Multishape multishape3 = rigidBody.Shape as Multishape;
					multishape3 = multishape3.RequestWorkingClone();
					TSBBox boundingBox2 = rigidBody2.boundingBox;
					boundingBox2.InverseTransform(ref rigidBody.position, ref rigidBody.orientation);
					int num3 = multishape3.Prepare(ref boundingBox2);
					bool flag18 = num3 == 0;
					if (flag18)
					{
						multishape3.ReturnWorkingClone();
					}
					else
					{
						for (int k = 0; k < num3; k++)
						{
							multishape3.SetCurrentShape(k);
							TSVector tSVector;
							TSVector value;
							FP fP;
							bool flag19 = XenoCollide.Detect(multishape3, rigidBody2.Shape, ref rigidBody.orientation, ref rigidBody2.orientation, ref rigidBody.position, ref rigidBody2.position, out tSVector, out value, out fP);
							if (flag19)
							{
								TSVector tSVector6;
								TSVector tSVector7;
								this.FindSupportPoints(rigidBody, rigidBody2, multishape3, rigidBody2.Shape, ref tSVector, ref value, out tSVector6, out tSVector7);
								bool flag20 = this.useTerrainNormal && multishape3 is TerrainShape;
								if (flag20)
								{
									(multishape3 as TerrainShape).CollisionNormal(out value);
									TSVector.Transform(ref value, ref rigidBody.orientation, out value);
								}
								else
								{
									bool flag21 = this.useTriangleMeshNormal && multishape3 is TriangleMeshShape;
									if (flag21)
									{
										(multishape3 as TriangleMeshShape).CollisionNormal(out value);
										TSVector.Transform(ref value, ref rigidBody.orientation, out value);
									}
								}
								this.RaiseCollisionDetected(rigidBody, rigidBody2, ref tSVector6, ref tSVector7, ref value, fP);
							}
							else
							{
								bool flag22 = flag3;
								if (flag22)
								{
									TSVector value8;
									TSVector value9;
									bool flag23 = GJKCollide.ClosestPoints(multishape3, rigidBody2.Shape, ref rigidBody.orientation, ref rigidBody2.orientation, ref rigidBody.position, ref rigidBody2.position, out value8, out value9, out value);
									if (flag23)
									{
										TSVector value10 = value9 - value8;
										bool flag24 = value10.sqrMagnitude < (body1.sweptDirection - body2.sweptDirection).sqrMagnitude;
										if (flag24)
										{
											fP = value10 * value;
											bool flag25 = fP < FP.Zero;
											if (flag25)
											{
												this.RaiseCollisionDetected(rigidBody, rigidBody2, ref value8, ref value9, ref value, fP);
											}
										}
									}
								}
							}
						}
						multishape3.ReturnWorkingClone();
					}
				}
			}
		}

		private void DetectSoftRigid(RigidBody rigidBody, SoftBody softBody)
		{
			bool flag = rigidBody.Shape is Multishape;
			if (flag)
			{
				Multishape multishape = rigidBody.Shape as Multishape;
				multishape = multishape.RequestWorkingClone();
				TSBBox boundingBox = softBody.BoundingBox;
				boundingBox.InverseTransform(ref rigidBody.position, ref rigidBody.orientation);
				int num = multishape.Prepare(ref boundingBox);
				List<int> @new = this.potentialTriangleLists.GetNew();
				softBody.dynamicTree.Query(@new, ref rigidBody.boundingBox);
				foreach (int current in @new)
				{
					SoftBody.Triangle userData = softBody.dynamicTree.GetUserData(current);
					for (int i = 0; i < num; i++)
					{
						multishape.SetCurrentShape(i);
						TSVector tSVector;
						TSVector tSVector2;
						FP penetration;
						bool flag2 = XenoCollide.Detect(multishape, userData, ref rigidBody.orientation, ref TSMatrix.InternalIdentity, ref rigidBody.position, ref TSVector.InternalZero, out tSVector, out tSVector2, out penetration);
						bool flag3 = flag2;
						if (flag3)
						{
							int index = CollisionSystem.FindNearestTrianglePoint(softBody, current, ref tSVector);
							this.RaiseCollisionDetected(rigidBody, softBody.VertexBodies[index], ref tSVector, ref tSVector, ref tSVector2, penetration);
						}
					}
				}
				@new.Clear();
				this.potentialTriangleLists.GiveBack(@new);
				multishape.ReturnWorkingClone();
			}
			else
			{
				List<int> new2 = this.potentialTriangleLists.GetNew();
				softBody.dynamicTree.Query(new2, ref rigidBody.boundingBox);
				foreach (int current2 in new2)
				{
					SoftBody.Triangle userData2 = softBody.dynamicTree.GetUserData(current2);
					TSVector tSVector3;
					TSVector tSVector4;
					FP penetration2;
					bool flag4 = XenoCollide.Detect(rigidBody.Shape, userData2, ref rigidBody.orientation, ref TSMatrix.InternalIdentity, ref rigidBody.position, ref TSVector.InternalZero, out tSVector3, out tSVector4, out penetration2);
					bool flag5 = flag4;
					if (flag5)
					{
						int index2 = CollisionSystem.FindNearestTrianglePoint(softBody, current2, ref tSVector3);
						this.RaiseCollisionDetected(rigidBody, softBody.VertexBodies[index2], ref tSVector3, ref tSVector3, ref tSVector4, penetration2);
					}
				}
				new2.Clear();
				this.potentialTriangleLists.GiveBack(new2);
			}
		}

		public static int FindNearestTrianglePoint(SoftBody sb, int id, ref TSVector point)
		{
			SoftBody.Triangle userData = sb.dynamicTree.GetUserData(id);
			TSVector position = sb.VertexBodies[userData.indices.I0].position;
			TSVector.Subtract(ref position, ref point, out position);
			FP sqrMagnitude = position.sqrMagnitude;
			position = sb.VertexBodies[userData.indices.I1].position;
			TSVector.Subtract(ref position, ref point, out position);
			FP sqrMagnitude2 = position.sqrMagnitude;
			position = sb.VertexBodies[userData.indices.I2].position;
			TSVector.Subtract(ref position, ref point, out position);
			FP sqrMagnitude3 = position.sqrMagnitude;
			bool flag = sqrMagnitude < sqrMagnitude2;
			int result;
			if (flag)
			{
				bool flag2 = sqrMagnitude < sqrMagnitude3;
				if (flag2)
				{
					result = userData.indices.I0;
				}
				else
				{
					result = userData.indices.I2;
				}
			}
			else
			{
				bool flag3 = sqrMagnitude2 < sqrMagnitude3;
				if (flag3)
				{
					result = userData.indices.I1;
				}
				else
				{
					result = userData.indices.I2;
				}
			}
			return result;
		}

		private void FindSupportPoints(RigidBody body1, RigidBody body2, Shape shape1, Shape shape2, ref TSVector point, ref TSVector normal, out TSVector point1, out TSVector point2)
		{
			TSVector tSVector;
			TSVector.Negate(ref normal, out tSVector);
			TSVector tSVector2;
			this.SupportMapping(body1, shape1, ref tSVector, out tSVector2);
			TSVector tSVector3;
			this.SupportMapping(body2, shape2, ref normal, out tSVector3);
			TSVector.Subtract(ref tSVector2, ref point, out tSVector2);
			TSVector.Subtract(ref tSVector3, ref point, out tSVector3);
			FP scaleFactor = TSVector.Dot(ref tSVector2, ref normal);
			FP scaleFactor2 = TSVector.Dot(ref tSVector3, ref normal);
			TSVector.Multiply(ref normal, scaleFactor, out tSVector2);
			TSVector.Multiply(ref normal, scaleFactor2, out tSVector3);
			TSVector.Add(ref point, ref tSVector2, out point1);
			TSVector.Add(ref point, ref tSVector3, out point2);
		}

		private void SupportMapping(RigidBody body, Shape workingShape, ref TSVector direction, out TSVector result)
		{
			TSVector.Transform(ref direction, ref body.invOrientation, out result);
			workingShape.SupportMapping(ref result, out result);
			TSVector.Transform(ref result, ref body.orientation, out result);
			TSVector.Add(ref result, ref body.position, out result);
		}

		public abstract bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out RigidBody body, out TSVector normal, out FP fraction);

		public abstract bool Raycast(RigidBody body, TSVector rayOrigin, TSVector rayDirection, out TSVector normal, out FP fraction);

		public bool CheckBothStaticOrInactive(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			return entity1.IsStaticOrInactive && entity2.IsStaticOrInactive;
		}

		public bool CheckBothStaticNonKinematic(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			return !this.world.CanBodiesCollide((RigidBody)entity1, (RigidBody)entity2);
		}

		public bool CheckBoundingBoxes(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			TSBBox boundingBox = entity1.BoundingBox;
			TSBBox boundingBox2 = entity2.BoundingBox;
			return boundingBox.max.z >= boundingBox2.min.z && boundingBox.min.z <= boundingBox2.max.z && boundingBox.max.y >= boundingBox2.min.y && boundingBox.min.y <= boundingBox2.max.y && boundingBox.max.x >= boundingBox2.min.x && boundingBox.min.x <= boundingBox2.max.x;
		}

		public bool RaisePassedBroadphase(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
		{
			bool flag = this.PassedBroadphase != null;
			return !flag || this.PassedBroadphase(entity1, entity2);
		}

		protected void RaiseCollisionDetected(RigidBody body1, RigidBody body2, ref TSVector point1, ref TSVector point2, ref TSVector normal, FP penetration)
		{
			bool flag = this.collisionDetected != null;
			if (flag)
			{
				this.collisionDetected(body1, body2, point1, point2, normal, penetration);
			}
		}

		public abstract void Detect(bool multiThreaded);
	}
}
