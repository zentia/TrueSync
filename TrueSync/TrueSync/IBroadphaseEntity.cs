namespace TrueSync
{
    using System;

    public interface IBroadphaseEntity
    {
        TSBBox BoundingBox { get; }

        bool IsStaticNonKinematic { get; }

        bool IsStaticOrInactive { get; }
    }
}

