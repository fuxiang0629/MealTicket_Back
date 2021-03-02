using FXCommon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionDataTri
{
    public sealed class WaitQuery
    {
        object _lock = new object();

        int SendPeriodTime;//发送时间间隔(毫秒)

        int StopPeriodTime;//停止间隔时间（毫秒）

        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly WaitQuery instance = new WaitQuery();

        /// <summary>
        /// 系统参数更新线程
        /// </summary>
        Thread SysparUpdateThread;

        /// <summary>
        /// 系统参数更新线程等待队列
        /// </summary>
        private ThreadMsgTemplate<int> UpdateWait = new ThreadMsgTemplate<int>();

        public static WaitQuery Instance
        {
            get
            {
                return instance;
            }
        }

        public List<WaitQueryModel> WaitQueryList;//等待队列

        private WaitQuery()
        {
            WaitQueryList = new List<WaitQueryModel>();
            SendPeriodTime = 3000;
            StopPeriodTime = 15000;
            UpdateWait.Init();
            StartSysparUpdateThread();
        }

        /// <summary>
        /// 更新等待队列数据
        /// </summary>
        /// <param name="model">更新模型</param>
        /// <param name="from">来源1客户端 2提交发送完成 3.发送完成</param>
        public void updateWaitQuery(WaitQueryModel model,int from=1)
        {
            lock (_lock) 
            {
                if (WaitQueryList == null)
                {
                    return;
                }
                var temp = WaitQueryList.Where(e => e.Market == model.Market && e.SharesCode == model.SharesCode).FirstOrDefault();
                if (temp == null)
                {
                    var tempQuery = new WaitQueryModel
                    {
                        SharesCode = model.SharesCode,
                        Market = model.Market,
                        LastSendTime=null,
                        IsSend = 1,
                        TriTime = DateTime.Now
                    };
                    tempQuery.timer = new Timer(SendToUpdateQuery, tempQuery, 0, SendPeriodTime);
                    WaitQueryList.Add(tempQuery);

                }
                else if (from == 1)
                {
                    temp.TriTime = DateTime.Now;
                    if (temp.IsSend == 0)//激活发送状态
                    {
                        temp.IsSend = 1;
                    }
                }
                else if (from == 2)
                {
                    temp.IsSend = 2;
                    temp.LastSendTime = DateTime.Now;
                }
                else if (from == 3)
                {
                    if ((DateTime.Now - temp.TriTime).TotalMilliseconds > StopPeriodTime)
                    {
                        if (temp.timer != null)
                        {
                            temp.timer.Dispose();
                            temp.timer = null;
                        }
                        WaitQueryList.Remove(temp);
                    }
                    else
                    {
                        temp.IsSend = 1;
                    }
                }
            }
        }

        /// <summary>
        /// 发送到数据更新队列
        /// </summary>
        /// <param name="state"></param>
        private void SendToUpdateQuery(object state)
        {
            try
            {
                var data = state as WaitQueryModel;
                if (data.IsSend != 1)
                {
                    if (data.LastSendTime != null && (DateTime.Now - data.LastSendTime.Value).TotalSeconds > 60 && data.IsSend==2)
                    {
                        updateWaitQuery(new WaitQueryModel
                        {
                            SharesCode = data.SharesCode,
                            Market = data.Market
                        }, 3);
                    }
                    return;
                }
                var sendData = new
                {
                    Market = data.Market,
                    SharesCode = data.SharesCode
                };
                bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Update");
                if (isSendSuccess)
                {
                    updateWaitQuery(new WaitQueryModel 
                    {
                        SharesCode=data.SharesCode,
                        Market=data.Market
                    }, 2);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose() 
        {
            lock (_lock)
            {
                foreach (var item in WaitQueryList)
                {
                    if (item.timer!=null)
                    {
                        item.timer.Dispose();
                    }
                }
                WaitQueryList = null;
            }


            if (SysparUpdateThread != null)
            {
                UpdateWait.AddMessage(0);
                SysparUpdateThread.Join();
                UpdateWait.Release();
            }
        }

        /// <summary>
        /// 启动系统参数更新线程
        /// </summary>
        private void StartSysparUpdateThread()
        {
            SysparUpdateThread = new Thread(() =>
            {
                do
                {
                    try
                    {
                        using (var db = new meal_ticketEntities())
                        {
                            try
                            {
                                var sysPar1 = (from item in db.t_system_param
                                               where item.ParamName == "TransactiondataPar"
                                               select item).FirstOrDefault();
                                if (sysPar1 != null)
                                {
                                    var sysValue = JsonConvert.DeserializeObject<dynamic>(sysPar1.ParamValue);
                                    if (sysValue.SendPeriodTime != null && sysValue.SendPeriodTime >= 3000 && sysValue.SendPeriodTime <= 10000)
                                    {
                                        this.SendPeriodTime = sysValue.SendPeriodTime;
                                    }
                                    if (sysValue.StopPeriodTime != null && sysValue.StopPeriodTime >= 15000 && sysValue.StopPeriodTime <= 60000)
                                    {
                                        this.StopPeriodTime = sysValue.StopPeriodTime;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    { }

                    int msgId = 0;
                    if (UpdateWait.WaitMessage(ref msgId, 600000))
                    {
                        break;
                    }
                } while (true);
            });
            SysparUpdateThread.Start();
        }
    }
}
