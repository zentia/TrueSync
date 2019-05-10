using System;
using System.Collections;
using System.Collections.Generic;

namespace TrueSync
{
	public class ArbiterMap : IEnumerable
	{
		private Dictionary<ArbiterKey, Arbiter> dictionaryKeys = new Dictionary<ArbiterKey, Arbiter>(new ArbiterKeyComparer());

		private HashList<ArbiterKey> keysSortedList = new HashList<ArbiterKey>();

		public ArbiterKey lookUpKey;

		public Dictionary<ArbiterKey, Arbiter>.ValueCollection Arbiters
		{
			get
			{
				return this.dictionaryKeys.Values;
			}
		}

		public ArbiterMap()
		{
			this.lookUpKey = new ArbiterKey(null, null);
		}

		public bool LookUpArbiter(RigidBody body1, RigidBody body2, out Arbiter arbiter)
		{
			this.lookUpKey.SetBodies(body1, body2);
			bool flag = !this.dictionaryKeys.ContainsKey(this.lookUpKey);
			bool result;
			if (flag)
			{
				arbiter = null;
				result = false;
			}
			else
			{
				arbiter = this.dictionaryKeys[this.lookUpKey];
				result = true;
			}
			return result;
		}

		internal void Add(ArbiterKey key, Arbiter arbiter)
		{
			this.keysSortedList.Add(key);
			this.dictionaryKeys.Add(key, arbiter);
		}

		internal void Clear()
		{
			this.keysSortedList.Clear();
			this.dictionaryKeys.Clear();
		}

		internal void Remove(Arbiter arbiter)
		{
			this.lookUpKey.SetBodies(arbiter.body1, arbiter.body2);
			this.keysSortedList.Remove(this.lookUpKey);
			this.dictionaryKeys.Remove(this.lookUpKey);
		}

		public bool ContainsArbiter(RigidBody body1, RigidBody body2)
		{
			this.lookUpKey.SetBodies(body1, body2);
			return this.dictionaryKeys.ContainsKey(this.lookUpKey);
		}

		public IEnumerator GetEnumerator()
		{
			return this.dictionaryKeys.Values.GetEnumerator();
		}
	}
}
