using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FXCommon.Common
{
    public class ThreadMsgTemplate<T>
    {
        private int m_iMaxMsgCnt = -1;
        private AutoResetEvent m_event = null;
        private Semaphore m_semaphore = null;
        private List<T> m_msgQueue;

        #region public 初始化
        /// <summary>
        /// 初始化队列
        /// </summary>
        /// <returns></returns>
        public bool Init(int iMaxMsgCnt = -1)
        {
            try
            {
                m_event = new AutoResetEvent(true);
                m_semaphore = new Semaphore(0, 0x7FFFFFFF);
                m_msgQueue = new List<T>();
                m_iMaxMsgCnt = iMaxMsgCnt;
                return true;
            }
            catch
            {
                Release();
            }
            return false;
        }
        #endregion

        #region public 释放队列
        /// <summary>
        /// 释放队列
        /// </summary>
        public void Release()
        {
            if (m_msgQueue != null)
            {
                m_msgQueue.Clear();
                m_msgQueue = null;
            }

            if (m_event != null)
            {
                m_event.Close();
                m_event = null;
            }

            if (m_semaphore != null)
            {
                m_semaphore.Close();
                m_semaphore = null;
            }
        }
        #endregion

        public bool AddMessage(T obj, bool bWaitCnter = true,int index=-1)
        {
            if (!AddData(obj, index))
            {
                return false;
            }

            if (bWaitCnter)
            {
                m_semaphore.Release();
            }
            return true;
        }

        private bool AddData(T obj,int index= -1)
        {
            m_event.WaitOne();
            bool bResult = _AddData(obj, index);
            m_event.Set();

            return bResult;
        }

        private bool _AddData(T obj, int index = -1)
        {
            if (m_iMaxMsgCnt != -1 && m_msgQueue.Count() >= m_iMaxMsgCnt)
            {
                return false;
            }
            if (index == -1)
            {
                m_msgQueue.Add(obj);
            }
            else
            {
                m_msgQueue.Insert(index, obj);
            }
            return true;
        }

        public bool WaitMessage(ref T obj, int timeout = Timeout.Infinite)
        {
            do
            {
                if (!m_semaphore.WaitOne(timeout)) {
                    return false;
                }
            } while (!GetData(ref obj));

            return true;
        }

        private bool GetData(ref T obj)
        {
            m_event.WaitOne();
            bool bResult = _GetData(ref obj);
            m_event.Set();
            return bResult;
        }

        private bool _GetData(ref T obj)
        {
            if (m_msgQueue.Count() == 0)
            {
                return false;
            }
                
            obj = m_msgQueue[0];
            m_msgQueue.RemoveAt(0);
            return true;
        }

        public void ClearMessage()
        {
            m_event.WaitOne();
            _ClearMessage();
            m_event.Set();
        }

        private void _ClearMessage()
        {
            m_msgQueue.Clear();
        }

        public bool GetMessage(ref T obj, bool bRemoveIfExisting = false)
        {
            m_event.WaitOne();
            bool bResult = _GetMessage(ref obj, bRemoveIfExisting);
            m_event.Set();
            return bResult;
        }

        private bool _GetMessage(ref T obj, bool bRemoveIfExisting)
        {
            if (m_msgQueue.Count() == 0)
            {
                return false;
            }

            obj = m_msgQueue[0];
            if (bRemoveIfExisting)
            {
                m_msgQueue.RemoveAt(0);
            }
            return true;
        }

        public int GetCount() 
        {
            m_event.WaitOne();
            int count = m_msgQueue.Count();
            m_event.Set();
            return count;
        }

        public bool IsExists(T obj)
        {
            bool isExists=false;
            m_event.WaitOne();
            if (m_msgQueue.Contains(obj))
            {
                isExists = true;
            }
            m_event.Set();
            return isExists;
        }
    }
}
