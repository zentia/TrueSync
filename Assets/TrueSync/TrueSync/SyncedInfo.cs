namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class SyncedInfo
    {
        // 校验码
        public string checksum;
        private const int CHECKSUM_LENGTH = 0x20;
        // 玩家ID
        public byte playerId;
        // 帧
        public int tick;

        public SyncedInfo()
        {
        }

        public SyncedInfo(byte playerId, int tick, string checksum)
        {
            this.playerId = playerId;
            this.tick = tick;
            this.checksum = checksum;
        }
        // 序列化
        public static SyncedInfo Decode(byte[] infoBytes)
        {
            SyncedInfo info = new SyncedInfo();
            int startIndex = 0;
            info.playerId = infoBytes[startIndex++];
            if (startIndex < infoBytes.Length)
            {
                info.tick = BitConverter.ToInt32(infoBytes, startIndex);
                startIndex += 4;
                info.checksum = Encoding.ASCII.GetString(infoBytes, startIndex, 0x20);
            }
            return info;
        }

        public static byte[] Encode(SyncedInfo info)
        {
            List<byte> list = new List<byte> {
                info.playerId
            };
            if (info.checksum != null)
            {
                list.AddRange(BitConverter.GetBytes(info.tick));
                list.AddRange(Encoding.ASCII.GetBytes(info.checksum));
            }
            return list.ToArray();
        }
    }
}