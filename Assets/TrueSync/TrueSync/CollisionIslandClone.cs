namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class CollisionIslandClone
    {
        public List<ArbiterClone> arbiters = new List<ArbiterClone>();
        public List<RigidBody> bodies = new List<RigidBody>();
        public List<Constraint> constraints = new List<Constraint>();
        private int index;
        private int length;

        public void Clone(CollisionIsland ci)
        {
            this.bodies.Clear();
            this.index = 0;
            this.length = ci.bodies.Count;
            while (this.index < this.length)
            {
                this.bodies.Add(ci.bodies[this.index]);
                this.index++;
            }
            this.arbiters.Clear();
            this.index = 0;
            this.length = ci.arbiter.Count;
            while (this.index < this.length)
            {
                ArbiterClone item = WorldClone.poolArbiterClone.GetNew();
                item.Clone(ci.arbiter[this.index]);
                this.arbiters.Add(item);
                this.index++;
            }
            this.constraints.Clear();
            this.index = 0;
            this.length = ci.constraints.Count;
            while (this.index < this.length)
            {
                this.constraints.Add(ci.constraints[this.index]);
                this.index++;
            }
        }

        public void Reset()
        {
            this.index = 0;
            this.length = this.arbiters.Count;
            while (this.index < this.length)
            {
                ArbiterClone clone = this.arbiters[this.index];
                clone.Reset();
                WorldClone.poolArbiterClone.GiveBack(clone);
                this.index++;
            }
        }

        public void Restore(CollisionIsland ci, World world)
        {
            ci.ClearLists();
            ci.islandManager = world.islands;
            this.index = 0;
            this.length = this.bodies.Count;
            while (this.index < this.length)
            {
                RigidBody item = this.bodies[this.index];
                item.island = ci;
                ci.bodies.Add(item);
                this.index++;
            }
            this.index = 0;
            this.length = this.arbiters.Count;
            while (this.index < this.length)
            {
                ArbiterClone clone = this.arbiters[this.index];
                Arbiter arbiter = null;
                world.ArbiterMap.LookUpArbiter(clone.body1, clone.body2, out arbiter);
                ci.arbiter.Add(arbiter);
                this.index++;
            }
            this.index = 0;
            this.length = this.constraints.Count;
            while (this.index < this.length)
            {
                Constraint constraint = this.constraints[this.index];
                ci.constraints.Add(constraint);
                this.index++;
            }
        }
    }
}

