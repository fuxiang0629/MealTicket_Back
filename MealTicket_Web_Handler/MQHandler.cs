using FXCommon.Common;
using FXCommon.MqQueue;
using MealTicket_Web_Handler.Model;
using MealTicket_Web_Handler.Transactiondata;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using stock_db_core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using static StockTrendMonitor.Define.StockMonitorDefine;

namespace MealTicket_Web_Handler
{
    public class MQHandler:MQTask
    {
        public MQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost) 
            : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            try
            {
                var resultData=JsonConvert.DeserializeObject<TransactiondataTaskQueueInfo>(data);
                Singleton.Instance._transactionDataTask.TransactiondataQueue.AddMessage(new QueueMsgObj 
                {
                    MsgId=1,
                    MsgObj= resultData
                });
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错",ex);
            }
        }
    }
}
