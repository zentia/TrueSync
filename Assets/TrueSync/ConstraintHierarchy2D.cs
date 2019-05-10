using System;
using TrueSync.Physics2D;

namespace TrueSync
{
	public class ConstraintHierarchy2D : IBodyConstraint
	{
		private Body parent;

		private Body child;

		private TSVector2 childOffset;

		public ConstraintHierarchy2D(Body parent, Body child, TSVector2 childOffset)
		{
			this.parent = parent;
			this.child = child;
			this.childOffset = childOffset;
		}

		public void PostStep()
		{
			TSVector2 tSVector = this.childOffset + this.parent.Position;
			this.child.SetTransformIgnoreContacts(ref tSVector, this.child.Rotation);
		}
	}
}
