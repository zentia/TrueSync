using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class CollisionSystemPersistentSAP : CollisionSystem
	{
		private const int AddedObjectsBruteForceIsUsed = 250;

		public List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();

		public List<SweepPoint> axis1 = new List<SweepPoint>();

		public List<SweepPoint> axis2 = new List<SweepPoint>();

		public List<SweepPoint> axis3 = new List<SweepPoint>();

		public HashList<OverlapPair> fullOverlaps = new HashList<OverlapPair>();

		private Action<object> detectCallback;

		private Action<object> sortCallback;

		public List<IBroadphaseEntity> activeList = new List<IBroadphaseEntity>();

		public int addCounter = 0;

		public Stack<OverlapPair> depricated = new Stack<OverlapPair>();

		public bool swapOrder = false;

		public CollisionSystemPersistentSAP()
		{
			this.detectCallback = new Action<object>(this.DetectCallback);
			this.sortCallback = new Action<object>(this.SortCallback);
		}

		private int QuickSort(SweepPoint sweepPoint1, SweepPoint sweepPoint2)
		{
			FP value = sweepPoint1.Value;
			FP value2 = sweepPoint2.Value;
			bool flag = value > value2;
			int result;
			if (flag)
			{
				result = 1;
			}
			else
			{
				bool flag2 = value2 > value;
				if (flag2)
				{
					result = -1;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}

		private void DirtySortAxis(List<SweepPoint> axis)
		{
			axis.Sort(new Comparison<SweepPoint>(this.QuickSort));
			this.activeList.Clear();
			for (int i = 0; i < axis.Count; i++)
			{
				SweepPoint sweepPoint = axis[i];
				bool begin = sweepPoint.Begin;
				if (begin)
				{
					foreach (IBroadphaseEntity current in this.activeList)
					{
						bool flag = base.CheckBoundingBoxes(current, sweepPoint.Body);
						if (flag)
						{
							this.fullOverlaps.Add(new OverlapPair(current, sweepPoint.Body));
						}
					}
					this.activeList.Add(sweepPoint.Body);
				}
				else
				{
					this.activeList.Remove(sweepPoint.Body);
				}
			}
		}

		private void SortAxis(List<SweepPoint> axis)
		{
			for (int i = 1; i < axis.Count; i++)
			{
				SweepPoint sweepPoint = axis[i];
				FP value = sweepPoint.Value;
				int num = i - 1;
				while (num >= 0 && axis[num].Value > value)
				{
					SweepPoint sweepPoint2 = axis[num];
					bool flag = sweepPoint.Begin && !sweepPoint2.Begin;
					if (flag)
					{
						bool flag2 = base.CheckBoundingBoxes(sweepPoint2.Body, sweepPoint.Body);
						if (flag2)
						{
							HashList<OverlapPair> obj = this.fullOverlaps;
							lock (obj)
							{
								this.fullOverlaps.Add(new OverlapPair(sweepPoint2.Body, sweepPoint.Body));
							}
						}
					}
					bool flag3 = !sweepPoint.Begin && sweepPoint2.Begin;
					if (flag3)
					{
						HashList<OverlapPair> obj2 = this.fullOverlaps;
						lock (obj2)
						{
							this.fullOverlaps.Remove(new OverlapPair(sweepPoint2.Body, sweepPoint.Body));
						}
					}
					axis[num + 1] = sweepPoint2;
					num--;
				}
				axis[num + 1] = sweepPoint;
			}
		}

		public override void AddEntity(IBroadphaseEntity body)
		{
			this.bodyList.Add(body);
			this.axis1.Add(new SweepPoint(body, true, 0));
			this.axis1.Add(new SweepPoint(body, false, 0));
			this.axis2.Add(new SweepPoint(body, true, 1));
			this.axis2.Add(new SweepPoint(body, false, 1));
			this.axis3.Add(new SweepPoint(body, true, 2));
			this.axis3.Add(new SweepPoint(body, false, 2));
			this.addCounter++;
		}

		public override bool RemoveEntity(IBroadphaseEntity body)
		{
			int num = 0;
			for (int i = 0; i < this.axis1.Count; i++)
			{
				bool flag = this.axis1[i].Body == body;
				if (flag)
				{
					num++;
					this.axis1.RemoveAt(i);
					bool flag2 = num == 2;
					if (flag2)
					{
						break;
					}
					i--;
				}
			}
			num = 0;
			for (int j = 0; j < this.axis2.Count; j++)
			{
				bool flag3 = this.axis2[j].Body == body;
				if (flag3)
				{
					num++;
					this.axis2.RemoveAt(j);
					bool flag4 = num == 2;
					if (flag4)
					{
						break;
					}
					j--;
				}
			}
			num = 0;
			for (int k = 0; k < this.axis3.Count; k++)
			{
				bool flag5 = this.axis3[k].Body == body;
				if (flag5)
				{
					num++;
					this.axis3.RemoveAt(k);
					bool flag6 = num == 2;
					if (flag6)
					{
						break;
					}
					k--;
				}
			}
			foreach (OverlapPair current in this.fullOverlaps)
			{
				bool flag7 = current.Entity1 == body || current.Entity2 == body;
				if (flag7)
				{
					this.depricated.Push(current);
				}
			}
			while (this.depricated.Count > 0)
			{
				this.fullOverlaps.Remove(this.depricated.Pop());
			}
			this.bodyList.Remove(body);
			return true;
		}

		public override void Detect(bool multiThreaded)
		{
			bool flag = this.addCounter > 250;
			if (flag)
			{
				this.fullOverlaps.Clear();
				this.DirtySortAxis(this.axis1);
				this.DirtySortAxis(this.axis2);
				this.DirtySortAxis(this.axis3);
			}
			else if (multiThreaded)
			{
				this.threadManager.AddTask(this.sortCallback, this.axis1);
				this.threadManager.AddTask(this.sortCallback, this.axis2);
				this.threadManager.AddTask(this.sortCallback, this.axis3);
				this.threadManager.Execute();
			}
			else
			{
				this.sortCallback(this.axis1);
				this.sortCallback(this.axis2);
				this.sortCallback(this.axis3);
			}
			this.addCounter = 0;
			foreach (OverlapPair current in this.fullOverlaps)
			{
				bool flag2 = base.CheckBothStaticNonKinematic(current.Entity1, current.Entity2);
				if (!flag2)
				{
					bool flag3 = base.RaisePassedBroadphase(current.Entity1, current.Entity2);
					if (flag3)
					{
						if (multiThreaded)
						{
							CollisionSystem.BroadphasePair @new = CollisionSystem.BroadphasePair.Pool.GetNew();
							bool flag4 = this.swapOrder;
							if (flag4)
							{
								@new.Entity1 = current.Entity1;
								@new.Entity2 = current.Entity2;
							}
							else
							{
								@new.Entity2 = current.Entity2;
								@new.Entity1 = current.Entity1;
							}
							this.threadManager.AddTask(this.detectCallback, @new);
						}
						else
						{
							bool flag5 = this.swapOrder;
							if (flag5)
							{
								this.Detect(current.Entity1, current.Entity2);
							}
							else
							{
								this.Detect(current.Entity2, current.Entity1);
							}
						}
						this.swapOrder = !this.swapOrder;
					}
				}
			}
			if (multiThreaded)
			{
				this.threadManager.Execute();
			}
		}

		private void SortCallback(object obj)
		{
			this.SortAxis(obj as List<SweepPoint>);
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
