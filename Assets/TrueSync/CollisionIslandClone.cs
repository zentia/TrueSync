using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class CollisionIslandClone
	{
		public List<RigidBody> bodies = new List<RigidBody>();

		public List<ArbiterClone> arbiters = new List<ArbiterClone>();

		public List<Constraint> constraints = new List<Constraint>();

		private int index;

		private int length;

		public void Reset()
		{
			this.index = 0;
			this.length = this.arbiters.Count;
			while (this.index < this.length)
			{
				ArbiterClone arbiterClone = this.arbiters[this.index];
				arbiterClone.Reset();
				WorldClone.poolArbiterClone.GiveBack(arbiterClone);
				this.index++;
			}
		}

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
				ArbiterClone @new = WorldClone.poolArbiterClone.GetNew();
				@new.Clone(ci.arbiter[this.index]);
				this.arbiters.Add(@new);
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

		public void Restore(CollisionIsland ci, World world)
		{
			ci.ClearLists();
			ci.islandManager = world.islands;
			this.index = 0;
			this.length = this.bodies.Count;
			while (this.index < this.length)
			{
				RigidBody rigidBody = this.bodies[this.index];
				rigidBody.island = ci;
				ci.bodies.Add(rigidBody);
				this.index++;
			}
			this.index = 0;
			this.length = this.arbiters.Count;
			while (this.index < this.length)
			{
				ArbiterClone arbiterClone = this.arbiters[this.index];
				Arbiter item = null;
				world.ArbiterMap.LookUpArbiter(arbiterClone.body1, arbiterClone.body2, out item);
				ci.arbiter.Add(item);
				this.index++;
			}
			this.index = 0;
			this.length = this.constraints.Count;
			while (this.index < this.length)
			{
				Constraint item2 = this.constraints[this.index];
				ci.constraints.Add(item2);
				this.index++;
			}
		}
	}
}
