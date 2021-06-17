using FXCommon.Common;
using MealTicket_Handler.Model;
using MealTicket_Handler.RunnerHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace MealTicket_Handler
{
    public class TradeHandler
    {
        /// <summary>
        /// 根据股票代码获取股票列表
        /// </summary>
        /// <returns></returns>
        public GetTradeSharesListRes GetTradeSharesList(GetTradeSharesListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.SharesCode == null)
                {
                    request.SharesCode = "";
                }
                request.SharesCode = request.SharesCode.ToLower();
                var tempList = from item in db.t_shares_all
                               where (item.SharesCode.Contains(request.SharesCode) || item.SharesPyjc.Contains(request.SharesCode) || item.SharesName.Contains(request.SharesCode)) && item.Status == 1
                               select item;
                Regex regex0 = new Regex(Singleton.Instance.SharesCodeMatch0);
                Regex regex1 = new Regex(Singleton.Instance.SharesCodeMatch1);
                var sharesList = (from item in tempList.AsEnumerable()
                                  where ((regex0.IsMatch(item.SharesCode) && item.Market == 0) || (regex1.IsMatch(item.SharesCode) && item.Market == 1))
                                  select new TradeSharesInfo
                                  {
                                      SharesCode = item.SharesCode,
                                      Market = item.Market,
                                      SharesName = item.SharesName
                                  }).Take(10).ToList();

                foreach (var item in sharesList)
                {
                    var quotes = (from x in db.v_shares_quotes_last
                                  where x.Market == item.Market && x.SharesCode == item.SharesCode
                                  select x).FirstOrDefault();
                    if (quotes != null)
                    {
                        item.PresentPrice = quotes.PresentPrice <= 0 ? quotes.ClosedPrice : quotes.PresentPrice;
                        item.Rise = quotes.ClosedPrice <= 0 ? "0" : string.Format(" {0:N2} ", Math.Round((item.PresentPrice - quotes.ClosedPrice) * 100.0 / quotes.ClosedPrice, 2));
                    }
                }
                return new GetTradeSharesListRes
                {
                    Context = request.Context,
                    List = sharesList
                };
            }
        }

        /// <summary>
        /// 添加热门搜索结果
        /// </summary>
        /// <param name="request"></param>
        public void AddTradeSharesHotSearch(AddTradeSharesHotSearchRequest request, HeadBase basedata)
        {
            try
            {
                AuthorityHandler _authorityHandler = new AuthorityHandler();
                _authorityHandler.CheckAccountLogin(basedata.UserToken, basedata);
            }
            catch (Exception ex)
            {

            }
            using (var db = new meal_ticketEntities())
            {
                var sharesInfo = (from item in db.t_shares_all
                                  where item.SharesCode == request.SharesCode && item.Market == request.Market
                                  select item).FirstOrDefault();
                if (sharesInfo == null)
                {
                    return;
                }

                try
                {
                    db.t_shares_search.Add(new t_shares_search
                    {
                        SharesCode = request.SharesCode,
                        SharesName = sharesInfo.SharesName,
                        AccountId = basedata.AccountId,
                        CreateTime = DateTime.Now,
                        Market = request.Market
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                { }
            }
        }

        /// <summary>
        /// 查询热门搜索结果
        /// </summary>
        /// <returns></returns>
        public List<TradeSharesInfo> QueryTradeSharesHotSearch()
        {
            using (var db = new meal_ticketEntities())
            {
                var searchTemp = from item in db.t_shares_search
                                 group item by new { item.Market, item.SharesCode, item.SharesName } into g
                                 select g;
                int totalCount = searchTemp.Count();
                var search = (from item in searchTemp
                              let SearchCount = item.Count()
                              orderby SearchCount descending
                              select new TradeSharesInfo
                              {
                                  SharesCode = item.Key.SharesCode,
                                  SharesName = item.Key.SharesName,
                                  Market = item.Key.Market
                              }).Take(20).ToList();
                foreach (var item in search)
                {
                    var quotes = (from x in db.v_shares_quotes_last
                                  where x.Market == item.Market && x.SharesCode == item.SharesCode
                                  select x).FirstOrDefault();
                    if (quotes != null)
                    {
                        item.PresentPrice = quotes.PresentPrice <= 0 ? quotes.ClosedPrice : quotes.PresentPrice;
                        item.Rise = quotes.ClosedPrice <= 0 ? "0" : string.Format(" {0:N2} ", Math.Round((item.PresentPrice - quotes.ClosedPrice) * 100.0 / quotes.ClosedPrice, 2));
                    }
                }
                return search;
            }
        }

        /// <summary>
        /// 获取某只股票五档数据
        /// </summary>
        /// <returns></returns>
        public TradeSharesQuotesInfo GetTradeSharesQuotesInfo(GetTradeSharesQuotesInfoRequest request, HeadBase basedata)
        {
            try
            {
                AuthorityHandler _authorityHandler = new AuthorityHandler();
                _authorityHandler.CheckAccountLogin(basedata.UserToken, basedata);
            }
            catch (Exception ex)
            {

            }
            DateTime timeNow = DateTime.Now;
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var sharesQuotes = (from item in db.v_shares_quotes_last
                                    join item2 in db.t_shares_all on new { item.SharesCode, item.Market } equals new { item2.SharesCode, item2.Market }
                                    where item.Market == request.Market && item.SharesCode == request.SharesCode
                                    select new TradeSharesQuotesInfo
                                    {
                                        SharesCode = item.SharesCode,
                                        SellCount1 = item.SellCount1,
                                        SellCount2 = item.SellCount2,
                                        SellCount3 = item.SellCount3,
                                        SellCount4 = item.SellCount4,
                                        SellCount5 = item.SellCount5,
                                        SellPrice1 = item.SellPrice1,
                                        SellPrice2 = item.SellPrice2,
                                        SellPrice3 = item.SellPrice3,
                                        SellPrice4 = item.SellPrice4,
                                        SellPrice5 = item.SellPrice5,
                                        SharesName = item2.SharesName,
                                        SpeedUp = item.SpeedUp,
                                        Activity = item.Activity,
                                        BuyCount1 = item.BuyCount1,
                                        BuyCount2 = item.BuyCount2,
                                        BuyCount3 = item.BuyCount3,
                                        BuyCount4 = item.BuyCount4,
                                        BuyCount5 = item.BuyCount5,
                                        BuyPrice1 = item.BuyPrice1,
                                        BuyPrice2 = item.BuyPrice2,
                                        BuyPrice3 = item.BuyPrice3,
                                        BuyPrice4 = item.BuyPrice4,
                                        BuyPrice5 = item.BuyPrice5,
                                        ClosedPrice = item.ClosedPrice,
                                        Market = item.Market,
                                        MaxPrice = item.MaxPrice,
                                        InvolCount = item.InvolCount,
                                        MinPrice = item.MinPrice,
                                        OpenedPrice = item.OpenedPrice,
                                        OuterCount = item.OuterCount,
                                        PresentCount = item.PresentCount,
                                        PresentPrice = item.PresentPrice,
                                        TotalAmount = item.TotalAmount,
                                        TotalCount = item.TotalCount,
                                        HandCount = item2.SharesHandCount
                                    }).FirstOrDefault();
                if (sharesQuotes == null)
                {
                    return null;
                }

                //计算持仓数量
                var accountHold = (from item in db.t_account_shares_hold
                                   where item.AccountId == basedata.AccountId && item.Market == request.Market && item.SharesCode == request.SharesCode && item.Status == 1 && (item.RemainCount > 0 || item.LastModified > dateNow)
                                   select item).FirstOrDefault();
                if (accountHold == null)
                {
                    sharesQuotes.HoldCount = 0;
                    sharesQuotes.CanSoldCount = 0;
                    sharesQuotes.ProfitAmount = 0;
                    sharesQuotes.HoldId = 0;
                }
                else
                {
                    sharesQuotes.HoldCount = accountHold.RemainCount;
                    sharesQuotes.CanSoldCount = accountHold.CanSoldCount;
                    sharesQuotes.ProfitAmount = (accountHold.RemainCount * sharesQuotes.PresentPrice + accountHold.SoldAmount - accountHold.BuyTotalAmount - DBUtils.CalculateOtherCost(new List<long> { accountHold.Id }, 1)) / 100 * 100;
                    sharesQuotes.HoldId = accountHold.Id;
                }
                //是否st股票
                bool isSt = false;
                string SharesSTKey = "";
                var SharesST = (from item in db.t_system_param
                                where item.ParamName == "SharesST"
                                select item).FirstOrDefault();
                if (SharesST != null)
                {
                    SharesSTKey = SharesST.ParamValue;
                }
                if (!string.IsNullOrEmpty(SharesSTKey))
                {
                    try
                    {
                        string[] stList = SharesSTKey.Split(',');
                        foreach (var item in stList)
                        {
                            if (string.IsNullOrEmpty(item))
                            {
                                continue;
                            }
                            if (sharesQuotes.SharesName.Contains(item))
                            {
                                isSt = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                }

                //计算杠杆倍数
                var rules = (from item in db.t_shares_limit_fundmultiple
                             where (item.LimitMarket == sharesQuotes.Market || item.LimitMarket == -1) && (sharesQuotes.SharesCode.StartsWith(item.LimitKey))
                             orderby item.Priority descending, item.FundMultiple
                             select item).FirstOrDefault();
                if (rules == null)
                {
                    sharesQuotes.FundMultiple = 0;
                    sharesQuotes.TotalRise = 0;
                }
                else
                {
                    sharesQuotes.TotalRise = isSt ? rules.Range / 2 : rules.Range;
                    sharesQuotes.FundMultiple = rules.FundMultiple;
                    //查询用户杠杆倍数
                    var account_fundmultiple = (from item in db.t_shares_limit_fundmultiple_account
                                                where item.FundmultipleId == rules.Id && item.AccountId == basedata.AccountId
                                                select item).FirstOrDefault();
                    if (account_fundmultiple != null && account_fundmultiple.FundMultiple < sharesQuotes.FundMultiple)
                    {
                        sharesQuotes.FundMultiple = account_fundmultiple.FundMultiple;
                    }
                }
                //判断是否停牌
                var Suspension = (from item in db.t_shares_suspension
                                  where item.Status == 1 && item.Market == sharesQuotes.Market && item.SharesCode == sharesQuotes.SharesCode && item.SuspensionStartTime <= timeNow && item.SuspensionEndTime > timeNow
                                  select item).FirstOrDefault();
                if (Suspension != null)
                {
                    sharesQuotes.IsSuspension = true;
                }
                //判断是否闭市时间
                if (RunnerHelper.CheckTradeCloseTime())
                {
                    sharesQuotes.IsSuspension = true;
                }
                return sharesQuotes;
            }
        }

        /// <summary>
        /// 获取某只股票分笔成交数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetTradeSharesTransactionDataListRes GetTradeSharesTransactionDataList(GetTradeSharesTransactionDataListRequest request)
        {
            DateTime DateNow = DateTime.Now.Date;
            //计算交易日
            while (true)
            {
                if (RunnerHelper.CheckTradeDate(DateNow))
                {
                    break;
                }
                DateNow = DateNow.AddDays(-1);
            }
            DateTime startDate = DateNow;
            DateTime endDate = DateNow.AddDays(1);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                using (var db = new meal_ticketEntities())
                {
                    var transactiondata = from item in db.t_shares_transactiondata.AsNoTracking()
                                          where item.Market == request.Market && item.SharesCode == request.SharesCode && item.Time >= startDate && item.Time < endDate
                                          select item;
                    if (request.MaxOrderIndex > 0)
                    {
                        transactiondata = from item in transactiondata
                                          where item.OrderIndex > request.MaxOrderIndex
                                          select item;
                        request.TakeCount = 200;
                    }
                    if (request.MinOrderIndex > 0)
                    {
                        transactiondata = from item in transactiondata
                                          where item.OrderIndex < request.MinOrderIndex
                                          select item;
                    }

                    var list = (from item in transactiondata
                                orderby item.Time descending, item.OrderIndex descending
                                select new TradeSharesTransactionDataInfo
                                {
                                    SharesCode = item.SharesCode,
                                    TimeStr = item.TimeStr,
                                    Market = item.Market,
                                    Price = item.Price,
                                    Type = item.Type,
                                    Stock = item.Stock,
                                    OrderIndex = item.OrderIndex
                                }).Take(request.TakeCount).ToList().OrderBy(e => e.TimeStr).ThenBy(e => e.OrderIndex).ToList();



                    int MaxOrderIndex = 0;
                    int MinOrderIndex = 0;
                    if (list.Count() > 0)
                    {
                        MaxOrderIndex = list.Max(e => e.OrderIndex);
                        MinOrderIndex = list.Min(e => e.OrderIndex);
                    }

                    try
                    {
                        var sendData = new
                        {
                            Market = request.Market,
                            SharesCode = request.SharesCode
                        };
                        MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Tri");
                    }
                    catch (Exception)
                    { }

                    scope.Complete();
                    return new GetTradeSharesTransactionDataListRes
                    {
                        MaxOrderIndex = MaxOrderIndex,
                        MinOrderIndex = MinOrderIndex,
                        List = list
                    };
                }
            }
        }

        /// <summary>
        /// 获取交易账户信息
        /// </summary>
        /// <returns></returns>
        public TradeAccountInfo GetTradeAccountInfo(HeadBase basedata)
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                //剩余保证金
                long RemainDeposit = 0;
                long RetainDeposit = 0;
                var accountWallet = (from item in db.t_account_wallet
                                     where item.AccountId == basedata.AccountId
                                     select item).FirstOrDefault();
                if (accountWallet != null)
                {
                    RemainDeposit = accountWallet.Deposit / 100 * 100;

                }
                var buysetting = (from item in db.t_account_shares_buy_setting
                                  where item.AccountId == basedata.AccountId && item.Type == 1
                                  select item).FirstOrDefault();
                if (buysetting != null)
                {
                    RetainDeposit = buysetting.ParValue / 100 * 100;
                }
                //股票持有
                var sharesHold = from item in db.t_account_shares_hold
                                 join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                 where item.AccountId == basedata.AccountId
                                 select new { item, item2 };
                var sharesHold_ing = from item in sharesHold
                                     join item2 in db.v_shares_quotes_last on new { item.item.Market, item.item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                     where item.item.Status == 1 && (item.item.RemainCount > 0 || item.item.LastModified > dateNow)
                                     select new { item, item2 };
                //总市值
                long TotalMarketValue = 0;
                //总成本-卖出金额
                long remainAmount = 0;
                long useRemainDeposit = 0;
                long totalFundAmount = 0;
                long otherCost = 0;//其他成本


                if (sharesHold_ing.Count() > 0)
                {
                    TotalMarketValue = (sharesHold_ing.Sum(e => e.item.item.RemainCount * (e.item2.PresentPrice <= 0 ? e.item2.ClosedPrice : e.item2.PresentPrice))) / 100 * 100;
                    remainAmount = (sharesHold_ing.Sum(e => e.item.item.BuyTotalAmount - e.item.item.SoldAmount)) / 100 * 100;
                    otherCost = DBUtils.CalculateOtherCost(sharesHold_ing.Select(e => e.item.item.Id).ToList(), 1);
                    useRemainDeposit = (sharesHold_ing.Sum(e => e.item.item.RemainDeposit)) / 100 * 100;
                    totalFundAmount= (sharesHold_ing.Sum(e => e.item.item.FundAmount)) / 100 * 100;
                }
                //总盈亏(当前市值-（总成本-卖出金额）)
                long TotalProfit = 0;
                TotalProfit = TotalMarketValue - remainAmount - otherCost;

                //当天盈亏(（当天每笔卖出价-昨日收盘价）*卖出数量+（当前价-当天每笔买入价）*当天买入数量+（当前价-昨日收盘价）*非当天买入数量)
                long TodayProfit = 0;
                long todaySoldProfit = 0;
                long todayBuyProfit = 0;
                long otherHoldProfit = 0;
                int todayBuyCount = 0;
                otherCost = 0;

                long totalDeposit = 0;
                var entrust = from item in db.t_account_shares_entrust
                              join item2 in db.v_shares_quotes_last on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                              where item.AccountId == basedata.AccountId && item.CreateTime >= dateNow && item.Status == 3
                              select new { item, item2 };

                var buyToday = entrust.Where(e => e.item.TradeType == 1).ToList();
                var sellToday = entrust.Where(e => e.item.TradeType == 2).ToList();
                if (buyToday.Count() > 0)
                {
                    todayBuyProfit = (buyToday.Sum(e => ((e.item2.PresentPrice == 0 ? e.item2.ClosedPrice : e.item2.PresentPrice) - e.item.DealPrice) * e.item.DealCount)) / 100 * 100;
                    todayBuyCount = buyToday.Sum(e => e.item.DealCount);
                    otherCost = otherCost + DBUtils.CalculateOtherCost(buyToday.Select(e => e.item.Id).ToList(), 2);
                }
                if (sellToday.Count() > 0)
                {
                    todaySoldProfit = (sellToday.Sum(e => (e.item.DealPrice - e.item2.ClosedPrice) * e.item.DealCount)) / 100 * 100;
                    otherCost = otherCost + DBUtils.CalculateOtherCost(sellToday.Select(e => e.item.Id).ToList(), 2);
                }
                if (sharesHold_ing.Count() > 0)
                {
                    var todayCanSold = (from item in buyToday
                                        where item.item.IsJoinCanSold == true
                                        select item).ToList();
                    var temp = (from item in sharesHold_ing.ToList()
                                join item2 in todayCanSold on item.item.item.Id equals item2.item.HoldId into a
                                from ai in a.DefaultIfEmpty()
                                group new { item, ai } by item into g
                                select new
                                {
                                    PresentPrice = g.Key.item2.PresentPrice <= 0 ? g.Key.item2.ClosedPrice : g.Key.item2.PresentPrice,
                                    ClosedPrice = g.Key.item2.ClosedPrice,
                                    CanSoldCount = g.Key.item.item.CanSoldCount,
                                    SellingCount = g.Key.item.item.SellingCount,
                                    TodayCanSoldList = (from x in g
                                                        where x.ai != null
                                                        select x.ai).ToList()
                                }).ToList();
                    otherHoldProfit = temp.Sum(e => (e.PresentPrice - e.ClosedPrice) * (e.CanSoldCount + e.SellingCount - (e.TodayCanSoldList.Count() > 0 ? e.TodayCanSoldList.Sum(y => y.item.DealCount) : 0))) / 100 * 100;
                    totalDeposit = (sharesHold_ing.Sum(e => e.item.item.RemainDeposit)) / 100 * 100;
                }
                TodayProfit = todayBuyProfit + todaySoldProfit + otherHoldProfit - otherCost;


                long EntrustDeposit = 0;
                var buyEntrust = (from item in db.t_account_shares_entrust
                                  where item.AccountId == basedata.AccountId && item.TradeType == 1 && item.Status != 3
                                  select item).ToList();
                if (buyEntrust.Count() > 0)
                {
                    EntrustDeposit = (buyEntrust.Sum(e => e.FreezeDeposit)) / 100 * 100;
                }
                return new TradeAccountInfo
                {
                    RemainDeposit = RemainDeposit,
                    TotalMarketValue = TotalMarketValue,
                    TotalProfit = TotalProfit,
                    RetainDeposit= RetainDeposit,
                    UseRemainDeposit = useRemainDeposit,
                    TodayProfit = RunnerHelper.CheckTodayProfitTime(DateTime.Now) ? TodayProfit : 0,
                    TotalAssets = TotalMarketValue + RemainDeposit + totalDeposit + EntrustDeposit,
                    TotalFundAmount= totalFundAmount
                };
            }
        }

        /// <summary>
        /// 获取持仓列表
        /// </summary>
        /// <returns></returns>
        public PageRes<TradeHoldInfo> GetTradeHoldList(PageRequest request, HeadBase basedata)
        {
            DateTime timeNow = DateTime.Now;
            TimeSpan timeSpanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var sharesHold = from item in db.t_account_shares_hold
                                 join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                 join item3 in db.v_shares_quotes_last on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                                 where item.AccountId == basedata.AccountId && item.Status == 1 && (item.RemainCount > 0 || item.LastModified >= dateNow)
                                 select new { item, item2, item3 };
                if (request.MaxId > 0)
                {
                    sharesHold = from item in sharesHold
                                 where item.item.Id <= request.MaxId
                                 select item;
                }
                int totalCount = sharesHold.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = sharesHold.Max(e => e.item.Id);
                }

                var tempList = (from item in sharesHold
                                orderby item.item.CreateTime descending
                                select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                List<TradeHoldInfo> list = new List<TradeHoldInfo>();
                foreach (var item in tempList)
                {
                    long PresentPrice = item.item3.PresentPrice <= 0 ? item.item3.ClosedPrice : item.item3.PresentPrice;
                    long MarketValue = item.item.RemainCount * PresentPrice;
                    long ProfitAmount = MarketValue - (item.item.BuyTotalAmount - item.item.SoldAmount) - DBUtils.CalculateOtherCost(new List<long> { item.item.Id }, 1);

                    List<ClosingLineInfo> closingList = new List<ClosingLineInfo>();
                    var traderules = (from x in db.t_shares_limit_traderules
                                      where (x.LimitMarket == -1 || x.LimitMarket == item.item.Market) && ((x.LimitType == 1 && item.item.SharesCode.StartsWith(x.LimitKey)) || (x.LimitType == 2 && item.item2.SharesName.StartsWith(x.LimitKey))) && x.Status == 1
                                      select x).ToList();
                    long closingPrice = 0;
                    long cordonPrice = 0;
                    long addDepositAmount = 0;
                    foreach (var y in traderules)
                    {
                        CalculateClosingInfo(PresentPrice, item.item.RemainCount, item.item.RemainDeposit, item.item.FundAmount, y.Cordon, y.ClosingLine, out closingPrice, out addDepositAmount, out cordonPrice);

                        closingList.Add(new ClosingLineInfo
                        {
                            Type = 1,
                            Time = "全天",
                            ClosingPrice = closingPrice,
                            AddDepositAmount = addDepositAmount,
                            ColorType = PresentPrice <= closingPrice ? 2 : PresentPrice <= cordonPrice ? 3 : 1
                        });

                        //判断额外平仓规则
                        var rulesOther = (from z in db.t_shares_limit_traderules_other
                                          where z.Status == 1 && z.RulesId == y.Id
                                          orderby z.Times descending
                                          select z).ToList();
                        foreach (var other in rulesOther)
                        {
                            try
                            {
                                TimeSpan startTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                                TimeSpan endTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                                if (timeSpanNow < endTime)
                                {
                                    CalculateClosingInfo(PresentPrice, item.item.RemainCount, item.item.RemainDeposit, item.item.FundAmount, other.Cordon, other.ClosingLine, out closingPrice, out addDepositAmount, out cordonPrice);

                                    closingList.Add(new ClosingLineInfo
                                    {
                                        Type = 2,
                                        Time = startTime.ToString(@"hh\:mm") + "-" + endTime.ToString(@"hh\:mm"),
                                        ClosingPrice = closingPrice,
                                        AddDepositAmount = addDepositAmount,
                                        ColorType = PresentPrice < closingPrice ? 2 : PresentPrice < cordonPrice ? 3 : 1
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteFileLog("出错了", ex);
                                continue;
                            }
                        }
                    }

                    list.Add(new TradeHoldInfo
                    {
                        SharesCode = item.item.SharesCode,
                        SharesName = item.item2.SharesName,
                        HoldId = item.item.Id,
                        Market = item.item.Market,
                        RemainDeposit = item.item.RemainDeposit / 100 * 100,
                        FundAmount = item.item.FundAmount / 100 * 100,
                        PresentPrice = PresentPrice,
                        BuyTotalAmount = item.item.BuyTotalAmount,
                        CostPrice = item.item.RemainCount > 0 ? (long)(Math.Round((item.item.BuyTotalAmount - item.item.SoldAmount + DBUtils.CalculateOtherCost(new List<long> { item.item.Id }, 1)) * 1.0 / item.item.RemainCount, 0)) : (long)(Math.Round((item.item.BuyTotalAmount + DBUtils.CalculateOtherCost(new List<long> { item.item.Id }, 1)) * 1.0 / item.item.BuyTotalCount, 0)),
                        RemainCount = item.item.RemainCount,
                        MarketValue = MarketValue / 100 * 100,
                        CanSellCount = item.item.CanSoldCount,
                        ProfitAmount = ProfitAmount / 100 * 100,
                        ClosingTime = item.item.ClosingTime,
                        ClosingLineList = closingList,
                        ClosedPrice= item.item3.ClosedPrice
                    });
                }

                return new PageRes<TradeHoldInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 计算平仓信息
        /// </summary>
        private void CalculateClosingInfo(long CurrPrice, int RemainCount, long RemainDeposit, long RemainFundAmount, int Cordon, int ClosingLine, out long ClosingPrice, out long AddDepositAmount, out long CordonPrice)
        {
            //计算平仓信息
            //（市值+保证金-借钱）/借钱=价格线
            ClosingPrice = RemainCount <= 0 ? 0 : (long)(ClosingLine * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - RemainDeposit) / RemainCount;
            CordonPrice = RemainCount <= 0 ? 0 : (long)(Cordon * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - RemainDeposit) / RemainCount;
            long needDepositAmount = (long)(Cordon * 1.0 / 10000 * RemainFundAmount + RemainFundAmount - CurrPrice * RemainCount);
            AddDepositAmount = needDepositAmount - RemainDeposit < 0 ? 0 : needDepositAmount - RemainDeposit;
        }

        /// <summary>
        /// 获取交易委托列表
        /// </summary>
        /// <returns></returns>
        public PageRes<TradeEntrustInfo> GetTradeEntrustList(GetTradeEntrustListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var entrustList = from item in db.t_account_shares_entrust
                                  where item.AccountId == basedata.AccountId
                                  select item;
                if (request.HoldId > 0)
                {
                    entrustList = from item in entrustList
                                  where item.HoldId == request.HoldId
                                  select item;
                }
                if (request.MaxId > 0)
                {
                    entrustList = from item in entrustList
                                  where item.Id <= request.MaxId
                                  select item;
                }
                if (request.EntrustStatus > 0)
                {
                    entrustList = from item in entrustList
                                  where (item.Status == 3 && request.EntrustStatus == 2) || (item.Status != 3 && request.EntrustStatus == 1)
                                  select item;
                }
                if (request.EntrustType > 0)
                {
                    entrustList = from item in entrustList
                                  where item.TradeType == request.EntrustType
                                  select item;
                }
                int totalCount = entrustList.Count();
                long MaxId = 0;
                if (totalCount > 0)
                {
                    MaxId = entrustList.Max(e => e.Id);
                }

                var result = (from item in entrustList
                              orderby item.CreateTime descending
                              select new TradeEntrustInfo
                              {
                                  HoldId = item.HoldId,
                                  Status = item.Status,
                                  SharesCode = item.SharesCode,
                                  ClosingTime = item.ClosingTime,
                                  TradeType = item.TradeType,
                                  CreateTime = item.CreateTime,
                                  EntrustCount = item.EntrustCount,
                                  EntrustPrice = item.EntrustPrice,
                                  EntrustType = item.EntrustType,
                                  Id = item.Id,
                                  SoldProfitAmount = item.SoldProfitAmount / 100 * 100,
                                  IsExecuteClosing = item.IsExecuteClosing,
                                  Market = item.Market,
                                  SoldType = item.Type
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                foreach (var item in result)
                {
                    item.SoldProfitAmount = item.SoldProfitAmount - DBUtils.CalculateOtherCost(new List<long> { item.Id }, 2);
                    item.PresentPrice = 0;
                    var shares = (from x in db.t_shares_all
                                  where x.Market == item.Market && x.SharesCode == item.SharesCode
                                  select x).FirstOrDefault();
                    if (shares != null)
                    {
                        item.SharesName = shares.SharesName;
                        var quotes = (from x in db.v_shares_quotes_last
                                      where x.Market == item.Market && x.SharesCode == item.SharesCode
                                      select x).FirstOrDefault();
                        if (quotes != null)
                        {
                            item.PresentPrice = quotes.PresentPrice <= 0 ? quotes.ClosedPrice : quotes.PresentPrice;
                        }
                    }

                    var manager = (from x in db.t_account_shares_entrust_manager
                                   where x.BuyId == item.Id && ((x.DealCount >= shares.SharesHandCount && x.TradeType == 1) || x.TradeType == 2)
                                   select x).ToList();
                    if (manager.Count() > 0)
                    {
                        int dealCount = manager.Sum(e => e.DealCount);
                        long dealAmount = (manager.Sum(e => e.DealAmount)) / 100 * 100;
                        item.DealCount = dealCount;
                        item.DealPrice = (dealCount <= 0 ? 0 : dealAmount / dealCount) / 100 * 100;
                    }
                }
                return new PageRes<TradeEntrustInfo>
                {
                    TotalCount = totalCount,
                    MaxId = MaxId,
                    List = result
                };
            }
        }

        /// <summary>
        /// 获取交易成交记录列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<TradeEntrustDealDetailsInfo> GetTradeEntrustDealDetailsList(GetTradeEntrustDealDetailsListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var dealList = from item in db.t_account_shares_entrust
                               join item2 in db.t_account_shares_entrust_manager on item.Id equals item2.BuyId
                               join item3 in db.t_account_shares_entrust_manager_dealdetails on item2.Id equals item3.EntrustManagerId
                               join item4 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item4.Market, item4.SharesCode }
                               where ((request.Type != 2 && item.Id == request.Id) || (request.Type == 2 && item.HoldId == request.Id)) && item2.DealCount > 0 && ((item2.DealCount >= item4.SharesHandCount && item.TradeType == 1) || item.TradeType == 2)
                               select new { item, item2, item3, item4 };
                if (request.MaxId > 0)
                {
                    dealList = from item in dealList
                               where item.item3.Id <= request.MaxId
                               select item;
                }
                int totalCount = dealList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = dealList.Max(e => e.item3.Id);
                }

                return new PageRes<TradeEntrustDealDetailsInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in dealList
                            orderby item.item3.DealTime descending
                            select new TradeEntrustDealDetailsInfo
                            {
                                Id = item.item3.Id,
                                SharesCode = item.item.SharesCode,
                                SharesName = item.item4.SharesName,
                                DealAmount = item.item3.DealAmount / 100 * 100,
                                DealCount = item.item3.DealCount,
                                DealTime = item.item3.DealTime,
                                TradeType = item.item.TradeType,
                                DealPrice = item.item3.DealPrice / 100 * 100
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 设置自动平仓时间
        /// </summary>
        public void SetTradeClosingTime(SetTradeClosingTimeRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_SetTradeClosingTime(basedata.AccountId, request.HoldId, request.ClosingTime, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 补交保证金
        /// </summary>
        public void MakeupDeposit(MakeupDepositRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                bool success = false;
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                        ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                        db.P_MakeupDeposit(basedata.AccountId, request.HoldId, request.Deposit, errorCodeDb, errorMessageDb);
                        int errorCode = (int)errorCodeDb.Value;
                        string errorMessage = errorMessageDb.Value.ToString();
                        if (errorCode != 0)
                        {
                            throw new WebApiException(errorCode, errorMessage);
                        }
                        tran.Commit();
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
                if (success)
                {
                    DateTime timeNow = DateTime.Now;
                    TimeSpan timeSpanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
                    try
                    {
                        var noticeHold1 = (from item in db.t_notice_hold
                                           where item.HoldId == request.HoldId && item.Type == 1
                                           select item).FirstOrDefault();
                        var noticeHold2 = (from item in db.t_notice_hold
                                           where item.HoldId == request.HoldId && item.Type == 2
                                           select item).FirstOrDefault();
                        if (noticeHold1 == null && noticeHold2 == null)
                        {
                            return;
                        }

                        //判断是否回到警戒线,回到警戒线消除警戒线通知发送状态
                        var hold = (from item in db.t_account_shares_hold
                                    where item.Id == request.HoldId
                                    select item).FirstOrDefault();
                        var quotes = (from x in db.v_shares_quotes_last
                                      where x.Market == hold.Market && x.SharesCode == hold.SharesCode
                                      select x).FirstOrDefault();
                        var shares = (from x in db.t_shares_all
                                      where x.Market == hold.Market && x.SharesCode == hold.SharesCode
                                      select x).FirstOrDefault();
                        long PresentPrice = quotes.PresentPrice <= 0 ? quotes.ClosedPrice : quotes.PresentPrice;

                        //查询强制平仓线
                        int cordon = 0;
                        var traderulesList = (from x in db.t_shares_limit_traderules
                                              where (x.LimitMarket == -1 || x.LimitMarket == hold.Market) && ((x.LimitType == 1 && hold.SharesCode.StartsWith(x.LimitKey)) || (x.LimitType == 2 && shares.SharesName.StartsWith(x.LimitKey))) && x.Status == 1
                                              select x).ToList();
                        foreach (var traderules in traderulesList)
                        {
                            int tempCordon = traderules.Cordon;
                            //查询额外强制平仓线
                            var traderulesOther = (from x in db.t_shares_limit_traderules_other
                                                   where x.RulesId == traderules.Id && x.Status == 1
                                                   select x).ToList();
                            foreach (var other in traderulesOther)
                            {
                                try
                                {
                                    TimeSpan startTime = TimeSpan.Parse(other.Times.Split('-')[0]);
                                    TimeSpan endTime = TimeSpan.Parse(other.Times.Split('-')[1]);
                                    if (timeSpanNow >= startTime && timeSpanNow < endTime)
                                    {
                                        tempCordon = other.Cordon;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                            }

                            if (cordon < tempCordon)
                            {
                                cordon = tempCordon;
                            }
                        }

                        long closingPrice = 0;
                        long cordonPrice = 0;
                        long addDepositAmount = 0;
                        CalculateClosingInfo(PresentPrice, hold.RemainCount, hold.RemainDeposit, hold.FundAmount, cordon, 0, out closingPrice, out addDepositAmount, out cordonPrice);
                        if (PresentPrice >= cordonPrice)
                        {
                            if (noticeHold1 != null)
                            {
                                noticeHold1.LastNotifyTime = null;
                                noticeHold1.TimeId = -1;
                            }
                            if (noticeHold2 != null)
                            {
                                noticeHold2.LastNotifyTime = null;
                                noticeHold2.TimeId = -1;
                            }
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }
        }

        /// <summary>
        /// 股票申请买入
        /// </summary>
        public void ApplyTradeBuy(ApplyTradeBuyRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter buyIdDb = new ObjectParameter("buyId", 0);
                    db.P_ApplyTradeBuy(basedata.AccountId, request.Market, request.SharesCode, request.BuyAmount, request.FundMultiple, request.BuyPrice, request.ClosingTime, false,0, errorCodeDb, errorMessageDb, buyIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    long buyId = (long)buyIdDb.Value;

                    var entrust = (from item in db.t_account_shares_entrust
                                   where item.Id == buyId
                                   select item).FirstOrDefault();
                    if (entrust == null)
                    {
                        throw new WebApiException(400, "未知错误");
                    }

                    var sendData = new
                    {
                        BuyId = buyId,
                        BuyCount = entrust.EntrustCount,
                        BuyTime = DateTime.Now.ToString("yyyy-MM-dd")
                    };
                    bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesBuy", "s1");
                    if (!isSendSuccess)
                    {
                        throw new WebApiException(400, "买入失败,请重试");
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 股票申请卖出
        /// </summary>
        public void ApplyTradeSell(ApplyTradeSellRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter sellIdDb = new ObjectParameter("sellId", 0);
                    db.P_ApplyTradeSell(basedata.AccountId, request.HoldId, request.SellCount, request.SellType, request.SellPrice, 0, false, errorCodeDb, errorMessageDb, sellIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    long sellId = (long)sellIdDb.Value;

                    var entrustManager = (from item in db.t_account_shares_entrust_manager
                                          join item2 in db.t_broker_account_info on item.TradeAccountCode equals item2.AccountCode
                                          where item.BuyId == sellId && item.TradeType == 2 && item.Status == 1
                                          select new { item, item2 }).ToList();
                    if (entrustManager.Count() <= 0)
                    {
                        throw new WebApiException(400, "内部错误");
                    }
                    foreach (var item in entrustManager)
                    {
                        var sendData = new
                        {
                            SellManagerId = item.item.Id,
                            SellTime = DateTime.Now.ToString("yyyy-MM-dd")
                        };

                        var server = (from x in db.t_server_broker_account_rel
                                      join x2 in db.t_server on x.ServerId equals x2.ServerId
                                      where x.BrokerAccountId == item.item2.Id
                                      select x).FirstOrDefault();
                        if (server == null)
                        {
                            throw new WebApiException(400, "服务器配置有误");
                        }

                        bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesSell", server.ServerId);
                        if (!isSendSuccess)
                        {
                            throw new WebApiException(400, "卖出失败,请重试");
                        }
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 股票申请撤单
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ApplyTradeCancel(ApplyTradeCancelRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_ApplyTradeCancel(basedata.AccountId, request.Id, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }

                    var entrustManager = (from item in db.t_account_shares_entrust_manager
                                          join item2 in db.t_broker_account_info on item.TradeAccountCode equals item2.AccountCode
                                          where item.BuyId == request.Id
                                          select new { item, item2 }).ToList();
                    if (entrustManager.Count() <= 0)
                    {
                        var sendData = new
                        {
                            TradeManagerId = -1,
                            EntrustId = request.Id,
                            CancelTime = DateTime.Now.ToString("yyyy-MM-dd")
                        };
                        bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", "s1");
                    }
                    else
                    {
                        foreach (var item in entrustManager)
                        {
                            var sendData = new
                            {
                                TradeManagerId = item.item.Id,
                                EntrustId = request.Id,
                                CancelTime = DateTime.Now.ToString("yyyy-MM-dd")
                            };

                            var server = (from x in db.t_server_broker_account_rel
                                          join x2 in db.t_server on x.ServerId equals x2.ServerId
                                          where x.BrokerAccountId == item.item2.Id
                                          select x).FirstOrDefault();
                            if (server == null)
                            {
                                throw new WebApiException(400, "服务器配置有误");
                            }

                            bool isSendSuccess = MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", server.ServerId);
                            if (!isSendSuccess)
                            {
                                throw new WebApiException(400, "撤销失败,请重试");
                            }
                        }
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 获取用户交易杠杆倍数
        /// </summary>
        /// <returns></returns>
        public List<FundmultipleInfo> GetFundmultipleList(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var account_fundmultiple = from item in db.t_shares_limit_fundmultiple_account
                                           where item.AccountId == basedata.AccountId
                                           select item;

                var fundmultiple = (from item in db.t_shares_limit_fundmultiple
                                    join item2 in account_fundmultiple on item.Id equals item2.FundmultipleId into a from ai in a.DefaultIfEmpty()
                                    select new FundmultipleInfo
                                    {
                                        FundmultipleId = item.Id,
                                        MarketName = item.MarketName,
                                        FundMultiple = ai == null ? item.FundMultiple : ai.FundMultiple,
                                        MaxFundMultiple = item.FundMultiple
                                    }).ToList();
                return fundmultiple;
            }
        }

        /// <summary>
        /// 修改用户交易杠杆倍数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyFundmultiple(ModifyFundmultipleRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var account = (from item in db.t_account_baseinfo
                                   where item.Id == basedata.AccountId
                                   select item).FirstOrDefault();
                    if (account == null)
                    {
                        throw new WebApiException(400,"用户不存在");
                    }
                    if (account.MultipleChangeStatus != 1)
                    {
                        throw new WebApiException(400,"无修改权限，请联系客服");
                    }
                    //判断杠杆倍数是否存在
                    var fundmultiple = (from item in db.t_shares_limit_fundmultiple
                                        where item.Id == request.FundmultipleId
                                        select item).FirstOrDefault();
                    if (fundmultiple == null)
                    {
                        throw new WebApiException(400, "配置不存在");
                    }

                    //查询是否存在
                    string sql = string.Format("select top 1 Id from t_shares_limit_fundmultiple_account with(xlock) where FundmultipleId={0} and AccountId={1}", request.FundmultipleId, basedata.AccountId);
                    object sqlResult = null;
                    using (var cmd = db.Database.Connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        cmd.Transaction = tran.UnderlyingTransaction;
                        sqlResult = cmd.ExecuteScalar();
                    }
                    if (sqlResult == null)
                    {
                        db.t_shares_limit_fundmultiple_account.Add(new t_shares_limit_fundmultiple_account
                        {
                            AccountId = basedata.AccountId,
                            CreateTime = DateTime.Now,
                            FundMultiple = request.FundMultiple > fundmultiple.FundMultiple ? fundmultiple.FundMultiple : request.FundMultiple,
                            FundmultipleId = request.FundmultipleId,
                            LastModified = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        sql = string.Format("update t_shares_limit_fundmultiple_account set FundMultiple={0},LastModified='{1}' where Id={2}", request.FundMultiple > fundmultiple.FundMultiple ? fundmultiple.FundMultiple : request.FundMultiple, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Convert.ToInt64(sqlResult));
                        db.Database.ExecuteSqlCommand(sql);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 获取用户跟投信息
        /// </summary>
        /// <returns></returns>
        public AccountFollowInfo GetAccountFollowInfo(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断是否存在跟投
                var followRel = (from item in db.t_account_follow_rel
                                 where item.FollowAccountId == basedata.AccountId
                                 select item).FirstOrDefault();
                if (followRel != null)
                {
                    return new AccountFollowInfo
                    {
                        StatusDesc = "",
                        FollowStatus = 3
                    };
                }
                //查询最后一次请求
                var record = (from item in db.t_account_follow_apply_record
                              where item.FollowAccountId == basedata.AccountId
                              orderby item.CreateTime descending
                              select item).FirstOrDefault();
                if (record == null)
                {
                    return new AccountFollowInfo
                    {
                        FollowStatus = 0,
                        StatusDesc = ""
                    };
                }
                if (record.Status == 1)
                {
                    return new AccountFollowInfo
                    {
                        FollowStatus = 1,
                        StatusDesc = ""
                    };
                }
                if (record.Status == 2)
                {
                    return new AccountFollowInfo
                    {
                        FollowStatus = 2,
                        StatusDesc = ""
                    };
                }
                if (record.Status == 3)
                {
                    return new AccountFollowInfo
                    {
                        FollowStatus = 0,
                        StatusDesc = ""
                    };
                }
                if (record.Status == 4)
                {
                    return new AccountFollowInfo
                    {
                        FollowStatus = 4,
                        StatusDesc = record.StatusDes
                    };
                }
                if (record.Status == 5)
                {
                    return new AccountFollowInfo
                    {
                        FollowStatus = 0,
                        StatusDesc = ""
                    };
                }
                return new AccountFollowInfo
                {
                    FollowStatus = 0,
                    StatusDesc = ""
                };
            }
        }

        /// <summary>
        /// 用户申请跟投
        /// </summary>
        public void ApplyFollow(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_FollowApply(basedata.AccountId, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 用户取消跟投
        /// </summary>
        public void CancelFollow(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_FollowCancel(basedata.AccountId, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 获取用户历史持仓
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public GetAccountHoldHistoryListRes GetAccountHoldHistoryList(GetAccountHoldHistoryListRequest request, HeadBase basedata)
        {
            DateTime dateNow = DateTime.Now.Date;
            DateTime startTime;
            DateTime endTime;
            if (!DateTime.TryParse(request.StartTime, out startTime))
            {
                startTime = DateTime.Parse("1900-01-01 00:00:00");
            }
            if (!DateTime.TryParse(request.EndTime, out endTime))
            {
                endTime = DateTime.Parse("2999-01-01 00:00:00");
            }
            using (var db = new meal_ticketEntities())
            {
                var holdHistory = from item in db.t_account_shares_hold
                                  where item.RemainCount <= 0 && item.LastModified < dateNow && item.Status != 0 && item.AccountId == basedata.AccountId && item.LastModified >= startTime && item.LastModified < endTime
                                  select item;

                int totalCount = holdHistory.Count();
                int sharesCount = (from item in holdHistory
                                   group item by new { item.Market, item.SharesCode } into g
                                   select g).Count();
                long totalBuyAmount = totalCount > 0 ? holdHistory.Sum(e => e.BuyTotalAmount) : 0;

                List<long> holdList = holdHistory.Select(e => e.Id).ToList();
                long otherCost = DBUtils.CalculateOtherCost(holdList, 1);
                long totalProfitAmount = totalCount > 0 ? holdHistory.Sum(e => e.SoldAmount - e.BuyTotalAmount) - otherCost : 0;

                return new GetAccountHoldHistoryListRes
                {
                    MaxId = 0,
                    SharesCount = sharesCount,
                    TotalBuyAmount = totalBuyAmount,
                    TotalCount = totalCount,
                    TotalProfitAmount = totalProfitAmount,
                    List = (from item in holdHistory
                            join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                            orderby item.LastModified descending
                            select new AccountHoldHistoryInfo
                            {
                                EndTime = item.LastModified,
                                SellAmount = item.SoldAmount,
                                HoldId = item.Id,
                                SellCount = item.SoldCount,
                                SharesCode = item.SharesCode,
                                Market = item.Market,
                                StartTime = item.CreateTime,
                                SharesName = item2.SharesName,
                                BuyTotalAmount = item.BuyTotalAmount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 获取条件单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<AccountHoldConditionTradeInfo> GetAccountHoldConditionTradeList(GetAccountHoldConditionTradeListRequest request, HeadBase basedata)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var conditiontrade = (from item in db.t_account_shares_hold_conditiontrade.AsNoTracking()
                                      where item.HoldId == request.HoldId && item.Type == request.Type && item.AccountId == basedata.AccountId && item.SourceFrom==0 && item.Status==1
                                      select new AccountHoldConditionTradeInfo
                                      {
                                          ConditionPrice = item.ConditionPrice,
                                          ConditionTime = item.ConditionTime,
                                          EntrustCount = item.EntrustCount,
                                          EntrustPriceGear = item.EntrustPriceGear,
                                          EntrustType = item.EntrustType,
                                          Id = item.Id,
                                          ForbidType=item.ForbidType,
                                          TriggerTime = item.TriggerTime,
                                          EntrustId=item.EntrustId
                                      }).ToList();
                foreach (var item in conditiontrade)
                {
                    if (item.EntrustId > 0)
                    {
                        var entrust = (from x in db.t_account_shares_entrust
                                       where x.Id == item.EntrustId
                                       select x).FirstOrDefault();
                        if (entrust != null)
                        {
                            if (item.EntrustType == 2)
                            {
                                item.EntrustPrice = entrust.EntrustPrice;
                            }
                            item.RelEntrustCount = entrust.EntrustCount;
                            item.RelDealCount = entrust.DealCount;
                            item.EntrustStatus = entrust.Status;
                        }
                    }
                }
                switch (request.Type)
                {
                    case 1:
                        conditiontrade = conditiontrade.OrderBy(e => e.ConditionTime).ToList();
                        break;
                    case 2:
                        conditiontrade = conditiontrade.OrderBy(e => e.ConditionPrice).ToList();
                        break;
                    case 3:
                        conditiontrade = conditiontrade.OrderByDescending(e => e.ConditionPrice).ToList();
                        break;
                }

                scope.Complete();
                return conditiontrade;
            }
        }

        /// <summary>
        /// 添加条件单
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddAccountHoldConditionTrade(AddAccountHoldConditionTradeRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断持仓是否存在
                var hold = (from item in db.t_account_shares_hold
                            where item.Id == request.HoldId && item.AccountId == basedata.AccountId
                            select item).FirstOrDefault();
                if (hold == null)
                {
                    throw new WebApiException(400, "持仓不存在");
                }

                db.t_account_shares_hold_conditiontrade.Add(new t_account_shares_hold_conditiontrade
                {
                    AccountId = basedata.AccountId,
                    ConditionPrice = request.ConditionPrice,
                    ConditionTime = request.ConditionTime,
                    CreateTime = DateTime.Now,
                    EntrustCount = request.EntrustCount,
                    EntrustPriceGear = request.EntrustPriceGear,
                    EntrustType = request.EntrustType,
                    HoldId = request.HoldId,
                    LastModified = DateTime.Now,
                    TradeType = request.TradeType,
                    TriggerTime = null,
                    SourceFrom=0,
                    Status=1,
                    FatherId=0,
                    ForbidType=request.ForbidType,
                    Name="",
                    Type = request.Type
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件单
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteAccountHoldConditionTrade(DeleteRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var ConditionTrade = (from item in db.t_account_shares_hold_conditiontrade
                                      where item.Id == request.Id && item.AccountId == basedata.AccountId
                                      select item).FirstOrDefault();
                if (ConditionTrade == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                db.t_account_shares_hold_conditiontrade.Remove(ConditionTrade);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 判断是否交易时间
        /// </summary>
        /// <param name="request"></param>
        public void IsTradeTimeRequest(IsTradeTimeRequest request) 
        {
            DateTime? time;
            DateTime tempTime;
            if (!DateTime.TryParse(request.Time, out tempTime))
            {
                time = null;
            }
            else
            {
                time = tempTime;
            }
            if(!RunnerHelper.CheckTradeTime2(time, false, true, false))
            {
                throw new WebApiException(400,"非交易时间");
            }
        }
    }
}
