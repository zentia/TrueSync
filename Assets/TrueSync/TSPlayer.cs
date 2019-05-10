using System;
using System.Linq;
using UnityEngine;

namespace TrueSync
{
	[Serializable]
	public class TSPlayer
	{
		[SerializeField]
		public TSPlayerInfo playerInfo;

		[NonSerialized]
		public int dropCount;

		[NonSerialized]
		public bool dropped;

		[NonSerialized]
		public bool sentSyncedStart;

		[SerializeField]
		internal SerializableDictionaryIntSyncedData controls;

		public byte ID
		{
			get
			{
				return this.playerInfo.id;
			}
		}

		internal TSPlayer(byte id, string name)
		{
			this.playerInfo = new TSPlayerInfo(id, name);
			this.dropCount = 0;
			this.dropped = false;
			this.controls = new SerializableDictionaryIntSyncedData();
		}

		public bool IsDataReady(int tick)
		{
			return this.controls.ContainsKey(tick) && !this.controls[tick].fake;
		}

		public bool IsDataDirty(int tick)
		{
			bool flag = this.controls.ContainsKey(tick);
			return flag && this.controls[tick].dirty;
		}

		public SyncedData GetData(int tick)
		{
			bool flag = !this.controls.ContainsKey(tick);
			SyncedData result;
			if (flag)
			{
				bool flag2 = this.controls.ContainsKey(tick - 1);
				SyncedData syncedData;
				if (flag2)
				{
					syncedData = this.controls[tick - 1].clone();
					syncedData.tick = tick;
				}
				else
				{
					syncedData = new SyncedData(this.ID, tick);
				}
				syncedData.fake = true;
				this.controls[tick] = syncedData;
				result = syncedData;
			}
			else
			{
				result = this.controls[tick];
			}
			return result;
		}

		public void AddData(SyncedData data)
		{
			int tick = data.tick;
			bool flag = !this.controls.ContainsKey(tick);
			if (flag)
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

		public void RemoveData(int refTick)
		{
			this.controls.Remove(refTick);
		}

		public void AddDataProjected(int refTick, int window)
		{
			SyncedData syncedData = this.GetData(refTick);
			for (int i = 1; i <= window; i++)
			{
				SyncedData data = this.GetData(refTick + i);
				bool fake = data.fake;
				if (fake)
				{
					SyncedData syncedData2 = syncedData.clone();
					syncedData2.fake = true;
					syncedData2.tick = refTick + i;
					this.controls[syncedData2.tick] = syncedData2;
				}
				else
				{
					bool dirty = data.dirty;
					if (dirty)
					{
						data.dirty = false;
						syncedData = data;
					}
				}
			}
		}

		public void AddDataRollback(SyncedData[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				SyncedData data2 = this.GetData(data[i].tick);
				bool fake = data2.fake;
				if (fake)
				{
					bool flag = data2.EqualsData(data[i]);
					if (flag)
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

		public SyncedData[] GetSendDataForDrop(byte fromPlayerId, int sendWindow)
		{
			bool flag = this.controls.Count == 0;
			SyncedData[] result;
			if (flag)
			{
				result = new SyncedData[0];
			}
			else
			{
				SyncedData[] dataFromTick = this.GetDataFromTick(this.controls.Keys.Last<int>(), sendWindow);
				dataFromTick[0] = dataFromTick[0].clone();
				dataFromTick[0].dropFromPlayerId = fromPlayerId;
				dataFromTick[0].dropPlayer = true;
				result = dataFromTick;
			}
			return result;
		}

		public SyncedData[] GetSendData(int tick, int sendWindow)
		{
			return this.GetDataFromTick(tick, sendWindow);
		}

		private SyncedData[] GetDataFromTick(int tick, int sendWindow)
		{
			bool flag = tick < sendWindow;
			if (flag)
			{
				sendWindow = tick + 1;
			}
			SyncedData[] array = new SyncedData[sendWindow];
			for (int i = 0; i < sendWindow; i++)
			{
				array[i] = this.GetData(tick - i);
			}
			return array;
		}
	}
}
