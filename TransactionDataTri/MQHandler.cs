using FXCommon.Common;
using FXCommon.MqQueue;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionDataTri
{
    public class MQHandler : MQTask
    {

        public MQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost)
            : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            try
            {
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(data);
                int Market = dataJson.Market;
                string SharesCode = dataJson.SharesCode;

                WaitQuery.Instance.updateWaitQuery(new WaitQueryModel
                {
                    SharesCode = SharesCode,
                    Market = Market
                }, 3);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行队列失败", ex);
            }
        }
    }
}
