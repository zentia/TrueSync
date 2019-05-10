using System;

namespace TrueSync
{
	public interface IBroadphaseEntity
	{
		TSBBox BoundingBox
		{
			get;
		}

		bool IsStaticOrInactive
		{
			get;
		}

		bool IsStaticNonKinematic
		{
			get;
		}
	}
}
