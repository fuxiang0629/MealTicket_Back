using FXCommon.Common;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace MealTicket_Web_Handler
{
    public class TrendHandler
    {
        /// <summary>
        /// 获取用户保证金余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AccountWalletInfo AccountGetWalletInfo(DetailsRequest request, HeadBase basedata) 
        {
            if (request.Id == 0)
            {
                request.Id = basedata.AccountId;
            }
            using (var db = new meal_ticketEntities())
            {
                var wallet = (from item in db.t_account_wallet
                              where item.AccountId == request.Id
                              select item).FirstOrDefault();
                if (wallet == null)
                {
                    return new AccountWalletInfo
                    {
                        DepositAmount = 0
                    };
                }
                return new AccountWalletInfo
                {
                    DepositAmount = wallet.Deposit / 100 * 100
                };
            }
        }

        /// <summary>
        /// 根据股票代码/名称/简拼获取股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<SharesInfo> GetSharesList(GetSharesListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.SharesInfo == null)
                {
                    request.SharesInfo = "";
                }
                request.SharesInfo = request.SharesInfo.ToLower();
                var sharesList = (from item in db.t_shares_all.AsNoTracking()
                                  join item2 in db.t_shares_monitor on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                  where (item.SharesCode.Contains(request.SharesInfo) || item.SharesPyjc.Contains(request.SharesInfo) || item.SharesName.Contains(request.SharesInfo)) && item.Status == 1 && item2.Status == 1
                                  orderby item.SharesCode
                                  select item).ToList();
                Regex regex0 = new Regex(Singleton.Instance.SharesCodeMatch0);
                Regex regex1 = new Regex(Singleton.Instance.SharesCodeMatch1);
                sharesList = (from item in sharesList
                              where ((regex0.IsMatch(item.SharesCode) && item.Market == 0) || (regex1.IsMatch(item.SharesCode) && item.Market == 1))
                              select item).ToList();
                var resultList = (from item in sharesList
                                  orderby item.SharesCode
                                  select new SharesInfo
                                  {
                                      SharesCode = item.SharesCode,
                                      SharesName = item.SharesName,
                                      Market = item.Market
                                  }).Take(10).ToList();
                return resultList;
            }
        }

        /// <summary>
        /// 用户登入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AccountLoginInfo AccountLogin(AccountLoginRequest request, HeadBase basedata)
        {
            string errorMessage = "";
            long accountId = 0;
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter tokenDb = new ObjectParameter("token", "");
                    ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                    db.P_AccountLogin_Web(request.Mobile, request.LoginPassword.ToMD5(), errorCodeDb, errorMessageDb, tokenDb, accountIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    errorMessage = errorMessageDb.Value.ToString();
                    accountId = (long)accountIdDb.Value;
                    string token = tokenDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    var account = (from item in db.t_account_baseinfo
                                   where item.Id == accountId
                                   select item).FirstOrDefault();
                    if (account == null)
                    {
                        throw new WebApiException(400, "登入失败");
                    }
                    tran.Commit();

                    return new AccountLoginInfo
                    {
                        Token = token,
                        AccountId=accountId,
                        RecommandCode = account.RecommandCode
                    };
                }
                catch (WebApiException ex)
                {
                    errorMessage = ex.Message;
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    try
                    {
                        string sql = string.Format("insert into t_account_loginlog([AccountId],[AccountName],[CreateTime],[DeviceUA],[LoginError],[LoginIp],[LoginoutError],[LoginTime],[LoginType],[LogoutTime]) values({0},'{1}','{2}','{3}','{4}','{5}','','{6}',2,null);", accountId, request.Mobile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), basedata.UserAgent, errorMessage, basedata.Ip, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        Singleton.Instance.sqlHelper.ExecuteNonQuery(sql);
                    }
                    catch (Exception) { }
                }
            }
        }

        /// <summary>
        /// 用户退出登入
        /// </summary>
        public void AccountLogout(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var token = (from item in db.t_account_login_token_web
                             where item.Login_Uuid == basedata.UserToken && item.Status == 1
                             select item).FirstOrDefault();
                if (token != null)
                {
                    token.Status = 2;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 查询自选股列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountOptionalInfo> GetAccountOptionalList(GetAccountOptionalListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //自选股
                var optional = from item in db.t_account_shares_optional.AsNoTracking()
                               join item2 in db.t_shares_all.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                               join item4 in db.t_shares_monitor.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item4.Market, item4.SharesCode }
                               join item3 in db.t_shares_quotes.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into a
                               from ai in a.DefaultIfEmpty()
                               join item5 in db.t_shares_markettime.AsNoTracking() on new { item.Market, item.SharesCode } equals new { item5.Market, item5.SharesCode } into b
                               from bi in b.DefaultIfEmpty()
                               where item.AccountId == basedata.AccountId && item4.Status == 1
                               select new { item, item2, ai, item4, bi };
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    optional = from item in optional
                               where item.item.SharesCode.Contains(request.SharesInfo) || item.item2.SharesName.Contains(request.SharesInfo) || item.item2.SharesPyjc.StartsWith(request.SharesInfo)
                               select item;
                }

                int totalCount = optional.Count();

                var seatList = (from item in db.t_account_shares_seat
                                join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.SeatId
                                where item.AccountId == basedata.AccountId && item.Status == 1
                                select new { item, item2 }).ToList();

                DateTime maxTime = DateTime.Parse("9999-01-01");
                DateTime timeNow = DateTime.Now;
                return new PageRes<AccountOptionalInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in optional.ToList()
                            join item2 in seatList on item.item.Id equals item2.item2.OptionalId into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.item.CreateTime descending
                            let currPrice = item.ai == null ? 0 : item.ai.PresentPrice <= 0 ? item.ai.ClosedPrice : item.ai.PresentPrice
                            select new AccountOptionalInfo
                            {
                                SharesCode = item.item.SharesCode,
                                SharesName = item.item2.SharesName,
                                CreateTime = item.item.CreateTime,
                                CurrPrice = currPrice,
                                Id = item.item.Id,
                                Industry = item.bi == null ? "" : item.bi.Industry,
                                Business = item.bi == null ? "" : item.bi.Business,
                                RisePrice = item.ai == null ? 0 : (currPrice - item.ai.ClosedPrice),
                                RiseRate = (item.ai == null || item.ai.ClosedPrice <= 0) ? 0 : (int)((currPrice - item.ai.ClosedPrice) * 1.0 / item.ai.ClosedPrice * 10000),
                                SeatId = ai == null ? 0 : ai.item.Id,
                                SeatName = ai == null ? "" : ai.item.Name,
                                ValidTime = ai == null ? "" : (ai.item.ValidEndTime >= maxTime ? "永久" : ai.item.ValidEndTime < timeNow ? "已过期" : ("剩余" + (int)(ai.item.ValidEndTime.Date - timeNow.Date).TotalDays) + "天")
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };

            }
        }

        /// <summary>
        /// 添加自选股
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountOptional(AddAccountOptionalRequest request, HeadBase basedata)
        {
            using (SqlConnection conn = new SqlConnection(Singleton.Instance.ConnectionString_meal_ticket))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var command = conn.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.Transaction = tran;

                            //判断自选股是否已存在
                            string sql = string.Format("select top 1 Id from t_account_shares_optional with(xlock) where AccountId={0} and Market={1} and SharesCode='{2}'", basedata.AccountId, request.Market, request.SharesCode);
                            command.CommandText = sql;
                            var result = command.ExecuteScalar();
                            if (result != null)
                            {
                                throw new WebApiException(400, "该自选股已添加");
                            }
                            //查询股票名称
                            sql = string.Format("select top 1 SharesName from t_shares_all with(nolock) where Market={0} and SharesCode='{1}'", request.Market, request.SharesCode);
                            command.CommandText = sql;
                            result = command.ExecuteScalar();
                            if (result == null)
                            {
                                throw new WebApiException(400, "股票不存在");
                            }
                            string SharesName = Convert.ToString(result);
                            //判断是否系统支持股票
                            sql = string.Format("select top 1 Id from t_shares_monitor with(nolock) where [Status]=1 and SharesCode='{0}' and Market={1}", request.SharesCode, request.Market);
                            command.CommandText = sql;
                            var monitorId = command.ExecuteScalar();
                            if (monitorId == null)
                            {
                                throw new WebApiException(400, "不支持的股票");
                            }

                            //添加自选股
                            sql = string.Format("insert into t_account_shares_optional(AccountId,Market,SharesCode,CreateTime) values({0},{1},'{2}','{3}'); select @@IDENTITY;", basedata.AccountId, request.Market, request.SharesCode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                            command.CommandText = sql;
                            long optionalId = Convert.ToInt64(command.ExecuteScalar());
                            //查询走势Id列表
                            sql = string.Format("select Id,TrendId from t_shares_monitor_trend_rel with(nolock) where MonitorId={0} and [Status]=1", Convert.ToInt64(monitorId));
                            command.CommandText = sql;
                            SqlDataReader reader = command.ExecuteReader();
                            List<t_shares_monitor_trend_rel> trendList = new List<t_shares_monitor_trend_rel>();
                            while (reader.Read())
                            {
                                long TrendRelId = Convert.ToInt64(reader["Id"]);
                                long TrendId = Convert.ToInt64(reader["TrendId"]);
                                trendList.Add(new t_shares_monitor_trend_rel
                                {
                                    TrendId = TrendId,
                                    Id = TrendRelId
                                });
                            }
                            reader.Close();
                            //循环添加走势
                            foreach (var trend in trendList)
                            {
                                sql = string.Format("insert into t_account_shares_optional_trend_rel(OptionalId,TrendId,[Status],[CreateTime],[LastModified]) values({0},{1},1,'{2}','{2}'); select @@IDENTITY;", optionalId, trend.TrendId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                command.CommandText = sql;
                                long relId = Convert.ToInt64(command.ExecuteScalar());

                                sql = string.Format("select ParamsInfo from t_shares_monitor_trend_rel_par with(nolock) where RelId={0}", trend.Id);
                                command.CommandText = sql;
                                reader = command.ExecuteReader();
                                List<string> parList = new List<string>();
                                while (reader.Read())
                                {
                                    string ParamsInfo = Convert.ToString(reader["ParamsInfo"]);
                                    parList.Add(ParamsInfo);
                                }
                                reader.Close();

                                sql = "";
                                foreach (string par in parList)
                                {
                                    sql += string.Format("insert into t_account_shares_optional_trend_rel_par(RelId,ParamsInfo,CreateTime,LastModified) values({0},'{1}','{2}','{2}');", relId, par, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                }
                                command.CommandText = sql;
                                command.ExecuteNonQuery();

                                sql = string.Format("insert into t_account_shares_optional_trend_rel_tri(RelId,LastPushTime,LastPushRiseRate,LastPushPrice,TriCountToday,MinPushTimeInterval,MinPushRateInterval,MinTodayPrice,CreateTime,LastModified) values({0},null,null,null,0,{2},{3},-1,'{1}','{1}')", relId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), Singleton.Instance.MinPushTimeInterval, Singleton.Instance.MinPushRateInterval);
                                command.CommandText = sql;
                                command.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 删除自选股
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountOptional(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var optional = (from item in db.t_account_shares_optional
                                    where item.Id == request.Id && item.AccountId == basedata.AccountId
                                    select item).FirstOrDefault();
                    if (optional == null)
                    {
                        throw new WebApiException(400, "自选股不存在");
                    }
                    long optionalId = optional.Id;

                    var optionalTrend = (from item in db.t_account_shares_optional_trend_rel
                                         where item.OptionalId == optionalId
                                         select item).ToList();
                    List<long> relIdList = optionalTrend.Select(e => e.Id).ToList();

                    var trendPar = (from item in db.t_account_shares_optional_trend_rel_par
                                    where relIdList.Contains(item.RelId)
                                    select item).ToList();

                    db.t_account_shares_optional.Remove(optional);
                    if (optionalTrend.Count() > 0)
                    {
                        db.t_account_shares_optional_trend_rel.RemoveRange(optionalTrend);
                    }
                    if (trendPar.Count() > 0)
                    {
                        db.t_account_shares_optional_trend_rel_par.RemoveRange(trendPar);
                    }
                    var seatRel = (from item in db.t_account_shares_optional_seat_rel
                                   where item.OptionalId == optionalId
                                   select item).ToList();
                    db.t_account_shares_optional_seat_rel.RemoveRange(seatRel);
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
        /// 绑定自选股席位
        /// </summary>
        /// <param name="request"></param>
        public void BindAccountOptionalSeat(BindAccountOptionalSeatRequest request, HeadBase basedata)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                //判断自选股是否存在
                var optional = (from item in db.t_account_shares_optional
                                where item.Id == request.OptionalId && item.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "自选股不存在");
                }

                var seatRel = (from item in db.t_account_shares_optional_seat_rel
                               where item.OptionalId == request.OptionalId
                               select item).FirstOrDefault();
                if (request.SeatId != -1)
                {
                    //判断席位是否有效
                    var seat = (from item in db.t_account_shares_seat
                                where item.Id == request.SeatId && item.Status == 1 && item.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                    if (seat == null)
                    {
                        throw new WebApiException(400, "无效的席位");
                    }
                    if (seat.ValidEndTime < timeNow)
                    {
                        if (seatRel == null || seatRel.SeatId != request.SeatId)
                        {
                            throw new WebApiException(400, "席位已过期");
                        }
                    }

                    //判断席位是否已被绑定
                    var rel = (from item in db.t_account_shares_optional_seat_rel
                               where item.SeatId == request.SeatId && item.OptionalId != request.OptionalId
                               select item).FirstOrDefault();
                    if (rel != null)
                    {
                        throw new WebApiException(400, "该席位已被其他股票绑定");
                    }

                    if (seatRel == null)
                    {
                        db.t_account_shares_optional_seat_rel.Add(new t_account_shares_optional_seat_rel
                        {
                            SeatId = request.SeatId,
                            OptionalId = request.OptionalId
                        });
                        db.SaveChanges();
                    }
                    else
                    {
                        seatRel.SeatId = request.SeatId;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (seatRel != null)
                    {
                        db.t_account_shares_optional_seat_rel.Remove(seatRel);
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 查询自选股监控走势
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountOptionalTrendInfo> GetAccountOptionalTrend(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = from item in db.t_account_shares_optional_trend_rel
                                    join item2 in db.t_shares_monitor_trend on item.TrendId equals item2.Id
                                    join item3 in db.t_account_shares_optional on item.OptionalId equals item3.Id
                                    where item2.Status == 1 && item.OptionalId == request.Id && item3.AccountId == basedata.AccountId
                                    select new { item, item2 };
                int totalCount = optionalTrend.Count();

                return new PageRes<AccountOptionalTrendInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in optionalTrend
                            orderby item.item.TrendId
                            select new AccountOptionalTrendInfo
                            {
                                Status = item.item.Status,
                                CreateTime = item.item.LastModified,
                                RelId = item.item.Id,
                                TrendDescription = item.item2.Description,
                                TrendId = item.item.TrendId,
                                TrendName = item.item2.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改自选股监控走势状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalTrendStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = (from item in db.t_account_shares_optional_trend_rel
                                     join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                     where item.Id == request.Id && item2.AccountId == basedata.AccountId
                                     select item).FirstOrDefault();
                if (optionalTrend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                optionalTrend.Status = request.Status;
                optionalTrend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountOptionalTrendParInfo> GetAccountOptionalTrendPar(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_account_shares_optional_trend_rel_par
                               join item2 in db.t_account_shares_optional_trend_rel on item.RelId equals item2.Id
                               join item3 in db.t_account_shares_optional on item2.OptionalId equals item3.Id
                               where item.RelId == request.Id && item3.AccountId == basedata.AccountId
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<AccountOptionalTrendParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new AccountOptionalTrendParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountOptionalTrendPar(AddAccountOptionalTrendParRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var optionalTrend = (from item in db.t_account_shares_optional_trend_rel
                                     join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                     where item.Id == request.RelId && item2.AccountId == basedata.AccountId
                                     select item).FirstOrDefault();
                if (optionalTrend == null)
                {
                    throw new WebApiException(400, "自选股不存在");
                }
                if (optionalTrend.TrendId != 1)
                {
                    var par = (from item in db.t_account_shares_optional_trend_rel_par
                               where item.RelId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_account_shares_optional_trend_rel_par.Add(new t_account_shares_optional_trend_rel_par
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
                    db.t_account_shares_optional_trend_rel_par.Add(new t_account_shares_optional_trend_rel_par
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
        /// 编辑自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalTrendPar(ModifyAccountOptionalTrendParRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_optional_trend_rel_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item2.AccountId == basedata.AccountId && item.Id == trendPar.RelId && item.TrendId == 1
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                trendPar.ParamsInfo = request.ParamsInfo;
                trendPar.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除自选股走势参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountOptionalTrendPar(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_optional_trend_rel_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item2.AccountId == basedata.AccountId && item.Id == trendPar.RelId && item.TrendId == 1
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_optional_trend_rel_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AccountOptionalTrendTriInfo GetAccountOptionalTrendTri(DetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item2.AccountId == basedata.AccountId && item.Id == request.Id
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    return null;
                }

                var relTri = (from item in db.t_account_shares_optional_trend_rel_tri
                              where item.RelId == request.Id
                              select new AccountOptionalTrendTriInfo
                              {
                                  MinPushRateInterval = item.MinPushRateInterval,
                                  MinPushTimeInterval = item.MinPushTimeInterval,
                                  MinTodayPrice = item.MinTodayPrice
                              }).FirstOrDefault();
                return relTri;
            }
        }

        /// <summary>
        /// 设置自选股走势再触发配置
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalTrendTri(ModifyAccountOptionalTrendTriRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var optional = (from item in db.t_account_shares_optional_trend_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item2.AccountId == basedata.AccountId && item.Id == request.RelId
                                select item2).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                var relTri = (from item in db.t_account_shares_optional_trend_rel_tri
                              where item.RelId == request.RelId
                              select item).FirstOrDefault();
                if (relTri == null)
                {
                    db.t_account_shares_optional_trend_rel_tri.Add(new t_account_shares_optional_trend_rel_tri
                    {
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        LastPushPrice = null,
                        LastPushRiseRate = null,
                        LastPushTime = null,
                        MinPushRateInterval = request.MinPushRateInterval,
                        MinPushTimeInterval = request.MinPushTimeInterval,
                        MinTodayPrice = request.MinTodayPrice,
                        RelId = request.RelId
                    });
                }
                else
                {
                    relTri.MinPushRateInterval = request.MinPushRateInterval;
                    relTri.MinPushTimeInterval = request.MinPushTimeInterval;
                    if (request.MinTodayPrice != -1)
                    {
                        relTri.MinTodayPrice = request.MinTodayPrice;
                    }
                    relTri.LastModified = DateTime.Now;
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询用户席位列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountSeatInfo> GetAccountSeatList(GetAccountSeatListRequest request, HeadBase basedata)
        {
            DateTime maxTime = DateTime.Parse("9999-01-01");
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var seat = from item in db.t_account_shares_seat
                           where item.AccountId == basedata.AccountId && item.Status == 1
                           select item;
                if (request.Status != 0)
                {
                    seat = from item in seat
                           where item.ValidEndTime > timeNow
                           select item;
                }

                int totalCount = seat.Count();

                return new PageRes<AccountSeatInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in seat
                            join item2 in db.t_account_shares_optional_seat_rel on item.Id equals item2.SeatId into a
                            from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new AccountSeatInfo
                            {
                                Description = item.Description,
                                Id = item.Id,
                                Name = item.Name,
                                OptionalId = ai == null ? 0 : ai.OptionalId,
                                ValidEndTime = item.ValidEndTime >= maxTime ? "永久" : item.ValidEndTime <= timeNow ? "已过期" : ("剩余" + SqlFunctions.DateDiff("DAY", timeNow, item.ValidEndTime) + "天")
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 编辑用户席位
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountSeat(ModifyAccountSeatRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var seat = (from item in db.t_account_shares_seat
                            where item.AccountId == basedata.AccountId && item.Id == request.Id && item.Status == 1
                            select item).FirstOrDefault();
                if (seat == null)
                {
                    throw new WebApiException(400, "席位不存在");
                }

                seat.Description = request.Description;
                seat.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除用户席位
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountSeat(DeleteRequest request, HeadBase basedata)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var seat = (from item in db.t_account_shares_seat
                            where item.AccountId == basedata.AccountId && item.Id == request.Id && item.Status == 1
                            select item).FirstOrDefault();
                if (seat == null)
                {
                    throw new WebApiException(400, "席位不存在");
                }
                if (seat.ValidEndTime >= timeNow)
                {
                    throw new WebApiException(400, "未过期席位不能删除");
                }

                db.t_account_shares_seat.Remove(seat);


                var seatRel = (from item in db.t_account_shares_optional_seat_rel
                               where item.SeatId == request.Id
                               select item).ToList();
                db.t_account_shares_optional_seat_rel.RemoveRange(seatRel);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询今日行情数据
        /// </summary>
        /// <returns></returns>
        public GetSharesQuotesTodayRes GetSharesQuotesToday()
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var list = (from item in db.t_shares_today
                            join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                            join item3 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into a
                            from ai in a.DefaultIfEmpty()
                            where item.Status == 1
                            orderby item.OrderIndex
                            select new SharesQuotesTodayInfo
                            {
                                SharesName = item2.SharesName,
                                ClosedPrice = ai == null ? 0 : ai.ClosedPrice,
                                PresentPrice = ai == null ? 0 : ai.PresentPrice,
                                DataTime = ai == null ? timeNow : ai.LastModified
                            }).ToList();
                DateTime? dataTime = null;
                if (list.Count() > 0)
                {
                    dataTime = list.Where(e => e.DataTime != timeNow).Max(e => e.DataTime);
                }

                string week = "";
                if (dataTime != null)
                {
                    int dateInt = int.Parse(dataTime.Value.Date.ToString("yyyyMMdd"));
                    week = (from item in db.t_dim_time
                            where item.the_date == dateInt
                            select item.week_day_name).FirstOrDefault();
                }

                return new GetSharesQuotesTodayRes
                {
                    List = list,
                    DataTime = dataTime == null ? null : (dataTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "(" + week + ")")
                };
            }
        }

        /// <summary>
        /// 查询监控股票分组列表
        /// </summary>
        /// <returns></returns>
        public List<AccountOptionalGroupInfo> GetAccountOptionalGroup(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var groupList = (from item in db.t_account_shares_optional_group
                                 where item.AccountId == basedata.AccountId
                                 orderby item.OrderIndex, item.CreateTime descending
                                 select new AccountOptionalGroupInfo
                                 {
                                     GroupId = item.Id,
                                     GroupName = item.Name,
                                     CreateTime = item.CreateTime,
                                     GroupDescription = item.Description,
                                     OrderIndex = item.OrderIndex,
                                     SharesCount = (from x in db.t_account_shares_optional_group_rel
                                                    join x2 in db.t_account_shares_optional on x.OptionalId equals x2.Id
                                                    join x3 in db.t_shares_monitor on new { x2.Market, x2.SharesCode } equals new { x3.Market, x3.SharesCode }
                                                    join x4 in db.t_shares_all on new { x2.Market, x2.SharesCode } equals new { x4.Market, x4.SharesCode }
                                                    where x.GroupId == item.Id && x2.AccountId == basedata.AccountId
                                                    select x2).Count()
                                 }).ToList();
                return groupList;
            }
        }

        /// <summary>
        /// 添加监控股票分组
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountOptionalGroup(AddAccountOptionalGroupRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断名称是否存在
                    var groupInfo = (from item in db.t_account_shares_optional_group
                                     where item.AccountId == basedata.AccountId && item.Name == request.GroupName
                                     select item).FirstOrDefault();
                    if (groupInfo != null)
                    {
                        throw new WebApiException(400, "分组名称已存在");
                    }
                    t_account_shares_optional_group info = new t_account_shares_optional_group
                    {
                        AccountId = basedata.AccountId,
                        CreateTime = DateTime.Now,
                        Description = request.GroupDescription,
                        LastModified = DateTime.Now,
                        Name = request.GroupName,
                        OrderIndex = request.OrderIndex
                    };
                    db.t_account_shares_optional_group.Add(info);
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
        /// 编辑监控股票分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalGroup(ModifyAccountOptionalGroupRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.AccountId == basedata.AccountId && item.Id == request.Id
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }
                //判断名称是否存在
                var groupTemp = (from item in db.t_account_shares_optional_group
                                 where item.AccountId == basedata.AccountId && item.Name == request.GroupName && item.Id != request.Id
                                 select item).FirstOrDefault();
                if (groupTemp != null)
                {
                    throw new WebApiException(400, "分组名称已存在");
                }

                groupInfo.Name = request.GroupName;
                groupInfo.Description = request.GroupDescription;
                groupInfo.OrderIndex = request.OrderIndex;
                groupInfo.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除监控股票分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountOptionalGroup(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.AccountId == basedata.AccountId && item.Id == request.Id
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                db.t_account_shares_optional_group.Remove(groupInfo);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountOptionalGroupRelInfo> GetAccountOptionalGroupRelList(GetAccountOptionalGroupRelListRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var groupShares = from item in db.t_account_shares_optional_group_rel
                                  join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                  join item3 in db.t_shares_monitor on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                                  join item4 in db.t_shares_all on new { item2.Market, item2.SharesCode } equals new { item4.Market, item4.SharesCode }
                                  where item.GroupId == request.Id && item2.AccountId == basedata.AccountId
                                  select new { item, item4 };
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    groupShares = from item in groupShares
                                  where item.item4.SharesName.Contains(request.SharesInfo) || item.item4.SharesCode.Contains(request.SharesInfo) || item.item4.SharesPyjc.StartsWith(request.SharesInfo)
                                  select item;
                }

                int totalCount = groupShares.Count();
                return new PageRes<AccountOptionalGroupRelInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in groupShares
                            orderby item.item.Id descending
                            select new AccountOptionalGroupRelInfo
                            {
                                Id = item.item.Id,
                                SharesCode = item.item4.SharesCode,
                                SharesName = item.item4.SharesName,
                                ValidStartTimeDt = item.item.ValidStartTime,
                                ValidEndTimeDt = item.item.ValidEndTime,
                                GroupIsContinue = item.item.GroupIsContinue
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountOptionalGroupRel(AddAccountOptionalGroupRelRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断自选股是否存在
                var optional = (from item in db.t_account_shares_optional
                                where item.Id == request.OptionalId && item.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "股票不存在");
                }
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.Id == request.GroupId && item.AccountId == basedata.AccountId
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    throw new WebApiException(400, "监控分组不存在");
                }
                //判断是否已经添加
                var groupRel = (from item in db.t_account_shares_optional_group_rel
                                where item.GroupId == request.GroupId && item.OptionalId == request.OptionalId
                                select item).FirstOrDefault();
                if (groupRel != null)
                {
                    throw new WebApiException(400, "股票已添加");
                }

                db.t_account_shares_optional_group_rel.Add(new t_account_shares_optional_group_rel
                {
                    GroupId = request.GroupId,
                    GroupIsContinue = request.GroupIsContinue,
                    OptionalId = request.OptionalId,
                    ValidStartTime = request.ValidStartTime,
                    ValidEndTime = request.ValidEndTime
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除监控分组内股票
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountOptionalGroupRel(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {

                var groupRel = (from item in db.t_account_shares_optional_group_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == request.Id && item2.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                if (groupRel == null)
                {
                    throw new WebApiException(400, "数据不添加");
                }
                db.t_account_shares_optional_group_rel.Remove(groupRel);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改监控分组内股票是否持续
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountOptionalGroupRelGroupIsContinue(ModifyAccountOptionalGroupRelGroupIsContinueRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {

                var groupRel = (from item in db.t_account_shares_optional_group_rel
                                join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                where item.Id == request.Id && item2.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                if (groupRel == null)
                {
                    throw new WebApiException(400, "数据不添加");
                }
                groupRel.GroupIsContinue = request.GroupIsContinue;
                groupRel.ValidStartTime = request.ValidStartTime;
                groupRel.ValidEndTime = request.ValidEndTime;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入监控分组内股票
        /// </summary>
        public int BatchAddAccountOptionalGroupRel(long groupId, bool groupIsContinue, DateTime? ValidStartTime, DateTime? ValidEndTime, List<SharesInfo> sharesList, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var groupInfo = (from item in db.t_account_shares_optional_group
                                 where item.Id == groupId && item.AccountId == basedata.AccountId
                                 select item).FirstOrDefault();
                if (groupInfo == null)
                {
                    return 0;
                }
                int i = 0;
                foreach (var shares in sharesList)
                {
                    //判断自选股是否存在
                    var optional = (from item in db.t_account_shares_optional
                                    where item.Market == shares.Market && item.SharesCode == shares.SharesCode && item.AccountId == basedata.AccountId
                                    select item).FirstOrDefault();
                    if (optional == null)
                    {
                        continue;
                    }
                    //判断是否已经添加
                    var groupRel = (from item in db.t_account_shares_optional_group_rel
                                    where item.GroupId == groupId && item.OptionalId == optional.Id
                                    select item).FirstOrDefault();
                    if (groupRel != null)
                    {
                        continue;
                    }
                    db.t_account_shares_optional_group_rel.Add(new t_account_shares_optional_group_rel
                    {
                        GroupId = groupId,
                        GroupIsContinue = groupIsContinue,
                        OptionalId = optional.Id,
                        ValidStartTime = ValidStartTime,
                        ValidEndTime = ValidEndTime
                    });
                    i++;
                }
                db.SaveChanges();
                return i;
            }
        }

        /// <summary>
        /// 查询今日触发股票数据
        /// </summary>
        /// <returns></returns>
        public List<AccountTrendTriInfo> GetAccountTrendTriList(HeadBase basedata)
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                string sql = string.Format(@"select t.Id OptionalId,t.Market,t.SharesCode,t.SharesName,t.Industry,t.Business,t.PresentPrice,t.ClosedPrice,t.TriPushCount TriCountToday,t.LastPushTime PushTime,t.TrendId,t.RelId,t.LastPushDesc TriDesc
from
(
	select t.Id,t.Market,t.SharesCode,t3.SharesName,t5.Industry,t5.Business,t4.PresentPrice,t7.TriPushCount,t2.LastPushTime,t1.TrendId,
	t1.Id RelId,t2.LastPushDesc,t4.ClosedPrice,
	ROW_NUMBER()OVER(partition by t.Market,t.SharesCode order by t2.LastPushTime desc) num
	from t_account_shares_optional t with(nolock)
	inner join t_account_shares_optional_trend_rel t1 with(nolock) on t.Id=t1.OptionalId
	inner join t_account_shares_optional_trend_rel_tri t2 with(nolock) on t1.Id=t2.RelId
	inner join t_shares_all t3 with(nolock) on t.Market=t3.Market and t.SharesCode=t3.SharesCode
	left join t_shares_quotes t4 with(nolock) on t.Market=t4.Market and t.SharesCode=t4.SharesCode 
	left join t_shares_markettime t5 with(nolock) on t.Market=t5.Market and t.SharesCode=t5.SharesCode 
	inner join t_shares_monitor t6 with(nolock) on t.Market=t6.Market and t.SharesCode=t6.SharesCode 
	inner join t_account_shares_optional_trend_rel_tri_record_statistic t7 with(nolock) on t.Market=t7.Market and t.SharesCode=t7.SharesCode 
	and t.AccountId=t7.AccountId and t7.[Date]='{1}'
	where t.AccountId={0} and t1.[Status]=1 and t2.LastPushTime>'{1}' and t.IsTrendClose=0 
)t
where t.num=1", basedata.AccountId, dateNow.ToString("yyyy-MM-dd"));
                var result = db.Database.SqlQuery<AccountTrendTriInfo>(sql).ToList();
                var groupList = (from x in db.t_account_shares_optional_group_rel
                                 where x.GroupIsContinue || (x.ValidStartTime <= dateNow && x.ValidEndTime >= dateNow)
                                 select x).ToList();

                var accountTrend = (from item in result
                                    join item2 in groupList on item.OptionalId equals item2.OptionalId into a
                                    from ai in a.DefaultIfEmpty()
                                    group new { item, ai } by item into g
                                    orderby g.Key.PushTime descending
                                    select new AccountTrendTriInfo
                                    {
                                        SharesCode = g.Key.SharesCode,
                                        SharesName = g.Key.SharesName,
                                        Business = g.Key.Business,
                                        ClosedPrice = g.Key.ClosedPrice,
                                        GroupList = (from x in g
                                                     where x.ai != null
                                                     select new AccountTrendTriInfoGroup
                                                     {
                                                         GroupId = x.ai.GroupId,
                                                         GroupIsContinue = x.ai.GroupIsContinue,
                                                         ValidStartTime = x.ai.ValidStartTime,
                                                         ValidEndTime = x.ai.ValidEndTime
                                                     }).ToList(),
                                        Industry = g.Key.Industry,
                                        Market = g.Key.Market,
                                        OptionalId = g.Key.OptionalId,
                                        PresentPrice = g.Key.PresentPrice,
                                        PushTime = g.Key.PushTime,
                                        RelId = g.Key.RelId,
                                        TrendId = g.Key.TrendId,
                                        TriCountToday = g.Key.TriCountToday,
                                        TriDesc = g.Key.TriDesc
                                    }).ToList();
                return accountTrend;
            }
        }

        /// <summary>
        /// 查询触发股票详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<AccountTrendTriDetails> GetAccountTrendTriDetails(GetAccountTrendTriDetailsRequest request, HeadBase basedata)
        {
            DateTime dateNow = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(request.Date))
            {
                dateNow = DateTime.Parse(request.Date).Date;
            }
            DateTime dateTomorrow = dateNow.AddDays(1);
            using (var db = new meal_ticketEntities())
            {
                var record = (from item in db.t_account_shares_optional_trend_rel_tri_record
                              where item.AccountId == basedata.AccountId && item.Market == request.Market && item.SharesCode == request.SharesCode && item.IsPush == true && item.CreateTime >= dateNow && item.CreateTime < dateTomorrow
                              orderby item.CreateTime descending
                              select new AccountTrendTriDetails
                              {
                                  SharesCode = item.SharesCode,
                                  SharesName = item.SharesName,
                                  CreateTime = item.CreateTime,
                                  TrendName = item.TrendName,
                                  TrendPrice = item.TriPrice
                              }).ToList();
                return record;
            }
        }

        /// <summary>
        /// 关闭已触发股票
        /// </summary>
        /// <param name="request"></param>
        public void CloseAccountTrendTri(DetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var optional = (from item in db.t_account_shares_optional
                                where item.Id == request.Id && item.AccountId == basedata.AccountId
                                select item).FirstOrDefault();
                if (optional == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                optional.IsTrendClose = true;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询实时监控再触发配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<AccountOptionalTrendTriInfo> GetAccountRealTimeTrendTri(DetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var relTri = (from item in db.t_account_shares_optional
                              join item2 in db.t_account_shares_optional_trend_rel on item.Id equals item2.OptionalId
                              join item3 in db.t_account_shares_optional_trend_rel_tri on item2.Id equals item3.RelId into a
                              from ai in a.DefaultIfEmpty()
                              where item.Id == request.Id
                              orderby item2.TrendId
                              select new AccountOptionalTrendTriInfo
                              {
                                  RelId = item2.Id,
                                  TrendId = item2.TrendId,
                                  MinPushRateInterval = ai == null ? 0 : ai.MinPushRateInterval,
                                  MinPushTimeInterval = ai == null ? 0 : ai.MinPushTimeInterval,
                                  MinTodayPrice = ai == null ? -1 : ai.MinTodayPrice
                              }).ToList();
                return relTri;
            }
        }

        /// <summary>
        /// 设置实时监控再触发配置
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountRealTimeTrendTri(ModifyAccountRealTimeTrendTriRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var x in request.List)
                    {
                        var optional = (from item in db.t_account_shares_optional_trend_rel
                                        join item2 in db.t_account_shares_optional on item.OptionalId equals item2.Id
                                        where item2.AccountId == basedata.AccountId && item.Id == x.RelId
                                        select item2).FirstOrDefault();
                        if (optional == null)
                        {
                            throw new WebApiException(400, "数据不存在");
                        }

                        var relTri = (from item in db.t_account_shares_optional_trend_rel_tri
                                      where item.RelId == x.RelId
                                      select item).FirstOrDefault();
                        if (relTri == null)
                        {
                            db.t_account_shares_optional_trend_rel_tri.Add(new t_account_shares_optional_trend_rel_tri
                            {
                                CreateTime = DateTime.Now,
                                LastModified = DateTime.Now,
                                LastPushPrice = null,
                                LastPushRiseRate = null,
                                LastPushTime = null,
                                MinPushRateInterval = x.MinPushRateInterval,
                                MinPushTimeInterval = x.MinPushTimeInterval,
                                MinTodayPrice = x.MinTodayPrice,
                                RelId = x.RelId
                            });
                        }
                        else
                        {
                            relTri.MinPushRateInterval = x.MinPushRateInterval;
                            relTri.MinPushTimeInterval = x.MinPushTimeInterval;
                            if (x.MinTodayPrice != -1)
                            {
                                relTri.MinTodayPrice = x.MinTodayPrice;
                            }
                            relTri.LastModified = DateTime.Now;
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
        /// 设置监控分组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyAccountRealTimeTrendTriGroup(ModifyAccountRealTimeTrendTriGroupRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var optional = (from item in db.t_account_shares_optional
                                    where item.AccountId == basedata.AccountId && item.Id == request.OptionalId
                                    select item).FirstOrDefault();
                    if (optional == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }

                    var groupList = (from item in db.t_account_shares_optional_group_rel
                                     where item.OptionalId == request.OptionalId
                                     select item).ToList();
                    db.t_account_shares_optional_group_rel.RemoveRange(groupList);
                    db.SaveChanges();


                    foreach (var item in request.GroupList)
                    {
                        db.t_account_shares_optional_group_rel.Add(new t_account_shares_optional_group_rel
                        {
                            GroupId = item.GroupId,
                            GroupIsContinue = item.GroupIsContinue,
                            OptionalId = request.OptionalId,
                            ValidStartTime = item.ValidStartTime,
                            ValidEndTime = item.ValidEndTime
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
        /// 查询监控记录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountRealTimeTrendRecordInfo> GetAccountRealTimeTrendRecord(GetAccountRealTimeTrendRecordRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var record = (from item in db.t_account_shares_optional_trend_rel_tri_record
                              join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                              where item.AccountId == basedata.AccountId && item.IsPush == true
                              select new { item, item2 }).ToList();
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    record = (from item in record
                              where item.item.SharesCode.Contains(request.SharesInfo) || item.item2.SharesName.Contains(request.SharesInfo) || item.item2.SharesPyjc.Contains(request.SharesInfo)
                              select item).ToList();
                }
                if (request.Date != null)
                {
                    DateTime startDate = request.Date.Value.Date;
                    DateTime endDate = startDate.AddDays(1);
                    record = (from item in record
                              where item.item.CreateTime >= startDate && item.item.CreateTime < endDate
                              select item).ToList();
                }

                var result = from item in record
                             group item by new { item.item.Market, item.item.SharesCode, item.item.CreateTime.Date, item.item2.SharesName } into g
                             select g;
                int totalCount = result.Count();

                return new PageRes<AccountRealTimeTrendRecordInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            let lastData = item.OrderByDescending(e => e.item.CreateTime).FirstOrDefault()
                            orderby item.Key.Date descending
                            select new AccountRealTimeTrendRecordInfo
                            {
                                SharesCode = item.Key.SharesCode,
                                SharesName = item.Key.SharesName,
                                LastTime = lastData.item.CreateTime,
                                LastTrendName = lastData.item.TrendName,
                                LastTriPrice = lastData.item.TriPrice,
                                Market = item.Key.Market,
                                Date = item.Key.Date,
                                LastTriDesc = lastData.item.TriDesc,
                                TriCount = item.Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询监控记录详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountRealTimeTrendRecordDetailsInfo> GetAccountRealTimeTrendRecordDetails(GetAccountRealTimeTrendRecordDetailsRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var record = from item in db.t_account_shares_optional_trend_rel_tri_record
                             where item.AccountId == basedata.AccountId && item.Market == request.Market && item.SharesCode == request.SharesCode && item.IsPush == true
                             select item;
                if (request.Date != null)
                {
                    DateTime startDate = request.Date.Value.Date;
                    DateTime endDate = startDate.AddDays(1);
                    record = from item in record
                             where item.CreateTime >= startDate && item.CreateTime < endDate
                             select item;
                }

                int totalCount = record.Count();

                return new PageRes<AccountRealTimeTrendRecordDetailsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in record
                            orderby item.CreateTime descending
                            select new AccountRealTimeTrendRecordDetailsInfo
                            {
                                SharesCode = item.SharesCode,
                                CreateTime = item.CreateTime,
                                TrendName = item.TrendName,
                                TrendPrice = item.TriPrice,
                                TriDesc = item.TriDesc,
                                SharesName = item.SharesName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询商品列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<GoodsInfo> GetGoodsList(GetGoodsListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var goods = (from item in db.t_goods
                             where item.GoodsType == request.GoodsType && item.Status == 1
                             orderby item.OrderIndex
                             select new GoodsInfo
                             {
                                 GoodsDescription = item.GoodsDescription,
                                 GoodsDetails = item.GoodsDetails,
                                 GoodsName = item.GoodsName,
                                 ImgUrl = item.ImgUrl
                             }).ToList();
                return goods;
            }
        }

        /// <summary>
        /// 查询支付方式列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<PaymentInfo> GetPaymentList(GetPaymentListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var payment = (from item in db.t_payment_channel
                               where item.Status == 1 && item.BusinessCode == request.BusinessCode
                               orderby item.OrderIndex
                               select new PaymentInfo
                               {
                                   ChannelCode = item.ChannelCode
                               }).ToList();
                return payment;
            }
        }

        /// <summary>
        /// 查询跟投人
        /// </summary>
        /// <returns></returns>
        public List<FollowAccountInfo> GetFollowAccountList(HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var followList = (from item in db.t_account_follow_rel
                                  join item2 in db.t_account_baseinfo on item.FollowAccountId equals item2.Id
                                  where item.AccountId == basedata.AccountId && item2.Status == 1
                                  orderby item.CreateTime
                                  select new FollowAccountInfo
                                  {
                                      AccountId = item.FollowAccountId,
                                      AccountMobile = item2.Mobile,
                                      AccountName = item2.NickName
                                  }).ToList();
                return followList;
            }
        }

        /// <summary>
        /// 查询用户交易账户信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AccountTradeInfo GetAccountTradeInfo(DetailsRequest request, HeadBase basedata)
        {
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                if (request.Id == 0)
                {
                    request.Id = basedata.AccountId;
                }
                else
                {
                    //判断是否跟投人
                    var follow = (from item in db.t_account_follow_rel
                                  where item.AccountId == basedata.AccountId && item.FollowAccountId == request.Id
                                  select item).FirstOrDefault();
                    if (follow == null)
                    {
                        return new AccountTradeInfo();
                    }
                }
                //剩余保证金
                long RemainDeposit = 0;
                var accountWallet = (from item in db.t_account_wallet
                                     where item.AccountId == request.Id
                                     select item).FirstOrDefault();
                if (accountWallet != null)
                {
                    RemainDeposit = accountWallet.Deposit / 100 * 100;

                }
                //股票持有
                var sharesHold = from item in db.t_account_shares_hold
                                 join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                 where item.AccountId == request.Id
                                 select new { item, item2 };
                var sharesHold_ing = from item in sharesHold
                                     join item2 in db.t_shares_quotes on new { item.item.Market, item.item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                     where item.item.Status == 1 && (item.item.RemainCount > 0 || item.item.LastModified > dateNow)
                                     select new { item, item2 };
                //总市值
                long TotalMarketValue = 0;
                //总成本-卖出金额
                long remainAmount = 0;
                long otherCost = 0;//其他成本

                if (sharesHold_ing.Count() > 0)
                {
                    TotalMarketValue = (sharesHold_ing.Sum(e => e.item.item.RemainCount * (e.item2.PresentPrice <= 0 ? e.item2.ClosedPrice : e.item2.PresentPrice))) / 100 * 100;
                    remainAmount = (sharesHold_ing.Sum(e => e.item.item.BuyTotalAmount - e.item.item.SoldAmount)) / 100 * 100;
                    otherCost = Helper.CalculateOtherCost(sharesHold_ing.Select(e => e.item.item.Id).ToList(), 1);
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
                              join item2 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                              where item.AccountId == request.Id && item.CreateTime >= dateNow && item.Status == 3
                              select new { item, item2 };

                var buyToday = entrust.Where(e => e.item.TradeType == 1).ToList();
                var sellToday = entrust.Where(e => e.item.TradeType == 2).ToList();
                if (buyToday.Count() > 0)
                {
                    todayBuyProfit = (buyToday.Sum(e => ((e.item2.PresentPrice == 0 ? e.item2.ClosedPrice : e.item2.PresentPrice) - e.item.DealPrice) * e.item.DealCount)) / 100 * 100;
                    todayBuyCount = buyToday.Sum(e => e.item.DealCount);
                    otherCost = otherCost + Helper.CalculateOtherCost(buyToday.Select(e => e.item.Id).ToList(), 2);
                }
                if (sellToday.Count() > 0)
                {
                    todaySoldProfit = (sellToday.Sum(e => (e.item.DealPrice - e.item2.ClosedPrice) * e.item.DealCount)) / 100 * 100;
                    otherCost = otherCost + Helper.CalculateOtherCost(sellToday.Select(e => e.item.Id).ToList(), 2);
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
                                  where item.AccountId == request.Id && item.TradeType == 1 && item.Status != 3
                                  select item).ToList();
                if (buyEntrust.Count() > 0)
                {
                    EntrustDeposit = (buyEntrust.Sum(e => e.FreezeDeposit)) / 100 * 100;
                }
                return new AccountTradeInfo
                {
                    RemainDeposit = RemainDeposit,
                    TotalMarketValue = TotalMarketValue,
                    TotalProfit = TotalProfit,
                    TodayProfit = Helper.CheckTodayProfitTime(DateTime.Now) ? TodayProfit : 0,
                    TotalAssets = TotalMarketValue + RemainDeposit + totalDeposit + EntrustDeposit
                };
            }
        }

        /// <summary>
        /// 获取用户交易账户持仓列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountTradeHoldInfo> GetAccountTradeHoldList(DetailsPageRequest request, HeadBase basedata)
        {
            DateTime timeNow = DateTime.Now;
            TimeSpan timeSpanNow = TimeSpan.Parse(timeNow.ToString("HH:mm:ss"));
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                if (request.Id == 0)
                {
                    request.Id = basedata.AccountId;
                }
                else
                {
                    //判断是否跟投人
                    var follow = (from item in db.t_account_follow_rel
                                  where item.AccountId == basedata.AccountId && item.FollowAccountId == request.Id
                                  select item).FirstOrDefault();
                    if (follow == null)
                    {
                        return new PageRes<AccountTradeHoldInfo>();
                    }
                }
                var sharesHold = from item in db.t_account_shares_hold
                                 join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode }
                                 join item3 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode }
                                 where item.AccountId == request.Id && item.Status == 1 && (item.RemainCount > 0 || item.LastModified >= dateNow)
                                 select new { item, item2, item3 };
                int totalCount = sharesHold.Count();

                var tempList = (from item in sharesHold
                                orderby item.item.CreateTime descending
                                select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                List<AccountTradeHoldInfo> list = new List<AccountTradeHoldInfo>();
                foreach (var item in tempList)
                {
                    long ClosedPrice = item.item3.ClosedPrice;
                    long PresentPrice = item.item3.PresentPrice <= 0 ? item.item3.ClosedPrice : item.item3.PresentPrice;
                    long MarketValue = item.item.RemainCount * PresentPrice;
                    long ProfitAmount = MarketValue - (item.item.BuyTotalAmount - item.item.SoldAmount) - Helper.CalculateOtherCost(new List<long> { item.item.Id }, 1);

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
                    list.Add(new AccountTradeHoldInfo
                    {
                        SharesCode = item.item.SharesCode,
                        SharesName = item.item2.SharesName,
                        HoldId = item.item.Id,
                        Market = item.item.Market,
                        FundAmount=item.item.FundAmount/100*100,
                        RemainDeposit = item.item.RemainDeposit / 100 * 100,
                        PresentPrice = PresentPrice,
                        BuyTotalAmount = item.item.BuyTotalAmount,
                        CostPrice = item.item.RemainCount > 0 ? (long)(Math.Round((item.item.BuyTotalAmount - item.item.SoldAmount + Helper.CalculateOtherCost(new List<long> { item.item.Id }, 1)) * 1.0 / item.item.RemainCount, 0)) : (long)(Math.Round((item.item.BuyTotalAmount + Helper.CalculateOtherCost(new List<long> { item.item.Id }, 1)) * 1.0 / item.item.BuyTotalCount, 0)),
                        RemainCount = item.item.RemainCount,
                        MarketValue = MarketValue / 100 * 100,
                        CanSellCount = item.item.CanSoldCount,
                        ProfitAmount = ProfitAmount / 100 * 100,
                        TodayRiseAmount = PresentPrice - ClosedPrice,
                        ClosedPrice = ClosedPrice,
                        ClosingTime = item.item.ClosingTime,
                        ClosingLineList = closingList
                    });
                }

                return new PageRes<AccountTradeHoldInfo>
                {
                    MaxId = 0,
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
        /// 获取用户交易账户委托列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountTradeEntrustInfo> GetAccountTradeEntrustList(GetAccountTradeEntrustListRequest request, HeadBase basedata)
        {
            using (var ts = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                if (request.Id == 0)
                {
                    request.Id = basedata.AccountId;
                }
                else
                {
                    //判断是否跟投人
                    var follow = (from item in db.t_account_follow_rel
                                  where item.AccountId == basedata.AccountId && item.FollowAccountId == request.Id
                                  select item).FirstOrDefault();
                    if (follow == null)
                    {
                        return new PageRes<AccountTradeEntrustInfo>();
                    }
                }
                var entrustList = from item in db.t_account_shares_entrust
                                  where item.AccountId == request.Id
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

                var result = (from item in entrustList
                              orderby item.CreateTime descending
                              select new AccountTradeEntrustInfo
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
                                  SoldType = item.Type,
                                  Deposit = item.FreezeDeposit
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();

                foreach (var item in result)
                {
                    item.SoldProfitAmount = item.SoldProfitAmount - Helper.CalculateOtherCost(new List<long> { item.Id }, 2);
                    item.PresentPrice = 0;
                    var shares = (from x in db.t_shares_all
                                  where x.Market == item.Market && x.SharesCode == item.SharesCode
                                  select x).FirstOrDefault();
                    if (shares != null)
                    {
                        item.SharesName = shares.SharesName;
                        var quotes = (from x in db.t_shares_quotes
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
                ts.Complete();
                return new PageRes<AccountTradeEntrustInfo>
                {
                    TotalCount = totalCount,
                    MaxId = 0,
                    List = result
                };
            }
        }

        /// <summary>
        /// 获取用户交易账户成交记录列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountTradeEntrustDealInfo> GetAccountTradeEntrustDealList(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var dealList = from item in db.t_account_shares_entrust
                               join item2 in db.t_account_shares_entrust_manager on item.Id equals item2.BuyId
                               join item3 in db.t_account_shares_entrust_manager_dealdetails on item2.Id equals item3.EntrustManagerId
                               join item4 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item4.Market, item4.SharesCode }
                               where item.Id == request.Id && item2.DealCount > 0 && ((item2.DealCount >= item4.SharesHandCount && item.TradeType == 1) || item.TradeType == 2)
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

                return new PageRes<AccountTradeEntrustDealInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in dealList
                            orderby item.item3.DealTime descending
                            select new AccountTradeEntrustDealInfo
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
        /// 根据股票代码获取股票列表
        /// </summary>
        /// <param name="request"></param>
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
                    var quotes = (from x in db.t_shares_quotes
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
        /// 获取某只股票五档数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public TradeSharesQuotesInfo GetTradeSharesQuotesInfo(GetTradeSharesQuotesInfoRequest request, HeadBase basedata)
        {
            if (request.AccountId == 0)
            {
                request.AccountId = basedata.AccountId;
            }
            DateTime timeNow = DateTime.Now;
            DateTime dateNow = DateTime.Now.Date;
            using (var db = new meal_ticketEntities())
            {
                var sharesQuotes = (from item in db.t_shares_quotes
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
                                   where item.AccountId == request.AccountId && item.Market == request.Market && item.SharesCode == request.SharesCode && item.Status == 1 && (item.RemainCount > 0 || item.LastModified > dateNow)
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
                    sharesQuotes.ProfitAmount = (accountHold.RemainCount * sharesQuotes.PresentPrice + accountHold.SoldAmount - accountHold.BuyTotalAmount - Helper.CalculateOtherCost(new List<long> { accountHold.Id }, 1)) / 100 * 100;
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
                    sharesQuotes.FundMultiple = rules.FundMultiple;
                    sharesQuotes.TotalRise = isSt ? rules.Range / 2 : rules.Range;
                }
                return sharesQuotes;
            }
        }

        /// <summary>
        /// 股票申请买入
        /// </summary>
        /// <param name="request"></param>
        public void AccountApplyTradeBuy(AccountApplyTradeBuyRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询跟投人员
                var followList = (from item in db.t_account_follow_rel
                                  join item2 in db.t_account_baseinfo on item.FollowAccountId equals item2.Id
                                  join item3 in db.t_account_wallet on item.FollowAccountId equals item3.AccountId
                                  where item.AccountId == basedata.AccountId && item2.Status == 1
                                  select item3).ToList();

                List<dynamic> buyList = new List<dynamic>();
                if (request.AccountId == 0)
                {
                    request.AccountId = basedata.AccountId;
                    buyList.Add(new
                    {
                        AccountId = request.AccountId,
                        BuyAmount = request.BuyAmount
                    });
                    //计算本人购买仓位比
                    var buyRateTemp = (from item in db.t_account_wallet
                                       where item.AccountId == request.AccountId
                                       select item).FirstOrDefault();
                    if (buyRateTemp == null)
                    {
                        throw new WebApiException(400, "账户有误");
                    }
                    var buyRate = request.BuyAmount * 1.0 / buyRateTemp.Deposit;//仓位占比
                    foreach (var account in request.FollowList)
                    {
                        var temp = followList.Where(e => e.AccountId == account).FirstOrDefault();
                        if (temp == null)
                        {
                            continue;
                        }
                        buyList.Add(new
                        {
                            AccountId = account,
                            BuyAmount = (long)(temp.Deposit * buyRate)
                        });
                    }
                }
                else
                {
                    if (followList.Where(e => e.AccountId == request.AccountId).FirstOrDefault() == null)
                    {
                        throw new WebApiException(400, "无权操作");
                    }
                    buyList.Add(new
                    {
                        AccountId = request.AccountId,
                        BuyAmount = request.BuyAmount
                    });
                }

                long mainEntrustId = 0;

                List<dynamic> sendDataList = new List<dynamic>();
                string error = "";
                int i = 0;
                foreach (var buy in buyList)
                {
                    if (buy.BuyAmount <= 0)
                    {
                        continue;
                    }
                    long buyId = 0;
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                            ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                            ObjectParameter buyIdDb = new ObjectParameter("buyId", 0);
                            db.P_ApplyTradeBuy(buy.AccountId, request.Market, request.SharesCode, buy.BuyAmount, request.FundMultiple, request.BuyPrice, null, buy.AccountId == basedata.AccountId ? false : true, errorCodeDb, errorMessageDb, buyIdDb);
                            int errorCode = (int)errorCodeDb.Value;
                            string errorMessage = errorMessageDb.Value.ToString();
                            if (errorCode != 0)
                            {
                                throw new WebApiException(errorCode, errorMessage);
                            }
                            buyId = (long)buyIdDb.Value;

                            var entrust = (from item in db.t_account_shares_entrust
                                           where item.Id == buyId
                                           select item).FirstOrDefault();
                            if (entrust == null)
                            {
                                throw new WebApiException(400, "未知错误");
                            }
                            if (buy.AccountId != basedata.AccountId)//不是自己买，绑定跟买关系
                            {
                                db.t_account_shares_entrust_follow.Add(new t_account_shares_entrust_follow
                                {
                                    CreateTime = DateTime.Now,
                                    MainAccountId = basedata.AccountId,
                                    MainEntrustId = mainEntrustId,
                                    FollowAccountId = buy.AccountId,
                                    FollowEntrustId = entrust.Id,
                                });
                                db.SaveChanges();
                            }
                            else
                            {
                                mainEntrustId = entrust.Id;
                            }

                            sendDataList.Add(new
                            {
                                BuyId = buyId,
                                BuyCount = entrust.EntrustCount,
                                BuyTime = DateTime.Now.ToString("yyyy-MM-dd")
                            });

                            tran.Commit();
                            i++;
                        }
                        catch (Exception ex)
                        {
                            error = ex.Message;
                            Logger.WriteFileLog("购买失败", ex);
                            tran.Rollback();
                        }
                    }

                    if (request.AutoDetailsId>0 && i==1 && buyId>0)
                    {
                        var details = (from x in db.t_account_shares_conditiontrade_buy_details
                                       where x.Id == request.AutoDetailsId
                                       select x).FirstOrDefault();
                        if (details != null)
                        {
                            details.BusinessStatus = 4;
                            details.EntrustId = buyId;
                            db.SaveChanges();
                        }
                    }
                }
                if (sendDataList.Count() <= 0)
                {
                    throw new WebApiException(400, error);
                }
                bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { type = 1, data = sendDataList })), "SharesBuy", "s1");
                if (!isSendSuccess)
                {
                    throw new WebApiException(400, "买入失败,请撤销重试");
                }
            }
        }

        /// <summary>
        /// 股票申请卖出
        /// </summary>
        /// <param name="request"></param>
        public void AccountApplyTradeSell(AccountApplyTradeSellRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询跟投人员
                var followList = (from item in db.t_account_follow_rel
                                  join item2 in db.t_account_baseinfo on item.FollowAccountId equals item2.Id
                                  join item3 in db.t_account_wallet on item.FollowAccountId equals item3.AccountId
                                  where item.AccountId == basedata.AccountId && item2.Status == 1
                                  select item3).ToList();
                //查询持仓
                var hold = (from item in db.t_account_shares_hold
                            where item.Id == request.HoldId
                            select item).FirstOrDefault();
                if (hold == null)
                {
                    throw new WebApiException(400, "持仓不存在");
                }
                List<dynamic> sellList = new List<dynamic>();
                if (hold.AccountId == basedata.AccountId)//自己卖
                {
                    sellList.Add(new
                    {
                        AccountId = hold.AccountId,
                        HoldId = hold.Id,
                        SellCount = request.SellCount,
                    });
                    //计算本人卖出仓位比
                    var sellRate = request.SellCount * 1.0 / hold.CanSoldCount;
                    foreach (var account in request.FollowList)
                    {
                        var temp = followList.Where(e => e.AccountId == account).FirstOrDefault();
                        if (temp == null)
                        {
                            continue;
                        }
                        var followHold = (from item in db.t_account_shares_hold
                                          where item.AccountId == account && item.SharesCode == hold.SharesCode && item.Market == hold.Market && item.Status == 1 && item.CanSoldCount > 0
                                          select item).FirstOrDefault();
                        if (followHold == null)
                        {
                            continue;
                        }
                        int sellCount = (int)(followHold.CanSoldCount * sellRate);
                        sellCount = sellCount / 100 * 100;
                        sellList.Add(new
                        {
                            AccountId = account,
                            HoldId = followHold.Id,
                            SellCount = sellCount
                        });
                    }
                }
                else//给跟投人卖
                {
                    if (followList.Where(e => e.AccountId == hold.AccountId).FirstOrDefault() == null)
                    {
                        throw new WebApiException(400, "无权操作");
                    }
                    sellList.Add(new
                    {
                        AccountId = hold.AccountId,
                        HoldId = hold.Id,
                        SellCount = request.SellCount,
                    });
                }

                long mainEntrustId = 0;

                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        Dictionary<string, List<dynamic>> sendDataDic = new Dictionary<string, List<dynamic>>();
                        foreach (var sell in sellList)
                        {
                            if (sell.SellCount <= 0)
                            {
                                continue;
                            }
                            ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                            ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                            ObjectParameter sellIdDb = new ObjectParameter("sellId", 0);
                            db.P_ApplyTradeSell(sell.AccountId, sell.HoldId, sell.SellCount, request.SellType, request.SellPrice, 0, sell.AccountId == basedata.AccountId ? false : true, errorCodeDb, errorMessageDb, sellIdDb);
                            int errorCode = (int)errorCodeDb.Value;
                            string errorMessage = errorMessageDb.Value.ToString();
                            if (errorCode != 0)
                            {
                                throw new WebApiException(errorCode, errorMessage);
                            }
                            long sellId = (long)sellIdDb.Value;

                            if (sell.AccountId != basedata.AccountId)//不是自己卖，绑定跟卖关系
                            {
                                db.t_account_shares_entrust_follow.Add(new t_account_shares_entrust_follow
                                {
                                    CreateTime = DateTime.Now,
                                    MainAccountId = basedata.AccountId,
                                    MainEntrustId = mainEntrustId,
                                    FollowAccountId = sell.AccountId,
                                    FollowEntrustId = sellId,
                                });
                                db.SaveChanges();
                            }
                            else
                            {
                                mainEntrustId = sellId;
                            }


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
                                var server = (from x in db.t_server_broker_account_rel
                                              join x2 in db.t_server on x.ServerId equals x2.ServerId
                                              where x.BrokerAccountId == item.item2.Id
                                              select x).FirstOrDefault();
                                if (server == null)
                                {
                                    throw new WebApiException(400, "服务器配置有误");
                                }
                                if (sendDataDic.ContainsKey(server.ServerId))
                                {
                                    sendDataDic[server.ServerId].Add(new
                                    {
                                        SellManagerId = item.item.Id,
                                        SellTime = DateTime.Now.ToString("yyyy-MM-dd")
                                    });
                                }
                                else
                                {
                                    sendDataDic.Add(server.ServerId, new List<dynamic>
                                    {
                                        new
                                        {
                                            SellManagerId = item.item.Id,
                                            SellTime = DateTime.Now.ToString("yyyy-MM-dd")
                                        }
                                    });
                                }
                            }
                        }

                        foreach (var item in sendDataDic)
                        {
                            Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(new { type = 1, data = item.Value })), "SharesSell", item.Key);
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
        }

        /// <summary>
        /// 股票申请撤单
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AccountApplyTradeCancel(AccountApplyTradeCancelRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询跟投人员
                var followList = (from item in db.t_account_follow_rel
                                  join item2 in db.t_account_baseinfo on item.FollowAccountId equals item2.Id
                                  join item3 in db.t_account_wallet on item.FollowAccountId equals item3.AccountId
                                  where item.AccountId == basedata.AccountId && item2.Status == 1
                                  select item3).ToList();
                //查询委托
                var entrust = (from item in db.t_account_shares_entrust
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (entrust == null)
                {
                    throw new WebApiException(400, "委托不存在");
                }
                List<dynamic> cancelList = new List<dynamic>();
                if (entrust.AccountId == basedata.AccountId)//自己撤销
                {
                    cancelList.Add(new
                    {
                        AccountId = entrust.AccountId,
                        EntrustId = entrust.Id
                    });
                    foreach (var account in request.FollowList)
                    {
                        var temp = followList.Where(e => e.AccountId == account).FirstOrDefault();
                        if (temp == null)
                        {
                            continue;
                        }
                        var followEntrust = (from item in db.t_account_shares_entrust_follow
                                             where item.MainAccountId == entrust.AccountId && item.MainEntrustId == entrust.Id && item.FollowAccountId == account
                                             select item
                                            ).FirstOrDefault();
                        if (followEntrust == null)
                        {
                            continue;
                        }
                        cancelList.Add(new
                        {
                            AccountId = account,
                            EntrustId = followEntrust.FollowEntrustId
                        });
                    }
                }
                else//给跟投人撤销
                {
                    if (followList.Where(e => e.AccountId == entrust.AccountId).FirstOrDefault() == null)
                    {
                        throw new WebApiException(400, "无权操作");
                    }
                    cancelList.Add(new
                    {
                        AccountId = entrust.AccountId,
                        EntrustId = entrust.Id
                    });
                }

                foreach (var cancel in cancelList)
                {
                    long accountId = cancel.AccountId;
                    long entrustId = cancel.EntrustId;
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                            ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                            db.P_ApplyTradeCancel(accountId, entrustId, errorCodeDb, errorMessageDb);
                            int errorCode = (int)errorCodeDb.Value;
                            string errorMessage = errorMessageDb.Value.ToString();
                            if (errorCode != 0)
                            {
                                throw new WebApiException(errorCode, errorMessage);
                            }

                            var entrustManager = (from item in db.t_account_shares_entrust_manager
                                                  join item2 in db.t_broker_account_info on item.TradeAccountCode equals item2.AccountCode
                                                  where item.BuyId == entrustId
                                                  select new { item, item2 }).ToList();
                            if (entrustManager.Count() <= 0)
                            {
                                var sendData = new
                                {
                                    TradeManagerId = -1,
                                    EntrustId = entrustId,
                                    CancelTime = DateTime.Now.ToString("yyyy-MM-dd")
                                };
                                bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", "s1");
                            }
                            else
                            {
                                foreach (var item in entrustManager)
                                {
                                    var sendData = new
                                    {
                                        TradeManagerId = item.item.Id,
                                        EntrustId = entrustId,
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

                                    bool isSendSuccess = Singleton.Instance.mqHandler.SendMessage(Encoding.GetEncoding("utf-8").GetBytes(JsonConvert.SerializeObject(sendData)), "SharesCancel", server.ServerId);
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
                            if (accountId == basedata.AccountId)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 补交保证金
        /// </summary>
        public void AccountMakeupDeposit(AccountMakeupDepositRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                //查询跟投人员
                var followList = (from item in db.t_account_follow_rel
                                  join item2 in db.t_account_baseinfo on item.FollowAccountId equals item2.Id
                                  join item3 in db.t_account_wallet on item.FollowAccountId equals item3.AccountId
                                  where item.AccountId == basedata.AccountId && item2.Status == 1
                                  select item3).ToList();
                //查询持仓
                var hold = (from item in db.t_account_shares_hold
                            where item.Id == request.HoldId
                            select item).FirstOrDefault();
                if (hold == null)
                {
                    throw new WebApiException(400, "持仓不存在");
                }
                List<dynamic> depositList = new List<dynamic>();
                if (hold.AccountId == basedata.AccountId)//自己卖
                {
                    depositList.Add(new
                    {
                        AccountId = hold.AccountId,
                        HoldId = hold.Id,
                        Deposit = request.Deposit,
                    });
                    var rate = request.Deposit * 1.0 / hold.RemainDeposit;//仓位占比
                    foreach (var account in request.FollowList)
                    {
                        var temp = followList.Where(e => e.AccountId == account).FirstOrDefault();
                        if (temp == null)
                        {
                            continue;
                        }
                        var followHold = (from item in db.t_account_shares_hold
                                          where item.AccountId == account && item.SharesCode == hold.SharesCode && item.Market == hold.Market && item.Status == 1 && item.CanSoldCount > 0
                                          select item).FirstOrDefault();
                        if (followHold == null)
                        {
                            continue;
                        }
                        depositList.Add(new
                        {
                            AccountId = account,
                            HoldId = followHold.Id,
                            Deposit = (long)(followHold.RemainDeposit * rate)
                        });
                    }
                }
                else//给跟投人追加
                {
                    if (followList.Where(e => e.AccountId == hold.AccountId).FirstOrDefault() == null)
                    {
                        throw new WebApiException(400, "无权操作");
                    }
                    depositList.Add(new
                    {
                        AccountId = hold.AccountId,
                        HoldId = hold.Id,
                        Deposit = request.Deposit,
                    });
                }
                foreach (var deposit in depositList)
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                            ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                            db.P_MakeupDeposit(deposit.AccountId, deposit.HoldId, deposit.Deposit, errorCodeDb, errorMessageDb);
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
                            if (deposit.AccountId == basedata.AccountId)
                            {
                                throw ex;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取跟投记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountFollowRecordInfo> GetAccountFollowRecord(GetAccountFollowRecordRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var entrstList = from item in db.t_account_shares_entrust
                                 join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id
                                 where item.AccountId == basedata.AccountId
                                 select new { item, item2 };
                if (request.Type == 1)
                {
                    entrstList = from item in entrstList
                                 where item.item.HoldId == request.Id
                                 select item;
                }
                else if (request.Type == 2)
                {
                    entrstList = from item in entrstList
                                 where item.item.Id == request.Id
                                 select item;
                }
                else
                {
                    throw new WebApiException(400, "参数错误");
                }

                var followList = from item in db.t_account_shares_entrust_follow
                                 join item2 in db.t_account_shares_entrust on item.FollowEntrustId equals item2.Id
                                 join item3 in db.t_account_baseinfo on item.FollowAccountId equals item3.Id
                                 where item.MainAccountId == basedata.AccountId
                                 select new { item, item2, item3 };

                var result = from item in entrstList
                             join item2 in followList on item.item.Id equals item2.item.MainEntrustId into a
                             from ai in a.DefaultIfEmpty()
                             group new { item, ai } by item into g
                             select g;
                int totalCount = result.Count();
                var resultList = (from item in result
                                  orderby item.Key.item.CreateTime descending
                                  select item).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();


                return new PageRes<AccountFollowRecordInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in resultList
                            orderby item.Key.item.CreateTime descending
                            select new AccountFollowRecordInfo
                            {
                                CreateTime = item.Key.item.CreateTime,
                                Status = item.Key.item.Status,
                                EntrustId = item.Key.item.Id,
                                AccountInfo = item.Key.item2.Mobile + (string.IsNullOrEmpty(item.Key.item2.NickName) ? "" : "(" + item.Key.item2.NickName + ")"),
                                DealCount = item.Key.item.DealCount,
                                EntrustCount = item.Key.item.EntrustCount,
                                EntrustPrice = item.Key.item.EntrustPrice,
                                EntrustType = item.Key.item.EntrustType,
                                TradeType = item.Key.item.TradeType,
                                IsFollow = false,
                                FollowList = (from x in item
                                              where x.ai != null
                                              orderby x.ai.item2.CreateTime descending
                                              select new AccountFollowRecordInfo
                                              {
                                                  EntrustId = x.ai.item2.Id,
                                                  AccountInfo = x.ai.item3.Mobile + (string.IsNullOrEmpty(x.ai.item3.NickName) ? "" : "(" + x.ai.item3.NickName + ")"),
                                                  DealCount = x.ai.item2.DealCount,
                                                  EntrustCount = x.ai.item2.EntrustCount,
                                                  EntrustPrice = x.ai.item2.EntrustPrice,
                                                  EntrustType = x.ai.item2.EntrustType,
                                                  TradeType = x.ai.item2.TradeType,
                                                  IsFollow = true,
                                                  CreateTime = x.ai.item2.CreateTime,
                                              }).ToList()
                            }).ToList()
                };
            }
        }

        /// <summary>
        /// 获取条件单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<AccountHoldConditionTradeInfo> GetAccountHoldConditionTradeList(GetAccountHoldConditionTradeListRequest request)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var conditiontrade = (from item in db.t_account_shares_hold_conditiontrade.AsNoTracking()
                                      where item.HoldId == request.HoldId && item.Type == request.Type && item.SourceFrom == 1
                                      select new AccountHoldConditionTradeInfo
                                      {
                                          ConditionPrice = item.ConditionPrice,
                                          ConditionTime = item.ConditionTime,
                                          EntrustCount = item.EntrustCount,
                                          EntrustPriceGear = item.EntrustPriceGear,
                                          EntrustType = item.EntrustType,
                                          Id = item.Id,
                                          TriggerTime = item.TriggerTime,
                                          Status = item.Status,
                                          ForbidType = item.ForbidType,
                                          Name = item.Name,
                                          ConditionRelativeRate=item.ConditionRelativeRate,
                                          ConditionRelativeType=item.ConditionRelativeType,
                                          ConditionType=item.ConditionType,
                                          EntrustId = item.EntrustId,
                                          ChildList = (from x in db.t_account_shares_hold_conditiontrade_child
                                                       where x.ConditionId == item.Id
                                                       select new ConditionChild
                                                       {
                                                           Status = x.Status,
                                                           ChildId = x.ChildId
                                                       }).ToList()
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
        public void AddAccountHoldConditionTrade(AddAccountHoldConditionTradeRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //判断持仓是否存在
                    var hold = (from item in db.t_account_shares_hold
                                where item.Id == request.HoldId
                                select item).FirstOrDefault();
                    if (hold == null)
                    {
                        throw new WebApiException(400, "持仓不存在");
                    }
                    var temp = new t_account_shares_hold_conditiontrade
                    {
                        AccountId = hold.AccountId,
                        ConditionPrice = request.ConditionPrice,
                        ConditionTime = request.ConditionTime,
                        CreateTime = DateTime.Now,
                        EntrustCount = request.EntrustCount,
                        EntrustPriceGear = request.EntrustPriceGear,
                        EntrustType = request.EntrustType,
                        HoldId = request.HoldId,
                        LastModified = DateTime.Now,
                        TradeType = request.TradeType,
                        ConditionRelativeRate=request.ConditionRelativeRate,
                        ConditionRelativeType=request.ConditionRelativeType,
                        ConditionType=request.ConditionType,
                        SourceFrom = 1,
                        Status = 2,
                        FatherId = 0,
                        EntrustId = 0,
                        ForbidType = request.ForbidType,
                        Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name,
                        TriggerTime = null,
                        Type = request.Type
                    };
                    db.t_account_shares_hold_conditiontrade.Add(temp);
                    db.SaveChanges();

                    int i = 0;
                    foreach (var item in request.ChildList)
                    {
                        if (item.ChildId > 0)
                        {
                            db.t_account_shares_hold_conditiontrade_child.Add(new t_account_shares_hold_conditiontrade_child
                            {
                                Status = item.Status,
                                ChildId = item.ChildId,
                                ConditionId = temp.Id,
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
        /// 编辑条件单
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountHoldConditionTrade(ModifyAccountHoldConditionTradeRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var conditiontrade = (from item in db.t_account_shares_hold_conditiontrade
                                          where item.Id == request.Id
                                          select item).FirstOrDefault();
                    if (conditiontrade == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    if (conditiontrade.TriggerTime != null)
                    {
                        throw new WebApiException(400, "已执行无法修改");
                    }
                    conditiontrade.EntrustCount = request.EntrustCount;
                    conditiontrade.EntrustPriceGear = request.EntrustPriceGear;
                    conditiontrade.EntrustType = request.EntrustType;
                    conditiontrade.ForbidType = request.ForbidType;
                    conditiontrade.LastModified = DateTime.Now;
                    conditiontrade.Name = request.Name;
                    conditiontrade.ConditionType = request.ConditionType;
                    conditiontrade.ConditionRelativeRate = request.ConditionRelativeRate;
                    conditiontrade.ConditionRelativeType = request.ConditionRelativeType;
                    conditiontrade.ConditionTime = request.ConditionTime;
                    conditiontrade.ConditionPrice = request.ConditionPrice;
                    db.SaveChanges();

                    var child = (from item in db.t_account_shares_hold_conditiontrade_child
                                 where item.ConditionId == request.Id
                                 select item).ToList();
                    if (child.Count() > 0)
                    {
                        db.t_account_shares_hold_conditiontrade_child.RemoveRange(child);
                        db.SaveChanges();
                    }

                    int i = 0;
                    foreach (var item in request.ChildList)
                    {
                        if (item.ChildId > 0)
                        {
                            db.t_account_shares_hold_conditiontrade_child.Add(new t_account_shares_hold_conditiontrade_child
                            {
                                Status = item.Status,
                                ChildId = item.ChildId,
                                ConditionId = request.Id,
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
        /// 修改触发条件
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountHoldConditionTradeStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var ConditionTrade = (from item in db.t_account_shares_hold_conditiontrade
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                if (ConditionTrade == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (ConditionTrade.TriggerTime != null)
                {
                    throw new WebApiException(400, "已执行无法编辑");
                }
                ConditionTrade.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除条件单
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteAccountHoldConditionTrade(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var ConditionTrade = (from item in db.t_account_shares_hold_conditiontrade
                                      where item.Id == request.Id
                                      select item).FirstOrDefault();
                if (ConditionTrade == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (ConditionTrade.TriggerTime != null)
                {
                    throw new WebApiException(400, "已执行无法删除");
                }
                db.t_account_shares_hold_conditiontrade.Remove(ConditionTrade);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取买入条件单股票列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionTradeSharesInfo> GetAccountBuyConditionTradeSharesList(GetAccountBuyConditionTradeSharesListRequest request, HeadBase basedata)
        {
            if (request.Id == 0)
            {
                request.Id = basedata.AccountId;
            }
            using (var db = new meal_ticketEntities())
            {
                var result = from item in db.t_account_shares_conditiontrade_buy
                             join item2 in db.t_shares_all on new { item.Market, item.SharesCode } equals new { item2.Market, item2.SharesCode } into a
                             from ai in a.DefaultIfEmpty()
                             join item3 in db.t_shares_quotes on new { item.Market, item.SharesCode } equals new { item3.Market, item3.SharesCode } into b
                             from bi in b.DefaultIfEmpty()
                             where item.AccountId == request.Id
                             select new { item, ai, bi };
                if (!string.IsNullOrEmpty(request.SharesInfo))
                {
                    result = from item in result
                             where item.ai != null && (item.ai.SharesCode.Contains(request.SharesInfo) || item.ai.SharesName.Contains(request.SharesInfo) || item.ai.SharesPyjc.StartsWith(request.SharesInfo))
                             select item;
                }
                int totalCount = result.Count();

                return new PageRes<AccountBuyConditionTradeSharesInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in result
                            orderby item.item.SharesCode
                            let currPrice = item.bi == null ? 0 : item.bi.PresentPrice
                            select new AccountBuyConditionTradeSharesInfo
                            {
                                SharesCode = item.item.SharesCode,
                                SharesName = item.ai == null ? "" : item.ai.SharesName,
                                Status = item.item.Status,
                                CreateTime = item.item.CreateTime,
                                CurrPrice = currPrice,
                                ClosedPrice=item.bi==null?0:item.bi.ClosedPrice,
                                Id = item.item.Id,
                                Market = item.item.Market,
                                RisePrice = item.bi == null ? 0 : (currPrice - item.bi.ClosedPrice),
                                RiseRate = (item.bi == null || item.bi.ClosedPrice <= 0) ? 0 : (int)((currPrice - item.bi.ClosedPrice) * 1.0 / item.bi.ClosedPrice * 10000)
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加买入条件单股票
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountBuyConditionTradeShares(AddAccountBuyConditionTradeSharesRequest request, HeadBase basedata)
        {
            if (request.AccountId == 0)
            {
                request.AccountId = basedata.AccountId;
            }
            using (var db = new meal_ticketEntities())
            {
                //判断股票是否已添加
                var conditiontrade_buy = (from item in db.t_account_shares_conditiontrade_buy
                                          where item.AccountId == request.AccountId && item.Market == request.Market && item.SharesCode == request.SharesCode
                                          select item).FirstOrDefault();
                if (conditiontrade_buy != null)
                {
                    throw new WebApiException(400, "股票已添加");
                }
                db.t_account_shares_conditiontrade_buy.Add(new t_account_shares_conditiontrade_buy
                {
                    SharesCode = request.SharesCode,
                    Status = 1,
                    AccountId = request.AccountId,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Market = request.Market
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 批量导入行情监控数据
        /// </summary>
        /// <param name="list"></param>
        public int BatchAddAccountBuyConditionTradeShares(List<SharesInfo> list, long accountId)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断是否存在
                var conditiontrade_buy = (from x in db.t_account_shares_conditiontrade_buy
                                          where x.AccountId == accountId
                                          select x).ToList();
                int resultCount = 0;
                foreach (var item in list)
                {
                    if (conditiontrade_buy.Where(e => e.Market == item.Market && e.SharesCode == item.SharesCode).FirstOrDefault() != null)
                    {
                        continue;
                    }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            db.t_account_shares_conditiontrade_buy.Add(new t_account_shares_conditiontrade_buy 
                            { 
                                SharesCode=item.SharesCode,
                                AccountId=accountId,
                                CreateTime=DateTime.Now,
                                LastModified=DateTime.Now,
                                Market=item.Market,
                                Status=1
                            });
                            db.SaveChanges();
                            resultCount++;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                }
                return resultCount;
            }
        }

        /// <summary>
        /// 删除买入条件单股票
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountBuyConditionTradeShares(DeleteRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var result = (from item in db.t_account_shares_conditiontrade_buy
                                  where item.Id == request.Id
                                  select item).FirstOrDefault();
                    if (result == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    db.t_account_shares_conditiontrade_buy.Remove(result);
                    db.SaveChanges();

                    //删除条件
                    string sql = @"delete t_account_shares_conditiontrade_buy_details where ConditionId={0}";
                    db.Database.ExecuteSqlCommand(string.Format(sql, request.Id));

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
        /// 获取股票买入条件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionInfo> GetAccountBuyConditionList(DetailsPageRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var BuyConditionList = from item in db.t_account_shares_conditiontrade_buy_details
                                       where item.ConditionId == request.Id
                                       select item;
                int totalCount = BuyConditionList.Count();

                return new PageRes<AccountBuyConditionInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in BuyConditionList
                            join item2 in db.t_account_shares_entrust on item.EntrustId equals item2.Id into a from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime
                            select new AccountBuyConditionInfo
                            {
                                BuyAuto = item.BuyAuto,
                                Status = item.Status,
                                ConditionRelativeRate = item.ConditionRelativeRate,
                                ConditionRelativeType = item.ConditionRelativeType,
                                ConditionType = item.ConditionType,
                                EntrustId = item.EntrustId,
                                EntrustPrice = item.ConditionPrice,
                                Id = item.Id,
                                IsGreater = item.IsGreater,
                                EntrustType = item.EntrustType,
                                ForbidType = item.ForbidType,
                                ConditionPrice = item.ConditionPrice,
                                TriggerTime = item.TriggerTime,
                                EntrustPriceGear = item.EntrustPriceGear,
                                EntrustAmount = item.EntrustAmount,
                                Name = item.Name,
                                BusinessStatus = item.BusinessStatus,
                                EntrustStatus = ai == null ? 0 : ai.Status,
                                RelDealCount = ai == null ? 0 : ai.DealCount,
                                RelEntrustCount = ai == null ? 0 : ai.EntrustCount,
                                OtherConditionCount = (from x in db.t_account_shares_conditiontrade_buy_details_other
                                                       where x.DetailsId == item.Id
                                                       select x).Count(),
                                AutoConditionCount = (from x in db.t_account_shares_conditiontrade_buy_details_auto
                                                      where x.DetailsId == item.Id
                                                      select x).Count(),
                                FollowAccountList = (from x in db.t_account_shares_conditiontrade_buy_details_follow
                                                     where x.DetailsId == item.Id
                                                     select x.FollowAccountId).ToList()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票买入条件
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountBuyCondition(AddAccountBuyConditionRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    //判断股票是否存在
                    var conditiontrade_buy = (from item in db.t_account_shares_conditiontrade_buy
                                              where item.Id == request.ConditionId
                                              select item).FirstOrDefault();
                    if (conditiontrade_buy == null)
                    {
                        throw new WebApiException(400, "设定的股票不存在");
                    }
                    t_account_shares_conditiontrade_buy_details temp = new t_account_shares_conditiontrade_buy_details
                    {
                        SourceFrom = 1,
                        Status = 2,
                        ConditionId = request.ConditionId,
                        BuyAuto = request.BuyAuto,
                        ConditionPrice = request.ConditionPrice,
                        CreateTime = DateTime.Now,
                        EntrustAmount = request.EntrustAmount,
                        EntrustId = 0,
                        EntrustPriceGear = request.EntrustPriceGear,
                        EntrustType = request.EntrustType,
                        ForbidType = request.ForbidType,
                        IsGreater = request.IsGreater,
                        LastModified = DateTime.Now,
                        TriggerTime = null,
                        Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name,
                        ConditionRelativeRate = request.ConditionRelativeRate,
                        ConditionRelativeType = request.ConditionRelativeType,
                        ConditionType = request.ConditionType
                    };
                    db.t_account_shares_conditiontrade_buy_details.Add(temp);
                    db.SaveChanges();

                    foreach (var followId in request.FollowAccountList)
                    {
                        db.t_account_shares_conditiontrade_buy_details_follow.Add(new t_account_shares_conditiontrade_buy_details_follow 
                        {
                            CreateTime=DateTime.Now,
                            FollowAccountId=followId,
                            DetailsId= temp.Id
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
        /// 编辑股票买入条件
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyCondition(ModifyAccountBuyConditionRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var buy_details = (from item in db.t_account_shares_conditiontrade_buy_details
                                       where item.Id == request.Id
                                       select item).FirstOrDefault();
                    if (buy_details == null)
                    {
                        throw new WebApiException(400, "数据不存在");
                    }
                    if (buy_details.TriggerTime != null)
                    {
                        throw new WebApiException(400, "已执行无法编辑");
                    }
                    buy_details.BuyAuto = request.BuyAuto;
                    buy_details.ConditionPrice = request.ConditionPrice;
                    buy_details.EntrustAmount = request.EntrustAmount;
                    buy_details.EntrustPriceGear = request.EntrustPriceGear;
                    buy_details.EntrustType = request.EntrustType;
                    buy_details.ForbidType = request.ForbidType;
                    buy_details.IsGreater = request.IsGreater;
                    buy_details.LastModified = DateTime.Now;
                    buy_details.Name = string.IsNullOrEmpty(request.Name) ? Guid.NewGuid().ToString("N") : request.Name;
                    buy_details.ConditionRelativeRate = request.ConditionRelativeRate;
                    buy_details.ConditionRelativeType = request.ConditionRelativeType;
                    buy_details.ConditionType = request.ConditionType;
                    db.SaveChanges();

                    var follow = (from item in db.t_account_shares_conditiontrade_buy_details_follow
                                  where item.DetailsId == request.Id
                                  select item).ToList();
                    if (follow.Count() > 0)
                    {
                        db.t_account_shares_conditiontrade_buy_details_follow.RemoveRange(follow);
                        db.SaveChanges();
                    }

                    foreach (var followId in request.FollowAccountList)
                    {
                        db.t_account_shares_conditiontrade_buy_details_follow.Add(new t_account_shares_conditiontrade_buy_details_follow
                        {
                            CreateTime = DateTime.Now,
                            FollowAccountId = followId,
                            DetailsId = request.Id
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
        /// 修改股票买入条件状态
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ModifyAccountBuyConditionStatus(ModifyStatusRequest request, HeadBase basedata) {
            using (var db = new meal_ticketEntities())
            {
                var buy_details = (from item in db.t_account_shares_conditiontrade_buy_details
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (buy_details == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (buy_details.TriggerTime != null)
                {
                    throw new WebApiException(400, "已执行无法修改状态");
                }
                
                buy_details.Status = request.Status;
                buy_details.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除股票买入条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountBuyCondition(DeleteRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var buy_details = (from item in db.t_account_shares_conditiontrade_buy_details
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (buy_details == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                if (buy_details.TriggerTime != null)
                {
                    throw new WebApiException(400, "已执行无法删除");
                }
                db.t_account_shares_conditiontrade_buy_details.Remove(buy_details);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取股票买入额外条件分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionOtherGroupInfo> GetAccountBuyConditionOtherGroupList(DetailsPageRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var other = from item in db.t_account_shares_conditiontrade_buy_details_other
                            where item.DetailsId == request.Id
                            select item;
                int totalCount = other.Count();
                return new PageRes<AccountBuyConditionOtherGroupInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in other
                            orderby item.CreateTime
                            select new AccountBuyConditionOtherGroupInfo
                            {
                                Id=item.Id,
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票买入额外条件分组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddAccountBuyConditionOtherGroup(AddAccountBuyConditionOtherGroupRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_account_shares_conditiontrade_buy_details_other.Add(new t_account_shares_conditiontrade_buy_details_other 
                {
                    Status=1,
                    CreateTime=DateTime.Now,
                    DetailsId=request.DetailsId,
                    LastModified=DateTime.Now,
                    Name=request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑股票买入额外条件分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionOtherGroup(ModifyAccountBuyConditionOtherGroupRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_account_shares_conditiontrade_buy_details_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                other.Name = request.Name;
                other.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改股票买入额外条件分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionOtherGroupStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_account_shares_conditiontrade_buy_details_other
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
        /// 删除股票买入额外条件分组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteAccountBuyConditionOtherGroup(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var other = (from item in db.t_account_shares_conditiontrade_buy_details_other
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (other == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_details_other.Remove(other);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取股票买入转自动条件分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionAutoGroupInfo> GetAccountBuyConditionAutoGroupList(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = from item in db.t_account_shares_conditiontrade_buy_details_auto
                            where item.DetailsId == request.Id
                            select item;
                int totalCount = auto.Count();
                return new PageRes<AccountBuyConditionAutoGroupInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in auto
                            orderby item.CreateTime
                            select new AccountBuyConditionAutoGroupInfo
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
        /// 添加股票买入转自动条件分组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void AddAccountBuyConditionAutoGroup(AddAccountBuyConditionAutoGroupRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_account_shares_conditiontrade_buy_details_auto.Add(new t_account_shares_conditiontrade_buy_details_auto
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    DetailsId = request.DetailsId,
                    LastModified = DateTime.Now,
                    Name = request.Name
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑股票买入转自动条件分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionAutoGroup(ModifyAccountBuyConditionAutoGroupRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = (from item in db.t_account_shares_conditiontrade_buy_details_auto
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
        /// 修改股票买入转自动条件分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionAutoGroupStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = (from item in db.t_account_shares_conditiontrade_buy_details_auto
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
        /// 删除股票买入转自动条件分组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void DeleteAccountBuyConditionAutoGroup(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var auto = (from item in db.t_account_shares_conditiontrade_buy_details_auto
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (auto == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_details_auto.Remove(auto);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取股票买入额外条件列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionOtherInfo> GetAccountBuyConditionOtherList(DetailsPageRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_account_shares_conditiontrade_buy_details_other_trend
                            where item.OtherId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<AccountBuyConditionOtherInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new AccountBuyConditionOtherInfo
                            {
                                Status = item.Status,
                                TrendId=item.TrendId,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TrendDescription = item.TrendDescription,
                                TrendName = item.TrendName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票买入额外条件
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountBuyConditionOther(AddAccountBuyConditionOtherRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    t_account_shares_conditiontrade_buy_details_other_trend temp = new t_account_shares_conditiontrade_buy_details_other_trend
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        OtherId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_account_shares_conditiontrade_buy_details_other_trend.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_account_shares_conditiontrade_buy_details_other_trend_par.Add(new t_account_shares_conditiontrade_buy_details_other_trend_par 
                        {
                            OtherTrendId= temp.Id,
                            CreateTime=DateTime.Now,
                            LastModified=DateTime.Now,
                            ParamsInfo= item.ParamsInfo
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
        /// 修改股票买入额外条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionOtherStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                trend.Status = request.Status;
                trend.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除股票买入额外条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountBuyConditionOther(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_details_other_trend.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 获取股票买入转自动条件列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionAutoInfo> GetAccountBuyConditionAutoList(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = from item in db.t_account_shares_conditiontrade_buy_details_auto_trend
                            where item.AutoId == request.Id
                            select item;
                int totalCount = trend.Count();

                return new PageRes<AccountBuyConditionAutoInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trend
                            orderby item.CreateTime descending
                            select new AccountBuyConditionAutoInfo
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
        /// 添加股票买入转自动条件
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountBuyConditionAuto(AddAccountBuyConditionAutoRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    t_account_shares_conditiontrade_buy_details_auto_trend temp = new t_account_shares_conditiontrade_buy_details_auto_trend
                    {
                        Status = 1,
                        CreateTime = DateTime.Now,
                        LastModified = DateTime.Now,
                        AutoId = request.RelId,
                        TrendDescription = request.TrendDescription,
                        TrendId = request.TrendId,
                        TrendName = request.TrendName
                    };
                    db.t_account_shares_conditiontrade_buy_details_auto_trend.Add(temp);
                    db.SaveChanges();

                    //查询参数值
                    var par = (from item in db.t_account_shares_conditiontrade_buy_trend_par_template.AsNoTracking()
                               where item.TrendId == request.TrendId
                               select item).ToList();
                    foreach (var item in par)
                    {
                        db.t_account_shares_conditiontrade_buy_details_auto_trend_par.Add(new t_account_shares_conditiontrade_buy_details_auto_trend_par
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
        /// 修改股票买入转自动条件状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionAutoStatus(ModifyStatusRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend
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
        /// 删除股票买入转自动条件
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountBuyConditionAuto(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trend = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend
                             where item.Id == request.Id
                             select item).FirstOrDefault();
                if (trend == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_details_auto_trend.Remove(trend);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionOtherParInfo> GetAccountBuyConditionOtherPar(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_account_shares_conditiontrade_buy_details_other_trend_par
                               where item.OtherTrendId==request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<AccountBuyConditionOtherParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new AccountBuyConditionOtherParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountBuyConditionOtherPar(AddAccountBuyConditionOtherParRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1)
                {
                    var par = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend_par
                               where item.OtherTrendId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_account_shares_conditiontrade_buy_details_other_trend_par.Add(new t_account_shares_conditiontrade_buy_details_other_trend_par
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
                    db.t_account_shares_conditiontrade_buy_details_other_trend_par.Add(new t_account_shares_conditiontrade_buy_details_other_trend_par
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
        /// 编辑股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionOtherPar(ModifyAccountBuyConditionOtherParRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend_par
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
        /// 删除股票买入额外条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountBuyConditionOtherPar(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_details_other_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_details_other_trend_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<AccountBuyConditionAutoParInfo> GetAccountBuyConditionAutoPar(DetailsPageRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = from item in db.t_account_shares_conditiontrade_buy_details_auto_trend_par
                               where item.AutoTrendId == request.Id
                               select item;
                int totalCount = trendPar.Count();

                return new PageRes<AccountBuyConditionAutoParInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in trendPar
                            orderby item.CreateTime descending
                            select new AccountBuyConditionAutoParInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ParamsInfo = item.ParamsInfo
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void AddAccountBuyConditionAutoPar(AddAccountBuyConditionAutoParRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.TrendId != 1)
                {
                    var par = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend_par
                               where item.AutoTrendId == request.RelId
                               select item).FirstOrDefault();
                    if (par != null)
                    {
                        par.ParamsInfo = request.ParamsInfo;
                        par.LastModified = DateTime.Now;
                    }
                    else
                    {
                        db.t_account_shares_conditiontrade_buy_details_auto_trend_par.Add(new t_account_shares_conditiontrade_buy_details_auto_trend_par
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
                    db.t_account_shares_conditiontrade_buy_details_auto_trend_par.Add(new t_account_shares_conditiontrade_buy_details_auto_trend_par
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
        /// 编辑股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void ModifyAccountBuyConditionAutoPar(ModifyAccountBuyConditionAutoParRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend_par
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
        /// 删除股票买入转自动条件类型参数
        /// </summary>
        /// <param name="request"></param>
        public void DeleteAccountBuyConditionAutoPar(DeleteRequest request, HeadBase basedata)
        {
            using (var db = new meal_ticketEntities())
            {
                var trendPar = (from item in db.t_account_shares_conditiontrade_buy_details_auto_trend_par
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (trendPar == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_account_shares_conditiontrade_buy_details_auto_trend_par.Remove(trendPar);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询买入提示列表
        /// </summary>
        /// <returns></returns>
        public List<BuyTipInfo> GetBuyTipList(HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                //查询跟投人员
                var follow = (from item in db.t_account_follow_rel
                              where item.AccountId == basedata.AccountId
                              select item.FollowAccountId).ToList();
                List<long> accountIdList = new List<long>();
                accountIdList.Add(basedata.AccountId);
                accountIdList.AddRange(follow);

                var result = (from item in db.t_account_shares_conditiontrade_buy_details
                              join item2 in db.t_account_shares_conditiontrade_buy on item.ConditionId equals item2.Id
                              join item3 in db.t_shares_quotes on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                              join item4 in db.t_account_baseinfo on item2.AccountId equals item4.Id
                              join item5 in db.t_shares_all on new { item2.Market, item2.SharesCode } equals new { item5.Market, item5.SharesCode }
                              join item6 in db.t_account_shares_entrust on item.EntrustId equals item6.Id into a
                              from ai in a.DefaultIfEmpty()
                              where item.Status == 1 && (item.BusinessStatus == 1 || item.BusinessStatus == 3 || item.BusinessStatus == 4) && item2.Status == 1 && item3.PresentPrice > 0 && item3.ClosedPrice > 0
                              orderby item.TriggerTime
                              select new BuyTipInfo
                              {
                                  SharesCode = item2.SharesCode,
                                  SharesName = item5.SharesName,
                                  AccountMobile = item4.Mobile,
                                  AccountName = item4.NickName,
                                  CurrPrice = item3.PresentPrice,
                                  TriggerTime = item.TriggerTime,
                                  EntrustPriceGear = item.EntrustPriceGear,
                                  EntrustAmount = item.EntrustAmount,
                                  Id = item.Id,
                                  AccountId = item2.AccountId,
                                  BuyAuto = item.BusinessStatus == 3 ? true : item.BusinessStatus == 4 ? false : item.BuyAuto,
                                  RisePrice = item3.PresentPrice - item3.ClosedPrice,
                                  RiseRate = (int)((item3.PresentPrice - item3.ClosedPrice) * 1.0 / item3.ClosedPrice * 10000),
                                  Status = item.BusinessStatus == 1 ? 1 : ai == null ? 2 : ai.Status != 3 ? 3 : ai.DealCount <= 0 ? 5 : ai.DealCount >= ai.EntrustCount ? 6 : 4,
                                  FollowAccountList = (from x in db.t_account_shares_conditiontrade_buy_details_follow
                                                       where x.DetailsId == item.Id
                                                       select x.FollowAccountId).ToList()
                              }).ToList();
                return result;
            }
        }

        /// <summary>
        /// 确认买入提示
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        public void ConfirmBuyTip(DetailsRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var result = (from item in db.t_account_shares_conditiontrade_buy_details
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (result == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                result.BusinessStatus = 2;
                db.SaveChanges();
            }
        }
    }
}
