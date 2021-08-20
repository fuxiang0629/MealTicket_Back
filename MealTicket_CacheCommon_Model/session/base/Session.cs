using FXCommon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_CacheCommon_Session
{
    public abstract class Session<T>
    {
        protected string username = "";
        protected string dataKey = "";
        protected string Name = "";

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
        public void StartUpdate(int sleepTime)
        {
            UpdateWait.Init();
            UpdateThread = new Thread(() =>
            {
                do
                {
                    _UpdateSession();
                    int msgId = 0;
                    if (UpdateWait.WaitMessage(ref msgId, sleepTime))
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
        public void StartUpdate(TimeSpan startTime,TimeSpan endTime)
        {
            UpdateWait.Init();
            UpdateThread = new Thread(() =>
            {
                try
                {
                    _UpdateSession();
                    DateTime lastDoDate = DateTime.Now.Date;
                    do
                    {
                        int msgId = 0;
                        if (UpdateWait.WaitMessage(ref msgId, 600000))
                        {
                            break;
                        }
                        DateTime timeNow = DateTime.Now;
                        if (timeNow.Date <= lastDoDate)
                        {
                            continue;
                        }
                        TimeSpan timeSpanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                        if (timeSpanNow < startTime || timeSpanNow >= endTime)
                        {
                            continue;
                        }
                        _UpdateSession();
                        lastDoDate = timeNow.Date;
                    } while (true);
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog(Name+"缓存线程启动失败",ex);
                }
            });
            UpdateThread.Start();
        }

        private bool _UpdateSession()
        {
            try
            {
                var SessionData = UpdateSession();
                Singleton.Instance.setCacheData(username, dataKey,JsonConvert.SerializeObject(SessionData));
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
        public T GetSessionData()
        {
            string result = Singleton.Instance.getCacheData(username, dataKey);
            if (string.IsNullOrEmpty(result))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(result);
        }


        public abstract T UpdateSession();

        /// <summary>
        /// 手动更新Session
        /// </summary>
        /// <returns></returns>
        public virtual bool UpdateSessionManual() 
        {
            return _UpdateSession();
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
