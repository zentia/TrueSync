namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    public interface IBroadPhase
    {
        int AddProxy(ref FixtureProxy proxy);
        void GetFatAABB(int proxyId, out AABB aabb);
        FixtureProxy GetProxy(int proxyId);
        void MoveProxy(int proxyId, ref AABB aabb, TSVector2 displacement);
        void Query(Func<int, bool> callback, ref AABB aabb);
        void RayCast(Func<RayCastInput, int, FP> callback, ref RayCastInput input);
        void RemoveProxy(int proxyId);
        void ShiftOrigin(TSVector2 newOrigin);
        bool TestOverlap(int proxyIdA, int proxyIdB);
        void TouchProxy(int proxyId);
        void UpdatePairs(BroadphaseDelegate callback);

        int ProxyCount { get; }
    }
}

