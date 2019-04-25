namespace TrueSync
{
    using System;
    using UnityEngine;

    [Serializable]
    public class InputData
    {
        [SerializeField]
        internal SerializableDictionaryByteByte byteTable;
        [SerializeField]
        internal SerializableDictionaryByteFP fpTable;
        [SerializeField]
        internal SerializableDictionaryByteInt intTable;
        public byte ownerID;
        [SerializeField]
        internal SerializableDictionaryByteString stringTable;
        [SerializeField]
        internal SerializableDictionaryByteTSVector tsVectorTable;

        public InputData(byte ownerID)
        {
            this.ownerID = ownerID;
            stringTable = new SerializableDictionaryByteString();
            byteTable = new SerializableDictionaryByteByte();
            intTable = new SerializableDictionaryByteInt();
            fpTable = new SerializableDictionaryByteFP();
            tsVectorTable = new SerializableDictionaryByteTSVector();
        }

        internal void AddByte(byte key, byte value)
        {
            byteTable[key] = value;
        }

        internal void AddFP(byte key, FP value)
        {
            this.fpTable[key] = value;
        }

        internal void AddInt(byte key, int value)
        {
            this.intTable[key] = value;
        }

        internal void AddString(byte key, string value)
        {
            this.stringTable[key] = value;
        }

        internal void AddTSVector(byte key, TSVector value)
        {
            this.tsVectorTable[key] = value;
        }

        public byte GetByte(byte key)
        {
            if (!this.byteTable.ContainsKey(key))
            {
                return 0;
            }
            return this.byteTable[key];
        }

        public FP GetFP(byte key)
        {
            if (!this.fpTable.ContainsKey(key))
            {
                return 0;
            }
            return this.fpTable[key];
        }

        public int GetInt(byte key)
        {
            if (!this.intTable.ContainsKey(key))
            {
                return 0;
            }
            return this.intTable[key];
        }

        public string GetString(byte key)
        {
            if (!this.stringTable.ContainsKey(key))
            {
                return "";
            }
            return this.stringTable[key];
        }

        public TSVector GetTSVector(byte key)
        {
            if (!this.tsVectorTable.ContainsKey(key))
            {
                return TSVector.zero;
            }
            return this.tsVectorTable[key];
        }

        public bool HasByte(byte key)
        {
            return this.byteTable.ContainsKey(key);
        }

        public bool HasFP(byte key)
        {
            return this.fpTable.ContainsKey(key);
        }

        public bool HasInt(byte key)
        {
            return this.intTable.ContainsKey(key);
        }

        public bool HasString(byte key)
        {
            return this.stringTable.ContainsKey(key);
        }

        public bool HasTSVector(byte key)
        {
            return this.tsVectorTable.ContainsKey(key);
        }

        internal bool IsEmpty()
        {
            return (this.Count == 0);
        }

        public int Count
        {
            get
            {
                return ((((this.stringTable.Count + this.byteTable.Count) + this.intTable.Count) + this.fpTable.Count) + this.tsVectorTable.Count);
            }
        }
    }
}

