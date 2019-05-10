using System;

namespace TrueSync
{
	public class ResourcePoolArbiterClone : ResourcePool<ArbiterClone>
	{
		protected override ArbiterClone NewInstance()
		{
			return new ArbiterClone();
		}
	}
}
