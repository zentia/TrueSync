namespace TrueSync
{
    using System;
    using System.Collections;

    public class CoroutineScheduler
    {
        private FP currentTime;
        private CoroutineNode first = null;
        private AbstractLockstep lockStep;

        public CoroutineScheduler(AbstractLockstep lockStep)
        {
            this.lockStep = lockStep;
        }

        private void AddCoroutine(CoroutineNode coroutine)
        {
            if (this.first > null)
            {
                coroutine.listNext = this.first;
                this.first.listPrevious = coroutine;
            }
            this.first = coroutine;
        }

        public bool HasCoroutines()
        {
            return (this.first > null);
        }

        private void RemoveCoroutine(CoroutineNode coroutine)
        {
            if (this.first == coroutine)
            {
                this.first = coroutine.listNext;
            }
            else if (coroutine.listNext > null)
            {
                coroutine.listPrevious.listNext = coroutine.listNext;
                coroutine.listNext.listPrevious = coroutine.listPrevious;
            }
            else if (coroutine.listPrevious > null)
            {
                coroutine.listPrevious.listNext = null;
            }
            coroutine.listPrevious = null;
            coroutine.listNext = null;
        }

        public CoroutineNode StartCoroutine(IEnumerator fiber)
        {
            if (fiber == null)
            {
                return null;
            }
            CoroutineNode coroutine = new CoroutineNode(fiber);
            this.AddCoroutine(coroutine);
            return coroutine;
        }

        public void StopAllCoroutines()
        {
            this.first = null;
        }

        public void UpdateAllCoroutines()
        {
            InputData currentSimulationData = TrueSyncInput.CurrentSimulationData;
            this.UpdateAllCoroutines(this.lockStep.Ticks, this.lockStep.time);
            TrueSyncInput.CurrentSimulationData = currentSimulationData;
        }

        public void UpdateAllCoroutines(int frame, FP time)
        {
            CoroutineNode listNext;
            this.currentTime = time;
            for (CoroutineNode node = this.first; node > null; node = listNext)
            {
                listNext = node.listNext;
                if ((node.waitForFrame > 0) && (frame >= node.waitForFrame))
                {
                    node.waitForFrame = -1;
                    this.UpdateCoroutine(node);
                }
                else if ((node.waitForTime > 0f) && (time >= node.waitForTime))
                {
                    node.waitForTime = -1f;
                    this.UpdateCoroutine(node);
                }
                else if ((node.waitForCoroutine != null) && node.waitForCoroutine.finished)
                {
                    node.waitForCoroutine = null;
                    this.UpdateCoroutine(node);
                }
                else if (((node.waitForFrame == -1) && (node.waitForTime == -1f)) && (node.waitForCoroutine == null))
                {
                    this.UpdateCoroutine(node);
                }
            }
        }

        private void UpdateCoroutine(CoroutineNode coroutine)
        {
            IEnumerator fiber = coroutine.fiber;
            if (coroutine.playerId > -1)
            {
                TrueSyncInput.CurrentSimulationData = this.lockStep.GetInputData(coroutine.playerId);
            }
            if (coroutine.fiber.MoveNext())
            {
                object obj2 = (fiber.Current == null) ? 1 : fiber.Current;
                if (obj2.GetType() != typeof(int))
                {
                    if (obj2.GetType() != typeof(float))
                    {
                        if (obj2.GetType() != typeof(FP))
                        {
                            if (obj2.GetType() != typeof(CoroutineNode))
                            {
                                throw new ArgumentException("CoroutineScheduler: Unexpected coroutine yield type: " + obj2.GetType());
                            }
                            coroutine.waitForCoroutine = (CoroutineNode) obj2;
                        }
                        else
                        {
                            coroutine.waitForTime = (FP) obj2;
                            coroutine.waitForTime += this.currentTime;
                        }
                    }
                    else
                    {
                        coroutine.waitForTime = (float) obj2;
                        coroutine.waitForTime += this.currentTime;
                    }
                }
                else
                {
                    coroutine.waitForTime = (int) obj2;
                    coroutine.waitForTime += this.currentTime;
                }
            }
            else
            {
                coroutine.finished = true;
                this.RemoveCoroutine(coroutine);
            }
        }
    }
}

