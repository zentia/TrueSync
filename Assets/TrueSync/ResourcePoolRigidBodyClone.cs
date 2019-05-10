using System;

namespace TrueSync
{
	public class ResourcePoolRigidBodyClone : ResourcePool<RigidBodyClone>
	{
		protected override RigidBodyClone NewInstance()
		{
			return new RigidBodyClone();
		}
	}
}
