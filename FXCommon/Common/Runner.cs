using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FXCommon.Common
{

    /// <summary>
    /// 循环任务
    /// </summary>
    public abstract class Runner
    {
        #region ====================抽象类====================
        /// <summary>
        /// 休眠时间
        /// </summary>
        public int SleepTime { get; set; }

        /// <summary>
        /// 任务名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 验证是否运行
        /// </summary>
        public abstract bool Check { get; }

        /// <summary>
        /// 当前计时器
        /// </summary>
        Timer timer = null;

        /// <summary>
        /// 释放资源，虚方法。
        /// </summary>
        public virtual void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        /// <summary>
        /// 运行 
        /// </summary>
        public void Run()
        {
            timer = new Timer(TimerCallBack, null, 0, Timeout.Infinite);
        }

        private void TimerCallBack(object state)
        {
            try
            {
                if (Check)
                {
                    Execute();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog(Name + "错误。", ex);
            }

            if (timer != null)
            {
                timer.Change(SleepTime, Timeout.Infinite);//重启计时器
            }
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        public abstract void Execute();
        #endregion
    }
}
