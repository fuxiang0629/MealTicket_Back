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
        public class GET_DATA_CXT
        {
            public GET_DATA_CXT(int _cmd_id, object _cxt)
            {
                CmdId = _cmd_id;
                cmd_cxt = _cxt;
            }
            public int CmdId;
            public object cmd_cxt;
        }

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
        Thread[] threadArr = null;

        #region===缓存信息===
        /// <summary>
        /// 缓存信息
        /// </summary>
        Dictionary<string, object> SessionData = new Dictionary<string, object>();

        /// <summary>
        /// 缓存锁
        /// </summary>
        ReaderWriterLockSlim _sessionReadWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public object GetDataWithLock(string DataKey, GET_DATA_CXT _cxt=null, bool isUpgradeable = false)
        {
            
            if (isUpgradeable)
            {
                _sessionReadWriteLock.EnterUpgradeableReadLock();
            }
            else
            {
                _sessionReadWriteLock.EnterReadLock();
            }

            try
            {
                return OnGetData(DataKey, _cxt);
            }
            catch(Exception)
            { }
            finally
            {
                if (isUpgradeable)
                {
                    _sessionReadWriteLock.ExitUpgradeableReadLock();
                }
                else
                {
                    _sessionReadWriteLock.ExitReadLock();
                }
            }
            return null;
        }

        public object GetDataWithNoLock(string key, bool bCopyData = false)
        {
            return (bCopyData ? OnGetData(key, null) : _getSession(key));
        }

        public virtual object OnGetData(string DataKey, GET_DATA_CXT _cxt)
        {
            object value = _getSession(DataKey);
            if (value != null)
            {
                value = CopySessionData(value, DataKey);
            }
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
        public void SetSessionWithLock(string key, object value)
        {
            _sessionReadWriteLock.EnterWriteLock();
            try
            {
                _setSession(key, value);

                OnSessionAfterWriting(key);
            }
            catch(Exception)
            { }
            finally
            {
                _sessionReadWriteLock.ExitWriteLock();
            }
        }

        protected virtual void OnSessionAfterWriting(string key)
        { }

        public void SetSessionWithNolock(string key, object value)
        {
            _setSession(key, value);
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
            threadArr = new Thread[systemInfo.dwNumberOfProcessors];
            for (int i = 0; i < systemInfo.dwNumberOfProcessors; i++)
            {
                threadArr[i]=new Thread(()=> 
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
                threadArr[i].Start();
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
            SetSessionWithLock(DataKey, data);
        }

        private void _toUpdateSession2(string DataKey, int ExcuteType, object oct = null)
        {
            object data = UpdateSession(ExcuteType, oct);
            _setSession(DataKey, data);
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
            try
            {
                _toUpdateSession(DataKey, ExcuteType, oct);
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// 手动更新某个数据缓存
        /// </summary>
        /// <param name="dataKey"></param>
        public void UpdateSessionManual2(string DataKey, int ExcuteType, object oct = null)
        {
            try
            {
                _toUpdateSession2(DataKey, ExcuteType, oct);
            }
            catch (Exception ex)
            { }
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
            if (threadArr != null)
            {
                for (int i = 0; i < threadArr.Count(); i++)
                {
                    UpdateWait.AddMessage(new Session_Time_Info_Msg 
                    {
                        Msg_Id=-1
                    });
                }
                for (int i = 0; i < threadArr.Count(); i++)
                {
                    threadArr[i].Join();
                }
                threadArr = null;
            }
            if (UpdateWait != null)
            {
                UpdateWait.Release();
            }
        }
    }
}
