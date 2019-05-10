using System;

namespace TrueSync.Physics2D
{
	[Flags]
	public enum ControllerType
	{
		GravityController = 1,
		VelocityLimitController = 2,
		AbstractForceController = 4,
		BuoyancyController = 8
	}
}
