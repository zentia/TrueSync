using System;

namespace TrueSync
{
	public class ConstraintHierarchy : Constraint
	{
		private RigidBody parent;

		private RigidBody child;

		private TSVector childOffset;

		public ConstraintHierarchy(IBody parent, IBody child, TSVector childOffset) : base((RigidBody)parent, (RigidBody)child)
		{
			this.parent = (RigidBody)parent;
			this.child = (RigidBody)child;
			this.childOffset = childOffset;
		}

		public override void PostStep()
		{
			this.child.Position = this.childOffset + this.parent.Position;
		}
	}
}
