using System;
using UnityEngine;

namespace TrueSync
{
	[Serializable]
	public class InputData
	{
		public byte ownerID;

		[SerializeField]
		internal SerializableDictionaryByteString stringTable;

		[SerializeField]
		internal SerializableDictionaryByteByte byteTable;

		[SerializeField]
		internal SerializableDictionaryByteInt intTable;

		[SerializeField]
		internal SerializableDictionaryByteFP fpTable;

		[SerializeField]
		internal SerializableDictionaryByteTSVector tsVectorTable;

		public int Count
		{
			get
			{
				return this.stringTable.Count + this.byteTable.Count + this.intTable.Count + this.fpTable.Count + this.tsVectorTable.Count;
			}
		}

		public InputData(byte ownerID)
		{
			this.ownerID = ownerID;
			this.stringTable = new SerializableDictionaryByteString();
			this.byteTable = new SerializableDictionaryByteByte();
			this.intTable = new SerializableDictionaryByteInt();
			this.fpTable = new SerializableDictionaryByteFP();
			this.tsVectorTable = new SerializableDictionaryByteTSVector();
		}

		internal bool IsEmpty()
		{
			return this.Count == 0;
		}

		internal void AddString(byte key, string value)
		{
			this.stringTable[key] = value;
		}

		internal void AddByte(byte key, byte value)
		{
			this.byteTable[key] = value;
		}

		internal void AddInt(byte key, int value)
		{
			this.intTable[key] = value;
		}

		internal void AddFP(byte key, FP value)
		{
			this.fpTable[key] = value;
		}

		internal void AddTSVector(byte key, TSVector value)
		{
			this.tsVectorTable[key] = value;
		}

		public string GetString(byte key)
		{
			bool flag = !this.stringTable.ContainsKey(key);
			string result;
			if (flag)
			{
				result = "";
			}
			else
			{
				result = this.stringTable[key];
			}
			return result;
		}

		public byte GetByte(byte key)
		{
			bool flag = !this.byteTable.ContainsKey(key);
			byte result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				result = this.byteTable[key];
			}
			return result;
		}

		public int GetInt(byte key)
		{
			bool flag = !this.intTable.ContainsKey(key);
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				result = this.intTable[key];
			}
			return result;
		}

		public FP GetFP(byte key)
		{
			bool flag = !this.fpTable.ContainsKey(key);
			FP result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				result = this.fpTable[key];
			}
			return result;
		}

		public TSVector GetTSVector(byte key)
		{
			bool flag = !this.tsVectorTable.ContainsKey(key);
			TSVector result;
			if (flag)
			{
				result = TSVector.zero;
			}
			else
			{
				result = this.tsVectorTable[key];
			}
			return result;
		}

		public bool HasString(byte key)
		{
			return this.stringTable.ContainsKey(key);
		}

		public bool HasByte(byte key)
		{
			return this.byteTable.ContainsKey(key);
		}

		public bool HasInt(byte key)
		{
			return this.intTable.ContainsKey(key);
		}

		public bool HasFP(byte key)
		{
			return this.fpTable.ContainsKey(key);
		}

		public bool HasTSVector(byte key)
		{
			return this.tsVectorTable.ContainsKey(key);
		}
	}
}
