using System;

namespace TrueSync.Physics2D
{
	public struct PhysicsLogicFilter
	{
		public PhysicsLogicType ControllerIgnores;

		public void IgnorePhysicsLogic(PhysicsLogicType type)
		{
			this.ControllerIgnores |= type;
		}

		public void RestorePhysicsLogic(PhysicsLogicType type)
		{
			this.ControllerIgnores &= ~type;
		}

		public bool IsPhysicsLogicIgnored(PhysicsLogicType type)
		{
			return (this.ControllerIgnores & type) == type;
		}
	}
}
