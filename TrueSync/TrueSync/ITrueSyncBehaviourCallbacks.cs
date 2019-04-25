namespace TrueSync
{
    using System;

    public interface ITrueSyncBehaviourCallbacks : ITrueSyncBehaviour
    {
        void OnGameEnded();
        void OnGamePaused();
        void OnGameUnPaused();
        void OnPlayerDisconnection(int playerId);
        void OnSyncedStart();
    }
}

