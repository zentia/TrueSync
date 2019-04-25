namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public class SyncedData
    {
        [NonSerialized]
        public bool dirty;
        [NonSerialized]
        public byte dropFromPlayerId;
        [NonSerialized]
        public bool dropPlayer;
        [NonSerialized]
        public bool fake;
        public InputData inputData;
        public int tick;

        public SyncedData(byte ownerID, int tick)
        {
            this.inputData = new InputData(ownerID);
            this.tick = tick;
            this.fake = false;
            this.dirty = false;
        }

        private static bool checkEqualsByteTable(SyncedData s1, SyncedData s2)
        {
            foreach (KeyValuePair<byte, byte> pair in s1.inputData.byteTable)
            {
                if (!s2.inputData.byteTable.ContainsKey(pair.Key) || (s1.inputData.byteTable[pair.Key] != s2.inputData.byteTable[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool checkEqualsFPTable(SyncedData s1, SyncedData s2)
        {
            foreach (KeyValuePair<byte, FP> pair in s1.inputData.fpTable)
            {
                if (!s2.inputData.fpTable.ContainsKey(pair.Key) || (s1.inputData.fpTable[pair.Key] != s2.inputData.fpTable[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool checkEqualsIntTable(SyncedData s1, SyncedData s2)
        {
            foreach (KeyValuePair<byte, int> pair in s1.inputData.intTable)
            {
                if (!s2.inputData.intTable.ContainsKey(pair.Key) || (s1.inputData.intTable[pair.Key] != s2.inputData.intTable[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool checkEqualsStringTable(SyncedData s1, SyncedData s2)
        {
            foreach (KeyValuePair<byte, string> pair in s1.inputData.stringTable)
            {
                if (!s2.inputData.stringTable.ContainsKey(pair.Key) || (s1.inputData.stringTable[pair.Key] != s2.inputData.stringTable[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool checkEqualsTSVectorTable(SyncedData s1, SyncedData s2)
        {
            foreach (KeyValuePair<byte, TSVector> pair in s1.inputData.tsVectorTable)
            {
                if (!s2.inputData.tsVectorTable.ContainsKey(pair.Key) || (s1.inputData.tsVectorTable[pair.Key] != s2.inputData.tsVectorTable[pair.Key]))
                {
                    return false;
                }
            }
            return true;
        }

        public SyncedData clone()
        {
            SyncedData data = new SyncedData(this.inputData.ownerID, this.tick);
            foreach (KeyValuePair<byte, byte> pair in this.inputData.byteTable)
            {
                data.inputData.AddByte(pair.Key, pair.Value);
            }
            foreach (KeyValuePair<byte, int> pair2 in this.inputData.intTable)
            {
                data.inputData.AddInt(pair2.Key, pair2.Value);
            }
            foreach (KeyValuePair<byte, string> pair3 in this.inputData.stringTable)
            {
                data.inputData.AddString(pair3.Key, pair3.Value);
            }
            foreach (KeyValuePair<byte, FP> pair4 in this.inputData.fpTable)
            {
                data.inputData.AddFP(pair4.Key, pair4.Value);
            }
            foreach (KeyValuePair<byte, TSVector> pair5 in this.inputData.tsVectorTable)
            {
                data.inputData.AddTSVector(pair5.Key, pair5.Value);
            }
            return data;
        }

        public static SyncedData[] Decode(byte[] data)
        {
            List<SyncedData> list = new List<SyncedData>();
            int startIndex = 0;
            int num2 = BitConverter.ToInt32(data, startIndex);
            startIndex += 4;
            byte ownerID = data[startIndex++];
            byte num4 = data[startIndex++];
            bool flag = data[startIndex++] == 1;
            int num5 = num2;
            while (startIndex < data.Length)
            {
                SyncedData item = new SyncedData(ownerID, num5--);
                byte num6 = data[startIndex++];
                for (int i = 0; i < num6; i++)
                {
                    byte key = data[startIndex++];
                    switch (data[startIndex++])
                    {
                        case 0:
                        {
                            byte num12 = data[startIndex++];
                            item.inputData.AddByte(key, num12);
                            break;
                        }
                        case 1:
                        {
                            byte count = data[startIndex++];
                            string str = Encoding.ASCII.GetString(data, startIndex, count);
                            startIndex += count;
                            item.inputData.AddString(key, str);
                            break;
                        }
                        case 2:
                        {
                            int num11 = BitConverter.ToInt32(data, startIndex);
                            item.inputData.AddInt(key, num11);
                            startIndex += 4;
                            break;
                        }
                        case 3:
                        {
                            FP fp = FP.FromRaw(BitConverter.ToInt64(data, startIndex));
                            item.inputData.AddFP(key, fp);
                            startIndex += 8;
                            break;
                        }
                        case 4:
                        {
                            FP x = FP.FromRaw(BitConverter.ToInt64(data, startIndex));
                            startIndex += 8;
                            FP y = FP.FromRaw(BitConverter.ToInt64(data, startIndex));
                            startIndex += 8;
                            FP z = FP.FromRaw(BitConverter.ToInt64(data, startIndex));
                            startIndex += 8;
                            item.inputData.AddTSVector(key, new TSVector(x, y, z));
                            break;
                        }
                    }
                }
                list.Add(item);
            }
            if (list.Count > 0)
            {
                list[0].dropPlayer = flag;
                list[0].dropFromPlayerId = num4;
            }
            return list.ToArray();
        }

        public static byte[] Encode(SyncedData[] syncedData)
        {
            List<byte> list = new List<byte>();
            if (syncedData.Length > 0)
            {
                list.AddRange(syncedData[0].GetEncodedHeader());
                for (int i = 0; i < syncedData.Length; i++)
                {
                    list.AddRange(syncedData[i].GetEncodedActions());
                }
            }
            return list.ToArray();
        }

        public byte[] Encoded()
        {
            List<byte> list = new List<byte>();
            list.AddRange(this.GetEncodedHeader());
            list.AddRange(this.GetEncodedActions());
            return list.ToArray();
        }

        public bool EqualsData(SyncedData other)
        {
            if (!checkEqualsByteTable(this, other) || !checkEqualsByteTable(other, this))
            {
                return false;
            }
            if (!checkEqualsIntTable(this, other) || !checkEqualsIntTable(other, this))
            {
                return false;
            }
            if (!checkEqualsStringTable(this, other) || !checkEqualsStringTable(other, this))
            {
                return false;
            }
            if (!checkEqualsFPTable(this, other) || !checkEqualsFPTable(other, this))
            {
                return false;
            }
            if (!checkEqualsTSVectorTable(this, other) || !checkEqualsTSVectorTable(other, this))
            {
                return false;
            }
            return true;
        }

        public List<byte> GetEncodedActions()
        {
            List<byte> list = new List<byte>();
            byte count = (byte) this.inputData.Count;
            list.Add(count);
            foreach (KeyValuePair<byte, byte> pair in this.inputData.byteTable)
            {
                list.Add(pair.Key);
                list.Add(0);
                list.Add(pair.Value);
            }
            foreach (KeyValuePair<byte, int> pair2 in this.inputData.intTable)
            {
                list.Add(pair2.Key);
                list.Add(2);
                list.AddRange(BitConverter.GetBytes(pair2.Value));
            }
            foreach (KeyValuePair<byte, string> pair3 in this.inputData.stringTable)
            {
                if (pair3.Value.Length < 0x100)
                {
                    list.Add(pair3.Key);
                    list.Add(1);
                    list.Add((byte) pair3.Value.Length);
                    list.AddRange(Encoding.ASCII.GetBytes(pair3.Value));
                }
            }
            foreach (KeyValuePair<byte, FP> pair4 in this.inputData.fpTable)
            {
                list.Add(pair4.Key);
                list.Add(3);
                list.AddRange(BitConverter.GetBytes(pair4.Value.RawValue));
            }
            foreach (KeyValuePair<byte, TSVector> pair5 in this.inputData.tsVectorTable)
            {
                list.Add(pair5.Key);
                list.Add(4);
                list.AddRange(BitConverter.GetBytes(pair5.Value.x.RawValue));
                list.AddRange(BitConverter.GetBytes(pair5.Value.y.RawValue));
                list.AddRange(BitConverter.GetBytes(pair5.Value.z.RawValue));
            }
            return list;
        }

        public List<byte> GetEncodedHeader()
        {
            List<byte> list = new List<byte>();
            byte[] bytes = BitConverter.GetBytes(this.tick);
            list.AddRange(bytes);
            list.Add(this.inputData.ownerID);
            list.Add(this.dropFromPlayerId);
            list.Add(this.dropPlayer ? ((byte) 1) : ((byte) 0));
            return list;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "SyncedData: { id: ", this.inputData.ownerID, " t : ", this.tick, " data {" };
            string str = string.Concat(objArray1);
            foreach (KeyValuePair<byte, byte> pair in this.inputData.byteTable)
            {
                object[] objArray2 = new object[] { str, pair.Key, ",", pair.Value, ";" };
                str = string.Concat(objArray2);
            }
            foreach (KeyValuePair<byte, int> pair2 in this.inputData.intTable)
            {
                object[] objArray3 = new object[] { str, pair2.Key, ",", pair2.Value, ";" };
                str = string.Concat(objArray3);
            }
            foreach (KeyValuePair<byte, string> pair3 in this.inputData.stringTable)
            {
                object[] objArray4 = new object[] { str, pair3.Key, ",", pair3.Value, ";" };
                str = string.Concat(objArray4);
            }
            foreach (KeyValuePair<byte, FP> pair4 in this.inputData.fpTable)
            {
                object[] objArray5 = new object[] { str, pair4.Key, ",", pair4.Value, ";" };
                str = string.Concat(objArray5);
            }
            foreach (KeyValuePair<byte, TSVector> pair5 in this.inputData.tsVectorTable)
            {
                object[] objArray6 = new object[] { str, pair5.Key, ",", pair5.Value, ";" };
                str = string.Concat(objArray6);
            }
            return (str + "} }");
        }

        private enum Types : byte
        {
            Byte = 0,
            FP = 3,
            Integer = 2,
            String = 1,
            TSVector = 4
        }
    }
}

