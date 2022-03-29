using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using MealTicket_DBCommon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace MealTicket_Admin_Handler
{
    public class TradeCenterHandler
    {
        #region====行情管理====
        /// <summary>
        /// 获取今日行情股票列表
        /// </summary>
        /// <returns></returns>
        public PageRes<TodaySharesInfo> GetTodaySharesList(GetTodaySharesListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = from item in db.t_shares_today
                             join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                             select new { item, item2 };
                if (!string.IsNullOrEmpty(request.SharesCode))
                {
                    shares = from item in shares
                             where item.item.SharesCode.Contains(request.SharesCode) || item.item2.SharesName.Contains(request.SharesCode)
                             select item;
                }
                if (request.MaxId > 0)
                {
                    shares = from item in shares
                             where item.item.Id <= request.MaxId
                             select item;
                }
                int totalCount = shares.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = shares.Max(e => e.item.Id);
                }

                return new PageRes<TodaySharesInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in shares
                            orderby item.item.CreateTime descending
                            select new TodaySharesInfo
                            {
                                SharesCode = item.item.SharesCode,
                                Status = item.item.Status,
                                CreateTime = item.item.CreateTime,
                                OrderIndex = item.item.OrderIndex,
                                SharesName = item.item2.SharesName,
                                Id = item.item.Id,
                                Market = item.item.Market,
                                ShowName = item.item.ShowName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加今日行情股票
        /// </summary>
        public void AddTodayShares(AddTodaySharesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否存在
                var shares = (from item in db.t_shares_all
                              where item.Market == request.Market && item.SharesCode == request.SharesCode
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }

                //判断股票是否已经添加
                var sharesToday = (from item in db.t_shares_today
                                   where item.Market == request.Market && item.SharesCode == request.SharesCode
                                   select item).FirstOrDefault();
                if (sharesToday != null)
                {
                    throw new WebApiException(400, "该股票已添加");
                }

                db.t_shares_today.Add(new t_shares_today
                {
                    CreateTime = DateTime.Now,
                    SharesCode = request.SharesCode,
                    ShowName = request.ShowName,
                    Status = 1,
                    OrderIndex = request.OrderIndex,
                    LastModified = DateTime.Now,
                    Market = request.Market
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑今日行情股票
        /// </summary>
        /// <param name="request"></param>
        public void ModifyTodayShares(ModifyTodaySharesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否已经添加
                var sharesToday = (from item in db.t_shares_today
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (sharesToday == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                sharesToday.ShowName = request.ShowName;
                sharesToday.OrderIndex = request.OrderIndex;
                sharesToday.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改今日行情股票状态
        /// </summary>
        public void ModifyTodaySharesStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否存在
                var shares = (from item in db.t_shares_today
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                shares.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除今日行情股票
        /// </summary>
        public void DeleteTodayShares(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否存在
                var shares = (from item in db.t_shares_today
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                db.t_shares_today.Remove(shares);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取今日行情股票实时数据
        /// </summary>
        /// <returns></returns>
        public List<TodaySharesQuotesInfo> GetTodaySharesQuotes()
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_today
                              join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                              join item3 in db.v_shares_quotes_last on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into a
                              from ai in a.DefaultIfEmpty()
                              where item.Status == 1
                              orderby item.OrderIndex
                              select new TodaySharesQuotesInfo
                              {
                                  SharesName = item2.SharesName,
                                  ClosedPrice = ai == null ? 0 : ai.ClosedPrice,
                                  PresentPrice = ai == null ? 0 : ai.PresentPrice
                              }).ToList();
                return result;
            }
        }

        /// <summary>
        /// 获取股票热门搜索列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesHotSearchInfo> GetSharesHotSearch(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var searchTemp = from item in db.t_shares_search
                                 group item by new { item.Market, item.SharesCode, item.SharesName } into g
                                 select g;
                int totalCount = searchTemp.Count();
                var search = (from item in searchTemp
                              select new SharesHotSearchInfo
                              {
                                  SharesCode = item.Key.SharesCode,
                                  SharesName = item.Key.SharesName,
                                  Market = item.Key.Market,
                                  SearchCount = item.Count()
                              }).OrderByDescending(e => e.SearchCount).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in search)
                {
                    var temp = (from x in db.v_shares_quotes_last
                                where x.Market == item.Market && x.SharesCode == item.SharesCode
                                select x).FirstOrDefault();
                    if (temp == null)
                    {
                        item.ClosedPrice = 0;
                        item.PresentPrice = 0;
                    }
                    else
                    {
                        item.ClosedPrice = temp.ClosedPrice;
                        item.PresentPrice = temp.PresentPrice;
                    }
                }
                return new PageRes<SharesHotSearchInfo>
                {
                    List = search,
                    MaxId = 0,
                    TotalCount = totalCount
                };
            }
        }

        /// <summary>
        /// 获取所有股票列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesInfo> GetSharesList(GetSharesListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = from item in db.t_shares_all
                             select item;
                if (!string.IsNullOrEmpty(request.SharesCode))
                {
                    shares = from item in shares
                             where item.SharesCode.Contains(request.SharesCode) || item.SharesName.Contains(request.SharesCode) || item.SharesPyjc.Contains(request.SharesCode)
                             select item;
                }
                if (request.ForbidStatus != -1)
                {
                    shares = from item in shares
                             where item.ForbidStatus == request.ForbidStatus
                             select item;
                }
                if (request.MaxId > 0)
                {
                    shares = from item in shares
                             where item.Id <= request.MaxId
                             select item;
                }
                Regex regex0 = new Regex(Singleton.Instance.SharesCodeMatch0);
                Regex regex1 = new Regex(Singleton.Instance.SharesCodeMatch1);
                var sharesList = (from item in shares.AsEnumerable()
                                  where ((regex0.IsMatch(item.SharesCode) && item.Market == 0) || (regex1.IsMatch(item.SharesCode) && item.Market == 1))
                                  select item).ToList();
                int totalCount = sharesList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = sharesList.Max(e => e.Id);
                }

                DateTime timeNow = DateTime.Now;
                var suspension = from item in db.t_shares_suspension
                                 where item.Status == 1 && item.SuspensionStartTime <= timeNow && item.SuspensionEndTime > timeNow
                                 select item;

                DateTime MarketTime = DateTime.Parse("1900-01-01");
                var result = (from item in sharesList
                              join item2 in db.t_shares_markettime on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a from ai in a.DefaultIfEmpty()
                              join item3 in suspension on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into b from bi in b.DefaultIfEmpty()
                              orderby item.SharesCode
                              select new SharesInfo
                              {
                                  SharesCode = item.SharesCode,
                                  SharesNamePY = item.SharesPyjc,
                                  Status = item.Status,
                                  ForbidStatus = item.ForbidStatus,
                                  MarketStatus = item.MarketStatus,
                                  SharesName = item.SharesName,
                                  Id = item.Id,
                                  CirculatingCapital = ai == null ? 0 : ai.CirculatingCapital,
                                  TotalCapital = ai == null ? 0 : ai.TotalCapital,
                                  MarketTime = ai == null ? MarketTime : ai.MarketTime,
                                  Business = ai == null ? "" : ai.Business,
                                  Market = item.Market,
                                  IsSuspension = bi == null ? false : true,
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in result)
                {
                    var plate = (from x in db.t_shares_plate_rel
                                 join x2 in db.t_shares_plate on x.PlateId equals x2.Id
                                 where x.Market == item.Market && x.SharesCode == item.SharesCode
                                 select x2).ToList();
                    item.Industry = string.Join("/", plate.Where(e => e.Type == 1).Select(e => e.Name).ToList());
                    item.Area = string.Join("/", plate.Where(e => e.Type == 2).Select(e => e.Name).ToList());
                    item.Idea = string.Join("/", plate.Where(e => e.Type == 3).Select(e => e.Name).ToList());
                }
                if (request.ShowSharesQuotes)
                {
                    result = (from x in result
                              join x2 in db.v_shares_quotes_last on new { x.Market, x.SharesCode } equals new { x2.Market, x2.SharesCode } into b
                              from bi in b.DefaultIfEmpty()
                              select new SharesInfo
                              {
                                  SharesCode = x.SharesCode,
                                  SharesNamePY = x.SharesNamePY,
                                  Status = x.Status,
                                  ForbidStatus = x.ForbidStatus,
                                  SharesName = x.SharesName,
                                  Id = x.Id,
                                  MarketStatus = x.MarketStatus,
                                  MarketTime = x.MarketTime,
                                  Market = x.Market,
                                  IsSuspension = x.IsSuspension,
                                  Area = x.Area,
                                  Idea = x.Idea,
                                  Industry = x.Industry,
                                  Business = x.Business,
                                  CirculatingCapital = x.CirculatingCapital,
                                  TotalCapital = x.TotalCapital,
                                  SharesQuotes = bi == null ? new SharesQuotes() : new SharesQuotes
                                  {
                                      ClosedPrice = bi.ClosedPrice,
                                      MaxPrice = bi.MaxPrice,
                                      MinPrice = bi.MinPrice,
                                      OpenedPrice = bi.OpenedPrice,
                                      PresentCount = bi.PresentCount,
                                      PresentPrice = bi.PresentPrice,
                                      TotalAmount = bi.TotalAmount,
                                      TotalCount = bi.TotalCount
                                  }
                              }).ToList();
                }
                return new PageRes<SharesInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 修改股票状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = (from item in db.t_shares_all
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                shares.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改股票限制状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesForbidStatus(ModifySharesForbidStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = (from item in db.t_shares_all
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                shares.ForbidStatus = request.ForbidStatus;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改股票退市状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMarketStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = (from item in db.t_shares_all
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                shares.MarketStatus = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取停牌设置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesSuspensionInfo> GetSharesSuspensionList(GetSharesSuspensionListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var suspensionList = from item in db.t_shares_suspension
                                     select item;
                if (!string.IsNullOrEmpty(request.SharesCode))
                {
                    suspensionList = from item in suspensionList
                                     where item.SharesCode.Contains(request.SharesCode)
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    suspensionList = from item in suspensionList
                                     where item.Id <= request.MaxId
                                     select item;
                }
                int totalCount = suspensionList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = suspensionList.Max(e => e.Id);
                }
                return new PageRes<SharesSuspensionInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in suspensionList
                            orderby item.CreateTime descending
                            select new SharesSuspensionInfo
                            {
                                SharesCode = item.SharesCode,
                                Status = item.Status,
                                SharesName = item.SharesName,
                                SuspensionStartTime = item.SuspensionStartTime,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                Market = item.Market,
                                SuspensionEndTime = item.SuspensionEndTime
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改停牌设置状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesSuspensionStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var suspension = (from item in db.t_shares_suspension
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (suspension == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                suspension.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入停/复牌数据
        /// </summary>
        public void BatchModifySharesSuspensionStatus(int type, List<SuspensionInfo> list)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                foreach (var item in list)
                {
                    if (type == 1)//停牌
                    {
                        db.t_shares_suspension.Add(new t_shares_suspension
                        {
                            SharesCode = item.SharesCode,
                            Status = 1,
                            SharesName = item.SharesName,
                            SuspensionEndTime = item.SuspensionEndTime,
                            CreateTime = timeNow,
                            SuspensionStartTime = item.SuspensionStartTime,
                            LastModified = timeNow,
                            Market = item.Market
                        });
                    }
                    else//复牌
                    {
                        var suspension = (from x in db.t_shares_suspension
                                          where x.Market == item.Market && x.SharesCode == item.SharesCode && x.Status == 1 && x.SuspensionStartTime <= timeNow && x.SuspensionEndTime > timeNow
                                          select x).ToList();
                        foreach (var y in suspension)
                        {
                            y.Status = 2;
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入上市时间数据
        /// </summary>
        /// <param name="list"></param>
        public void BatchModifySharesMarketTime(List<MarketTimeInfo> list)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var plateList = (from x in db.t_shares_plate
                                     select x).ToList();
                    Task[] tArr = new Task[list.Count()];
                    int i = 0;
                    foreach (var item in list)
                    {
                        tArr[i] = new Task(() =>
                          {
                              try
                              {
                                  string sql = "update t_shares_markettime set ";
                                  string sqlApp = "";
                                  if (item.MarketTime > DateTime.Parse("1970-01-01"))
                                  {
                                      sqlApp += string.Format("MarketTime='{0}'", item.MarketTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                      sqlApp += ",";
                                  }
                                  if (!string.IsNullOrEmpty(item.Business))
                                  {
                                      sqlApp += string.Format("Business='{0}'", item.Business.Trim());
                                      sqlApp += ",";
                                  }
                                  if (item.TotalCapital > 0)
                                  {
                                      sqlApp += string.Format("TotalCapital={0}", item.TotalCapital);
                                      sqlApp += ",";
                                  }
                                  if (item.CirculatingCapital > 0)
                                  {
                                      sqlApp += string.Format("CirculatingCapital={0}", item.CirculatingCapital);
                                      sqlApp += ",";
                                  }
                                  if (!string.IsNullOrEmpty(sqlApp))
                                  {
                                      sqlApp = sqlApp.Substring(0, sqlApp.Length - 1);
                                      sqlApp += string.Format(" where Market={0} and SharesCode='{1}'", item.Market, item.SharesCode);
                                      sql += sqlApp;

                                      if (db.Database.ExecuteSqlCommand(sql) < 1)
                                      {
                                          sql = string.Format("insert into t_shares_markettime(SharesCode,CreateTime,SharesName,Business,Industry,Market,MarketTime,CirculatingCapital,TotalCapital) values('{0}','{1}','{2}','{3}',null,{4},'{5}',{6},{7})", item.SharesCode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), item.SharesName, item.Business, item.Market, item.MarketTime.ToString("yyyy-MM-dd HH:mm:ss"), item.CirculatingCapital, item.TotalCapital);
                                          db.Database.ExecuteSqlCommand(sql);
                                      }
                                  }

                                  sql = string.Format("delete t_shares_plate_rel where Market={0} and SharesCode='{1}'", item.Market, item.SharesCode);
                                  db.Database.ExecuteSqlCommand(sql);


                                  //解析行业
                                  string[] IndustryArr = item.Industry.Split('/');
                                  List<string> IndustryList = new List<string>();
                                  for (int idx = 0; idx < IndustryArr.Length; idx++)
                                  {
                                      IndustryList.Add(IndustryArr[idx].Trim());
                                  }
                                  //解析地区
                                  string[] AresArr = item.Area.Split('/');
                                  List<string> AresList = new List<string>();
                                  for (int idx = 0; idx < AresArr.Length; idx++)
                                  {
                                      AresList.Add(AresArr[idx].Trim());
                                  }
                                  //解析概念
                                  string[] IdeaArr = item.Idea.Split('/');
                                  List<string> IdeaList = new List<string>();
                                  for (int idx = 0; idx < IdeaArr.Length; idx++)
                                  {
                                      IdeaList.Add(IdeaArr[idx].Trim());
                                  }



                                  var plateListTemp = (from x in plateList
                                                       where ((x.Type == 1 && IndustryList.Contains(x.Name.Trim())) || (x.Type == 2 && AresList.Contains(x.Name.Trim())) || (x.Type == 3 && IdeaList.Contains(x.Name.Trim()))) && !string.IsNullOrEmpty(x.Name.Trim())
                                                       select x).ToList();
                                  sql = "";
                                  foreach (var x in plateListTemp)
                                  {
                                      sql += string.Format("insert into t_shares_plate_rel(SharesCode,Market,CreateTime,PlateId) values('{0}',{1},'{2}',{3}); ", item.SharesCode, item.Market, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), x.Id);
                                  }
                                  db.Database.ExecuteSqlCommand(sql);
                              }
                              catch (Exception ex)
                              {
                                  Logger.WriteFileLog("基础信息导入有错,股票：" + item.SharesName + "(" + item.SharesCode + ")", ex);
                              }

                          });
                        tArr[i].Start();
                        i++;
                    }
                    Task.WaitAll(tArr);

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("基础信息导入有错", ex);
                    tran.Rollback();
                    throw new WebApiException(400, "基础信息导入出错");
                }
            }
        }

        /// <summary>
        /// 获取股票分笔明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesTransactionDataInfo> GetSharesTransactionDataList(GetSharesTransactionDataListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var transactiondata = from item in db.t_shares_transactiondata
                                      where item.Market == request.Market && item.SharesCode == request.SharesCode
                                      select item;
                if (request.StartTime != null && request.EndTime != null)
                {
                    transactiondata = from item in transactiondata
                                      where item.Time >= request.StartTime && item.Time <= request.EndTime
                                      select item;
                }
                int totalCount = transactiondata.Count();

                return new PageRes<SharesTransactionDataInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in transactiondata
                            orderby item.Time descending, item.OrderIndex descending
                            select new SharesTransactionDataInfo
                            {
                                Id = item.Id,
                                Stock = item.Stock,
                                Price = item.Price,
                                Time = item.Time,
                                TimeStr = item.TimeStr,
                                Type = item.Type,
                                LastModified = item.LastModified
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 重置股票分笔明细
        /// </summary>
        /// <param name="request"></param>
        public void ResetSharesTransactionData(ResetSharesTransactionDataRequest request)
        {
            DateTime startTime = request.StartDate.Date;
            DateTime endTime = request.EndDate;
            while (startTime <= endTime)
            {
                try
                {
                    var sendData = new
                    {
                        Market = request.Market,
                        SharesCode = request.SharesCode,
                        Type = 2,
                        Date = startTime.ToString("yyyy-MM-dd")
                    };
                    Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Update");
                }
                catch (Exception)
                { }
                startTime = startTime.AddDays(1);
            }
        }

        /// <summary>
        /// 获取股票勘误列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesCorrigendumInfo> GetSharesCorrigendumList(GetSharesCorrigendumListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_shares_all_corrigendum
                           select item;
                if (!string.IsNullOrEmpty(request.SharesCode))
                {
                    list = from item in list
                           where item.SharesCode.Contains(request.SharesCode) || item.SharesName.Contains(request.SharesCode)
                           select item;
                }

                int totalCount = list.Count();

                return new PageRes<SharesCorrigendumInfo>
                {
                    TotalCount = totalCount,
                    MaxId = 0,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new SharesCorrigendumInfo
                            {
                                SharesCode = item.SharesCode,
                                SharesPyjc = item.SharesPyjc,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                Market = item.Market,
                                SharesName = item.SharesName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票勘误
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesCorrigendum(AddSharesCorrigendumRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否已经添加
                var shares = (from item in db.t_shares_all_corrigendum
                              where item.Market == request.Market && item.SharesCode == request.SharesCode
                              select item).FirstOrDefault();
                if (shares != null)
                {
                    throw new WebApiException(400, "该股票已存在");
                }

                db.t_shares_all_corrigendum.Add(new t_shares_all_corrigendum
                {
                    CreateTime = DateTime.Now,
                    SharesName = request.SharesName,
                    SharesPyjc = request.SharesPyjc,
                    Market = request.Market,
                    LastModified = DateTime.Now,
                    SharesCode = request.SharesCode
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑股票勘误
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesCorrigendum(ModifySharesCorrigendumRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = (from item in db.t_shares_all_corrigendum
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                shares.SharesPyjc = request.SharesPyjc;
                shares.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除股票勘误
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesCorrigendum(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares = (from item in db.t_shares_all_corrigendum
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_shares_all_corrigendum.Remove(shares);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取各类型板块数量
        /// </summary>
        /// <returns></returns>
        public List<SharesPlateTypeCount> GetSharesPlateTypeCount() 
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = from item in db.t_shares_plate
                            where item.Status == 1 && item.ChooseStatus == 1
                            select item;

                var plateType = (from item in db.t_shares_plate_type_business
                                 join item2 in plate on item.Id equals item2.Type into a from ai in a.DefaultIfEmpty()
                                 group new { item,ai} by item into g
                                 orderby g.Key.Id
                                 select new SharesPlateTypeCount
                                 {
                                     CreateTime = g.Key.LastModified,
                                     Id = g.Key.Id,
                                     IsBasePlate = g.Key.IsBasePlate,
                                     Name = g.Key.Name,
                                     PlateCount=g.Where(e=>e.ai!=null).Count()
                                 }).ToList();
                return plateType;
            }
        }

        /// <summary>
        /// 修改类型板块是否包含基础板块
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPlateTypeIsBasePlate(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var plateType = (from item in db.t_shares_plate_type_business
                                where item.Id==request.Id
                                 select item).FirstOrDefault();
                if (plateType == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                plateType.IsBasePlate = request.Status;
                plateType.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取板块管理列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesPlateInfo> GetSharesPlateList(GetSharesPlateListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.v_plate_new
                           select item;

                if (request.Type != 0)
                {
                    list = from item in list
                           where item.PlateType == request.Type
                           select item;
                }
                if (request.ChooseStatus != 0)
                {
                    if (request.Level == 1)
                    {
                        list = from item in list
                               join item2 in db.t_shares_plate_type_business on item.PlateType equals item2.Id
                               where item.ChooseStatus == request.ChooseStatus || (item2.IsBasePlate == 1 && item.BaseStatus == 1)
                               select item;
                    }
                    else
                    {
                        list = from item in list
                               where item.ChooseStatus == request.ChooseStatus
                               select item;
                    }
                }
                if (!string.IsNullOrEmpty(request.Name))
                {
                    list = from item in list
                           where item.PlateName.Contains(request.Name)
                           select item;
                }
                if (request.Level == 1)
                {
                    return new PageRes<SharesPlateInfo>
                    {
                        MaxId = 0,
                        TotalCount = 0,
                        List = (from item in list
                                orderby item.PlateName
                                select new SharesPlateInfo
                                {
                                    Id = item.PlateId,
                                    Type = item.PlateType,
                                    Name = item.PlateName
                                }).ToList()
                    };
                }

                int totalCount = list.Count();
                int validCount = list.Where(e => e.Status == 1).Count();
                int baseCount= list.Where(e => e.Status == 1 && e.BaseStatus==1).Count();

                List<SharesPlateInfo> result = new List<SharesPlateInfo>();
                if (request.OrderType == 1)
                {
                    if (request.OrderMethod == "ascending")
                    {
                        result = (from item in list
                                  join item2 in db.t_shares_all on new { Market = item.WeightMarket, SharesCode = item.WeightSharesCode } equals new { item2.Market, item2.SharesCode } into a
                                  from ai in a.DefaultIfEmpty()
                                  join item3 in db.t_shares_all on new { Market = item.NoWeightMarket, SharesCode = item.NoWeightSharesCode } equals new { item3.Market, item3.SharesCode } into b
                                  from bi in b.DefaultIfEmpty()
                                  orderby item.RiseRate
                                  select new SharesPlateInfo
                                  {
                                      Status = item.Status,
                                      CreateTime = item.CreateTime,
                                      Id = item.PlateId,
                                      Type = item.PlateType,
                                      SharesCount = item.SharesCount??0,
                                      Name = item.PlateName,
                                      SharesCode = item.SharesCode,
                                      SharesMarket = item.SharesMarket,
                                      SharesType = item.SharesType,
                                      WeightSharesCode = item.WeightSharesCode,
                                      NoWeightSharesCode = item.NoWeightSharesCode,
                                      NoWeightMarket = item.NoWeightMarket,
                                      WeightMarket = item.WeightMarket,
                                      WeightSharesName = ai == null ? "" : ai.SharesName,
                                      NoWeightSharesName = bi == null ? "" : bi.SharesName,
                                      RiseRate = item.RiseRate,
                                      BaseStatus = item.BaseStatus,
                                      RiseIndex = item.RiseIndex,
                                      WeightRiseIndex = item.WeightRiseIndex,
                                      WeightRiseRate = item.WeightRiseRate,
                                      CalType = item.CalType,
                                      BaseDate=item.BaseDate
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                    else
                    {
                        result = (from item in list
                                  join item2 in db.t_shares_all on new { Market = item.WeightMarket, SharesCode = item.WeightSharesCode } equals new { item2.Market, item2.SharesCode } into a
                                  from ai in a.DefaultIfEmpty()
                                  join item3 in db.t_shares_all on new { Market = item.NoWeightMarket, SharesCode = item.NoWeightSharesCode } equals new { item3.Market, item3.SharesCode } into b
                                  from bi in b.DefaultIfEmpty()
                                  orderby item.RiseRate descending
                                  select new SharesPlateInfo
                                  {
                                      Status = item.Status,
                                      CreateTime = item.CreateTime,
                                      Id = item.PlateId,
                                      Type = item.PlateType,
                                      SharesCount = item.SharesCount ?? 0,
                                      Name = item.PlateName,
                                      SharesCode = item.SharesCode,
                                      SharesMarket = item.SharesMarket,
                                      SharesType = item.SharesType,
                                      WeightSharesCode = item.WeightSharesCode,
                                      NoWeightSharesCode = item.NoWeightSharesCode,
                                      NoWeightMarket = item.NoWeightMarket,
                                      WeightMarket = item.WeightMarket,
                                      WeightSharesName = ai == null ? "" : ai.SharesName,
                                      NoWeightSharesName = bi == null ? "" : bi.SharesName,
                                      RiseRate = item.RiseRate,
                                      BaseStatus = item.BaseStatus,
                                      RiseIndex = item.RiseIndex,
                                      WeightRiseIndex = item.WeightRiseIndex,
                                      WeightRiseRate = item.WeightRiseRate,
                                      CalType = item.CalType,
                                      BaseDate = item.BaseDate
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                    }
                }
                else
                {
                    result = (from item in list
                              join item2 in db.t_shares_all on new { Market = item.WeightMarket, SharesCode = item.WeightSharesCode } equals new { item2.Market, item2.SharesCode } into a
                              from ai in a.DefaultIfEmpty()
                              join item3 in db.t_shares_all on new { Market = item.NoWeightMarket, SharesCode = item.NoWeightSharesCode } equals new { item3.Market, item3.SharesCode } into b
                              from bi in b.DefaultIfEmpty()
                              orderby item.CreateTime descending, item.PlateId descending
                              select new SharesPlateInfo
                              {
                                  Status = item.Status,
                                  CreateTime = item.CreateTime,
                                  Id = item.PlateId,
                                  Type = item.PlateType,
                                  SharesCount = item.SharesCount ?? 0,
                                  Name = item.PlateName,
                                  SharesCode = item.SharesCode,
                                  SharesMarket = item.SharesMarket,
                                  SharesType = item.SharesType,
                                  WeightSharesCode = item.WeightSharesCode,
                                  NoWeightSharesCode = item.NoWeightSharesCode,
                                  NoWeightMarket = item.NoWeightMarket,
                                  WeightMarket = item.WeightMarket,
                                  WeightSharesName = ai == null ? "" : ai.SharesName,
                                  NoWeightSharesName = bi == null ? "" : bi.SharesName,
                                  RiseRate = item.RiseRate,
                                  BaseStatus = item.BaseStatus,
                                  RiseIndex = item.RiseIndex,
                                  WeightRiseIndex = item.WeightRiseIndex,
                                  WeightRiseRate = item.WeightRiseRate,
                                  CalType = item.CalType,
                                  BaseDate = item.BaseDate
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                }


                return new PageRes<SharesPlateInfo>
                {
                    MaxId = 0,
                    BaseCount=baseCount,
                    ValidCount=validCount,
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 添加板块管理
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPlate(AddSharesPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断名称是否存在
                var plate = (from item in db.t_shares_plate
                             where item.Type == request.Type && item.Name == request.Name
                             select item).FirstOrDefault();
                if (plate != null)
                {
                    throw new WebApiException(400, "已添加");
                }

                db.t_shares_plate.Add(new t_shares_plate
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Name = request.Name,
                    Type = request.Type,
                    SharesMarket = request.SharesMarket,
                    SharesCode = request.SharesCode,
                    SharesType = request.SharesType,
                    WeightSharesCode = request.WeightSharesCode,
                    NoWeightSharesCode = request.NoWeightSharesCode,
                    NoWeightMarket = request.NoWeightMarket,
                    WeightMarket = request.WeightMarket,
                    CalType = request.CalType

                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入板块数据
        /// </summary>
        /// <param name="list"></param>
        public int BatchAddSharesPlate(List<string> list, int type)
        {
            using (var db = new meal_ticketEntities())
            {
                int resultCount = 0;
                foreach (var name in list)
                {
                    //判断是否存在
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == type && x.Name == name
                                 select x).FirstOrDefault();
                    if (plate != null)
                    {
                        continue;
                    }
                    db.t_shares_plate.Add(new t_shares_plate
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = name,
                        Type = type
                    });
                    resultCount++;
                }
                db.SaveChanges();
                return resultCount;
            }
        }

        /// <summary>
        /// 编辑板块管理
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPlate(ModifySharesPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = (from item in db.t_shares_plate
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (plate == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断名称是否存在
                var temp = (from item in db.t_shares_plate
                            where item.Type == plate.Type && item.Name == request.Name && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "已添加");
                }

                plate.Name = request.Name;
                plate.SharesType = request.SharesType;
                plate.SharesCode = request.SharesCode;
                plate.SharesMarket = request.SharesMarket;
                plate.WeightSharesCode = request.WeightSharesCode;
                plate.NoWeightSharesCode = request.NoWeightSharesCode;
                plate.WeightMarket = request.WeightMarket;
                plate.NoWeightMarket = request.NoWeightMarket;
                plate.CalType = request.CalType;
                plate.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改板块管理状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPlateStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = (from item in db.t_shares_plate
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (plate == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                plate.Status = request.Status;
                plate.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改板块管理状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPlateBaseStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = (from item in db.t_shares_plate
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (plate == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                plate.BaseStatus = request.Status;
                plate.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改板块管理状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPlateChooseStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = (from item in db.t_shares_plate
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (plate == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                plate.ChooseStatus = request.Status;
                plate.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量修改板块管理挑选状态
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int ModifySharesPlateChooseStatusBatch(List<string> list,int plateType)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //查询板块
                    var plate = (from item in db.t_shares_plate
                                 select item).ToList();
                    int i = 0;
                    foreach (var item in list)
                    {
                        var tempplate = plate.Where(e => e.Name == item && e.Type == plateType).FirstOrDefault();
                        if (tempplate == null)
                        {
                            continue;
                        }
                        tempplate.ChooseStatus = 1;
                        tempplate.LastModified = DateTime.Now;
                        db.SaveChanges();
                        i++;
                    }
                    tran.Commit();
                    return i;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 批量修改板块管理挑选状态(删除)
        /// </summary>
        public void BatchDeleteSharesPlateChooseStatus(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //查询板块
                    var plate = (from item in db.t_shares_plate
                                 where item.ChooseStatus== 1 && item.Type == request.Id
                                 select item).ToList();
                    foreach (var item in plate)
                    {
                        item.ChooseStatus = 2;
                    }
                    db.SaveChanges();
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
        /// 批量修改板块管理基础版块状态
        /// </summary>
        /// <param name="list"></param>
        /// <param name="plateType"></param>
        /// <returns></returns>
        public int ModifySharesPlateBaseStatusBatch(List<string> list, int plateType)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //查询板块
                    var plate = (from item in db.t_shares_plate
                                 select item).ToList();
                    int i = 0;
                    foreach (var item in list)
                    {
                        var tempplate = plate.Where(e => e.Name == item && e.Type == plateType).FirstOrDefault();
                        if (tempplate == null)
                        {
                            continue;
                        }
                        tempplate.BaseStatus = 1;
                        tempplate.LastModified = DateTime.Now;
                        db.SaveChanges();
                        i++;
                    }
                    tran.Commit();
                    return i;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 批量修改板块管理基础版块状态(删除)
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteSharesPlateBaseStatus(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //查询板块
                    var plate = (from item in db.t_shares_plate
                                 where item.BaseStatus == 1 && item.Type== request.Id
                                 select item).ToList();
                    foreach (var item in plate)
                    {
                        item.BaseStatus = 0;
                    }
                    db.SaveChanges();
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
        /// 删除板块管理
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPlate(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var plate = (from item in db.t_shares_plate
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                    if (plate == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }

                    db.t_shares_plate.Remove(plate);
                    db.SaveChanges();

                    var plateRel = (from item in db.t_shares_plate_rel
                                    where item.PlateId == request.Id
                                    select item).ToList();
                    if (plateRel.Count() > 0)
                    {
                        db.t_shares_plate_rel.RemoveRange(plateRel);
                        db.SaveChanges();
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
        /// 批量删除板块管理
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteSharesPlate(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var plate = (from item in db.t_shares_plate
                                 where item.Type == request.Id
                                 select item).ToList();
                    List<long> PlateIdList = plate.Select(e => e.Id).ToList();
                    if (plate.Count() > 0)
                    {
                        db.t_shares_plate.RemoveRange(plate);
                        db.SaveChanges();
                    }

                    var plateRel = (from item in db.t_shares_plate_rel
                                    where PlateIdList.Contains(item.PlateId)
                                    select item).ToList();
                    if (plateRel.Count() > 0)
                    {
                        db.t_shares_plate_rel.RemoveRange(plateRel);
                        db.SaveChanges();
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
        /// 重置板块基准日
        /// </summary>
        /// <param name="request"></param>
        public void ResetSharesPlateBaseDate(ResetSharesPlateBaseDateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var plate = (from item in db.t_shares_plate
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                    if (plate == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }

                    if (plate.BaseDate == request.BaseDate)
                    {
                        return;
                    }

                    plate.BaseDate = request.BaseDate;
                    db.SaveChanges();

                    var temp = (from item in db.t_shares_plate_rel_snapshot_instructions
                                where item.IsExcute == false && item.Type == 1 && item.PlateId == request.Id
                                select item).FirstOrDefault();
                    if (temp == null)
                    {
                        db.t_shares_plate_rel_snapshot_instructions.Add(new t_shares_plate_rel_snapshot_instructions
                        {
                            SharesCode="",
                            Market=0,
                            ExcuteTime=null,
                            Context = JsonConvert.SerializeObject(new
                            {
                                Date = request.BaseDate
                            }),
                            CreateTime = DateTime.Now,
                            DataType = 0,
                            Date = DateTime.Now.Date,
                            IsExcute = false,
                            LastModified = DateTime.Now,
                            PlateId = request.Id,
                            PlateType = 0,
                            Type = 1
                        });
                    }
                    else
                    {
                        temp.LastModified = DateTime.Now;
                        temp.Context = JsonConvert.SerializeObject(new
                        {
                            Date = request.BaseDate
                        });
                    }
                    db.SaveChanges();

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
        /// 获取板块股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesInfo> GetSharesPlateSharesList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.v_plate_shares
                           join item2 in db.t_shares_all on new { Market = item.Market.Value, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                           where item.PlateId == request.Id && item.Market != null
                           select new { item, item2 };
                int totalCount = list.Count();
                return new PageRes<SharesInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.item.CreateTime descending
                            select new SharesInfo
                            {
                                Id = item.item.RelId??0,
                                SharesCode = item.item.SharesCode,
                                SharesName = item.item2.SharesName,
                                SharesNamePY = item.item2.SharesPyjc,
                                Market = item.item.Market.Value
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加板块股票
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPlateShares(AddSharesPlateSharesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断板块是否存在
                var plate = (from item in db.t_shares_plate
                             where item.Id == request.PlateId
                             select item).FirstOrDefault();
                if (plate == null)
                {
                    throw new WebApiException(400, "板块不存在");
                }
                //判断股票是否存在
                var shares = (from item in db.t_shares_all
                              where item.Market == request.Market && item.SharesCode == request.SharesCode
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                //判断是否已添加
                var plateRel = (from item in db.t_shares_plate_rel
                                where item.PlateId == request.PlateId && item.Market == request.Market && item.SharesCode == request.SharesCode
                                select item).FirstOrDefault();
                if (plateRel != null)
                {
                    throw new WebApiException(400, "股票已添加");
                }

                db.t_shares_plate_rel.Add(new t_shares_plate_rel
                {
                    SharesCode = request.SharesCode,
                    CreateTime = DateTime.Now,
                    Market = request.Market,
                    PlateId = request.PlateId
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入板块股票
        /// </summary>
        /// <returns></returns>
        public int BatchAddSharesPlateShares(List<SharesInfo> list, long plateId)
        {
            using (var db = new meal_ticketEntities())
            {
                int resultCount = 0;
                foreach (var item in list)
                {
                    //判断是否存在
                    var plateSHares = (from x in db.t_shares_plate_rel
                                       where x.PlateId == plateId && x.Market == item.Market && x.SharesCode == item.SharesCode
                                       select x).FirstOrDefault();
                    if (plateSHares != null)
                    {
                        continue;
                    }
                    db.t_shares_plate_rel.Add(new t_shares_plate_rel
                    {
                        PlateId = plateId,
                        CreateTime = DateTime.Now,
                        SharesCode = item.SharesCode,
                        Market = item.Market
                    });
                    resultCount++;
                }
                db.SaveChanges();
                return resultCount;
            }
        }

        /// <summary>
        /// 删除板块股票
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPlateShares(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var plateRel = (from item in db.t_shares_plate_rel
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (plateRel == null)
                {
                    throw new WebApiException(400, "数据集不存在");
                }
                db.t_shares_plate_rel.Remove(plateRel);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询板块涨跌幅过滤列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesForbidInfo> GetSharesPlateForbidList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_shares_limit
                             select item;
                int totalCount = result.Count();

                return new PageRes<SharesForbidInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            orderby item.CreateTime descending
                            select new SharesForbidInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                LimitKey = item.LimitKey,
                                LimitMarket = item.LimitMarket,
                                LimitType = item.LimitType
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加板块涨跌幅过滤
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPlateForbid(AddSharesForbidRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_plate_shares_limit.Add(new t_shares_plate_shares_limit
                {
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    LimitKey = request.LimitKey,
                    LimitMarket = request.LimitMarket,
                    LimitType = request.LimitType
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑板块涨跌幅过滤
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPlateForbid(ModifySharesForbidRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var forbidShares = (from item in db.t_shares_plate_shares_limit
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (forbidShares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                forbidShares.LastModified = DateTime.Now;
                forbidShares.LimitKey = request.LimitKey;
                forbidShares.LimitMarket = request.LimitMarket;
                forbidShares.LimitType = request.LimitType;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除板块涨跌幅过滤
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPlateForbid(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var forbidShares = (from item in db.t_shares_plate_shares_limit
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (forbidShares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_plate_shares_limit.Remove(forbidShares);
                db.SaveChanges();
            }
        }
        #endregion

        #region====交易管理====
        /// <summary>
        /// 查询禁止名单列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesForbidInfo> GetSharesForbidList(GetSharesForbidListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var forbidShares = from item in db.t_shares_limit
                                   select item;
                if (!string.IsNullOrEmpty(request.LimitKey))
                {
                    forbidShares = from item in forbidShares
                                   where item.LimitKey.Contains(request.LimitKey)
                                   select item;
                }
                if (request.ForbidType != 0)
                {
                    forbidShares = from item in forbidShares
                                   where item.ForbidType == request.ForbidType
                                   select item;
                }
                if (request.LimitMarket != -2)
                {
                    forbidShares = from item in forbidShares
                                   where (item.LimitMarket == request.LimitMarket || item.LimitMarket == -1)
                                   select item;
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    forbidShares = from item in forbidShares
                                   where item.CreateTime >= request.StartTime && item.CreateTime < request.EndTime
                                   select item;
                }
                if (request.LimitType != 0)
                {
                    forbidShares = from item in forbidShares
                                   where item.LimitType == request.LimitType
                                   select item;
                }
                if (request.MaxId > 0)
                {
                    forbidShares = from item in forbidShares
                                   where item.Id <= request.MaxId
                                   select item;
                }

                int totalCount = forbidShares.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = forbidShares.Max(e => e.Id);
                }

                return new PageRes<SharesForbidInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in forbidShares
                            orderby item.CreateTime descending
                            select new SharesForbidInfo
                            {
                                CreateTime = item.CreateTime,
                                ForbidType = item.ForbidType,
                                Id = item.Id,
                                LimitKey = item.LimitKey,
                                LimitMarket = item.LimitMarket,
                                LimitType = item.LimitType
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加禁止股票名单
        /// </summary>
        public void AddSharesForbid(AddSharesForbidRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit.Add(new t_shares_limit
                {
                    CreateTime = DateTime.Now,
                    ForbidType = request.ForbidType,
                    LastModified = DateTime.Now,
                    LimitKey = request.LimitKey,
                    LimitMarket = request.LimitMarket,
                    LimitType = request.LimitType
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑禁止股票名单
        /// </summary>
        public void ModifySharesForbid(ModifySharesForbidRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var forbidShares = (from item in db.t_shares_limit
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (forbidShares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                forbidShares.ForbidType = request.ForbidType;
                forbidShares.LastModified = DateTime.Now;
                forbidShares.LimitKey = request.LimitKey;
                forbidShares.LimitMarket = request.LimitMarket;
                forbidShares.LimitType = request.LimitType;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除禁止股票名单
        /// </summary>
        public void DeleteSharesForbid(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var forbidShares = (from item in db.t_shares_limit
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (forbidShares == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit.Remove(forbidShares);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询禁止日期分组列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesDateGroupForbidInfo> GetSharesDateGroupForbidList(GetSharesDateGroupForbidListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateGroup = from item in db.t_shares_limit_date_group
                                select item;
                if (!string.IsNullOrEmpty(request.GroupName))
                {
                    dateGroup = from item in dateGroup
                                where item.GroupName.Contains(request.GroupName)
                                select item;
                }
                if (request.MaxId > 0)
                {
                    dateGroup = from item in dateGroup
                                where item.Id <= request.MaxId
                                select item;
                }
                int totalCount = dateGroup.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = dateGroup.Max(e => e.Id);
                }

                return new PageRes<SharesDateGroupForbidInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in dateGroup
                            orderby item.CreateTime descending
                            select new SharesDateGroupForbidInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                GroupName = item.GroupName,
                                Id = item.Id,
                                DateCount = (from x in db.t_shares_limit_date
                                             where x.GroupId == item.Id
                                             select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加禁止日期分组
        /// </summary>
        public void AddSharesDateGroupForbid(AddSharesDateGroupForbidRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_date_group.Add(new t_shares_limit_date_group
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    GroupName = request.GroupName,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑禁止日期分组
        /// </summary>
        public void ModifySharesDateGroupForbid(ModifySharesDateGroupForbidRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateGroup = (from item in db.t_shares_limit_date_group
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                if (dateGroup == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                dateGroup.GroupName = request.GroupName;
                dateGroup.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改禁止日期分组状态
        /// </summary>
        public void ModifySharesDateGroupForbidStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateGroup = (from item in db.t_shares_limit_date_group
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                if (dateGroup == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                dateGroup.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除禁止日期分组
        /// </summary>
        public void DeleteSharesDateGroupForbid(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateGroup = (from item in db.t_shares_limit_date_group
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                if (dateGroup == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_date_group.Remove(dateGroup);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询禁止日期列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesDateForbidInfo> GetSharesDateForbidList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateForbid = from item in db.t_shares_limit_date
                                 where item.GroupId == request.Id
                                 select item;
                if (request.MaxId > 0)
                {
                    dateForbid = from item in dateForbid
                                 where item.Id <= request.MaxId
                                 select item;
                }
                int totalCount = dateForbid.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = dateForbid.Max(e => e.Id);
                }

                var list = (from item in dateForbid
                            orderby item.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                return new PageRes<SharesDateForbidInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new SharesDateForbidInfo
                            {
                                Status = item.Status,
                                BeginDate = item.BeginDate.ToString("yyyy-MM-dd"),
                                EndDate = item.EndDate.ToString("yyyy-MM-dd"),
                                CreateTime = item.CreateTime,
                                Id = item.Id
                            }).ToList()
                };
            }
        }

        /// <summary>
        /// 添加禁止日期
        /// </summary>
        public void AddSharesDateForbid(AddSharesDateForbidRequest request)
        {
            DateTime beginDate;
            DateTime endDate;
            if (!DateTime.TryParseExact(request.BeginDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out beginDate))
            {
                throw new WebApiException(400, "时间参数错误");
            }
            if (!DateTime.TryParseExact(request.EndDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
            {
                throw new WebApiException(400, "时间参数错误");
            }
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_shares_limit_date_group
                                 where item.Id == request.GroupId
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                db.t_shares_limit_date.Add(new t_shares_limit_date
                {
                    Status = 1,
                    BeginDate = beginDate,
                    CreateTime = DateTime.Now,
                    EndDate = endDate,
                    GroupId = request.GroupId,
                    LastModified = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑禁止日期
        /// </summary>
        public void ModifySharesDateForbid(ModifySharesDateForbidRequest request)
        {
            DateTime beginDate;
            DateTime endDate;
            if (!DateTime.TryParseExact(request.BeginDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out beginDate))
            {
                throw new WebApiException(400, "时间参数错误");
            }
            if (!DateTime.TryParseExact(request.EndDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
            {
                throw new WebApiException(400, "时间参数错误");
            }
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var dateInfo = (from item in db.t_shares_limit_date
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (dateInfo == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                dateInfo.BeginDate = beginDate;
                dateInfo.EndDate = endDate;
                dateInfo.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改禁止日期状态
        /// </summary>
        public void ModifySharesDateForbidStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateInfo = (from item in db.t_shares_limit_date
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (dateInfo == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                dateInfo.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除禁止日期
        /// </summary>
        public void DeleteSharesDateForbid(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var dateInfo = (from item in db.t_shares_limit_date
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (dateInfo == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_shares_limit_date.Remove(dateInfo);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询交易时间分组列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesTradeTimeGroupInfo> GetSharesTradeTimeGroupList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var timeList = from item in db.t_shares_limit_time
                               select item;
                if (request.MaxId > 0)
                {
                    timeList = from item in timeList
                               where item.Id <= request.MaxId
                               select item;
                }

                int totalCount = timeList.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = timeList.Max(e => e.Id);
                }

                var list = (from item in timeList
                            orderby item.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                return new PageRes<SharesTradeTimeGroupInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new SharesTradeTimeGroupInfo
                            {
                                Time1 = item.Time1,
                                Time2 = item.Time2,
                                Time3 = item.Time3,
                                Time4 = item.Time4,
                                Time5 = item.Time5,
                                Time6 = item.Time6,
                                Time7 = item.Time7,
                                MarketName = item.MarketName,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                LimitKey = item.LimitKey,
                                LimitMarket = item.LimitMarket
                            }).ToList()
                };
            }
        }

        /// <summary>
        /// 添加交易时间分组
        /// </summary>
        public void AddSharesTradeTimeGroup(AddSharesTradeTimeGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_time.Add(new t_shares_limit_time
                {
                    Time1 = null,
                    Time2 = null,
                    Time3 = null,
                    Time4 = null,
                    Time5 = null,
                    Time6 = null,
                    Time7 = null,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    LimitKey = request.LimitKey,
                    LimitMarket = request.LimitMarket,
                    MarketName = request.MarketName
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑交易时间分组
        /// </summary>
        public void ModifySharesTradeTimeGroup(ModifySharesTradeTimeGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var timeInfo = (from item in db.t_shares_limit_time
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (timeInfo == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                timeInfo.LastModified = DateTime.Now;
                timeInfo.LimitKey = request.LimitKey;
                timeInfo.LimitMarket = request.LimitMarket;
                timeInfo.MarketName = request.MarketName;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑交易时间
        /// </summary>
        public void ModifySharesTradeTime(ModifySharesTradeTimeRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var timeInfo = (from item in db.t_shares_limit_time
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (timeInfo == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (request.Type == 1)
                {
                    timeInfo.Time1 = request.Time;
                    db.SaveChanges();
                }
                if (request.Type == 2)
                {
                    timeInfo.Time2 = request.Time;
                    db.SaveChanges();
                }
                if (request.Type == 3)
                {
                    timeInfo.Time3 = request.Time;
                    db.SaveChanges();
                }
                if (request.Type == 4)
                {
                    timeInfo.Time4 = request.Time;
                    db.SaveChanges();
                }
                if (request.Type == 5)
                {
                    timeInfo.Time5 = request.Time;
                    db.SaveChanges();
                }
                if (request.Type == 6)
                {
                    timeInfo.Time6 = request.Time;
                    db.SaveChanges();
                }
                if (request.Type == 7)
                {
                    timeInfo.Time7 = request.Time;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 删除交易时间分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesTradeTimeGroup(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var timeInfo = (from item in db.t_shares_limit_time
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (timeInfo == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_time.Remove(timeInfo);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询交易杠杆列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesTradeLeverInfo> GetSharesTradeLeverList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var fundmultiple = from item in db.t_shares_limit_fundmultiple
                                   select item;
                if (request.MaxId > 0)
                {
                    fundmultiple = from item in fundmultiple
                                   where item.Id <= request.MaxId
                                   select item;
                }

                int totalCount = fundmultiple.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = fundmultiple.Max(e => e.Id);
                }

                var list = (from item in fundmultiple
                            orderby item.CreateTime descending
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                return new PageRes<SharesTradeLeverInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new SharesTradeLeverInfo
                            {
                                HighOpenRange=item.HighOpenRange,
                                LowOpenRange = item.LowOpenRange,
                                FundMultiple = item.FundMultiple,
                                Priority = item.Priority,
                                Range = item.Range,
                                MarketName = item.MarketName,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                NearLimitRange = item.NearLimitRange,
                                LimitKey = item.LimitKey,
                                LimitMarket = item.LimitMarket
                            }).ToList()
                };
            }
        }

        /// <summary>
        /// 添加交易杠杆
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesTradeLever(AddSharesTradeLeverRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_fundmultiple.Add(new t_shares_limit_fundmultiple
                {
                    CreateTime = DateTime.Now,
                    FundMultiple = request.FundMultiple,
                    LastModified = DateTime.Now,
                    LimitKey = request.LimitKey,
                    LimitMarket = request.LimitMarket,
                    MarketName = request.MarketName,
                    Priority = request.Priority,
                    NearLimitRange = request.NearLimitRange,
                    HighOpenRange=request.HighOpenRange,
                    LowOpenRange=request.LowOpenRange,
                    Range = request.Range
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑交易杠杆
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesTradeLever(ModifySharesTradeLeverRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var fundmultiple = (from item in db.t_shares_limit_fundmultiple
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (fundmultiple == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                fundmultiple.LastModified = DateTime.Now;
                fundmultiple.LimitKey = request.LimitKey;
                fundmultiple.LimitMarket = request.LimitMarket;
                fundmultiple.MarketName = request.MarketName;
                fundmultiple.Priority = request.Priority;
                fundmultiple.Range = request.Range;
                fundmultiple.NearLimitRange = request.NearLimitRange;
                fundmultiple.HighOpenRange = request.HighOpenRange; 
                fundmultiple.LowOpenRange = request.LowOpenRange;
                fundmultiple.FundMultiple = request.FundMultiple;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除交易杠杆
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesTradeLever(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var fundmultiple = (from item in db.t_shares_limit_fundmultiple
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (fundmultiple == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_fundmultiple.Remove(fundmultiple);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询交易规则列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesTradeRulesInfo> GetSharesTradeRulesList(GetSharesTradeRulesListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradeRules = from item in db.t_shares_limit_traderules
                                 select item;
                if (!string.IsNullOrEmpty(request.SharesKey))
                {
                    tradeRules = from item in tradeRules
                                 where item.LimitKey.Contains(request.SharesKey)
                                 select item;
                }
                if (request.SharesType != 0)
                {
                    tradeRules = from item in tradeRules
                                 where item.LimitType == request.SharesType
                                 select item;
                }
                if (request.MaxId > 0)
                {
                    tradeRules = from item in tradeRules
                                 where item.Id <= request.MaxId
                                 select item;
                }

                int totalCount = tradeRules.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = tradeRules.Max(e => e.Id);
                }

                return new PageRes<SharesTradeRulesInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in tradeRules
                            orderby item.CreateTime descending
                            select new SharesTradeRulesInfo
                            {
                                Status = item.Status,
                                ClosingLine = item.ClosingLine,
                                Cordon = item.Cordon,
                                CreateTime = item.CreateTime,
                                LimitKey = item.LimitKey,
                                LimitMarket = item.LimitMarket,
                                LimitType = item.LimitType,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加交易规则
        /// </summary>
        public void AddSharesTradeRules(AddSharesTradeRulesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_traderules.Add(new t_shares_limit_traderules
                {
                    Status = 2,
                    ClosingLine = request.ClosingLine,
                    Cordon = request.Cordon,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    LimitKey = request.LimitKey,
                    LimitMarket = request.LimitMarket,
                    LimitType = request.LimitType
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑交易规则
        /// </summary>
        public void ModifySharesTradeRules(ModifySharesTradeRulesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradeRules = (from item in db.t_shares_limit_traderules
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (tradeRules == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                tradeRules.ClosingLine = request.ClosingLine;
                tradeRules.Cordon = request.Cordon;
                tradeRules.LastModified = DateTime.Now;
                tradeRules.LimitKey = request.LimitKey;
                tradeRules.LimitMarket = request.LimitMarket;
                tradeRules.LimitType = request.LimitType;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改交易规则状态
        /// </summary>
        public void ModifySharesTradeRulesStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradeRules = (from item in db.t_shares_limit_traderules
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (tradeRules == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                tradeRules.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除交易规则
        /// </summary>
        public void DeleteSharesTradeRules(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradeRules = (from item in db.t_shares_limit_traderules
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (tradeRules == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_traderules.Remove(tradeRules);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询交易规则额外平仓线列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesTradeRulesOtherInfo> GetSharesTradeRulesOtherList(GetSharesTradeRulesOtherListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rulesOther = from item in db.t_shares_limit_traderules_other
                                 where item.RulesId == request.Id && item.Type==request.Type
                                 select item;
                if (request.MaxId > 0)
                {
                    rulesOther = from item in rulesOther
                                 where item.Id <= request.MaxId
                                 select item;
                }
                int totalCount = rulesOther.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = rulesOther.Max(e => e.Id);
                }

                return new PageRes<SharesTradeRulesOtherInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in rulesOther
                            orderby item.CreateTime descending
                            select new SharesTradeRulesOtherInfo
                            {
                                Status = item.Status,
                                ClosingLine = item.ClosingLine,
                                CreateTime = item.CreateTime,
                                Cordon = item.Cordon,
                                Id = item.Id,
                                Times = item.Times
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加交易规则额外平仓线
        /// </summary>
        public void AddSharesTradeRulesOther(AddSharesTradeRulesOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_traderules_other.Add(new t_shares_limit_traderules_other
                {
                    Type=request.Type,
                    Status = 2,
                    ClosingLine = request.ClosingLine,
                    Cordon = request.Cordon,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    RulesId = request.RulesId,
                    Times = request.Times
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑交易规则额外平仓线
        /// </summary>
        public void ModifySharesTradeRulesOther(ModifySharesTradeRulesOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rulesOther = (from item in db.t_shares_limit_traderules_other
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (rulesOther == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                rulesOther.Times = request.Times;
                rulesOther.ClosingLine = request.ClosingLine;
                rulesOther.Cordon = request.Cordon;
                rulesOther.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改交易规则额外平仓线状态
        /// </summary>
        public void ModifySharesTradeRulesOtherStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rulesOther = (from item in db.t_shares_limit_traderules_other
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (rulesOther == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                rulesOther.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除交易规则额外平仓线
        /// </summary>
        public void DeleteSharesTradeRulesOther(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rulesOther = (from item in db.t_shares_limit_traderules_other
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (rulesOther == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_shares_limit_traderules_other.Remove(rulesOther);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询仓位规则列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesPositionRulesInfo> GetSharesPositionRulesList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequota = from item in db.t_shares_limit_tradequota
                                 select item;

                if (request.MaxId > 0)
                {
                    tradequota = from item in tradequota
                                 where item.Id <= request.MaxId
                                 select item;
                }

                int totalCount = tradequota.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = tradequota.Max(e => e.Id);
                }

                return new PageRes<SharesPositionRulesInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in tradequota
                            orderby item.CreateTime descending
                            select new SharesPositionRulesInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                LimitMarket = item.LimitMarket,
                                LimitKey = item.LimitKey,
                                Rate = item.Rate,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加仓位规则
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPositionRules(AddSharesPositionRulesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_tradequota.Add(new t_shares_limit_tradequota
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    LimitKey = request.LimitKey,
                    LimitMarket = request.LimitMarket,
                    Rate = request.Rate
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑仓位规则
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPositionRules(ModifySharesPositionRulesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequota = (from item in db.t_shares_limit_tradequota
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (tradequota == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                tradequota.LastModified = DateTime.Now;
                tradequota.LimitKey = request.LimitKey;
                tradequota.LimitMarket = request.LimitMarket;
                tradequota.Rate = request.Rate;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改仓位规则状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPositionRulesStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequota = (from item in db.t_shares_limit_tradequota
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (tradequota == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                tradequota.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除仓位规则
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPositionRules(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequota = (from item in db.t_shares_limit_tradequota
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                if (tradequota == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_tradequota.Remove(tradequota);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询额外仓位规则列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesPositionRulesOtherInfo> GetSharesPositionRulesOtherList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequotaOther = from item in db.t_shares_limit_tradequota_other
                                      where item.QuotaId == request.Id
                                      select item;

                if (request.MaxId > 0)
                {
                    tradequotaOther = from item in tradequotaOther
                                      where item.Id <= request.MaxId
                                      select item;
                }

                int totalCount = tradequotaOther.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = tradequotaOther.Max(e => e.Id);
                }

                return new PageRes<SharesPositionRulesOtherInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in tradequotaOther
                            orderby item.CreateTime descending
                            select new SharesPositionRulesOtherInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                AccountCode = item.AccountCode,
                                Rate = item.Rate,
                                QuotaId = item.QuotaId,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加额外仓位规则
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPositionRulesOther(AddSharesPositionRulesOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断QuotaId是否存在
                var rules = (from item in db.t_shares_limit_tradequota
                             where item.Id == request.QuotaId
                             select item).FirstOrDefault();
                if (rules == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_tradequota_other.Add(new t_shares_limit_tradequota_other
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    AccountCode = request.AccountCode,
                    QuotaId = request.QuotaId,
                    Rate = request.Rate
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑额外仓位规则
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPositionRulesOther(ModifySharesPositionRulesOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequotaOther = (from item in db.t_shares_limit_tradequota_other
                                       where item.Id == request.Id
                                       select item).FirstOrDefault();
                if (tradequotaOther == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                tradequotaOther.AccountCode = request.AccountCode;
                tradequotaOther.LastModified = DateTime.Now;
                tradequotaOther.Rate = request.Rate;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改额外仓位规则状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPositionRulesOtherStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequotaOther = (from item in db.t_shares_limit_tradequota_other
                                       where item.Id == request.Id
                                       select item).FirstOrDefault();
                if (tradequotaOther == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                tradequotaOther.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除额外仓位规则
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPositionRulesOther(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var tradequotaOther = (from item in db.t_shares_limit_tradequota_other
                                       where item.Id == request.Id
                                       select item).FirstOrDefault();
                if (tradequotaOther == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_tradequota_other.Remove(tradequotaOther);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询风控规则列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesRiskRulesInfo> GetSharesRiskRulesList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var risk = from item in db.t_shares_limit_risk
                           select item;

                if (request.MaxId > 0)
                {
                    risk = from item in risk
                           where item.Id <= request.MaxId
                           select item;
                }

                int totalCount = risk.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = risk.Max(e => e.Id);
                }

                return new PageRes<SharesRiskRulesInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in risk
                            orderby item.CreateTime descending
                            select new SharesRiskRulesInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                ForbidStatus = item.ForbidStatus,
                                MatchDes = item.MatchDes,
                                Type = item.Type,
                                Rules = item.Rules,
                                Id = item.Id
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 编辑风控规则
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesRiskRules(ModifySharesRiskRulesRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var risk = (from item in db.t_shares_limit_risk
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (risk == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                risk.LastModified = DateTime.Now;
                risk.Rules = request.Rules;
                risk.ForbidStatus = request.ForbidStatus;
                risk.MatchDes = request.MatchDes;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改风控规则禁止状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesRiskRulesForbidStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var risk = (from item in db.t_shares_limit_risk
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (risk == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                risk.ForbidStatus = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改风控规则状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesRiskRulesStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var risk = (from item in db.t_shares_limit_risk
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (risk == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                risk.Status = request.Status;
                db.SaveChanges();
            }
        }
        #endregion

        #region====行情监控====
        /// <summary>
        /// 获取行情监控列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorInfo> GetSharesMonitorList(GetSharesMonitorListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var list = from item in db.t_shares_monitor
                           join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                           select new { item, item2 };
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    list = from item in list
                           where item.item.SharesCode.Contains(request.SharesInfo) || item.item2.SharesPyjc.StartsWith(request.SharesInfo) || item.item2.SharesName.Contains(request.SharesInfo)
                           select item;
                }

                int totalCount = list.Count();
                return new PageRes<SharesMonitorInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.item.CreateTime descending
                            select new SharesMonitorInfo
                            {
                                Status = item.item.Status,
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id,
                                SharesCode = item.item.SharesCode,
                                Market = item.item.Market,
                                SharesName = item.item2.SharesName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加行情监控
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesMonitor(AddSharesMonitorRequest request)
        {
            BatchAddSharesMonitor(new List<MarketTimeInfo>
            {
                new MarketTimeInfo
                {
                    SharesCode=request.SharesCode,
                    Market=request.Market
                }
            });
        }

        /// <summary>
        /// 修改行情监控状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMonitorStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var monitor = (from item in db.t_shares_monitor
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (monitor == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                monitor.Status = request.Status;
                monitor.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除行情监控
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesMonitor(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var monitor = (from item in db.t_shares_monitor
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                    if (monitor == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    long monitorId = monitor.Id;

                    var monitorTrend = (from item in db.t_shares_monitor_trend_rel
                                        where item.MonitorId == monitorId
                                        select item).ToList();
                    List<long> relIdList = monitorTrend.Select(e => e.Id).ToList();

                    var trendPar = (from item in db.t_shares_monitor_trend_rel_par
                                    where relIdList.Contains(item.RelId)
                                    select item).ToList();

                    db.t_shares_monitor.Remove(monitor);
                    if (monitorTrend.Count() > 0)
                    {
                        db.t_shares_monitor_trend_rel.RemoveRange(monitorTrend);
                    }
                    if (trendPar.Count() > 0)
                    {
                        db.t_shares_monitor_trend_rel_par.RemoveRange(trendPar);
                    }
                    db.SaveChanges();


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
        /// 批量删除行情监控
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteSharesMonitor(BatchDeleteSharesMonitorRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var monitor = (from item in db.t_shares_monitor
                                   where request.IdList.Contains(item.Id)
                                   select item).ToList();
                    if (monitor.Count()<=0)
                    {
                        return;
                    }
                    List<long> monitorId = monitor.Select(e => e.Id).ToList();

                    var monitorTrend = (from item in db.t_shares_monitor_trend_rel
                                        where monitorId.Contains(item.MonitorId)
                                        select item).ToList();
                    List<long> relIdList = monitorTrend.Select(e => e.Id).ToList();

                    var trendPar = (from item in db.t_shares_monitor_trend_rel_par
                                    where relIdList.Contains(item.RelId)
                                    select item).ToList();

                    db.t_shares_monitor.RemoveRange(monitor);
                    if (monitorTrend.Count() > 0)
                    {
                        db.t_shares_monitor_trend_rel.RemoveRange(monitorTrend);
                    }
                    if (trendPar.Count() > 0)
                    {
                        db.t_shares_monitor_trend_rel_par.RemoveRange(trendPar);
                    }
                    db.SaveChanges();


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
        /// 批量导入行情监控数据
        /// </summary>
        /// <param name="list"></param>
        public int BatchAddSharesMonitor(List<MarketTimeInfo> list)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断是否存在
                    var monitor = (from x in db.t_shares_monitor
                                   select x).ToList();
                    List<MarketTimeInfo> tempList = new List<MarketTimeInfo>();
                    foreach (var item in list)
                    {
                        if (monitor.Where(e => e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault() != null)
                        {
                            continue;
                        }
                        tempList.Add(item);
                    }
                    int resultCount = BatchAddSharesMonitorFun(tempList, db, tran.UnderlyingTransaction);
                    tran.Commit();
                    return resultCount;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        private int BatchAddSharesMonitorFun(List<MarketTimeInfo> list, meal_ticketEntities db, DbTransaction tran, List<long> trendIdList = null)
        {
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = tran;
            int resultCount = 0;
            var tempTrendPar = (from item in db.t_shares_monitor_trend_par
                                select item).ToList();
            if (trendIdList != null)
            {
                tempTrendPar = (from item in tempTrendPar
                                where trendIdList.Contains(item.TrendId)
                                select item).ToList();
            }
            foreach (var item in list)
            {
                List<t_shares_monitor_trend_par> trendPar = new List<t_shares_monitor_trend_par>();
                trendPar.AddRange(tempTrendPar);
                if (trendIdList != null)
                {
                    var relPar = (from x in db.t_shares_monitor
                                  join x2 in db.t_shares_monitor_trend_rel on x.Id equals x2.MonitorId
                                  join x3 in db.t_shares_monitor_trend_rel_par on x2.Id equals x3.RelId
                                  where x.Market == item.Market && x.SharesCode == item.SharesCode && !trendIdList.Contains(x2.TrendId)
                                  select new { x2, x3 }).ToList();
                    var tempRelPar = (from x in relPar
                                      select new t_shares_monitor_trend_par
                                      {
                                          ParamsInfo = x.x3.ParamsInfo,
                                          TrendId = x.x2.TrendId
                                      }).ToList();
                    trendPar.AddRange(tempRelPar);
                }
                string sql = string.Format("insert into t_shares_monitor ([Market],[SharesCode],[Status],[CreateTime],[LastModified],DataType) values({0},'{1}',1,'{2}','{3}',1);select @@IDENTITY", item.Market, item.SharesCode, DateTime.Now, DateTime.Now);

                cmd.CommandText = sql;   //sql语句
                long monitorId = Convert.ToInt64(cmd.ExecuteScalar());
                //trendId=1
                sql = string.Format("insert into t_shares_monitor_trend_rel([MonitorId],[TrendId],[Status],[CreateTime],[LastModified]) values({0},1,1,'{1}','{2}');select @@IDENTITY", monitorId, DateTime.Now, DateTime.Now);

                cmd.CommandText = sql;   //sql语句
                long relId = Convert.ToInt64(cmd.ExecuteScalar());

                sql = "";
                var tempList = trendPar.Where(e => e.TrendId == 1).ToList();
                foreach (var x in tempList)
                {
                    sql += string.Format("insert into t_shares_monitor_trend_rel_par([RelId],[ParamsInfo],[CreateTime],[LastModified]) values({0},'{1}','{2}','{3}');", relId, x.ParamsInfo, DateTime.Now, DateTime.Now);
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    cmd.CommandText = sql;   //sql语句
                    cmd.ExecuteNonQuery();
                }


                //trendId=2
                sql = string.Format("insert into t_shares_monitor_trend_rel([MonitorId],[TrendId],[Status],[CreateTime],[LastModified]) values({0},2,1,'{1}','{2}');select @@IDENTITY", monitorId, DateTime.Now, DateTime.Now);

                cmd.CommandText = sql;   //sql语句
                relId = Convert.ToInt64(cmd.ExecuteScalar());

                sql = "";
                tempList = trendPar.Where(e => e.TrendId == 2).ToList();
                foreach (var x in tempList)
                {
                    sql += string.Format("insert into t_shares_monitor_trend_rel_par([RelId],[ParamsInfo],[CreateTime],[LastModified]) values({0},'{1}','{2}','{3}');", relId, x.ParamsInfo, DateTime.Now, DateTime.Now);
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    cmd.CommandText = sql;   //sql语句
                    cmd.ExecuteNonQuery();
                }


                //trendId=3
                sql = string.Format("insert into t_shares_monitor_trend_rel([MonitorId],[TrendId],[Status],[CreateTime],[LastModified]) values({0},3,1,'{1}','{2}');select @@IDENTITY", monitorId, DateTime.Now, DateTime.Now);

                cmd.CommandText = sql;   //sql语句
                relId = Convert.ToInt64(cmd.ExecuteScalar());

                sql = "";
                tempList = trendPar.Where(e => e.TrendId == 3).ToList();
                foreach (var x in tempList)
                {
                    sql += string.Format("insert into t_shares_monitor_trend_rel_par([RelId],[ParamsInfo],[CreateTime],[LastModified]) values({0},'{1}','{2}','{3}');", relId, x.ParamsInfo, DateTime.Now, DateTime.Now);
                }
                if (!string.IsNullOrEmpty(sql))
                {
                    cmd.CommandText = sql;   //sql语句
                    cmd.ExecuteNonQuery();
                }
                resultCount++;
            }
            return resultCount;
        }

        /// <summary>
        /// 获取走势模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendInfo> GetSharesMonitorTrendList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_shares_monitor_trend
                            select item;

                int totalCount = trend.Count();

                return new PageRes<SharesMonitorTrendInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加走势模板
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesMonitorTrend(AddSharesMonitorTrendRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_monitor_trend.Add(new t_shares_monitor_trend
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    Description = request.Description,
                    LastModified = DateTime.Now,
                    Name = request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑走势模板
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMonitorTrend(ModifySharesMonitorTrendRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_shares_monitor_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                trend.Name = request.Name;
                trend.Description = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改走势模板状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMonitorTrendStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_shares_monitor_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除走势模板
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesMonitorTrend(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_shares_monitor_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_shares_monitor_trend.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendParInfo> GetSharesMonitorTrendPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_shares_monitor_trend_par
                               where item.TrendId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<SharesMonitorTrendParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加走势模板参数
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesMonitorTrendPar(AddSharesMonitorTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_shares_monitor_trend_par
                               where item.TrendId == request.TrendId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_shares_monitor_trend_par.Add(new t_shares_monitor_trend_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            TrendId = request.TrendId
                        });
                    }
                }
                else
                {
                    db.t_shares_monitor_trend_par.Add(new t_shares_monitor_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        TrendId = request.TrendId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑走势模板参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMonitorTrendPar(ModifySharesMonitorTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_shares_monitor_trend_par
                                where item.Id == request.Id && item.TrendId == 1
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除走势模板参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesMonitorTrendPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_shares_monitor_trend_par
                                where item.Id == request.Id && item.TrendId == 1
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_monitor_trend_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取监控走势关系列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendRelInfo> GetSharesMonitorTrendRelList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = from item in db.t_shares_monitor_trend_rel
                          where item.MonitorId == request.Id
                          select item;
                int totalCount = rel.Count();
                return new PageRes<SharesMonitorTrendRelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in rel
                            join item2 in db.t_shares_monitor_trend on item.TrendId equals item2.Id into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendRelInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Description = ai == null ? "" : ai.Description,
                                Id = item.Id,
                                TrendId = item.TrendId,
                                Name = ai == null ? "" : ai.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加监控走势关系
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesMonitorTrendRel(AddSharesMonitorTrendRelRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var x in request.TrendId)
                    {
                        //判断走势是否存在
                        var trend = (from item in db.t_shares_monitor_trend
                                     where item.Id == x
                                     select item).FirstOrDefault();
                        if (trend == null)
                        {
                            throw new WebApiException(400, "走势模板不存在");
                        }

                        var trendRel = (from item in db.t_shares_monitor_trend_rel
                                        where item.MonitorId == request.MonitorId && item.TrendId == x
                                        select item).FirstOrDefault();
                        if (trendRel != null)
                        {
                            continue;
                        }

                        t_shares_monitor_trend_rel rel = new t_shares_monitor_trend_rel
                        {
                            Status = 1,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            MonitorId = request.MonitorId,
                            TrendId = x
                        };
                        db.t_shares_monitor_trend_rel.Add(rel);
                        db.SaveChanges();

                        var trendPar = (from item in db.t_shares_monitor_trend_par
                                        where item.TrendId == x
                                        select item).ToList();
                        foreach (var item in trendPar)
                        {
                            db.t_shares_monitor_trend_rel_par.Add(new t_shares_monitor_trend_rel_par
                            {
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                RelId = rel.Id,
                                ParamsInfo = item.ParamsInfo
                            });
                        }
                        db.SaveChanges();
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
        /// 修改监控走势关系状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMonitorTrendRelStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = (from item in db.t_shares_monitor_trend_rel
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (rel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                rel.Status = request.Status;
                rel.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除监控走势关系
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesMonitorTrendRel(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = (from item in db.t_shares_monitor_trend_rel
                           where item.Id == request.Id
                           select item).FirstOrDefault();
                if (rel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_shares_monitor_trend_rel.Remove(rel);

                var relPar = (from item in db.t_shares_monitor_trend_rel_par
                              where item.RelId == request.Id
                              select item).ToList();
                db.t_shares_monitor_trend_rel_par.RemoveRange(relPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendParInfo> GetSharesMonitorTrendRelPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_shares_monitor_trend_rel_par
                               where item.RelId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<SharesMonitorTrendParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesMonitorTrendRelPar(AddSharesMonitorTrendRelParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rel = (from item in db.t_shares_monitor_trend_rel
                           where item.Id == request.RelId
                           select item).FirstOrDefault();
                if (rel == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                if (rel.TrendId != 1)
                {
                    var par = (from item in db.t_shares_monitor_trend_rel_par
                               where item.RelId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_shares_monitor_trend_rel_par.Add(new t_shares_monitor_trend_rel_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            RelId = request.RelId
                        });
                    }
                }
                else
                {
                    db.t_shares_monitor_trend_rel_par.Add(new t_shares_monitor_trend_rel_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        RelId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesMonitorTrendRelPar(ModifySharesMonitorTrendRelParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_shares_monitor_trend_rel_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除监控走势关系参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesMonitorTrendRelPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_shares_monitor_trend_rel_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_monitor_trend_rel_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取监控股票分笔明细列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesTransactionDataInfo> GetSharesMonitorTransactionDataList(GetSharesTransactionDataListRequest request)
        {
            using (var db = new meal_ticketEntities(ConfigurationManager.ConnectionStrings["meal_ticketEntities2"].ConnectionString))
            {
                var transactiondata = from item in db.t_shares_transactiondata
                                      where item.Market == request.Market && item.SharesCode == request.SharesCode
                                      select item;
                if (request.StartTime != null && request.EndTime != null)
                {
                    transactiondata = from item in transactiondata
                                      where item.Time >= request.StartTime && item.Time <= request.EndTime
                                      select item;
                }
                int totalCount = transactiondata.Count();

                return new PageRes<SharesTransactionDataInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in transactiondata
                            orderby item.Time descending, item.OrderIndex descending
                            select new SharesTransactionDataInfo
                            {
                                Id = item.Id,
                                Stock = item.Stock,
                                Price = item.Price,
                                Time = item.Time,
                                TimeStr = item.TimeStr,
                                Type = item.Type,
                                LastModified = item.LastModified
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 重置监控股票分笔明细
        /// </summary>
        /// <param name="request"></param>
        public void ResetSharesMonitorTransactionData(ResetSharesTransactionDataRequest request)
        {
            DateTime startTime = request.StartDate.Date;
            DateTime endTime = request.EndDate;
            while (startTime <= endTime)
            {
                try
                {
                    var sendData = new
                    {
                        Market = request.Market,
                        SharesCode = request.SharesCode,
                        Type = 2,
                        Date = startTime.ToString("yyyy-MM-dd")
                    };
                    Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "UpdateNew");
                }
                catch (Exception)
                { }
                startTime = startTime.AddDays(1);
            }
        }

        /// <summary>
        /// 批量更新监控参数
        /// </summary>
        /// <param name="request"></param>
        public int BatchUpdateMonitorTrendPar(BatchUpdateMonitorTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //查询需要更新的监控股票
                    var monitor = (from item in db.t_shares_monitor
                                   select item).ToList();
                    if (!string.IsNullOrEmpty(request.SharesCode))
                    {
                        monitor = (from item in monitor
                                   where item.SharesCode.StartsWith(request.SharesCode)
                                   select item).ToList();
                    }

                    var batchList = (from item in monitor
                                     select new MarketTimeInfo
                                     {
                                         SharesCode = item.SharesCode,
                                         Market = item.Market
                                     }).ToList();

                    List<long> monitorIdList = monitor.Select(e => e.Id).ToList();

                    var monitorRel = (from item in db.t_shares_monitor_trend_rel
                                      where monitorIdList.Contains(item.MonitorId)
                                      select item).ToList();

                    List<long> relIdList = monitorRel.Select(e => e.Id).ToList();

                    var par = (from item in db.t_shares_monitor_trend_rel_par
                               where relIdList.Contains(item.RelId)
                               select item).ToList();

                    db.t_shares_monitor.RemoveRange(monitor);
                    db.t_shares_monitor_trend_rel.RemoveRange(monitorRel);
                    db.t_shares_monitor_trend_rel_par.RemoveRange(par);


                    int resultCount = BatchAddSharesMonitorFun(batchList, db, tran.UnderlyingTransaction, request.TrendIdList);

                    db.SaveChanges();
                    tran.Commit();
                    return resultCount;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 获取条件模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateInfo> GetConditiontradeTemplateList(GetConditiontradeTemplateListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var template = from item in db.t_sys_conditiontrade_template
                               select item;
                if (request.Type == 45)
                {
                    template = from item in db.t_sys_conditiontrade_template
                               where item.Type == 4 || item.Type == 5 || item.Type==6
                               select item;
                }
                else
                {
                    template = from item in template
                               where item.Type == request.Type
                               select item;
                }
                int totalCount = template.Count();

                return new PageRes<ConditiontradeTemplateInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in template
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                Type = item.Type,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 复制条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void CopyConditiontradeTemplate(DetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var template = (from item in db.t_sys_conditiontrade_template
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                    if (template == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    t_sys_conditiontrade_template newTemplate = new t_sys_conditiontrade_template
                    {
                        Status = template.Status,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = template.Name,
                        Type = template.Type
                    };
                    db.t_sys_conditiontrade_template.Add(newTemplate);
                    db.SaveChanges();

                    if (template.Type == 1)//买入模板复制
                    {
                        CopyConditiontradeTemplate_Buy(request.Id, newTemplate.Id, db);
                    }
                    else if (template.Type == 2)//卖出模板复制
                    {
                        CopyConditiontradeTemplate_Sell(request.Id, newTemplate.Id, db);
                    }
                    else if (template.Type == 3)//自动加入模板复制
                    {
                        CopyConditiontradeTemplate_Join(request.Id, newTemplate.Id, db);
                    }
                    else if (template.Type == 4 || template.Type==5 || template.Type == 6)//自动加入模板复制
                    {
                        CopyConditiontradeTemplate_Search(request.Id, newTemplate.Id, db);
                    }
                    else
                    {
                        throw new WebApiException(400, "数据有误");
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

        private void CopyConditiontradeTemplate_Buy(long templateId, long newTemplateId, meal_ticketEntities db) 
        {
            var template_buy = (from item in db.t_sys_conditiontrade_template_buy
                                where item.TemplateId == templateId
                                select item).ToList();
            Dictionary<long, long> idDic = new Dictionary<long, long>();
            foreach (var item in template_buy)
            {
                t_sys_conditiontrade_template_buy new_template_buy = new t_sys_conditiontrade_template_buy 
                {
                    Status=item.Status,
                    BuyAuto=item.BuyAuto,
                    ConditionPriceBase=item.ConditionPriceBase,
                    ConditionPriceRate=item.ConditionPriceRate,
                    ConditionPriceType=item.ConditionPriceType,
                    ConditionType=item.ConditionType,
                    CreateTime=DateTime.Now,
                    EntrustAmount=item.EntrustAmount,
                    EntrustAmountType=item.EntrustAmountType,
                    EntrustPriceGear=item.EntrustPriceGear,
                    EntrustType=item.EntrustType,
                    ForbidType=item.ForbidType,
                    IsGreater=item.IsGreater,
                    IsHold=item.IsHold,
                    LastModified=DateTime.Now,
                    LimitUp=item.LimitUp,
                    Name=item.Name,
                    OtherConditionRelative=item.OtherConditionRelative,
                    TemplateId= newTemplateId
                };
                db.t_sys_conditiontrade_template_buy.Add(new_template_buy);
                db.SaveChanges();
                idDic.Add(item.Id, new_template_buy.Id);

                var buy_other = (from x in db.t_sys_conditiontrade_template_buy_other
                                 where x.TemplateBuyId == item.Id
                                 select x).ToList();
                foreach (var other in buy_other)
                {
                    t_sys_conditiontrade_template_buy_other new_buy_other = new t_sys_conditiontrade_template_buy_other
                    {
                        Status = other.Status,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = other.Name,
                        TemplateBuyId = new_template_buy.Id
                    };
                    db.t_sys_conditiontrade_template_buy_other.Add(new_buy_other);
                    db.SaveChanges();

                    var buy_other_trend = (from x in db.t_sys_conditiontrade_template_buy_other_trend
                                           where x.OtherId == other.Id
                                           select x).ToList();
                    foreach (var other_trend in buy_other_trend)
                    {
                        t_sys_conditiontrade_template_buy_other_trend new_other_trend = new t_sys_conditiontrade_template_buy_other_trend
                        {
                            Status = other_trend.Status,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            OtherId = new_buy_other.Id,
                            TrendDescription = other_trend.TrendDescription,
                            TrendId = other_trend.TrendId,
                            TrendName = other_trend.TrendName
                        };
                        db.t_sys_conditiontrade_template_buy_other_trend.Add(new_other_trend);
                        db.SaveChanges();

                        var buy_other_trend_par = (from x in db.t_sys_conditiontrade_template_buy_other_trend_par
                                                   where x.OtherTrendId == other_trend.Id
                                                   select x).ToList();
                        foreach (var other_trend_par in buy_other_trend_par)
                        {
                            db.t_sys_conditiontrade_template_buy_other_trend_par.Add(new t_sys_conditiontrade_template_buy_other_trend_par
                            {
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                OtherTrendId = new_other_trend.Id,
                                ParamsInfo = other_trend_par.ParamsInfo
                            });
                        }
                        db.SaveChanges();

                        var buy_other_trend_other = (from x in db.t_sys_conditiontrade_template_buy_other_trend_other
                                                     where x.OtherTrendId == other_trend.Id
                                                     select x).ToList();
                        foreach (var other_trend_other in buy_other_trend_other)
                        {
                            t_sys_conditiontrade_template_buy_other_trend_other new_other_trend_other = new t_sys_conditiontrade_template_buy_other_trend_other
                            {
                                Status = other_trend_other.Status,
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                OtherTrendId = new_other_trend.Id,
                                TrendDescription = other_trend_other.TrendDescription,
                                TrendId = other_trend_other.TrendId,
                                TrendName = other_trend_other.TrendName
                            };
                            db.t_sys_conditiontrade_template_buy_other_trend_other.Add(new_other_trend_other);
                            db.SaveChanges();

                            var buy_other_trend_other_par = (from x in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                                             where x.OtherTrendOtherId == other_trend_other.Id
                                                             select x).ToList();
                            foreach (var other_trend_other_par in buy_other_trend_other_par)
                            {
                                db.t_sys_conditiontrade_template_buy_other_trend_other_par.Add(new t_sys_conditiontrade_template_buy_other_trend_other_par
                                {
                                    CreateTime = DateTime.Now,
                                    LastModified = DateTime.Now,
                                    OtherTrendOtherId = new_other_trend_other.Id,
                                    ParamsInfo = other_trend_other_par.ParamsInfo
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                }

                var buy_auto = (from x in db.t_sys_conditiontrade_template_buy_auto
                                where x.TemplateBuyId == item.Id
                                 select x).ToList();
                foreach (var auto in buy_auto)
                {
                    t_sys_conditiontrade_template_buy_auto new_buy_auto = new t_sys_conditiontrade_template_buy_auto
                    {
                        Status = auto.Status,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = auto.Name,
                        TemplateBuyId = new_template_buy.Id
                    };
                    db.t_sys_conditiontrade_template_buy_auto.Add(new_buy_auto);
                    db.SaveChanges();

                    var buy_auto_trend = (from x in db.t_sys_conditiontrade_template_buy_auto_trend
                                           where x.AutoId == auto.Id
                                           select x).ToList();
                    foreach (var auto_trend in buy_auto_trend)
                    {
                        t_sys_conditiontrade_template_buy_auto_trend new_auto_trend = new t_sys_conditiontrade_template_buy_auto_trend
                        {
                            Status = auto_trend.Status,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            AutoId = new_buy_auto.Id,
                            TrendDescription = auto_trend.TrendDescription,
                            TrendId = auto_trend.TrendId,
                            TrendName = auto_trend.TrendName
                        };
                        db.t_sys_conditiontrade_template_buy_auto_trend.Add(new_auto_trend);
                        db.SaveChanges();

                        var buy_auto_trend_par = (from x in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                                   where x.AutoTrendId == auto_trend.Id
                                                   select x).ToList();
                        foreach (var auto_trend_par in buy_auto_trend_par)
                        {
                            db.t_sys_conditiontrade_template_buy_auto_trend_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_par
                            {
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                AutoTrendId = new_auto_trend.Id,
                                ParamsInfo = auto_trend_par.ParamsInfo
                            });
                        }
                        db.SaveChanges();

                        var buy_auto_trend_other = (from x in db.t_sys_conditiontrade_template_buy_auto_trend_other
                                                     where x.AutoTrendId == auto_trend.Id
                                                     select x).ToList();
                        foreach (var auto_trend_other in buy_auto_trend_other)
                        {
                            t_sys_conditiontrade_template_buy_auto_trend_other new_auto_trend_other = new t_sys_conditiontrade_template_buy_auto_trend_other
                            {
                                Status = auto_trend_other.Status,
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                AutoTrendId = new_auto_trend.Id,
                                TrendDescription = auto_trend_other.TrendDescription,
                                TrendId = auto_trend_other.TrendId,
                                TrendName = auto_trend_other.TrendName
                            };
                            db.t_sys_conditiontrade_template_buy_auto_trend_other.Add(new_auto_trend_other);
                            db.SaveChanges();

                            var buy_auto_trend_other_par = (from x in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                                           where x.AutoTrendOtherId == auto_trend_other.Id
                                                             select x).ToList();
                            foreach (var auto_trend_other_par in buy_auto_trend_other_par)
                            {
                                db.t_sys_conditiontrade_template_buy_auto_trend_other_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_other_par
                                {
                                    CreateTime = DateTime.Now,
                                    LastModified = DateTime.Now,
                                    AutoTrendOtherId = new_auto_trend_other.Id,
                                    ParamsInfo = auto_trend_other_par.ParamsInfo
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            foreach (var item in idDic)
            {
                var buy_child = (from x in db.t_sys_conditiontrade_template_buy_child
                                 where x.FatherId == item.Key
                                 select x).ToList();
                foreach (var child in buy_child)
                {
                    db.t_sys_conditiontrade_template_buy_child.Add(new t_sys_conditiontrade_template_buy_child
                    {
                        Status = child.Status,
                        FatherId = item.Value,
                        ChildId = idDic[child.ChildId]
                    });
                }
                db.SaveChanges();
            }
        }
        private void CopyConditiontradeTemplate_Sell(long templateId,long newTemplateId, meal_ticketEntities db) 
        {
            var template_sell = (from item in db.t_sys_conditiontrade_template_sell
                                 where item.TemplateId == templateId
                                 select item).ToList();

            Dictionary<long, long> idDic = new Dictionary<long, long>();
            foreach (var item in template_sell)
            {
                t_sys_conditiontrade_template_sell new_template_sell = new t_sys_conditiontrade_template_sell
                {
                    Status = item.Status,
                    ConditionDay = item.ConditionDay,
                    ConditionPriceBase = item.ConditionPriceBase,
                    ConditionPriceRate = item.ConditionPriceRate,
                    ConditionPriceType = item.ConditionPriceType,
                    ConditionTime = item.ConditionTime,
                    ConditionType = item.ConditionType,
                    CreateTime = DateTime.Now,
                    EntrustCount = item.EntrustCount,
                    EntrustPriceGear = item.EntrustPriceGear,
                    EntrustType = item.EntrustType,
                    ForbidType = item.ForbidType,
                    LastModified = DateTime.Now,
                    Name = item.Name,
                    OtherConditionRelative = item.OtherConditionRelative,
                    TemplateId = newTemplateId,
                    Type = item.Type
                };
                db.t_sys_conditiontrade_template_sell.Add(new_template_sell);
                db.SaveChanges();
                idDic.Add(item.Id, new_template_sell.Id);
            }

            foreach (var item in idDic)
            {
                var sell_child = (from x in db.t_sys_conditiontrade_template_sell_child
                                  where x.FatherId == item.Key
                                  select x).ToList();
                foreach (var child in sell_child)
                {
                    db.t_sys_conditiontrade_template_sell_child.Add(new t_sys_conditiontrade_template_sell_child
                    {
                        Status = child.Status,
                        FatherId = item.Value,
                        ChildId = idDic[child.ChildId]
                    });
                }
                db.SaveChanges();
            }
        }
        private void CopyConditiontradeTemplate_Join(long templateId, long newTemplateId, meal_ticketEntities db)
        {
            var template_join = (from item in db.t_sys_conditiontrade_template_join
                                 where item.TemplateId == templateId
                                 select item).ToList();
            foreach (var item in template_join)
            {
                t_sys_conditiontrade_template_join new_template_join = new t_sys_conditiontrade_template_join
                {
                    Status = item.Status,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Name = item.Name,
                    IsClearOriginal=item.IsClearOriginal,
                    TimeCycle=item.TimeCycle,
                    TimeCycleType=item.TimeCycleType,
                    TemplateId = newTemplateId
                };
                db.t_sys_conditiontrade_template_join.Add(new_template_join);
                db.SaveChanges();

                var join_other = (from x in db.t_sys_conditiontrade_template_join_other
                                  where x.TemplateJoinId == item.Id
                                  select x).ToList();
                foreach (var other in join_other)
                {
                    t_sys_conditiontrade_template_join_other new_join_other = new t_sys_conditiontrade_template_join_other
                    {
                        Status = other.Status,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = other.Name,
                        TemplateJoinId = new_template_join.Id
                    };
                    db.t_sys_conditiontrade_template_join_other.Add(new_join_other);
                    db.SaveChanges();

                    var join_other_trend = (from x in db.t_sys_conditiontrade_template_join_other_trend
                                            where x.OtherId == other.Id
                                            select x).ToList();
                    foreach (var other_trend in join_other_trend)
                    {
                        t_sys_conditiontrade_template_join_other_trend new_other_trend = new t_sys_conditiontrade_template_join_other_trend
                        {
                            Status = other_trend.Status,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            OtherId = new_join_other.Id,
                            TrendDescription = other_trend.TrendDescription,
                            TrendId = other_trend.TrendId,
                            TrendName = other_trend.TrendName
                        };
                        db.t_sys_conditiontrade_template_join_other_trend.Add(new_other_trend);
                        db.SaveChanges();

                        var join_other_trend_par = (from x in db.t_sys_conditiontrade_template_join_other_trend_par
                                                    where x.OtherTrendId == other_trend.Id
                                                   select x).ToList();
                        foreach (var other_trend_par in join_other_trend_par)
                        {
                            db.t_sys_conditiontrade_template_join_other_trend_par.Add(new t_sys_conditiontrade_template_join_other_trend_par
                            {
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                OtherTrendId = new_other_trend.Id,
                                ParamsInfo = other_trend_par.ParamsInfo
                            });
                        }
                        db.SaveChanges();

                        var join_other_trend_other = (from x in db.t_sys_conditiontrade_template_join_other_trend_other
                                                      where x.OtherTrendId == other_trend.Id
                                                     select x).ToList();
                        foreach (var other_trend_other in join_other_trend_other)
                        {
                            t_sys_conditiontrade_template_join_other_trend_other new_other_trend_other = new t_sys_conditiontrade_template_join_other_trend_other
                            {
                                Status = other_trend_other.Status,
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                OtherTrendId = new_other_trend.Id,
                                TrendDescription = other_trend_other.TrendDescription,
                                TrendId = other_trend_other.TrendId,
                                TrendName = other_trend_other.TrendName
                            };
                            db.t_sys_conditiontrade_template_join_other_trend_other.Add(new_other_trend_other);
                            db.SaveChanges();

                            var join_other_trend_other_par = (from x in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                                              where x.OtherTrendOtherId == other_trend_other.Id
                                                             select x).ToList();
                            foreach (var other_trend_other_par in join_other_trend_other_par)
                            {
                                db.t_sys_conditiontrade_template_join_other_trend_other_par.Add(new t_sys_conditiontrade_template_join_other_trend_other_par
                                {
                                    CreateTime = DateTime.Now,
                                    LastModified = DateTime.Now,
                                    OtherTrendOtherId = new_other_trend_other.Id,
                                    ParamsInfo = other_trend_other_par.ParamsInfo
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
        private void CopyConditiontradeTemplate_Search(long templateId, long newTemplateId, meal_ticketEntities db)
        {
            var template_search = (from item in db.t_sys_conditiontrade_template_search
                                   where item.TemplateId == templateId
                                   select item).FirstOrDefault();
            if (template_search != null)
            {
                db.t_sys_conditiontrade_template_search.Add(new t_sys_conditiontrade_template_search
                {
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    TemplateId = newTemplateId,
                    TemplateContent = template_search.TemplateContent
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 添加条件模板
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplate(AddConditiontradeTemplateRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断模板名称是否存在
                var template = (from item in db.t_sys_conditiontrade_template
                                where item.Type == request.Type && item.Name == request.Name
                                select item).FirstOrDefault();
                if (template != null)
                {
                    throw new WebApiException(400, "模板名称已存在");
                }
                db.t_sys_conditiontrade_template.Add(new t_sys_conditiontrade_template
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Name = request.Name,
                    Type = request.Type
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyConditiontradeTemplate(ModifyConditiontradeTemplateRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var template = (from item in db.t_sys_conditiontrade_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (template == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
                //判断模板名称是否存在
                var temp = (from item in db.t_sys_conditiontrade_template
                            where item.Type == template.Type && item.Name == request.Name && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "模板名称已存在");
                }

                template.Name = request.Name;
                template.Type = request.Type;
                template.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件模板状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyConditiontradeTemplateStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var template = (from item in db.t_sys_conditiontrade_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (template == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }

                template.Status = request.Status;
                template.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件模板
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteConditiontradeTemplate(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var template = (from item in db.t_sys_conditiontrade_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (template == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
                db.t_sys_conditiontrade_template.Remove(template);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取条件卖出模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public List<ConditiontradeTemplateSellDetailsInfo> GetConditiontradeTemplateSellDetailsList(GetConditiontradeTemplateSellDetailsListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var sell_details = (from item in db.t_sys_conditiontrade_template_sell
                                    where item.TemplateId == request.TemplateId && item.Type == request.Type
                                    select new ConditiontradeTemplateSellDetailsInfo
                                    {
                                        Status = item.Status,
                                        ConditionDay = item.ConditionDay,
                                        ConditionPriceBase = item.ConditionPriceBase,
                                        ConditionPriceType = item.ConditionType,
                                        ConditionRelativeRate = item.ConditionPriceRate,
                                        ConditionRelativeType = item.ConditionPriceType,
                                        ConditionTime = item.ConditionTime,
                                        CreateTime = item.CreateTime,
                                        EntrustCount = item.EntrustCount,
                                        EntrustPriceGear = item.EntrustPriceGear,
                                        ForbidType = item.ForbidType,
                                        EntrustType = item.EntrustType,
                                        OtherConditionRelative = item.OtherConditionRelative,
                                        Id = item.Id,
                                        Name = item.Name,
                                        ChildList = (from x in db.t_sys_conditiontrade_template_sell_child
                                                     join x2 in db.t_sys_conditiontrade_template_sell on x.ChildId equals x2.Id
                                                     where x.FatherId == item.Id
                                                     select new ConditionChild
                                                     {
                                                         Status = x.Status,
                                                         Type = x2.Type,
                                                         ChildId = x.ChildId
                                                     }).ToList()
                                    }).ToList();
                return sell_details;
            }
        }

        /// <summary>
        /// 添加条件卖出模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddConditiontradeTemplateSellDetails(AddConditiontradeTemplateSellDetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断模板是否存在
                    var template = (from item in db.t_sys_conditiontrade_template
                                    where item.Id == request.TemplateId && item.Type == 2
                                    select item).FirstOrDefault();
                    if (template == null)
                    {
                        throw new WebApiException(400, "模板不存在");
                    }
                    var temp = new t_sys_conditiontrade_template_sell
                    {
                        ConditionTime = request.ConditionTime,
                        CreateTime = DateTime.Now,
                        EntrustCount = request.EntrustCount,
                        EntrustPriceGear = request.EntrustPriceGear,
                        EntrustType = request.EntrustType,
                        LastModified = DateTime.Now,
                        Status = 1,
                        ForbidType = request.ForbidType,
                        Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name,
                        Type = request.Type,
                        ConditionDay = request.ConditionDay,
                        ConditionPriceBase = request.ConditionPriceBase,
                        ConditionPriceRate = request.ConditionRelativeRate,
                        ConditionPriceType = request.ConditionRelativeType,
                        ConditionType = request.ConditionPriceType,
                        TemplateId = request.TemplateId,
                        OtherConditionRelative = request.OtherConditionRelative
                    };
                    db.t_sys_conditiontrade_template_sell.Add(temp);
                    db.SaveChanges();

                    int i = 0;
                    foreach (var item in request.ChildList)
                    {
                        if (item.ChildId > 0)
                        {
                            db.t_sys_conditiontrade_template_sell_child.Add(new t_sys_conditiontrade_template_sell_child
                            {
                                Status = item.Status,
                                ChildId = item.ChildId,
                                FatherId = temp.Id,
                            });
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        db.SaveChanges();
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex.InnerException;
                }
            }
        }

        /// <summary>
        /// 编辑条件卖出模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyConditiontradeTemplateSellDetails(ModifyConditiontradeTemplateSellDetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var conditiontrade = (from item in db.t_sys_conditiontrade_template_sell
                                          where item.Id == request.Id
                                          select item).FirstOrDefault();
                    if (conditiontrade == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    conditiontrade.EntrustCount = request.EntrustCount;
                    conditiontrade.EntrustPriceGear = request.EntrustPriceGear;
                    conditiontrade.EntrustType = request.EntrustType;
                    conditiontrade.ForbidType = request.ForbidType;
                    conditiontrade.LastModified = DateTime.Now;
                    conditiontrade.Name = request.Name;
                    conditiontrade.ConditionType = request.ConditionPriceType;
                    conditiontrade.ConditionPriceBase = request.ConditionPriceBase;
                    conditiontrade.ConditionPriceRate = request.ConditionRelativeRate;
                    conditiontrade.ConditionTime = request.ConditionTime;
                    conditiontrade.ConditionDay = request.ConditionDay;
                    conditiontrade.ConditionPriceType = request.ConditionRelativeType;
                    conditiontrade.OtherConditionRelative = request.OtherConditionRelative;
                    db.SaveChanges();

                    var child = (from item in db.t_sys_conditiontrade_template_sell_child
                                 where item.FatherId == request.Id
                                 select item).ToList();
                    if (child.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_sell_child.RemoveRange(child);
                        db.SaveChanges();
                    }

                    int i = 0;
                    foreach (var item in request.ChildList)
                    {
                        if (item.ChildId > 0)
                        {
                            db.t_sys_conditiontrade_template_sell_child.Add(new t_sys_conditiontrade_template_sell_child
                            {
                                Status = item.Status,
                                ChildId = item.ChildId,
                                FatherId = request.Id,
                            });
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        db.SaveChanges();
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
        /// 修改条件卖出模板状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyConditiontradeTemplateSellDetailsStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var template_sell = (from item in db.t_sys_conditiontrade_template_sell
                                     where item.Id == request.Id
                                     select item).FirstOrDefault();
                if (template_sell == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                template_sell.Status = request.Status;
                template_sell.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件卖出模板
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteConditiontradeTemplateSellDetails(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var template_sell = (from item in db.t_sys_conditiontrade_template_sell
                                         where item.Id == request.Id
                                         select item).FirstOrDefault();
                    if (template_sell == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_sys_conditiontrade_template_sell.Remove(template_sell);
                    db.SaveChanges();

                    var child = (from item in db.t_sys_conditiontrade_template_sell_child
                                 where item.FatherId == request.Id
                                 select item).ToList();
                    if (child.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_sell_child.RemoveRange(child);
                        db.SaveChanges();
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
        /// 获取条件买入模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyDetailsInfo> GetConditiontradeTemplateBuyDetailsList(GetConditiontradeTemplateBuyDetailsListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var BuyConditionList = from item in db.t_sys_conditiontrade_template_buy
                                       where item.TemplateId == request.TemplateId
                                       select item;
                int totalCount = BuyConditionList.Count();

                return new PageRes<ConditiontradeTemplateBuyDetailsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in BuyConditionList
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyDetailsInfo
                            {
                                Status = item.Status,
                                ConditionRelativeRate = item.ConditionPriceRate,
                                ConditionRelativeType = item.ConditionPriceType,
                                ConditionPriceBase = item.ConditionPriceBase,
                                ConditionPriceType = item.ConditionType,
                                Id = item.Id,
                                IsGreater = item.IsGreater,
                                EntrustType = item.EntrustType,
                                ForbidType = item.ForbidType,
                                EntrustPriceGear = item.EntrustPriceGear,
                                EntrustAmount = item.EntrustAmount,
                                EntrustAmountType=item.EntrustAmountType,
                                Name = item.Name,
                                BuyAuto = item.BuyAuto,
                                LimitUp = item.LimitUp,
                                IsHold = item.IsHold,
                                OtherConditionRelative = item.OtherConditionRelative,
                                CreateTime = item.CreateTime,
                                OtherConditionCount = (from x in db.t_sys_conditiontrade_template_buy_other
                                                       where x.TemplateBuyId == item.Id
                                                       select x).Count(),
                                AutoConditionCount = (from x in db.t_sys_conditiontrade_template_buy_auto
                                                      where x.TemplateBuyId == item.Id
                                                      select x).Count(),
                                ChildList = (from x in db.t_sys_conditiontrade_template_buy_child
                                             where x.FatherId == item.Id
                                             select new ConditionChild
                                             {
                                                 Status = x.Status,
                                                 ChildId = x.ChildId
                                             }).ToList()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyDetails(AddConditiontradeTemplateBuyDetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断模板是否存在
                    var template = (from item in db.t_sys_conditiontrade_template
                                    where item.Id == request.TemplateId
                                    select item).FirstOrDefault();
                    if (template == null)
                    {
                        throw new WebApiException(400, "设定的股票不存在");
                    }
                    t_sys_conditiontrade_template_buy temp = new t_sys_conditiontrade_template_buy
                    {
                        Status = 1,
                        BuyAuto = request.BuyAuto,
                        CreateTime = DateTime.Now,
                        EntrustAmount = request.EntrustAmount,
                        EntrustAmountType=request.EntrustAmountType,
                        EntrustPriceGear = request.EntrustPriceGear,
                        EntrustType = request.EntrustType,
                        LimitUp = request.LimitUp,
                        ForbidType = request.ForbidType,
                        IsGreater = request.IsGreater,
                        LastModified = DateTime.Now,
                        Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name,
                        ConditionType = request.ConditionPriceType,
                        ConditionPriceBase = request.ConditionPriceBase,
                        ConditionPriceRate = request.ConditionRelativeRate,
                        ConditionPriceType = request.ConditionRelativeType,
                        OtherConditionRelative = request.OtherConditionRelative,
                        TemplateId = request.TemplateId,
                        IsHold = request.IsHold
                    };
                    db.t_sys_conditiontrade_template_buy.Add(temp);
                    db.SaveChanges();

                    int i = 0;
                    foreach (var item in request.ChildList)
                    {
                        if (item.ChildId > 0)
                        {
                            db.t_sys_conditiontrade_template_buy_child.Add(new t_sys_conditiontrade_template_buy_child
                            {
                                Status = item.Status,
                                ChildId = item.ChildId,
                                FatherId = temp.Id,
                            });
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyDetails(ModifyConditiontradeTemplateBuyDetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var conditiontrade = (from item in db.t_sys_conditiontrade_template_buy
                                          where item.Id == request.Id
                                          select item).FirstOrDefault();
                    if (conditiontrade == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    conditiontrade.BuyAuto = request.BuyAuto;
                    conditiontrade.EntrustAmount = request.EntrustAmount;
                    conditiontrade.EntrustAmountType = request.EntrustAmountType;
                    conditiontrade.EntrustPriceGear = request.EntrustPriceGear;
                    conditiontrade.EntrustType = request.EntrustType;
                    conditiontrade.ForbidType = request.ForbidType;
                    conditiontrade.LastModified = DateTime.Now;
                    conditiontrade.Name = request.Name;
                    conditiontrade.ConditionType = request.ConditionPriceType;
                    conditiontrade.ConditionPriceBase = request.ConditionPriceBase;
                    conditiontrade.ConditionPriceRate = request.ConditionRelativeRate;
                    conditiontrade.ConditionPriceType = request.ConditionRelativeType;
                    conditiontrade.IsGreater = request.IsGreater;
                    conditiontrade.LimitUp = request.LimitUp;
                    conditiontrade.IsHold = request.IsHold;
                    conditiontrade.OtherConditionRelative = request.OtherConditionRelative;
                    db.SaveChanges();

                    var child = (from item in db.t_sys_conditiontrade_template_buy_child
                                 where item.FatherId == request.Id
                                 select item).ToList();
                    if (child.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_buy_child.RemoveRange(child);
                        db.SaveChanges();
                    }

                    int i = 0;
                    foreach (var item in request.ChildList)
                    {
                        if (item.ChildId > 0)
                        {
                            db.t_sys_conditiontrade_template_buy_child.Add(new t_sys_conditiontrade_template_buy_child
                            {
                                Status = item.Status,
                                ChildId = item.ChildId,
                                FatherId = request.Id,
                            });
                            i++;
                        }
                    }
                    if (i > 0)
                    {
                        db.SaveChanges();
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
        /// 修改条件买入模板详情状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyDetailsStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var template_buy = (from item in db.t_sys_conditiontrade_template_buy
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (template_buy == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                template_buy.Status = request.Status;
                template_buy.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板详情
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyDetails(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var template_buy = (from item in db.t_sys_conditiontrade_template_buy
                                        where item.Id == request.Id
                                        select item).FirstOrDefault();
                    if (template_buy == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_sys_conditiontrade_template_buy.Remove(template_buy);
                    db.SaveChanges();

                    var child = (from item in db.t_sys_conditiontrade_template_buy_child
                                 where item.FatherId == request.Id
                                 select item).ToList();
                    if (child.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_buy_child.RemoveRange(child);
                        db.SaveChanges();
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
        /// 获取条件买入模板额外条件分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherGroupInfo> GetConditiontradeTemplateBuyOtherGroupList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = from item in db.t_sys_conditiontrade_template_buy_other
                            where item.TemplateBuyId == request.Id
                            select item;
                int totalCount = other.Count();
                return new PageRes<ConditiontradeTemplateBuyOtherGroupInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in other
                            orderby item.CreateTime
                            select new ConditiontradeTemplateBuyOtherGroupInfo
                            {
                                Id = item.Id,
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyOtherGroup(AddConditiontradeTemplateBuyOtherGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_sys_conditiontrade_template_buy_other.Add(new t_sys_conditiontrade_template_buy_other
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    TemplateBuyId = request.DetailsId,
                    LastModified = DateTime.Now,
                    Name = request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑条件买入模板额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOtherGroup(ModifyConditiontradeTemplateBuyOtherGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_sys_conditiontrade_template_buy_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                other.Name = request.Name;
                other.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件买入模板额外条件分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOtherGroupStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_sys_conditiontrade_template_buy_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                other.Status = request.Status;
                other.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyOtherGroup(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_sys_conditiontrade_template_buy_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_other.Remove(other);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取条件买入模板转自动条件分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoGroupInfo> GetConditiontradeTemplateBuyAutoGroupList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = from item in db.t_sys_conditiontrade_template_buy_auto
                           where item.TemplateBuyId == request.Id
                           select item;
                int totalCount = auto.Count();
                return new PageRes<ConditiontradeTemplateBuyAutoGroupInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in auto
                            orderby item.CreateTime
                            select new ConditiontradeTemplateBuyAutoGroupInfo
                            {
                                Id = item.Id,
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板转自动条件分组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddConditiontradeTemplateBuyAutoGroup(AddConditiontradeTemplateBuyAutoGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_sys_conditiontrade_template_buy_auto.Add(new t_sys_conditiontrade_template_buy_auto
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    TemplateBuyId = request.DetailsId,
                    LastModified = DateTime.Now,
                    Name = request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyAutoGroup(ModifyConditiontradeTemplateBuyAutoGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = (from item in db.t_sys_conditiontrade_template_buy_auto
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (auto == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                auto.Name = request.Name;
                auto.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件买入模板转自动条件分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyAutoGroupStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = (from item in db.t_sys_conditiontrade_template_buy_auto
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (auto == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                auto.Status = request.Status;
                auto.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板转自动条件分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyAutoGroup(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = (from item in db.t_sys_conditiontrade_template_buy_auto
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (auto == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_auto.Remove(auto);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取条件买入模板额外条件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateBuyOtherList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_sys_conditiontrade_template_buy_other_trend
                            where item.OtherId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherInfo
                            {
                                Status = item.Status,
                                TrendId = item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName,
                                OtherParCount = (from x in db.t_sys_conditiontrade_template_buy_other_trend_other
                                                 where x.OtherTrendId == item.Id
                                                 select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyOther(AddConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_sys_conditiontrade_template_buy_other_trend temp = new t_sys_conditiontrade_template_buy_other_trend
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_sys_conditiontrade_template_buy_other_trend.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_sys_conditiontrade_template_buy_other_trend_par.Add(new t_sys_conditiontrade_template_buy_other_trend_par
                        {
                            OtherTrendId = temp.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                    if (par.Count() > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑条件买入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOther(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.TrendDescription = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件买入模板额外条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOtherStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyOther(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_other_trend.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取条件买入模板转自动条件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoInfo> GetConditiontradeTemplateBuyAutoList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_sys_conditiontrade_template_buy_auto_trend
                            where item.AutoId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<ConditiontradeTemplateBuyAutoInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyAutoInfo
                            {
                                Status = item.Status,
                                TrendId = item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName,
                                OtherParCount = (from x in db.t_sys_conditiontrade_template_buy_auto_trend_other
                                                 where x.AutoTrendId == item.Id
                                                 select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板转自动条件
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyAuto(AddConditiontradeTemplateBuyAutoRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_sys_conditiontrade_template_buy_auto_trend temp = new t_sys_conditiontrade_template_buy_auto_trend
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        AutoId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_sys_conditiontrade_template_buy_auto_trend.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_sys_conditiontrade_template_buy_auto_trend_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_par
                        {
                            AutoTrendId = temp.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                    if (par.Count() > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑条件买入模板转自动条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void ModifyConditiontradeTemplateBuyAuto(ModifyConditiontradeTemplateBuyAutoRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.TrendDescription = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件买入模板转自动条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyAutoStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板转自动条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyAuto(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_auto_trend.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                               where item.OtherTrendId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherParPlate(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                                where item.OtherTrendId == request.Id
                                select item).ToList();
                List<ConditiontradeTemplateBuyOtherParInfo> list = new List<ConditiontradeTemplateBuyOtherParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new ConditiontradeTemplateBuyOtherParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyOtherPar(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                               where item.OtherTrendId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_sys_conditiontrade_template_buy_other_trend_par.Add(new t_sys_conditiontrade_template_buy_other_trend_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            OtherTrendId = request.RelId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                                       where item.OtherTrendId == request.RelId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_sys_conditiontrade_template_buy_other_trend_par.Add(new t_sys_conditiontrade_template_buy_other_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        OtherTrendId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅)
        /// </summary>
        /// <param name="TrendId"></param>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyOtherPar(int Type, long RelId, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                               where item.OtherTrendId == RelId
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList();
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_sys_conditiontrade_template_buy_other_trend_par
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  OtherTrendId = RelId,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_sys_conditiontrade_template_buy_other_trend_par.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅2)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyOtherPar2(int Type, long RelId, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate = item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_sys_conditiontrade_template_buy_other_trend_par.Add(new t_sys_conditiontrade_template_buy_other_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherTrendId = RelId,
                        ParamsInfo = JsonConvert.SerializeObject(new
                        {
                            GroupId = plate.Id,
                            GroupName = plate.Name,
                            GroupType = plate.Type,
                            DataType = 2,
                            Compare = compare,
                            Rate = rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 批量删除条件买入模板额外条件类型参数（板块涨跌幅）
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteConditiontradeTemplateBuyOtherPar(BatchDeleteConditiontradeTemplateBuyOtherParRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                                where item.OtherTrendId == request.OtherTrendId
                                select item).ToList();
                List<t_sys_conditiontrade_template_buy_other_trend_par> list = new List<t_sys_conditiontrade_template_buy_other_trend_par>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId >= 0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_sys_conditiontrade_template_buy_other_trend_par.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 编辑条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOtherPar(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyOtherPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_other_trend_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                               where item.AutoTrendId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<ConditiontradeTemplateBuyAutoParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyAutoParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoParPlate(GetConditiontradeTemplateBuyAutoParPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                where item.AutoTrendId == request.Id
                                select item).ToList();
                List<ConditiontradeTemplateBuyAutoParInfo> list = new List<ConditiontradeTemplateBuyAutoParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new ConditiontradeTemplateBuyAutoParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<ConditiontradeTemplateBuyAutoParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyAutoParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyAutoPar(AddConditiontradeTemplateBuyAutoParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                               where item.AutoTrendId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_sys_conditiontrade_template_buy_auto_trend_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            AutoTrendId = request.RelId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                       where item.AutoTrendId == request.RelId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_sys_conditiontrade_template_buy_auto_trend_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        AutoTrendId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="TrendId"></param>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyAutoPar(int Type, long RelId, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                               where item.AutoTrendId == RelId
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList();
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_sys_conditiontrade_template_buy_auto_trend_par
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  AutoTrendId = RelId,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_sys_conditiontrade_template_buy_auto_trend_par.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyAutoPar2(int Type, long RelId, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate = item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_sys_conditiontrade_template_buy_auto_trend_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        AutoTrendId = RelId,
                        ParamsInfo = JsonConvert.SerializeObject(new
                        {
                            GroupId = plate.Id,
                            GroupName = plate.Name,
                            GroupType = plate.Type,
                            DataType = 2,
                            Compare = compare,
                            Rate = rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 批量删除条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteConditiontradeTemplateBuyAutoPar(BatchDeleteConditiontradeTemplateBuyAutoParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                where item.AutoTrendId == request.AutoTrendId
                                select item).ToList();
                List<t_sys_conditiontrade_template_buy_auto_trend_par> list = new List<t_sys_conditiontrade_template_buy_auto_trend_par>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId >= 0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_sys_conditiontrade_template_buy_auto_trend_par.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyAutoPar(ModifyConditiontradeTemplateBuyAutoParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyAutoPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_auto_trend_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        #region=======额外关系=======
        /// <summary>
        /// 获取条件买入模板额外条件-额外关系列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateBuyOtherList_Other(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_sys_conditiontrade_template_buy_other_trend_other
                            where item.OtherTrendId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherInfo
                            {
                                Status = item.Status,
                                TrendId = item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyOther_Other(AddConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_sys_conditiontrade_template_buy_other_trend_other temp = new t_sys_conditiontrade_template_buy_other_trend_other
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherTrendId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_sys_conditiontrade_template_buy_other_trend_other.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_sys_conditiontrade_template_buy_other_trend_other_par.Add(new t_sys_conditiontrade_template_buy_other_trend_other_par
                        {
                            OtherTrendOtherId = temp.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                    if (par.Count() > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑条件买入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOther_Other(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.TrendDescription = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件买入模板额外条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOtherStatus_Other(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyOther_Other(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_other_trend_other.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取条件买入模板转自动条件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoInfo> GetConditiontradeTemplateBuyAutoList_Other(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_sys_conditiontrade_template_buy_auto_trend_other
                            where item.AutoTrendId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<ConditiontradeTemplateBuyAutoInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyAutoInfo
                            {
                                Status = item.Status,
                                TrendId = item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板转自动条件
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyAuto_Other(AddConditiontradeTemplateBuyAutoRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_sys_conditiontrade_template_buy_auto_trend_other temp = new t_sys_conditiontrade_template_buy_auto_trend_other
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        AutoTrendId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_sys_conditiontrade_template_buy_auto_trend_other.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_sys_conditiontrade_template_buy_auto_trend_other_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_other_par
                        {
                            AutoTrendOtherId = temp.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                    if (par.Count() > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑条件买入模板转自动条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void ModifyConditiontradeTemplateBuyAuto_Other(ModifyConditiontradeTemplateBuyAutoRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.TrendDescription = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改条件买入模板转自动条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyAutoStatus_Other(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板转自动条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyAuto_Other(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_auto_trend_other.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherPar_Other(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                               where item.OtherTrendOtherId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询条件买入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateBuyOtherParPlate_Other(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                where item.OtherTrendOtherId == request.Id
                                select item).ToList();
                List<ConditiontradeTemplateBuyOtherParInfo> list = new List<ConditiontradeTemplateBuyOtherParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new ConditiontradeTemplateBuyOtherParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyOtherPar_Other(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                               where item.OtherTrendOtherId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_sys_conditiontrade_template_buy_other_trend_other_par.Add(new t_sys_conditiontrade_template_buy_other_trend_other_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            OtherTrendOtherId = request.RelId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                       where item.OtherTrendOtherId == request.RelId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_sys_conditiontrade_template_buy_other_trend_other_par.Add(new t_sys_conditiontrade_template_buy_other_trend_other_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        OtherTrendOtherId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅)
        /// </summary>
        /// <param name="TrendId"></param>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyOtherPar_Other(int Type, long RelId, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                               where item.OtherTrendOtherId == RelId
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList();
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_sys_conditiontrade_template_buy_other_trend_other_par
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  OtherTrendOtherId = RelId,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_sys_conditiontrade_template_buy_other_trend_other_par.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅2)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyOtherPar2_Other(int Type, long RelId, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate = item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_sys_conditiontrade_template_buy_other_trend_other_par.Add(new t_sys_conditiontrade_template_buy_other_trend_other_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherTrendOtherId = RelId,
                        ParamsInfo = JsonConvert.SerializeObject(new
                        {
                            GroupId = plate.Id,
                            GroupName = plate.Name,
                            GroupType = plate.Type,
                            DataType = 2,
                            Compare = compare,
                            Rate = rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 批量删除条件买入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteConditiontradeTemplateBuyOtherPar_Other(BatchDeleteConditiontradeTemplateBuyOtherPar_OtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                where item.OtherTrendOtherId == request.OtherTrendOtherId
                                select item).ToList();
                List<t_sys_conditiontrade_template_buy_other_trend_other_par> list = new List<t_sys_conditiontrade_template_buy_other_trend_other_par>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId >= 0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_sys_conditiontrade_template_buy_other_trend_other_par.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 编辑条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyOtherPar_Other(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyOtherPar_Other(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_other_trend_other_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_other_trend_other_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoPar_Other(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                               where item.AutoTrendOtherId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<ConditiontradeTemplateBuyAutoParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyAutoParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyAutoParInfo> GetConditiontradeTemplateBuyAutoParPlate_Other(GetConditiontradeTemplateBuyAutoParPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                where item.AutoTrendOtherId == request.Id
                                select item).ToList();
                List<ConditiontradeTemplateBuyAutoParInfo> list = new List<ConditiontradeTemplateBuyAutoParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new ConditiontradeTemplateBuyAutoParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<ConditiontradeTemplateBuyAutoParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyAutoParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateBuyAutoPar_Other(AddConditiontradeTemplateBuyAutoParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                               where item.AutoTrendOtherId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_sys_conditiontrade_template_buy_auto_trend_other_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_other_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            AutoTrendOtherId = request.RelId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                       where item.AutoTrendOtherId == request.RelId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_sys_conditiontrade_template_buy_auto_trend_other_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_other_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        AutoTrendOtherId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="TrendId"></param>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyAutoPar_Other(int Type, long RelId, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                               where item.AutoTrendOtherId == RelId
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList();
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_sys_conditiontrade_template_buy_auto_trend_other_par
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  AutoTrendOtherId = RelId,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_sys_conditiontrade_template_buy_auto_trend_other_par.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量添加条件买入模板转自动条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateBuyAutoPar2_Other(int Type, long RelId, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate = item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_sys_conditiontrade_template_buy_auto_trend_other_par.Add(new t_sys_conditiontrade_template_buy_auto_trend_other_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        AutoTrendOtherId = RelId,
                        ParamsInfo = JsonConvert.SerializeObject(new
                        {
                            GroupId = plate.Id,
                            GroupName = plate.Name,
                            GroupType = plate.Type,
                            DataType = 2,
                            Compare = compare,
                            Rate = rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 批量删除条件买入模板转自动条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteConditiontradeTemplateBuyAutoPar_Other(BatchDeleteConditiontradeTemplateBuyAutoPar_OtherRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                where item.AutoTrendOtherId == request.AutoTrendOtherId
                                select item).ToList();
                List<t_sys_conditiontrade_template_buy_auto_trend_other_par> list = new List<t_sys_conditiontrade_template_buy_auto_trend_other_par>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId >= 0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_sys_conditiontrade_template_buy_auto_trend_other_par.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 编辑条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateBuyAutoPar_Other(ModifyConditiontradeTemplateBuyAutoParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件买入模板转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateBuyAutoPar_Other(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_buy_auto_trend_other_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_buy_auto_trend_other_par.Remove(trendPar);
                db.SaveChanges();
            }
        }
        #endregion

        /// <summary>
        /// 获取走势模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendInfo> GetSharesConditionTrendList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_account_shares_conditiontrade_buy_trend_template
                            select item;

                int totalCount = trend.Count();

                return new PageRes<SharesMonitorTrendInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加走势模板
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesConditionTrend(AddSharesMonitorTrendRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_account_shares_conditiontrade_buy_trend_template.Add(new t_account_shares_conditiontrade_buy_trend_template
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    Description = request.Description,
                    LastModified = DateTime.Now,
                    Name = request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑走势模板
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesConditionTrend(ModifySharesMonitorTrendRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_trend_template
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                trend.Name = request.Name;
                trend.Description = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改走势模板状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesConditionTrendStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_trend_template
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除走势模板
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesConditionTrend(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_trend_template
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_account_shares_conditiontrade_buy_trend_template.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取走势模板参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendParInfo> GetSharesConditionTrendPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                               where item.TrendId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<SharesMonitorTrendParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 获取条件单走势模板参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesMonitorTrendParInfo> GetSharesConditionTrendParPlate(GetSharesConditionTrendParPlatePageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                                where item.TrendId == request.Id
                                select item).ToList();
                List<SharesMonitorTrendParInfo> list = new List<SharesMonitorTrendParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new SharesMonitorTrendParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<SharesMonitorTrendParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new SharesMonitorTrendParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加走势模板参数
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesConditionTrendPar(AddSharesMonitorTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                               where item.TrendId == request.TrendId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_account_shares_conditiontrade_buy_trend_par_template.Add(new t_account_shares_conditiontrade_buy_trend_par_template
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            TrendId = request.TrendId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                                       where item.TrendId == request.TrendId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_account_shares_conditiontrade_buy_trend_par_template.Add(new t_account_shares_conditiontrade_buy_trend_par_template
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        TrendId = request.TrendId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddSharesConditionTrendPar(int Type, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                               where item.TrendId == 7
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList(); 
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_account_shares_conditiontrade_buy_trend_par_template
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  TrendId = 7,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_account_shares_conditiontrade_buy_trend_par_template.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量删除条件单走势模板参数(板块涨跌幅)
        /// </summary>
        public void BatchDeleteSharesConditionTrendPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                                where item.TrendId==7
                                select item).ToList();
                List<t_account_shares_conditiontrade_buy_trend_par_template> list = new List<t_account_shares_conditiontrade_buy_trend_par_template>();
                foreach (var item in trendPar)
                {
                    var temp=JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId>=0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_account_shares_conditiontrade_buy_trend_par_template.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 批量条件单走势模板参数(板块涨跌幅2)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddSharesConditionTrendPar2(int Type, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate =item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_account_shares_conditiontrade_buy_trend_par_template.Add(new t_account_shares_conditiontrade_buy_trend_par_template
                    { 
                        CreateTime=DateTime.Now,
                        LastModified=DateTime.Now,
                        TrendId=7,
                        ParamsInfo=JsonConvert.SerializeObject(new 
                        {
                            GroupId= plate.Id,
                            GroupName=plate.Name,
                            GroupType= plate.Type,
                            DataType= 2,
                            Compare= compare,
                            Rate= rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 编辑走势模板参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesConditionTrendPar(ModifySharesMonitorTrendParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                                where item.Id == request.Id && (item.TrendId == 1 || item.TrendId==7)
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除走势模板参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesConditionTrendPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template
                                where item.Id == request.Id && (item.TrendId == 1 || item.TrendId==7)
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_trend_par_template.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取自动买入限制列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SharesLimitAutoBuyInfo> GetSharesLimitAutoBuyList(PageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var limit = from item in db.t_shares_limit_autobuy
                            select item;

                int totalCount = limit.Count();

                return new PageRes<SharesLimitAutoBuyInfo>
                {
                    TotalCount = totalCount,
                    MaxId = 0,
                    List = (from item in limit
                            orderby item.CreateTime descending
                            select new SharesLimitAutoBuyInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                LimitKey = item.LimitKey,
                                LimitMarket = item.LimitMarket,
                                MaxBuyCount = item.MaxBuyCount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动买入限制
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesLimitAutoBuy(AddSharesLimitAutoBuyRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_shares_limit_autobuy.Add(new t_shares_limit_autobuy 
                { 
                    Status=1,
                    CreateTime=DateTime.Now,
                    LastModified=DateTime.Now,
                    LimitKey=request.LimitKey,
                    LimitMarket=request.LimitMarket,
                    MaxBuyCount=request.MaxBuyCount
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑自动买入限制
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesLimitAutoBuy(ModifySharesLimitAutoBuyRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var limit = (from item in db.t_shares_limit_autobuy
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (limit == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                limit.LastModified = DateTime.Now;
                limit.LimitKey = request.LimitKey;
                limit.LimitMarket = request.LimitMarket;
                limit.MaxBuyCount = request.MaxBuyCount;
                db.SaveChanges(); 
            }
        }

        /// <summary>
        /// 修改自动买入限制状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesLimitAutoBuyStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var limit = (from item in db.t_shares_limit_autobuy
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (limit == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                limit.LastModified = DateTime.Now;
                limit.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动买入限制
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesLimitAutoBuy(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var limit = (from item in db.t_shares_limit_autobuy
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (limit == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_autobuy.Remove(limit);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取自动买入优先级列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesPriorityAutoBuyInfo> GetSharesPriorityAutoBuyList(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_limit_autobuy_priority
                             join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                             from ai in a.DefaultIfEmpty()
                             select new { item, ai };
                int totalCount = result.Count();

                return new PageRes<SharesPriorityAutoBuyInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            orderby item.item.CreateTime descending
                            select new SharesPriorityAutoBuyInfo
                            {
                                AccountId = item.ai == null ? 0 : item.ai.Id,
                                AccountMobile = item.ai == null ? "" : item.ai.Mobile,
                                AccountNickName = item.ai == null ? "" : item.ai.NickName,
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id,
                                Priority = item.item.Priority
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动买入优先级
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPriorityAutoBuy(AddSharesPriorityAutoBuyRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断用户是否存在
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.AccountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400,"账户不存在");
                }
                //判断是否已添加
                var priority = (from item in db.t_shares_limit_autobuy_priority
                                where item.AccountId == request.AccountId
                                select item).FirstOrDefault();
                if (priority != null)
                {
                    throw new WebApiException(400,"账户已添加");
                }

                db.t_shares_limit_autobuy_priority.Add(new t_shares_limit_autobuy_priority 
                { 
                    AccountId=request.AccountId,
                    CreateTime=DateTime.Now,
                    LastModified=DateTime.Now,
                    Priority=request.Priority
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑自动买入优先级
        /// </summary>
        /// <param name="request"></param>
        public void ModifySharesPriorityAutoBuy(ModifySharesPriorityAutoBuyRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_limit_autobuy_priority
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断用户是否存在
                var account = (from item in db.t_account_baseinfo
                               where item.Id == request.AccountId
                               select item).FirstOrDefault();
                if (account == null)
                {
                    throw new WebApiException(400, "账户不存在");
                }
                //判断是否已添加
                var priority = (from item in db.t_shares_limit_autobuy_priority
                                where item.AccountId == request.AccountId && item.Id != request.Id
                                select item).FirstOrDefault();
                if (priority != null)
                {
                    throw new WebApiException(400, "账户已添加");
                }

                result.AccountId = request.AccountId;
                result.LastModified = DateTime.Now;
                result.Priority = request.Priority;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动买入优先级
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPriorityAutoBuy(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_limit_autobuy_priority
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_autobuy_priority.Remove(result);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取自动买入优先级适用板块列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesPriorityAutoBuyPlateInfo> GetSharesPriorityAutoBuyPlateList(GetSharesPriorityAutoBuyPlateListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var plate = from item in db.t_shares_plate
                            where item.Type == request.Type
                            select item;
                var priority_plate = from item in db.t_shares_limit_autobuy_priority_plate
                                     join item2 in plate on item.PlateId equals item2.Id into a from ai in a.DefaultIfEmpty()
                                     where item.PriorityId == request.PriorityId && (item.PlateId > 0 || -request.Type == item.PlateId)
                                     select new { item, ai };
                int totalCount = priority_plate.Count();
                return new PageRes<SharesPriorityAutoBuyPlateInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in priority_plate
                            orderby item.item.CreateTime descending
                            select new SharesPriorityAutoBuyPlateInfo
                            {
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id,
                                PlateId = item.item.PlateId,
                                PlateName = item.ai==null?(item.item.PlateId==-1?"全部行业": item.item.PlateId == -2 ? "全部地区" : item.item.PlateId == -3 ? "全部概念" :"未知") : item.ai.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动买入优先级适用板块
        /// </summary>
        /// <param name="request"></param>
        public void AddSharesPriorityAutoBuyPlate(AddSharesPriorityAutoBuyPlateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断优先级是否存在
                var priority = (from item in db.t_shares_limit_autobuy_priority
                                where item.Id == request.PriorityId
                                select item).FirstOrDefault();
                if (priority == null)
                {
                    throw new WebApiException(400,"优先级不存在");
                }
                //判断板块是否存在
                if (request.PlateId >= 0)
                {
                    var plate = (from item in db.t_shares_plate
                                 where item.Id == request.PlateId
                                 select item).FirstOrDefault();
                    if (plate == null)
                    {
                        throw new WebApiException(400, "板块不存在");
                    }
                }
                //判断板块是否已经添加
                var priority_plate = (from item in db.t_shares_limit_autobuy_priority_plate
                                      where item.PriorityId == request.PriorityId && item.PlateId == request.PlateId
                                      select item).FirstOrDefault();
                if (priority_plate != null)
                {
                    throw new WebApiException(400,"板块已添加");
                }
                db.t_shares_limit_autobuy_priority_plate.Add(new t_shares_limit_autobuy_priority_plate 
                {
                    CreateTime=DateTime.Now,
                    LastModified=DateTime.Now,
                    PlateId=request.PlateId,
                    PriorityId=request.PriorityId
                });
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 删除自动买入优先级适用板块
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPriorityAutoBuyPlate(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断板块是否已经添加
                var priority_plate = (from item in db.t_shares_limit_autobuy_priority_plate
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                if (priority_plate == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_shares_limit_autobuy_priority_plate.Remove(priority_plate);
                db.SaveChanges();

            }
        }

        #endregion

        #region===自动加入===
        /// <summary>
        /// 获取自动加入模板详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateJoinDetailsInfo> GetConditiontradeTemplateJoinDetailsList(GetConditiontradeTemplateJoinDetailsListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var joinList = from item in db.t_sys_conditiontrade_template_join
                               where item.TemplateId == request.TemplateId
                               select item;
                int totalCount = joinList.Count();

                var list = (from item in joinList
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateJoinDetailsInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                IsClearOriginal = item.IsClearOriginal,
                                Name = item.Name,
                                TimeCycle = item.TimeCycle,
                                TimeCycleType = item.TimeCycleType,
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                List<long> JoinIdList = list.Select(e => e.Id).ToList();
                var join_other = (from item in db.t_sys_conditiontrade_template_join_other
                                  where JoinIdList.Contains(item.TemplateJoinId)
                                  select item).ToList();
                foreach (var item in list)
                {
                    item.OtherCount = join_other.Where(e => e.TemplateJoinId == item.Id).Count();
                }
                return new PageRes<ConditiontradeTemplateJoinDetailsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 添加自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateJoinDetails(AddConditiontradeTemplateJoinDetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断模板是否存在
                    var template = (from item in db.t_sys_conditiontrade_template
                                    where item.Id == request.TemplateId
                                    select item).FirstOrDefault();
                    if (template == null)
                    {
                        throw new WebApiException(400, "模板不存在");
                    }
                    t_sys_conditiontrade_template_join temp = new t_sys_conditiontrade_template_join
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name,
                        TemplateId = request.TemplateId,
                        IsClearOriginal = request.IsClearOriginal,
                        TimeCycle = request.TimeCycle,
                        TimeCycleType = request.TimeCycleType
                    };
                    db.t_sys_conditiontrade_template_join.Add(temp);
                    db.SaveChanges();

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
        /// 编辑自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinDetails(ModifyConditiontradeTemplateJoinDetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var joinInfo = (from item in db.t_sys_conditiontrade_template_join
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                    if (joinInfo == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    joinInfo.LastModified = DateTime.Now;
                    joinInfo.Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name;
                    joinInfo.IsClearOriginal = request.IsClearOriginal;
                    joinInfo.TimeCycle = request.TimeCycle;
                    joinInfo.TimeCycleType = request.TimeCycleType;
                    db.SaveChanges();

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
        /// 修改自动加入模板详情状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinDetailsStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var joinInfo = (from item in db.t_sys_conditiontrade_template_join
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                    if (joinInfo == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    joinInfo.LastModified = DateTime.Now;
                    joinInfo.Status = request.Status;
                    db.SaveChanges();

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
        /// 删除自动加入模板详情
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateJoinDetails(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var joinInfo = (from item in db.t_sys_conditiontrade_template_join
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                    if (joinInfo == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_sys_conditiontrade_template_join.Remove(joinInfo);
                    db.SaveChanges();

                    //删除条件
                    var other = (from item in db.t_sys_conditiontrade_template_join_other
                                 where item.TemplateJoinId == request.Id
                                 select item).ToList();
                    if (other.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other.RemoveRange(other);
                        db.SaveChanges();
                    }

                    List<long> OtherIdList = other.Select(e => e.Id).ToList();
                    var otherTrend = (from item in db.t_sys_conditiontrade_template_join_other_trend
                                      where OtherIdList.Contains(item.OtherId)
                                      select item).ToList();
                    if (otherTrend.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend.RemoveRange(otherTrend);
                        db.SaveChanges();
                    }

                    List<long> OtherTrendIdList = otherTrend.Select(e => e.Id).ToList();
                    var otherTrendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                         where OtherTrendIdList.Contains(item.OtherTrendId)
                                         select item).ToList();
                    if (otherTrendPar.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_par.RemoveRange(otherTrendPar);
                        db.SaveChanges();
                    }

                    var otherTrendOther = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                                           where OtherTrendIdList.Contains(item.OtherTrendId)
                                           select item).ToList();
                    if (otherTrendOther.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other.RemoveRange(otherTrendOther);
                        db.SaveChanges();
                    }

                    List<long> otherTrendOtherIdList = otherTrendOther.Select(e => e.Id).ToList();
                    var otherTrendOtherPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                              where otherTrendOtherIdList.Contains(item.OtherTrendOtherId)
                                              select item).ToList();
                    if (otherTrendOther.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other_par.RemoveRange(otherTrendOtherPar);
                        db.SaveChanges();
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
        /// 获取自动加入模板额外条件分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherGroupInfo> GetConditiontradeTemplateJoinOtherGroupList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = from item in db.t_sys_conditiontrade_template_join_other
                            where item.TemplateJoinId == request.Id
                            select item;
                int totalCount = other.Count();
                return new PageRes<ConditiontradeTemplateBuyOtherGroupInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in other
                            orderby item.CreateTime
                            select new ConditiontradeTemplateBuyOtherGroupInfo
                            {
                                Id = item.Id,
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动加入模板额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateJoinOtherGroup(AddConditiontradeTemplateBuyOtherGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_sys_conditiontrade_template_join_other.Add(new t_sys_conditiontrade_template_join_other
                {
                    TemplateJoinId = request.DetailsId,
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Name = request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑自动加入模板额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOtherGroup(ModifyConditiontradeTemplateBuyOtherGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_sys_conditiontrade_template_join_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                other.Name = request.Name;
                other.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改自动加入模板额外条件分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOtherGroupStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_sys_conditiontrade_template_join_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                other.Status = request.Status;
                other.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动加入模板额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateJoinOtherGroup(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //删除条件
                    var other = (from item in db.t_sys_conditiontrade_template_join_other
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                    if (other == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_sys_conditiontrade_template_join_other.Remove(other);
                    db.SaveChanges();

                    var otherTrend = (from item in db.t_sys_conditiontrade_template_join_other_trend
                                      where item.OtherId == request.Id
                                      select item).ToList();
                    if (otherTrend.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend.RemoveRange(otherTrend);
                        db.SaveChanges();
                    }

                    List<long> OtherTrendIdList = otherTrend.Select(e => e.Id).ToList();
                    var otherTrendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                         where OtherTrendIdList.Contains(item.OtherTrendId)
                                         select item).ToList();
                    if (otherTrendPar.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_par.RemoveRange(otherTrendPar);
                        db.SaveChanges();
                    }

                    var otherTrendOther = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                                           where OtherTrendIdList.Contains(item.OtherTrendId)
                                           select item).ToList();
                    if (otherTrendOther.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other.RemoveRange(otherTrendOther);
                        db.SaveChanges();
                    }

                    List<long> otherTrendOtherIdList = otherTrendOther.Select(e => e.Id).ToList();
                    var otherTrendOtherPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                              where otherTrendOtherIdList.Contains(item.OtherTrendOtherId)
                                              select item).ToList();
                    if (otherTrendOther.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other_par.RemoveRange(otherTrendOtherPar);
                        db.SaveChanges();
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
        /// 获取自动加入模板额外条件列表
        /// </summary>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateJoinOtherList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_sys_conditiontrade_template_join_other_trend
                            where item.OtherId == request.Id
                            select item;
                int totalCount = trend.Count();

                var list = (from item in trend
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherInfo
                            {
                                Status = item.Status,
                                TrendId = item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                List<long> otehrTrendIdList = trend.Select(e => e.Id).ToList();
                var otherTrendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                                     where otehrTrendIdList.Contains(item.OtherTrendId)
                                     select item).ToList();

                foreach (var item in list)
                {
                    item.OtherParCount = otherTrendPar.Where(e => e.OtherTrendId == item.Id).Count();
                }

                return new PageRes<ConditiontradeTemplateBuyOtherInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 添加自动加入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateJoinOther(AddConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_sys_conditiontrade_template_join_other_trend temp = new t_sys_conditiontrade_template_join_other_trend
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_sys_conditiontrade_template_join_other_trend.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_par.Add(new t_sys_conditiontrade_template_join_other_trend_par
                        {
                            OtherTrendId = temp.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                    if (par.Count() > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑自动加入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void ModifyConditiontradeTemplateJoinOther(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_join_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.TrendDescription = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改自动加入模板额外条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOtherStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_join_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动加入模板额外条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateJoinOther(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var otherTrend = (from item in db.t_sys_conditiontrade_template_join_other_trend
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                    if (otherTrend == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_sys_conditiontrade_template_join_other_trend.Remove(otherTrend);
                    db.SaveChanges();

                    var otherTrendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                         where item.OtherTrendId == request.Id
                                         select item).ToList();
                    if (otherTrendPar.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_par.RemoveRange(otherTrendPar);
                        db.SaveChanges();
                    }

                    var otherTrendOther = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                                           where item.OtherTrendId == request.Id
                                           select item).ToList();
                    if (otherTrendOther.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other.RemoveRange(otherTrendOther);
                        db.SaveChanges();
                    }

                    List<long> otherTrendOtherIdList = otherTrendOther.Select(e => e.Id).ToList();
                    var otherTrendOtherPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                              where otherTrendOtherIdList.Contains(item.OtherTrendOtherId)
                                              select item).ToList();
                    if (otherTrendOther.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other_par.RemoveRange(otherTrendOtherPar);
                        db.SaveChanges();
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
        /// 查询自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherPar(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_sys_conditiontrade_template_join_other_trend_par
                               where item.OtherTrendId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数(板块涨跌幅)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherParPlate(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                where item.OtherTrendId == request.Id
                                select item).ToList();
                List<ConditiontradeTemplateBuyOtherParInfo> list = new List<ConditiontradeTemplateBuyOtherParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new ConditiontradeTemplateBuyOtherParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateJoinOtherPar(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                               where item.OtherTrendId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_par.Add(new t_sys_conditiontrade_template_join_other_trend_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            OtherTrendId = request.RelId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                       where item.OtherTrendId == request.RelId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_sys_conditiontrade_template_join_other_trend_par.Add(new t_sys_conditiontrade_template_join_other_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        OtherTrendId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅1)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateJoinOtherPar(int Type, long RelId, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                               where item.OtherTrendId == RelId
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList();
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_sys_conditiontrade_template_join_other_trend_par
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  OtherTrendId = RelId,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_sys_conditiontrade_template_join_other_trend_par.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅2)
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateJoinOtherPar2(int Type, long RelId, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate = item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_sys_conditiontrade_template_join_other_trend_par.Add(new t_sys_conditiontrade_template_join_other_trend_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherTrendId = RelId,
                        ParamsInfo = JsonConvert.SerializeObject(new
                        {
                            GroupId = plate.Id,
                            GroupName = plate.Name,
                            GroupType = plate.Type,
                            DataType = 2,
                            Compare = compare,
                            Rate = rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 批量删除自动加入模板额外条件类型参数（板块涨跌幅）
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteConditiontradeTemplateJoinOtherPar(BatchDeleteConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                where item.OtherTrendId == request.OtherTrendId
                                select item).ToList();
                List<t_sys_conditiontrade_template_join_other_trend_par> list = new List<t_sys_conditiontrade_template_join_other_trend_par>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId >= 0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_sys_conditiontrade_template_join_other_trend_par.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 编辑自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOtherPar(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动加入模板额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateJoinOtherPar(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_join_other_trend_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取自动加入模板额外条件列表-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherInfo> GetConditiontradeTemplateJoinOtherList_Other(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_sys_conditiontrade_template_join_other_trend_other
                            where item.OtherTrendId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherInfo
                            {
                                Status = item.Status,
                                TrendId = item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动加入模板额外条件-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateJoinOther_Other(AddConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_sys_conditiontrade_template_join_other_trend_other temp = new t_sys_conditiontrade_template_join_other_trend_other
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherTrendId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_sys_conditiontrade_template_join_other_trend_other.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other_par.Add(new t_sys_conditiontrade_template_join_other_trend_other_par
                        {
                            OtherTrendOtherId = temp.Id,
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                    if (par.Count() > 0)
                    {
                        db.SaveChanges();
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
        /// 编辑自动加入模板额外条件-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOther_Other(ModifyConditiontradeTemplateBuyOtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.TrendDescription = request.Description;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改自动加入模板额外条件状态-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOtherStatus_Other(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动加入模板额外条件-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateJoinOther_Other(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var trend = (from item in db.t_sys_conditiontrade_template_join_other_trend_other
                                 where item.Id == request.Id
                                 select item).FirstOrDefault();
                    if (trend == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_sys_conditiontrade_template_join_other_trend_other.Remove(trend);
                    db.SaveChanges();

                    //删除par
                    var other_par = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                     where item.OtherTrendOtherId == request.Id
                                     select item).ToList();
                    if (other_par.Count() > 0)
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other_par.RemoveRange(other_par);
                        db.SaveChanges();
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
        /// 查询自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherPar_Other(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                               where item.OtherTrendOtherId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询自动加入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<ConditiontradeTemplateBuyOtherParInfo> GetConditiontradeTemplateJoinOtherParPlate_Other(GetConditiontradeTemplateBuyOtherParPlateRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                where item.OtherTrendOtherId == request.Id
                                select item).ToList();
                List<ConditiontradeTemplateBuyOtherParInfo> list = new List<ConditiontradeTemplateBuyOtherParInfo>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new ConditiontradeTemplateBuyOtherParInfo
                        {
                            CreateTime = item.CreateTime,
                            Id = item.Id,
                            ParamsInfo = item.ParamsInfo
                        });
                    }
                }
                int totalCount = list.Count();

                return new PageRes<ConditiontradeTemplateBuyOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in list
                            orderby item.CreateTime descending
                            select new ConditiontradeTemplateBuyOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void AddConditiontradeTemplateJoinOtherPar_Other(AddConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1 && request.TrendId != 7)
                {
                    var par = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                               where item.OtherTrendOtherId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_sys_conditiontrade_template_join_other_trend_other_par.Add(new t_sys_conditiontrade_template_join_other_trend_other_par
                        {
                            CreateTime = DateTime.Now,
                            LastModified = DateTime.Now,
                            ParamsInfo = request.ParamsInfo,
                            OtherTrendOtherId = request.RelId
                        });
                    }
                }
                else
                {
                    if (request.TrendId == 7)
                    {
                        var source = JsonConvert.DeserializeObject<dynamic>(request.ParamsInfo);
                        long sourcegroupId = source.GroupId;
                        int sourcegroupType = source.GroupType;
                        int sourcedataType = source.DataType;
                        //判断分组是否存在
                        var tempLit = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                       where item.OtherTrendOtherId == request.RelId
                                       select item).ToList();
                        foreach (var item in tempLit)
                        {
                            var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                            long groupId = 0;
                            int groupType = 0;
                            int dataType = 0;
                            try
                            {
                                groupId = temp.GroupId;
                                groupType = temp.GroupType;
                                dataType = temp.DataType;
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (sourcegroupId == groupId && sourcegroupType == groupType && sourcedataType == dataType)
                            {
                                throw new WebApiException(400, "该分组已添加");
                            }
                        }
                    }
                    db.t_sys_conditiontrade_template_join_other_trend_other_par.Add(new t_sys_conditiontrade_template_join_other_trend_other_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        ParamsInfo = request.ParamsInfo,
                        OtherTrendOtherId = request.RelId
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅1)-额外关系
        /// </summary>
        /// <param name="TrendId"></param>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateJoinOtherPar_Other(int Type, long RelId, List<string> list)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询分组
                var plate = (from item in db.t_shares_plate
                             where item.Type == Type && list.Contains(item.Name)
                             select item).ToList();
                //判断分组是否存在
                var tempLit = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                               where item.OtherTrendOtherId == RelId
                               select item).ToList();
                List<long> groupIdList = plate.Select(e => e.Id).ToList();
                foreach (var item in tempLit)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    long groupId = 0;
                    int groupType = 0;
                    int dataType = 0;
                    try
                    {
                        groupId = temp.GroupId;
                        groupType = temp.GroupType;
                        dataType = temp.DataType;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (groupIdList.Contains(groupId) && groupType == Type && dataType == 1)
                    {
                        groupIdList.Remove(groupId);
                    }
                }
                plate = (from item in plate
                         where groupIdList.Contains(item.Id)
                         select item).ToList();
                var result = (from item in plate
                              select new t_sys_conditiontrade_template_join_other_trend_other_par
                              {
                                  CreateTime = DateTime.Now,
                                  LastModified = DateTime.Now,
                                  OtherTrendOtherId = RelId,
                                  ParamsInfo = JsonConvert.SerializeObject(new
                                  {
                                      GroupId = item.Id,
                                      GroupName = item.Name,
                                      GroupType = Type,
                                      DataType = 1,
                                  })
                              }).ToList();
                db.t_sys_conditiontrade_template_join_other_trend_other_par.AddRange(result);
                db.SaveChanges();
                return result.Count();
            }
        }

        /// <summary>
        /// 批量添加自动加入模板额外条件类型参数(板块涨跌幅2)-额外关系
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int BatchAddConditiontradeTemplateJoinOtherPar2_Other(int Type, long RelId, List<BatchAddSharesConditionTrendPar2Obj> list)
        {
            using (var db = new meal_ticketEntities())
            {
                int i = 0;
                foreach (var item in list)
                {
                    string groupName = item.GroupName;
                    int compare = item.Compare;
                    string rate = item.Rate;
                    //查询分组
                    var plate = (from x in db.t_shares_plate
                                 where x.Type == Type && x.Name == groupName
                                 select x).FirstOrDefault();
                    if (plate == null)
                    {
                        continue;
                    }
                    db.t_sys_conditiontrade_template_join_other_trend_other_par.Add(new t_sys_conditiontrade_template_join_other_trend_other_par
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherTrendOtherId = RelId,
                        ParamsInfo = JsonConvert.SerializeObject(new
                        {
                            GroupId = plate.Id,
                            GroupName = plate.Name,
                            GroupType = plate.Type,
                            DataType = 2,
                            Compare = compare,
                            Rate = rate,
                        })
                    });

                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 批量删除自动加入模板额外条件类型参数(板块涨跌幅)-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void BatchDeleteConditiontradeTemplateJoinOtherPar_Other(BatchDeleteConditiontradeTemplateBuyOtherPar_OtherRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                where item.OtherTrendOtherId == request.OtherTrendOtherId
                                select item).ToList();
                List<t_sys_conditiontrade_template_join_other_trend_other_par> list = new List<t_sys_conditiontrade_template_join_other_trend_other_par>();
                foreach (var item in trendPar)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    int dataType = 0;
                    int groupType = 0;
                    long groupId = 0;
                    try
                    {
                        dataType = temp.DataType;
                        groupType = temp.GroupType;
                        groupId = temp.GroupId;
                    }
                    catch (Exception ex)
                    { }
                    if ((dataType == request.Id / 10 || dataType == 0) && (groupType == request.Id % 10 || groupType == 0) && groupId >= 0)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count() > 0)
                {
                    db.t_sys_conditiontrade_template_join_other_trend_other_par.RemoveRange(list);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 编辑自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateJoinOtherPar_Other(ModifyConditiontradeTemplateBuyOtherParRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自动加入模板额外条件类型参数-额外关系
        /// </summary>
        /// <param name="request"></param>
        public void DeleteConditiontradeTemplateJoinOtherPar_Other(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_sys_conditiontrade_template_join_other_trend_other_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_sys_conditiontrade_template_join_other_trend_other_par.Remove(trendPar);
                db.SaveChanges();
            }
        }
        #endregion

        #region===搜索===
        /// <summary>
        /// 获取搜索模板详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object GetConditiontradeTemplateSearchDetails(DetailsRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_sys_conditiontrade_template_search
                              where item.TemplateId == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    return null;
                }
                return new 
                {
                    TemplateId=result.TemplateId,
                    TemplateContent=result.TemplateContent,
                };
            }
        }

        /// <summary>
        /// 编辑搜索模板详情
        /// </summary>
        /// <param name="request"></param>
        public void ModifyConditiontradeTemplateSearchDetails(ModifyConditiontradeTemplateSearchDetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_sys_conditiontrade_template_search
                              where item.TemplateId == request.TemplateId
                              select item).FirstOrDefault();
                if (result == null)
                {
                    db.t_sys_conditiontrade_template_search.Add(new t_sys_conditiontrade_template_search
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        TemplateContent = request.TemplateContent,
                        TemplateId = request.TemplateId
                    });
                }
                else
                {
                    result.TemplateContent = request.TemplateContent;
                    result.LastModified = DateTime.Now;
                }
                db.SaveChanges();
            }
        }
        #endregion

        /// <summary>
        /// 获取k线数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesKlineInfo> GetSharesKlineList(GetSharesKlineListRequest request) 
        {
            switch (request.DateType)
            {
                case 1:
                    return null;
                case 2:
                    return GetSharesKline_1minList(request);
                case 3:
                    return GetSharesKline_5minList(request);
                case 4:
                    return GetSharesKline_15minList(request);
                case 5:
                    return GetSharesKline_30minList(request);
                case 6:
                    return GetSharesKline_60minList(request);
                case 7:
                    return GetSharesKline_1dayList(request);
                case 8:
                    return GetSharesKline_1weekList(request);
                case 9:
                    return GetSharesKline_1monthList(request);
                case 10:
                    return GetSharesKline_1quarterList(request);
                case 11:
                    return GetSharesKline_1yearList(request);
                default:
                    throw new WebApiException(400,"参数有误");
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_1minList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_1min
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_5minList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_5min
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_15minList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_15min
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_30minList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_30min
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_60minList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_60min
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_1dayList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_1day
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_1weekList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_1week
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_1monthList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_1month
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_1quarterList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_1quarter
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesKline_1yearList(GetSharesKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_securitybarsdata_1year
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        /// <summary>
        /// 重置K线数据
        /// </summary>
        /// <param name="request"></param>
        public void ResetSharesKLine(ResetSharesKLineRequest request)
        {
            DateTime timeNow = DateTime.Now;
            //判断是否交易时间
            if (Helper.CheckTradeTime(timeNow) || Helper.CheckTradeTime(timeNow.AddMinutes(15)))
            {
                throw new WebApiException(400, "交易时间范围内无法重置");
            }
            DateTime tempDate = DateTime.Parse("1991-01-01 00:00:00");
            if (request.DateType == 1)
            {
                tempDate = DbHelper.GetLastTradeDate2(0,0,0,0, timeNow.Date);

                long StartGroupTimeKey = -1;
                if (!ParseTimeGroupKey(DateTime.Parse(tempDate.ToString("yyyy-MM-dd 09:30:00")), request.DataType, ref StartGroupTimeKey))
                {
                    throw new WebApiException(400,"时间值错误");
                }
                request.StartGroupTimeKey = StartGroupTimeKey;

                long EndGroupTimeKey = -1;
                if (!ParseTimeGroupKey(DateTime.Parse(tempDate.ToString("yyyy-MM-dd 15:00:00")), request.DataType, ref EndGroupTimeKey))
                {
                    throw new WebApiException(400, "时间值错误");
                }
                request.EndGroupTimeKey = EndGroupTimeKey;
            }
            else
            {
                switch (request.DataType)
                {
                    case 2:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 3:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 4:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 5:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 6:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 7:
                        tempDate = DateTime.ParseExact(request.StartGroupTimeKey.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 8:
                        var temp1 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_week == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp1 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp1.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 9:
                        var temp2 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_month == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp2 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp2.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 10:
                        var temp3 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_quarter == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp3 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp3.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 11:
                        var temp4 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_year == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp4 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp4.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new WebApiException(400, "参数错误");
                }
            }

            long PreClosePrice = 0;
            using (var db = new meal_ticketEntities())
            {
                var quoteDate = (from item in db.t_shares_quotes_date
                                 where item.Market == request.Market && item.SharesCode == request.SharesCode && item.LastModified >= tempDate
                                 orderby item.LastModified
                                 select item).FirstOrDefault();
                if (quoteDate != null)
                {
                    PreClosePrice = quoteDate.ClosedPrice;
                }
            }
            int SecurityBarsGetCount = int.Parse(ConfigurationManager.AppSettings["SecurityBarsGetCount"]);

            List<dynamic> list = new List<dynamic>();
            list.Add(new
            {
                SharesCode = request.SharesCode,
                Market = request.Market,
                StartTimeKey = request.StartGroupTimeKey,
                EndTimeKey = request.EndGroupTimeKey,
                PreClosePrice = PreClosePrice,
                YestodayClosedPrice = PreClosePrice,
                LastTradeStock = 0,
                LastTradeAmount = 0
            });


            var packageList = new List<dynamic>();
            packageList.Add(new
            {
                DataList = list,
                DataType = request.DataType,
                RetryCount = 1,
                TotalRetryCount = Singleton.Instance.TotalRetryCount,
                SecurityBarsGetCount = SecurityBarsGetCount
            });
            var sendData = new
            {
                HandlerType = request.DateType == 1 ? 21 : 2,
                PackageList = packageList,
            };
            Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SecurityBars", "1min");
        }

        /// <summary>
        /// 解析时间值
        /// </summary>
        /// <returns></returns>
        private bool ParseTimeGroupKey(DateTime date, int type, ref long groupTimeKey)
        {
            switch (type)
            {
                case 2:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 3:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 4:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 5:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 6:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMddHHmm"));
                    break;
                case 7:
                    groupTimeKey = long.Parse(date.ToString("yyyyMMdd"));
                    break;
                case 8:
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(date.ToString("yyyyMMdd"))
                                    select item.the_week).FirstOrDefault() ?? 0;
                    break;
                case 9:
                    groupTimeKey = long.Parse(date.ToString("yyyyMM"));
                    break;
                case 10:
                    groupTimeKey = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                    where item.the_date == int.Parse(date.ToString("yyyyMMdd"))
                                    select item.the_quarter).FirstOrDefault() ?? 0;
                    break;
                case 11:
                    groupTimeKey = long.Parse(date.ToString("yyyy"));
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 批量重置K线数据
        /// </summary>
        /// <param name="request"></param>
        public void BatchResetSharesKLine(BatchResetSharesKLineRequest request)
        {
            DateTime timeNow = DateTime.Now;
            //判断是否交易时间
            if (Helper.CheckTradeTime(timeNow) || Helper.CheckTradeTime(timeNow.AddMinutes(15)))
            {
                throw new WebApiException(400, "交易时间范围内无法重置");
            }

            DateTime tempDate = DateTime.Parse("1991-01-01 00:00:00");
            if (request.DateType == 1)
            {
                tempDate = DbHelper.GetLastTradeDate2(0, 0, 0, 0, timeNow.Date);

                long StartGroupTimeKey = -1;
                if (!ParseTimeGroupKey(DateTime.Parse(tempDate.ToString("yyyy-MM-dd 09:30:00")), request.DataType, ref StartGroupTimeKey))
                {
                    throw new WebApiException(400, "时间值错误");
                }
                request.StartGroupTimeKey = StartGroupTimeKey;

                long EndGroupTimeKey = -1;
                if (!ParseTimeGroupKey(DateTime.Parse(tempDate.ToString("yyyy-MM-dd 15:00:00")), request.DataType, ref EndGroupTimeKey))
                {
                    throw new WebApiException(400, "时间值错误");
                }
                request.EndGroupTimeKey = EndGroupTimeKey;
            }
            else
            {
                switch (request.DataType)
                {
                    case 2:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 3:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 4:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 5:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 6:
                        tempDate = DateTime.ParseExact((request.StartGroupTimeKey / 10000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 7:
                        tempDate = DateTime.ParseExact(request.StartGroupTimeKey.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 8:
                        var temp1 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_week == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp1 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp1.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 9:
                        var temp2 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_month == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp2 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp2.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 10:
                        var temp3 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_quarter == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp3 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp3.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    case 11:
                        var temp4 = (from item in Singleton.Instance._DimTimeSession.GetSessionData()
                                     where item.the_year == request.StartGroupTimeKey
                                     orderby item.the_date
                                     select item).FirstOrDefault();
                        if (temp4 == null)
                        {
                            throw new WebApiException(400, "时间超出界限");
                        }
                        tempDate = DateTime.ParseExact(temp4.the_date.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new WebApiException(400, "参数错误");
                }
            }

            using (var db = new meal_ticketEntities())
            {
                var quoteDate = (from item in db.t_shares_quotes_date
                                 where item.LastModified >= tempDate
                                 group item by new { item.Market, item.SharesCode } into g
                                 select new
                                 {
                                     Market = g.Key.Market,
                                     SharesCode = g.Key.SharesCode,
                                     PreClosePrice = g.OrderBy(e => e.LastModified).Select(e => e.ClosedPrice).FirstOrDefault()
                                 }).ToList();
                int SecurityBarsGetCount = int.Parse(ConfigurationManager.AppSettings["SecurityBarsGetCount"]);



                int totalCount = quoteDate.Count();
                int finishCount = 0;
                int batchCount = 500;
                do 
                {
                    if (finishCount >= totalCount)
                    {
                        break;
                    }
                    var packageList = new List<dynamic>();
                    List<dynamic> list = new List<dynamic>();
                    var batchList = quoteDate.Skip(finishCount).Take(batchCount).ToList();
                    int currCount = batchList.Count();
                    foreach (var item in batchList)
                    {
                        list.Add(new
                        {
                            SharesCode = item.SharesCode,
                            Market = item.Market,
                            StartTimeKey = request.StartGroupTimeKey,
                            EndTimeKey = request.EndGroupTimeKey,
                            PreClosePrice = item.PreClosePrice,
                            YestodayClosedPrice = item.PreClosePrice,
                            LastTradeStock = 0,
                            LastTradeAmount = 0
                        });
                    }

                    packageList.Add(new
                    {
                        DataList = list,
                        DataType = request.DataType,
                        RetryCount = 1,
                        TotalRetryCount = Singleton.Instance.TotalRetryCount,
                        SecurityBarsGetCount = SecurityBarsGetCount
                    });
                    var sendData = new
                    {
                        HandlerType = request.DateType == 1 ? 21 : 2,
                        PackageList = packageList
                    };
                    Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SecurityBars", "1min");
                    finishCount = finishCount + currCount;
                } while (true);
            }
        }

        /// <summary>
        /// 获取板块k线数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesKlineInfo> GetSharesPlateKlineList(GetSharesPlateKlineListRequest request)
        {
            switch (request.DateType)
            {
                case 1:
                    return null;
                case 2:
                    return GetSharesPlateKline_1minList(request);
                case 3:
                    return GetSharesPlateKline_5minList(request);
                case 4:
                    return GetSharesPlateKline_15minList(request);
                case 5:
                    return GetSharesPlateKline_30minList(request);
                case 6:
                    return GetSharesPlateKline_60minList(request);
                case 7:
                    return GetSharesPlateKline_1dayList(request);
                case 8:
                    return GetSharesPlateKline_1weekList(request);
                case 9:
                    return GetSharesPlateKline_1monthList(request);
                case 10:
                    return GetSharesPlateKline_1quarterList(request);
                case 11:
                    return GetSharesPlateKline_1yearList(request);
                default:
                    throw new WebApiException(400, "参数有误");
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_1minList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_1min
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_5minList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_5min
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_15minList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_15min
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_30minList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_30min
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_60minList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_60min
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_1dayList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_1day
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_1weekList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_1week
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_1monthList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_1month
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_1quarterList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_1quarter
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        private PageRes<SharesKlineInfo> GetSharesPlateKline_1yearList(GetSharesPlateKlineListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_shares_plate_securitybarsdata_1year
                             where item.PlateId==request.PlateId
                             select item;
                if (request.StartGroupTimeKey != -1 && request.EndGroupTimeKey != -1)
                {
                    result = from item in result
                             where item.GroupTimeKey >= request.StartGroupTimeKey && item.GroupTimeKey <= request.EndGroupTimeKey
                             select item;
                }

                int totalCount = result.Count();

                var resultList = (from item in result
                                  orderby item.Time descending
                                  select new SharesKlineInfo
                                  {
                                      TradeStock = item.TradeStock,
                                      ClosedPrice = item.ClosedPrice,
                                      Id = item.Id,
                                      LastModified = item.LastModified,
                                      MaxPrice = item.MaxPrice,
                                      MinPrice = item.MinPrice,
                                      OpenedPrice = item.OpenedPrice,
                                      Time = item.Time,
                                      TradeAmount = item.TradeAmount
                                  }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                scope.Complete();
                return new PageRes<SharesKlineInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = resultList
                };
            }
        }

        /// <summary>
        /// 重置板块K线数据
        /// </summary>
        /// <param name="request"></param>
        public void ResetSharesPlateKLine(ResetSharesPlateKLineRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var context = new
                {
                    GroupTimeKey = request.StartGroupTimeKey
                };
                BuildInstructions(DateTime.Now.Date.AddDays(1), 3, request.PlateId, 0, "", request.DataType, JsonConvert.SerializeObject(context),0, db);
            }
        }

        /// <summary>
        /// 批量重置板块K线数据
        /// </summary>
        /// <param name="request"></param>
        public void BatchResetSharesPlateKLine(BatchResetSharesPlateKLineRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var context = new
                {
                    GroupTimeKey = request.StartGroupTimeKey
                };
                BuildInstructions(DateTime.Now.Date.AddDays(1), 2, 0, 0, "", request.DataType, JsonConvert.SerializeObject(context), request.PlateType, db);
            }
        }

        private void BuildInstructions(DateTime date, int type, long plateId, int market, string sharesCode, int dataType, string context,int plateType, meal_ticketEntities db)
        {
            string sql = "";
            if (type == 1)
            {
                sql = string.Format("merge into t_shares_plate_rel_snapshot_instructions t using (select '{0}' [Date],{1} [Type],{2} PlateId,{3} Market,'{4}' SharesCode,'{5}' Context) t1 on t.[Date] = t1.[Date] and t.[Type]= t1.[Type] and t.PlateId = t1.PlateId and t.Market=t1.Market and t.SharesCode=t1.SharesCode when matched then update set Context = t1.Context, IsExcute = 0, ExcuteTime = null, LastModified = getdate() when not matched then insert([Date],[Type],PlateId,Market,SharesCode,DataType,Context,PlateType,IsExcute,ExcuteTime,CreateTime,LastModified) values(t1.[Date], t1.[Type], t1.PlateId, t1.Market, t1.SharesCode,0, t1.Context,0, 0, null, getdate(), getdate()); ", date, type, plateId, market, sharesCode, context);
            }
            else if (type == 2)
            {
                sql = string.Format("merge into t_shares_plate_rel_snapshot_instructions t using (select '{0}' [Date],{1} [Type],'{2}' Context, {3} DataType,{4} PlateType) t1 on t.[Date] = t1.[Date] and t.[Type]= t1.[Type] and t.DataType=t1.DataType and t.PlateType=t1.PlateType when matched then update set Context = t1.Context, IsExcute = 0, ExcuteTime = null, LastModified = getdate() when not matched then insert([Date],[Type],PlateId,Market,SharesCode,DataType,Context,PlateType,IsExcute,ExcuteTime,CreateTime,LastModified) values(t1.[Date], t1.[Type], 0, 0, '',t1.DataType, t1.Context,t1.PlateType, 0, null, getdate(), getdate()); ", date, type, context, dataType, plateType);
            }
            else if (type == 3)
            {
                sql = string.Format("merge into t_shares_plate_rel_snapshot_instructions t using (select '{0}' [Date],{1} [Type],{2} PlateId,'{3}' Context, {4} DataType) t1 on t.[Date] = t1.[Date] and t.[Type]= t1.[Type] and t.PlateId=t1.PlateId and t.DataType=t1.DataType when matched then update set Context = t1.Context, IsExcute = 0, ExcuteTime = null, LastModified = getdate() when not matched then insert([Date],[Type],PlateId,Market,SharesCode,DataType,Context,PlateType,IsExcute,ExcuteTime,CreateTime,LastModified) values(t1.[Date], t1.[Type], t1.PlateId, 0, '',t1.DataType, t1.Context,0, 0, null, getdate(), getdate()); ", date, type, plateId, context, dataType);
            }
            else
            {
                throw new WebApiException(400, "参数不正确");
            }
            db.Database.ExecuteSqlCommand(sql);
        }

        /// <summary>
        /// 获取股票所属基础板块
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetSharesBasePlateListRes GetSharesBasePlateList(GetSharesBasePlateListRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var force = (from item in db.t_shares_plate_rel_tag_force
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select new ForceInfo
                             {
                                 PlateId=item.PlateId,
                                 ForceType =item.Type,
                                 IsForce1=item.IsForce1,
                                 IsForce2=item.IsForce2
                             }).ToList().GroupBy(e=>e.PlateId).ToDictionary(k=>k.Key,v=>v.ToList());
                var plateList = (from item in db.t_shares_plate_rel
                                 join item2 in db.t_shares_plate on item.PlateId equals item2.Id
                                 join item3 in db.t_shares_plate_rel_tag_focuson on new { item.PlateId, item.Market, item.SharesCode } equals new { item3.PlateId, item3.Market, item3.SharesCode } into a
                                 from ai in a.DefaultIfEmpty()
                                 join item4 in db.t_shares_plate_rel_tag_trendlike on new { item.PlateId, item.Market, item.SharesCode } equals new { item4.PlateId, item4.Market, item4.SharesCode } into b
                                 from bi in b.DefaultIfEmpty()
                                 where item.Market == request.Market && item.SharesCode == request.SharesCode && item2.BaseStatus == 1 && item2.Status == 1 && item2.Type == 3
                                 select new SharesBasePlateInfo
                                 {
                                     IsFocusOn = ai == null ? false : ai.IsFocusOn,
                                     IsTrendLike = bi == null ? false : bi.IsTrendLike,
                                     PlateId = item.PlateId,
                                     PlateName = item2.Name
                                 }).ToList();
                foreach (var item in plateList)
                {
                    bool IsForce1_3 = false;
                    bool IsForce2_3 = false;
                    bool IsForce1_5 = false;
                    bool IsForce2_5 = false;
                    bool IsForce1_10 = false;
                    bool IsForce2_10 = false;
                    bool IsForce1_15 = false;
                    bool IsForce2_15 = false;
                    if (force.ContainsKey(item.PlateId))
                    {
                        var forceList = force[item.PlateId];
                        var item3 = forceList.Where(e => e.ForceType == 1).FirstOrDefault();
                        if (item3 != null)
                        {
                            IsForce1_3 = item3.IsForce1;
                            IsForce2_3 = item3.IsForce2;
                        }
                        var item5 = forceList.Where(e => e.ForceType == 2).FirstOrDefault();
                        if (item5 != null)
                        {
                            IsForce1_5 = item5.IsForce1;
                            IsForce2_5 = item5.IsForce2;
                        }
                        var item10 = forceList.Where(e => e.ForceType == 3).FirstOrDefault();
                        if (item10 != null)
                        {
                            IsForce1_10 = item10.IsForce1;
                            IsForce2_10 = item10.IsForce2;
                        }
                        var item15 = forceList.Where(e => e.ForceType == 4).FirstOrDefault();
                        if (item15 != null)
                        {
                            IsForce1_15 = item15.IsForce1;
                            IsForce2_15 = item15.IsForce2;
                        }
                    }
                    item.ForceList = new List<ForceInfo>();
                    item.ForceList.Add(new ForceInfo
                    {
                        PlateId = item.PlateId,
                        ForceType = 1,
                        IsForce1 = IsForce1_3,
                        IsForce2 = IsForce2_3,
                    });
                    item.ForceList.Add(new ForceInfo
                    {
                        PlateId = item.PlateId,
                        ForceType = 2,
                        IsForce1 = IsForce1_5,
                        IsForce2 = IsForce2_5,
                    });
                    item.ForceList.Add(new ForceInfo
                    {
                        PlateId = item.PlateId,
                        ForceType = 3,
                        IsForce1 = IsForce1_10,
                        IsForce2 = IsForce2_10,
                    });
                    item.ForceList.Add(new ForceInfo
                    {
                        PlateId = item.PlateId,
                        ForceType = 4,
                        IsForce1 = IsForce1_15,
                        IsForce2 = IsForce2_15,
                    });
                }
                bool isAuto = true;
                var setting = (from item in db.t_shares_plate_rel_tag_setting
                               where item.Market == request.Market && item.SharesCode == request.SharesCode
                               select item).FirstOrDefault();
                if (setting != null)
                {
                    isAuto = setting.IsAuto;
                }
                return new GetSharesBasePlateListRes 
                {
                    List= plateList,
                    IsAuto=isAuto
                };
            }
        }

        /// <summary>
        /// 设置股票所属基础板块标记
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifySharesBasePlateTag(ModifySharesBasePlateTagRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    _modifySharesBasePlateTag(request,db);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }

            _sendPushNotice((int)MsgType.MODIFYPLATETAG, request);
        }
        private void _modifySharesBasePlateTag(ModifySharesBasePlateTagRequest request, meal_ticketEntities db)
        {
            var setting = (from item in db.t_shares_plate_rel_tag_setting
                           where item.Market == request.Market && item.SharesCode == request.SharesCode
                           select item).FirstOrDefault();
            if (setting == null)
            {
                db.t_shares_plate_rel_tag_setting.Add(new t_shares_plate_rel_tag_setting
                {
                    SharesCode = request.SharesCode,
                    Market = request.Market,
                    IsAuto = request.IsAuto
                });
            }
            else
            {
                setting.IsAuto = request.IsAuto;
            }
            db.SaveChanges();

            var tag_focuson = (from item in db.t_shares_plate_rel_tag_focuson
                               where item.Market == request.Market && item.SharesCode == request.SharesCode
                               select item).ToList();
            //删除原先设置
            if (tag_focuson.Count() > 0)
            {
                db.t_shares_plate_rel_tag_focuson.RemoveRange(tag_focuson);
                db.SaveChanges();
            }

            var tag_trendlike = (from item in db.t_shares_plate_rel_tag_trendlike
                                 where item.Market == request.Market && item.SharesCode == request.SharesCode
                                 select item).ToList();
            //删除原先设置
            if (tag_trendlike.Count() > 0)
            {
                db.t_shares_plate_rel_tag_trendlike.RemoveRange(tag_trendlike);
                db.SaveChanges();
            }

            var tag_force = (from item in db.t_shares_plate_rel_tag_force
                             where item.Market == request.Market && item.SharesCode == request.SharesCode
                             select item).ToList();
            //删除原先设置
            if (tag_force.Count() > 0)
            {
                db.t_shares_plate_rel_tag_force.RemoveRange(tag_force);
                db.SaveChanges();
            }

            foreach (var item in request.ParList)
            {
                db.t_shares_plate_rel_tag_focuson.Add(new t_shares_plate_rel_tag_focuson
                {
                    SharesCode=request.SharesCode,
                    Market=request.Market,
                    PlateId=item.PlateId,
                    IsFocusOn=item.IsFocusOn
                });
                db.t_shares_plate_rel_tag_trendlike.Add(new t_shares_plate_rel_tag_trendlike
                {
                    SharesCode = request.SharesCode,
                    Market = request.Market,
                    PlateId = item.PlateId,
                    IsTrendLike = item.IsTrendLike
                });
                foreach (var force in item.ForceList)
                {
                    db.t_shares_plate_rel_tag_force.Add(new t_shares_plate_rel_tag_force
                    {
                        SharesCode = request.SharesCode,
                        Market = request.Market,
                        PlateId = item.PlateId,
                        IsForce1= force.IsForce1,
                        IsForce2= force.IsForce2,
                        Type= force.ForceType
                    });
                }
            }
            db.SaveChanges();
        }

        private enum MsgType
        {
            MODIFYPLATETAG,
            RESETPLATETAG,
            RESESHARESTAG,
        }
        private void _sendPushNotice(int msgType,object sendData) 
        {
            try
            {
                Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new 
                {
                    MsgType= msgType,
                    SendData= sendData
                })), "SharesPlateTag", "notice");
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("修改板块标记发送通知出错",ex);
            }
        }

        /// <summary>
        /// 重置板块标记
        /// </summary>
        /// <param name="basedata"></param>
        public void ResetSharesBasePlateTag(HeadBase basedata)
        {
            _sendPushNotice((int)MsgType.RESETPLATETAG, null);
        }

        /// <summary>
        /// 获取股票标记设置列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<SharesTagSettingInfo> GetSharesTagSettingList(PageRequest request,HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_plate_shares_rel_tag_setting
                             select item;
                int totalCount = result.Count();

                return new PageRes<SharesTagSettingInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            join item2 in db.t_plate_shares_rel_tag_setting_details on item.Type equals item2.SettingType into a
                            from ai in a.DefaultIfEmpty()
                            group new { item, ai } by item into g
                            orderby g.Key.CreateTime descending
                            select new SharesTagSettingInfo
                            {
                                Status = g.Key.Status,
                                CreateTime = g.Key.CreateTime,
                                DefaultCount = g.Key.DefaultCount,
                                Description = g.Key.Description,
                                Id = g.Key.Id,
                                Name = g.Key.Name,
                                Type = g.Key.Type,
                                DetailsCount = g.Where(e=>e.ai!=null).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 编辑股票标记设置
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifySharesTagSetting(ModifySharesTagSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_plate_shares_rel_tag_setting
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                result.DefaultCount = request.DefaultCount;
                result.Description = request.Description;
                result.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改股票标记设置状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifySharesTagSettingStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var setting = (from item in db.t_plate_shares_rel_tag_setting
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (setting == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                setting.Status = request.Status;
                setting.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取股票标记设置详情列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<SharesTagSettingDetails> GetSharesTagSettingDetailsList(DetailsPageRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var setting_details = from item in db.t_plate_shares_rel_tag_setting_details
                                      where item.SettingType == request.Id
                                      select item;
                int totalCount = setting_details.Count();

                return new PageRes<SharesTagSettingDetails>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in setting_details
                            orderby item.BaseCount
                            select new SharesTagSettingDetails
                            {
                                BaseCount = item.BaseCount,
                                CreateTime = item.CreateTime,
                                DisCount = item.DisCount,
                                Id = item.Id,
                                Status = item.Status
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票标记设置详情
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddSharesTagSettingDetails(AddSharesTagSettingDetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断基础数量是否存在
                var details = (from item in db.t_plate_shares_rel_tag_setting_details
                               where item.SettingType == request.SettingType && item.BaseCount == request.BaseCount
                               select item).FirstOrDefault();
                if (details != null)
                {
                    throw new WebApiException(400,"基础数量已存在");
                }
                db.t_plate_shares_rel_tag_setting_details.Add(new t_plate_shares_rel_tag_setting_details 
                {
                    SettingType=request.SettingType,
                    Status=1,
                    BaseCount=request.BaseCount,
                    CreateTime=DateTime.Now,
                    DisCount=request.DisCount,
                    LastModified=DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑股票标记设置详情
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifySharesTagSettingDetails(ModifySharesTagSettingDetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var details = (from item in db.t_plate_shares_rel_tag_setting_details
                               where item.Id==request.Id
                               select item).FirstOrDefault();
                if (details == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                //判断基础数量是否存在
                var temp = (from item in db.t_plate_shares_rel_tag_setting_details
                            where item.SettingType == details.SettingType && item.BaseCount == request.BaseCount && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "基础数量已存在");
                }
                details.BaseCount = request.BaseCount;
                details.DisCount = request.DisCount;
                details.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改股票标记设置详情状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifySharesTagSettingDetailsStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var details = (from item in db.t_plate_shares_rel_tag_setting_details
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (details == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
               
                details.Status = request.Status;
                details.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除股票标记设置详情
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteSharesTagSettingDetails(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var details = (from item in db.t_plate_shares_rel_tag_setting_details
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (details == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_plate_shares_rel_tag_setting_details.Remove(details);
                db.SaveChanges();
            }
        }

        public void ResetSharesTag(HeadBase basedata)
        {
            _sendPushNotice((int)MsgType.RESESHARESTAG, null);
        }

        /// <summary>
        /// 获取板块指数配置信息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<PlateIndexSettingInfo> GetPlateIndexSettingList(PageRequest request,HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var resultList = (from item in db.t_setting_plate_index
                                  select item).ToList();
                int totalCount = resultList.Count();
                resultList = (from item in resultList
                              orderby item.CreateTime descending
                              select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                List<PlateIndexSettingInfo> list = new List<PlateIndexSettingInfo>();
                foreach (var item in resultList)
                {
                    var temp = JsonConvert.DeserializeObject<dynamic>(item.ValueJson);
                    int Coefficient = temp.coefficient;
                    int CalDays = 0;
                    int CalPriceType = 0;
                    if (item.Type == 4)
                    {
                        CalDays = temp.calDays;
                        CalPriceType = temp.calPriceType;
                    }

                    list.Add(new PlateIndexSettingInfo
                    {
                        Description = item.Description,
                        Id = item.Id,
                        LastModified = item.LastModified,
                        Status=item.Status,
                        Name = item.Name,
                        Type = item.Type,
                        Coefficient = Coefficient,
                        CalDays = CalDays,
                        CalPriceType= CalPriceType
                    });
                }

                return new PageRes<PlateIndexSettingInfo> 
                {
                    TotalCount=0,
                    List= list
                };
            }
        }

        /// <summary>
        /// 编辑板块指数配置信息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyPlateIndexSetting(ModifyPlateIndexSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_setting_plate_index
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (temp == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                temp.Description = request.Description;
                temp.ValueJson = request.ValueJson;
                temp.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改板块指数配置信息状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyPlateIndexSettingStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_setting_plate_index
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (temp == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                temp.Status = request.Status;
                temp.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取板块联动列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<PlateLinkageSettingInfo> GetPlateLinkageSettingList(GetPlateLinkageSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var plate_linkage = from item in db.t_setting_plate_linkage
                                    join item2 in db.t_shares_plate on item.MainPlateId equals item2.Id
                                    where item2.Status == 1 && item2.BaseStatus == 1
                                    group item by new { item2.Id, item2.Name,item.IsReal } into g
                                    select g;
                if (!string.IsNullOrEmpty(request.PlateName))
                {
                    plate_linkage = from item in plate_linkage
                                    where item.Key.Name.Contains(request.PlateName)
                                    select item;
                }
                int totalCount = plate_linkage.Count();
                var list = (from item in plate_linkage
                            orderby item.Key.Name
                            select new PlateLinkageSettingInfo
                            {
                                MainPlateId = item.Key.Id,
                                MainPlateName = item.Key.Name,
                                IsReal=item.Key.IsReal,
                                LinkagePlateList = item.Select(e => e.LinkagePlateId).ToList()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                var basePlateIdList = (from item in db.t_shares_plate
                                       where item.Status == 1 && item.BaseStatus == 1
                                       select item.Id).ToList();
                foreach (var item in list)
                {
                    item.LinkagePlateList = item.LinkagePlateList.Intersect(basePlateIdList).ToList();
                }
                return new PageRes<PlateLinkageSettingInfo>
                {
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 同步板块联动
        /// </summary>
        /// <param name="basedata"></param>
        public void SynchroPlateLinkageSetting(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try

                {
                    //查询基础版块
                    var plate_base = (from item in db.t_shares_plate
                                      where item.Status == 1 && item.BaseStatus == 1
                                      select item).ToList();
                    //查询已存在板块联动
                    var plate_linkage = (from item in db.t_setting_plate_linkage
                                         group item by item.MainPlateId into g
                                         select g.Key).ToList();
                    //需要删除的板块
                    List<long> plate_remove = (from item in plate_linkage
                                               join item2 in plate_base on item equals item2.Id into a
                                               from ai in a.DefaultIfEmpty()
                                               where ai == null
                                               select item).ToList();
                    if (plate_remove.Count() > 0)
                    {
                        var remove_item = (from item in db.t_setting_plate_linkage
                                           where plate_remove.Contains(item.MainPlateId)
                                           select item).ToList();
                        db.t_setting_plate_linkage.RemoveRange(remove_item);
                        db.SaveChanges();
                    }

                    foreach (var item in plate_base)
                    {
                        if (!plate_linkage.Contains(item.Id))
                        {
                            db.t_setting_plate_linkage.Add(new t_setting_plate_linkage
                            {
                                IsReal = false,
                                MainPlateId = item.Id,
                                LinkagePlateId = 0
                            });
                        }
                    }
                    db.SaveChanges();
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
        /// 批量导入板块联动数据
        /// </summary>
        /// <param name="dataDic"></param>
        public void ExportPlateLinkageSetting(Dictionary<string, List<string>> dataDic) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var plate_base = (from item in db.t_shares_plate
                                      where item.Status == 1 && item.BaseStatus == 1
                                      select item).ToList().ToDictionary(k => k.Name, v => v);
                    Dictionary<long, List<long>> dataPar = new Dictionary<long, List<long>>();
                    foreach (var item in dataDic)
                    {
                        if (!plate_base.ContainsKey(item.Key))
                        {
                            continue;
                        }
                        var mainPlate = plate_base[item.Key].Id;
                        foreach (var linkage in item.Value)
                        {
                            if (!plate_base.ContainsKey(linkage))
                            {
                                continue;
                            }
                            var linkagePlate = plate_base[linkage].Id;
                            if (!dataPar.ContainsKey(mainPlate))
                            {
                                dataPar.Add(mainPlate, new List<long>());
                            }
                            dataPar[mainPlate].Add(linkagePlate);
                        }
                    }


                    //查询已有板块
                    var plate_linkage = (from item in db.t_setting_plate_linkage
                                         group item by new { item.MainPlateId, item.IsReal } into g
                                         select g.Key).ToList();
                    List<long> remove_plate = new List<long>();
                    List<t_setting_plate_linkage> add_plate = new List<t_setting_plate_linkage>();
                    foreach (var item in plate_linkage)
                    {
                        if (!dataPar.ContainsKey(item.MainPlateId))
                        {
                            continue;
                        }
                        remove_plate.Add(item.MainPlateId);
                        var tempAdd = dataPar[item.MainPlateId].Distinct().ToList();
                        if (tempAdd.Count() == 0)
                        {
                            add_plate.Add(new t_setting_plate_linkage
                            {
                                IsReal = item.IsReal,
                                MainPlateId = item.MainPlateId,
                                LinkagePlateId = 0
                            });
                        }
                        foreach (long plateId in tempAdd)
                        {
                            add_plate.Add(new t_setting_plate_linkage
                            {
                                IsReal = item.IsReal,
                                MainPlateId = item.MainPlateId,
                                LinkagePlateId = plateId
                            });
                        }
                    }

                    if (remove_plate.Count() > 0)
                    {
                        var remove_item = (from item in db.t_setting_plate_linkage
                                           where remove_plate.Contains(item.MainPlateId)
                                           select item).ToList();
                        db.t_setting_plate_linkage.RemoveRange(remove_item);
                        db.SaveChanges();
                    }
                    if (add_plate.Count() > 0)
                    {
                        db.t_setting_plate_linkage.AddRange(add_plate);
                        db.SaveChanges();
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
        /// 添加板块联动
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddPlateLinkageSetting(AddPlateLinkageSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断板块是否已经添加
                var plate_linkage = (from item in db.t_setting_plate_linkage
                                     where item.MainPlateId == request.MainPlateId
                                     select item).FirstOrDefault();
                if (plate_linkage != null)
                {
                    throw new WebApiException(400,"板块已存在");
                }
                db.t_setting_plate_linkage.Add(new t_setting_plate_linkage 
                {
                    LinkagePlateId=0,
                    MainPlateId= request.MainPlateId,
                    IsReal=false
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除板块联动
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public void DeletePlateLinkageSetting(DeleteRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var plate_linkage = (from item in db.t_setting_plate_linkage
                                     where item.MainPlateId == request.Id
                                     select item).ToList();
                if (plate_linkage.Count() > 0)
                {
                    db.t_setting_plate_linkage.RemoveRange(plate_linkage);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 设置板块联动列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyPlateLinkageSetting(ModifyPlateLinkageSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var plate_linkage = (from item in db.t_setting_plate_linkage
                                         where item.MainPlateId == request.MainPlateId
                                         select item).ToList();
                    if (plate_linkage.Count() > 0)
                    {
                        db.t_setting_plate_linkage.RemoveRange(plate_linkage);
                        db.SaveChanges();
                    }
                    int addCount = 0;
                    foreach (var item in request.LinkagePlateList)
                    {
                        if (request.MainPlateId == item)
                        {
                            continue;
                        }
                        db.t_setting_plate_linkage.Add(new t_setting_plate_linkage 
                        {
                            MainPlateId=request.MainPlateId,
                            LinkagePlateId=item,
                            IsReal = request.IsReal
                        });
                        addCount++;
                    }
                    if (addCount == 0)
                    {
                        db.t_setting_plate_linkage.Add(new t_setting_plate_linkage
                        {
                            MainPlateId = request.MainPlateId,
                            LinkagePlateId = 0,
                            IsReal = request.IsReal
                        });
                    }
                    db.SaveChanges();

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
        /// 获取可设置联动板块列表
        /// </summary>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public List<PlateLinkageAllInfo> GetPlateLinkageAllList(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_plate
                              where item.Status == 1 && item.BaseStatus == 1
                              group item by item.Type into g
                              orderby g.Key
                              select new PlateLinkageAllInfo
                              {
                                  PlateType = g.Key,
                                  PlateTypeName = g.Key == 1 ? "行业" : g.Key == 2 ? "地区" : "概念",
                                  PlateList = (from x in g
                                               orderby x.Name
                                               select new PlateNameInfo
                                               {
                                                   PlateId = x.Id,
                                                   PlateName = x.Name
                                               }).ToList()

                              }).ToList();
                return result;
            }
        }

        /// <summary>
        /// 获取板块股票联动列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<PlateSharesLinkageSettingInfo> GetPlateSharesLinkageSettingList(GetPlateSharesLinkageSettingListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares_linkage = from item in db.t_setting_plate_shares_linkage
                                    join item2 in db.t_shares_plate on item.MainPlateId equals item2.Id
                                    where item2.Status == 1 && item2.BaseStatus == 1
                                    group item by new { item2.Id, item2.Name,item.IsReal,item.IsDefault} into g
                                    select g;
                if (!string.IsNullOrEmpty(request.PlateName))
                {
                    shares_linkage = from item in shares_linkage
                                     where item.Key.Name.Contains(request.PlateName)
                                     select item;
                }
                int totalCount = shares_linkage.Count();
                var list = (from item in shares_linkage
                            orderby item.Key.Name
                            select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                List<PlateSharesLinkageSettingInfo> result = new List<PlateSharesLinkageSettingInfo>();
                foreach (var item in list)
                {
                    var LinkageSharesList = item.Where(e=>!string.IsNullOrEmpty(e.SharesCode)).Select(e => long.Parse(e.SharesCode) * 10 + e.Market).ToList();
                    result.Add(new PlateSharesLinkageSettingInfo 
                    {
                        MainPlateId=item.Key.Id,
                        MainPlateName=item.Key.Name,
                        IsReal= item.Key.IsReal,
                        IsDefault=item.Key.IsDefault,
                        LinkageSharesList = LinkageSharesList
                    });
                }
                return new PageRes<PlateSharesLinkageSettingInfo>
                {
                    TotalCount = totalCount,
                    List = result
                };
            }
        }

        /// <summary>
        /// 同步板块股票联动
        /// </summary>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public void SynchroPlateSharesLinkageSetting(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //查询基础版块
                    var plate_base = (from item in db.t_shares_plate
                                      where item.Status == 1 && item.BaseStatus == 1
                                      select item).ToList();
                    //查询已存在板块股票联动
                    var plate_shares_linkage = (from item in db.t_setting_plate_shares_linkage
                                                group item by new { item.MainPlateId, item.IsDefault } into g
                                                select g.Key).ToList().ToDictionary(k => k.MainPlateId, v => v);
                    //需要删除的板块
                    List<long> plate_shares_remove = (from item in plate_shares_linkage
                                                      join item2 in plate_base on new { MainPlateId = item.Key } equals new { MainPlateId = item2.Id } into a
                                                      from ai in a.DefaultIfEmpty()
                                                      where ai == null
                                                      select item.Key).ToList();
                    if (plate_shares_remove.Count() > 0)
                    {
                        var remove_item = (from item in db.t_setting_plate_shares_linkage
                                           where plate_shares_remove.Contains(item.MainPlateId)
                                           select item).ToList();
                        db.t_setting_plate_shares_linkage.RemoveRange(remove_item);
                        db.SaveChanges();
                    }

                    foreach (var item in plate_base)
                    {
                        if (!plate_shares_linkage.ContainsKey(item.Id))
                        {
                            db.t_setting_plate_shares_linkage.Add(new t_setting_plate_shares_linkage
                            {
                                IsReal = false,
                                MainPlateId = item.Id,
                                SharesCode = "",
                                Market = 0,
                                IsDefault=false
                            });
                        }
                    }
                    db.SaveChanges();
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
        /// 添加板块股票联动
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public void AddPlateSharesLinkageSetting(AddPlateSharesLinkageSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断板块是否已经添加
                var shares_linkage = (from item in db.t_setting_plate_shares_linkage
                                      where item.MainPlateId == request.MainPlateId
                                     select item).FirstOrDefault();
                if (shares_linkage != null)
                {
                    throw new WebApiException(400, "板块已存在");
                }
                db.t_setting_plate_shares_linkage.Add(new t_setting_plate_shares_linkage
                {
                    Market = 0,
                    SharesCode="",
                    MainPlateId = request.MainPlateId,
                    IsReal=false,
                    IsDefault=false,
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除板块股票联动
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public void DeletePlateSharesLinkageSetting(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares_linkage = (from item in db.t_setting_plate_shares_linkage
                                      where item.MainPlateId == request.Id
                                     select item).ToList();
                if (shares_linkage.Count() > 0)
                {
                    db.t_setting_plate_shares_linkage.RemoveRange(shares_linkage);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 设置联动板块股票
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public void ModifyPlateSharesLinkageSetting(ModifyPlateSharesLinkageSettingRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    bool IsDefault = false;
                    var shares_linkage = (from item in db.t_setting_plate_shares_linkage
                                         where item.MainPlateId == request.MainPlateId
                                         select item).ToList();
                    if (shares_linkage.Count() > 0)
                    {
                        IsDefault = shares_linkage[0].IsDefault;
                        db.t_setting_plate_shares_linkage.RemoveRange(shares_linkage);
                        db.SaveChanges();
                    }
                    request.LinkageSharesList = request.LinkageSharesList.Distinct().ToList();
                    foreach (var item in request.LinkageSharesList)
                    {
                        int market = (int)(item % 10);
                        string sharesCode = (item / 10).ToString().PadLeft(6, '0');
                        db.t_setting_plate_shares_linkage.Add(new t_setting_plate_shares_linkage
                        {
                            MainPlateId = request.MainPlateId,
                            SharesCode = sharesCode,
                            Market= market,
                            IsReal=request.IsReal,
                            IsDefault= IsDefault
                        });
                    }
                    if (request.LinkageSharesList.Count() == 0)
                    {
                        db.t_setting_plate_shares_linkage.Add(new t_setting_plate_shares_linkage
                        {
                            MainPlateId = request.MainPlateId,
                            SharesCode = "",
                            Market = 0,
                            IsReal= request.IsReal,
                            IsDefault = IsDefault
                        });
                    }
                    db.SaveChanges();

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
        /// 设置联动板块股票是否默认
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyPlateSharesLinkageSettingDefault(ModifyPlateSharesLinkageSettingDefaultRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var shares_linkage = (from item in db.t_setting_plate_shares_linkage
                                      where item.MainPlateId == request.MainPlateId
                                      select item).ToList();
                foreach (var item in shares_linkage)
                {
                    item.IsDefault = request.IsDefault;
                }
                db.SaveChanges();

            }
        }

        /// <summary>
        /// 获取可设置联动股票列表
        /// </summary>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public List<PlateSharesLinkageAllInfo> GetPlateSharesLinkageAllList(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_shares_plate
                              join item2 in db.t_shares_plate_rel on item.Id equals item2.PlateId
                              join item3 in db.t_shares_all on new { item2.Market,item2.SharesCode} equals new { item3.Market,item3.SharesCode}
                              where item.Status == 1 && item.BaseStatus == 1
                              group item3 by item into g
                              orderby g.Key.Id
                              select new PlateSharesLinkageAllInfo
                              {
                                  PlateId = g.Key.Id,
                                  PlateName = g.Key.Name+"(" + (g.Key.Type == 1 ? "行业" : g.Key.Type == 2 ? "地区" : "概念") + ")",
                                  SharesList = (from x in g
                                               orderby x.SharesCode
                                               select new SharesNameInfo
                                               {
                                                   SharesCode = x.SharesCode,
                                                   SharesName = x.SharesName,
                                                   Market=x.Market
                                               }).ToList()

                              }).ToList();
                return result;
            }
        }
    }
}
