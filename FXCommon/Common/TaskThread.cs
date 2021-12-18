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
        protected System.Threading.Thread m_thread = null;

        public bool CreateTask(ParameterizedThreadStart TaskThreadProc)
        {
            if (m_thread != null)
            {
                return true;
            }

            try
            {
                m_thread = new Thread(TaskThreadProc);
                m_thread.Start();
                return true;
            }
            catch (Exception)
            {
                m_thread = null; ;
            }

            return false;
        }

        public bool CreateTask(ParameterizedThreadStart TaskThreadProc, object cxt)
        {
            if (m_thread != null)
            {
                return true;
            }

            try
            {
                m_thread = new Thread(TaskThreadProc);
                m_thread.Start(cxt);
                return true;
            }
            catch (Exception)
            {
                m_thread = null; ;
            }

            return false;
        }

        public static void WaitAll(ref TaskThread[] task_array)
        {
            int taskCount = task_array.Count();
            for (int idx = 0; idx < taskCount; idx++)
            {
                if (task_array[idx].m_thread == null)
                {
                    continue;
                }

                task_array[idx].m_thread.Join();
                task_array[idx].m_thread = null;
            }
        }
    }
}
