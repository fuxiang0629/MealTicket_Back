using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FXCommon.Common.Utils;

namespace FXCommon.Common
{
    public abstract class Session_New
    {
        /// <summary>
        /// 最小时间间隔
        /// </summary>
        int MinInterval;

        /// <summary>
        /// 缓存执行类型
        /// </summary>
        List<Session_Time_Info> Session_Time_Info_List;

        public void SetTimerStatus(string dataKey, int status)
        {
            if (Session_Time_Info_List == null)
            {
                return;
            }

            foreach (var item in Session_Time_Info_List)
            {
                if (dataKey == item.DataKey || string.IsNullOrEmpty(dataKey))
                {
                    item.TimerStatus = status;
                }
            }
        }

        /// <summary>
        /// 系统参数更新线程等待队列
        /// </summary>
        ThreadMsgTemplate<Session_Time_Info_Msg> UpdateWait = new ThreadMsgTemplate<Session_Time_Info_Msg>();

        Timer UpdateTimer = null;
        Task[] taskArr = null;

        #region===缓存信息===
        /// <summary>
        /// 缓存信息
        /// </summary>
        Dictionary<string, object> SessionData = new Dictionary<string, object>();

        /// <summary>
        /// 缓存锁
        /// </summary>
        ReaderWriterLock _sessionReadWriteLock = new ReaderWriterLock();

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        public object GetSession(string key, object oct = null)
        {
            _sessionReadWriteLock.AcquireReaderLock(Timeout.Infinite);
            object value = _getSession(key);
            if (value != null)
            {
                value = CopySessionData(value, key);
            }
            _sessionReadWriteLock.ReleaseReaderLock();
            return value;
        }

        private object _getSession(string key)
        {
            if (!SessionData.ContainsKey(key))
            {
                return null;
            }
            return SessionData[key];
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetSession(string key, object value, bool isCopyValue = false)
        {
            if (value != null && isCopyValue)
            {
                value = CopySessionData(value, key);
            }

            _sessionReadWriteLock.AcquireWriterLock(Timeout.Infinite);
            _setSession(key, value);
            _sessionReadWriteLock.ReleaseWriterLock();
        }

        private void _setSession(string key, object value)
        {
            SessionData[key] = value;
        }

        public virtual object CopySessionData(object objData,string dataKey) 
        {
            return objData;
        }
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="minInterval"></param>
        /// <param name="session_Time_Info_List"></param>
        public void Init(int minInterval, List<Session_Time_Info> session_Time_Info_List)
        {
            MinInterval = minInterval;
            Session_Time_Info_List = session_Time_Info_List;
        }

        /// <summary>
        /// 启动更新
        /// </summary>
        public void StartUpdate()
        {
            UpdateWait.Init();

            SYSTEM_INFO systemInfo=new SYSTEM_INFO();
            Utils.GetSystemInfo(ref systemInfo);
            taskArr = new Task[systemInfo.dwNumberOfProcessors];
            for (int i = 0; i < systemInfo.dwNumberOfProcessors; i++)
            {
                taskArr[i]=Task.Factory.StartNew(()=> 
                {
                    do
                    {
                        Session_Time_Info_Msg time_info = new Session_Time_Info_Msg();
                        UpdateWait.WaitMessage(ref time_info);
                        if (time_info.Msg_Id == -1)
                        {
                            break;
                        }

                        if (time_info.Msg_Id == 1)
                        {
                            _doTask(time_info.Session_Time_Info);
                        }

                    } while (true);
                });
            }

            UpdateTimer = new Timer(DoTimeTask, null, MinInterval, MinInterval);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        private void DoTimeTask(object state)
        {
            DateTime timeNow = DateTime.Now;
            foreach (var item in Session_Time_Info_List)
            {
                if (item.TimerStatus == 1)
                {
                    continue;
                }

                if (item.NextExcuteTime == null)
                {
                    item.NextExcuteTime = timeNow;
                }
                if (timeNow >= item.NextExcuteTime)
                {
                    if (item.TimerType == 0)
                    {
                        item.NextExcuteTime = item.NextExcuteTime.Value.AddSeconds(item.ExcuteInterval);
                    }
                    else
                    {
                        item.TimerStatus = 1;
                    }

                    UpdateWait.AddMessage(new Session_Time_Info_Msg
                    {
                        Msg_Id = 1,
                        Session_Time_Info = item
                    });
                }
            }
        }

        private void _doTask(Session_Time_Info item)
        {
            _toUpdateSession(item.DataKey, item.ExcuteType);
            if (item.TimerType == 1)
            {
                item.NextExcuteTime = DateTime.Now.AddSeconds(item.ExcuteInterval);
                item.TimerStatus = 0;
            }
        }

        private void _toUpdateSession(string DataKey,int ExcuteType, object oct = null)
        {
            object data = UpdateSession(ExcuteType, oct);
            SetSession(DataKey, data);
        }

        /// <summary>
        /// 数据更新处理
        /// </summary>
        /// <param name="ExcuteType"></param>
        /// <returns></returns>
        public abstract object UpdateSession(int ExcuteType, object oct = null);

        /// <summary>
        /// 手动更新所有Session
        /// </summary>
        /// <returns></returns>
        public void UpdateSessionManual()
        {
            foreach (var item in Session_Time_Info_List)
            {
                if (item.TimerStatus == 1)
                {
                    continue;
                }
                _toUpdateSession(item.DataKey,item.ExcuteType);

                if (item.NextExcuteTime == null)
                {
                    item.NextExcuteTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 手动更新某个数据缓存
        /// </summary>
        /// <param name="dataKey"></param>
        public void UpdateSessionManual(string DataKey,int ExcuteType,object oct=null)
        {
            _toUpdateSession(DataKey, ExcuteType, oct);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (UpdateTimer != null)
            {
                UpdateTimer.Dispose();
                UpdateTimer = null;
            }
            if (taskArr != null)
            {
                for (int i = 0; i < taskArr.Count(); i++)
                {
                    UpdateWait.AddMessage(new Session_Time_Info_Msg 
                    {
                        Msg_Id=-1
                    });
                }
                Task.WaitAll(taskArr);
            }
            if (UpdateWait != null)
            {
                UpdateWait.Release();
            }
        }
    }
}
