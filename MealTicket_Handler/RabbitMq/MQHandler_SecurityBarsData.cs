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
                if (resultData.HandlerType == 3 || resultData.HandlerType == 4)
                {
                    foreach (var package in resultData.PackageList)
                    {
                        var sessionDataList = (from item in package.Value.DataList
                                               join item2 in Singleton.Instance._sharesBaseSession.GetSessionData() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                                               from ai in a.DefaultIfEmpty()
                                               select new SharesKlineData
                                               {
                                                   PlateId=item.PlateId,
                                                   WeightType=item.WeightType,
                                                   SharesCode = item.SharesCode,
                                                   LastTradeStock = item.LastTradeStock,
                                                   ClosedPrice = item.ClosedPrice,
                                                   GroupTimeKey = item.GroupTimeKey,
                                                   TradeStock = item.TradeStock,
                                                   LastTradeAmount = item.LastTradeAmount,
                                                   Market = item.Market,
                                                   MaxPrice = item.MaxPrice,
                                                   MinPrice = item.MinPrice,
                                                   OpenedPrice = item.OpenedPrice,
                                                   PreClosePrice = item.PreClosePrice,
                                                   Time = item.Time,
                                                   TotalCapital = ai == null ? 0 : ai.TotalCapital,
                                                   Tradable = ai == null ? 0 : ai.CirculatingCapital,
                                                   TradeAmount = item.TradeAmount,
                                                   YestodayClosedPrice = item.YestodayClosedPrice
                                               }).ToList();
                        Singleton.Instance._newIindexSecurityBarsDataTask.ToPushData(new SharesKlineDataContain
                        {
                            DataType = package.Value.DataType,
                            CalType=1,
                            SharesKlineData = sessionDataList
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }
    }
}
