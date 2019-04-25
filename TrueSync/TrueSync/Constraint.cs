namespace TrueSync
{
    using System;

    public abstract class Constraint : IConstraint, IDebugDrawable, IComparable
    {
        internal RigidBody body1;
        internal RigidBody body2;
        private int instance;
        internal static int instanceCount = 0;

        public Constraint(RigidBody body1, RigidBody body2)
        {
            this.body1 = body1;
            this.body2 = body2;
            instanceCount++;
            this.instance = instanceCount;
            if (body1 > null)
            {
                body1.Update();
            }
            if (body2 > null)
            {
                body2.Update();
            }
        }

        public int CompareTo(object otherObj)
        {
            Constraint constraint = (Constraint) otherObj;
            if (constraint.instance < this.instance)
            {
                return -1;
            }
            if (constraint.instance > this.instance)
            {
                return 1;
            }
            return 0;
        }

        public virtual void DebugDraw(IDebugDrawer drawer)
        {
        }

        public virtual void Iterate()
        {
        }

        public virtual void PostStep()
        {
        }

        public virtual void PrepareForIteration(FP timestep)
        {
        }

        public RigidBody Body1
        {
            get
            {
                return this.body1;
            }
        }

        public RigidBody Body2
        {
            get
            {
                return this.body2;
            }
        }
    }
}

