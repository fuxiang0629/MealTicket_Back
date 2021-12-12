using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class ThreadTask
    {
        /// <summary>
        /// 线程数组
        /// </summary>
        Thread[] thdArr;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="threadCount">开启线程数量</param>
        public ThreadTask(int threadCount)
        {
            parQueue.Init();

            threadCount = threadCount <= 0 ? 1 : threadCount;
            thdArr = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                thdArr[i] = new Thread(DoTask);
                thdArr[i].Start();
            }
        }

        /// <summary>
        /// 任务数据队列
        /// </summary>
        private ThreadMsgTemplate<TaskDataInfo> parQueue = new ThreadMsgTemplate<TaskDataInfo>();

        /// <summary>
        /// 添加到队列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddParQueue(TaskDataInfo data) 
        {
            return parQueue.AddMessage(data);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        private void DoTask()
        {
            int hqClient = -1;
            StringBuilder sResult = new StringBuilder(1024 * 800);
            StringBuilder sErrInfo = new StringBuilder(512);
            do
            {
                TaskDataInfo tempData = new TaskDataInfo();
                parQueue.WaitMessage(ref tempData);

                if (tempData == null)
                {
                    break;
                }

                if (hqClient == -1)
                {
                    hqClient = Singleton.Instance.GetHqClient();
                }

                if (!_doTask(hqClient, sResult, sErrInfo, tempData))
                {
                    Singleton.Instance.AddRetryClient(hqClient);
                    hqClient = -1;
                }

                if (tempData.semap.Release() + 1 >= tempData.TotalCount)
                {
                    tempData.WatiHandle.Set();
                }
            } while (true);
        }

        private bool _doTask(int hqClient,StringBuilder sResult, StringBuilder sErrInfo, TaskDataInfo tempData) 
        {
            try
            {
                sResult.Clear();
                sErrInfo.Clear();

                int download_second = 0;
                bool isReconnectClient = false;
                var tempList = DataHelper.TdxHq_GetSecurityBarsData_byShares(hqClient, tempData.HandlerType, tempData.Data, ref isReconnectClient, ref sResult, ref sErrInfo,ref download_second);

                lock (tempData.DownLoadLock)
                {
                    int ThreadId = Thread.CurrentThread.ManagedThreadId;
                    if (tempData.downCounter.ContainsKey(ThreadId))
                    {
                        tempData.downCounter[ThreadId] += download_second;
                    }
                    else
                    {
                        tempData.downCounter.Add(ThreadId, download_second);
                    }
                }

                if (isReconnectClient)
                {
                    lock (tempData.FailLock)
                    {
                        tempData.FailPackage[tempData.Data.DataType].DataList.Add(tempData.Data);//加入失败列表
                    }
                    return false;
                }

                lock (tempData.SuccessLock)
                {
                    if (tempData.ResultList.ContainsKey(tempData.Data.DataType))
                    {
                        tempData.ResultList[tempData.Data.DataType].DataList.AddRange(tempList);
                    }
                    else
                    {
                        tempData.ResultList.Add(tempData.Data.DataType, new SecurityBarsDataRes
                        {
                            DataType = tempData.Data.DataType,
                            DataList = tempList
                        });
                    }
                    tempData.SuccessPackage[tempData.Data.DataType].DataList.Add(tempData.Data);//加入成功列表
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("获取K线数据出错",ex);
                return false;
            }
        }

        /// <summary>
        /// 资源回收
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < thdArr.Count(); i++)
            {
                parQueue.AddMessage(null);
            }
            for (int i = 0; i < thdArr.Count(); i++)
            {
                thdArr[i].Join();
            }
            parQueue.Release();
        }
    }
}
