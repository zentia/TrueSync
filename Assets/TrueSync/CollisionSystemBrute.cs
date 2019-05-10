using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class CollisionSystemBrute : CollisionSystem
	{
		private List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();

		private Action<object> detectCallback;

		private bool swapOrder = false;

		public CollisionSystemBrute()
		{
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
			int count = this.bodyList.Count;
			if (multiThreaded)
			{
				for (int i = 0; i < count; i++)
				{
					for (int j = i + 1; j < count; j++)
					{
						bool flag = !base.CheckBothStaticOrInactive(this.bodyList[i], this.bodyList[j]) && base.CheckBoundingBoxes(this.bodyList[i], this.bodyList[j]);
						if (flag)
						{
							bool flag2 = base.RaisePassedBroadphase(this.bodyList[i], this.bodyList[j]);
							if (flag2)
							{
								CollisionSystem.BroadphasePair @new = CollisionSystem.BroadphasePair.Pool.GetNew();
								bool flag3 = this.swapOrder;
								if (flag3)
								{
									@new.Entity1 = this.bodyList[i];
									@new.Entity2 = this.bodyList[j];
								}
								else
								{
									@new.Entity2 = this.bodyList[j];
									@new.Entity1 = this.bodyList[i];
								}
								this.swapOrder = !this.swapOrder;
								threadManager.AddTask(detectCallback, @new);
							}
						}
					}
				}
				threadManager.Execute();
			}
			else
			{
				for (int k = 0; k < count; k++)
				{
					for (int l = k + 1; l < count; l++)
					{
						bool flag4 = !base.CheckBothStaticOrInactive(this.bodyList[k], this.bodyList[l]) && base.CheckBoundingBoxes(this.bodyList[k], this.bodyList[l]);
						if (flag4)
						{
							bool flag5 = base.RaisePassedBroadphase(this.bodyList[k], this.bodyList[l]);
							if (flag5)
							{
								bool flag6 = this.swapOrder;
								if (flag6)
								{
									this.Detect(this.bodyList[k], this.bodyList[l]);
								}
								else
								{
									this.Detect(this.bodyList[l], this.bodyList[k]);
								}
								this.swapOrder = !this.swapOrder;
							}
						}
					}
				}
			}
		}

		private void DetectCallback(object obj)
		{
			CollisionSystem.BroadphasePair broadphasePair = obj as CollisionSystem.BroadphasePair;
			base.Detect(broadphasePair.Entity1, broadphasePair.Entity2);
			CollisionSystem.BroadphasePair.Pool.GiveBack(broadphasePair);
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
