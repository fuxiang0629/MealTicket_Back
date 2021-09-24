using FXCommon.Common;
using FXCommon.MqQueue;
using MealTicket_Web_Handler.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class SharesQuotesMQHandler : MQTask
    {
        ThreadMsgTemplate<int> TradeAutoBuyData;
        ThreadMsgTemplate<int> TradeAutoSellData;
        ThreadMsgTemplate<int> SharesAutoJoinData;

        public SharesQuotesMQHandler(string _hostName, int _port, string _userName, string _password, string _virtualHost) :
            base(_hostName, _port, _userName, _password, _virtualHost)
        {
            TradeAutoBuyData = new ThreadMsgTemplate<int>();
            TradeAutoBuyData.Init();
            TradeAutoSellData = new ThreadMsgTemplate<int>();
            TradeAutoSellData.Init();
            SharesAutoJoinData = new ThreadMsgTemplate<int>();
            SharesAutoJoinData.Init();
            Task.Factory.StartNew(() =>
            {
                do
                {
                    int msg = 0;
                    if (!TradeAutoBuyData.WaitMessage(ref msg))
                    {
                        continue;
                    }
                    if (msg == -1)
                    {
                        TradeAutoBuyData.Release();
                        break;
                    }
                    if (!RunnerHelper.CheckTradeTime2(null, false, true, false))
                    {
                        continue;
                    }
                    RunnerHelper.TradeAutoBuy();
                } while (true);
            });
            Task.Factory.StartNew(() =>
            {
                do
                {
                    int msg = 0;
                    if (!TradeAutoSellData.WaitMessage(ref msg))
                    {
                        continue;
                    }
                    if (msg == -1)
                    {
                        TradeAutoSellData.Release();
                        break;
                    }
                    if (!RunnerHelper.CheckTradeTime2(null, false, true, false))
                    {
                        continue;
                    }
                    RunnerHelper.TradeAuto();
                } while (true);
            });
            Task.Factory.StartNew(() =>
            {
                do
                {
                    int msg = 0;
                    if (!SharesAutoJoinData.WaitMessage(ref msg))
                    {
                        continue;
                    }
                    if (msg == -1)
                    {
                        SharesAutoJoinData.Release();
                        break;
                    }
                    if (!RunnerHelper.CheckTradeTime2(null, false, true, false))
                    {
                        continue;
                    }
                    RunnerHelper.SharesAutoJoin();
                } while (true);
            });
        }
        public override void ReceivedExecute(string data)
        {
            try
            {
                if (TradeAutoBuyData.GetCount() <= 0)
                {
                    TradeAutoBuyData.AddMessage(1);
                }
                if (TradeAutoSellData.GetCount() <= 0)
                {
                    TradeAutoSellData.AddMessage(1);
                }
                if (SharesAutoJoinData.GetCount() <= 0)
                {
                    SharesAutoJoinData.AddMessage(1);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("接受数据回调处理出错", ex);
            }
        }

        public override void Dispose()
        {
            if (TradeAutoBuyData != null)
            {
                TradeAutoBuyData.AddMessage(-1,true,0);
            }
            if (TradeAutoSellData != null)
            {
                TradeAutoSellData.AddMessage(-1, true, 0);
            }
            if (SharesAutoJoinData != null)
            {
                SharesAutoJoinData.AddMessage(-1, true, 0);
            }
            base.Dispose();
        }
    }
}
