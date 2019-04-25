namespace TrueSync
{
    using System;

    internal class DefaultLockstep : AbstractLockstep
    {
        public DefaultLockstep(FP deltaTime, ICommunicator communicator, IPhysicsManager physicsManager, int syncWindow, int panicTime, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData) : base(deltaTime, communicator, physicsManager, syncWindow, panicTime, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData)
        {
        }

        protected override void BeforeStepUpdate(int syncedDataTick, int referenceTick)
        {
            base.BeforeStepUpdate(syncedDataTick, referenceTick);
            base.CheckSafeRemotion(syncedDataTick);
        }

        protected override string GetChecksumForSyncedInfo()
        {
            return ChecksumExtractor.GetEncodedChecksum();
        }

        protected override int GetRefTick(int syncedDataTick)
        {
            return syncedDataTick;
        }

        protected override int GetSimulatedTick(int syncedDataTick)
        {
            return syncedDataTick;
        }

        protected override bool IsStepReady(int syncedDataTick)
        {
            bool flag = true;
            foreach (TSPlayer player in base.players.Values)
            {
                flag = flag && (player.IsDataReady(syncedDataTick) || player.dropped);
            }
            return flag;
        }

        protected override void OnSyncedDataReceived(TSPlayer player, SyncedData[] data)
        {
            player.AddData(data);
        }
    }
}

