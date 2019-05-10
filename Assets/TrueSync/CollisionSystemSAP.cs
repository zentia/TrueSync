using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class CollisionSystemSAP : CollisionSystem
	{
		private class IBroadphaseEntityXCompare : IComparer<IBroadphaseEntity>
		{
			public int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
			{
				FP x = body1.BoundingBox.min.x - body2.BoundingBox.min.x;
				return (x < 0) ? -1 : ((x > 0) ? 1 : 0);
			}
		}

		private List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();

		private List<IBroadphaseEntity> active = new List<IBroadphaseEntity>();

		private CollisionSystemSAP.IBroadphaseEntityXCompare xComparer;

		private bool swapOrder = false;

		private Action<object> detectCallback;

		public CollisionSystemSAP()
		{
			this.xComparer = new CollisionSystemSAP.IBroadphaseEntityXCompare();
			this.detectCallback = new Action<object>(this.DetectCallback);
		}

		public override bool RemoveEntity(IBroadphaseEntity body)
		{
			return this.bodyList.Remove(body);
		}

		public override void AddEntity(IBroadphaseEntity body)
		{
			bool flag = this.bodyList.Contains(body);
			if (flag)
			{
				throw new ArgumentException("The body was already added to the collision system.", "body");
			}
			this.bodyList.Add(body);
		}

		public override void Detect(bool multiThreaded)
		{
			this.bodyList.Sort(this.xComparer);
			this.active.Clear();
			if (multiThreaded)
			{
				for (int i = 0; i < this.bodyList.Count; i++)
				{
					this.AddToActiveMultithreaded(this.bodyList[i], false);
				}
				threadManager.Execute();
			}
			else
			{
				for (int j = 0; j < this.bodyList.Count; j++)
				{
					this.AddToActive(this.bodyList[j], false);
				}
			}
		}

		private void AddToActive(IBroadphaseEntity body, bool addToList)
		{
			FP x = body.BoundingBox.min.x;
			int num = this.active.Count;
			bool isStaticOrInactive = body.IsStaticOrInactive;
			int num2 = 0;
			while (num2 != num)
			{
				IBroadphaseEntity broadphaseEntity = this.active[num2];
				TSBBox boundingBox = broadphaseEntity.BoundingBox;
				bool flag = boundingBox.max.x < x;
				if (flag)
				{
					num--;
					this.active.RemoveAt(num2);
				}
				else
				{
					TSBBox boundingBox2 = body.BoundingBox;
					bool flag2 = (!isStaticOrInactive || !broadphaseEntity.IsStaticOrInactive) && (boundingBox2.max.z >= boundingBox.min.z && boundingBox2.min.z <= boundingBox.max.z) && boundingBox2.max.y >= boundingBox.min.y && boundingBox2.min.y <= boundingBox.max.y;
					if (flag2)
					{
						bool flag3 = base.RaisePassedBroadphase(broadphaseEntity, body);
						if (flag3)
						{
							bool flag4 = this.swapOrder;
							if (flag4)
							{
								this.Detect(body, broadphaseEntity);
							}
							else
							{
								this.Detect(broadphaseEntity, body);
							}
							this.swapOrder = !this.swapOrder;
						}
					}
					num2++;
				}
			}
			this.active.Add(body);
		}

		private void AddToActiveMultithreaded(IBroadphaseEntity body, bool addToList)
		{
			FP x = body.BoundingBox.min.x;
			int num = this.active.Count;
			bool isStaticOrInactive = body.IsStaticOrInactive;
			int num2 = 0;
			while (num2 != num)
			{
				IBroadphaseEntity broadphaseEntity = this.active[num2];
				TSBBox boundingBox = broadphaseEntity.BoundingBox;
				bool flag = boundingBox.max.x < x;
				if (flag)
				{
					num--;
					this.active.RemoveAt(num2);
				}
				else
				{
					TSBBox boundingBox2 = body.BoundingBox;
					bool flag2 = (!isStaticOrInactive || !broadphaseEntity.IsStaticOrInactive) && (boundingBox2.max.z >= boundingBox.min.z && boundingBox2.min.z <= boundingBox.max.z) && boundingBox2.max.y >= boundingBox.min.y && boundingBox2.min.y <= boundingBox.max.y;
					if (flag2)
					{
						bool flag3 = base.RaisePassedBroadphase(broadphaseEntity, body);
						if (flag3)
						{
							CollisionSystem.BroadphasePair @new = CollisionSystem.BroadphasePair.Pool.GetNew();
							bool flag4 = this.swapOrder;
							if (flag4)
							{
								@new.Entity1 = body;
								@new.Entity2 = broadphaseEntity;
							}
							else
							{
								@new.Entity2 = body;
								@new.Entity1 = broadphaseEntity;
							}
							this.swapOrder = !this.swapOrder;
							this.threadManager.AddTask(this.detectCallback, @new);
						}
					}
					num2++;
				}
			}
			this.active.Add(body);
		}

		private void DetectCallback(object obj)
		{
			CollisionSystem.BroadphasePair broadphasePair = obj as CollisionSystem.BroadphasePair;
			base.Detect(broadphasePair.Entity1, broadphasePair.Entity2);
			CollisionSystem.BroadphasePair.Pool.GiveBack(broadphasePair);
		}

		private int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
		{
			FP x = body1.BoundingBox.min.x - body2.BoundingBox.min.x;
			return (x < 0) ? -1 : ((x > 0) ? 1 : 0);
		}

		public override bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out RigidBody body, out TSVector normal, out FP fraction)
		{
			body = null;
			normal = TSVector.zero;
			fraction = FP.MaxValue;
			bool result = false;
			foreach (IBroadphaseEntity current in this.bodyList)
			{
				bool flag = current is SoftBody;
				if (flag)
				{
					SoftBody softBody = current as SoftBody;
					foreach (RigidBody current2 in softBody.VertexBodies)
					{
						TSVector tSVector;
						FP fP;
						bool flag2 = this.Raycast(current2, rayOrigin, rayDirection, out tSVector, out fP);
						if (flag2)
						{
							bool flag3 = fP < fraction && (raycast == null || raycast(current2, tSVector, fP));
							if (flag3)
							{
								body = current2;
								normal = tSVector;
								fraction = fP;
								result = true;
							}
						}
					}
				}
				else
				{
					RigidBody rigidBody = current as RigidBody;
					TSVector tSVector;
					FP fP;
					bool flag4 = this.Raycast(rigidBody, rayOrigin, rayDirection, out tSVector, out fP);
					if (flag4)
					{
						bool flag5 = fP < fraction && (raycast == null || raycast(rigidBody, tSVector, fP));
						if (flag5)
						{
							body = rigidBody;
							normal = tSVector;
							fraction = fP;
							result = true;
						}
					}
				}
			}
			return result;
		}

		public override bool Raycast(RigidBody body, TSVector rayOrigin, TSVector rayDirection, out TSVector normal, out FP fraction)
		{
			fraction = FP.MaxValue;
			normal = TSVector.zero;
			bool flag = !body.BoundingBox.RayIntersect(ref rayOrigin, ref rayDirection);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = body.Shape is Multishape;
				if (flag2)
				{
					Multishape multishape = (body.Shape as Multishape).RequestWorkingClone();
					bool flag3 = false;
					TSVector tSVector;
					TSVector.Subtract(ref rayOrigin, ref body.position, out tSVector);
					TSVector.Transform(ref tSVector, ref body.invOrientation, out tSVector);
					TSVector tSVector2;
					TSVector.Transform(ref rayDirection, ref body.invOrientation, out tSVector2);
					int num = multishape.Prepare(ref tSVector, ref tSVector2);
					for (int i = 0; i < num; i++)
					{
						multishape.SetCurrentShape(i);
						FP fP;
						TSVector tSVector3;
						bool flag4 = GJKCollide.Raycast(multishape, ref body.orientation, ref body.invOrientation, ref body.position, ref rayOrigin, ref rayDirection, out fP, out tSVector3);
						if (flag4)
						{
							bool flag5 = fP < fraction;
							if (flag5)
							{
								bool flag6 = this.useTerrainNormal && multishape is TerrainShape;
								if (flag6)
								{
									(multishape as TerrainShape).CollisionNormal(out tSVector3);
									TSVector.Transform(ref tSVector3, ref body.orientation, out tSVector3);
									tSVector3.Negate();
								}
								else
								{
									bool flag7 = this.useTriangleMeshNormal && multishape is TriangleMeshShape;
									if (flag7)
									{
										(multishape as TriangleMeshShape).CollisionNormal(out tSVector3);
										TSVector.Transform(ref tSVector3, ref body.orientation, out tSVector3);
										tSVector3.Negate();
									}
								}
								normal = tSVector3;
								fraction = fP;
								flag3 = true;
							}
						}
					}
					multishape.ReturnWorkingClone();
					result = flag3;
				}
				else
				{
					result = GJKCollide.Raycast(body.Shape, ref body.orientation, ref body.invOrientation, ref body.position, ref rayOrigin, ref rayDirection, out fraction, out normal);
				}
			}
			return result;
		}
	}
}
