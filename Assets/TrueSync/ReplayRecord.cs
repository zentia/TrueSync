using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrueSync
{
	[Serializable]
	public class ReplayRecord
	{
		public static ReplayRecordSave ReplayRecordSave;

		public static ReplayRecord replayToLoad;

		public static ReplayMode replayMode = ReplayMode.RECORD_REPLAY;

		[SerializeField]
		private SerializableDictionaryBytePlayer players = new SerializableDictionaryBytePlayer();

		internal void AddSyncedData(SyncedData[] data)
		{
			for (int i = 0; i < data.Length; i++)
			{
				SyncedData syncedData = data[i];
				this.players[syncedData.inputData.ownerID].AddData(syncedData);
			}
		}

		internal void AddPlayer(TSPlayer player)
		{
			this.players[player.ID] = new TSPlayer(player.ID, player.playerInfo.name);
		}

		public static void SaveRecord(ReplayRecord replay)
		{
			bool flag = ReplayRecord.ReplayRecordSave == null;
			if (!flag)
			{
				try
				{
					ReplayRecord.ReplayRecordSave(JsonUtility.ToJson(replay), replay.players.Count);
				}
				catch (Exception)
				{
				}
			}
		}

		public static ReplayRecord GetReplayRecord(string replayContent)
		{
			ReplayRecord result = null;
			try
			{
				result = JsonUtility.FromJson<ReplayRecord>(replayContent);
			}
			catch (Exception)
			{
			}
			return result;
		}

		internal void ApplyRecord(AbstractLockstep lockStep)
		{
			foreach (KeyValuePair<byte, TSPlayer> current in this.players)
			{
				bool flag = lockStep.localPlayer == null;
				if (flag)
				{
					lockStep.localPlayer = current.Value;
				}
				lockStep.players.Add(current.Key, current.Value);
			}
		}
	}
}
