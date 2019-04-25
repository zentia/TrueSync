namespace TrueSync
{
    using System;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    public class TSPlayer
    {
        [SerializeField]
        internal SerializableDictionaryIntSyncedData controls;
        [NonSerialized]
        public int dropCount;
        [NonSerialized]
        public bool dropped;
        [SerializeField]
        public TSPlayerInfo playerInfo;
        [NonSerialized]
        public bool sentSyncedStart;

        internal TSPlayer(byte id, string name)
        {
            // 创建玩家信息
            playerInfo = new TSPlayerInfo(id, name);
            dropCount = 0;
            dropped = false;
            // 创建玩家操作字典存储器
            controls = new SerializableDictionaryIntSyncedData();
        }

        public void AddData(SyncedData data)
        {
            int tick = data.tick;
            if (!this.controls.ContainsKey(tick))
            {
                this.controls[tick] = data;
            }
        }

        public void AddData(SyncedData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                this.AddData(data[i]);
            }
        }

        public void AddDataProjected(int refTick, int window)
        {
            SyncedData data = this.GetData(refTick);
            for (int i = 1; i <= window; i++)
            {
                SyncedData data2 = this.GetData(refTick + i);
                if (data2.fake)
                {
                    SyncedData data3 = data.clone();
                    data3.fake = true;
                    data3.tick = refTick + i;
                    this.controls[data3.tick] = data3;
                }
                else if (data2.dirty)
                {
                    data2.dirty = false;
                    data = data2;
                }
            }
        }

        public void AddDataRollback(SyncedData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                SyncedData data2 = this.GetData(data[i].tick);
                if (data2.fake)
                {
                    if (data2.EqualsData(data[i]))
                    {
                        data2.fake = false;
                        data2.dirty = false;
                    }
                    else
                    {
                        data[i].dirty = true;
                        this.controls[data[i].tick] = data[i];
                    }
                }
            }
        }
        // 获取该帧的同步操作数据
        public SyncedData GetData(int tick)
        {
            if (!controls.ContainsKey(tick))
            {
                // 如果不存在，就查找上一帧是否存在
                SyncedData data = null;
                if (controls.ContainsKey(tick - 1))
                {
                    // 如果存在上一帧，就克隆上一帧的同步数据
                    data = controls[tick - 1].clone();
                    data.tick = tick;
                }
                else
                {
                    // 否则就新建一个同步数据
                    data = new SyncedData(ID, tick);
                }
                // 设置为伪造的
                data.fake = true;
                // 保存到帧字典
                this.controls[tick] = data;
                return data;
            }
            // 如果存在该,就返回该帧数据
            return controls[tick];
        }

        private SyncedData[] GetDataFromTick(int tick, int sendWindow)
        {
            if (tick < sendWindow)
            {
                sendWindow = tick + 1;
            }
            SyncedData[] dataArray = new SyncedData[sendWindow];
            for (int i = 0; i < sendWindow; i++)
            {
                dataArray[i] = this.GetData(tick - i);
            }
            return dataArray;
        }

        public SyncedData[] GetSendData(int tick, int sendWindow)
        {
            return this.GetDataFromTick(tick, sendWindow);
        }

        public SyncedData[] GetSendDataForDrop(byte fromPlayerId, int sendWindow)
        {
            if (this.controls.Count == 0)
            {
                return new SyncedData[0];
            }
            SyncedData[] dataFromTick = this.GetDataFromTick(this.controls.Keys.Last<int>(), sendWindow);
            dataFromTick[0] = dataFromTick[0].clone();
            dataFromTick[0].dropFromPlayerId = fromPlayerId;
            dataFromTick[0].dropPlayer = true;
            return dataFromTick;
        }
        // 获取某帧是否有模拟同步操作数据, 客户端先行，是客户端预测的操作, 回滚添加的。
        public bool IsDataDirty(int tick)
        {
            return (controls.ContainsKey(tick) && this.controls[tick].dirty);
        }
        // 获取某帧是否有真实同步操作数据
        public bool IsDataReady(int tick)
        {
            return (controls.ContainsKey(tick) && !controls[tick].fake);
        }

        public void RemoveData(int refTick)
        {
            controls.Remove(refTick);
        }

        public byte ID
        {
            get
            {
                return this.playerInfo.id;
            }
        }
    }
}

