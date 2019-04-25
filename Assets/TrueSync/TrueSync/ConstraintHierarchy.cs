namespace TrueSync
{
    using System;

    public class ConstraintHierarchy : Constraint
    {
        private RigidBody child;
        private TSVector childOffset;
        private RigidBody parent;

        public ConstraintHierarchy(IBody parent, IBody child, TSVector childOffset) : base((RigidBody) parent, (RigidBody) child)
        {
            this.parent = (RigidBody) parent;
            this.child = (RigidBody) child;
            this.childOffset = childOffset;
        }

        public override void PostStep()
        {
            this.child.Position = this.childOffset + this.parent.Position;
        }
    }
}

