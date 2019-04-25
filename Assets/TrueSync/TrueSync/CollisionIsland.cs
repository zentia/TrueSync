namespace TrueSync
{
    using System;

    public class CollisionIsland : ResourcePoolItem
    {
        internal HashList<TrueSync.Arbiter> arbiter = new HashList<TrueSync.Arbiter>();
        internal HashList<RigidBody> bodies = new HashList<RigidBody>();
        internal HashList<Constraint> constraints = new HashList<Constraint>();
        internal IslandManager islandManager;
        private ReadOnlyHashset<TrueSync.Arbiter> readOnlyArbiter;
        private ReadOnlyHashset<RigidBody> readOnlyBodies;
        private ReadOnlyHashset<Constraint> readOnlyConstraints;

        public CollisionIsland()
        {
            this.readOnlyBodies = new ReadOnlyHashset<RigidBody>(this.bodies);
            this.readOnlyArbiter = new ReadOnlyHashset<TrueSync.Arbiter>(this.arbiter);
            this.readOnlyConstraints = new ReadOnlyHashset<Constraint>(this.constraints);
        }

        public void CleanUp()
        {
            this.bodies.Clear();
            this.arbiter.Clear();
            this.constraints.Clear();
        }

        internal void ClearLists()
        {
            this.arbiter.Clear();
            this.bodies.Clear();
            this.constraints.Clear();
        }

        public bool IsActive()
        {
            return true;
        }

        public void SetStatus(bool active)
        {
            foreach (RigidBody body in this.bodies)
            {
                body.IsActive = active;
                if (active && !body.IsActive)
                {
                    body.inactiveTime = FP.Zero;
                }
            }
        }

        public ReadOnlyHashset<TrueSync.Arbiter> Arbiter
        {
            get
            {
                return this.readOnlyArbiter;
            }
        }

        public ReadOnlyHashset<RigidBody> Bodies
        {
            get
            {
                return this.readOnlyBodies;
            }
        }

        public ReadOnlyHashset<Constraint> Constraints
        {
            get
            {
                return this.readOnlyConstraints;
            }
        }
    }
}

