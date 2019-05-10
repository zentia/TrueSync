using System;

namespace TrueSync
{
	public interface IConstraint
	{
		RigidBody Body1
		{
			get;
		}

		RigidBody Body2
		{
			get;
		}

		void PrepareForIteration(FP timestep);

		void Iterate();
	}
}
