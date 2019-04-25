namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class ReplayRecord
    {
        [SerializeField]
        private SerializableDictionaryBytePlayer players = new SerializableDictionaryBytePlayer();
        public static ReplayMode replayMode = ReplayMode.RECORD_REPLAY;
        public static TrueSync.ReplayRecordSave ReplayRecordSave;
        public static ReplayRecord replayToLoad;

        internal void AddPlayer(TSPlayer player)
        {
            this.players[player.ID] = new TSPlayer(player.ID, player.playerInfo.name);
        }

        internal void AddSyncedData(SyncedData[] data)
        {
            foreach (SyncedData data2 in data)
            {
                players[data2.inputData.ownerID].AddData(data2);
            }
        }

        internal void ApplyRecord(AbstractLockstep lockStep)
        {
            foreach (KeyValuePair<byte, TSPlayer> pair in this.players)
            {
                if (lockStep.localPlayer == null)
                {
                    lockStep.localPlayer = pair.Value;
                }
                lockStep.players.Add(pair.Key, pair.Value);
            }
        }

        public static ReplayRecord GetReplayRecord(string replayContent)
        {
            ReplayRecord record = null;
            try
            {
                record = JsonUtility.FromJson<ReplayRecord>(replayContent);
            }
            catch (Exception)
            {
            }
            return record;
        }

        public static void SaveRecord(ReplayRecord replay)
        {
            if (ReplayRecordSave != null)
            {
                try
                {
                    ReplayRecordSave(JsonUtility.ToJson(replay), replay.players.Count);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}

