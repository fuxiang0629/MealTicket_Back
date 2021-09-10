using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public abstract class Session<T>
    {
        protected ReaderWriterLock _readWriteLock = new ReaderWriterLock();

        protected T SessionData = default(T);

        public string Name = "";

        /// <summary>
        /// 系统参数更新线程
        /// </summary>
        Thread UpdateThread;

        /// <summary>
        /// 系统参数更新线程等待队列
        /// </summary>
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();

        /// <summary>
        /// 启动更新
        /// </summary>
        public void StartUpdate(TimeSpan startTime,TimeSpan endTime)
        {
            UpdateWait.Init();
            UpdateThread = new Thread(() =>
            {
                DateTime lastTime = DateTime.Now;
                do
                {
                    DateTime timeNow = DateTime.Now;
                    if (timeNow.Date > lastTime.Date)
                    {
                        TimeSpan timeSpanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                        if (timeSpanNow >= startTime && timeSpanNow < endTime)
                        {
                            bool IsSuccess=UpdateSessionWithLock();
                            if (IsSuccess)
                            {
                                lastTime = timeNow;
                            }
                        }
                    }
                    int msgId = 0;
                    if (UpdateWait.WaitMessage(ref msgId,600000))
                    {
                        break;
                    }
                } while (true);
            });
            UpdateThread.Start();
        }

        /// <summary>
        /// 启动更新
        /// </summary>
        public void StartUpdate(int sleepTime)
        {
            UpdateWait.Init();
            UpdateSessionWithLock();
            UpdateThread = new Thread(() =>
            {
                do
                {
                    int msgId = 0;
                    if (UpdateWait.WaitMessage(ref msgId, sleepTime))
                    {
                        break;
                    }
                    UpdateSessionWithLock();
                } while (true);
            });
            UpdateThread.Start();
        }
        
        private bool UpdateSessionWithLock()
        {
            _readWriteLock.AcquireWriterLock(-1);
            bool isSuccess = _UpdateSession();
            _readWriteLock.ReleaseWriterLock();
            return isSuccess;
        }

        private bool _UpdateSession()
        {
            try
            {
                SessionData = UpdateSession();
                return true;
            }
            catch (Exception ex) 
            {
                Logger.WriteFileLog(Name+"缓存加载失败",ex);
                return false;
            }
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <returns></returns>
        public T GetSessionData(int timeout=Timeout.Infinite)
        {
            _readWriteLock.AcquireReaderLock(timeout);
            T tempSessionData = _GetSessionData();
            _readWriteLock.ReleaseReaderLock();
            return tempSessionData;
        }

        private T _GetSessionData() 
        {
            return SessionData;
        }


        public abstract T UpdateSession();

        /// <summary>
        /// 手动更新Session
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateSessionManual() 
        {
            return UpdateSessionWithLock();
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (UpdateThread != null)
            {
                UpdateWait.AddMessage(0);
                UpdateThread.Join();
                UpdateWait.Release();
            }
        }
    }
}
