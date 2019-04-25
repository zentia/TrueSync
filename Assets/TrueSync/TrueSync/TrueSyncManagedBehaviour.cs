namespace TrueSync
{
    public class TrueSyncManagedBehaviour : ITrueSyncBehaviourGamePlay, ITrueSyncBehaviour, ITrueSyncBehaviourCallbacks
    {
        [AddTracking]
        public bool disabled;
        public ITrueSyncBehaviour trueSyncBehavior;

        public TrueSyncManagedBehaviour(ITrueSyncBehaviour trueSyncBehavior)
        {
            StateTracker.AddTracking(this);
            StateTracker.AddTracking(trueSyncBehavior);
            this.trueSyncBehavior = trueSyncBehavior;
        }

        public void OnGameEnded()
        {
            if (trueSyncBehavior is ITrueSyncBehaviourCallbacks)
            {
                ((ITrueSyncBehaviourCallbacks)trueSyncBehavior).OnGameEnded();
            }
        }

        public void OnGamePaused()
        {
            if (trueSyncBehavior is ITrueSyncBehaviourCallbacks)
            {
                ((ITrueSyncBehaviourCallbacks)trueSyncBehavior).OnGamePaused();
            }
        }

        public void OnGameUnPaused()
        {
            if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
            {
                ((ITrueSyncBehaviourCallbacks) this.trueSyncBehavior).OnGameUnPaused();
            }
        }

        public void OnPlayerDisconnection(int playerId)
        {
            if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
            {
                ((ITrueSyncBehaviourCallbacks) this.trueSyncBehavior).OnPlayerDisconnection(playerId);
            }
        }

        public void OnPreSyncedUpdate()
        {
            if (this.trueSyncBehavior is ITrueSyncBehaviourGamePlay)
            {
                ((ITrueSyncBehaviourGamePlay) this.trueSyncBehavior).OnPreSyncedUpdate();
            }
        }

        public void OnSyncedInput()
        {
            if (this.trueSyncBehavior is ITrueSyncBehaviourGamePlay)
            {
                ((ITrueSyncBehaviourGamePlay) this.trueSyncBehavior).OnSyncedInput();
            }
        }

        public void OnSyncedStart()
        {
            if (this.trueSyncBehavior is ITrueSyncBehaviourCallbacks)
            {
                ((ITrueSyncBehaviourCallbacks) this.trueSyncBehavior).OnSyncedStart();
            }
        }

        public void OnSyncedUpdate()
        {
            if (this.trueSyncBehavior is ITrueSyncBehaviourGamePlay)
            {
                ((ITrueSyncBehaviourGamePlay) this.trueSyncBehavior).OnSyncedUpdate();
            }
        }

        public void SetGameInfo(TSPlayerInfo localOwner, int numberOfPlayers)
        {
            this.trueSyncBehavior.SetGameInfo(localOwner, numberOfPlayers);
        }
    }
}

