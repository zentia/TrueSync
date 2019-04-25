namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class CollisionSystemPersistentSAPClone
    {
        public List<IBroadphaseEntity> activeList = new List<IBroadphaseEntity>();
        public List<SweetPointClone> axis1 = new List<SweetPointClone>();
        public List<SweetPointClone> axis2 = new List<SweetPointClone>();
        public List<SweetPointClone> axis3 = new List<SweetPointClone>();
        public List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();
        public List<OverlapPair> fullOverlaps = new List<OverlapPair>();
        private int index;
        private int length;
        public static ResourcePoolSweetPointClone poolSweetPointClone = new ResourcePoolSweetPointClone();
        public bool swapOrder;

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
                SweetPointClone item = poolSweetPointClone.GetNew();
                item.Clone(cs.axis1[this.index]);
                this.axis1.Add(item);
                this.index++;
            }
            this.axis2.Clear();
            this.index = 0;
            this.length = cs.axis2.Count;
            while (this.index < this.length)
            {
                SweetPointClone clone2 = poolSweetPointClone.GetNew();
                clone2.Clone(cs.axis2[this.index]);
                this.axis2.Add(clone2);
                this.index++;
            }
            this.axis3.Clear();
            this.index = 0;
            this.length = cs.axis3.Count;
            while (this.index < this.length)
            {
                SweetPointClone clone3 = poolSweetPointClone.GetNew();
                clone3.Clone(cs.axis3[this.index]);
                this.axis3.Add(clone3);
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

        public void Reset()
        {
            this.index = 0;
            this.length = this.axis1.Count;
            while (this.index < this.length)
            {
                poolSweetPointClone.GiveBack(this.axis1[this.index]);
                this.index++;
            }
            this.index = 0;
            this.length = this.axis2.Count;
            while (this.index < this.length)
            {
                poolSweetPointClone.GiveBack(this.axis2[this.index]);
                this.index++;
            }
            this.index = 0;
            this.length = this.axis3.Count;
            while (this.index < this.length)
            {
                poolSweetPointClone.GiveBack(this.axis3[this.index]);
                this.index++;
            }
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
                SweetPointClone clone = this.axis1[this.index];
                SweepPoint sp = new SweepPoint(null, false, 0);
                clone.Restore(sp);
                cs.axis1.Add(sp);
                this.index++;
            }
            cs.axis2.Clear();
            this.index = 0;
            this.length = this.axis2.Count;
            while (this.index < this.length)
            {
                SweetPointClone clone2 = this.axis2[this.index];
                SweepPoint point2 = new SweepPoint(null, false, 0);
                clone2.Restore(point2);
                cs.axis2.Add(point2);
                this.index++;
            }
            cs.axis3.Clear();
            this.index = 0;
            this.length = this.axis3.Count;
            while (this.index < this.length)
            {
                SweetPointClone clone3 = this.axis3[this.index];
                SweepPoint point3 = new SweepPoint(null, false, 0);
                clone3.Restore(point3);
                cs.axis3.Add(point3);
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

