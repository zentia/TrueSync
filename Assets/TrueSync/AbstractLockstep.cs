using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrueSync
{
	public abstract class AbstractLockstep
	{
		private enum SimulationState
		{
			NOT_STARTED,
			WAITING_PLAYERS,
			RUNNING,
			PAUSED,
			ENDED
		}

		private const byte SYNCED_GAME_START_CODE = 196;

		private const byte SIMULATION_CODE = 197;

		private const byte CHECKSUM_CODE = 198;

		private const byte SEND_CODE = 199;

		private const byte SIMULATION_EVENT_PAUSE = 0;

		private const byte SIMULATION_EVENT_RUN = 1;

		private const byte SIMULATION_EVENT_END = 3;

		private const int MAX_PANIC_BEFORE_END_GAME = 5;

		private const int SYNCED_INFO_BUFFER_WINDOW = 3;

		internal SortedDictionary<byte, TSPlayer> players;

		internal TSPlayer localPlayer;

		protected TrueSyncUpdateCallback StepUpdate;

		private TrueSyncInputCallback GetLocalData;

		private TrueSyncEventCallback OnGameStarted;

		private TrueSyncEventCallback OnGamePaused;

		private TrueSyncEventCallback OnGameUnPaused;

		private TrueSyncEventCallback OnGameEnded;

		private TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection;

		public TrueSyncIsReady GameIsReady;

		protected int ticks;

		private int panicWindow;

		protected int syncWindow;

		private int elapsedPanicTicks;

		private AbstractLockstep.SimulationState simulationState;

		internal int rollbackWindow;

		internal ICommunicator communicator;

		protected IPhysicsManager physicsManager;

		private GenericBufferWindow<SyncedInfo> bufferSyncedInfo;

		protected int totalWindow;

		public bool checksumOk;

		public CompoundStats compoundStats;

		public FP deltaTime;

		public FP time;

		public int _lastSafeTick = 0;

		protected Dictionary<int, List<IBody>> bodiesToDestroy;

		protected Dictionary<int, List<Delegate>> delegatesToExecute;

		private ReplayMode replayMode;

		private ReplayRecord replayRecord;

		public IDictionary<byte, TSPlayer> Players
		{
			get
			{
				return this.players;
			}
		}

		public TSPlayer LocalPlayer
		{
			get
			{
				return this.localPlayer;
			}
		}

		public int Ticks
		{
			get
			{
				return this.GetSimulatedTick(this.GetSyncedDataTick()) - 1;
			}
		}

		public int LastSafeTick
		{
			get
			{
				bool flag = this._lastSafeTick < 0;
				int result;
				if (flag)
				{
					result = -1;
				}
				else
				{
					result = this._lastSafeTick - 1;
				}
				return result;
			}
		}

		private ReplayMode ReplayMode
		{
			set
			{
				this.replayMode = value;
				bool flag = this.replayMode == ReplayMode.RECORD_REPLAY;
				if (flag)
				{
					this.replayRecord = new ReplayRecord();
				}
			}
		}

		private ReplayRecord ReplayRecord
		{
			set
			{
				this.replayRecord = value;
				bool flag = this.replayRecord != null;
				if (flag)
				{
					this.replayMode = ReplayMode.LOAD_REPLAY;
					this.replayRecord.ApplyRecord(this);
				}
			}
		}

		public static AbstractLockstep NewInstance(FP deltaTime, ICommunicator communicator, IPhysicsManager physicsManager, int syncWindow, int panicWindow, int rollbackWindow, TrueSyncEventCallback OnGameStarted, TrueSyncEventCallback OnGamePaused, TrueSyncEventCallback OnGameUnPaused, TrueSyncEventCallback OnGameEnded, TrueSyncPlayerDisconnectionCallback OnPlayerDisconnection, TrueSyncUpdateCallback OnStepUpdate, TrueSyncInputCallback GetLocalData)
		{
			bool flag = rollbackWindow <= 0 || communicator == null;
			AbstractLockstep result;
			if (flag)
			{
				result = new DefaultLockstep(deltaTime, communicator, physicsManager, syncWindow, panicWindow, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData);
			}
			else
			{
				result = new RollbackLockstep(deltaTime, communicator, physicsManager, syncWindow, panicWindow, rollbackWindow, OnGameStarted, OnGamePaused, OnGameUnPaused, OnGameEnded, OnPlayerDisconnection, OnStepUpdate, GetLocalData);
			}
			return result;
		}

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
			this.players = new SortedDictionary<byte, TSPlayer>();
			this.communicator = communicator;
			bool flag = communicator != null;
			if (flag)
			{
				this.communicator.AddEventListener(new OnEventReceived(this.OnEventDataReceived));
			}
			this.physicsManager = physicsManager;
			this.compoundStats = new CompoundStats();
			this.bufferSyncedInfo = new GenericBufferWindow<SyncedInfo>(3);
			this.checksumOk = true;
			this.simulationState = AbstractLockstep.SimulationState.NOT_STARTED;
			this.bodiesToDestroy = new Dictionary<int, List<IBody>>();
			this.delegatesToExecute = new Dictionary<int, List<Delegate>>();
			this.ReplayRecord = ReplayRecord.replayToLoad;
			this.ReplayMode = ReplayRecord.replayMode;
			this.time = FP.Zero;
			StateTracker.AddTracking(this, "time");
		}

		protected int GetSyncedDataTick()
		{
			return this.ticks - this.syncWindow;
		}

		protected abstract int GetRefTick(int syncedDataTick);

		protected virtual void BeforeStepUpdate(int syncedDataTick, int referenceTick)
		{
		}

		protected virtual void AfterStepUpdate(int syncedDataTick, int referenceTick)
		{
			foreach (TSPlayer current in this.players.Values)
			{
				current.RemoveData(referenceTick);
			}
		}

		protected abstract bool IsStepReady(int syncedDataTick);

		protected abstract void OnSyncedDataReceived(TSPlayer player, SyncedData[] data);

		protected abstract string GetChecksumForSyncedInfo();

		protected abstract int GetSimulatedTick(int syncedDataTick);

		private void Run()
		{
			bool flag = this.simulationState == AbstractLockstep.SimulationState.NOT_STARTED;
			if (flag)
			{
				this.simulationState = AbstractLockstep.SimulationState.WAITING_PLAYERS;
			}
			else
			{
				bool flag2 = this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS || this.simulationState == AbstractLockstep.SimulationState.PAUSED;
				if (flag2)
				{
					bool flag3 = this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS;
					if (flag3)
					{
						this.OnGameStarted();
					}
					else
					{
						this.OnGameUnPaused();
					}
					this.simulationState = AbstractLockstep.SimulationState.RUNNING;
				}
			}
		}

		private void Pause()
		{
			bool flag = this.simulationState == AbstractLockstep.SimulationState.RUNNING;
			if (flag)
			{
				this.OnGamePaused();
				this.simulationState = AbstractLockstep.SimulationState.PAUSED;
			}
		}

		private void End()
		{
			bool flag = this.simulationState != AbstractLockstep.SimulationState.ENDED;
			if (flag)
			{
				this.OnGameEnded();
				bool flag2 = this.replayMode == ReplayMode.RECORD_REPLAY;
				if (flag2)
				{
					ReplayRecord.SaveRecord(this.replayRecord);
				}
				this.simulationState = AbstractLockstep.SimulationState.ENDED;
			}
		}

		public void Update()
		{
			bool flag = this.simulationState == AbstractLockstep.SimulationState.WAITING_PLAYERS;
			if (flag)
			{
				this.CheckGameStart();
			}
			else
			{
				bool flag2 = this.simulationState == AbstractLockstep.SimulationState.RUNNING;
				if (flag2)
				{
					this.compoundStats.UpdateTime(this.deltaTime);
					bool flag3 = this.communicator != null;
					if (flag3)
					{
						this.compoundStats.AddValue("ping", (long)this.communicator.RoundTripTime());
					}
					bool flag4 = this.syncWindow == 0;
					if (flag4)
					{
						this.UpdateData();
					}
					foreach (TSPlayer current in this.players.Values)
					{
						this.CheckDrop(current);
					}
					int syncedDataTick = this.GetSyncedDataTick();
					bool flag5 = this.CheckGameIsReady() && this.IsStepReady(syncedDataTick);
					bool flag6 = flag5;
					if (flag6)
					{
						this.time += this.deltaTime;
						this.compoundStats.Increment("simulated_frames");
						this.UpdateData();
						this.elapsedPanicTicks = 0;
						int refTick = this.GetRefTick(syncedDataTick);
						bool flag7 = refTick > 1 && refTick % 100 == 0;
						if (flag7)
						{
							this.SendInfoChecksum(refTick);
						}
						this._lastSafeTick = refTick;
						this.BeforeStepUpdate(syncedDataTick, refTick);
						SyncedData[] tickData = this.GetTickData(syncedDataTick);
						bool flag8 = this.replayMode == ReplayMode.RECORD_REPLAY;
						if (flag8)
						{
							this.replayRecord.AddSyncedData(this.GetTickData(refTick));
						}
						this.ExecutePhysicsStep(tickData, syncedDataTick);
						this.AfterStepUpdate(syncedDataTick, refTick);
						this.ticks++;
					}
					else
					{
						bool flag9 = this.ticks >= this.totalWindow;
						if (flag9)
						{
							bool flag10 = this.replayMode == ReplayMode.LOAD_REPLAY;
							if (flag10)
							{
								this.End();
							}
							else
							{
								this.compoundStats.Increment("missed_frames");
								this.elapsedPanicTicks++;
								bool flag11 = this.elapsedPanicTicks > this.panicWindow;
								if (flag11)
								{
									this.compoundStats.Increment("panic");
									bool flag12 = this.compoundStats.globalStats.GetInfo("panic").count >= 5L;
									if (flag12)
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
			}
		}

		private bool CheckGameIsReady()
		{
			bool flag = this.GameIsReady != null;
			bool result;
			if (flag)
			{
				Delegate[] invocationList = this.GameIsReady.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					Delegate @delegate = invocationList[i];
					bool flag2 = (bool)@delegate.DynamicInvoke(new object[0]);
					bool flag3 = !flag2;
					if (flag3)
					{
						result = false;
						return result;
					}
				}
			}
			result = true;
			return result;
		}

		protected void ExecutePhysicsStep(SyncedData[] data, int syncedDataTick)
		{
			this.ExecuteDelegates(syncedDataTick);
			this.StepUpdate(AbstractLockstep.SyncedArrayToInputArray(data));
			this.physicsManager.UpdateStep();
		}

		private void ExecuteDelegates(int syncedDataTick)
		{
			syncedDataTick++;
			bool flag = this.delegatesToExecute.ContainsKey(syncedDataTick);
			if (flag)
			{
				foreach (Delegate current in this.delegatesToExecute[syncedDataTick])
				{
					current.DynamicInvoke(new object[0]);
				}
			}
		}

		private int[] GetActivePlayers()
		{
			List<int> list = new List<int>();
			foreach (TSPlayer current in this.players.Values)
			{
				bool flag = !current.dropped;
				if (flag)
				{
					list.Add((int)current.ID);
				}
			}
			return list.ToArray();
		}

		private void CheckGameStart()
		{
			bool flag = this.replayMode == ReplayMode.LOAD_REPLAY;
			if (flag)
			{
				this.RunSimulation(false);
			}
			else
			{
				bool flag2 = true;
				foreach (TSPlayer current in this.players.Values)
				{
					flag2 &= current.sentSyncedStart;
				}
				bool flag3 = flag2;
				if (flag3)
				{
					this.RunSimulation(false);
				}
				else
				{
					this.RaiseEvent(196, SyncedInfo.Encode(new SyncedInfo
					{
						playerId = this.localPlayer.ID
					}));
				}
			}
		}

		protected static InputData[] SyncedArrayToInputArray(SyncedData[] data)
		{
			InputData[] array = new InputData[data.Length];
			int i = 0;
			int num = data.Length;
			while (i < num)
			{
				array[i] = data[i].inputData;
				i++;
			}
			return array;
		}

		public void PauseSimulation()
		{
			this.Pause();
			this.RaiseEvent(197, new byte[1], true, this.GetActivePlayers());
		}

		public void RunSimulation(bool firstRun)
		{
			this.Run();
			bool flag = !firstRun;
			if (flag)
			{
				this.RaiseEvent(197, new byte[]
				{
					1
				}, true, this.GetActivePlayers());
			}
		}

		public void EndSimulation()
		{
			this.End();
			this.RaiseEvent(197, new byte[]
			{
				3
			}, true, this.GetActivePlayers());
		}

		public void Destroy(IBody rigidBody)
		{
			rigidBody.TSDisabled = true;
			int key = this.GetSimulatedTick(this.GetSyncedDataTick()) + 1;
			bool flag = !this.bodiesToDestroy.ContainsKey(key);
			if (flag)
			{
				this.bodiesToDestroy[key] = new List<IBody>();
			}
			this.bodiesToDestroy[key].Add(rigidBody);
		}

		protected void CheckSafeRemotion(int refTick)
		{
			bool flag = this.bodiesToDestroy.ContainsKey(refTick);
			if (flag)
			{
				List<IBody> list = this.bodiesToDestroy[refTick];
				foreach (IBody current in list)
				{
					bool tSDisabled = current.TSDisabled;
					if (tSDisabled)
					{
						this.physicsManager.RemoveBody(current);
					}
				}
				this.bodiesToDestroy.Remove(refTick);
			}
			bool flag2 = this.delegatesToExecute.ContainsKey(refTick);
			if (flag2)
			{
				this.delegatesToExecute.Remove(refTick);
			}
		}

		private void DropLagPlayers()
		{
			List<TSPlayer> list = new List<TSPlayer>();
			int refTick = this.GetRefTick(this.GetSyncedDataTick());
			bool flag = refTick >= 0;
			if (flag)
			{
				foreach (TSPlayer current in this.players.Values)
				{
					bool flag2 = !current.IsDataReady(refTick);
					if (flag2)
					{
						current.dropCount++;
						this.CheckDrop(current);
						list.Add(current);
					}
				}
			}
			foreach (TSPlayer current2 in list)
			{
				SyncedData[] sendDataForDrop = current2.GetSendDataForDrop(this.localPlayer.ID, 1);
				bool flag3 = sendDataForDrop.Length != 0;
				if (flag3)
				{
					this.communicator.OpRaiseEvent(199, SyncedData.Encode(sendDataForDrop), true, null);
				}
			}
		}

		private SyncedData UpdateData()
		{
			bool flag = this.replayMode == ReplayMode.LOAD_REPLAY;
			SyncedData result;
			if (flag)
			{
				result = null;
			}
			else
			{
				SyncedData syncedData = new SyncedData(this.localPlayer.ID, this.ticks);
				this.GetLocalData(syncedData.inputData);
				this.localPlayer.AddData(syncedData);
				bool flag2 = this.communicator != null;
				if (flag2)
				{
					SyncedData[] sendData = this.localPlayer.GetSendData(this.ticks, 1);
					bool flag3 = sendData.Length != 0;
					if (flag3)
					{
						this.communicator.OpRaiseEvent(199, SyncedData.Encode(sendData), true, this.GetActivePlayers());
					}
				}
				result = syncedData;
			}
			return result;
		}

		internal InputData GetInputData(int playerId)
		{
			return this.players[(byte)playerId].GetData(this.GetSyncedDataTick()).inputData;
		}

		private void SendInfoChecksum(int tick)
		{
			bool flag = this.replayMode == ReplayMode.LOAD_REPLAY;
			if (!flag)
			{
				SyncedInfo syncedInfo = this.bufferSyncedInfo.Current();
				syncedInfo.playerId = this.localPlayer.ID;
				syncedInfo.tick = tick;
				syncedInfo.checksum = this.GetChecksumForSyncedInfo();
				this.bufferSyncedInfo.MoveNext();
				this.RaiseEvent(198, SyncedInfo.Encode(syncedInfo));
			}
		}

		private void RaiseEvent(byte eventCode, object message)
		{
			this.RaiseEvent(eventCode, message, true, null);
		}

		private void RaiseEvent(byte eventCode, object message, bool reliable, int[] toPlayers)
		{
			bool flag = this.communicator != null;
			if (flag)
			{
				this.communicator.OpRaiseEvent(eventCode, message, reliable, toPlayers);
			}
		}

		private void OnEventDataReceived(byte eventCode, object content)
		{
			bool flag = eventCode == 199;
			if (flag)
			{
				byte[] data = content as byte[];
				SyncedData[] array = SyncedData.Decode(data);
				bool flag2 = array.Length != 0;
				if (flag2)
				{
					TSPlayer tSPlayer = this.players[array[0].inputData.ownerID];
					bool flag3 = !tSPlayer.dropped;
					if (flag3)
					{
						this.OnSyncedDataReceived(tSPlayer, array);
						bool flag4 = array[0].dropPlayer && tSPlayer.ID != this.localPlayer.ID && !this.players[array[0].dropFromPlayerId].dropped;
						if (flag4)
						{
							tSPlayer.dropCount++;
						}
					}
				}
			}
			else
			{
				bool flag5 = eventCode == 198;
				if (flag5)
				{
					byte[] infoBytes = content as byte[];
					this.OnChecksumReceived(SyncedInfo.Decode(infoBytes));
				}
				else
				{
					bool flag6 = eventCode == 197;
					if (flag6)
					{
						byte[] array2 = content as byte[];
						bool flag7 = array2.Length != 0;
						if (flag7)
						{
							bool flag8 = array2[0] == 0;
							if (flag8)
							{
								this.Pause();
							}
							else
							{
								bool flag9 = array2[0] == 1;
								if (flag9)
								{
									this.Run();
								}
								else
								{
									bool flag10 = array2[0] == 3;
									if (flag10)
									{
										this.End();
									}
								}
							}
						}
					}
					else
					{
						bool flag11 = eventCode == 196;
						if (flag11)
						{
							byte[] infoBytes2 = content as byte[];
							SyncedInfo syncedInfo = SyncedInfo.Decode(infoBytes2);
							this.players[syncedInfo.playerId].sentSyncedStart = true;
						}
					}
				}
			}
		}

		private void OnChecksumReceived(SyncedInfo syncedInfo)
		{
			bool dropped = this.players[syncedInfo.playerId].dropped;
			if (!dropped)
			{
				this.checksumOk = true;
				SyncedInfo[] buffer = this.bufferSyncedInfo.buffer;
				for (int i = 0; i < buffer.Length; i++)
				{
					SyncedInfo syncedInfo2 = buffer[i];
					bool flag = syncedInfo2.tick == syncedInfo.tick && syncedInfo2.checksum != syncedInfo.checksum;
					if (flag)
					{
						this.checksumOk = false;
						break;
					}
				}
			}
		}

		protected SyncedData[] GetTickData(int tick)
		{
			List<SyncedData> list = new List<SyncedData>();
			foreach (TSPlayer current in this.players.Values)
			{
				bool flag = !current.dropped;
				if (flag)
				{
					list.Add(current.GetData(tick));
				}
			}
			return list.ToArray();
		}

		public void AddPlayer(byte playerId, string playerName, bool isLocal)
		{
			TSPlayer tSPlayer = new TSPlayer(playerId, playerName);
			this.players.Add(tSPlayer.ID, tSPlayer);
			if (isLocal)
			{
				this.localPlayer = tSPlayer;
				this.localPlayer.sentSyncedStart = true;
			}
			bool flag = this.replayMode == ReplayMode.RECORD_REPLAY;
			if (flag)
			{
				this.replayRecord.AddPlayer(tSPlayer);
			}
		}

		private void CheckDrop(TSPlayer p)
		{
			bool flag = p != this.localPlayer && !p.dropped && p.dropCount > 0;
			if (flag)
			{
				int num = -1;
				foreach (TSPlayer current in this.players.Values)
				{
					bool flag2 = !current.dropped;
					if (flag2)
					{
						num++;
					}
				}
				bool flag3 = p.dropCount >= num;
				if (flag3)
				{
					this.compoundStats.globalStats.GetInfo("panic").count = 0L;
					p.dropped = true;
					Debug.Log("Player dropped (stopped sending input)");
					int key = this.GetSyncedDataTick() + 1;
					bool flag4 = !this.delegatesToExecute.ContainsKey(key);
					if (flag4)
					{
						this.delegatesToExecute[key] = new List<Delegate>();
					}
					this.delegatesToExecute[key].Add(new Action(delegate
					{
						this.OnPlayerDisconnection(p.ID);
					}));
				}
			}
		}
	}
}
