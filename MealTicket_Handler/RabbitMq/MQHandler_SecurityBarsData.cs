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
                if (resultData.HandlerType == 1 || resultData.HandlerType == 2)
                {
                    Singleton.Instance._securityBarsDataTask.SecurityBarsDataQueue.AddMessage(new QueueMsgObj
                    {
                        MsgId = 1,
                        MsgObj = resultData
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
