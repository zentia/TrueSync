using System;

namespace TrueSync.Physics2D
{
	public abstract class PhysicsLogic : FilterData
	{
		private PhysicsLogicType _type;

		public World World;

		public override bool IsActiveOn(Body body)
		{
			bool flag = body.PhysicsLogicFilter.IsPhysicsLogicIgnored(this._type);
			return !flag && base.IsActiveOn(body);
		}

		public PhysicsLogic(World world, PhysicsLogicType type)
		{
			this._type = type;
			this.World = world;
		}
	}
}
