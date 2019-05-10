using System;

namespace TrueSync
{
	public class ResourcePoolSweetPointClone : ResourcePool<SweetPointClone>
	{
		protected override SweetPointClone NewInstance()
		{
			return new SweetPointClone();
		}
	}
}
