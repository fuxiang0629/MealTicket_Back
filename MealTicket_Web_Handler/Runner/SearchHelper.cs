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

    public class Template_Source_Info 
    {
        public long TemplateId { get; set; }

        public string TemplateContent { get; set; }

        public int TemplateType { get; set; }
    }

    public class SearchMarkResult
    {
        public long TemplateId { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }
    }

    public class SearchHelper
    {
        private static Dictionary<string, List<SharesBase>> searchSessionDic = new Dictionary<string, List<SharesBase>>();
        private static ReaderWriterLock readWriterLock = new ReaderWriterLock();
        private List<SharesBase> GetSearchSession(string key)
        {
            var result = new List<SharesBase>();
            readWriterLock.AcquireReaderLock(-1);
            if (searchSessionDic.ContainsKey(key))
            {
                result = searchSessionDic[key];
            }
            readWriterLock.ReleaseReaderLock();
            return result;
        }
        private void SetSearchSession(string key, List<SharesBase> value) 
        {
            readWriterLock.AcquireWriterLock(-1);
            searchSessionDic[key] = value;
            readWriterLock.ReleaseWriterLock();
        }

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

        private List<SearchMarkResult> SearchMarkResult = new List<SearchMarkResult>();
        private object SearchMarkResultLock = new object();

        /// <summary>
        /// 模板自动搜索
        /// </summary>
        public void SearchMonitor(int type)
        {
            //查询需要搜索的模板
            ThreadMsgTemplate<Template_Source_Info> template_queue = new ThreadMsgTemplate<Template_Source_Info>();
            template_queue.Init();
            if (!BuildSearchTemplateQueue(type,ref template_queue))
            {
                return;
            }
            if (type == 2)
            {
                SearchMarkResult = new List<SearchMarkResult>();
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

            if (type == 2)
            {
                CalDataToDataBase_Type6(SearchMarkResult);
            }
        }

        /// <summary>
        /// 生成需要搜索的模板队列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool BuildSearchTemplateQueue(int type,ref ThreadMsgTemplate<Template_Source_Info> data)
        {
            using (var db = new meal_ticketEntities())
            {
                var searchTemplate = (from item in db.t_sys_conditiontrade_template
                                      join item2 in db.t_sys_conditiontrade_template_search on item.Id equals item2.TemplateId
                                      where item.Status == 1 && ((type==1 && item.Type == 5) || (type==2 &&item.Type==6))
                                      select new Template_Source_Info 
                                      {
                                          TemplateId=item2.TemplateId,
                                          TemplateContent=item2.TemplateContent,
                                          TemplateType=item.Type
                                      }).ToList();
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
            var template_queue = context as ThreadMsgTemplate<Template_Source_Info>;
            do
            {
                Template_Source_Info searchCondition = new Template_Source_Info();
                if (!template_queue.GetMessage(ref searchCondition, true))
                {
                    break;
                }
                var searchList=ParseSearchCondition(searchCondition.TemplateContent);
                var dataResult=DoSearchTemplate(searchList);
                //入库
                if (searchCondition.TemplateType == 5)
                {
                    CalDataToDataBase(dataResult, searchCondition.TemplateId);
                }
                else if (searchCondition.TemplateType == 6)
                {
                    lock (SearchMarkResultLock) 
                    {
                        foreach (var x in dataResult)
                        {
                            SearchMarkResult.Add(new SearchMarkResult 
                            { 
                                Market=x.Market,
                                SharesCode=x.SharesCode,
                                TemplateId= searchCondition.TemplateId
                            });
                        }
                    }
                }
            } while (true);
        }

        public List<SharesBase> DoSearchTemplate(List<searchInfo> parList,int type=0)
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
            return Analysis_Template_Search_Result(resultList, type);
            //return new List<SharesBase>();
        }

        private List<SharesBase> Analysis_Template_Search_Result(List<searchInfo> resultData,int type=0)
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
                        SharesCode = item.Value.SharesCode,
                        Market = item.Value.Market
                    });
                }
            }
            return dataResult;
        }

        public List<searchInfo> ParseSearchCondition(string cnt)
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
                    result = Analysis_PlateRiseRate(par.content);
                    break;
                case 5:
                    result = Analysis_CurrentPrice(par.content);
                    break;
                case 6:
                    result = Analysis_ReferAverage(par.content);
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
                case 12:
                    result = Analysis_DealCondition(par.content);
                    break;
                case 13:
                    result = Analysis_BiddingCondition(par.content);
                    break;
                case 14:
                    result = Analysis_Hands(par.content);
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

        private void CalDataToDataBase_Type6(List<SearchMarkResult> dataList)
        {
            DataTable table = new DataTable();

            #region====定义表字段数据类型====
            table.Columns.Add("TemplateId", typeof(long));
            table.Columns.Add("SharesKey", typeof(long));
            table.Columns.Add("Market", typeof(int));
            table.Columns.Add("SharesCode", typeof(string));
            #endregion

            #region====绑定数据====
            foreach (var item in dataList)
            {
                long sharesKey = long.Parse(item.SharesCode) * 10 + item.Market;
                DataRow row = table.NewRow();
                row["TemplateId"] = item.TemplateId;
                row["SharesKey"] = sharesKey;
                row["Market"] = item.Market;
                row["SharesCode"] = item.SharesCode;
                table.Rows.Add(row);
            }
            #endregion

            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //关键是类型
                    SqlParameter parameter = new SqlParameter("@searchMonitorMarkList", SqlDbType.Structured);
                    //必须指定表类型名
                    parameter.TypeName = "dbo.SearchMonitorMark";
                    //赋值
                    parameter.Value = table;
                    db.Database.ExecuteSqlCommand("exec P_SearchMonitorMark_Update @searchMonitorMarkList", parameter);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("搜索结果入库失败了2", ex);
                    tran.Rollback();
                }
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
            string parKey = (par + DateTime.Now.AddHours(-9).AddMinutes(-26).ToString("yyyy-MM-dd")).ToMD5();
            if (searchSessionDic.ContainsKey(parKey))
            {
                return GetSearchSession(parKey);
            }
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int priceRiseType = 0;
                int startDay1 = -1;
                int endDay1 = -1;
                int startDay2 = -1;
                int dealCompareType = 0;
                int priceType1 = 0;
                int priceType2 = 0;
                int compare = 0;
                int count = 0;
                if (!Analysis_Price_BuildPar(par, ref priceRiseType, ref startDay1, ref endDay1,ref startDay2,ref dealCompareType,ref priceType1,ref priceType2,ref compare,ref count))
                {
                    return new List<SharesBase>();
                }
                int days=Math.Max(startDay1,startDay2);
                days = Math.Max(days, endDay1);
                days = days + 1;

                var shares_quotes_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(days, false);

                foreach (var item in shares_quotes_session)
                {
                    //涨跌幅
                    if (priceRiseType == 1) 
                    {
                        int disRiserate = 0;
                        var quotesList=item.Value.Values.Skip(startDay1).Take(endDay1 - startDay1 + 1);
                        if (quotesList.Count() == 0)
                        {
                            continue;
                        }
                        #region===计算目标涨跌幅===
                        if (dealCompareType == 1 && priceType1 == 1)//最高开盘价
                        {
                            disRiserate = quotesList.Max(e => e.OpenRiseRate);
                        }
                        else if (dealCompareType == 1 && priceType1 == 2)//最高收盘价
                        {
                            disRiserate = quotesList.Max(e => e.RiseRate);
                        }
                        else if (dealCompareType == 1 && priceType1 == 3)//最高最高价
                        {
                            disRiserate = quotesList.Max(e => e.MaxRiseRate);
                        }
                        else if (dealCompareType == 1 && priceType1 == 3)//最高最低价
                        {
                            disRiserate = quotesList.Max(e => e.MinRiseRate);
                        }
                        else if (dealCompareType == 2 && priceType1 == 1)//最低开盘价
                        {
                            disRiserate = quotesList.Min(e => e.OpenRiseRate);
                        }
                        else if (dealCompareType == 2 && priceType1 == 2)//最低收盘价
                        {
                            disRiserate = quotesList.Min(e => e.RiseRate);
                        }
                        else if (dealCompareType == 2 && priceType1 == 3)//最低最高价
                        {
                            disRiserate = quotesList.Min(e => e.MaxRiseRate);
                        }
                        else if (dealCompareType == 2 && priceType1 == 3)//最低最低价
                        {
                            disRiserate = quotesList.Min(e => e.MinRiseRate);
                        }
                        else if (dealCompareType == 3 && priceType1 == 1)//平均开盘价
                        {
                            disRiserate = (int)quotesList.Average(e => e.OpenRiseRate);
                        }
                        else if (dealCompareType == 3 && priceType1 == 2)//平均收盘价
                        {
                            disRiserate = (int)quotesList.Average(e => e.RiseRate);
                        }
                        else if (dealCompareType == 3 && priceType1 == 3)//平均最高价
                        {
                            disRiserate = (int)quotesList.Average(e => e.MaxRiseRate);
                        }
                        else if (dealCompareType == 3 && priceType1 == 3)//平均最低价
                        {
                            disRiserate = (int)quotesList.Average(e => e.MinRiseRate);
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        #endregion

                        if ((compare == 1 && disRiserate >= count) || (compare == 2 && disRiserate <= count))
                        {
                            result.Add(new SharesBase
                            {
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                                Market = (int)(item.Key % 10)
                            });
                        }
                    }
                    //涨跌幅比较
                    else if (priceRiseType == 2) 
                    {
                        int disRiserate1 = 0;
                        int dispriceType1 = 0;
                        int disRiserate2 = 0;
                        int dispriceType2 = 0;
                        var quotes1 = item.Value.Values.Skip(startDay1).Take(1).FirstOrDefault();
                        var quotes2 = item.Value.Values.Skip(startDay2).Take(1).FirstOrDefault();
                        if (quotes1 == null || quotes2 == null)
                        {
                            continue;
                        }
                        #region===计算目标涨跌幅===
                        if (priceType1 == 1)//开盘价
                        {
                            disRiserate1 = quotes1.OpenRiseRate;
                            dispriceType1 = quotes1.OpenedPrice == quotes1.LimitUpPrice ? 1 : quotes1.OpenedPrice == quotes1.LimitDownPrice ? 2 : 0;
                        }
                        else if (priceType1 == 2)//收盘价
                        {
                            disRiserate1 = quotes1.RiseRate;
                            dispriceType1 = quotes1.ClosedPrice == quotes1.LimitUpPrice ? 1 : quotes1.ClosedPrice == quotes1.LimitDownPrice ? 2 : 0;
                        }
                        else if (priceType1 == 3)//最高价
                        {
                            disRiserate1 = quotes1.MaxRiseRate;
                            dispriceType1 = quotes1.MaxPrice == quotes1.LimitUpPrice ? 1 : quotes1.MaxPrice == quotes1.LimitDownPrice ? 2 : 0;
                        }
                        else if (priceType1 == 4)//最低价
                        {
                            disRiserate1 = quotes1.MinRiseRate;
                            dispriceType1 = quotes1.MinPrice == quotes1.LimitUpPrice ? 1 : quotes1.MinPrice == quotes1.LimitDownPrice ? 2 : 0;
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        if (priceType2 == 1)//开盘价
                        {
                            disRiserate2 = quotes2.OpenRiseRate;
                            dispriceType2 = quotes2.OpenedPrice == quotes2.LimitUpPrice ? 1 : quotes2.OpenedPrice == quotes2.LimitDownPrice ? 2 : 0;
                        }
                        else if (priceType2 == 2)//收盘价
                        {
                            disRiserate2 = quotes2.RiseRate;
                            dispriceType2 = quotes2.ClosedPrice == quotes2.LimitUpPrice ? 1 : quotes2.ClosedPrice == quotes2.LimitDownPrice ? 2 : 0;
                        }
                        else if (priceType2 == 3)//最高价
                        {
                            disRiserate2 = quotes2.MaxRiseRate;
                            dispriceType2 = quotes2.MaxPrice == quotes2.LimitUpPrice ? 1 : quotes2.MaxPrice == quotes2.LimitDownPrice ? 2 : 0;
                        }
                        else if (priceType2 == 4)//最低价
                        {
                            disRiserate2 = quotes2.MinRiseRate;
                            dispriceType2 = quotes2.MinPrice == quotes2.LimitUpPrice ? 1 : quotes2.MinPrice == quotes2.LimitDownPrice ? 2 : 0;
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        #endregion

                        if (disRiserate1 == dispriceType2 && disRiserate1 != 0)
                        {
                            dispriceType2 = disRiserate1;
                        }

                        if ((compare == 1 && disRiserate1 >= (count*1.0 / 10000 * disRiserate2)) || (compare == 2 && disRiserate1 <= (count * 1.0 / 10000 * disRiserate2)))
                        {
                            result.Add(new SharesBase
                            {
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                                Market = (int)(item.Key % 10)
                            });
                        }
                    }
                    //价格比较
                    else if (priceRiseType == 3)
                    {
                        long disRiserate1 = 0;
                        long disRiserate2 = 0;
                        var quotes1 = item.Value.Values.Skip(startDay1).Take(1).FirstOrDefault();
                        var quotes2 = item.Value.Values.Skip(startDay2).Take(1).FirstOrDefault();
                        if (quotes1 == null || quotes2 == null)
                        {
                            continue;
                        }
                        #region===计算目标涨跌幅===
                        if (priceType1 == 1)//开盘价
                        {
                            disRiserate1 = quotes1.OpenedPrice;
                        }
                        else if (priceType1 == 2)//收盘价
                        {
                            disRiserate1 = quotes1.ClosedPrice;
                        }
                        else if (priceType1 == 3)//最高价
                        {
                            disRiserate1 = quotes1.MaxPrice;
                        }
                        else if (priceType1 == 4)//最低价
                        {
                            disRiserate1 = quotes1.MinPrice;
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        if (priceType2 == 1)//开盘价
                        {
                            disRiserate2 = quotes2.OpenedPrice;
                        }
                        else if (priceType2 == 2)//收盘价
                        {
                            disRiserate2 = quotes2.ClosedPrice;
                        }
                        else if (priceType2 == 3)//最高价
                        {
                            disRiserate2 = quotes2.MaxPrice;
                        }
                        else if (priceType2 == 4)//最低价
                        {
                            disRiserate2 = quotes2.MinPrice;
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        #endregion

                        if ((compare == 1 && disRiserate1 >= (count * 1.0 / 10000 * disRiserate2)) || (compare == 2 && disRiserate1 <= (count * 1.0 / 10000 * disRiserate2)))
                        {
                            result.Add(new SharesBase
                            {
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                                Market = (int)(item.Key % 10)
                            });
                        }
                    }
                    else
                    {
                        return new List<SharesBase>();
                    }
                }

                if (startDay1 != 0 && endDay1 != 0 && startDay2 != 0)
                {
                    SetSearchSession(parKey, result);
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_Price出错", ex);
            }
            return result;
        }

        private bool Analysis_Price_BuildPar(string par, ref int priceRiseType, ref int startDay1, ref int endDay1, ref int startDay2, ref int dealCompareType, ref int priceType1, ref int priceType2, ref int compare, ref int count)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                priceRiseType = Convert.ToInt32(temp.priceRiseType);
            }
            catch (Exception ex)
            {
            }
            try
            {
                startDay1 = Convert.ToInt32(temp.startDay1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                endDay1 = Convert.ToInt32(temp.endDay1);
            }
            catch (Exception)
            {
            }
            try
            {
                startDay2 = Convert.ToInt32(temp.startDay2);
            }
            catch (Exception)
            {
            }
            try
            {
                dealCompareType = Convert.ToInt32(temp.dealCompareType);
            }
            catch (Exception)
            {
            }
            try
            {
                priceType1 = Convert.ToInt32(temp.priceType1);
            }
            catch (Exception)
            {
            }
            try
            {
                priceType2 = Convert.ToInt32(temp.priceType2);
            }
            catch (Exception)
            {
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
                count = (int)Math.Round(Convert.ToDouble(temp.count)*100,0);
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
            string parKey = (par + DateTime.Now.AddHours(-9).AddMinutes(-26).ToString("yyyy-MM-dd")).ToMD5();
            if (searchSessionDic.ContainsKey(parKey))
            {
                return GetSearchSession(parKey);
            }
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
                int priceType2 = 0;
                bool limitUpT = false;
                bool limitDownT = false;
                int modelType = 2;
                int baseType2 = 2;
                int day2 = 0;
                if (!Analysis_HisRiseRate_BuildPar(par, ref day, ref type, ref compare,ref count, ref rateCompare, ref rateCount, ref limitDay, ref flatRise, ref triPrice, ref priceCompare, ref priceType, ref priceError, ref direct, ref baseType,ref priceType2,ref limitUpT,ref limitDownT,ref modelType,ref baseType2,ref day2))
                {
                    return new List<SharesBase>();
                }

                var quotes_date_session=Analysis_HisRiseRate_GetSession(day, modelType, priceCompare, priceType, direct, priceError);

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
                        result = _analysis_HisRiseRate_13(quotes_date_session,compare,count,flatRise,rateCompare,rateCount, priceType2);
                        break;
                    case 14:
                        result = _analysis_HisRiseRate_14(quotes_date_session,compare,count,flatRise);
                        break;
                    case 15:
                        result = _analysis_HisRiseRate_15(quotes_date_session,baseType,15, baseType2, day2);
                        break;
                    case 16:
                        result = _analysis_HisRiseRate_15(quotes_date_session,baseType,16, baseType2, day2);
                        break;
                    case 17:
                        result = _analysis_HisRiseRate_17(quotes_date_session, compare, count, rateCompare, rateCount, priceType2);
                        break;
                    case 18:
                        result = _analysis_HisRiseRate_18(quotes_date_session, compare, count, limitUpT);
                        break;
                    case 19:
                        result = _analysis_HisRiseRate_19(quotes_date_session, compare, count, limitDownT);
                        break;
                    default:
                        break;
                }

                if (modelType==2)
                {
                    SetSearchSession(parKey, result);
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_HisRiseRate出错", ex);
            }
            return result;
        }

        private bool Analysis_HisRiseRate_BuildPar(string par, ref int day, ref int type, ref int compare, ref int count, ref int rateCompare, ref int rateCount, ref int limitDay, ref bool flatRise, ref bool triPrice, ref int priceCompare, ref int priceType, ref long priceError, ref int direct, ref int baseType,ref int priceType2,ref bool limitUpT,ref bool limitDownT, ref int modelType, ref int baseType2, ref int day2)
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
            try
            {
                priceType2 = Convert.ToInt32(temp.PriceType2);
            }
            catch (Exception) { }
            try
            {
                limitUpT = Convert.ToBoolean(temp.LimitUpT);
            }
            catch (Exception) { }
            try
            {
                limitDownT = Convert.ToBoolean(temp.LimitDownT);
            }
            catch (Exception) { }
            try
            {
                modelType = Convert.ToInt32(temp.modelType);
            }
            catch (Exception ex)
            {
            }
            try
            {
                baseType2 = Convert.ToInt32(temp.BaseType2);
            }
            catch (Exception ex)
            {
            }
            try
            {
                day2 = Convert.ToInt32(temp.Day2);
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        private Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> Analysis_HisRiseRate_GetSession(int day,int modelType,int priceCompare,int priceType,int direct,long priceError)
        {
            Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date_dic = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            if (modelType == 1)
            {
                var quotes_date_dic_temp = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(day, false);
                foreach (var item in quotes_date_dic_temp)
                {
                    quotes_date_dic.Add(item.Key, item.Value.Take(day).ToDictionary(k => k.Key, v => v.Value));
                }
            }
            else if (modelType == 2)//上个交易日
            {
                var quotes_date_dic_temp = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(day+1, false);
                foreach (var item in quotes_date_dic_temp)
                {
                    quotes_date_dic.Add(item.Key, item.Value.Skip(1).Take(day).ToDictionary(k => k.Key, v => v.Value));
                }

            }
            else if (modelType == 3)//交易结束时间
            {
                var quotes_date_dic_temp = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(day + 1, false);
                if (DbHelper.CheckTradeTime7())
                {
                    foreach (var item in quotes_date_dic_temp)
                    {
                        quotes_date_dic.Add(item.Key, item.Value.Skip(1).Take(day).ToDictionary(k => k.Key, v => v.Value));
                    }
                }
                else 
                {
                    foreach (var item in quotes_date_dic_temp)
                    {
                        quotes_date_dic.Add(item.Key, item.Value.Take(day).ToDictionary(k => k.Key, v => v.Value));
                    }
                }
            }
            else
            {
                return new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            }

            Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> session = new Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>>();
            if (direct == 0)
            {
                direct = 2;
            }
            foreach (var item in quotes_date_dic)
            {
                Dictionary<DateTime,Shares_Quotes_Session_Info> tempList = new Dictionary<DateTime, Shares_Quotes_Session_Info>();
                if (direct == 2)
                {
                    var direct_quotes_date = item.Value.ToDictionary(k=>k.Key,v=>v.Value);
                    int i = 0;
                    foreach (var quote in direct_quotes_date)
                    {
                        if (i >= day)
                        {
                            break;
                        }
                        i++;
                        int RiseRate = (quote.Value.YestodayClosedPrice == 0 || quote.Value.ClosedPrice == 0) ? 0 : (int)Math.Round((quote.Value.ClosedPrice - quote.Value.YestodayClosedPrice) * 1.0 / quote.Value.YestodayClosedPrice * 10000, 0);
                        if ((priceType == 1 && priceCompare == 1 && quote.Value.ClosedPrice >= quote.Value.LimitUpPrice + priceError) || (priceType == 1 && priceCompare == 2 && quote.Value.ClosedPrice <= quote.Value.LimitUpPrice + priceError))
                        {
                            tempList.Add(quote.Key, quote.Value);
                            break;
                        }
                        else if ((priceType == 2 && priceCompare == 1 && quote.Value.ClosedPrice >= quote.Value.LimitDownPrice + priceError) || (priceType == 2 && priceCompare == 2 && quote.Value.ClosedPrice <= quote.Value.LimitDownPrice + priceError))
                        {
                            tempList.Add(quote.Key, quote.Value);
                            break;
                        }
                        else if ((priceType == 3 && priceCompare == 1 && RiseRate >= priceError) || (priceType == 3 && priceCompare == 2 && RiseRate <= priceError))
                        {
                            tempList.Add(quote.Key, quote.Value);
                            break;
                        }
                        tempList.Add(quote.Key, quote.Value);
                    }
                    session.Add(item.Key, tempList);
                }
                else 
                {
                    var direct_quotes_date = item.Value.OrderBy(e => e.Value.Date).ToDictionary(k => k.Key, v => v.Value);
                    bool isAdd = false;
                    int i = 0;
                    foreach (var quote in direct_quotes_date)
                    {
                        i++;
                        if (i <= direct_quotes_date.Count()-day)
                        {
                            continue;
                        }
                        if (!isAdd)
                        {
                            int RiseRate = (quote.Value.YestodayClosedPrice == 0 || quote.Value.ClosedPrice == 0) ? 0 : (int)Math.Round((quote.Value.ClosedPrice - quote.Value.YestodayClosedPrice) * 1.0 / quote.Value.YestodayClosedPrice * 10000, 0);
                            if ((priceType == 1 && priceCompare == 1 && quote.Value.ClosedPrice >= quote.Value.LimitUpPrice + priceError) || (priceType == 1 && priceCompare == 2 && quote.Value.ClosedPrice <= quote.Value.LimitUpPrice + priceError))
                            {
                                isAdd = true;
                            }
                            else if ((priceType == 2 && priceCompare == 1 && quote.Value.ClosedPrice >= quote.Value.LimitDownPrice + priceError) || (priceType == 2 && priceCompare == 2 && quote.Value.ClosedPrice <= quote.Value.LimitDownPrice + priceError))
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
                            tempList.Add(quote.Key, quote.Value);
                        }
                    }
                    session.Add(item.Key, tempList);
                }
            }
            return session;
        }

        //历史涨跌幅-没涨停次数
        private List<SharesBase> _analysis_HisRiseRate_1(Dictionary<long, Dictionary<DateTime,Shares_Quotes_Session_Info>> quotes_date, int compare,int count) 
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = item.Value.Where(e => e.Value.PriceType != 1).Count();
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
        private List<SharesBase> _analysis_HisRiseRate_2(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count,bool triPrice)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = 0;
                if (triPrice)
                {
                    tempCount = item.Value.Where(e=>e.Value.TriPriceType==1).Count();
                }
                else
                {
                    tempCount = item.Value.Where(e => e.Value.PriceType == 1).Count();
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
        private List<SharesBase> _analysis_HisRiseRate_3(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count, bool triPrice)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = 0;
                if (triPrice)
                {
                    tempCount = item.Value.Where(e => e.Value.TriPriceType == 2).Count();
                }
                else
                {
                    tempCount = item.Value.Where(e => e.Value.PriceType == 2).Count();
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
        private List<SharesBase> _analysis_HisRiseRate_4(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempCount = item.Value.Where(e => e.Value.PriceType != 1 && e.Value.LimitUpCount > 0).Count();

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
        private List<SharesBase> _analysis_HisRiseRate_7(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date,int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续涨停天数
                foreach (var quote in item.Value)
                {
                    if (quote.Value.PriceType == 1)
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
        private List<SharesBase> _analysis_HisRiseRate_8(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续涨停天数
                bool isContinue = false;
                foreach (var quote in item.Value)
                {
                    if (quote.Value.PriceType == 1)
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
        private List<SharesBase> _analysis_HisRiseRate_9(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续跌停天数
                foreach (var quote in item.Value)
                {
                    if (quote.Value.PriceType == 2)
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
        private List<SharesBase> _analysis_HisRiseRate_10(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int limitDay)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int tempi = 0;//连续跌停天数
                bool isContinue = false;
                foreach (var quote in item.Value)
                {
                    if (quote.Value.PriceType == 2)
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
        private List<SharesBase> _analysis_HisRiseRate_13(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count, bool flatRise, int rateCompare, int rateCount,int priceType2)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int i = 0;
                foreach (var quote in item.Value)
                {
                    if (quote.Value.ClosedPrice <= 0 || quote.Value.ClosedPrice <= 0)
                    {
                        continue;
                    }
                    int rate = (int)Math.Round((quote.Value.ClosedPrice - quote.Value.YestodayClosedPrice) * 1.0 / quote.Value.YestodayClosedPrice * 10000, 0);
                    if (!flatRise && rate == 0)
                    {
                        continue;
                    }

                    if (rateCompare == 1)
                    {
                        if (priceType2 == 1 && quote.Value.ClosedPrice >= quote.Value.LimitUpPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 2 && quote.Value.ClosedPrice >= quote.Value.LimitDownPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 3 && rate >= rateCount)
                        {
                            i++;
                            continue;
                        }
                    }
                    else if (rateCompare == 2)
                    {
                        if (priceType2 == 1 && quote.Value.ClosedPrice <= quote.Value.LimitUpPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 2 && quote.Value.ClosedPrice <= quote.Value.LimitDownPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 3 && rate <= rateCount)
                        {
                            i++;
                            continue;
                        }
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
        private List<SharesBase> _analysis_HisRiseRate_14(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count,bool flatRise)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                long closePrice = item.Value.OrderBy(e => e.Key).Select(e => e.Value.YestodayClosedPrice).FirstOrDefault();
                long presentPrice = item.Value.OrderByDescending(e => e.Key).Select(e => e.Value.ClosedPrice).FirstOrDefault();
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
        private List<SharesBase> _analysis_HisRiseRate_15(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date,int baseType,int type,int baseType2,int day2)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                var currInfo = item.Value.OrderByDescending(e => e.Key).Skip(day2).Take(1).FirstOrDefault();
                var quotes_date_his = item.Value.Where(e => e.Key != currInfo.Key).ToList();
                if (quotes_date_his.Count() <= 0)
                {
                    continue;
                }
                try
                {
                    long sourcePrice = baseType2 == 1 ? currInfo.Value.OpenedPrice : baseType2 == 4 ? currInfo.Value.MinPrice : baseType2 == 3 ? currInfo.Value.MaxPrice : currInfo.Value.ClosedPrice;
                    long disMaxPrice = 0;
                    long disMinPrice = 0;
                    if (baseType == 1)
                    {
                        disMaxPrice = quotes_date_his.Max(e => e.Value.OpenedPrice);
                        disMinPrice = quotes_date_his.Min(e => e.Value.OpenedPrice);
                    }
                    else if (baseType == 3)
                    {
                        disMaxPrice = quotes_date_his.Max(e => e.Value.MaxPrice);
                        disMinPrice = quotes_date_his.Min(e => e.Value.MaxPrice);
                    }
                    else if (baseType == 4)
                    {
                        disMaxPrice = quotes_date_his.Max(e => e.Value.MinPrice);
                        disMinPrice = quotes_date_his.Min(e => e.Value.MinPrice);
                    }
                    else
                    {
                        disMaxPrice = quotes_date_his.Max(e => e.Value.ClosedPrice);
                        disMinPrice = quotes_date_his.Min(e => e.Value.ClosedPrice);
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
                catch (Exception ex)
                {
                }
            }
            return result;
        }
        //历史涨跌幅-每日开盘价
        private List<SharesBase> _analysis_HisRiseRate_17(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count,int rateCompare, int rateCount, int priceType2)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int i = 0;
                foreach (var quote in item.Value)
                {
                    if (quote.Value.ClosedPrice <= 0 || quote.Value.OpenedPrice <= 0)
                    {
                        continue;
                    }
                    long openPrice = quote.Value.OpenedPrice;
                    int rate = (int)Math.Round((quote.Value.OpenedPrice - quote.Value.YestodayClosedPrice) * 1.0 / quote.Value.YestodayClosedPrice * 10000, 0);

                    if (rateCompare == 1)
                    {
                        if (priceType2 == 1 && openPrice >= quote.Value.LimitUpPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 2 && openPrice >= quote.Value.LimitDownPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 3 && rate >= rateCount)
                        {
                            i++;
                            continue;
                        }
                    }
                    else if (rateCompare == 2)
                    {
                        if (priceType2 == 1 && openPrice <= quote.Value.LimitUpPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 2 && openPrice <= quote.Value.LimitDownPrice + rateCount * 100)
                        {
                            i++;
                            continue;
                        }
                        else if (priceType2 == 3 && rate <= rateCount)
                        {
                            i++;
                            continue;
                        }
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

        //历史涨跌幅-一字涨停次数
        private List<SharesBase> _analysis_HisRiseRate_18(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count, bool limitUpT)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int i = 0;
                foreach (var quote in item.Value)
                {
                    if (quote.Value.ClosedPrice <= 0)
                    {
                        continue;
                    }
                    if (quote.Value.PriceType != 1)
                    {
                        continue;
                    }
                    long openPrice = quote.Value.OpenedPrice;
                    long minPrice = quote.Value.MinPrice;

                    if (openPrice != quote.Value.LimitUpPrice)
                    {
                        continue;
                    }

                    if (!limitUpT && minPrice != quote.Value.LimitUpPrice)
                    {
                        continue;
                    }
                    i++;
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

        //历史涨跌幅-一字跌停次数
        private List<SharesBase> _analysis_HisRiseRate_19(Dictionary<long, Dictionary<DateTime, Shares_Quotes_Session_Info>> quotes_date, int compare, int count, bool limitDownT)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_date)
            {
                int i = 0;
                foreach (var quote in item.Value)
                {
                    if (quote.Value.ClosedPrice <= 0)
                    {
                        continue;
                    }
                    if (quote.Value.PriceType != 2)
                    {
                        continue;
                    }
                    long openPrice = quote.Value.OpenedPrice;
                    long maxPrice = quote.Value.MaxPrice;

                    if (openPrice != quote.Value.LimitDownPrice)
                    {
                        continue;
                    }

                    if (!limitDownT && maxPrice != quote.Value.LimitDownPrice)
                    {
                        continue;
                    }
                    i++;
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
                count = (type == 5 || type == 6 || type == 10 || type==11) ? (int)(Convert.ToDouble(temp.Count) * 100) : Convert.ToInt64(temp.Count);
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
                if ((compare == 1 && rateNow >= count) || (compare == 2 && rateNow <= count))
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
                int priceType = 0;
                long priceError = 0;
                if (!Analysis_CurrentPrice_BuildPar(par, ref riseType, ref compare, ref count,ref priceType,ref priceError))
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
                    case 8:
                        result = Analysis_CurrentPrice_8(shares_quotes_last_dic, compare, priceType, priceError);
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

        private bool Analysis_CurrentPrice_BuildPar(string par,ref int riseType,ref int compare,ref long count, ref int priceType, ref long priceError)
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
                priceError = (long)(Convert.ToDouble(temp.PriceError) * 10000);
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
        private List<SharesBase> Analysis_CurrentPrice_8(Dictionary<long, Shares_Quotes_Session_Info_Last> quotes_last, int compare,int priceType,long priceError)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_last)
            {
                if (item.Value.shares_quotes_info.LimitUpPrice <= 0 || item.Value.shares_quotes_info.LimitDownPrice <= 0)
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

                int days = day1 + day2;
                var quotes_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(days, false);

                result=_analysis_ReferAverage(quotes_session, day1, day2, dayShortageType, compare, count, upOrDown);
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

        private List<SharesBase> _analysis_ReferAverage(Dictionary<long, Dictionary<DateTime,Shares_Quotes_Session_Info>> quotes_session, int day1, int day2, int dayShortageType, int compare, double count, int upOrDown)
        {
            List<SharesBase> result = new List<SharesBase>();
            foreach (var item in quotes_session)
            {
                var presentInfo = item.Value.Values.FirstOrDefault();
                if (presentInfo == null)
                {
                    continue;
                }
                long presentPrice = presentInfo.ClosedPrice;
                if (presentPrice <= 0)
                {
                    continue;
                }
                //计算均线价格
                var list1 = item.Value.Values.Take(day1).ToList();
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

                long averagePrice = (long)list1.Average(e => e.ClosedPrice);
                if (count != 0)//计算偏差
                {
                    averagePrice = (long)(averagePrice * (count / 100) + averagePrice);
                }

                if ((compare == 1 && presentPrice < averagePrice) || (compare == 2 && presentPrice > averagePrice))
                {
                    continue;
                }

                bool IsSuccess = true;
                if (day2 >= 2)//计算每日均线
                {
                    long lastAveragePrice = 0;
                    for (int i = 0; i < day2; i++)
                    {
                        long tempAveragePrice;
                        var list2 = item.Value.Values.Skip(i).Take(day1);
                        if (list2.Count() <= 0)
                        {
                            tempAveragePrice = 0;
                        }
                        else
                        {
                            tempAveragePrice = (long)list2.Average(e => e.ClosedPrice);
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

        #region====成交量条件====
        //分析成交量条件
        public List<SharesBase> Analysis_DealCondition(string par)
        {
            string parKey = (par + DateTime.Now.AddHours(-9).AddMinutes(-26).ToString("yyyy-MM-dd")).ToMD5();
            if (searchSessionDic.ContainsKey(parKey))
            {
                return GetSearchSession(parKey);
            }
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int startDay1 = -1;
                int endDay1 = -1;
                int dealType = 0;
                int compare = 0;
                int startDay2 = -1;
                int endDay2 = -1;
                int dealCompareType = 0;
                double count = 0;
                if (!Analysis_DealCondition_BuildPar(par, ref startDay1, ref endDay1, ref dealType, ref compare, ref startDay2, ref endDay2, ref dealCompareType, ref count))
                {
                    return new List<SharesBase>();
                }

                int days = endDay1 > endDay2 ? endDay1 : endDay2;
                var shares_quotes_date_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(days,false);

                foreach (var item in shares_quotes_date_session)
                {
                    var list1 = item.Value.Values.Skip(startDay1).Take(endDay1 - startDay1 + 1);
                    var list2 = item.Value.Values.Skip(startDay2).Take(endDay2 - startDay2 + 1);
                    if (list1.Count() <= 0 || list2.Count()<=0) 
                    {
                        continue;
                    }

                    long dealValue1 = 0;
                    long dealValue2 = 0;
                    if (dealType == 1)
                    {
                        dealValue1 = (long)list1.Average(e => e.TotalCount);
                        if (dealCompareType == 1)
                        {
                            dealValue2= (long)list2.Max(e => e.TotalCount);
                        }
                        else if (dealCompareType == 2)
                        {
                            dealValue2 = (long)list2.Min(e => e.TotalCount);
                        }
                        else if (dealCompareType == 3)
                        {
                            dealValue2 = (long)list2.Average(e => e.TotalCount);
                        }
                        else 
                        {
                            continue;
                        }
                    }
                    else if (dealType == 2)
                    {
                        dealValue1 = (long)list1.Average(e => e.TotalAmount);
                        if (dealCompareType == 1)
                        {
                            dealValue2 = (long)list2.Max(e => e.TotalAmount);
                        }
                        else if (dealCompareType == 2)
                        {
                            dealValue2 = (long)list2.Min(e => e.TotalAmount);
                        }
                        else if (dealCompareType == 3)
                        {
                            dealValue2 = (long)list2.Average(e => e.TotalAmount);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else 
                    {
                        continue;
                    }
                    if ((compare == 1 && dealValue1>= dealValue2* (count/100)) || (compare == 2 && dealValue1 <= dealValue2 * (count / 100)))
                    {
                        result.Add(new SharesBase
                        {
                            Market = (int)(item.Key % 10),
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                        });
                    }
                }
                if (startDay1 != 0 && endDay1 != 0 && startDay2 != 0 && endDay2 != 0)
                {
                    SetSearchSession(parKey, result);
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_DealCondition出错", ex);
            }
            return result;
        }

        private bool Analysis_DealCondition_BuildPar(string par, ref int startDay1, ref int endDay1, ref int dealType, ref int compare, ref int startDay2, ref int endDay2, ref int dealCompareType,ref double count)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                startDay1 = Convert.ToInt32(temp.startDay1);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-startDay1参数解析出错", ex);
                return false;
            }
            try
            {
                endDay1 = Convert.ToInt32(temp.endDay1);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-endDay1参数解析出错", ex);
                return false;
            }
            try
            {
                dealType = Convert.ToInt32(temp.dealType);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-dealType参数解析出错", ex);
                return false;
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-Compare参数解析出错", ex);
                return false;
            }
            try
            {
                startDay2 = Convert.ToInt32(temp.startDay2);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-startDay2参数解析出错", ex);
                return false;
            }
            try
            {
                endDay2 = Convert.ToInt32(temp.endDay2);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-endDay2参数解析出错", ex);
                return false;
            }
            try
            {
                dealCompareType = Convert.ToInt32(temp.dealCompareType);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-dealCompareType参数解析出错", ex);
                return false;
            }
            try
            {
                count = Convert.ToDouble(temp.count);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("分析成交量条件-count参数解析出错", ex);
                return false;
            }
            return true;
        }
        #endregion

        #region===竞价条件===
        public List<SharesBase> Analysis_BiddingCondition(string par)
        {
            string parKey = (par + DateTime.Now.AddHours(-9).AddMinutes(-26).ToString("yyyy-MM-dd")).ToMD5();
            if (searchSessionDic.ContainsKey(parKey))
            {
                return GetSearchSession(parKey);
            }
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int startDay1 = -1;
                int endDay1 = -1;
                int dealType = 0;
                int compare = 0;
                int startDay2 = -1;
                int endDay2 = -1;
                double count = 0;
                int biddingType = 1;

                int riserateType = 0;
                int riserateCompare = 0;
                int riserate = 0;
                bool isReverse = false;
                int compare1 = 0;
                double count1 = 0;
                int compare2 = 0;
                double count2 = 0;
                int compare3 = 0;
                int dealCompareType = 0;
                double count3 = 0;
                if (!Analysis_BiddingCondition_BuildPar(par, ref startDay1, ref endDay1, ref dealType, ref compare, ref startDay2, ref endDay2, ref count, ref biddingType, ref riserateType, ref riserateCompare, ref riserate, ref isReverse, ref compare1, ref count1, ref compare2, ref count2, ref compare3, ref dealCompareType, ref count3))
                {
                    return new List<SharesBase>();
                }
                int days = Math.Max(startDay1, startDay2);
                days = Math.Max(days, endDay1);
                days = Math.Max(days, endDay2);
                if (days == 0)
                {
                    days = 60;
                }
                else
                {
                    days = days + 1;
                }

                var quotes_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(days, false);

                bool isTradeTime = DbHelper.CheckTradeTime7();
                foreach (var item in quotes_session)
                {
                    if (biddingType == 1)
                    {
                        var list1 = item.Value.Values.Skip(startDay1).Take(endDay1 - startDay1 + 1);
                        var list2 = item.Value.Values.Skip(startDay2).Take(endDay2 - startDay2 + 1);
                        if (list1.Count() <= 0 || list2.Count() <= 0)
                        {
                            continue;
                        }
                        long dealValue1 = 0;
                        long dealValue2 = 0;
                        if (dealType == 1)
                        {
                            dealValue1 = (long)list1.Average(e => e.BiddingTotalCount);
                            dealValue2 = (long)list2.Average(e => e.BiddingTotalCount);
                        }
                        else if (dealType == 2)
                        {
                            dealValue1 = (long)list1.Average(e => e.BiddingTotalAmount);
                            dealValue2 = (long)list2.Average(e => e.BiddingTotalAmount);
                        }
                        else
                        {
                            continue;
                        }
                        if ((compare == 1 && dealValue1 >= dealValue2 * (count / 100)) || (compare == 2 && dealValue1 <= dealValue2 * (count / 100)))
                        {
                            result.Add(new SharesBase
                            {
                                Market = (int)(item.Key % 10),
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                            });
                        }
                    }
                    else if (biddingType == 2)
                    {
                        var list = item.Value.Values.Skip(startDay1).Take(endDay1 - startDay1 + 1);
                        if (list.Count() <= 0)
                        {
                            continue;
                        }
                        long dealValue = 0;
                        if (dealType == 1)
                        {
                            dealValue = (long)list.Average(e => e.BiddingTotalCount);
                        }
                        else if (dealType == 2)
                        {
                            dealValue = (long)list.Average(e => e.BiddingTotalAmount);
                        }
                        else
                        {
                            continue;
                        }

                        double comcount = dealType == 1 ? (count * 100) : (count * 10000 * 10000);
                        if ((compare == 1 && dealValue >= comcount) || (compare == 2 && dealValue <= comcount))
                        {
                            result.Add(new SharesBase
                            {
                                Market = (int)(item.Key % 10),
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                            });
                        }
                    }
                    else if (biddingType == 3)
                    {
                        int breakCount = 1;
                        if (isReverse && (riserateType == 1 || riserateType == 2))
                        {
                            breakCount = 0;
                        }
                        bool isSuccess = false;
                        List<long> totalCountList = new List<long>();
                        long todayTotalCount = 0;//最近成交量
                        int idx = 0;
                        int limitCount = 0;
                        bool lastValid = true;
                        foreach (var quote in item.Value)
                        {
                            int priceType = quote.Value.PriceType;
                            idx++;
                            if (idx == 1)
                            {
                                todayTotalCount = quote.Value.BiddingTotalCount;
                            }

                            if (((compare1 == 1 && (quote.Value.BiddingTotalCountRate >= count1 * 100 || quote.Value.BiddingTotalCountRate==-1)) || (compare1 == 2 && quote.Value.BiddingTotalCountRate <= count1 * 100)) && ((compare2 == 1 && quote.Value.BiddingTotalAmount >= count2 * 10000 * 10000) || (compare2 == 2 && quote.Value.BiddingTotalAmount <= count2 * 10000 * 10000)) && !isSuccess)
                            {
                                isSuccess = true;
                            }

                            if (riserateType == 1)
                            {
                                priceType = idx == 1 ? 1 : priceType;
                                if (priceType != 1 && breakCount >= 1)
                                {
                                    break;
                                }
                                if (priceType != 1)
                                {
                                    breakCount++;
                                }
                                if (idx > 1 && priceType == 1)
                                {
                                    limitCount++;
                                }
                            }
                            else if (riserateType == 2)
                            {
                                priceType = idx == 1 ? 2 : priceType;
                                if (priceType != 2 && breakCount >= 1)
                                {
                                    break;
                                }
                                if (priceType != 2)
                                {
                                    breakCount++;
                                }
                                if (idx > 1 && priceType == 2)
                                {
                                    limitCount++;
                                }
                            }
                            else if (riserateType == 3)
                            {
                                if (riserateCompare == 0)
                                {
                                    return new List<SharesBase>();
                                }
                                if ((riserateCompare == 1 && quote.Value.RiseRate < riserate * 100) || (riserateCompare == 2 && quote.Value.RiseRate > riserate * 100))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                return new List<SharesBase>();
                            }

                            if (idx > 1)
                            {
                                totalCountList.Add(quote.Value.BiddingTotalCount);
                                lastValid = (priceType==1);
                            }
                        }
                        if (totalCountList.Count() <= 0 || limitCount <= 0)
                        {
                            continue;
                        }
                        if (!lastValid)
                        {
                            totalCountList.RemoveAt(totalCountList.Count() - 1);
                        }
                        if (totalCountList.Count() <= 0)
                        {
                            continue;
                        }
                        if (isSuccess)
                        {
                            long disTotalCount = 0;
                            if (dealCompareType == 1)
                            {
                                disTotalCount = totalCountList.Max();
                            }
                            else if (dealCompareType == 2)
                            {
                                disTotalCount = totalCountList.Min();
                            }
                            else if (dealCompareType == 3)
                            {
                                disTotalCount = (int)totalCountList.Average();
                            }
                            else
                            {
                                return new List<SharesBase>();
                            }

                            if ((compare3 == 1 && todayTotalCount >= disTotalCount * count3) || (compare3 == 2 && todayTotalCount <= disTotalCount * count3))
                            {
                                result.Add(new SharesBase
                                {
                                    Market = (int)(item.Key % 10),
                                    SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                                });
                            }
                        }
                    }
                    else if (biddingType == 4)
                    {
                        var list = item.Value.Values.Skip(startDay1).Take(1).FirstOrDefault();
                        if (list == null)
                        {
                            continue;
                        }
                        long dealValue = list.BiddingTotalAmountRate;
                        if ((compare == 1 && dealValue >= count*100) || (compare == 2 && dealValue <= count*100))
                        {
                            result.Add(new SharesBase
                            {
                                Market = (int)(item.Key % 10),
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0')
                            });
                        }
                    }
                    else
                    {
                        return new List<SharesBase>();
                    }
                }
                if (startDay1 != 0 && endDay1 != 0 && startDay2 != 0 && endDay2 != 0 && biddingType != 3)
                {
                    SetSearchSession(parKey, result);
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_DealCondition出错", ex);
            }
            return result;
        }

        private bool Analysis_BiddingCondition_BuildPar(string par, ref int startDay1, ref int endDay1, ref int dealType, ref int compare, ref int startDay2, ref int endDay2, ref double count,ref int biddingType, ref int riserateType, ref int riserateCompare, ref int riserate, ref bool isReverse, ref int compare1, ref double count1, ref int compare2, ref double count2, ref int compare3, ref int dealCompareType, ref double count3)
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                riserateType = Convert.ToInt32(temp.riserateType);
            }
            catch (Exception ex)
            {
            }
            try
            {
                riserateCompare = Convert.ToInt32(temp.riserateCompare);
            }
            catch (Exception ex)
            {
            }
            try
            {
                riserate = Convert.ToInt32(temp.riserate);
            }
            catch (Exception ex)
            {
            }
            try
            {
                isReverse = Convert.ToBoolean(temp.isReverse);
            }
            catch (Exception ex)
            {
            }
            try
            {
                compare1 = Convert.ToInt32(temp.compare1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                count1 = Convert.ToDouble(temp.count1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                compare2 = Convert.ToInt32(temp.compare2);
            }
            catch (Exception ex)
            {
            }
            try
            {
                count2 = Convert.ToDouble(temp.count2);
            }
            catch (Exception ex)
            {
            }
            try
            {
                compare3 = Convert.ToInt32(temp.compare3);
            }
            catch (Exception ex)
            {
            }
            try
            {
                dealCompareType = Convert.ToInt32(temp.dealCompareType);
            }
            catch (Exception ex)
            {
            }
            try
            {
                count3 = Convert.ToDouble(temp.count3);
            }
            catch (Exception ex)
            {
            }
            try
            {
                startDay1 = Convert.ToInt32(temp.startDay1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                endDay1 = Convert.ToInt32(temp.endDay1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                dealType = Convert.ToInt32(temp.dealType);
            }
            catch (Exception ex)
            {
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
            }
            try
            {
                startDay2 = Convert.ToInt32(temp.startDay2);
            }
            catch (Exception ex)
            {
            }
            try
            {
                endDay2 = Convert.ToInt32(temp.endDay2);
            }
            catch (Exception ex)
            {
            }
            try
            {
                count = Convert.ToDouble(temp.count);
            }
            catch (Exception ex)
            {
            }
            try
            {
                biddingType = Convert.ToInt32(temp.biddingType);
            }
            catch (Exception ex)
            {
            }
            return true;
        }
        #endregion

        #region===换手率条件===
        public List<SharesBase> Analysis_Hands(string par)
        {
            string parKey = (par + DateTime.Now.AddHours(-9).AddMinutes(-26).ToString("yyyy-MM-dd")).ToMD5();
            if (searchSessionDic.ContainsKey(parKey))
            {
                return GetSearchSession(parKey);
            }
            List<SharesBase> result = new List<SharesBase>();
            try
            {
                int handsType = 0;
                int startDay1 = -1;
                int compare = 0;
                int averageDay1 = 0;
                int count = 0;
                int averageDay2 = 0;
                int upOrDown = 0;
                int DayShortageType = 0;

                int endDay1 = -1;
                int dealCompareType = 0;
                int startDay2 = -1;
                int endDay2 = -1;
                int dealCompareType2 = 0;

                if (!Analysis_Hands_BuildPar(par,ref handsType, ref startDay1, ref compare, ref averageDay1, ref count, ref averageDay2, ref upOrDown, ref DayShortageType, ref endDay1, ref dealCompareType,ref startDay2,ref endDay2,ref dealCompareType2))
                {
                    return new List<SharesBase>();
                }

                int days = Math.Max(startDay1, startDay2);
                days = Math.Max(days, endDay1);
                days = Math.Max(days, endDay2);
                days = days + 1 + averageDay1 + averageDay2;

                var shares_quotes_session = Singleton.Instance.sessionHandler.GetShares_Quotes_Total_Session(days, false);
                foreach (var item in shares_quotes_session)
                {
                    if (handsType == 1)
                    {
                        int disHandsRate = 0;
                        var quotesList = item.Value.Values.Skip(startDay1).Take(endDay1 - startDay1 + 1);
                        if (quotesList.Count() == 0)
                        {
                            continue;
                        }
                        #region===计算目标换手率===
                        if (dealCompareType == 1)//最高
                        {
                            disHandsRate = quotesList.Max(e=>e.HandsRate);
                        }
                        else if (dealCompareType == 2)//最低
                        {
                            disHandsRate = quotesList.Min(e => e.HandsRate);
                        }
                        else if (dealCompareType == 3)//平均
                        {
                            disHandsRate = (int)quotesList.Average(e => e.HandsRate);
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        #endregion

                        if ((compare == 1 && disHandsRate >= count) || (compare == 2 && disHandsRate <= count))
                        {
                            result.Add(new SharesBase
                            {
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                                Market = (int)(item.Key % 10)
                            });
                        }
                    }
                    else if (handsType == 2)
                    {
                        int disHandsRate1 = 0;
                        int disHandsRate2 = 0;
                        var quotesList1 = item.Value.Values.Skip(startDay1).Take(endDay1 - startDay1 + 1);
                        var quotesList2 = item.Value.Values.Skip(startDay2).Take(endDay2 - startDay2 + 1);
                        if (quotesList1.Count() <= 0 || quotesList2.Count() <= 0)
                        {
                            continue;
                        }

                        #region===计算目标换手率===
                        if (dealCompareType == 1)
                        {
                            disHandsRate1 = quotesList1.Max(e => e.HandsRate);
                        }
                        else if (dealCompareType == 2)
                        {
                            disHandsRate1 = quotesList1.Min(e => e.HandsRate);
                        }
                        else if (dealCompareType == 3)
                        {
                            disHandsRate1 = (int)quotesList1.Average(e => e.HandsRate);
                        }
                        else 
                        {
                            return new List<SharesBase>();
                        }
                        if (dealCompareType2 == 1)
                        {
                            disHandsRate2 = quotesList2.Max(e => e.HandsRate);
                        }
                        else if (dealCompareType2 == 2)
                        {
                            disHandsRate2 = quotesList2.Min(e => e.HandsRate);
                        }
                        else if (dealCompareType2 == 3)
                        {
                            disHandsRate2 = (int)quotesList2.Average(e => e.HandsRate);
                        }
                        else
                        {
                            return new List<SharesBase>();
                        }
                        #endregion
                        if ((compare == 1 && disHandsRate1 >= (count * 1.0 / 10000 * disHandsRate2)) || (compare == 2 && disHandsRate1 <= (count * 1.0 / 10000 * disHandsRate2)))
                        {
                            result.Add(new SharesBase
                            {
                                SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                                Market = (int)(item.Key % 10)
                            });
                        }
                    }
                    else if (handsType == 3)
                    {
                        var disquotes = item.Value.Values.Skip(startDay1).Take(1).FirstOrDefault();
                        if (disquotes == null)
                        {
                            continue;
                        }
                        int disHandsRate = disquotes.HandsRate;
                        //计算均线
                        var avgquotes = item.Value.Values.Skip(startDay1).Take(averageDay1).ToList();
                        if (avgquotes.Count() <= 0)
                        {
                            continue;
                        }
                        //均线不足
                        if (avgquotes.Count() < averageDay1)
                        {
                            if (DayShortageType == 2)
                            {
                                result.Add(new SharesBase
                                {
                                    SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                                    Market = (int)(item.Key % 10)
                                });
                            }
                            if (DayShortageType == 3)
                            {
                                continue;
                            }
                        }
                        int avgHandsRate = (int)avgquotes.Average(e => e.HandsRate);
                        if ((compare == 1 && disHandsRate < count * 1.0 / 10000 * avgHandsRate) || (compare == 2 && disHandsRate > count * 1.0 / 10000 * avgHandsRate))
                        {
                            continue;
                        }

                        bool isSuccess = true;
                        if (upOrDown > 0 && averageDay2>0)
                        {
                            //判断均线向上/向下
                            var avgupordownquotes = item.Value.Values.Skip(startDay1).ToList();
                            int tempLastAvgHandsRate = -1;
                            for (int idxday = 0; idxday < averageDay2; idxday++)
                            {
                                var tempList = avgupordownquotes.Skip(idxday).Take(averageDay1).ToList();
                                if (tempList.Count() <= 0)
                                {
                                    break;
                                }
                                int tempAvgHandsRate = (int)tempList.Average(e => e.HandsRate);
                                if (tempLastAvgHandsRate != -1 && ((upOrDown == 1 && tempAvgHandsRate > tempLastAvgHandsRate) || (upOrDown == 2 && tempAvgHandsRate < tempLastAvgHandsRate)))
                                {
                                    isSuccess = false;
                                    break;
                                }
                                tempLastAvgHandsRate = tempAvgHandsRate;
                            }
                        }
                        if (!isSuccess)
                        {
                            continue;
                        }
                        result.Add(new SharesBase
                        {
                            SharesCode = (item.Key / 10).ToString().PadLeft(6, '0'),
                            Market = (int)(item.Key % 10)
                        });
                    }
                    else 
                    {
                        return new List<SharesBase>();
                    }
                }

                if (startDay1 != 0 && endDay1 != 0 && startDay2 != 0 && endDay2 != 0)
                {
                    SetSearchSession(parKey, result);
                }
            }
            catch (Exception ex)
            {
                result = new List<SharesBase>();
                Logger.WriteFileLog("Analysis_Hands出错", ex);
            }
            return result;
        }

        private bool Analysis_Hands_BuildPar(string par,ref int handsType, ref int startDay1, ref int compare, ref int averageDay1, ref int count, ref int averageDay2, ref int upOrDown, ref int DayShortageType, ref int endDay1, ref int dealCompareType, ref int startDay2, ref int endDay2, ref int dealCompareType2) 
        {
            var temp = JsonConvert.DeserializeObject<dynamic>(par);
            try
            {
                handsType = Convert.ToInt32(temp.handsType);
            }
            catch (Exception ex)
            {
            }
            try
            {
                startDay1 = Convert.ToInt32(temp.startDay1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                compare = Convert.ToInt32(temp.Compare);
            }
            catch (Exception ex)
            {
            }
            try
            {
                averageDay1 = Convert.ToInt32(temp.averageDay1);
            }
            catch (Exception ex)
            {
            }
            try
            {
                count = (int)Math.Round(Convert.ToDouble(temp.count) * 100, 0);
            }
            catch (Exception)
            {
            }
            try
            {
                averageDay2 = Convert.ToInt32(temp.averageDay2);
                averageDay2 = averageDay2 + 1;
            }
            catch (Exception)
            {
            }
            try
            {
                upOrDown = Convert.ToInt32(temp.upOrDown);
            }
            catch (Exception)
            {
            }
            try
            {
                DayShortageType = Convert.ToInt32(temp.DayShortageType);
            }
            catch (Exception)
            {
            }
            try
            {
                endDay1 = Convert.ToInt32(temp.endDay1);
            }
            catch (Exception)
            {
            }
            try
            {
                dealCompareType = Convert.ToInt32(temp.dealCompareType);
            }
            catch (Exception)
            {
            }
            try
            {
                startDay2 = Convert.ToInt32(temp.startDay2);
            }
            catch (Exception)
            {
            }
            try
            {
                endDay2 = Convert.ToInt32(temp.endDay2);
            }
            catch (Exception)
            {
            }
            try
            {
                dealCompareType2 = Convert.ToInt32(temp.dealCompareType2);
            }
            catch (Exception)
            {
            }
            return true;
        }
        #endregion
    }
}
