using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class TaskThread
    {
        const int DEFAULT_MIN_TASK_CNT = 32;
        const int DEFAULT_MAX_TASK_CNT = 64;

        struct TASK_CONTEXT
        {
            public ManualResetEvent _event;
            public object _obj;
            public WaitCallback _TaskCallback;
        }

        public static void SetTaskThreads(int iMinTaskCnt = DEFAULT_MIN_TASK_CNT, int iMaxTaskCnt = DEFAULT_MAX_TASK_CNT)
        {
            ThreadPool.SetMinThreads(iMinTaskCnt, iMinTaskCnt);
            ThreadPool.SetMaxThreads(iMaxTaskCnt, iMaxTaskCnt);
        }

        public static WaitHandle CreateTask(WaitCallback _TaskCallback, object cxt)
        {
            try
            {
                TASK_CONTEXT _context = new TASK_CONTEXT();
                _context._event = new ManualResetEvent(false);
                _context._obj = cxt;
                _context._TaskCallback = _TaskCallback;
                if (ThreadPool.QueueUserWorkItem(TaskCallbackProc, _context))
                {
                    return _context._event;
                }

                _context._event.Close();
            }
            catch (Exception)
            { }

            return null;
        }

        private static void TaskCallbackProc(object obj)
        {
            TASK_CONTEXT _context = (TASK_CONTEXT)obj;
            _context._TaskCallback(_context._obj);
            _context._event.Set();
        }

        public static bool WaitOne(WaitHandle _handle, int _timeout)
        {
            return _handle.WaitOne(_timeout);
        }

        public static bool WaitAll(WaitHandle[] handle_array, int _timeout)
        {
            return WaitHandle.WaitAll(handle_array, _timeout);
        }

        public static void CloseAllTasks(WaitHandle[] handle_array)
        {
            for (int idx = 0; idx < handle_array.Count(); idx++)
            {
                handle_array[idx].Close();
            }
        }
    }
}