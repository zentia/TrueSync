using System;
using System.Collections;

namespace TrueSync
{
	public class CoroutineScheduler
	{
		private CoroutineNode first = null;

		private FP currentTime;

		private AbstractLockstep lockStep;

		public CoroutineScheduler(AbstractLockstep lockStep)
		{
			this.lockStep = lockStep;
		}

		public CoroutineNode StartCoroutine(IEnumerator fiber)
		{
			bool flag = fiber == null;
			CoroutineNode result;
			if (flag)
			{
				result = null;
			}
			else
			{
				CoroutineNode coroutineNode = new CoroutineNode(fiber);
				this.AddCoroutine(coroutineNode);
				result = coroutineNode;
			}
			return result;
		}

		public void StopAllCoroutines()
		{
			this.first = null;
		}

		public bool HasCoroutines()
		{
			return this.first != null;
		}

		public void UpdateAllCoroutines()
		{
			InputData currentSimulationData = TrueSyncInput.CurrentSimulationData;
			this.UpdateAllCoroutines(this.lockStep.Ticks, this.lockStep.time);
			TrueSyncInput.CurrentSimulationData = currentSimulationData;
		}

		public void UpdateAllCoroutines(int frame, FP time)
		{
			this.currentTime = time;
			CoroutineNode listNext;
			for (CoroutineNode coroutineNode = this.first; coroutineNode != null; coroutineNode = listNext)
			{
				listNext = coroutineNode.listNext;
				bool flag = coroutineNode.waitForFrame > 0 && frame >= coroutineNode.waitForFrame;
				if (flag)
				{
					coroutineNode.waitForFrame = -1;
					this.UpdateCoroutine(coroutineNode);
				}
				else
				{
					bool flag2 = coroutineNode.waitForTime > 0f && time >= coroutineNode.waitForTime;
					if (flag2)
					{
						coroutineNode.waitForTime = -1f;
						this.UpdateCoroutine(coroutineNode);
					}
					else
					{
						bool flag3 = coroutineNode.waitForCoroutine != null && coroutineNode.waitForCoroutine.finished;
						if (flag3)
						{
							coroutineNode.waitForCoroutine = null;
							this.UpdateCoroutine(coroutineNode);
						}
						else
						{
							bool flag4 = coroutineNode.waitForFrame == -1 && coroutineNode.waitForTime == -1f && coroutineNode.waitForCoroutine == null;
							if (flag4)
							{
								this.UpdateCoroutine(coroutineNode);
							}
						}
					}
				}
			}
		}

		private void UpdateCoroutine(CoroutineNode coroutine)
		{
			IEnumerator fiber = coroutine.fiber;
			bool flag = coroutine.playerId > -1;
			if (flag)
			{
				TrueSyncInput.CurrentSimulationData = this.lockStep.GetInputData(coroutine.playerId);
			}
			bool flag2 = coroutine.fiber.MoveNext();
			if (flag2)
			{
				object obj = (fiber.Current == null) ? 1 : fiber.Current;
				bool flag3 = obj.GetType() == typeof(int);
				if (flag3)
				{
					coroutine.waitForTime = (int)obj;
					coroutine.waitForTime += this.currentTime;
				}
				else
				{
					bool flag4 = obj.GetType() == typeof(float);
					if (flag4)
					{
						coroutine.waitForTime = (float)obj;
						coroutine.waitForTime += this.currentTime;
					}
					else
					{
						bool flag5 = obj.GetType() == typeof(FP);
						if (flag5)
						{
							coroutine.waitForTime = (FP)obj;
							coroutine.waitForTime += this.currentTime;
						}
						else
						{
							bool flag6 = obj.GetType() == typeof(CoroutineNode);
							if (!flag6)
							{
								throw new ArgumentException("CoroutineScheduler: Unexpected coroutine yield type: " + obj.GetType());
							}
							coroutine.waitForCoroutine = (CoroutineNode)obj;
						}
					}
				}
			}
			else
			{
				coroutine.finished = true;
				this.RemoveCoroutine(coroutine);
			}
		}

		private void AddCoroutine(CoroutineNode coroutine)
		{
			bool flag = this.first != null;
			if (flag)
			{
				coroutine.listNext = this.first;
				this.first.listPrevious = coroutine;
			}
			this.first = coroutine;
		}

		private void RemoveCoroutine(CoroutineNode coroutine)
		{
			bool flag = this.first == coroutine;
			if (flag)
			{
				this.first = coroutine.listNext;
			}
			else
			{
				bool flag2 = coroutine.listNext != null;
				if (flag2)
				{
					coroutine.listPrevious.listNext = coroutine.listNext;
					coroutine.listNext.listPrevious = coroutine.listPrevious;
				}
				else
				{
					bool flag3 = coroutine.listPrevious != null;
					if (flag3)
					{
						coroutine.listPrevious.listNext = null;
					}
				}
			}
			coroutine.listPrevious = null;
			coroutine.listNext = null;
		}
	}
}
