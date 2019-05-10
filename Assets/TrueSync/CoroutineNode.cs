using System;
using System.Collections;

namespace TrueSync
{
	public class CoroutineNode
	{
		public CoroutineNode listPrevious = null;

		public CoroutineNode listNext = null;

		public IEnumerator fiber;

		public bool finished = false;

		public int waitForFrame = -1;

		public FP waitForTime = -1f;

		public CoroutineNode waitForCoroutine;

		public int playerId = -1;

		public CoroutineNode(IEnumerator _fiber)
		{
			this.fiber = _fiber;
			bool flag = TrueSyncInput.CurrentSimulationData != null;
			if (flag)
			{
				this.playerId = (int)TrueSyncInput.CurrentSimulationData.ownerID;
			}
		}
	}
}
