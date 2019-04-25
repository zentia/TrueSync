namespace TrueSync
{
    using System;
    using TrueSync.Physics2D;

    internal class DynamicTreeBroadPhaseClone2D
    {
        public int[] _moveBuffer;
        public int _moveCapacity;
        public int _moveCount;
        public Pair[] _pairBuffer;
        public int _pairCapacity;
        public int _pairCount;
        public int _proxyCount;
        public int _queryProxyId;
        public DynamicTreeClone2D dynamicTreeClone = new DynamicTreeClone2D();

        public void Clone(DynamicTreeBroadPhase dynamicTreeBroadPhase)
        {
            this._moveBuffer = dynamicTreeBroadPhase._moveBuffer;
            this._moveCapacity = dynamicTreeBroadPhase._moveCapacity;
            this._moveCount = dynamicTreeBroadPhase._moveCount;
            this._pairBuffer = new Pair[dynamicTreeBroadPhase._pairBuffer.Length];
            Array.Copy(dynamicTreeBroadPhase._pairBuffer, this._pairBuffer, this._pairBuffer.Length);
            this._pairCapacity = dynamicTreeBroadPhase._pairCapacity;
            this._pairCount = dynamicTreeBroadPhase._pairCount;
            this._proxyCount = dynamicTreeBroadPhase._proxyCount;
            this._queryProxyId = dynamicTreeBroadPhase._queryProxyId;
            this.dynamicTreeClone.Clone(dynamicTreeBroadPhase._tree);
        }

        public void Restore(DynamicTreeBroadPhase dynamicTreeBroadPhase)
        {
            dynamicTreeBroadPhase._moveBuffer = this._moveBuffer;
            dynamicTreeBroadPhase._moveCapacity = this._moveCapacity;
            dynamicTreeBroadPhase._moveCount = this._moveCount;
            dynamicTreeBroadPhase._pairBuffer = new Pair[this._pairBuffer.Length];
            Array.Copy(this._pairBuffer, dynamicTreeBroadPhase._pairBuffer, dynamicTreeBroadPhase._pairBuffer.Length);
            dynamicTreeBroadPhase._pairCapacity = this._pairCapacity;
            dynamicTreeBroadPhase._pairCount = this._pairCount;
            dynamicTreeBroadPhase._proxyCount = this._proxyCount;
            dynamicTreeBroadPhase._queryProxyId = this._queryProxyId;
            this.dynamicTreeClone.Restore(dynamicTreeBroadPhase._tree);
        }
    }
}

