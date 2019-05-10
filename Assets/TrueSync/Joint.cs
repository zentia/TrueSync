using System;

namespace TrueSync
{
	public abstract class Joint
	{
		public World World
		{
			get;
			private set;
		}

		public Joint(World world)
		{
			this.World = world;
		}

		public abstract void Activate();

		public abstract void Deactivate();
	}
}
