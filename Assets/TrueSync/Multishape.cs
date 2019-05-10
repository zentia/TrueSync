using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync
{
	public abstract class Multishape : Shape
	{
		internal bool isClone = false;

		private Stack<Multishape> workingCloneStack = new Stack<Multishape>();

		public bool IsClone
		{
			get
			{
				return this.isClone;
			}
		}

		public abstract void SetCurrentShape(int index);

		public abstract int Prepare(ref TSBBox box);

		public abstract int Prepare(ref TSVector rayOrigin, ref TSVector rayDelta);

		protected abstract Multishape CreateWorkingClone();

		public Multishape RequestWorkingClone()
		{
			Debug.Assert(this.workingCloneStack.Count < 10, "Unusual size of the workingCloneStack. Forgot to call ReturnWorkingClone?");
			Debug.Assert(!this.isClone, "Can't clone clones! Something wrong here!");
			Stack<Multishape> obj = this.workingCloneStack;
			Multishape multishape;
			lock (obj)
			{
				bool flag = this.workingCloneStack.Count == 0;
				if (flag)
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

		public override void UpdateShape()
		{
			Stack<Multishape> obj = this.workingCloneStack;
			lock (obj)
			{
				this.workingCloneStack.Clear();
			}
			base.UpdateShape();
		}

		public void ReturnWorkingClone()
		{
			Debug.Assert(this.isClone, "Only clones can be returned!");
			Stack<Multishape> obj = this.workingCloneStack;
			lock (obj)
			{
				this.workingCloneStack.Push(this);
			}
		}

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

		public override void CalculateMassInertia()
		{
			this.geomCen = TSVector.zero;
			this.inertia = TSMatrix.Identity;
			TSVector tSVector;
			TSVector.Subtract(ref this.boundingBox.max, ref this.boundingBox.min, out tSVector);
			this.mass = tSVector.x * tSVector.y * tSVector.z;
			this.inertia.M11 = FP.One / (12 * FP.One) * this.mass * (tSVector.y * tSVector.y + tSVector.z * tSVector.z);
			this.inertia.M22 = FP.One / (12 * FP.One) * this.mass * (tSVector.x * tSVector.x + tSVector.z * tSVector.z);
			this.inertia.M33 = FP.One / (12 * FP.One) * this.mass * (tSVector.x * tSVector.x + tSVector.y * tSVector.y);
		}
	}
}
