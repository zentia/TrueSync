public interface ICommunicator
{
    // 添加监听
    void AddEventListener(OnEventReceived onEventReceived);
    // 操作事件
    void OpRaiseEvent(byte eventCode, object message, bool reliable, int[] toPlayers);
    // 往返时间
    int RoundTripTime();
}