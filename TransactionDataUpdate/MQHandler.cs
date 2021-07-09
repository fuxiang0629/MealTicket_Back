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

namespace TransactionDataUpdate
{
    public class MQHandler:MQTask
    {
        public MQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost)
            : base(_hostName, _port, _userName, _password, _virtualHost)
        {
        }

        public override void ReceivedExecute(string data)
        {
            DateTime timeNow = DateTime.Now;
            int Market = -1;
            string SharesCode = string.Empty;
            int Type = 0;
            try
            {
                //解析队列数据
                var dataJson = JsonConvert.DeserializeObject<dynamic>(data);
                Type = dataJson.Type == null ? 0 : dataJson.Type;
                Market = dataJson.Market;
                SharesCode = dataJson.SharesCode;
                DateTime Date = dataJson.Date == null ? timeNow.Date : DateTime.Parse(dataJson.Date.ToString());

                if (!DataHelper.CheckTradeDate(Date))
                {
                    return;
                }
                if (string.IsNullOrEmpty(SharesCode))
                {
                    return;
                }
                if (Date == timeNow.Date && timeNow < Date.AddHours(9).AddMinutes(25))
                {
                    return;
                }

                DateTime? lastTime;
                var list = DataHelper.TdxHq_GetTransactionData(Market, SharesCode, Date, Type, out lastTime);
                if (list.Count() > 0)
                {
                    lock (Singleton.Instance.TransactionDataLock)
                    {
                        DataHelper.UpdateTransactionData(list, Market, SharesCode, Date, lastTime);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("执行队列失败", ex);
            }
            finally
            {
                if (Market != -1 && Type == 0)
                {
                    var sendData = new
                    {
                        Market = Market,
                        SharesCode = SharesCode
                    };
                    SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Finish");
                }
            }
        }
    }
}
