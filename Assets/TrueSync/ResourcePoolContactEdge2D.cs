using System;
using TrueSync.Physics2D;

namespace TrueSync
{
	public class ResourcePoolContactEdge2D : ResourcePool<ContactEdge>
	{
		protected override ContactEdge NewInstance()
		{
			return new ContactEdge();
		}
	}
}
