namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class AbstractLockstep
    {
        public int _lastSafeTick = 0;
        protected Dictionary<int, List<IBody>> bodiesToDestroy;
        private GenericBufferWindow<SyncedInfo> bufferSyncedInfo;
        private const byte CHECKSUM_CODE = 0xc6;
        public bool checksumOk;
        internal ICommunicator communicator;
        public CompoundStats compoundStats;
        protected Dictionary<int, List<Delegate>> delegatesToExecute;
        public FP deltaTime;
        private int elapsedPanicTicks;
        public TrueSyncIsReady GameIsReady;
        private TrueSyncInputCallback GetLocalData;
        internal TSPlayer localPlayer;
        private const int MAX_PANIC_BEFORE_END_GAME = 5;
        private TrueSyncEventCallback OnGameEnded;
        private TrueSyncEventCallback OnGamePaused;
        private TrueSyncEventCallback OnGameStarted;
        private TrueSyncEventCallback OnGameUnPaused;
        private TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection;
        private int panicWindow;
        protected IPhysicsManager physicsManager;
        internal SortedDictionary<byte, TSPlayer> players;
        private TrueSync.ReplayMode replayMode;
        private TrueSync.ReplayRecord replayRecord;
        internal int rollbackWindow;
        private const byte SEND_CODE = 0xc7;
        private const byte SIMULATION_CODE = 0xc5;
        private const byte SIMULATION_EVENT_END = 3;
        private const byte SIMULATION_EVENT_PAUSE = 0;
        private const byte SIMULATION_EVENT_RUN = 1;
        private SimulationState simulationState;
        protected TrueSyncUpdateCallback StepUpdate;
        private const byte SYNCED_GAME_START_CODE = 0xc4;
        private const int SYNCED_INFO_BUFFER_WINDOW = 3;
        protected int syncWindow;
        protected int ticks;
        public FP time;
        protected int totalWindow;

        public AbstractLockstep(FP deltaTime, ICommunicator communicator, IPhysicsManager physicsManager, int syncWindow, int panicWindow, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData)
        {
            this.deltaTime = deltaTime;
            this.syncWindow = syncWindow;
            this.panicWindow = panicWindow;
            this.rollbackWindow = rollbackWindow;
            this.totalWindow = syncWindow + rollbackWindow;
            this.StepUpdate = OnStepUpdate;
            this.OnGameStarted = OnGameStarted;
            this.OnGamePaused = OnGamePaused;
            this.OnGameUnPaused = OnGameUnPaused;
            this.OnGameEnded = OnGameEnded;
            this.OnPlayerDisconnection = OnPlayerDisconnection;
            this.GetLocalData = GetLocalData;
            this.ticks = 0;
            players = new SortedDictionary<byte, TSPlayer>();
            this.communicator = communicator;
            if (communicator != null)
            {
                this.communicator.AddEventListener(new OnEventReceived(OnEventDataReceived));
            }
            this.physicsManager = physicsManager;
            this.compoundStats = new CompoundStats();
            this.bufferSyncedInfo = new GenericBufferWindow<SyncedInfo>(3);
            this.checksumOk = true;
            this.simulationState = SimulationState.NOT_STARTED;
            this.bodiesToDestroy = new Dictionary<int, List<IBody>>();
            this.delegatesToExecute = new Dictionary<int, List<Delegate>>();
            this.ReplayRecord = TrueSync.ReplayRecord.replayToLoad;
            this.ReplayMode = TrueSync.ReplayRecord.replayMode;
            this.time = FP.Zero;
            StateTracker.AddTracking(this, "time");
        }

        public void AddPlayer(byte playerId, string playerName, bool isLocal)
        {
            TSPlayer player = new TSPlayer(playerId, playerName);
            this.players.Add(player.ID, player);
            if (isLocal)
            {
                this.localPlayer = player;
                this.localPlayer.sentSyncedStart = true;
            }
            if (this.replayMode == TrueSync.ReplayMode.RECORD_REPLAY)
            {
                this.replayRecord.AddPlayer(player);
            }
        }

        protected virtual void AfterStepUpdate(int syncedDataTick, int referenceTick)
        {
            foreach (TSPlayer player in players.Values)
            {
                player.RemoveData(referenceTick);
            }
        }

        protected virtual void BeforeStepUpdate(int syncedDataTick, int referenceTick)
        {
        }

        private void CheckDrop(TSPlayer p)
        {
            if (((p != this.localPlayer) && !p.dropped) && (p.dropCount > 0))
            {
                int num = -1;
                foreach (TSPlayer player in this.players.Values)
                {
                    if (!player.dropped)
                    {
                        num++;
                    }
                }
                if (p.dropCount >= num)
                {
                    this.compoundStats.globalStats.GetInfo("panic").count = 0L;
                    p.dropped = true;
                    Debug.Log("Player dropped (stopped sending input)");
                    int key = this.GetSyncedDataTick() + 1;
                    if (!this.delegatesToExecute.ContainsKey(key))
                    {
                        this.delegatesToExecute[key] = new List<Delegate>();
                    }
                    this.delegatesToExecute[key].Add(() => this.OnPlayerDisconnection(p.ID));
                }
            }
        }

        private bool CheckGameIsReady()
        {
            if (this.GameIsReady > null)
            {
                foreach (Delegate delegate2 in this.GameIsReady.GetInvocationList())
                {
                    if (!((bool) delegate2.DynamicInvoke(new object[0])))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void CheckGameStart()
        {
            if (this.replayMode == TrueSync.ReplayMode.LOAD_REPLAY)
            {
                this.RunSimulation(false);
            }
            else
            {
                bool flag = true;
                foreach (TSPlayer player in this.players.Values)
                {
                    flag &= player.sentSyncedStart;
                }
                if (flag)
                {
                    this.RunSimulation(false);
                }
                else
                {
                    SyncedInfo info = new SyncedInfo {
                        playerId = this.localPlayer.ID
                    };
                    this.RaiseEvent(0xc4, SyncedInfo.Encode(info));
                }
            }
        }

        protected void CheckSafeRemotion(int refTick)
        {
            if (this.bodiesToDestroy.ContainsKey(refTick))
            {
                List<IBody> list = this.bodiesToDestroy[refTick];
                foreach (IBody body in list)
                {
                    if (body.TSDisabled)
                    {
                        this.physicsManager.RemoveBody(body);
                    }
                }
                this.bodiesToDestroy.Remove(refTick);
            }
            if (this.delegatesToExecute.ContainsKey(refTick))
            {
                this.delegatesToExecute.Remove(refTick);
            }
        }

        public void Destroy(IBody rigidBody)
        {
            rigidBody.TSDisabled = true;
            int key = this.GetSimulatedTick(this.GetSyncedDataTick()) + 1;
            if (!this.bodiesToDestroy.ContainsKey(key))
            {
                this.bodiesToDestroy[key] = new List<IBody>();
            }
            this.bodiesToDestroy[key].Add(rigidBody);
        }

        private void DropLagPlayers()
        {
            List<TSPlayer> list = new List<TSPlayer>();
            int refTick = this.GetRefTick(this.GetSyncedDataTick());
            if (refTick >= 0)
            {
                foreach (TSPlayer player in this.players.Values)
                {
                    if (!player.IsDataReady(refTick))
                    {
                        player.dropCount++;
                        this.CheckDrop(player);
                        list.Add(player);
                    }
                }
            }
            foreach (TSPlayer player2 in list)
            {
                SyncedData[] sendDataForDrop = player2.GetSendDataForDrop(this.localPlayer.ID, 1);
                if (sendDataForDrop.Length > 0)
                {
                    this.communicator.OpRaiseEvent(0xc7, SyncedData.Encode(sendDataForDrop), true, null);
                }
            }
        }

        private void End()
        {
            if (this.simulationState != SimulationState.ENDED)
            {
                OnGameEnded();
                if (this.replayMode == TrueSync.ReplayMode.RECORD_REPLAY)
                {
                    TrueSync.ReplayRecord.SaveRecord(this.replayRecord);
                }
                this.simulationState = SimulationState.ENDED;
            }
        }

        public void EndSimulation()
        {
            End();
            byte[] message = new byte[] { 3 };
            RaiseEvent(0xc5, message, true, this.GetActivePlayers());
        }

        private void ExecuteDelegates(int syncedDataTick)
        {
            syncedDataTick++;
            if (this.delegatesToExecute.ContainsKey(syncedDataTick))
            {
                foreach (Delegate delegate2 in this.delegatesToExecute[syncedDataTick])
                {
                    delegate2.DynamicInvoke(new object[0]);
                }
            }
        }

        protected void ExecutePhysicsStep(SyncedData[] data, int syncedDataTick)
        {
            ExecuteDelegates(syncedDataTick);
            StepUpdate(SyncedArrayToInputArray(data));
            physicsManager.UpdateStep();
        }

        private int[] GetActivePlayers()
        {
            List<int> list = new List<int>();
            foreach (TSPlayer player in players.Values)
            {
                if (!player.dropped)
                {
                    list.Add(player.ID);
                }
            }
            return list.ToArray();
        }

        protected abstract string GetChecksumForSyncedInfo();
        internal InputData GetInputData(int playerId)
        {
            return this.players[(byte) playerId].GetData(this.GetSyncedDataTick()).inputData;
        }

        protected abstract int GetRefTick(int syncedDataTick);
        protected abstract int GetSimulatedTick(int syncedDataTick);
        protected int GetSyncedDataTick()
        {
            return (this.ticks - this.syncWindow);
        }

        protected SyncedData[] GetTickData(int tick)
        {
            List<SyncedData> list = new List<SyncedData>();
            foreach (TSPlayer player in this.players.Values)
            {
                if (!player.dropped)
                {
                    list.Add(player.GetData(tick));
                }
            }
            return list.ToArray();
        }

        protected abstract bool IsStepReady(int syncedDataTick);
        public static AbstractLockstep NewInstance(FP deltaTime, ICommunicator communicator, IPhysicsManager physicsManager, int syncWindow, int panicWindow, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData)
        {
            if ((rollbackWindow <= 0) || (communicator == null))
            {
                return new DefaultLockstep(deltaTime, communicator, physicsManager, syncWindow, panicWindow, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData);
            }
            return new RollbackLockstep(deltaTime, communicator, physicsManager, syncWindow, panicWindow, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData);
        }

        private void OnChecksumReceived(SyncedInfo syncedInfo)
        {
            if (!this.players[syncedInfo.playerId].dropped)
            {
                this.checksumOk = true;
                foreach (SyncedInfo info in this.bufferSyncedInfo.buffer)
                {
                    if ((info.tick == syncedInfo.tick) && (info.checksum != syncedInfo.checksum))
                    {
                        this.checksumOk = false;
                        break;
                    }
                }
            }
        }

        private void OnEventDataReceived(byte eventCode, object content)
        {
            if (eventCode == 0xc7)
            {
                byte[] data = content as byte[];
                SyncedData[] dataArray = SyncedData.Decode(data);
                if (dataArray.Length > 0)
                {
                    TSPlayer player = this.players[dataArray[0].inputData.ownerID];
                    if (!player.dropped)
                    {
                        this.OnSyncedDataReceived(player, dataArray);
                        if ((dataArray[0].dropPlayer && (player.ID != localPlayer.ID)) && !this.players[dataArray[0].dropFromPlayerId].dropped)
                        {
                            player.dropCount++;
                        }
                    }
                }
            }
            else if (eventCode == 0xc6)
            {
                byte[] infoBytes = content as byte[];
                OnChecksumReceived(SyncedInfo.Decode(infoBytes));
            }
            else if (eventCode == 0xc5)
            {
                byte[] buffer3 = content as byte[];
                if (buffer3.Length > 0)
                {
                    if (buffer3[0] == 0)
                    {
                        this.Pause();
                    }
                    else if (buffer3[0] == 1)
                    {
                        this.Run();
                    }
                    else if (buffer3[0] == 3)
                    {
                        this.End();
                    }
                }
            }
            else if (eventCode == 0xc4)
            {
                byte[] buffer4 = content as byte[];
                SyncedInfo info = SyncedInfo.Decode(buffer4);
                players[info.playerId].sentSyncedStart = true;
            }
        }

        protected abstract void OnSyncedDataReceived(TSPlayer player, SyncedData[] data);
        private void Pause()
        {
            if (this.simulationState == SimulationState.RUNNING)
            {
                this.OnGamePaused();
                this.simulationState = SimulationState.PAUSED;
            }
        }

        public void PauseSimulation()
        {
            this.Pause();
            this.RaiseEvent(0xc5, new byte[1], true, this.GetActivePlayers());
        }

        private void RaiseEvent(byte eventCode, object message)
        {
            RaiseEvent(eventCode, message, true, null);
        }

        private void RaiseEvent(byte eventCode, object message, bool reliable, int[] toPlayers)
        {
            if (communicator != null)
            {
                this.communicator.OpRaiseEvent(eventCode, message, reliable, toPlayers);
            }
        }

        private void Run()
        {
            if (simulationState == SimulationState.NOT_STARTED)
            {
                this.simulationState = SimulationState.WAITING_PLAYERS;
            }
            else if ((this.simulationState == SimulationState.WAITING_PLAYERS) || (this.simulationState == SimulationState.PAUSED))
            {
                if (this.simulationState == SimulationState.WAITING_PLAYERS)
                {
                    this.OnGameStarted();
                }
                else
                {
                    this.OnGameUnPaused();
                }
                simulationState = SimulationState.RUNNING;
            }
        }

        public void RunSimulation(bool firstRun)
        {
            Run();
            if (!firstRun)
            {
                byte[] message = new byte[] { 1 };
                this.RaiseEvent(0xc5, message, true, this.GetActivePlayers());
            }
        }

        private void SendInfoChecksum(int tick)
        {
            if (this.replayMode != TrueSync.ReplayMode.LOAD_REPLAY)
            {
                SyncedInfo info = this.bufferSyncedInfo.Current();
                info.playerId = this.localPlayer.ID;
                info.tick = tick;
                info.checksum = this.GetChecksumForSyncedInfo();
                this.bufferSyncedInfo.MoveNext();
                this.RaiseEvent(0xc6, SyncedInfo.Encode(info));
            }
        }

        protected static InputData[] SyncedArrayToInputArray(SyncedData[] data)
        {
            InputData[] dataArray = new InputData[data.Length];
            int index = 0;
            int length = data.Length;
            while (index < length)
            {
                dataArray[index] = data[index].inputData;
                index++;
            }
            return dataArray;
        }

        public void Update()
        {
            if (this.simulationState == SimulationState.WAITING_PLAYERS)
            {
                this.CheckGameStart();
            }
            else if (this.simulationState == SimulationState.RUNNING)
            {
                this.compoundStats.UpdateTime(this.deltaTime);
                if (this.communicator != null)
                {
                    this.compoundStats.AddValue("ping", (long) communicator.RoundTripTime());
                }
                if (this.syncWindow == 0)
                {
                    this.UpdateData();
                }
                foreach (TSPlayer player in this.players.Values)
                {
                    this.CheckDrop(player);
                }
                int syncedDataTick = this.GetSyncedDataTick();
                if (this.CheckGameIsReady() && this.IsStepReady(syncedDataTick))
                {
                    this.time += this.deltaTime;
                    this.compoundStats.Increment("simulated_frames");
                    this.UpdateData();
                    this.elapsedPanicTicks = 0;
                    int refTick = this.GetRefTick(syncedDataTick);
                    if ((refTick > 1) && ((refTick % 100) == 0))
                    {
                        this.SendInfoChecksum(refTick);
                    }
                    this._lastSafeTick = refTick;
                    this.BeforeStepUpdate(syncedDataTick, refTick);
                    SyncedData[] tickData = this.GetTickData(syncedDataTick);
                    if (this.replayMode == ReplayMode.RECORD_REPLAY)
                    {
                        replayRecord.AddSyncedData(this.GetTickData(refTick));
                    }
                    this.ExecutePhysicsStep(tickData, syncedDataTick);
                    this.AfterStepUpdate(syncedDataTick, refTick);
                    this.ticks++;
                }
                else if (this.ticks >= this.totalWindow)
                {
                    if (this.replayMode == TrueSync.ReplayMode.LOAD_REPLAY)
                    {
                        this.End();
                    }
                    else
                    {
                        this.compoundStats.Increment("missed_frames");
                        this.elapsedPanicTicks++;
                        if (this.elapsedPanicTicks > this.panicWindow)
                        {
                            this.compoundStats.Increment("panic");
                            if (this.compoundStats.globalStats.GetInfo("panic").count >= 5L)
                            {
                                this.End();
                            }
                            else
                            {
                                this.elapsedPanicTicks = 0;
                                this.DropLagPlayers();
                            }
                        }
                    }
                }
                else
                {
                    this.time += this.deltaTime;
                    this.compoundStats.Increment("simulated_frames");
                    this.physicsManager.UpdateStep();
                    this.UpdateData();
                    this.ticks++;
                }
            }
        }

        private SyncedData UpdateData()
        {
            if (this.replayMode == TrueSync.ReplayMode.LOAD_REPLAY)
            {
                return null;
            }
            SyncedData data = new SyncedData(this.localPlayer.ID, this.ticks);
            this.GetLocalData(data.inputData);
            this.localPlayer.AddData(data);
            if (communicator != null)
            {
                SyncedData[] sendData = this.localPlayer.GetSendData(this.ticks, 1);
                if (sendData.Length > 0)
                {
                    this.communicator.OpRaiseEvent(0xc7, SyncedData.Encode(sendData), true, this.GetActivePlayers());
                }
            }
            return data;
        }

        public int LastSafeTick
        {
            get
            {
                if (this._lastSafeTick < 0)
                {
                    return -1;
                }
                return (this._lastSafeTick - 1);
            }
        }

        public TSPlayer LocalPlayer
        {
            get
            {
                return this.localPlayer;
            }
        }

        public IDictionary<byte, TSPlayer> Players
        {
            get
            {
                return this.players;
            }
        }

        private TrueSync.ReplayMode ReplayMode
        {
            set
            {
                this.replayMode = value;
                if (this.replayMode == TrueSync.ReplayMode.RECORD_REPLAY)
                {
                    this.replayRecord = new TrueSync.ReplayRecord();
                }
            }
        }

        private TrueSync.ReplayRecord ReplayRecord
        {
            set
            {
                this.replayRecord = value;
                if (this.replayRecord > null)
                {
                    this.replayMode = TrueSync.ReplayMode.LOAD_REPLAY;
                    this.replayRecord.ApplyRecord(this);
                }
            }
        }

        public int Ticks
        {
            get
            {
                return (GetSimulatedTick(this.GetSyncedDataTick()) - 1);
            }
        }

        private enum SimulationState
        {
            NOT_STARTED,
            WAITING_PLAYERS,
            RUNNING,
            PAUSED,
            ENDED
        }
    }
}

