namespace TrueSync
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class ArbiterMap : IEnumerable
    {
        private Dictionary<ArbiterKey, Arbiter> dictionaryKeys = new Dictionary<ArbiterKey, Arbiter>(new ArbiterKeyComparer());
        private HashList<ArbiterKey> keysSortedList = new HashList<ArbiterKey>();
        public ArbiterKey lookUpKey = new ArbiterKey(null, null);

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

        public bool ContainsArbiter(RigidBody body1, RigidBody body2)
        {
            this.lookUpKey.SetBodies(body1, body2);
            return this.dictionaryKeys.ContainsKey(this.lookUpKey);
        }

        public IEnumerator GetEnumerator()
        {
            return this.dictionaryKeys.Values.GetEnumerator();
        }

        public bool LookUpArbiter(RigidBody body1, RigidBody body2, out Arbiter arbiter)
        {
            this.lookUpKey.SetBodies(body1, body2);
            if (!this.dictionaryKeys.ContainsKey(this.lookUpKey))
            {
                arbiter = null;
                return false;
            }
            arbiter = this.dictionaryKeys[this.lookUpKey];
            return true;
        }

        internal void Remove(Arbiter arbiter)
        {
            this.lookUpKey.SetBodies(arbiter.body1, arbiter.body2);
            this.keysSortedList.Remove(this.lookUpKey);
            this.dictionaryKeys.Remove(this.lookUpKey);
        }

        public Dictionary<ArbiterKey, Arbiter>.ValueCollection Arbiters
        {
            get
            {
                return this.dictionaryKeys.Values;
            }
        }
    }
}

