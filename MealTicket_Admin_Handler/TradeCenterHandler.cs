using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                              join item3 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into a
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
                    var temp = (from x in db.t_shares_quotes
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
                                  MarketStatus=item.MarketStatus,
                                  SharesName = item.SharesName,
                                  Id = item.Id,
                                  CirculatingCapital=ai==null?0:ai.CirculatingCapital,
                                  TotalCapital=ai==null?0:ai.TotalCapital,
                                  MarketTime = ai == null ? MarketTime : ai.MarketTime,
                                  Business = ai == null ?"": ai.Business,
                                  Market = item.Market,
                                  IsSuspension = bi == null ? false : true,
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in result)
                {
                    var plate = (from x in db.t_shares_plate_rel
                                 join x2 in db.t_shares_plate on x.PlateId equals x2.Id
                                 where x.Market == item.Market && x.SharesCode == item.SharesCode
                                 select x2).ToList();
                    item.Industry = string.Join("/", plate.Where(e => e.Type == 1).Select(e=>e.Name).ToList());
                    item.Area = string.Join("/", plate.Where(e => e.Type == 2).Select(e => e.Name).ToList());
                    item.Idea = string.Join("/", plate.Where(e => e.Type == 3).Select(e => e.Name).ToList());
                }
                if (request.ShowSharesQuotes)
                {
                    result = (from x in result
                              join x2 in db.t_shares_quotes on new { x.Market, x.SharesCode } equals new { x2.Market, x2.SharesCode } into b
                              from bi in b.DefaultIfEmpty()
                              select new SharesInfo
                              {
                                  SharesCode = x.SharesCode,
                                  SharesNamePY = x.SharesNamePY,
                                  Status = x.Status,
                                  ForbidStatus = x.ForbidStatus,
                                  SharesName = x.SharesName,
                                  Id = x.Id,
                                  MarketStatus=x.MarketStatus,
                                  MarketTime = x.MarketTime,
                                  Market = x.Market,
                                  IsSuspension = x.IsSuspension,
                                  Area=x.Area,
                                  Idea=x.Idea,
                                  Industry=x.Industry,
                                  Business=x.Business,
                                  CirculatingCapital=x.CirculatingCapital,
                                  TotalCapital=x.TotalCapital,
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
            {
                var plateList = (from x in db.t_shares_plate
                                 select x).ToList();
                foreach (var item in list)
                {
                    List<t_shares_plate_rel> tempList = new List<t_shares_plate_rel>();
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //判断是否存在
                            var markettime = (from x in db.t_shares_markettime
                                              where x.Market == item.Market && x.SharesCode == item.SharesCode
                                              select x).FirstOrDefault();
                            if (markettime != null)
                            {
                                if (item.MarketTime > DateTime.Parse("1970-01-01"))
                                {
                                    markettime.MarketTime = item.MarketTime;
                                }
                                if (!string.IsNullOrEmpty(item.Business))
                                {
                                    markettime.Business = item.Business;
                                }
                                if (item.TotalCapital > 0)
                                {
                                    markettime.TotalCapital = item.TotalCapital;
                                }
                                if (item.CirculatingCapital > 0)
                                {
                                    markettime.CirculatingCapital = item.CirculatingCapital;
                                }
                            }
                            else
                            {
                                db.t_shares_markettime.Add(new t_shares_markettime
                                {
                                    SharesCode = item.SharesCode,
                                    CreateTime = DateTime.Now,
                                    SharesName = item.SharesName,
                                    Business = item.Business,
                                    Industry = item.Industry,
                                    Market = item.Market,
                                    MarketTime = item.MarketTime,
                                    CirculatingCapital = item.CirculatingCapital,
                                    TotalCapital = item.TotalCapital
                                });
                            }

                            //删除股票板块关系
                            var plateRel = (from x in db.t_shares_plate_rel
                                            where x.Market == item.Market && x.SharesCode == item.SharesCode
                                            select x).ToList();
                            db.t_shares_plate_rel.RemoveRange(plateRel);
                            db.SaveChanges();

                            //解析行业
                            string[] IndustryArr = item.Industry.Replace(" ", "").Split('/');
                            //解析地区
                            string[] AresArr = item.Area.Replace(" ", "").Split('/');
                            //解析概念
                            string[] IdeaArr = item.Idea.Replace(" ", "").Split('/');
                            var plateListTemp = (from x in plateList
                                                 where ((x.Type == 1 && IndustryArr.Contains(x.Name.Trim())) || (x.Type == 2 && AresArr.Contains(x.Name.Trim())) || (x.Type == 3 && IdeaArr.Contains(x.Name))) && !string.IsNullOrEmpty(x.Name.Trim())
                                                 select x).ToList();
                            foreach (var x in plateListTemp)
                            {
                                tempList.Add(new t_shares_plate_rel
                                {
                                    SharesCode = item.SharesCode,
                                    Market = item.Market,
                                    CreateTime = DateTime.Now,
                                    PlateId = x.Id
                                });
                            }

                            db.t_shares_plate_rel.AddRange(tempList);
                            db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("基础信息导入有错",ex);
                            tran.Rollback();
                        }
                    }
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
                    MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "Update");
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
        /// 获取板块管理列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<SharesPlateInfo> GetSharesPlateList(GetSharesPlateListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format("select PlateId Id,PlateName Name,[Status],CreateTime,count(Market) SharesCount,cast(round(sum((PresentPrice-ClosedPrice)*TotalCapital)*1.0/sum(ClosedPrice*TotalCapital)*10000,0) as int) RiseRate from v_plate_shares group by PlateId,PlateName,PlateType,[Status],CreateTime having PlateType = {0}",request.Type);

                var list=db.Database.SqlQuery<SharesPlateInfo>(sql).ToList();

                if (!string.IsNullOrEmpty(request.Name))
                {
                    list = list.Where(e => e.Name.Contains(request.Name)).ToList();
                }

                int totalCount = list.Count();

                return new PageRes<SharesPlateInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = list.OrderByDescending(e=>e.RiseRate).Skip((request.PageIndex-1)*request.PageSize).Take(request.PageSize).ToList()
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
                    throw new WebApiException(400,"已添加");
                }

                db.t_shares_plate.Add(new t_shares_plate 
                { 
                    Status=1,
                    CreateTime=DateTime.Now,
                    LastModified=DateTime.Now,
                    Name=request.Name,
                    Type=request.Type
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入板块数据
        /// </summary>
        /// <param name="list"></param>
        public int BatchAddSharesPlate(List<string> list,int type)
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
                        Status=1,
                        CreateTime=DateTime.Now,
                        LastModified=DateTime.Now,
                        Name=name,
                        Type=type
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
                    throw new WebApiException(400,"数据不存在");
                }
                //判断名称是否存在
                var temp = (from item in db.t_shares_plate
                             where item.Type == plate.Type && item.Name == request.Name && item.Id!=request.Id
                             select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "已添加");
                }

                plate.Name = request.Name;
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
        /// 删除板块管理
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSharesPlate(DeleteRequest request)
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

                db.t_shares_plate.Remove(plate);
                db.SaveChanges();
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
                var list = from item in db.t_shares_plate_rel
                           join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                           where item.PlateId == request.Id
                           select new { item, item2 };
                int totalCount = list.Count();
                return new PageRes<SharesInfo> 
                {
                    MaxId=0,
                    TotalCount=totalCount,
                    List=(from item in list
                         orderby item.item.CreateTime descending
                         select new SharesInfo
                         {
                             Id=item.item.Id,
                             SharesCode=item.item2.SharesCode,
                             SharesName=item.item2.SharesName,
                             SharesNamePY=item.item2.SharesPyjc,
                             Market=item.item2.Market
                         }).Skip((request.PageIndex-1)*request.PageSize).Take(request.PageSize).ToList()
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
                    throw new WebApiException(400,"板块不存在");
                }
                //判断股票是否存在
                var shares = (from item in db.t_shares_all
                              where item.Market == request.Market && item.SharesCode == request.SharesCode
                              select item).FirstOrDefault();
                if (shares == null)
                {
                    throw new WebApiException(400,"股票不存在");
                }
                //判断是否已添加
                var plateRel = (from item in db.t_shares_plate_rel
                                where item.PlateId == request.PlateId && item.Market == request.Market && item.SharesCode == request.SharesCode
                                select item).FirstOrDefault();
                if (plateRel != null)
                {
                    throw new WebApiException(400,"股票已添加");
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
                    throw new WebApiException(400,"数据集不存在");
                }
                db.t_shares_plate_rel.Remove(plateRel);
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
                                FundMultiple = item.FundMultiple,
                                Priority = item.Priority,
                                Range = item.Range,
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
        public PageRes<SharesTradeRulesOtherInfo> GetSharesTradeRulesOtherList(DetailsPageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rulesOther = from item in db.t_shares_limit_traderules_other
                                 where item.RulesId == request.Id
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
                //判断rulesId是否存在
                var rules = (from item in db.t_shares_limit_traderules
                             where item.Id == request.RulesId
                             select item).FirstOrDefault();
                if (rules == null)
                {
                    throw new WebApiException(400, "交易规则不存在");
                }
                db.t_shares_limit_traderules_other.Add(new t_shares_limit_traderules_other
                {
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
                           join item2 in db.t_shares_all on new { item.Market,item.SharesCode} equals new { item2.Market,item2.SharesCode}
                           select new { item,item2};
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
                                SharesName=item.item2.SharesName
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
                    int resultCount=BatchAddSharesMonitorFun(tempList, db, tran.UnderlyingTransaction);
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

        private int BatchAddSharesMonitorFun(List<MarketTimeInfo> list, meal_ticketEntities db, DbTransaction tran,List<long> trendIdList=null)
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
                                  select new { x2,x3}).ToList();
                    var tempRelPar = (from x in relPar
                                      select new t_shares_monitor_trend_par
                                      {
                                          ParamsInfo = x.x3.ParamsInfo,
                                          TrendId = x.x2.TrendId
                                      }).ToList();
                    trendPar.AddRange(tempRelPar);
                }
                string sql = string.Format("insert into t_shares_monitor ([Market],[SharesCode],[Status],[CreateTime],[LastModified]) values({0},'{1}',1,'{2}','{3}');select @@IDENTITY", item.Market, item.SharesCode, DateTime.Now, DateTime.Now);

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
                                where item.Id == request.Id && item.TrendId==1
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400,"数据不存在");
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
                                TrendId=item.TrendId,
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
            using (var tran=db.Database.BeginTransaction())
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
                                CreateTime=DateTime.Now,
                                LastModified=DateTime.Now,
                                RelId=rel.Id,
                                ParamsInfo=item.ParamsInfo
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
                    throw new WebApiException(400,"数据不存在");
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
                    throw new WebApiException(400,"数据不存在");
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
                    MQHandler.instance.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "TransactionData", "UpdateNew");
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


                    int resultCount=BatchAddSharesMonitorFun(batchList,db,tran.UnderlyingTransaction, request.TrendIdList);

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
                               where item.Type == request.Type
                               select item;
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
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
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
                                where item.Type == request.Type  && item.Name == request.Name
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
                                        Id = item.Id,
                                        Name = item.Name,
                                        ChildList = (from x in db.t_sys_conditiontrade_template_sell_child
                                                     where x.FatherId == item.Id
                                                     select new ConditionChild
                                                     {
                                                         Status = x.Status,
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
                        TemplateId = request.TemplateId
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
                                Name = item.Name,
                                BuyAuto=item.BuyAuto,
                                LimitUp=item.LimitUp,
                                OtherConditionRelative=item.OtherConditionRelative,
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
                        EntrustPriceGear = request.EntrustPriceGear,
                        EntrustType = request.EntrustType,
                        LimitUp=request.LimitUp,
                        ForbidType = request.ForbidType,
                        IsGreater = request.IsGreater,
                        LastModified = DateTime.Now,
                        Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name,
                        ConditionType = request.ConditionPriceType,
                        ConditionPriceBase=request.ConditionPriceBase,
                        ConditionPriceRate=request.ConditionRelativeRate,
                        ConditionPriceType=request.ConditionRelativeType,
                        OtherConditionRelative=request.OtherConditionRelative,
                        TemplateId=request.TemplateId
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
                                TrendName = item.TrendName
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
                    throw new WebApiException(400,"数据不存在");
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
                                TrendName = item.TrendName
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
                    var temp=JsonConvert.DeserializeObject<dynamic>(item.ParamsInfo);
                    if (temp.GroupType == request.GroupType && temp.DataType == request.DataType)
                    {
                        list.Add(new SharesMonitorTrendParInfo 
                        {
                            CreateTime=item.CreateTime,
                            Id=item.Id,
                            ParamsInfo= item.ParamsInfo
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
                if (request.TrendId != 1 && request.TrendId!=7)
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
        #endregion
    }
}
