namespace TrueSync
{
    using System;
    using System.Collections;

    public class CoroutineNode
    {
        public IEnumerator fiber;
        public bool finished = false;
        public CoroutineNode listNext = null;
        public CoroutineNode listPrevious = null;
        public int playerId = -1;
        public CoroutineNode waitForCoroutine;
        public int waitForFrame = -1;
        public FP waitForTime = -1f;

        public CoroutineNode(IEnumerator _fiber)
        {
            this.fiber = _fiber;
            if (TrueSyncInput.CurrentSimulationData > null)
            {
                this.playerId = TrueSyncInput.CurrentSimulationData.ownerID;
            }
        }
    }
}

