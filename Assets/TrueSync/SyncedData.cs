using System;
using System.Collections.Generic;
using System.Text;

namespace TrueSync
{
	[Serializable]
	public class SyncedData
	{
		private enum Types : byte
		{
			Byte,
			String,
			Integer,
			FP,
			TSVector
		}

		public InputData inputData;

		public int tick;

		[NonSerialized]
		public bool fake;

		[NonSerialized]
		public bool dirty;

		[NonSerialized]
		public bool dropPlayer;

		[NonSerialized]
		public byte dropFromPlayerId;

		public SyncedData(byte ownerID, int tick)
		{
			this.inputData = new InputData(ownerID);
			this.tick = tick;
			this.fake = false;
			this.dirty = false;
		}

		public byte[] Encoded()
		{
			List<byte> list = new List<byte>();
			list.AddRange(this.GetEncodedHeader());
			list.AddRange(this.GetEncodedActions());
			return list.ToArray();
		}

		public List<byte> GetEncodedHeader()
		{
			List<byte> list = new List<byte>();
			byte[] bytes = BitConverter.GetBytes(this.tick);
			list.AddRange(bytes);
			list.Add(this.inputData.ownerID);
			list.Add(this.dropFromPlayerId);
			list.Add(this.dropPlayer ? 1 : 0);
			return list;
		}

		public List<byte> GetEncodedActions()
		{
			List<byte> list = new List<byte>();
			byte item = (byte)this.inputData.Count;
			list.Add(item);
			foreach (KeyValuePair<byte, byte> current in this.inputData.byteTable)
			{
				list.Add(current.Key);
				list.Add(0);
				list.Add(current.Value);
			}
			foreach (KeyValuePair<byte, int> current2 in this.inputData.intTable)
			{
				list.Add(current2.Key);
				list.Add(2);
				list.AddRange(BitConverter.GetBytes(current2.Value));
			}
			foreach (KeyValuePair<byte, string> current3 in this.inputData.stringTable)
			{
				bool flag = current3.Value.Length < 256;
				if (flag)
				{
					list.Add(current3.Key);
					list.Add(1);
					list.Add((byte)current3.Value.Length);
					list.AddRange(Encoding.ASCII.GetBytes(current3.Value));
				}
			}
			foreach (KeyValuePair<byte, FP> current4 in this.inputData.fpTable)
			{
				list.Add(current4.Key);
				list.Add(3);
				list.AddRange(BitConverter.GetBytes(current4.Value.RawValue));
			}
			foreach (KeyValuePair<byte, TSVector> current5 in this.inputData.tsVectorTable)
			{
				list.Add(current5.Key);
				list.Add(4);
				List<byte> arg_235_0 = list;
				TSVector value = current5.Value;
				arg_235_0.AddRange(BitConverter.GetBytes(value.x.RawValue));
				List<byte> arg_256_0 = list;
				value = current5.Value;
				arg_256_0.AddRange(BitConverter.GetBytes(value.y.RawValue));
				List<byte> arg_277_0 = list;
				value = current5.Value;
				arg_277_0.AddRange(BitConverter.GetBytes(value.z.RawValue));
			}
			return list;
		}

		public static SyncedData[] Decode(byte[] data)
		{
			List<SyncedData> list = new List<SyncedData>();
			int i = 0;
			int num = BitConverter.ToInt32(data, i);
			i += 4;
			byte ownerID = data[i++];
			byte b = data[i++];
			bool flag = data[i++] == 1;
			int num2 = num;
			while (i < data.Length)
			{
				SyncedData syncedData = new SyncedData(ownerID, num2--);
				byte b2 = data[i++];
				for (int j = 0; j < (int)b2; j++)
				{
					byte key = data[i++];
					switch (data[i++])
					{
					case 0:
					{
						byte value = data[i++];
						syncedData.inputData.AddByte(key, value);
						break;
					}
					case 1:
					{
						byte b3 = data[i++];
						string @string = Encoding.ASCII.GetString(data, i, (int)b3);
						i += (int)b3;
						syncedData.inputData.AddString(key, @string);
						break;
					}
					case 2:
					{
						int value2 = BitConverter.ToInt32(data, i);
						syncedData.inputData.AddInt(key, value2);
						i += 4;
						break;
					}
					case 3:
					{
						FP value3 = FP.FromRaw(BitConverter.ToInt64(data, i));
						syncedData.inputData.AddFP(key, value3);
						i += 8;
						break;
					}
					case 4:
					{
						FP x = FP.FromRaw(BitConverter.ToInt64(data, i));
						i += 8;
						FP y = FP.FromRaw(BitConverter.ToInt64(data, i));
						i += 8;
						FP z = FP.FromRaw(BitConverter.ToInt64(data, i));
						i += 8;
						syncedData.inputData.AddTSVector(key, new TSVector(x, y, z));
						break;
					}
					}
				}
				list.Add(syncedData);
			}
			bool flag2 = list.Count > 0;
			if (flag2)
			{
				list[0].dropPlayer = flag;
				list[0].dropFromPlayerId = b;
			}
			return list.ToArray();
		}

		public static byte[] Encode(SyncedData[] syncedData)
		{
			List<byte> list = new List<byte>();
			bool flag = syncedData.Length != 0;
			if (flag)
			{
				list.AddRange(syncedData[0].GetEncodedHeader());
				for (int i = 0; i < syncedData.Length; i++)
				{
					list.AddRange(syncedData[i].GetEncodedActions());
				}
			}
			return list.ToArray();
		}

		public SyncedData clone()
		{
			SyncedData syncedData = new SyncedData(this.inputData.ownerID, this.tick);
			foreach (KeyValuePair<byte, byte> current in this.inputData.byteTable)
			{
				syncedData.inputData.AddByte(current.Key, current.Value);
			}
			foreach (KeyValuePair<byte, int> current2 in this.inputData.intTable)
			{
				syncedData.inputData.AddInt(current2.Key, current2.Value);
			}
			foreach (KeyValuePair<byte, string> current3 in this.inputData.stringTable)
			{
				syncedData.inputData.AddString(current3.Key, current3.Value);
			}
			foreach (KeyValuePair<byte, FP> current4 in this.inputData.fpTable)
			{
				syncedData.inputData.AddFP(current4.Key, current4.Value);
			}
			foreach (KeyValuePair<byte, TSVector> current5 in this.inputData.tsVectorTable)
			{
				syncedData.inputData.AddTSVector(current5.Key, current5.Value);
			}
			return syncedData;
		}

		public bool EqualsData(SyncedData other)
		{
			bool flag = !SyncedData.checkEqualsByteTable(this, other) || !SyncedData.checkEqualsByteTable(other, this);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !SyncedData.checkEqualsIntTable(this, other) || !SyncedData.checkEqualsIntTable(other, this);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !SyncedData.checkEqualsStringTable(this, other) || !SyncedData.checkEqualsStringTable(other, this);
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = !SyncedData.checkEqualsFPTable(this, other) || !SyncedData.checkEqualsFPTable(other, this);
						if (flag4)
						{
							result = false;
						}
						else
						{
							bool flag5 = !SyncedData.checkEqualsTSVectorTable(this, other) || !SyncedData.checkEqualsTSVectorTable(other, this);
							result = !flag5;
						}
					}
				}
			}
			return result;
		}

		private static bool checkEqualsByteTable(SyncedData s1, SyncedData s2)
		{
			bool result;
			foreach (KeyValuePair<byte, byte> current in s1.inputData.byteTable)
			{
				bool flag = !s2.inputData.byteTable.ContainsKey(current.Key) || s1.inputData.byteTable[current.Key] != s2.inputData.byteTable[current.Key];
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		private static bool checkEqualsIntTable(SyncedData s1, SyncedData s2)
		{
			bool result;
			foreach (KeyValuePair<byte, int> current in s1.inputData.intTable)
			{
				bool flag = !s2.inputData.intTable.ContainsKey(current.Key) || s1.inputData.intTable[current.Key] != s2.inputData.intTable[current.Key];
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		private static bool checkEqualsStringTable(SyncedData s1, SyncedData s2)
		{
			bool result;
			foreach (KeyValuePair<byte, string> current in s1.inputData.stringTable)
			{
				bool flag = !s2.inputData.stringTable.ContainsKey(current.Key) || s1.inputData.stringTable[current.Key] != s2.inputData.stringTable[current.Key];
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		private static bool checkEqualsFPTable(SyncedData s1, SyncedData s2)
		{
			bool result;
			foreach (KeyValuePair<byte, FP> current in s1.inputData.fpTable)
			{
				bool flag = !s2.inputData.fpTable.ContainsKey(current.Key) || s1.inputData.fpTable[current.Key] != s2.inputData.fpTable[current.Key];
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		private static bool checkEqualsTSVectorTable(SyncedData s1, SyncedData s2)
		{
			bool result;
			foreach (KeyValuePair<byte, TSVector> current in s1.inputData.tsVectorTable)
			{
				bool flag = !s2.inputData.tsVectorTable.ContainsKey(current.Key) || s1.inputData.tsVectorTable[current.Key] != s2.inputData.tsVectorTable[current.Key];
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		public override string ToString()
		{
			string text = string.Concat(new object[]
			{
				"SyncedData: { id: ",
				this.inputData.ownerID,
				" t : ",
				this.tick,
				" data {"
			});
			foreach (KeyValuePair<byte, byte> current in this.inputData.byteTable)
			{
				text = string.Concat(new object[]
				{
					text,
					current.Key,
					",",
					current.Value,
					";"
				});
			}
			foreach (KeyValuePair<byte, int> current2 in this.inputData.intTable)
			{
				text = string.Concat(new object[]
				{
					text,
					current2.Key,
					",",
					current2.Value,
					";"
				});
			}
			foreach (KeyValuePair<byte, string> current3 in this.inputData.stringTable)
			{
				text = string.Concat(new object[]
				{
					text,
					current3.Key,
					",",
					current3.Value,
					";"
				});
			}
			foreach (KeyValuePair<byte, FP> current4 in this.inputData.fpTable)
			{
				text = string.Concat(new object[]
				{
					text,
					current4.Key,
					",",
					current4.Value,
					";"
				});
			}
			foreach (KeyValuePair<byte, TSVector> current5 in this.inputData.tsVectorTable)
			{
				text = string.Concat(new object[]
				{
					text,
					current5.Key,
					",",
					current5.Value,
					";"
				});
			}
			text += "} }";
			return text;
		}
	}
}
