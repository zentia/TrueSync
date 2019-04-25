namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;

        public SerializableDictionary()
        {
            keys = new List<TKey>();
            values = new List<TValue>();
        }

        public void OnAfterDeserialize()
        {
            base.Clear();
            if (this.keys.Count != this.values.Count)
            {
                throw new Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", new object[0]));
            }
            for (int i = 0; i < this.keys.Count; i++)
            {
                base.Add(this.keys[i], this.values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            this.keys.Clear();
            this.values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                this.keys.Add(pair.Key);
                this.values.Add(pair.Value);
            }
        }
    }
}