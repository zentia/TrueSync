namespace TrueSync
{
    using System;

    public interface ITrueSyncBehaviour
    {
        void SetGameInfo(TSPlayerInfo localOwner, int numberOfPlayers);
    }
}

