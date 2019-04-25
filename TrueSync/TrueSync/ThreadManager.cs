namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ThreadManager
    {
        private int currentTaskIndex;
        private ManualResetEvent currentWaitHandle;
        private static ThreadManager instance = null;
        private volatile List<object> parameters = new List<object>();
        private volatile List<Action<object>> tasks = new List<Action<object>>();
        internal int threadCount;
        private Thread[] threads;
        public const int ThreadsPerProcessor = 1;
        private ManualResetEvent waitHandleA;
        private ManualResetEvent waitHandleB;
        private int waitingThreadCount;

        private ThreadManager()
        {
        }

        public void AddTask(Action<object> task, object param)
        {
            this.tasks.Add(task);
            this.parameters.Add(param);
        }

        public void Execute()
        {
            this.currentTaskIndex = 0;
            this.waitingThreadCount = 0;
            this.currentWaitHandle.Set();
            this.PumpTasks();
            while (this.waitingThreadCount < (this.threads.Length - 1))
            {
                Thread.Sleep(0);
            }
            this.currentWaitHandle.Reset();
            this.currentWaitHandle = (this.currentWaitHandle == this.waitHandleA) ? this.waitHandleB : this.waitHandleA;
            this.tasks.Clear();
            this.parameters.Clear();
        }

        private void Initialize()
        {
            ThreadStart <>9__0;
            this.threadCount = Environment.ProcessorCount;
            this.threads = new Thread[this.threadCount];
            this.waitHandleA = new ManualResetEvent(false);
            this.waitHandleB = new ManualResetEvent(false);
            this.currentWaitHandle = this.waitHandleA;
            AutoResetEvent initWaitHandle = new AutoResetEvent(false);
            for (int i = 1; i < this.threads.Length; i++)
            {
                this.threads[i] = new Thread(<>9__0 ?? (<>9__0 = delegate {
                    initWaitHandle.Set();
                    this.ThreadProc();
                }));
                this.threads[i].IsBackground = true;
                this.threads[i].Start();
                initWaitHandle.WaitOne();
            }
        }

        private void PumpTasks()
        {
            int count = this.tasks.Count;
            while (this.currentTaskIndex < count)
            {
                int currentTaskIndex = this.currentTaskIndex;
                if ((currentTaskIndex == Interlocked.CompareExchange(ref this.currentTaskIndex, currentTaskIndex + 1, currentTaskIndex)) && (currentTaskIndex < count))
                {
                    this.tasks[currentTaskIndex](this.parameters[currentTaskIndex]);
                }
            }
        }

        private void ThreadProc()
        {
            while (true)
            {
                Interlocked.Increment(ref this.waitingThreadCount);
                this.waitHandleA.WaitOne();
                this.PumpTasks();
                Interlocked.Increment(ref this.waitingThreadCount);
                this.waitHandleB.WaitOne();
                this.PumpTasks();
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
                return instance;
            }
        }

        public int ThreadCount
        {
            get
            {
                return this.threadCount;
            }
            private set
            {
                this.threadCount = value;
            }
        }
    }
}

