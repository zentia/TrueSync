using System;

namespace TrueSync.Physics2D
{
	public class DynamicTreeBroadPhase : IBroadPhase
	{
		private const int NullProxy = -1;

		internal int[] _moveBuffer;

		internal int _moveCapacity;

		internal int _moveCount;

		internal Pair[] _pairBuffer;

		internal int _pairCapacity;

		internal int _pairCount;

		internal int _proxyCount;

		private Func<int, bool> _queryCallback;

		internal int _queryProxyId;

		internal DynamicTree<FixtureProxy> _tree = new DynamicTree<FixtureProxy>();

		public int ProxyCount
		{
			get
			{
				return this._proxyCount;
			}
		}

		public FP TreeQuality
		{
			get
			{
				return this._tree.AreaRatio;
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

		public DynamicTreeBroadPhase()
		{
			this._queryCallback = new Func<int, bool>(this.QueryCallback);
			this._proxyCount = 0;
			this._pairCapacity = 16;
			this._pairCount = 0;
			this._pairBuffer = new Pair[this._pairCapacity];
			this._moveCapacity = 16;
			this._moveCount = 0;
			this._moveBuffer = new int[this._moveCapacity];
		}

		public int AddProxy(ref FixtureProxy proxy)
		{
			int num = this._tree.AddProxy(ref proxy.AABB, proxy);
			this._proxyCount++;
			this.BufferMove(num);
			return num;
		}

		public void RemoveProxy(int proxyId)
		{
			this.UnBufferMove(proxyId);
			this._proxyCount--;
			this._tree.RemoveProxy(proxyId);
		}

		public void MoveProxy(int proxyId, ref AABB aabb, TSVector2 displacement)
		{
			bool flag = this._tree.MoveProxy(proxyId, ref aabb, displacement);
			bool flag2 = flag;
			if (flag2)
			{
				this.BufferMove(proxyId);
			}
		}

		public void TouchProxy(int proxyId)
		{
			this.BufferMove(proxyId);
		}

		private void BufferMove(int proxyId)
		{
			bool flag = this._moveCount == this._moveCapacity;
			if (flag)
			{
				int[] moveBuffer = this._moveBuffer;
				this._moveCapacity *= 2;
				this._moveBuffer = new int[this._moveCapacity];
				Array.Copy(moveBuffer, this._moveBuffer, this._moveCount);
			}
			this._moveBuffer[this._moveCount] = proxyId;
			this._moveCount++;
		}

		private void UnBufferMove(int proxyId)
		{
			for (int i = 0; i < this._moveCount; i++)
			{
				bool flag = this._moveBuffer[i] == proxyId;
				if (flag)
				{
					this._moveBuffer[i] = -1;
				}
			}
		}

		private bool QueryCallback(int proxyId)
		{
			bool flag = proxyId == this._queryProxyId;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = this._pairCount == this._pairCapacity;
				if (flag2)
				{
					Pair[] pairBuffer = this._pairBuffer;
					this._pairCapacity *= 2;
					this._pairBuffer = new Pair[this._pairCapacity];
					Array.Copy(pairBuffer, this._pairBuffer, this._pairCount);
				}
				this._pairBuffer[this._pairCount].ProxyIdA = Math.Min(proxyId, this._queryProxyId);
				this._pairBuffer[this._pairCount].ProxyIdB = Math.Max(proxyId, this._queryProxyId);
				this._pairCount++;
				result = true;
			}
			return result;
		}

		public void GetFatAABB(int proxyId, out AABB aabb)
		{
			this._tree.GetFatAABB(proxyId, out aabb);
		}

		public FixtureProxy GetProxy(int proxyId)
		{
			return this._tree.GetUserData(proxyId);
		}

		public bool TestOverlap(int proxyIdA, int proxyIdB)
		{
			AABB aABB;
			this._tree.GetFatAABB(proxyIdA, out aABB);
			AABB aABB2;
			this._tree.GetFatAABB(proxyIdB, out aABB2);
			return AABB.TestOverlap(ref aABB, ref aABB2);
		}

		public void UpdatePairs(BroadphaseDelegate callback)
		{
			this._pairCount = 0;
			for (int i = 0; i < this._moveCount; i++)
			{
				this._queryProxyId = this._moveBuffer[i];
				bool flag = this._queryProxyId == -1;
				if (!flag)
				{
					AABB aABB;
					this._tree.GetFatAABB(this._queryProxyId, out aABB);
					this._tree.Query(this._queryCallback, ref aABB);
				}
			}
			this._moveCount = 0;
			Array.Sort<Pair>(this._pairBuffer, 0, this._pairCount);
			int j = 0;
			while (j < this._pairCount)
			{
				Pair pair = this._pairBuffer[j];
				FixtureProxy userData = this._tree.GetUserData(pair.ProxyIdA);
				FixtureProxy userData2 = this._tree.GetUserData(pair.ProxyIdB);
				callback(ref userData, ref userData2);
				for (j++; j < this._pairCount; j++)
				{
					Pair pair2 = this._pairBuffer[j];
					bool flag2 = pair2.ProxyIdA != pair.ProxyIdA || pair2.ProxyIdB != pair.ProxyIdB;
					if (flag2)
					{
						break;
					}
				}
			}
		}

		public void Query(Func<int, bool> callback, ref AABB aabb)
		{
			this._tree.Query(callback, ref aabb);
		}

		public void RayCast(Func<RayCastInput, int, FP> callback, ref RayCastInput input)
		{
			this._tree.RayCast(callback, ref input);
		}

		public void ShiftOrigin(TSVector2 newOrigin)
		{
			this._tree.ShiftOrigin(newOrigin);
		}
	}
}
