using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadManager
{
    private static ThreadManager instance;
    private volatile List<Action<object>> tasks = new List<Action<object>>();
    private volatile List<object> parameters = new List<object>();
    private ManualResetEvent waitHandleA;
    private ManualResetEvent waitHandleB;
    private ManualResetEvent currentWaitHandle;
    private Thread[] threads;
    private int currentTaskIndex;
    private int waitingThreadCount;
    internal int threadCount;

    public int ThreadCount
    {
        private set { threadCount = value; }
        get { return threadCount; }
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

    private ThreadManager()
    {

    }
    // 下面这种写法感觉没有切换时间的概念呀，该进程内部不会发送切换时间片呀
    private void Initialize()
    {
        threadCount = Environment.ProcessorCount;
        threads = new Thread[threadCount];
        waitHandleA = new ManualResetEvent(false);
        waitHandleB = new ManualResetEvent(false);
        currentWaitHandle = waitHandleA;
        var initWaitHandle = new AutoResetEvent(false);
        for (var index = 1; index < threads.Length; index++)
        {
            threads[index] = new Thread(() =>
            {
                initWaitHandle.Set();
                ThreadProc();
            })
            {
                IsBackground = true
            };
            threads[index].Start();
            initWaitHandle.WaitOne();
        }
    }

    public void Execute()
    {
        currentTaskIndex = 0;
        waitingThreadCount = 0;
        currentWaitHandle.Set();
        PumpTasks();
        while (waitingThreadCount < threads.Length - 1)
        {
            Thread.Sleep(0);
        }
        currentWaitHandle.Reset();
        currentWaitHandle = currentWaitHandle == waitHandleA ? waitHandleB : waitHandleA;
        tasks.Clear();
        parameters.Clear();
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
            Interlocked.Increment(ref waitingThreadCount);
            waitHandleA.WaitOne();
            PumpTasks();
            Interlocked.Increment(ref waitingThreadCount);
            waitHandleB.WaitOne();
            PumpTasks();
        }
    }

    private void PumpTasks()
    {
        var count = tasks.Count;
        while (currentTaskIndex < count)
        {
            var curTaskIndex = currentTaskIndex;
            if (curTaskIndex == Interlocked.CompareExchange(ref currentTaskIndex, curTaskIndex + 1, curTaskIndex) && currentTaskIndex < count)
                tasks[currentTaskIndex](parameters[currentTaskIndex]);
        }
    }
}
