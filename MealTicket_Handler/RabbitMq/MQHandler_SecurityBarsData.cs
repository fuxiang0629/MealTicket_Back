using FXCommon.Common;
using FXCommon.MqQueue;
using MealTicket_Handler.SecurityBarsData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class MQHandler_SecurityBarsData : MQTask
    {
        public MQHandler_SecurityBarsData(string _hostName, int _port, string _userName, string _password, string _virtualHost)
              : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            try
            {
                var resultData = JsonConvert.DeserializeObject<SecurityBarsDataTaskQueueInfo>(data);
                switch (resultData.DataType)
                {
                    case 2:
                        if (Singleton.Instance._securityBarsDataTask_1min != null)
                        {
                            Singleton.Instance._securityBarsDataTask_1min.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 3:
                        if (Singleton.Instance._securityBarsDataTask_5min != null)
                        {
                            Singleton.Instance._securityBarsDataTask_5min.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 4:
                        if (Singleton.Instance._securityBarsDataTask_15min != null)
                        {
                            Singleton.Instance._securityBarsDataTask_15min.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 5:
                        if (Singleton.Instance._securityBarsDataTask_30min != null)
                        {
                            Singleton.Instance._securityBarsDataTask_30min.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 6:
                        if (Singleton.Instance._securityBarsDataTask_60min != null)
                        {
                            Singleton.Instance._securityBarsDataTask_60min.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 7:
                        if (Singleton.Instance._securityBarsDataTask_1day != null)
                        {
                            Singleton.Instance._securityBarsDataTask_1day.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 8:
                        if (Singleton.Instance._securityBarsDataTask_1week != null)
                        {
                            Singleton.Instance._securityBarsDataTask_1week.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 9:
                        if (Singleton.Instance._securityBarsDataTask_1month != null)
                        {
                            Singleton.Instance._securityBarsDataTask_1month.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 10:
                        if (Singleton.Instance._securityBarsDataTask_1quarter != null)
                        {
                            Singleton.Instance._securityBarsDataTask_1quarter.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    case 11:
                        if (Singleton.Instance._securityBarsDataTask_1year != null)
                        {
                            Singleton.Instance._securityBarsDataTask_1year.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                            {
                                MsgId = 1,
                                MsgObj = resultData
                            });
                        }
                        break;
                    default:
                        break;
                }
                
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
