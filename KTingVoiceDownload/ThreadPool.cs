using System;
using System.Collections.Generic;
using System.Threading;

namespace KTingVoiceDownload
{
    public class ThreadPool<T> : IDisposable
    {
        public delegate void ProcessDataDelegate(T data);

        private Queue<T> _dataList;
        private int _maxThreadCount = 100;
        private ThreadState _threadState = ThreadState.Unstarted;
        private int _threadCount = 0;

        public event ProcessDataDelegate OnProcessData;

        public ThreadPool()
        {
            _dataList = new Queue<T>();
        }

        public ThreadPool(IEnumerable<T> datas)
        {
            _dataList = new Queue<T>(datas);
        }

        public int MaxThreadCount
        {
            get
            {
                return _maxThreadCount;
            }
            set
            {
                _maxThreadCount = value;
            }
        }

        public ThreadState State
        {
            get
            {
                if (_threadState == ThreadState.Running
                    && _threadCount == 0)
                {
                    return ThreadState.WaitSleepJoin;
                }
                return _threadState;
            }
        }

        public void AddData(T data)
        {
            lock (_dataList)
            {
                _dataList.Enqueue(data);
            }
            if (_threadState == ThreadState.Running)
            {
                Interlocked.Increment(ref _threadCount);
                StartupProcess(null);
            }
        }

        public void AddData(List<T> data)
        {
            lock (_dataList)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    _dataList.Enqueue(data[i]);
                }
            }
            if (_threadState == ThreadState.Running)
            {
                Interlocked.Increment(ref _threadCount);
                StartupProcess(null);
            }
        }

        public void Start(bool isAsyn)
        {
            if (_threadState != ThreadState.Running)
            {
                _threadState = ThreadState.Running;
                if (isAsyn)
                {
                    _threadCount = 1;
                    ThreadPool.QueueUserWorkItem(StartupProcess);
                }
                else
                {
                    Interlocked.Increment(ref _threadCount);
                    StartupProcess(null);
                    while (_threadCount != 0)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }

        public void Stop()
        {
            if (_threadState != ThreadState.Stopped
                || _threadState != ThreadState.StopRequested)
            {
                _threadState = ThreadState.StopRequested;
                if (_threadCount > 0)
                {
                    while (_threadState != ThreadState.Stopped)
                    {
                        Thread.Sleep(500);
                    }
                }

                _threadState = ThreadState.Stopped;
            }
        }

        private void StartupProcess(object o)
        {
            if (_dataList.Count > 0)
            {
                Interlocked.Increment(ref _threadCount);
                ThreadPool.QueueUserWorkItem(ThreadProcess);
                while (_dataList.Count > 2)
                {
                    if (_threadCount >= _maxThreadCount)
                    {
                        break;
                    }
                    Interlocked.Increment(ref _threadCount);
                    ThreadPool.QueueUserWorkItem(ThreadProcess);
                }
            }
            Interlocked.Decrement(ref _threadCount);
        }

        private void ThreadProcess(object o)
        {
            T data;
            while (_threadState == ThreadState.Running)
            {
                lock (_dataList)
                {
                    if (_dataList.Count > 0)
                    {
                        data = _dataList.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
                OnProcessData(data);
            }
            Interlocked.Decrement(ref _threadCount);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
