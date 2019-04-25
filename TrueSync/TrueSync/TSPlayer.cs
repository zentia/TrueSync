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
            this.playerInfo = new TSPlayerInfo(id, name);
            this.dropCount = 0;
            this.dropped = false;
            this.controls = new SerializableDictionaryIntSyncedData();
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

        public SyncedData GetData(int tick)
        {
            if (!this.controls.ContainsKey(tick))
            {
                SyncedData data = null;
                if (this.controls.ContainsKey(tick - 1))
                {
                    data = this.controls[tick - 1].clone();
                    data.tick = tick;
                }
                else
                {
                    data = new SyncedData(this.ID, tick);
                }
                data.fake = true;
                this.controls[tick] = data;
                return data;
            }
            return this.controls[tick];
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

        public bool IsDataDirty(int tick)
        {
            return (this.controls.ContainsKey(tick) && this.controls[tick].dirty);
        }

        public bool IsDataReady(int tick)
        {
            return (this.controls.ContainsKey(tick) && !this.controls[tick].fake);
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

