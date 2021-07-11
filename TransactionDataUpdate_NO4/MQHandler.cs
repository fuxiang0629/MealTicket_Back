using FXCommon.Common;
using FXCommon.MqQueue;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionDataUpdate_NO4
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
                var resultData = JsonConvert.DeserializeObject<TransactiondataTaskQueueInfo>(data);
                List<SharesInfo> sharesList = new List<SharesInfo>();
                foreach (var item in resultData.DataList)
                {
                    string sharesCode = item.SharesCode;
                    int market = item.Market;
                    sharesList.Add(new SharesInfo
                    {
                        SharesCode = sharesCode,
                        Market = market,
                        SharesInfoNum = int.Parse(sharesCode) * 10 + market
                    });
                }
                List<TransactiondataAnalyseInfo> list = new List<TransactiondataAnalyseInfo>();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("==========开始获取分笔数据======" + resultData.TaskGuid + "===");
                list = DataHelper.TdxHq_GetTransactionData(sharesList);
                stopwatch.Stop();
                Console.WriteLine("=====获取分笔数据结束:" + stopwatch.ElapsedMilliseconds + "============");
                Console.WriteLine("");
                Console.WriteLine("");

                SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new TransactiondataTaskQueueInfo
                {
                    DataList = list,
                    TaskGuid = resultData.TaskGuid,
                    DataType = resultData.DataType

                })), "TransactionData", "TrendAnalyse");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
