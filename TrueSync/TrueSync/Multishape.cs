namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public abstract class Multishape : Shape
    {
        internal bool isClone = false;
        private Stack<Multishape> workingCloneStack = new Stack<Multishape>();

        protected Multishape()
        {
        }

        public override void CalculateMassInertia()
        {
            TSVector vector;
            base.geomCen = TSVector.zero;
            base.inertia = TSMatrix.Identity;
            TSVector.Subtract(ref this.boundingBox.max, ref this.boundingBox.min, out vector);
            base.mass = (vector.x * vector.y) * vector.z;
            this.inertia.M11 = ((FP.One / (12 * FP.One)) * base.mass) * ((vector.y * vector.y) + (vector.z * vector.z));
            this.inertia.M22 = ((FP.One / (12 * FP.One)) * base.mass) * ((vector.x * vector.x) + (vector.z * vector.z));
            this.inertia.M33 = ((FP.One / (12 * FP.One)) * base.mass) * ((vector.x * vector.x) + (vector.y * vector.y));
        }

        protected abstract Multishape CreateWorkingClone();
        public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            TSBBox largeBox = TSBBox.LargeBox;
            int num = this.Prepare(ref largeBox);
            box = TSBBox.SmallBox;
            for (int i = 0; i < num; i++)
            {
                this.SetCurrentShape(i);
                base.GetBoundingBox(ref orientation, out largeBox);
                TSBBox.CreateMerged(ref box, ref largeBox, out box);
            }
        }

        public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
        {
        }

        public abstract int Prepare(ref TSBBox box);
        public abstract int Prepare(ref TSVector rayOrigin, ref TSVector rayDelta);
        public Multishape RequestWorkingClone()
        {
            Multishape multishape;
            Debug.Assert(this.workingCloneStack.Count < 10, "Unusual size of the workingCloneStack. Forgot to call ReturnWorkingClone?");
            Debug.Assert(!this.isClone, "Can't clone clones! Something wrong here!");
            Stack<Multishape> workingCloneStack = this.workingCloneStack;
            lock (workingCloneStack)
            {
                if (this.workingCloneStack.Count == 0)
                {
                    multishape = this.CreateWorkingClone();
                    multishape.workingCloneStack = this.workingCloneStack;
                    this.workingCloneStack.Push(multishape);
                }
                multishape = this.workingCloneStack.Pop();
                multishape.isClone = true;
            }
            return multishape;
        }

        public void ReturnWorkingClone()
        {
            Debug.Assert(this.isClone, "Only clones can be returned!");
            Stack<Multishape> workingCloneStack = this.workingCloneStack;
            lock (workingCloneStack)
            {
                this.workingCloneStack.Push(this);
            }
        }

        public abstract void SetCurrentShape(int index);
        public override void UpdateShape()
        {
            Stack<Multishape> workingCloneStack = this.workingCloneStack;
            lock (workingCloneStack)
            {
                this.workingCloneStack.Clear();
            }
            base.UpdateShape();
        }

        public bool IsClone
        {
            get
            {
                return this.isClone;
            }
        }
    }
}

