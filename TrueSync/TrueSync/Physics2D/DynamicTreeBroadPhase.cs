namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class DynamicTreeBroadPhase : IBroadPhase
    {
        internal int[] _moveBuffer;
        internal int _moveCapacity;
        internal int _moveCount;
        internal Pair[] _pairBuffer;
        internal int _pairCapacity;
        internal int _pairCount;
        internal int _proxyCount;
        private Func<int, bool> _queryCallback;
        internal int _queryProxyId;
        internal TrueSync.Physics2D.DynamicTree<FixtureProxy> _tree = new TrueSync.Physics2D.DynamicTree<FixtureProxy>();
        private const int NullProxy = -1;

        public DynamicTreeBroadPhase()
        {
            this._queryCallback = new Func<int, bool>(this.QueryCallback);
            this._proxyCount = 0;
            this._pairCapacity = 0x10;
            this._pairCount = 0;
            this._pairBuffer = new Pair[this._pairCapacity];
            this._moveCapacity = 0x10;
            this._moveCount = 0;
            this._moveBuffer = new int[this._moveCapacity];
        }

        public int AddProxy(ref FixtureProxy proxy)
        {
            int proxyId = this._tree.AddProxy(ref proxy.AABB, proxy);
            this._proxyCount++;
            this.BufferMove(proxyId);
            return proxyId;
        }

        private void BufferMove(int proxyId)
        {
            if (this._moveCount == this._moveCapacity)
            {
                int[] sourceArray = this._moveBuffer;
                this._moveCapacity *= 2;
                this._moveBuffer = new int[this._moveCapacity];
                Array.Copy(sourceArray, this._moveBuffer, this._moveCount);
            }
            this._moveBuffer[this._moveCount] = proxyId;
            this._moveCount++;
        }

        public void GetFatAABB(int proxyId, out AABB aabb)
        {
            this._tree.GetFatAABB(proxyId, out aabb);
        }

        public FixtureProxy GetProxy(int proxyId)
        {
            return this._tree.GetUserData(proxyId);
        }

        public void MoveProxy(int proxyId, ref AABB aabb, TSVector2 displacement)
        {
            if (this._tree.MoveProxy(proxyId, ref aabb, displacement))
            {
                this.BufferMove(proxyId);
            }
        }

        public void Query(Func<int, bool> callback, ref AABB aabb)
        {
            this._tree.Query(callback, ref aabb);
        }

        private bool QueryCallback(int proxyId)
        {
            if (proxyId != this._queryProxyId)
            {
                if (this._pairCount == this._pairCapacity)
                {
                    Pair[] sourceArray = this._pairBuffer;
                    this._pairCapacity *= 2;
                    this._pairBuffer = new Pair[this._pairCapacity];
                    Array.Copy(sourceArray, this._pairBuffer, this._pairCount);
                }
                this._pairBuffer[this._pairCount].ProxyIdA = Math.Min(proxyId, this._queryProxyId);
                this._pairBuffer[this._pairCount].ProxyIdB = Math.Max(proxyId, this._queryProxyId);
                this._pairCount++;
            }
            return true;
        }

        public void RayCast(Func<RayCastInput, int, FP> callback, ref RayCastInput input)
        {
            this._tree.RayCast(callback, ref input);
        }

        public void RemoveProxy(int proxyId)
        {
            this.UnBufferMove(proxyId);
            this._proxyCount--;
            this._tree.RemoveProxy(proxyId);
        }

        public void ShiftOrigin(TSVector2 newOrigin)
        {
            this._tree.ShiftOrigin(newOrigin);
        }

        public bool TestOverlap(int proxyIdA, int proxyIdB)
        {
            AABB aabb;
            AABB aabb2;
            this._tree.GetFatAABB(proxyIdA, out aabb);
            this._tree.GetFatAABB(proxyIdB, out aabb2);
            return AABB.TestOverlap(ref aabb, ref aabb2);
        }

        public void TouchProxy(int proxyId)
        {
            this.BufferMove(proxyId);
        }

        private void UnBufferMove(int proxyId)
        {
            for (int i = 0; i < this._moveCount; i++)
            {
                if (this._moveBuffer[i] == proxyId)
                {
                    this._moveBuffer[i] = -1;
                }
            }
        }

        public void UpdatePairs(BroadphaseDelegate callback)
        {
            this._pairCount = 0;
            for (int i = 0; i < this._moveCount; i++)
            {
                this._queryProxyId = this._moveBuffer[i];
                if (this._queryProxyId != -1)
                {
                    AABB aabb;
                    this._tree.GetFatAABB(this._queryProxyId, out aabb);
                    this._tree.Query(this._queryCallback, ref aabb);
                }
            }
            this._moveCount = 0;
            Array.Sort<Pair>(this._pairBuffer, 0, this._pairCount);
            int index = 0;
            while (index < this._pairCount)
            {
                Pair pair = this._pairBuffer[index];
                FixtureProxy userData = this._tree.GetUserData(pair.ProxyIdA);
                FixtureProxy proxyB = this._tree.GetUserData(pair.ProxyIdB);
                callback(ref userData, ref proxyB);
                index++;
                while (index < this._pairCount)
                {
                    Pair pair2 = this._pairBuffer[index];
                    if ((pair2.ProxyIdA != pair.ProxyIdA) || (pair2.ProxyIdB != pair.ProxyIdB))
                    {
                        break;
                    }
                    index++;
                }
            }
        }

        public int ProxyCount
        {
            get
            {
                return this._proxyCount;
            }
        }

        public int TreeBalance
        {
            get
            {
                return this._tree.MaxBalance;
            }
        }

        public int TreeHeight
        {
            get
            {
                return this._tree.Height;
            }
        }

        public FP TreeQuality
        {
            get
            {
                return this._tree.AreaRatio;
            }
        }
    }
}

