namespace TrueSync
{
    using System;
    using TrueSync.Physics2D;

    public class ConstraintHierarchy2D : IBodyConstraint
    {
        private Body child;
        private TSVector2 childOffset;
        private Body parent;

        public ConstraintHierarchy2D(Body parent, Body child, TSVector2 childOffset)
        {
            this.parent = parent;
            this.child = child;
            this.childOffset = childOffset;
        }

        public void PostStep()
        {
            TSVector2 position = this.childOffset + this.parent.Position;
            this.child.SetTransformIgnoreContacts(ref position, this.child.Rotation);
        }
    }
}

