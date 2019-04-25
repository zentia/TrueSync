namespace TrueSync
{
    public interface ITrueSyncBehaviourGamePlay : ITrueSyncBehaviour
    {
        void OnPreSyncedUpdate();
        // 同步 玩家输入操作
        void OnSyncedInput();
        // 同步 读取玩家操作
        void OnSyncedUpdate();
    }
}