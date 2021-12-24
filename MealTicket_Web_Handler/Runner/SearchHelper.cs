using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public struct SharesBase
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }
    }

    public struct Shares_Quotes_Date_Info
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string Date { get; set; }

        public long PresentPrice { get; set; }

        public long OpenedPrice { get; set; }

        public long ClosedPrice { get; set; }

        public long LimitUpPrice { get; set; }

        public long LimitDownPrice { get; set; }

        public int PriceType { get; set; }

        public int TriPriceType { get; set; }

        public int LimitUpCount { get; set; }


        public int LimitUpBombCount { get; set; }

        public long MaxPrice { get; set; }

        public long MinPrice { get; set; }
    }

    public struct Shares_NotLimitUp_Count
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public int NotLimitUpCount { get; set; }
    }

    public struct Shares_LimitUp_Count
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public int LimitUpCount { get; set; }

        public int TriLimitUpCount { get; set; }
    }

    public struct Shares_LimitDown_Count
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public int LimitDownCount { get; set; }

        public int TriLimitDownCount { get; set; }
    }

    public struct Shares_LimitUpBomb_Count
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public int LimitUpBombCount { get; set; }
    }

    public struct Shares_Deal_Info
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public long AvgTotalAmount { get; set; }

        public long AvgTotalCount { get; set; }

        public int DateCount { get; set; }
    }

    public struct Shares_AvgLine_Info 
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }
        
        public long PresentPrice{ get; set; } 
        public long OpenedPrice { get; set; }

        public long MaxPrice { get; set; }
        
        public long MinPrice { get; set; }
        
        public DateTime LastModified { get; set; }

        public string Date { get; set; }
    }

    public struct BuyOrSell_Info
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public int BuyCount1 { get; set; }

        public int BuyCount2 { get; set; }

        public int BuyCount3 { get; set; }

        public int BuyCount4 { get; set; }

        public int BuyCount5 { get; set; }

        public int SellCount1 { get; set; }

        public int SellCount2 { get; set; }

        public int SellCount3 { get; set; }

        public int SellCount4 { get; set; }

        public int SellCount5 { get; set; }
    }

    public struct BuyOrSell_Rate_Info
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public int DataType { get; set; }
        public long PresentPrice { get; set; }
        public int BuyCount1 { get; set; }
        public int BuyCount2 { get; set; }
        public int BuyCount3 { get; set; }
        public int BuyCount4 { get; set; }
        public int BuyCount5 { get; set; }
        public long BuyPrice1 { get; set; }
        public long BuyPrice2 { get; set; }
        public long BuyPrice3 { get; set; }
        public long BuyPrice4 { get; set; }
        public long BuyPrice5 { get; set; }
        public int SellCount1 { get; set; }
        public int SellCount2 { get; set; }
        public int SellCount3 { get; set; }
        public int SellCount4 { get; set; }
        public int SellCount5 { get; set; }
        public long SellPrice1 { get; set; }
        public long SellPrice2 { get; set; }
        public long SellPrice3 { get; set; }
        public long SellPrice4 { get; set; }
        public long SellPrice5 { get; set; }
    }

    public struct Quotes_Rate_Info 
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public int DataType { get; set; }
        public long PresentPrice { get; set; }
        public int BuyCount1 { get; set; }
        public int BuyCount2 { get; set; }
        public int BuyCount3 { get; set; }
        public int BuyCount4 { get; set; }
        public int BuyCount5 { get; set; }
        public long BuyPrice1 { get; set; }
        public long BuyPrice2 { get; set; }
        public long BuyPrice3 { get; set; }
        public long BuyPrice4 { get; set; }
        public long BuyPrice5 { get; set; }
        public int SellCount1 { get; set; }
        public int SellCount2 { get; set; }
        public int SellCount3 { get; set; }
        public int SellCount4 { get; set; }
        public int SellCount5 { get; set; }
        public long SellPrice1 { get; set; }
        public long SellPrice2 { get; set; }
        public long SellPrice3 { get; set; }
        public long SellPrice4 { get; set; }
        public long SellPrice5 { get; set; }
        public long LimitUpPrice { get; set; }
        public long LimitDownPrice { get; set; }
    }

    public struct SessionDataSet
    {
        public Dictionary<long, Shares_Base_Session_Info> Shares_Base_Session;

        public object Shares_Base_Session_lock;

        public Dictionary<long, Shares_Quotes_Session_Info_Last> Shares_Quotes_Last_Session;

        public object Shares_Quotes_Last_Session_lock;
    }

    public class Template_Result_DataSet
    {
        public ThreadMsgTemplate<searchInfo> condition_queue { get; set; }

        public List<searchInfo> ResultList { get; set; }

        public object ResultListLock { get; set; }
    }

    public class SearchHelper
    {
        private SessionDataSet sessionDataSet = new SessionDataSet();

        public SearchHelper()
        {
            sessionDataSet.Shares_Base_Session = null;
            sessionDataSet.Shares_Quotes_Last_Session = null;


            sessionDataSet.Shares_Base_Session_lock = new object();
            sessionDataSet.Shares_Quotes_Last_Session_lock = new object();
        }

        private Dictionary<long, Shares_Base_Session_Info> GetSharesBaseSession()
        {
            lock (sessionDataSet.Shares_Base_Session_lock)
            {
                if (sessionDataSet.Shares_Base_Session == null)
                {
                    sessionDataSet.Shares_Base_Session = Singleton.Instance.sessionHandler.GetShares_Base_Session();
                }
            }
            return sessionDataSet.Shares_Base_Session;
        }
        private Dictionary<long, Shares_Quotes_Session_Info_Last> GetSharesQuotesLastSession()
        {
            lock (sessionDataSet.Shares_Quotes_Last_Session_lock)
            {
                if (sessionDataSet.Shares_Quotes_Last_Session == null)
                {
                    sessionDataSet.Shares_Quotes_Last_Session = Singleton.Instance.sessionHandler.GetShares_Quotes_Last_Session();
                }
            }
            return sessionDataSet.Shares_Quotes_Last_Session;
        }

        /// <summary>
        /// 模板自动搜索
        /// </summary>
        public void SearchMonitor()
        {
            //查询需要搜索的模板
            ThreadMsgTemplate<t_sys_conditiontrade_template_search> template_queue = new ThreadMsgTemplate<t_sys_conditiontrade_template_search>();
            template_queue.Init();
            if (!BuildSearchTemplateQueue(ref template_queue))
            {
                return;
            }
            
            //创建任务执行搜索
            int dataCount = template_queue.GetCount();
            int taskCount = dataCount <= Singleton.Instance.TempThreadCount ? dataCount : Singleton.Instance.TempThreadCount;
            WaitHandle[] handlerArr = new WaitHandle[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                handlerArr[i] = TaskThread.CreateTask(DoSearchTemplate, template_queue);
            }
            TaskThread.WaitAll(handlerArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(handlerArr);
            template_queue.Release();
        }

        /// <summary>
        /// 生成需要搜索的模板队列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool BuildSearchTemplateQueue(ref ThreadMsgTemplate<t_sys_conditiontrade_template_search> data)
        {
            using (var db = new meal_ticketEntities())
            {
                var searchTemplate = (from item in db.t_sys_conditiontrade_template
                                      join item2 in db.t_sys_conditiontrade_template_search on item.Id equals item2.TemplateId
                                      where item.Status == 1 && item.Type == 5
                                      select item2).ToList();
                if (searchTemplate.Count() == 0)
                {
                    return false;
                }
                foreach (var item in searchTemplate)
                {
                    data.AddMessage(item);
                }
                return true;
            }
        }

        /// <summary>
        /// 分任务执行模板搜索
        /// </summary>
        /// <param name="data_queue"></param>
        private void DoSearchTemplate(object context)
        {
            var template_queue = context as ThreadMsgTemplate<t_sys_conditiontrade_template_search>;
            do
            {
                t_sys_conditiontrade_template_search searchCondition = new t_sys_conditiontrade_template_search();
                if (!template_queue.GetMessage(ref searchCondition, true))
                {
                    break;
                }
                var searchList=ParseSearchCondition(searchCondition.TemplateContent);
                var dataResult=DoSearchTemplate(searchList);
                //入库
                CalDataToDataBase(dataResult, searchCondition.TemplateId);
            } while (true);
        }

        public List<SharesBase> DoSearchTemplate(List<searchInfo> parList)
        {
            ThreadMsgTemplate<searchInfo> taskData = new ThreadMsgTemplate<searchInfo>();
            taskData.Init();
            if (!BuildSearchConditionQueue(parList, ref taskData))
            {
                return new List<SharesBase>();
            }

            int conditionCount = taskData.GetCount();
            int taskCount = conditionCount <= Singleton.Instance.ConditionThreadCount ? conditionCount : Singleton.Instance.ConditionThreadCount;
            WaitHandle[] taskArr = new WaitHandle[taskCount];
            List<searchInfo> resultList = new List<searchInfo>();
            object resultListLock = new object();
            for (int i = 0; i < taskCount; i++)
            {
                taskArr[i] = TaskThread.CreateTask(_doSearchCondition, new Template_Result_DataSet
                {
                    ResultList = resultList,
                    condition_queue = taskData,
                    ResultListLock = resultListLock
                });
            }
            TaskThread.WaitAll(taskArr, Timeout.Infinite);
            TaskThread.CloseAllTasks(taskArr);
            taskData.Release();

            //分析模板搜索结果
            List<SharesBase> dataResult = Analysis_Template_Search_Result(resultList);
            return dataResult;
        }

        private List<SharesBase> Analysis_Template_Search_Result(List<searchInfo> resultData)
        {
            List<SharesBase> dataResult = new List<SharesBase>();
            var sharesBaseDic = GetSharesBaseSession();

            resultData = resultData.OrderBy(e => e.orderIndex).ToList();
            foreach (var item in sharesBaseDic)
            {
                bool isCheck = false;
                string checkStr = "";
                int index = 0;
                foreach (var condition in resultData)
                {
                    index++;
                    bool isTrue = false;
                    if (condition.SharesDic.ContainsKey(item.Key))
                    {
                        isTrue = true;
                        isCheck = true;
                    }
                    if (condition.leftbracket == 1)//有左括号
                    {
                        checkStr = checkStr + " ( ";
                    }
                    if (isTrue)
                    {
                        checkStr = checkStr + " 1=1 ";
                    }
                    else
                    {
                        checkStr = checkStr + " 1=0 ";
                    }
                    if (condition.rightbracket == 1)//有右括号
                    {
                        checkStr = checkStr + " ) ";
                    }
                    if (index < resultData.Count())
                    {
                        if (condition.connect == 1)
                        {
                            checkStr = checkStr + " or ";
                        }
                        else
                        {
                            checkStr = checkStr + " and ";
                        }
                    }
                }
                bool checkResult = false;
                if (isCheck)
                {
                    //检查是否满足
                    NCalc.Expression expr = new NCalc.Expression(checkStr);
                    checkResult = (bool)expr.Evaluate();
                }
                if (checkResult)
                {
                    dataResult.Add(new SharesBase 
                    {
                        SharesCode=item.Value.SharesCode,
                        Market=item.Value.Market
                    });
                }
            }
            return dataResult;
        }

        private List<searchInfo> ParseSearchCondition(string cnt)
        {
            try
            {
                List<dynamic> searchList = JsonConvert.DeserializeObject<List<dynamic>>(cnt);
                if (searchList.Count() == 0)
                {
                    return new List<searchInfo>();
                }
                List<searchInfo> searchPar = new List<searchInfo>();
                foreach (var x in searchList)
                {
                    int connect = x.connect;
                    int leftbracket = string.IsNullOrEmpty(Convert.ToString(x.leftbracket)) ? 0 : Convert.ToInt32(x.leftbracket);
                    int rightbracket = string.IsNullOrEmpty(Convert.ToString(x.rightbracket)) ? 0 : Convert.ToInt32(x.rightbracket);
                    int type = x.type;
                    searchPar.Add(new searchInfo
                    {
                        connect = connect,
                        content = JsonConvert.SerializeObject(x.content),
                        leftbracket = leftbracket,
                        rightbracket = rightbracket,
                        type = type
                    });
                }
                return searchPar;
            }
            catch (Exception ex)
            {
                return new List<searchInfo>();
            }
        }

        /// <summary>
        /// 生成需要搜索的条件队列
        /// </summary>
        /// <param name="cnt"></param>
        /// <param name="taskData"></param>
        /// <returns></returns>
        private bool BuildSearchConditionQueue(List<searchInfo> searchList, ref ThreadMsgTemplate<searchInfo> taskData)
        {
            try
            {
                int idx = 0;
                foreach (var x in searchList)
                {
                    idx++;
                    taskData.AddMessage(new searchInfo
                    {
                        connect = x.connect,
                        content = x.content,
                        leftbracket = x.leftbracket,
                        rightbracket = x.rightbracket,
                        type = x.type,
                        orderIndex= idx
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("搜索参数错误", ex);
                return false;
            }
        }

        private void _doSearchCondition(object context)
        {
            var data = context as Template_Result_DataSet;
            do 
            {
                searchInfo tempData = new searchInfo();
                if (!data.condition_queue.GetMessage(ref tempData, true))
                {
                    break;
                }
                tempData.SharesDic = _toCheckCondition(tempData).ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
                lock (data.ResultListLock)
                {
                    data.ResultList.Add(tempData);
                }

            } while (true);
        }

        /// <summary>
        /// 判断条件
        /// </summary>
        private List<SharesBase> _toCheckCondition(searchInfo par)
        {
            List<SharesBase> result = new List<SharesBase>();
            switch (par.type)
            {
                case 1:
                    result = Analysis_Price(par.content);
                    break;
                case 2:
                    result = Analysis_HisRiseRate(par.content);
                    break;
                case 3:
                    result = Analysis_TodayRiseRate(par.content);
                    break;
                case 4:
                    result =Analysis_PlateRiseRate(par.content);
                    break;
                case 5:
                    result =Analysis_CurrentPrice(par.content);
                    break;
                case 6:
                    result =Analysis_ReferAverage(par.content);
                    break;
                case 7:
                    result = Analysis_ReferPrice(par.content);
                    break;
                case 8:
                    result = Analysis_BuyOrSellCount(par.content);
                    break;
                case 9:
                    result = Analysis_QuotesChangeRate(par.content);
                    break;
                case 10:
                    result = Analysis_QuotesTypeChangeRate(par.content);
                    break;
                case 11:
                    result = Analysis_SharesMarket(par.content);
                    break;
                default:
                    break;
            }
            return result;
        }

        private void CalDataToDataBase(List<SharesBase> dataList, long templateId)
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var accountSetting = (from item in db.t_search_tri_setting
                                      where item.TemplateId == templateId && item.Type == -13
                                      select item.AccountId).ToList();
                var tri = from item in db.t_search_tri
                          where item.LastPushTime > dateNow
                          select item;
                var search_tri = (from item in db.t_account_baseinfo
                                  join item2 in tri on item.Id equals item2.AccountId
                                  where item.Status == 1 && item.MonitorStatus == 1 && accountSetting.Contains(item.Id)
                                  select new
                                  {
                                      AccountId = item.Id,
                                      Market = item2.Market,
                                      SharesCode = item2.SharesCode,
                                      LastPushTime = item2.LastPushTime,
                                      LastPushMaxRate = item2.LastPushMaxRate,
                                      LastPushMinRate = item2.LastPushMinRate,
                                      LastPushRate = item2.LastPushRate,
                                      MinPushTimeInterval = item2.MinPushTimeInterval,
                                      MinPushMaxRateInterval = item2.MinPushMaxRateInterval,
                                      MinPushMinRateInterval = item2.MinPushMinRateInterval,
                                      MinPushRiseRateInterval = item2.MinPushRiseRateInterval,
                                      MinPushDownRateInterval = item2.MinPushDownRateInterval,
                                      TriCount = item2.TriCount
                                  }).ToList();
                var search_tri_dic = search_tri.ToDictionary(k => k.AccountId * 10000000 + int.Parse(k.SharesCode) * 10 + k.Market, v => v);
                var quotes_last_session = GetSharesQuotesLastSession();

                var resultList = new List<dynamic>();
                foreach (var item in dataList)
                {
                    long shares_key = long.Parse(item.SharesCode) * 10 + item.Market;
                    if (!quotes_last_session.ContainsKey(shares_key))
                    {
                        continue;
                    }
                    long PresentPrice = quotes_last_session[shares_key].shares_quotes_info.ClosedPrice;
                    long ClosePrice = quotes_last_session[shares_key].shares_quotes_info.YestodayClosedPrice;
                    if (PresentPrice == 0 || ClosePrice == 0)
                    {
                        continue;
                    }
                    int Rate = (int)Math.Round((PresentPrice - ClosePrice) * 1.0 / ClosePrice * 10000, 0);
                    foreach (var item2 in accountSetting)
                    {
                        long key = item2 * 10000000 + int.Parse(item.SharesCode) * 10 + item.Market;
                        if (!search_tri_dic.ContainsKey(key))
                        {
                            resultList.Add(new
                            {
                                AccountId = item2,
                                Market = item.Market,
                                SharesCode = item.SharesCode,
                                PresentPrice = PresentPrice,
                                LastPushTime = DateTime.Now,
                                LastPushMaxRate = Rate,
                                LastPushMinRate = Rate,
                                LastPushRate = Rate,
                                LastPushType = 0,
                                TriCount = 1
                            });
                            continue;
                        }

                        var temp = search_tri_dic[key];
                        if (Rate >= temp.LastPushMaxRate + temp.MinPushMaxRateInterval)
                        {
                            resultList.Add(new
                            {
                                AccountId = item2,
                                Market = item.Market,
                                SharesCode = item.SharesCode,
                                PresentPrice = PresentPrice,
                                LastPushTime = DateTime.Now,
                                LastPushMaxRate = Rate,
                                LastPushMinRate = temp.LastPushMinRate,
                                LastPushRate = Rate,
                                LastPushType = 1,
                                TriCount = temp.TriCount + 1
                            });
                        }
                        else if (Rate <= temp.LastPushMinRate + temp.MinPushMinRateInterval)
                        {
                            resultList.Add(new
                            {
                                AccountId = item2,
                                Market = item.Market,
                                SharesCode = item.SharesCode,
                                PresentPrice = PresentPrice,
                                LastPushTime = DateTime.Now,
                                LastPushMaxRate = temp.LastPushMaxRate,
                                LastPushMinRate = Rate,
                                LastPushRate = Rate,
                                LastPushType = 2,
                                TriCount = temp.TriCount + 1
                            });
                        }
                        else if (Rate >= temp.LastPushRate + temp.MinPushRiseRateInterval)
                        {
                            resultList.Add(new
                            {
                                AccountId = item2,
                                Market = item.Market,
                                SharesCode = item.SharesCode,
                                PresentPrice = PresentPrice,
                                LastPushTime = DateTime.Now,
                                LastPushMaxRate = temp.LastPushMaxRate,
                                LastPushMinRate = temp.LastPushMinRate,
                                LastPushRate = Rate,
                                LastPushType = 3,
                                TriCount = temp.TriCount + 1
                            });
                        }
                        else if (Rate <= temp.LastPushRate + temp.MinPushDownRateInterval)
                        {
                            resultList.Add(new
                            {
                                AccountId = item2,
                                Market = item.Market,
                                SharesCode = item.SharesCode,
                                PresentPrice = PresentPrice,
                                LastPushTime = DateTime.Now,
                                LastPushMaxRate = temp.LastPushMaxRate,
                                LastPushMinRate = temp.LastPushMinRate,
                                LastPushRate = Rate,
                                LastPushType = 4,
                                TriCount = temp.TriCount + 1
                            });
                        }
                    }
                }
                SearchToDataBase(resultList);
            }
        }
        private void SearchToDataBase(List<dynamic> list)
        {
            DataTable table = new DataTable();
            #region====定义表字段数据类型====
            table.Columns.Add("AccountId", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            table.Columns.Add("PresentPrice", typeof(long));
            table.Columns.Add("LastPushTime", typeof(DateTime));
            table.Columns.Add("LastPushMaxRate", typeof(int));
            table.Columns.Add("LastPushMinRate", typeof(int));
            table.Columns.Add("LastPushRate", typeof(int));
            table.Columns.Add("LastPushType", typeof(int));
            table.Columns.Add("TriCount", typeof(int));
            #endregion

            #region====绑定数据====
            foreach (var item in list)
            {
                DataRow row = table.NewRow();
                row["AccountId"] = item.AccountId;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                row["PresentPrice"] = item.PresentPrice;
                row["LastPushTime"] = item.LastPushTime;
                row["LastPushMaxRate"] = item.LastPushMaxRate;
                row["LastPushMinRate"] = item.LastPushMinRate;
                row["LastPushRate"] = item.LastPushRate;
                row["LastPushType"] = item.LastPushType;
                row["TriCount"] = item.TriCount;
                table.Rows.Add(row);
            }
            #endregion

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@searchMonitorList", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SearchMonitor";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_SearchMonitor_Update @searchMonitorList", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("搜索结果入库失败了", ex);
                    tran.Rollback();
                }
            }
        }

        #region====价格条件====
        //分析价格条件
        public List<SharesBase> Analysis_Price(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int priceType = 0;
                int compare = 0;
                long priceError = 0;
                if (!Analysis_Price_BuildPar(par, ref priceType, ref compare, ref priceError))
                {
                    return new List<SharesBase>();
                }
                //获取股票最后一条行情缓存
                var quotesDic = GetSharesQuotesLastSession();

                foreach (var item in quotesDic)
                {
                    if (item.Value.shares_quotes_info.LimitUpPrice <= 0 || item.Value.shares_quotes_info.LimitDownPrice<=0)
                    {
                        continue;
                    }
                    long disPrice = priceType == 1 ? (item.Value.shares_quotes_info.LimitUpPrice + priceError)
                        : priceType == 2 ? (item.Value.shares_quotes_info.LimitDownPrice + priceError)
                        : priceType == 3 ? (long)(item.Value.shares_quotes_info.YestodayClosedPrice * 1.0 / 100 * priceError / 10000 + item.Value.shares_quotes_info.YestodayClosedPrice)
                        : 0;
                    if (disPrice <= 0 || item.Value.shares_quotes_info.ClosedPrice <= 0)
                    {
                        continue;
                    }
                    if ((compare == 1 && item.Value.shares_quotes_info.ClosedPrice >= disPrice) || (compare == 2 && item.Value.shares_quotes_info.ClosedPrice <= disPrice))
                    {
                        result.Add(new SharesBase
                        {
                            Market = item.Value.shares_quotes_info.Market,
                            SharesCode = item.Value.shares_quotes_info.SharesCode
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_Price出错", ex);
            }
            return result;
        }

        private bool Analysis_Price_BuildPar(string par, ref int priceType, ref int compare, ref long priceError)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                priceType = Convert.ToInt32(temp.PriceType);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析价格条件-PriceType参数解析出错", ex);
                return false;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析价格条件-Compare参数解析出错", ex);
                return false;
            }
            try
            {
                priceError = (long)(Convert.ToDouble(temp.PriceError) * 10000);
            }
            catch (Exception)
            {
            }
            return true;
        }
        #endregion

        #region===股票市场====
        //分析股票市场
        public List<SharesBase> Analysis_SharesMarket(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                string sharesKey = string.Empty;
                int sharesType = 0;
                int sharesMarket = 0;
                bool sharesMarket0 = false;
                bool sharesMarket1 = false;
                if (!Analysis_SharesMarket_BuildPar(par, ref sharesKey, ref sharesType, ref sharesMarket, ref sharesMarket0, ref sharesMarket1))
                {
                    return new List<SharesBase>();
                }

                var shares_session_dic = GetSharesBaseSession();

                foreach (var item in shares_session_dic)
                {
                    if ((sharesMarket == -1 || sharesMarket == item.Value.Market) && ((sharesType == 1 && item.Value.SharesCode.StartsWith(sharesKey)) || (sharesType == 2 && item.Value.SharesName.StartsWith(sharesKey))))
                    {
                        result.Add(new SharesBase
                        {
                            Market = item.Value.Market,
                            SharesCode = item.Value.SharesCode
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_SharesMarket出错", ex);
            }
            return result;
        }

        private bool Analysis_SharesMarket_BuildPar(string par, ref string sharesKey, ref int sharesType, ref int sharesMarket, ref bool sharesMarket0, ref bool sharesMarket1)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                sharesKey = Convert.ToString(temp.SharesKey);
            }
            catch (Exception) { }
            try
            {
                sharesType = Convert.ToInt32(temp.SharesType);
            }
            catch (Exception) { }
            try
            {
                sharesMarket0 = Convert.ToBoolean(temp.SharesMarket0);
            }
            catch (Exception ex)
            { }
            try
            {
                sharesMarket1 = Convert.ToBoolean(temp.SharesMarket1);
            }
            catch (Exception ex)
            { }

            if (string.IsNullOrEmpty(sharesKey))
            {
                Logger.WriteFileLog("分析股票市场-sharesKey参数解析出错", null);
                return false;
            }
            if (sharesType == 0)
            {
                Logger.WriteFileLog("分析股票市场-sharesType参数解析出错", null);
                return false;
            }
            if (!sharesMarket0 && !sharesMarket1)
            {
                Logger.WriteFileLog("分析股票市场-sharesMarket参数解析出错", null);
                return false;
            }
            if (sharesMarket0 && sharesMarket1)
            {
                sharesMarket = -1;
            }
            else if (sharesMarket0)
            {
                sharesMarket = 0;
            }
            else
            {
                sharesMarket = 1;
            }
            return true;
        }
        #endregion

        #region====历史涨跌幅====
        //分析历史涨跌幅数据-批量
        public List<SharesBase> Analysis_HisRiseRate(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int day = 0;
                int type = 0;
                int compare = 0;
                int count = 0;
                int rateCompare = 0;
                int rateCount = 0;
                int limitDay = 0;
                bool flatRise = false;
                bool triPrice = false;
                int priceCompare = 0;
                int priceType = 0;
                long priceError = 0;
                int direct = 2;
                int baseType = 2;
                if (!Analysis_HisRiseRate_BuildPar(par, ref day, ref type, ref compare,ref count, ref rateCompare, ref rateCount, ref limitDay, ref flatRise, ref triPrice, ref priceCompare, ref priceType, ref priceError, ref direct, ref baseType))
                {
                    return new List<SharesBase>();
                }
                DateTime currDate = DateTime.Now.Date;
                if (type == 14 && flatRise)
                {
                    currDate = currDate.AddDays(1);
                }
                if (type == 15 || type == 16)
                {
                    currDate = currDate.AddDays(1);
                    day = day + 1;
                }

                var quotes_date_session=Analysis_HisRiseRate_GetSession(currDate, day, priceCompare, priceType, direct, priceError);

                switch (type) 
                {
                    case 1:
                        result=_analysis_HisRiseRate_1(quotes_date_session,compare,count);
                        break;
                    case 2:
                        result = _analysis_HisRiseRate_2(quotes_date_session, compare, count,triPrice);
                        break;
                    case 3:
                        result = _analysis_HisRiseRate_3(quotes_date_session, compare, count, triPrice);
                        break;
                    case 4:
                        result = _analysis_HisRiseRate_4(quotes_date_session, compare, count);
                        break;
                    case 7:
                        result = _analysis_HisRiseRate_7(quotes_date_session,limitDay);
                        break;
                    case 8:
                        result = _analysis_HisRiseRate_8(quotes_date_session,limitDay);
                        break;
                    case 9:
                        result = _analysis_HisRiseRate_9(quotes_date_session,limitDay);
                        break;
                    case 10:
                        result = _analysis_HisRiseRate_10(quotes_date_session,limitDay);
                        break;
                    case 13:
                        result = _analysis_HisRiseRate_13(quotes_date_session,compare,count,flatRise,rateCompare,rateCount);
                        break;
                    case 14:
                        result = _analysis_HisRiseRate_14(quotes_date_session,compare,count,flatRise);
                        break;
                    case 15:
                        result = _analysis_HisRiseRate_15(quotes_date_session,baseType,15);
                        break;
                    case 16:
                        result = _analysis_HisRiseRate_15(quotes_date_session,baseType,16);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_HisRiseRate出错", ex);
            }
            return result;
        }

        private bool Analysis_HisRiseRate_BuildPar(string par, ref int day, ref int type, ref int compare, ref int count, ref int rateCompare, ref int rateCount, ref int limitDay, ref bool flatRise, ref bool triPrice, ref int priceCompare, ref int priceType, ref long priceError, ref int direct, ref int baseType)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                day = Convert.ToInt32(temp.Day);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析价格条件-Day参数解析出错", ex);
                return false;
            }
            try
            {
                type = Convert.ToInt32(temp.Type);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析价格条件-Type参数解析出错", ex);
                return false;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception) { }
            try
            {
                count = (type == 11 || type == 12 || type == 14) ? (int)(Convert.ToDouble(temp.Rise) * 100) : Convert.ToInt32(temp.Count);
            }
            catch (Exception) { }
            try
            {
                rateCompare = Convert.ToInt32(temp.RateCompare);
            }
            catch (Exception) { }
            try
            {
                rateCount = (int)(Convert.ToDouble(temp.RateCount) * 100);
            }
            catch (Exception) { }
            try
            {
                limitDay = Convert.ToInt32(temp.LimitDay);
            }
            catch (Exception) { }
            try
            {
                triPrice = Convert.ToBoolean(temp.TriPrice);
            }
            catch (Exception) { }
            try
            {
                flatRise = Convert.ToBoolean(temp.FlatRise);
            }
            catch (Exception) { }
            try
            {
                priceCompare = Convert.ToInt32(temp.PriceCompare);
            }
            catch (Exception) { }
            try
            {
                priceType = Convert.ToInt32(temp.PriceType);
            }
            catch (Exception) { }
            try
            {
                priceError = (long)(Convert.ToDouble(temp.PriceError) * 10000);
            }
            catch (Exception) { }
            try
            {
                direct = Convert.ToInt32(temp.Direct);
            }
            catch (Exception) { }
            try
            {
                baseType = Convert.ToInt32(temp.BaseType);
            }
            catch (Exception) { }
            return true;
        }

        private Dictionary<long, List<Shares_Quotes_Date_Info>> Analysis_HisRiseRate_GetSession(DateTime currDate, int day,int priceCompare,int priceType,int direct,long priceError) 
        {
            string sql = string.Format(@"declare @minDate datetime;
  set @minDate=dbo.f_getTradeDate('{0}',-{1});
  select Market,SharesCode,[Date],PresentPrice,OpenedPrice,ClosedPrice,LimitUpPrice,LimitDownPrice,PriceType,TriPriceType,LimitUpCount,LimitUpBombCount,MaxPrice,MinPrice
  from t_shares_quotes_date
  where LastModified<'{0}' and LastModified>@minDate", currDate, day);
            List<Shares_Quotes_Date_Info> resultData = new List<Shares_Quotes_Date_Info>();
            using (var db = new meal_ticketEntities())
            {
                resultData = db.Database.SqlQuery<Shares_Quotes_Date_Info>(sql).ToList();
            }
            Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date_dic = new Dictionary<long, List<Shares_Quotes_Date_Info>>();
            quotes_date_dic = resultData.GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.ToList());

            if (priceCompare==0 || priceType==0)
            {
                return quotes_date_dic;
            }

            Dictionary<long, List<Shares_Quotes_Date_Info>> session = new Dictionary<long, List<Shares_Quotes_Date_Info>>();

            foreach (var item in quotes_date_dic)
            {
                List<Shares_Quotes_Date_Info> tempList = new List<Shares_Quotes_Date_Info>();
                if (direct == 2)
                {
                    var direct_quotes_date = item.Value.OrderByDescending(e => e.Date).ToList();
                    foreach (var quote in direct_quotes_date)
                    {
                        int RiseRate = (quote.ClosedPrice == 0 || quote.PresentPrice == 0) ? 0 : (int)Math.Round((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000, 0);
                        if ((priceType == 1 && priceCompare == 1 && quote.PresentPrice >= quote.LimitUpPrice + priceError) || (priceType == 1 && priceCompare == 2 && quote.PresentPrice <= quote.LimitUpPrice + priceError))
                        {
                            tempList.Add(quote);
                            break;
                        }
                        else if ((priceType == 2 && priceCompare == 1 && quote.PresentPrice >= quote.LimitDownPrice + priceError) || (priceType == 2 && priceCompare == 2 && quote.PresentPrice <= quote.LimitDownPrice + priceError))
                        {
                            tempList.Add(quote);
                            break;
                        }
                        else if ((priceType == 3 && priceCompare == 1 && RiseRate >= priceError) || (priceType == 3 && priceCompare == 2 && RiseRate <= priceError))
                        {
                            tempList.Add(quote);
                            break;
                        }
                        tempList.Add(quote);
                    }
                    session.Add(item.Key, tempList);
                }
                else 
                {
                    var direct_quotes_date = item.Value.OrderBy(e => e.Date).ToList();
                    bool isAdd = false;
                    foreach (var quote in direct_quotes_date)
                    {
                        if (!isAdd)
                        {
                            int RiseRate = (quote.ClosedPrice == 0 || quote.PresentPrice == 0) ? 0 : (int)Math.Round((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000, 0);
                            if ((priceType == 1 && priceCompare == 1 && quote.PresentPrice >= quote.LimitUpPrice + priceError) || (priceType == 1 && priceCompare == 2 && quote.PresentPrice <= quote.LimitUpPrice + priceError))
                            {
                                isAdd = true;
                            }
                            else if ((priceType == 2 && priceCompare == 1 && quote.PresentPrice >= quote.LimitDownPrice + priceError) || (priceType == 2 && priceCompare == 2 && quote.PresentPrice <= quote.LimitDownPrice + priceError))
                            {
                                isAdd = true;
                            }
                            else if ((priceType == 3 && priceCompare == 1 && RiseRate >= priceError) || (priceType == 3 && priceCompare == 2 && RiseRate <= priceError))
                            {
                                isAdd = true;
                            }
                        }
                        if (isAdd)
                        {
                            tempList.Add(quote);
                        }
                    }
                    session.Add(item.Key, tempList);
                }
            }
            return session;
        }

        //历史涨跌幅-没涨停次数
        private List<SharesBase> _analysis_HisRiseRate_1(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int compare,int count) 
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = item.Value.Where(e => e.PriceType != 1).Count();
                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6,'0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-涨停次数
        private List<SharesBase> _analysis_HisRiseRate_2(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int compare, int count,bool triPrice)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = 0;
                if (triPrice)
                {
                    tempCount = item.Value.Where(e=>e.TriPriceType==1).Count();
                }
                else
                {
                    tempCount = item.Value.Where(e => e.PriceType == 1).Count();
                }

                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-跌停次数
        private List<SharesBase> _analysis_HisRiseRate_3(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int compare, int count, bool triPrice)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = 0;
                if (triPrice)
                {
                    tempCount = item.Value.Where(e => e.TriPriceType == 2).Count();
                }
                else
                {
                    tempCount = item.Value.Where(e => e.PriceType == 2).Count();
                }

                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-炸板次数
        private List<SharesBase> _analysis_HisRiseRate_4(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int compare, int count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = item.Value.Where(e => e.PriceType != 1 && e.LimitUpCount > 0).Count();

                if ((compare == 1 && tempCount >= count) || (compare == 2 && tempCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-包含连续涨停
        private List<SharesBase> _analysis_HisRiseRate_7(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date,int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续涨停天数
                foreach (var quote in item.Value)
                {
                    if (quote.PriceType == 1)
                    {
                        tempi++;
                    }
                    else
                    {
                        tempi = 0;
                    }
                    if (tempi >= limitDay)
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                        break;
                    }
                }
            }
            return result;
        }
        //历史涨跌幅-排除连续涨停
        private List<SharesBase> _analysis_HisRiseRate_8(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续涨停天数
                bool isContinue = false;
                foreach (var quote in item.Value)
                {
                    if (quote.PriceType == 1)
                    {
                        tempi++;
                    }
                    else
                    {
                        tempi = 0;
                    }
                    if (tempi >= limitDay)
                    {
                        isContinue = true;
                        break;
                    }
                }
                if (isContinue)
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-包含连续跌停
        private List<SharesBase> _analysis_HisRiseRate_9(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续跌停天数
                foreach (var quote in item.Value)
                {
                    if (quote.PriceType == 2)
                    {
                        tempi++;
                    }
                    else
                    {
                        tempi = 0;
                    }
                    if (tempi >= limitDay)
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                        break;
                    }
                }
            }
            return result;
        }
        //历史涨跌幅-排除连续跌停
        private List<SharesBase> _analysis_HisRiseRate_10(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续跌停天数
                bool isContinue = false;
                foreach (var quote in item.Value)
                {
                    if (quote.PriceType == 2)
                    {
                        tempi++;
                    }
                    else
                    {
                        tempi = 0;
                    }
                    if (tempi >= limitDay)
                    {
                        isContinue = true;
                        break;
                    }
                }
                if (isContinue)
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-每日涨跌幅
        private List<SharesBase> _analysis_HisRiseRate_13(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int compare, int count, bool flatRise, int rateCompare, int rateCount)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int i = 0;
                foreach (var quote in item.Value)
                {
                    if (quote.PresentPrice <= 0 || quote.ClosedPrice <= 0)
                    {
                        continue;
                    }
                    int rate = (int)Math.Round((quote.PresentPrice - quote.ClosedPrice) * 1.0 / quote.ClosedPrice * 10000, 0);
                    if (!flatRise && rate == 0)
                    {
                        continue;
                    }
                    if (rateCompare == 1 && rate >= rateCount)
                    {
                        i++;
                        continue;
                    }
                    if (rateCompare == 2 && rate <= rateCount)
                    {
                        i++;
                        continue;
                    }
                }
                if ((compare == 1 && i >= count) || (compare == 2 && i <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-总涨跌幅
        private List<SharesBase> _analysis_HisRiseRate_14(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date, int compare, int count,bool flatRise)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                long closePrice = item.Value.OrderBy(e => e.Date).Select(e => e.ClosedPrice).FirstOrDefault();
                long presentPrice = item.Value.OrderByDescending(e => e.Date).Select(e => e.PresentPrice).FirstOrDefault();
                if (closePrice <= 0 || presentPrice <= 0)
                {
                    continue;
                }
                int rate = (int)Math.Round((presentPrice - closePrice) * 1.0 / closePrice * 10000,0);
                if (!flatRise && rate == 0)
                {
                    continue;
                }
                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        //历史涨跌幅-新高/新低
        private List<SharesBase> _analysis_HisRiseRate_15(Dictionary<long, List<Shares_Quotes_Date_Info>> quotes_date,int baseType,int type)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                var currInfo = item.Value.OrderByDescending(e => e.Date).FirstOrDefault();
                var quotes_date_his = item.Value.Where(e => e.Date != currInfo.Date).ToList();
                if (quotes_date_his.Count() <= 0)
                {
                    continue;
                }
                long sourcePrice = currInfo.PresentPrice;
                long disMaxPrice = 0;
                long disMinPrice = 0;
                if (baseType == 1)
                {
                    disMaxPrice = quotes_date_his.Max(e => e.OpenedPrice);
                    disMinPrice = quotes_date_his.Min(e => e.OpenedPrice);
                }
                else if (baseType == 3)
                {
                    disMaxPrice = quotes_date_his.Max(e => e.MaxPrice);
                    disMinPrice = quotes_date_his.Min(e => e.MaxPrice);
                }
                else if (baseType == 4)
                {
                    disMaxPrice = quotes_date_his.Max(e => e.MinPrice);
                    disMinPrice = quotes_date_his.Min(e => e.MinPrice);
                }
                else
                {
                    disMaxPrice = quotes_date_his.Max(e => e.PresentPrice);
                    disMinPrice = quotes_date_his.Min(e => e.PresentPrice);
                }

                if ((type == 15 && sourcePrice >= disMaxPrice) || (type == 16 && sourcePrice <= disMinPrice))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        #endregion

        #region====当日涨跌幅====
        //分析当日涨跌幅
        public List<SharesBase> Analysis_TodayRiseRate(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int type = 0;
                int compare = 0;
                long count = 0;
                bool triPrice = false;
                int firstDay = 0;
                int secondDay = 0;
                double multiple = 0;
                int compare2 = 0;
                int dayShortageType = 0;
                int dealLineType = 0;
                bool isCross = false;

                if (!Analysis_TodayRiseRate_BuildPar(par, ref type, ref compare, ref count, ref triPrice, ref firstDay, ref secondDay, ref multiple, ref compare2, ref dayShortageType, ref dealLineType, ref isCross))
                {
                    return new List<SharesBase>();
                }

                //获取股票最后一条行情缓存
                var quotes_date_last_dic = GetSharesQuotesLastSession();

                switch (type)
                {
                    case 1:
                        result = _analysis_TodayRiseRate_1(quotes_date_last_dic, triPrice, compare, count);
                        break;
                    case 2:
                        result = _analysis_TodayRiseRate_2(quotes_date_last_dic, triPrice, compare, count);
                        break;
                    case 3:
                        result = _analysis_TodayRiseRate_3(quotes_date_last_dic,compare, count);
                        break;
                    case 5:
                        result = _analysis_TodayRiseRate_5(quotes_date_last_dic, compare, count);
                        break;
                    case 6:
                        result = _analysis_TodayRiseRate_6(quotes_date_last_dic,compare, count);
                        break;
                    case 7:
                        result = _analysis_TodayRiseRate_7(quotes_date_last_dic,firstDay,secondDay,dayShortageType,compare,count,multiple,compare2);
                        break;
                    case 8:
                        result = _analysis_TodayRiseRate_8(quotes_date_last_dic, firstDay, secondDay, dayShortageType, compare, count, multiple, compare2);
                        break;
                    case 10:
                        result = _analysis_TodayRiseRate_10(quotes_date_last_dic,dealLineType,isCross,compare,count);
                        break;
                    case 11:
                        result = _analysis_TodayRiseRate_11(quotes_date_last_dic,compare,count);
                        break;
                    default:
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_TodayRiseRate出错了", ex);
            }
            return result;
        }

        private bool Analysis_TodayRiseRate_BuildPar(string par, ref int type, ref int compare, ref long count, ref bool triPrice, ref int firstDay, ref int secondDay, ref double multiple, ref int compare2, ref int dayShortageType, ref int dealLineType, ref bool isCross)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                type = Convert.ToInt32(temp.Type);
            }
            catch (Exception ex) 
            {
                Logger.WriteFileLog("分析当日涨跌幅-Type参数错误", ex);
                return false;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception) { }
            try
            {
                count = (type == 5 || type == 6 || type == 10) ? (int)(Convert.ToDouble(temp.Count) * 100) : Convert.ToInt64(temp.Count);
            }
            catch (Exception) { }
            try
            {
                triPrice = Convert.ToBoolean(temp.TriPrice);
            }
            catch (Exception) { }
            try
            {
                firstDay = Convert.ToInt32(temp.FirstDay);
            }
            catch (Exception) { }
            try
            {
                secondDay = Convert.ToInt32(temp.SecondDay);
            }
            catch (Exception) { }
            try
            {
                multiple = Convert.ToDouble(temp.Multiple);
            }
            catch (Exception) { }
            try
            {
                compare2 = Convert.ToInt32(temp.Compare2);
            }
            catch (Exception) { }
            try
            {
                dayShortageType = Convert.ToInt32(temp.DayShortageType);
            }
            catch (Exception) { }
            try
            {
                dealLineType = Convert.ToInt32(temp.DealLineType);
            }
            catch (Exception) { }
            try
            {
                isCross = Convert.ToBoolean(temp.IsCross);
            }
            catch (Exception) { }
            return true;
        }

        private List<SharesBase> _analysis_TodayRiseRate_1(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date,bool triPrice,int compare,long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int currCount = triPrice ? item.Value.shares_quotes_info.TriLimitUpCount : item.Value.shares_quotes_info.LimitUpCount;
                if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_2(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date, bool triPrice, int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int currCount = triPrice ? item.Value.shares_quotes_info.TriLimitDownCount : item.Value.shares_quotes_info.LimitDownCount;
                if ((compare == 1 && currCount >= count) || (compare == 2 && currCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_3(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date, int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                if ((compare == 1 && item.Value.shares_quotes_info.LimitUpBombCount >= count) || (compare == 2 && item.Value.shares_quotes_info.LimitUpBombCount <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_5(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date,int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                long openPrice = item.Value.shares_quotes_info.OpenedPrice;
                long closePrice = item.Value.shares_quotes_info.YestodayClosedPrice;
                if (closePrice <= 0 || openPrice <= 0)
                {
                    continue;
                }
                int rate = (int)((openPrice - closePrice) * 1.0 / closePrice * 10000);
                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_6(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date, int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                long maxPrice = item.Value.shares_quotes_info.MaxPrice;
                long minPrice = item.Value.shares_quotes_info.MinPrice;
                long closePrice = item.Value.shares_quotes_info.YestodayClosedPrice;
                if (closePrice <= 0 || maxPrice <= 0 || minPrice <= 0)
                {
                    continue;
                }
                int rate = (int)((maxPrice - minPrice) * 1.0 / closePrice * 10000);
                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_7(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date, int firstDay, int secondDay,int dayShortageType, int compare, long count,double multiple,int compare2)
        {
            List<SharesBase> result = new List<SharesBase>();
            string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
set @currTime=getdate();
set @currDate=convert(varchar(10), @currTime, 23)

declare @firstDayTime datetime,@secondDayTime datetime;
set @firstDayTime=dbo.f_getTradeDate(@currDate,-{0});
set @secondDayTime=dbo.f_getTradeDate(@currDate,-{1});

select Market,SharesCode,convert(bigint,avg(TotalAmount)) AvgTotalAmount,count(*) DateCount
from t_shares_quotes_date with(nolock)
where LastModified>@secondDayTime and LastModified<@firstDayTime
group by Market,SharesCode", firstDay <= 0 ? 0 : firstDay - 1, secondDay);
            Dictionary<long, Shares_Deal_Info> deal_dic = new Dictionary<long, Shares_Deal_Info>();
            using (var db = new meal_ticketEntities())
            {
                deal_dic = db.Database.SqlQuery<Shares_Deal_Info>(sql).ToList().ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }

            foreach (var item in quotes_date)
            {
                long key = long.Parse(item.Value.shares_quotes_info.SharesCode) * 10 + item.Value.shares_quotes_info.Market;
                if (!deal_dic.ContainsKey(key))
                {
                    continue;
                }
                var deal = deal_dic[key];

                int quotesCount = deal.DateCount;
                if (dayShortageType == 2 && quotesCount < firstDay)
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                    continue;
                }
                if (dayShortageType == 3 && quotesCount < firstDay)
                {
                    continue;
                }

                long avgResult = deal.AvgTotalAmount;
                if ((compare == 1 && item.Value.shares_quotes_info.TotalAmount >= avgResult * multiple) || (compare == 2 && item.Value.shares_quotes_info.TotalAmount <= avgResult * multiple))
                {
                    if ((compare2 == 1 && avgResult >= count * 10000) || (compare2 == 2 && avgResult <= count * 10000))
                    {
                        result.Add(new SharesBase
                        {
                            Market = item.Value.shares_quotes_info.Market,
                            SharesCode = item.Value.shares_quotes_info.SharesCode
                        });
                    }
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_8(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date, int firstDay, int secondDay, int dayShortageType, int compare, long count, double multiple, int compare2)
        {
            List<SharesBase> result = new List<SharesBase>();
            string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
set @currTime=getdate();
set @currDate=convert(varchar(10), @currTime, 23)

declare @firstDayTime datetime,@secondDayTime datetime;
set @firstDayTime=dbo.f_getTradeDate(@currDate,-{0});
set @secondDayTime=dbo.f_getTradeDate(@currDate,-{1});

select Market,SharesCode,convert(bigint,avg(TotalCount)) AvgTotalCount,count(*) DateCount
from t_shares_quotes_date with(nolock)
where LastModified>@secondDayTime and LastModified<@firstDayTime
group by Market,SharesCode", firstDay <= 0 ? 0 : firstDay - 1, secondDay);
            Dictionary<long, Shares_Deal_Info> deal_dic = new Dictionary<long, Shares_Deal_Info>();
            using (var db = new meal_ticketEntities())
            {
                deal_dic = db.Database.SqlQuery<Shares_Deal_Info>(sql).ToList().ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }

            foreach (var item in quotes_date)
            {
                long key = long.Parse(item.Value.shares_quotes_info.SharesCode) * 10 + item.Value.shares_quotes_info.Market;
                if (!deal_dic.ContainsKey(key))
                {
                    continue;
                }
                var deal = deal_dic[key];

                int quotesCount = deal.DateCount;
                if (dayShortageType == 2 && quotesCount < firstDay)
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                    continue;
                }
                if (dayShortageType == 3 && quotesCount < firstDay)
                {
                    continue;
                }

                long avgResult = deal.AvgTotalCount;
                if ((compare == 1 && item.Value.shares_quotes_info.TotalCount >= avgResult * multiple) || (compare == 2 && item.Value.shares_quotes_info.TotalCount <= avgResult * multiple))
                {
                    if ((compare2 == 1 && avgResult * 100 >= count) || (compare2 == 2 && avgResult * 100 <= count))
                    {
                        result.Add(new SharesBase
                        {
                            Market = item.Value.shares_quotes_info.Market,
                            SharesCode = item.Value.shares_quotes_info.SharesCode
                        });
                    }
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_10(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date,int dealLineType,bool isCross, int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                long openPrice = item.Value.shares_quotes_info.OpenedPrice;
                long currPrice = item.Value.shares_quotes_info.ClosedPrice;
                if (openPrice <= 0 || currPrice <= 0)
                {
                    continue;
                }
                int rate = (int)Math.Round((currPrice - openPrice) * 1.0 / openPrice*10000,0);

                if (dealLineType == 1 && ((isCross && rate == 0) || rate > 0) && ((compare == 1 && rate >= count) || (compare == 2 && rate <= count) || compare==0))
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
                else if (dealLineType == 2 && ((isCross && rate == 0) || rate < 0) && ((compare == 1 && (-rate) >= count) || (compare == 2 && (-rate) <= count) || compare == 0))
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> _analysis_TodayRiseRate_11(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_date, int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int rateNow = item.Value.RateNow;
                if ((compare == 1 && rateNow >= count * 100) || (compare == 2 && rateNow <= count * 100))
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        #endregion

        #region====板块涨跌幅====
        //分析板块涨跌幅
        public List<SharesBase> Analysis_PlateRiseRate(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                List<SharesBase_Session> resultList = new List<SharesBase_Session>();
                int compare = 0;
                int riseRate = 0;
                int connect = 0;
                List<long> plateList = new List<long>();
                if (!Analysis_PlateRiseRate_BuildPar(par, ref compare, ref riseRate, ref connect, ref plateList))
                {
                    return new List<SharesBase>();
                }

                result = _analysis_PlateRiseRate(compare, riseRate, connect, plateList);
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_PlateRiseRate出错了", ex);
            }
            return result;
        }

        private bool Analysis_PlateRiseRate_BuildPar(string par,ref int compare,ref int riseRate,ref int connect,ref List<long> plateList)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                compare = temp.Compare;
            }
            catch (Exception ex) { }
            try
            {
                riseRate = Convert.ToInt32(Convert.ToDouble(temp.RiseRate) * 100);
            }
            catch (Exception ex) { }
            try
            {
                connect = temp.PlateInfoJson.Connect;
            }
            catch (Exception ex) { }
            try
            {
                plateList = temp.PlateInfoJson.PlateList.ToObject<List<long>>();
            }
            catch (Exception ex) { }

            return true;
        }

        private List<SharesBase> _analysis_PlateRiseRate(int compare,int riseRate,int connect,List<long> plateList)
        {
            List<SharesBase> result = new List<SharesBase>();

            var totalPlateList = (from item in Singleton.Instance._SharesPlateSession.GetSessionData()
                                  join item2 in Singleton.Instance._SharesPlateQuotesSession.GetSessionData() on item.PlateId equals item2.PlateId
                                  where plateList.Contains(item.PlateId)
                                  select new { item, item2 }).ToList();
            var sharesPlate = (from item in Singleton.Instance._SharesPlateRelSession.GetSessionData()
                               where plateList.Contains(item.PlateId)
                               select item).ToList();
            var sharesList = Singleton.Instance._SharesBaseSession.GetSessionData();

            var tempResult = (from item in sharesList
                              join item2 in sharesPlate on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                              join item3 in totalPlateList on item2.PlateId equals item3.item.PlateId
                              where compare == 0 || (compare == 1 && item3.item2.RiseRate >= riseRate) || (compare == 2 && item3.item2.RiseRate <= riseRate)
                              group item by item into g
                              select new
                              {
                                  Market = g.Key.Market,
                                  SharesCode = g.Key.SharesCode,
                                  Count = g.Count()
                              }).ToList();
            if (connect == 1)//或
            {
                result = (from item in tempResult
                          select new SharesBase
                          {
                              Market = item.Market,
                              SharesCode = item.SharesCode
                          }).ToList();
            }
            else if (connect == 2)//且
            {
                int plateCount = totalPlateList.Count();
                result = (from item in tempResult
                          where item.Count >= plateCount
                          select new SharesBase
                          {
                              Market = item.Market,
                              SharesCode = item.SharesCode
                          }).ToList();
            }
            else
            {
                return new List<SharesBase>();
            }
            return result;
        }
        #endregion

        #region====当前价格====
        //分析按当前价格
        public List<SharesBase> Analysis_CurrentPrice(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int riseType = 0;
                int compare = 0;
                long count = 0;
                if (!Analysis_CurrentPrice_BuildPar(par, ref riseType, ref compare, ref count))
                {
                    return new List<SharesBase>();
                }

                var shares_quotes_last_dic = GetSharesQuotesLastSession();

                switch (riseType)
                {
                    case 1:
                        result = Analysis_CurrentPrice_1(shares_quotes_last_dic);
                        break;
                    case 2:
                        result = Analysis_CurrentPrice_2(shares_quotes_last_dic);
                        break;
                    case 3:
                        result = Analysis_CurrentPrice_3(shares_quotes_last_dic);
                        break;
                    case 4:
                        result = Analysis_CurrentPrice_4(shares_quotes_last_dic);
                        break;
                    case 5:
                        result = Analysis_CurrentPrice_5(shares_quotes_last_dic,compare,count);
                        break;
                    case 6:
                        result = Analysis_CurrentPrice_6(shares_quotes_last_dic,compare,count);
                        break;
                    case 7:
                        result = Analysis_CurrentPrice_7(shares_quotes_last_dic, compare, count);
                        break;
                    default:
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                result=new List<SharesBase>();
                Logger.WriteFileLog("Analysis_CurrentPrice出错了", ex);
            }
            return result;
        }

        private bool Analysis_CurrentPrice_BuildPar(string par,ref int riseType,ref int compare,ref long count)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                riseType = Convert.ToInt32(temp.RiseType);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按当前价格-riseType参数错误", ex);
                return false;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception)
            {
            }
            try
            {
                count = (long)(Convert.ToDouble(temp.Count) * 10000);
            }
            catch (Exception)
            {
            }
            return true;
        }

        private List<SharesBase> Analysis_CurrentPrice_1(Dictionary<long,Shares_Quotes_Session_Info_Last> quotes_last)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_last)
            {
                if (item.Value.shares_quotes_info.TriPriceType == 1)
                {
                    result.Add(new SharesBase 
                    {
                        Market= item.Value.shares_quotes_info.Market,
                        SharesCode= item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> Analysis_CurrentPrice_2(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_last)
            {
                if (item.Value.shares_quotes_info.TriPriceType == 2)
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> Analysis_CurrentPrice_3(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_last)
            {
                if (item.Value.shares_quotes_info.PriceType == 1)
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> Analysis_CurrentPrice_4(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_last)
            {
                if (item.Value.shares_quotes_info.PriceType == 2)
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> Analysis_CurrentPrice_5(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last,int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_last)
            {
                if ((compare == 1 && item.Value.shares_quotes_info.ClosedPrice >= count) || (compare == 2 && item.Value.shares_quotes_info.ClosedPrice <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> Analysis_CurrentPrice_6(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last,int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            var shares_base_session=GetSharesBaseSession();
            foreach (var item in quotes_last)
            {
                //查询流通股
                if (!shares_base_session.ContainsKey(item.Key))
                {
                    continue;
                }
                var sharesInfo = shares_base_session[item.Key];

                long circulatingCapital = sharesInfo.CirculatingCapital;
                if ((compare == 1 && item.Value.shares_quotes_info.ClosedPrice * circulatingCapital >= count) || (compare == 2 && item.Value.shares_quotes_info.ClosedPrice * circulatingCapital <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        private List<SharesBase> Analysis_CurrentPrice_7(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last, int compare, long count)
        {
            List<SharesBase> result = new List<SharesBase>();
            var shares_base_session = GetSharesBaseSession();
            foreach (var item in quotes_last)
            {
                //查询总股本

                if (!shares_base_session.ContainsKey(item.Key))
                {
                    continue;
                }
                var sharesInfo = shares_base_session[item.Key];

                long totalCapital = sharesInfo.TotalCapital;
                if ((compare == 1 && item.Value.shares_quotes_info.ClosedPrice * totalCapital >= count) || (compare == 2 && item.Value.shares_quotes_info.ClosedPrice * totalCapital <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = item.Value.shares_quotes_info.Market,
                        SharesCode = item.Value.shares_quotes_info.SharesCode
                    });
                }
            }
            return result;
        }
        #endregion

        #region====均线价格====
        //分析按均线价格
        public List<SharesBase> Analysis_ReferAverage(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int compare = 0;
                int day1 = 0;
                int day2 = 0;
                double count = 0;
                int upOrDown = 0;
                int dayShortageType = 1;

                if (!Analysis_ReferAverage_BuildPar(par, ref compare, ref day1, ref day2, ref count, ref upOrDown, ref dayShortageType))
                {
                    return new List<SharesBase>();
                }

                var quotes_date_list_dic = Analysis_ReferAverage_GetSession(day1,day2);

                result=_analysis_ReferAverage(quotes_date_list_dic, day1, day2, dayShortageType, compare, count, upOrDown);
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_ReferAverage出错了", ex);
            }
            return result;
        }

        private bool Analysis_ReferAverage_BuildPar(string par, ref int compare, ref int day1, ref int day2, ref double count, ref int upOrDown, ref int dayShortageType)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);

            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按均线价格-Compare参数错误", ex);
                return false;
            }
            try
            {
                day1 = Convert.ToInt32(temp.Day1);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按均线价格-day1参数错误", ex);
                return false;
            }

            try
            {
                day2 = Convert.ToInt32(temp.Day2);
                day2 = day2 + 1;//多取一天
            }
            catch (Exception)
            {

            }
            try
            {
                upOrDown = Convert.ToInt32(temp.UpOrDown);
            }
            catch (Exception)
            {

            }
            try
            {
                dayShortageType = Convert.ToInt32(temp.DayShortageType);
            }
            catch (Exception)
            {
            }
            try
            {
                count = Convert.ToDouble(temp.Count);
            }
            catch (Exception)
            {
            }
            return true;
        }

        private Dictionary<long,List<Shares_AvgLine_Info>> Analysis_ReferAverage_GetSession(int day1,int day2) 
        {
            string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
  set @currTime=getdate();
  set @currDate=convert(varchar(10), @currTime, 23)

  declare @minDate datetime;
  set @minDate=dbo.f_getTradeDate(@currDate,-{0});
 
  select Market,SharesCode,PresentPrice,OpenedPrice,MaxPrice,MinPrice,LastModified,[Date]
  from t_shares_quotes_date
  where LastModified>@minDate", day1 + day2);
            Dictionary<long, List<Shares_AvgLine_Info>> quotes_date_dic = new Dictionary<long, List<Shares_AvgLine_Info>>();
            using (var db = new meal_ticketEntities())
            {
                quotes_date_dic = db.Database.SqlQuery<Shares_AvgLine_Info>(sql).ToList().GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.ToList());
            }
            return quotes_date_dic;
        }

        private List<SharesBase> _analysis_ReferAverage(Dictionary<long, List<Shares_AvgLine_Info>> quotes_date_list_dic, int day1, int day2, int dayShortageType, int compare, double count, int upOrDown)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date_list_dic)
            {
                var quotes = item.Value.OrderByDescending(e => e.Date).ToList();
                if (quotes.Count() <= 0)
                {
                    continue;
                }
                var presentInfo = quotes.FirstOrDefault();
                long presentPrice = presentInfo.PresentPrice;
                if (presentPrice <= 0)
                {
                    continue;
                }
                //计算均线价格
                var list1 = quotes.Take(day1).ToList();
                if (list1.Count() <= 0)
                {
                    continue;
                }
                if (list1.Count() < day1)
                {
                    if (dayShortageType == 2)
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                        continue;
                    }
                    if (dayShortageType == 3)
                    {
                        continue;
                    }
                }
                long averagePrice = (long)list1.Average(e => e.PresentPrice);
                if (count != 0)//计算偏差
                {
                    averagePrice = (long)(averagePrice * (count / 100) + averagePrice);
                }

                if ((compare == 1 && presentPrice >= averagePrice) || (compare == 2 && presentPrice <= averagePrice))
                {
                    bool IsSuccess = true;
                    if (day2 >= 2)//计算每日均线
                    {
                        long lastAveragePrice = 0;
                        for (int i = 0; i < day2; i++)
                        {
                            long tempAveragePrice;
                            var list2 = quotes.Skip(i).Take(day1).ToList();
                            if (list2.Count() <= 0)
                            {
                                tempAveragePrice = 0;
                            }
                            else
                            {
                                tempAveragePrice = (long)list2.Average(e => e.PresentPrice);
                            }
                            if (upOrDown == 1)
                            {
                                //向上，则当前必须<=前一个数据
                                if (tempAveragePrice > lastAveragePrice && lastAveragePrice != 0)
                                {
                                    IsSuccess = false;
                                    break;
                                }
                                lastAveragePrice = tempAveragePrice;
                                continue;
                            }
                            else if (upOrDown == 2)
                            {
                                //向下，则当前必须>=前一个数据
                                if (tempAveragePrice < lastAveragePrice && lastAveragePrice != 0)
                                {
                                    IsSuccess = false;
                                    break;
                                }
                                lastAveragePrice = tempAveragePrice;
                                continue;
                            }
                            else
                            {
                                IsSuccess = false;
                                break;
                            }
                        }
                    }
                    if (IsSuccess)
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                    }
                }

            }
            return result;
        }
        #endregion

        #region====参照价格====
        //分析按参照价格
        public List<SharesBase> Analysis_ReferPrice(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int compare = 0;
                int day = 0;
                double count = 0;
                int priceType = 0;
                if (!Analysis_ReferPrice_BuildPar(par,ref compare,ref day,ref count,ref priceType))
                {
                    return new List<SharesBase>();
                }
                var quotes_date_dic = Analysis_ReferPrice_GetSession(day);

                result=_analysis_ReferPrice(quotes_date_dic,priceType,compare,count);
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_ReferPrice出错了", ex);
            }
            return result;
        }

        private bool Analysis_ReferPrice_BuildPar(string par, ref int compare, ref int day, ref double count, ref int priceType) 
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);


            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按参照价格-Compare参数错误", ex);
                return false;
            }
            try
            {
                day = Convert.ToInt32(temp.Day);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按参照价格-Day参数错误", ex);
                return false;
            }
            try
            {
                priceType = Convert.ToInt32(temp.PriceType);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析按参照价格-PriceType参数错误", ex);
                return false;
            }
            try
            {
                count = Convert.ToDouble(temp.Count);
            }
            catch (Exception) { }
            try
            {
                count = Convert.ToDouble(temp.Count);
            }
            catch (Exception) { }
            return true;
        }

        private Dictionary<long,List<Shares_AvgLine_Info>> Analysis_ReferPrice_GetSession(int day)
        {
            Dictionary<long, List<Shares_AvgLine_Info>> quotes_date_dic = new Dictionary<long, List<Shares_AvgLine_Info>>();
            string sql = string.Format(@"declare @currTime datetime,@currDate datetime;
  set @currTime=getdate();
  set @currDate=convert(varchar(10), @currTime, 23)

  declare @minDate datetime;
  set @minDate=dbo.f_getTradeDate(@currDate,-{0});
 
  select Market,SharesCode,PresentPrice,OpenedPrice,MaxPrice,MinPrice,LastModified,[Date]
  from t_shares_quotes_date t with(nolock)
  where t.LastModified>@minDate", day);
            using (var db = new meal_ticketEntities())
            {
                quotes_date_dic = db.Database.SqlQuery<Shares_AvgLine_Info>(sql).ToList().GroupBy(e=>new { e.Market,e.SharesCode}).ToDictionary(k=>long.Parse(k.Key.SharesCode)*10+k.Key.Market,v=>v.ToList());
            }
            return quotes_date_dic;
        }

        private List<SharesBase> _analysis_ReferPrice(Dictionary<long, List<Shares_AvgLine_Info>> quotes_date_dic,int priceType,int compare,double count) 
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date_dic)
            {
                var presentInfo = item.Value.OrderByDescending(e => e.Date).FirstOrDefault();
                long presentPrice = presentInfo.PresentPrice;
                if (presentPrice <= 0)
                {
                    continue;
                }

                var lastInfo = item.Value.Where(e => e.LastModified < presentInfo.LastModified).ToList();
                if (lastInfo.Count() <= 0)
                {
                    continue;
                }
                long realPrice;
                switch (priceType)
                {
                    case 1:
                        realPrice = compare == 1 ? lastInfo.Max(e => e.OpenedPrice) : lastInfo.Min(e => e.OpenedPrice);
                        break;
                    case 2:
                        realPrice = compare == 1 ? lastInfo.Max(e => e.PresentPrice) : lastInfo.Min(e => e.PresentPrice);
                        break;
                    case 3:
                        realPrice = compare == 1 ? lastInfo.Max(e => e.MaxPrice) : lastInfo.Min(e => e.MaxPrice);
                        break;
                    case 4:
                        realPrice = compare == 1 ? lastInfo.Max(e => e.MinPrice) : lastInfo.Min(e => e.MinPrice);
                        break;
                    default:
                        continue;
                }

                if (count != 0)//计算偏差
                {
                    realPrice = (long)(realPrice * (count / 100) + realPrice);
                }

                if ((compare == 1 && presentPrice >= realPrice) || (compare == 2 && presentPrice <= realPrice))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }
            }
            return result;
        }
        #endregion

        #region====买卖单占比====
        //分析买卖单占比
        public List<SharesBase> Analysis_BuyOrSellCount(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int type = 0;
                int compare = 0;
                double count = 0;
                int countType = 0;
                if (!Analysis_BuyOrSellCount_BuildPar(par, ref type, ref compare, ref count, ref countType))
                {
                    return new List<SharesBase>();
                }

                var quotes_dic = Analysis_BuyOrSellCount_GetSession();

                result = _analysis_BuyOrSellCount(quotes_dic,type, countType, compare, count);


            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("Analysis_BuyOrSellCount_New_Batch出错了", ex);
                result = new List<SharesBase>();
            }
            return result;
        }

        private bool Analysis_BuyOrSellCount_BuildPar(string par,ref int type,ref int compare,ref double count, ref int countType) 
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                type = Convert.ToInt32(temp.Type);
            }
            catch (Exception ex) 
            {
                Logger.WriteFileLog("分析买卖单占比-Type参数错误", ex);
                return false;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }

            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖单占比-Compare参数错误", ex);
                return false;
            }
            try
            {
                count = Convert.ToDouble(temp.Count);
            }

            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖单占比-Count参数错误", ex);
                return false;
            }
            try
            {
                countType = Convert.ToInt32(temp.CountType);
            }

            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖单占比-CountType参数错误", ex);
                return false;
            }
            return true;
        }

        private Dictionary<long,BuyOrSell_Info> Analysis_BuyOrSellCount_GetSession() 
        {
            string sql = @"select Market,SharesCode,BuyCount1,BuyCount2,BuyCount3,BuyCount4,BuyCount5,SellCount1,SellCount2,SellCount3,SellCount4,SellCount5
  from t_shares_quotes
  where DataType = 0;";
            Dictionary<long, BuyOrSell_Info> result = new Dictionary<long, BuyOrSell_Info>();
            using (var db = new meal_ticketEntities())
            {
                result=db.Database.SqlQuery<BuyOrSell_Info>(sql).ToList().ToDictionary(k => long.Parse(k.SharesCode) * 10 + k.Market, v => v);
            }
            return result;
        }

        private List<SharesBase> _analysis_BuyOrSellCount(Dictionary<long,BuyOrSell_Info> quotes_dic,int type,int countType,int compare, double count) 
        {
            var sharesBase = GetSharesBaseSession();
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_dic)
            {
                if (!sharesBase.ContainsKey(item.Key))
                {
                    continue;
                }
                int realCount;
                switch (type)
                {
                    case 1:
                        realCount = item.Value.BuyCount1;
                        break;
                    case 2:
                        realCount = item.Value.BuyCount2;
                        break;
                    case 3:
                        realCount = item.Value.BuyCount3;
                        break;
                    case 4:
                        realCount = item.Value.BuyCount4;
                        break;
                    case 5:
                        realCount = item.Value.BuyCount5;
                        break;
                    case 6:
                        realCount = item.Value.SellCount1;
                        break;
                    case 7:
                        realCount = item.Value.SellCount2;
                        break;
                    case 8:
                        realCount = item.Value.SellCount3;
                        break;
                    case 9:
                        realCount = item.Value.SellCount4;
                        break;
                    case 10:
                        realCount = item.Value.SellCount5;
                        break;
                    default:
                        continue;
                }
                realCount = realCount * 100;

                if (countType == 1)//流通股百分比
                {
                    //查询流通股
                    long circulatingCapital = sharesBase[item.Key].CirculatingCapital;
                    long setCount = (long)(circulatingCapital * (count / 100));

                    if ((compare == 1 && realCount >= setCount) || (compare == 2 && realCount <= setCount))
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                    }
                }
                else if (countType == 2)//股数
                {
                    if ((compare == 1 && realCount >= count) || (compare == 2 && realCount <= count))
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                    }
                }
                else if (countType == 3)//流通市值百分比
                {

                }

            }
            return result;
        }
        #endregion

        #region====买卖变化速率====
        //分析买卖变化速度
        public List<SharesBase> Analysis_QuotesChangeRate(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int compare = 0;
                int type = 0;
                double count = 0;
                int othercompare = 0;

                if (!Analysis_QuotesChangeRate_BuildPar(par, ref compare, ref type, ref count, ref othercompare))
                {
                    return new List<SharesBase>();
                }

                var data_session_dic=Analysis_QuotesChangeRate_GetSession();
                result=_analysis_QuotesChangeRate(data_session_dic,type,othercompare,compare,count);
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_QuotesChangeRate出错了", ex);
            }
            return result;
        }

        private bool Analysis_QuotesChangeRate_BuildPar(string par,ref int compare,ref int type,ref double count,ref int othercompare) 
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖变化速度-Compare参数错误", ex);
                return false;
            }
            try
            {
                type = Convert.ToInt32(temp.Type);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖变化速度-Type参数错误", ex);
                return false;
            }
            try
            {
                count = Convert.ToDouble(temp.Count);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析买卖变化速度-Count参数错误", ex);
                return false;
            }
            try
            {
                othercompare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception)
            {
            }
            return true;
        }

        private Dictionary<long, List<BuyOrSell_Rate_Info>> Analysis_QuotesChangeRate_GetSession()
        {
            string sql = @"select Market,SharesCode,DataType,PresentPrice,BuyCount1,BuyCount2,BuyCount3,BuyCount4,BuyCount5,BuyPrice1,BuyPrice2,BuyPrice3,BuyPrice4,BuyPrice5,
SellCount1,SellCount2,SellCount3,SellCount4,SellCount5,SellPrice1,SellPrice2,SellPrice3,SellPrice4,SellPrice5
from t_shares_quotes with(nolock)";
            Dictionary<long, List<BuyOrSell_Rate_Info>> quotes_dic = new Dictionary<long, List<BuyOrSell_Rate_Info>>();
            using (var db = new meal_ticketEntities())
            {
                quotes_dic = db.Database.SqlQuery<BuyOrSell_Rate_Info>(sql).ToList().GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.ToList());
            }
            return quotes_dic;
        }

        private List<SharesBase> _analysis_QuotesChangeRate(Dictionary<long, List<BuyOrSell_Rate_Info>> data_session_dic,int type,int othercompare,int compare, double count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in data_session_dic)
            {
                var quotesLast = item.Value.Where(e => e.DataType == 0).FirstOrDefault();
                var quotesPre = item.Value.Where(e => e.DataType == 1).FirstOrDefault();

                int disCount = 0;
                long disPrice = 0;
                int lastCount = 0;
                long lastPrice = 0;
                int tradetype = 0;

                switch (type)
                {
                    case 1:
                        disCount = quotesLast.BuyCount1;
                        disPrice = quotesLast.BuyPrice1;
                        lastCount = quotesPre.BuyCount1;
                        lastPrice = quotesPre.BuyPrice1;
                        tradetype = 1;
                        break;
                    case 2:
                        disCount = quotesLast.BuyCount2;
                        disPrice = quotesLast.BuyPrice2;
                        lastCount = quotesPre.BuyCount2;
                        lastPrice = quotesPre.BuyPrice2;
                        tradetype = 1;
                        break;
                    case 3:
                        disCount = quotesLast.BuyCount3;
                        disPrice = quotesLast.BuyPrice3;
                        lastCount = quotesPre.BuyCount3;
                        lastPrice = quotesPre.BuyPrice3;
                        tradetype = 1;
                        break;
                    case 4:
                        disCount = quotesLast.BuyCount4;
                        disPrice = quotesLast.BuyPrice4;
                        lastCount = quotesPre.BuyCount4;
                        lastPrice = quotesPre.BuyPrice4;
                        tradetype = 1;
                        break;
                    case 5:
                        disCount = quotesLast.BuyCount5;
                        disPrice = quotesLast.BuyPrice5;
                        lastCount = quotesPre.BuyCount5;
                        lastPrice = quotesPre.BuyPrice5;
                        tradetype = 1;
                        break;
                    case 6:
                        disCount = quotesLast.SellCount1;
                        disPrice = quotesLast.SellPrice1;
                        lastCount = quotesPre.SellCount1;
                        lastPrice = quotesPre.SellPrice1;
                        tradetype = 2;
                        break;
                    case 7:
                        disCount = quotesLast.SellCount2;
                        disPrice = quotesLast.SellPrice2;
                        lastCount = quotesPre.SellCount2;
                        lastPrice = quotesPre.SellPrice2;
                        tradetype = 2;
                        break;
                    case 8:
                        disCount = quotesLast.SellCount3;
                        disPrice = quotesLast.SellPrice3;
                        lastCount = quotesPre.SellCount3;
                        lastPrice = quotesPre.SellPrice3;
                        tradetype = 2;
                        break;
                    case 9:
                        disCount = quotesLast.SellCount4;
                        disPrice = quotesLast.SellPrice4;
                        lastCount = quotesPre.SellCount4;
                        lastPrice = quotesPre.SellPrice4;
                        tradetype = 2;
                        break;
                    case 10:
                        disCount = quotesLast.SellCount5;
                        disPrice = quotesLast.SellPrice5;
                        lastCount = quotesPre.SellCount5;
                        lastPrice = quotesPre.SellPrice5;
                        tradetype = 2;
                        break;
                    default:
                        continue;
                }

                long currPrice = quotesLast.PresentPrice;
                if ((othercompare == 1 && disPrice < currPrice) || (othercompare == 2 && disPrice > currPrice))
                {
                    continue;
                }
                if (lastCount <= 0 || lastPrice <= 0)
                {
                    continue;
                }

                double rate = 0;
                //当前价格等于0，速率为-100%
                if (disPrice <= 0)
                {
                    rate = -100;
                }
                //价格相等，计算速率
                else if (lastPrice == disPrice)
                {
                    rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                }
                else if (lastPrice > disPrice)
                {
                    //卖单则为无限大（999999999）
                    if (tradetype == 2)
                    {
                        rate = 999999999;
                    }
                    else
                    {
                        rate = -100;
                    }
                }
                else
                {
                    //买单则为无限大（999999999）
                    if (tradetype == 1)
                    {
                        rate = 999999999;
                    }
                    else
                    {
                        rate = -100;
                    }
                }

                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }

            }
            return result;
        }
        #endregion

        #region====分析五档变化速度====
        //分析五档变化速度
        public List<SharesBase> Analysis_QuotesTypeChangeRate(string par)
        {
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int compare = 0;
                int type = 0;
                double count = 0;
                int priceType = 1; 
                Analysis_QuotesTypeChangeRate_BuildPar(par, ref compare, ref type, ref count, ref priceType);

                var quotes_dic = Analysis_QuotesTypeChangeRate();

                result = _analysis_QuotesTypeChangeRate(quotes_dic,priceType,type,compare,count);
            }
            catch (Exception ex)
            {
                result=new List<SharesBase>();
                Logger.WriteFileLog("分析五档变化速度出错了", ex);
            }
            return result;
        }

        private bool Analysis_QuotesTypeChangeRate_BuildPar(string par,ref int compare,ref int type,ref double count,ref int priceType) 
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);

            try
            {
                type = Convert.ToInt32(temp.Type);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析五档变化速度-Type参数错误", ex);
                return false;
            }

            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception)
            {
            }

            try
            {
                count = Convert.ToDouble(temp.Count);
            }
            catch (Exception)
            {
            }

            try
            {
                priceType = Convert.ToInt32(temp.PriceType);
            }
            catch (Exception)
            {
            }
            return true;
        }

        private Dictionary<long, List<Quotes_Rate_Info>> Analysis_QuotesTypeChangeRate()
        {
            string sql = @"select Market,SharesCode,DataType,PresentPrice,BuyCount1,BuyCount2,BuyCount3,BuyCount4,BuyCount5,BuyPrice1,BuyPrice2,BuyPrice3,BuyPrice4,BuyPrice5,
SellCount1,SellCount2,SellCount3,SellCount4,SellCount5,SellPrice1,SellPrice2,SellPrice3,SellPrice4,SellPrice5,LimitUpPrice,LimitDownPrice
from t_shares_quotes with(nolock)";
            Dictionary<long, List<Quotes_Rate_Info>> quotes_dic = new Dictionary<long, List<Quotes_Rate_Info>>();
            using (var db = new meal_ticketEntities())
            {
                quotes_dic = db.Database.SqlQuery<Quotes_Rate_Info>(sql).ToList().GroupBy(e => new { e.Market, e.SharesCode }).ToDictionary(k => long.Parse(k.Key.SharesCode) * 10 + k.Key.Market, v => v.ToList());
            }
            return quotes_dic;
        }

        private List<SharesBase> _analysis_QuotesTypeChangeRate(Dictionary<long, List<Quotes_Rate_Info>> quotes_dic,int priceType,int type,int compare,double count) 
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_dic)
            {
                var quotesLast = item.Value.Where(e => e.DataType == 0).FirstOrDefault();
                var quotesPre = item.Value.Where(e => e.DataType == 1).FirstOrDefault();
                long currPrice = quotesLast.PresentPrice;
                if (priceType == 2)
                {
                    currPrice = quotesLast.LimitUpPrice;
                }
                else if (priceType == 3)
                {
                    currPrice = quotesLast.LimitDownPrice;
                }

                int disCount = 0;
                int lastCount = 0;

                double rate = 0;
                switch (type)
                {
                    case 1:
                        if (quotesPre.BuyPrice1 == currPrice)
                        {
                            lastCount = quotesPre.BuyCount1;
                        }
                        else if (quotesPre.BuyPrice2 == currPrice)
                        {
                            lastCount = quotesPre.BuyCount2;
                        }
                        else if (quotesPre.BuyPrice3 == currPrice)
                        {
                            lastCount = quotesPre.BuyCount3;
                        }
                        else if (quotesPre.BuyPrice4 == currPrice)
                        {
                            lastCount = quotesPre.BuyCount4;
                        }
                        else if (quotesPre.BuyPrice5 == currPrice)
                        {
                            lastCount = quotesPre.BuyCount5;
                        }
                        else
                        {
                            continue;
                        }
                        if (quotesLast.BuyPrice1 == currPrice)
                        {
                            disCount = quotesLast.BuyCount1;
                        }
                        else if (quotesLast.BuyPrice2 == currPrice)
                        {
                            disCount = quotesLast.BuyCount2;
                        }
                        else if (quotesLast.BuyPrice3 == currPrice)
                        {
                            disCount = quotesLast.BuyCount3;
                        }
                        else if (quotesLast.BuyPrice4 == currPrice)
                        {
                            disCount = quotesLast.BuyCount4;
                        }
                        else if (quotesLast.BuyPrice5 == currPrice)
                        {
                            disCount = quotesLast.BuyCount5;
                        }
                        else
                        {
                            if (quotesLast.BuyPrice1 > currPrice)
                            {
                                rate = 0x0FFFFFFF;
                            }
                            else
                            {
                                rate = -100;
                            }
                        }
                        break;
                    case 2:
                        if (quotesPre.SellPrice1 == currPrice)
                        {
                            lastCount = quotesPre.SellCount1;
                        }
                        else if (quotesPre.SellPrice2 == currPrice)
                        {
                            lastCount = quotesPre.SellCount2;
                        }
                        else if (quotesPre.SellPrice3 == currPrice)
                        {
                            lastCount = quotesPre.SellCount3;
                        }
                        else if (quotesPre.SellPrice4 == currPrice)
                        {
                            lastCount = quotesPre.SellCount4;
                        }
                        else if (quotesPre.SellPrice5 == currPrice)
                        {
                            lastCount = quotesPre.SellCount5;
                        }
                        else
                        {
                            continue;
                        }
                        if (quotesLast.SellPrice1 == currPrice)
                        {
                            disCount = quotesLast.SellCount1;
                        }
                        else if (quotesLast.SellPrice2 == currPrice)
                        {
                            disCount = quotesLast.SellCount2;
                        }
                        else if (quotesLast.SellPrice3 == currPrice)
                        {
                            disCount = quotesLast.SellCount3;
                        }
                        else if (quotesLast.SellPrice4 == currPrice)
                        {
                            disCount = quotesLast.SellCount4;
                        }
                        else if (quotesLast.SellPrice5 == currPrice)
                        {
                            disCount = quotesLast.SellCount5;
                        }
                        else
                        {
                            if (quotesLast.SellPrice1 > currPrice)
                            {
                                rate = -100;
                            }
                            else
                            {
                                rate = 0x0FFFFFFF;
                            }
                        }
                        break;
                    default:
                        continue;
                }

                if (rate == 0)
                {
                    if (lastCount == 0)
                    {
                        continue;
                    }
                    rate = (disCount - lastCount) * 1.0 / lastCount * 100;
                }


                if ((compare == 1 && rate >= count) || (compare == 2 && rate <= count))
                {
                    result.Add(new SharesBase
                    {
                        Market = (int)(item.Key % 10),
                        SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                    });
                }

            }
            return result;
        }
        #endregion
    }
}
