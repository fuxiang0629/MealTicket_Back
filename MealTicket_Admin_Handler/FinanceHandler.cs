using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using Newtonsoft.Json;
using NoticeHandler;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WxPayAPI;

namespace MealTicket_Admin_Handler
{
    public class FinanceHandler
    {

        #region====经营管理====
        /// <summary>
        /// 查询平台收支情况
        /// </summary>
        /// <returns></returns>
        public PageRes<PlatformWallet> GetPlatformWallet(PageRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var platform = from item in db.t_platform_wallet
                               select item;
                if (request.MaxId > 0)
                {
                    platform = from item in platform
                               where item.Id <= request.MaxId
                               select item;
                }
                int totalCount = platform.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = platform.Max(e => e.Id);
                }

                return new PageRes<PlatformWallet>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in platform
                            orderby item.OrderIndex
                            select new PlatformWallet
                            {
                                CreateTime = item.LastModified,
                                Id = item.Id,
                                ConfirmDeposit = item.ConfirmDeposit,
                                InCome=item.InCome,
                                Expend=item.Expend,
                                TypeCode = item.TypeCode,
                                TypeDes = item.TypeDes
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询充值记录
        /// </summary>
        /// <returns></returns>
        public PageRes<RechargeRecordInfo> GetRechargeRecordList(GetRechargeRecordListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var rechargeRecord = from item in db.t_recharge_record
                                     join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a
                                     from ai in a.DefaultIfEmpty()
                                     join item3 in db.t_payment_account on item.PaymentAccountId equals item3.Id into b
                                     from bi in b.DefaultIfEmpty()
                                     select new { item, ai, bi };
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    rechargeRecord = from item in rechargeRecord
                                     where item.ai != null && (item.ai.NickName.Contains(request.AccountInfo) || item.ai.Mobile.Contains(request.AccountInfo))
                                     select item;
                }
                if (!string.IsNullOrEmpty(request.OrderSn))
                {
                    rechargeRecord = from item in rechargeRecord
                                     where item.item.OrderSN.Contains(request.OrderSn)
                                     select item;
                }
                if (request.Status != 0)
                {
                    rechargeRecord = from item in rechargeRecord
                                     where item.item.PayStatus==request.Status
                                     select item;
                }
                if (request.StartTime != null && request.EndTime!=null)
                {
                    rechargeRecord = from item in rechargeRecord
                                     where item.item.CreateTime>= request.StartTime && item.item.CreateTime< request.EndTime
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    rechargeRecord = from item in rechargeRecord
                                     where item.item.Id <= request.MaxId
                               select item;
                }
                int totalCount = rechargeRecord.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = rechargeRecord.Max(e => e.item.Id);
                }

                return new PageRes<RechargeRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in rechargeRecord
                            orderby item.item.CreateTime descending
                            select new RechargeRecordInfo
                            {
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id,
                                OrderSN=item.item.OrderSN,
                                AccountMobile= item.ai ==null?"": item.ai.Mobile,
                                AccountName= item.ai ==null?"": item.ai.NickName,
                                PayStatus= item.item.PayStatus,
                                ChannelName= item.item.ChannelName,
                                PayAmount= item.item.PayAmount,
                                RechargeAmount= item.item.RechargeAmount,
                                PaymentAccountName= item.bi ==null?"": item.bi.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }
        #endregion


        #region====提现管理====
        /// <summary>
        /// 查询提现记录
        /// </summary>
        /// <returns></returns>
        public PageRes<CashRecordInfo> GetCashRecordList(GetCashRecordListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var cashRecord = from item in db.t_account_cash_record
                                 join item2 in db.t_account_baseinfo on item.AccountId equals item2.Id into a from ai in a.DefaultIfEmpty()
                                 select new { item, ai };
                if (!string.IsNullOrEmpty(request.AccountInfo))
                {
                    cashRecord = from item in cashRecord
                                 where item.ai != null && (item.ai.NickName.Contains(request.AccountInfo) || item.ai.Mobile.Contains(request.AccountInfo))
                                 select item;
                }
                if (!string.IsNullOrEmpty(request.OrderSn))
                {
                    cashRecord = from item in cashRecord
                                 where item.item.OrderSn.Contains(request.OrderSn)
                                     select item;
                }
                if (request.CashType != 0)
                {
                    cashRecord = from item in cashRecord
                                 where item.item.CashType == request.CashType
                                 select item;
                }
                if (request.Status != 0)
                {
                    if (request.Status == 6)
                    {
                        cashRecord = from item in cashRecord
                                     where item.item.Status == 1 || item.item.Status == 2 || item.item.Status == 21
                                     select item;
                    }
                    else if (request.Status == 7)
                    {
                        cashRecord = from item in cashRecord
                                     where item.item.Status == 3 || item.item.Status == 31 || item.item.Status == 4 || item.item.Status == 5
                                     select item;
                    }
                    else
                    {
                        cashRecord = from item in cashRecord
                                     where item.item.Status == request.Status
                                     select item;
                    }
                }
                if (request.StartTime != null && request.EndTime != null)
                {
                    cashRecord = from item in cashRecord
                                 where item.item.CreateTime >= request.StartTime && item.item.CreateTime < request.EndTime
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    cashRecord = from item in cashRecord
                                 where item.item.Id <= request.MaxId
                                     select item;
                }
                int totalCount = cashRecord.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = cashRecord.Max(e => e.item.Id);
                }

                return new PageRes<CashRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in cashRecord
                            join item2 in db.t_bank on item.item.BankCode equals item2.BankCode into a from ai in a.DefaultIfEmpty()
                            orderby item.item.CreateTime descending
                            select new CashRecordInfo
                            {
                                CreateTime = item.item.CreateTime,
                                Id = item.item.Id,
                                OrderSn = item.item.OrderSn,
                                AccountMobile = item.ai == null ? "" : item.ai.Mobile,
                                AccountName = item.ai == null ? "" : item.ai.NickName,
                                Status = item.item.Status,
                                ServiceFee = item.item.ServiceFee,
                                ServiceFeeRate = item.item.ServiceFeeRate,
                                ApplyAmount = item.item.ApplyAmount,
                                BankName= ai==null?"":ai.BankName,
                                CashedAmount=item.item.Amount,
                                CardBreed=item.item.CardBreed,
                                CardNumber=item.item.CardNumber,
                                CashType=item.item.CashType,
                                Mobile=item.item.Mobile,
                                RealName=item.item.RealName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 开始处理提现
        /// </summary>
        public void StartCashHandle(DetailsRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_Cash_StartHandle(request.Id, errorCodeDb, errorMessageDb);
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
        /// 撤销处理提现
        /// </summary>
        public void CancelCashHandle(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_Cash_CancelHandle(request.Id, errorCodeDb, errorMessageDb);
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
        /// 结束提现
        /// </summary>
        public void FinishCash(FinishCashRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                bool isSuccess = false; 
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                        ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                        db.P_Admin_Cash_Finish(request.Id, request.Remark, basedata.AccountId, errorCodeDb, errorMessageDb);
                        int errorCode = (int)errorCodeDb.Value;
                        string errorMessage = errorMessageDb.Value.ToString();
                        if (errorCode != 0)
                        {
                            throw new WebApiException(errorCode, errorMessage);
                        }
                        tran.Commit();
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
                if (isSuccess)
                {
                    try
                    {
                        var cashRecord = (from item in db.t_account_cash_record
                                          where item.Id == request.Id
                                          select item).FirstOrDefault();
                        if (cashRecord == null)
                        {
                            return;
                        }
                        if (cashRecord.Status == 3)//提现失败
                        {
                            var tempPara = JsonConvert.SerializeObject(new
                            {
                                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                amount = cashRecord.ApplyAmount
                            });
                            NoticeSender.SendExecute("NoticeSend.CashFail", basedata.AccountId, tempPara);
                        }
                        if (cashRecord.Status == 31)//拒绝提现
                        {
                            var tempPara = JsonConvert.SerializeObject(new
                            {
                                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                amount = cashRecord.ApplyAmount
                            });
                            NoticeSender.SendExecute("NoticeSend.CashReject", basedata.AccountId, tempPara);
                        }
                        if (cashRecord.Status == 4 || cashRecord.Status == 5)//提现成功
                        {
                            var tempPara = JsonConvert.SerializeObject(new
                            {
                                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                amount = cashRecord.Amount
                            });
                            NoticeSender.SendExecute("NoticeSend.CashSuccess", basedata.AccountId, tempPara);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteFileLog("提现通知推送失败",ex);
                    }
                }
            }
        }

        /// <summary>
        /// 开始提现
        /// </summary>
        public void StartCash(StartCashRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    db.P_Admin_Cash_Start(request.Id, request.PaymentChannel,request.Type,request.PaymentAccountId,request.ApplyAmount,request.VoucherImg, basedata.AccountId, errorCodeDb, errorMessageDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }

                    //退款方式
                    if (request.Type == 1)
                    {
                        var refundDetails = (from item in db.t_account_cash_record_details
                                             where item.RecordId == request.Id && item.Type == 1 && item.Status == 1
                                             select item).ToList();
                        foreach (var item in refundDetails)
                        {
                            item.Status = 11;
                        }
                        if (refundDetails.Count() > 0)
                        {
                            db.SaveChanges();

                            RefundDel del = new RefundDel(CashRefund);
                            IAsyncResult ar = del.BeginInvoke(refundDetails, null, del);
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

        private delegate void RefundDel(List<t_account_cash_record_details> list);

        private void CashRefund(List<t_account_cash_record_details> list)
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                foreach (var item in list)
                {
                    var recordDetails = (from x in db.t_account_cash_record_details
                                         where x.Id == item.Id && x.Status==11
                                         select x).FirstOrDefault();
                    if (recordDetails == null)
                    {
                        continue;
                    }
                    var recharge = (from x in db.t_recharge_record
                                    where x.Id == recordDetails.RechargeId
                                    select x).FirstOrDefault();
                    if (recharge == null)
                    {
                        recordDetails.Status = 3;
                        recordDetails.LastModified = timeNow;
                        db.SaveChanges();
                        continue;
                    }
                    var record = (from x in db.t_account_cash_record
                                  where x.Id == recordDetails.RecordId
                                  select x).FirstOrDefault();
                    if (record == null)
                    {
                        recordDetails.Status = 3;
                        recordDetails.LastModified = timeNow;
                        recharge.RefundedMoney = recharge.RefundedMoney + recordDetails.ApplyAmount;
                        db.SaveChanges();
                        continue;
                    }
                    //支付账号信息
                    var paymentSetting = (from x in db.t_payment_account
                                          join x2 in db.t_payment_account_settings on x.Id equals x2.PaymentAccountId
                                          where x.Id== recordDetails.PaymentAccountId
                                          select x2).ToList();
                    if (recharge.ChannelCode == "Alipay")//支付宝退款
                    {
                        try
                        {
                            string app_id="";
                            string rsaprivatekey = "";
                            foreach (var x in paymentSetting)
                            {
                                if (x.SettingKey == "app_id")
                                {
                                    app_id = x.SettingValue;
                                }
                                if (x.SettingKey == "rsaprivatekey")
                                {
                                    rsaprivatekey = x.SettingValue;
                                }
                            }

                            IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do",app_id, rsaprivatekey, "json", "1.0", "RSA2", "", "utf-8", false);
                            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
                            AlipayTradeRefundModel model = new AlipayTradeRefundModel();
                            model.OutTradeNo = recharge.OrderSN;
                            model.RefundAmount = Math.Round((recordDetails.ApplyAmount/100)*1.0/100,2).ToString();
                            model.OutRequestNo = recordDetails.RefundSn;
                            request.SetBizModel(model);
                            AlipayTradeRefundResponse response = client.Execute(request);
                            var res = JsonConvert.DeserializeObject<ZFB_Refund_Res>(response.Body);
                            if (res != null && res.alipay_trade_refund_response != null && res.alipay_trade_refund_response.code == "10000" && res.alipay_trade_refund_response.sub_code == null)
                            {
                            }
                            else
                            {
                                recordDetails.Status = 3;
                                recordDetails.LastModified = timeNow;
                                recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                                record.Amount = record.Amount - recordDetails.ApplyAmount;
                                db.SaveChanges();
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("支付宝申请退款异常", ex);
                            recordDetails.Status = 3;
                            recordDetails.LastModified = timeNow;
                            recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                            record.Amount = record.Amount - recordDetails.ApplyAmount;
                            db.SaveChanges();
                            continue;
                        }
                    }
                    else if (recharge.ChannelCode == "WeChat")//微信退款
                    {
                        try
                        {
                            string appid = "";
                            string mch_id = "";
                            string key = "";
                            string SSlCertPath = "";
                            string SSlCertPassword = "";
                            foreach (var x in paymentSetting)
                            {
                                if (x.SettingKey == "appid")
                                {
                                    appid = x.SettingValue;
                                }
                                if (x.SettingKey == "mch_id")
                                {
                                    mch_id = x.SettingValue;
                                }
                                if (x.SettingKey == "key")
                                {
                                    key = x.SettingValue;
                                }
                                if (x.SettingKey == "SSlCertPath")
                                {
                                    SSlCertPath = x.SettingValue;
                                }
                                if (x.SettingKey == "SSlCertPassword")
                                {
                                    SSlCertPassword = x.SettingValue;
                                }
                            }

                            //退钱
                            WxPayData payModel = new WxPayData();//传入数据
                            WxPayData outPayModel = new WxPayData();//微信返回数据
                            payModel.SetValue("appid",appid);//appid
                            payModel.SetValue("mch_id", mch_id);//mch_id
                            payModel.SetValue("out_trade_no", recharge.OrderSN);//订单号
                            payModel.SetValue("op_user_id", mch_id);//mch_id
                            payModel.SetValue("out_refund_no", recordDetails.RefundSn);//退款单号
                            payModel.SetValue("total_fee", (recharge.PayAmount/100).ToString());//订单总金额
                            payModel.SetValue("refund_fee", (recordDetails.ApplyAmount/100).ToString());//退款金额

                            outPayModel = WxPayApi.Refund(payModel, key, SSlCertPath, SSlCertPassword);//调用微信接口获得返回数据
                            //将微信的数据赋值给model
                            if (outPayModel != null)
                            {
                                string ReturnCode = outPayModel.GetValue("return_code").ToString();//返回状态码 SUCCESS/FAIL
                                string ReturnMsg = outPayModel.GetValue("return_msg").ToString();//返回信息 OK/异常信息
                                if (!ReturnCode.Equals("SUCCESS"))
                                {
                                    recordDetails.Status = 3;
                                    recordDetails.LastModified = timeNow;
                                    recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                                    record.Amount = record.Amount - recordDetails.ApplyAmount;
                                    db.SaveChanges();
                                    continue;
                                }
                                else
                                {
                                    string ResultCode = outPayModel.GetValue("result_code").ToString();//结果状态码 SUCCESS/FAIL
                                    if (!ResultCode.Equals("SUCCESS"))
                                    {
                                        recordDetails.Status = 3;
                                        recordDetails.LastModified = timeNow;
                                        recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                                        record.Amount = record.Amount - recordDetails.ApplyAmount;
                                        db.SaveChanges();
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                recordDetails.Status = 3;
                                recordDetails.LastModified = timeNow;
                                recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                                record.Amount = record.Amount - recordDetails.ApplyAmount;
                                db.SaveChanges();
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteFileLog("微信申请退款异常", ex);
                            recordDetails.Status = 3;
                            recordDetails.LastModified = timeNow;
                            recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                            record.Amount = record.Amount - recordDetails.ApplyAmount;
                            db.SaveChanges();
                            continue;
                        }
                    }
                    else
                    {
                        recordDetails.Status = 3;
                        recordDetails.LastModified = timeNow;
                        recharge.RefundedMoney = recharge.RefundedMoney - recordDetails.ApplyAmount;
                        record.Amount = record.Amount - recordDetails.ApplyAmount;
                        db.SaveChanges();
                        continue;
                    }
                }
            }
        }


        /// <summary>
        /// 查询提现详情
        /// </summary>
        /// <returns></returns>
        public CashRecordDetails GetCashRecordDetails(DetailsRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var record = (from item in db.t_account_cash_record
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (record == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }
                DateTime? finishTime = null;
                var details = (from item in db.t_account_cash_record_details
                               where item.RecordId == request.Id
                               orderby item.CreateTime descending
                               select new CashDetails
                               {
                                   ServiceFee = item.ServiceFee,
                                   Status = item.Status,
                                   StatusDes = item.StatusDes,
                                   VoucherImg = item.VoucherImg,
                                   RefundSn = item.RefundSn,
                                   AdminAccountName = item.AdminAccountName,
                                   ApplyAmount = item.ApplyAmount,
                                   CreateTime = item.CreateTime,
                                   FinishTime = item.Status == 2 ? item.LastModified : finishTime,
                                   CashedAmount = item.CashedAmount,
                                   Id = item.Id,
                                   Type = item.Type,
                                   PaymentAccountName = item.PaymentAccountName
                               }).ToList();
                return new CashRecordDetails
                {
                    ApplyAmount = record.ApplyAmount,
                    CashDetailsList = details,
                    CashedAmount = details.Count() <= 0 ? 0 : details.Sum(e => e.CashedAmount),
                    FinishInfo = record.AdminAccountId > 0 ?
                    (new CashFinishInfo
                    {
                        AdminAccountName = record.AdminAccountName,
                        FinishTime = record.LastModified,
                        Remark = record.StatusDes
                    }) : null
                };
            }
        }

        /// <summary>
        /// 查询转账记录
        /// </summary>
        /// <returns></returns>
        public PageRes<CashTransferRecord> GetCashTransferRecord(PageRequest request) 
        {
            using (var db=new meal_ticketEntities())
            {
                var cashDetails = from item in db.t_account_cash_record
                                  join item2 in db.t_account_cash_record_details on item.Id equals item2.RecordId
                                  group new { item, item2 } by item into g
                                  select g;
                int totalCount = cashDetails.Count();

                return new PageRes<CashTransferRecord>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in cashDetails
                            join item2 in db.t_account_baseinfo on item.Key.AccountId equals item2.Id into a from ai in a.DefaultIfEmpty()
                            join item3 in db.t_bank on item.Key.BankCode equals item3.BankCode
                            orderby item.Key.CreateTime descending
                            select new CashTransferRecord
                            {
                                CashOrderSn = item.Key.OrderSn,
                                TotalServiceFee = item.Sum(e => e.item2.ServiceFee),
                                ApplyAmount = item.Key.ApplyAmount,
                                CardNumber = item.Key.CardNumber,
                                Count = item.Count(),
                                Id = item.Key.Id,
                                LastTime = item.Max(e => e.item2.CreateTime),
                                RealName = item.Key.RealName,
                                TotalAmount = item.Sum(e => e.item2.ApplyAmount),
                                TransferType = "银行卡("+ item3.BankName+")",
                                Type = "提现",
                                AccountName = ai == null ? "" : ai.NickName,
                                AccountMobile = ai == null ? "" : ai.Mobile
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询转账详情
        /// </summary>
        /// <returns></returns>
        public List<CashDetails> GetCashTransferDetails(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var record = (from item in db.t_account_cash_record
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (record == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                DateTime? finishTime = null;
                var details = (from item in db.t_account_cash_record_details
                               where item.RecordId == request.Id
                               orderby item.CreateTime descending
                               select new CashDetails
                               {
                                   ServiceFee = item.ServiceFee,
                                   Status = item.Status,
                                   StatusDes = item.StatusDes,
                                   VoucherImg = item.VoucherImg,
                                   RefundSn = item.RefundSn,
                                   AdminAccountName = item.AdminAccountName,
                                   ApplyAmount = item.ApplyAmount,
                                   CreateTime = item.CreateTime,
                                   FinishTime = item.Status == 2 ? item.LastModified : finishTime,
                                   CashedAmount = item.CashedAmount,
                                   Id = item.Id,
                                   Type = item.Type,
                                   PaymentAccountName = item.PaymentAccountName
                               }).ToList();
                return details;
            }
        }

        /// <summary>
        /// 查询提现打款账户列表
        /// </summary>
        /// <returns></returns>
        public List<CashAccountInfo> GetCashAccountList(GetCashAccountListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var cashAccount = from item in db.t_payment_account
                                  where item.Status == 1 && item.ChannelCode == request.ChannelCode
                                  select item;

                if (request.Type == 2)
                {
                    var accountlist = (from item in db.t_payment_cash_channel_transfer_account_rel
                                       join item2 in cashAccount on item.PaymentAccountId equals item2.Id
                                       where item.ChannelCode == request.ChannelCode && item.BusinessCode == "Recharge"
                                       select new CashAccountInfo
                                       {
                                           MaxAmount = -1,
                                           PaymentAccountId = item2.Id,
                                           PaymentAccountName = item2.Name
                                       }).ToList();
                    return accountlist;
                }
                else if (request.Type == 1)
                {
                    var cash = (from item in db.t_account_cash_record
                                where item.Id == request.CashRecordId
                                select item).FirstOrDefault();
                    if (cash == null)
                    {
                        throw new WebApiException(400,"参数错误");
                    }
                    var accountlist = (from item in db.t_payment_cash_channel_refund_account_rel
                                       join item2 in cashAccount on item.PaymentAccountId equals item2.Id
                                       where item.ChannelCode == request.ChannelCode && item.BusinessCode == "Recharge"
                                       select new CashAccountInfo
                                       {
                                           PaymentAccountId = item2.Id,
                                           PaymentAccountName = item2.Name
                                       }).ToList();
                    foreach (var item in accountlist)
                    {
                        var recharge = from x in db.t_recharge_record
                                       where x.PayStatus == 4 && x.AccountId == cash.AccountId && x.ChannelCode == request.ChannelCode && x.PaymentAccountId == item.PaymentAccountId && x.PayAmount - x.RefundedMoney > 0 && x.SupportRefund == true
                                       select x;

                        item.MaxAmount = recharge.Count()<=0?0: recharge.Sum(e=>e.PayAmount-e.RefundedMoney);
                    }
                    return accountlist.OrderByDescending(e => e.MaxAmount).ToList();
                }
                else
                {
                    throw new WebApiException(400,"参数错误");
                }
            }
        }
        #endregion
    }
}
