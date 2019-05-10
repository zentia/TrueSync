using System;

namespace TrueSync
{
	public class CollisionIsland : ResourcePoolItem
	{
		internal IslandManager islandManager;

		internal HashList<RigidBody> bodies = new HashList<RigidBody>();

		internal HashList<Arbiter> arbiter = new HashList<Arbiter>();

		internal HashList<Constraint> constraints = new HashList<Constraint>();

		private ReadOnlyHashset<RigidBody> readOnlyBodies;

		private ReadOnlyHashset<Arbiter> readOnlyArbiter;

		private ReadOnlyHashset<Constraint> readOnlyConstraints;

		public ReadOnlyHashset<RigidBody> Bodies
		{
			get
			{
				return this.readOnlyBodies;
			}
		}

		public ReadOnlyHashset<Arbiter> Arbiter
		{
			get
			{
				return this.readOnlyArbiter;
			}
		}

		public ReadOnlyHashset<Constraint> Constraints
		{
			get
			{
				return this.readOnlyConstraints;
			}
		}

		public CollisionIsland()
		{
			this.readOnlyBodies = new ReadOnlyHashset<RigidBody>(this.bodies);
			this.readOnlyArbiter = new ReadOnlyHashset<Arbiter>(this.arbiter);
			this.readOnlyConstraints = new ReadOnlyHashset<Constraint>(this.constraints);
		}

		public void CleanUp()
		{
			this.bodies.Clear();
			this.arbiter.Clear();
			this.constraints.Clear();
		}

		public bool IsActive()
		{
			return true;
		}

		public void SetStatus(bool active)
		{
			foreach (RigidBody current in this.bodies)
			{
				current.IsActive = active;
				bool flag = active && !current.IsActive;
				if (flag)
				{
					current.inactiveTime = FP.Zero;
				}
			}
		}

		internal void ClearLists()
		{
			this.arbiter.Clear();
			this.bodies.Clear();
			this.constraints.Clear();
		}
	}
}
