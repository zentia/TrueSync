using System;
using System.Collections.Generic;
using System.Threading;

namespace TrueSync
{
    public class ThreadManager
    {
        private static ThreadManager instance;
        private volatile List<Action<object>> tasks = new List<Action<object>>();
        private volatile List<object> parameters = new List<object>();
        public const int ThreadsPerProcessor = 1;
        private ManualResetEvent waitHandleA;
        private ManualResetEvent waitHandleB;
        private ManualResetEvent currentWaitHandle;
        private Thread[] threads;
        private int currentTaskIndex;
        private int waitingThreadCount;
        internal int threadCount;

        public int ThreadCount
        {
            private set
            {
                threadCount = value;
            }
            get
            {
                return threadCount;
            }
        }

        public static ThreadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ThreadManager();
                    instance.Initialize();
                }
                return ThreadManager.instance;
            }
        }

        private ThreadManager()
        {
        }

        private void Initialize()
        {
            threadCount = Environment.ProcessorCount;
            this.threads = new Thread[this.threadCount];
            this.waitHandleA = new ManualResetEvent(false);
            this.waitHandleB = new ManualResetEvent(false);
            this.currentWaitHandle = this.waitHandleA;
            AutoResetEvent initWaitHandle = new AutoResetEvent(false);
            for (int index = 1; index < this.threads.Length; ++index)
            {
                threads[index] = new Thread((() =>
                {
                    initWaitHandle.Set();
                    this.ThreadProc();
                }));
                this.threads[index].IsBackground = true;
                this.threads[index].Start();
                initWaitHandle.WaitOne();
            }
        }

        public void Execute()
        {
            this.currentTaskIndex = 0;
            this.waitingThreadCount = 0;
            this.currentWaitHandle.Set();
            this.PumpTasks();
            while (this.waitingThreadCount < this.threads.Length - 1)
                Thread.Sleep(0);
            currentWaitHandle.Reset();
            this.currentWaitHandle = this.currentWaitHandle == this.waitHandleA ? this.waitHandleB : this.waitHandleA;
            this.tasks.Clear();
            this.parameters.Clear();
        }

        public void AddTask(Action<object> task, object param)
        {
            tasks.Add(task);
            parameters.Add(param);
        }

        private void ThreadProc()
        {
            while (true)
            {
                Interlocked.Increment(ref this.waitingThreadCount);
                waitHandleA.WaitOne();
                this.PumpTasks();
                Interlocked.Increment(ref waitingThreadCount);
                this.waitHandleB.WaitOne();
                PumpTasks();
            }
        }

        private void PumpTasks()
        {
            int count = tasks.Count;
            while (this.currentTaskIndex < count)
            {
                int currentTaskIndex = this.currentTaskIndex;
                if (currentTaskIndex == Interlocked.CompareExchange(ref this.currentTaskIndex, currentTaskIndex + 1, currentTaskIndex) && currentTaskIndex < count)
                    tasks[currentTaskIndex](parameters[currentTaskIndex]);
            }
        }
    }
}
