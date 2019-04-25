namespace TrueSync
{
    using System;

    internal class RollbackLockstep : AbstractLockstep
    {
        private GenericBufferWindow<IWorldClone> bufferWorldClone;
        internal int rollbackIndex;
        internal int rollbackIndexOffset;

        public RollbackLockstep(FP deltaTime, ICommunicator communicator, IPhysicsManager physicsManager, int syncWindow, int panicTime, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData) : base(deltaTime, communicator, physicsManager, syncWindow, panicTime, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData)
        {
            this.bufferWorldClone = new GenericBufferWindow<IWorldClone>(rollbackWindow, new GenericBufferWindow<IWorldClone>.NewInstance(physicsManager.GetWorldClone));
            this.rollbackIndex = rollbackWindow;
            StateTracker.Init(rollbackWindow);
        }

        protected override void BeforeStepUpdate(int syncedDataTick, int referenceTick)
        {
            base.BeforeStepUpdate(syncedDataTick, referenceTick);
            this.rollbackIndexOffset = 0;
            int rollbackWindow = base.rollbackWindow;
            bool flag = false;
            if (referenceTick >= 0)
            {
                if (base.bodiesToDestroy.ContainsKey(referenceTick))
                {
                    flag = true;
                }
                else
                {
                    for (int i = 0; i < base.rollbackWindow; i++)
                    {
                        foreach (TSPlayer player in base.players.Values)
                        {
                            flag = flag || (player.IsDataDirty(referenceTick) && !player.dropped);
                        }
                        if (flag)
                        {
                            break;
                        }
                        this.rollbackIndexOffset++;
                        referenceTick++;
                        rollbackWindow--;
                        this.bufferWorldClone.MoveNext();
                        StateTracker.instance.MoveNextState();
                    }
                }
            }
            if (flag)
            {
                base.compoundStats.Increment("rollback");
                this.Rollback(referenceTick, rollbackWindow);
            }
            this.rollbackIndexOffset = 0;
            this.SaveWorldClone(syncedDataTick);
        }

        protected override string GetChecksumForSyncedInfo()
        {
            return this.bufferWorldClone.Current().checksum;
        }

        protected override int GetRefTick(int syncedDataTick)
        {
            return (syncedDataTick - base.rollbackWindow);
        }

        protected override int GetSimulatedTick(int syncedDataTick)
        {
            int num = syncedDataTick - base.rollbackWindow;
            return ((num + this.rollbackIndex) + this.rollbackIndexOffset);
        }

        protected override bool IsStepReady(int syncedDataTick)
        {
            bool flag = base.localPlayer.IsDataReady(syncedDataTick);
            int tick = syncedDataTick - base.rollbackWindow;
            if (tick >= 0)
            {
                foreach (TSPlayer player in base.players.Values)
                {
                    flag = flag && (player.IsDataReady(tick) || player.dropped);
                }
            }
            return flag;
        }

        protected override void OnSyncedDataReceived(TSPlayer player, SyncedData[] data)
        {
            player.AddDataRollback(data);
        }

        private void RestorePreviousState()
        {
            StateTracker.instance.RestoreState();
            this.bufferWorldClone.Current().Restore(base.physicsManager.GetWorld());
        }

        private void Rollback(int rollbackTick, int temporaryRollbackWindow)
        {
            foreach (TSPlayer player in base.players.Values)
            {
                player.GetData(rollbackTick).dirty = false;
                player.AddDataProjected(rollbackTick, base.syncWindow + temporaryRollbackWindow);
            }
            this.RestorePreviousState();
            base.CheckSafeRemotion(rollbackTick);
            this.rollbackIndex = 0;
            while (this.rollbackIndex < temporaryRollbackWindow)
            {
                SyncedData[] tickData = base.GetTickData(rollbackTick + this.rollbackIndex);
                if (this.rollbackIndex > 0)
                {
                    this.SaveWorldClone(rollbackTick + this.rollbackIndex);
                }
                else
                {
                    this.bufferWorldClone.MoveNext();
                    StateTracker.instance.MoveNextState();
                }
                base.ExecutePhysicsStep(tickData, rollbackTick + this.rollbackIndex);
                this.rollbackIndex++;
            }
            this.rollbackIndex = base.rollbackWindow;
        }

        private void SaveWorldClone(int refTicks)
        {
            this.bufferWorldClone.Current().Clone(base.physicsManager.GetWorld(), (refTicks % 100) == 0);
            this.bufferWorldClone.MoveNext();
            StateTracker.instance.SaveState();
        }
    }
}

