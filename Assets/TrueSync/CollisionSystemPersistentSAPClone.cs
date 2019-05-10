using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class CollisionSystemPersistentSAPClone
	{
		public static ResourcePoolSweetPointClone poolSweetPointClone = new ResourcePoolSweetPointClone();

		public List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();

		public List<SweetPointClone> axis1 = new List<SweetPointClone>();

		public List<SweetPointClone> axis2 = new List<SweetPointClone>();

		public List<SweetPointClone> axis3 = new List<SweetPointClone>();

		public List<OverlapPair> fullOverlaps = new List<OverlapPair>();

		public List<IBroadphaseEntity> activeList = new List<IBroadphaseEntity>();

		public bool swapOrder;

		private int index;

		private int length;

		public void Reset()
		{
			this.index = 0;
			this.length = this.axis1.Count;
			while (this.index < this.length)
			{
				CollisionSystemPersistentSAPClone.poolSweetPointClone.GiveBack(this.axis1[this.index]);
				this.index++;
			}
			this.index = 0;
			this.length = this.axis2.Count;
			while (this.index < this.length)
			{
				CollisionSystemPersistentSAPClone.poolSweetPointClone.GiveBack(this.axis2[this.index]);
				this.index++;
			}
			this.index = 0;
			this.length = this.axis3.Count;
			while (this.index < this.length)
			{
				CollisionSystemPersistentSAPClone.poolSweetPointClone.GiveBack(this.axis3[this.index]);
				this.index++;
			}
		}

		public void Clone(CollisionSystemPersistentSAP cs)
		{
			this.bodyList.Clear();
			this.index = 0;
			this.length = cs.bodyList.Count;
			while (this.index < this.length)
			{
				this.bodyList.Add(cs.bodyList[this.index]);
				this.index++;
			}
			this.axis1.Clear();
			this.index = 0;
			this.length = cs.axis1.Count;
			while (this.index < this.length)
			{
				SweetPointClone @new = CollisionSystemPersistentSAPClone.poolSweetPointClone.GetNew();
				@new.Clone(cs.axis1[this.index]);
				this.axis1.Add(@new);
				this.index++;
			}
			this.axis2.Clear();
			this.index = 0;
			this.length = cs.axis2.Count;
			while (this.index < this.length)
			{
				SweetPointClone new2 = CollisionSystemPersistentSAPClone.poolSweetPointClone.GetNew();
				new2.Clone(cs.axis2[this.index]);
				this.axis2.Add(new2);
				this.index++;
			}
			this.axis3.Clear();
			this.index = 0;
			this.length = cs.axis3.Count;
			while (this.index < this.length)
			{
				SweetPointClone new3 = CollisionSystemPersistentSAPClone.poolSweetPointClone.GetNew();
				new3.Clone(cs.axis3[this.index]);
				this.axis3.Add(new3);
				this.index++;
			}
			this.fullOverlaps.Clear();
			this.index = 0;
			this.length = cs.fullOverlaps.Count;
			while (this.index < this.length)
			{
				this.fullOverlaps.Add(cs.fullOverlaps[this.index]);
				this.index++;
			}
			this.activeList.Clear();
			this.index = 0;
			this.length = cs.activeList.Count;
			while (this.index < this.length)
			{
				this.activeList.Add(cs.activeList[this.index]);
				this.index++;
			}
			this.swapOrder = cs.swapOrder;
		}

		public void Restore(CollisionSystemPersistentSAP cs)
		{
			cs.bodyList.Clear();
			cs.bodyList.AddRange(this.bodyList);
			cs.axis1.Clear();
			this.index = 0;
			this.length = this.axis1.Count;
			while (this.index < this.length)
			{
				SweetPointClone sweetPointClone = this.axis1[this.index];
				SweepPoint sweepPoint = new SweepPoint(null, false, 0);
				sweetPointClone.Restore(sweepPoint);
				cs.axis1.Add(sweepPoint);
				this.index++;
			}
			cs.axis2.Clear();
			this.index = 0;
			this.length = this.axis2.Count;
			while (this.index < this.length)
			{
				SweetPointClone sweetPointClone2 = this.axis2[this.index];
				SweepPoint sweepPoint2 = new SweepPoint(null, false, 0);
				sweetPointClone2.Restore(sweepPoint2);
				cs.axis2.Add(sweepPoint2);
				this.index++;
			}
			cs.axis3.Clear();
			this.index = 0;
			this.length = this.axis3.Count;
			while (this.index < this.length)
			{
				SweetPointClone sweetPointClone3 = this.axis3[this.index];
				SweepPoint sweepPoint3 = new SweepPoint(null, false, 0);
				sweetPointClone3.Restore(sweepPoint3);
				cs.axis3.Add(sweepPoint3);
				this.index++;
			}
			cs.fullOverlaps.Clear();
			cs.fullOverlaps.AddRange(this.fullOverlaps);
			cs.activeList.Clear();
			cs.activeList.AddRange(this.activeList);
			cs.swapOrder = this.swapOrder;
		}
	}
}
