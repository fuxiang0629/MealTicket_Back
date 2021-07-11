using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using MealTicket_DBCommon;
using MealTicket_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WxPayAPI;

namespace MealTicket_Handler.BusinessHandler
{
    public class RechargeHandler
    {
        /// <summary>
        /// 获取充值支付渠道分组
        /// </summary>
        /// <returns></returns>
        public List<RechargePaychannelGroupInfo> GetRechargePaychannelGroup() 
        {
            using (var db = new meal_ticketEntities())
            {
                var paygroup = (from item in db.t_payment_channel_group
                                where item.Status == 1
                                select new RechargePaychannelGroupInfo
                                {
                                    Description = item.Description,
                                    Name = item.Name,
                                    Tag = item.Tag,
                                    Type = item.Type
                                }).ToList();
                return paygroup;
            }
        }

        /// <summary>
        /// 获取充值在线支付渠道列表
        /// </summary>
        /// <returns></returns>
        public List<RechargePaychannelOnlineInfo> GetRechargePaychannelOnline() 
        {
            using (var db = new meal_ticketEntities())
            {
                var channelList = (from item in db.t_payment_channel
                                   where item.BusinessCode == "Recharge" && item.Status == 1 && item.Type==1
                                   orderby item.OrderIndex
                                   select new RechargePaychannelOnlineInfo
                                   {
                                       BusinessCode = item.BusinessCode,
                                       ChannelCode = item.ChannelCode,
                                       ChannelName = item.ChannelName
                                   }).ToList();
                return channelList;
            }
        }

        /// <summary>
        /// 获取充值线下卡卡支付信息
        /// </summary>
        /// <returns></returns>
        public RechargePayOflineInfo GetRechargePayOfflineInfo() 
        {
            RechargePayOflineInfo result = new RechargePayOflineInfo 
            {
                ChannelCode= "BankCard",
                BusinessCode= "Recharge"
            };
            using (var db = new meal_ticketEntities())
            {
                var channel = (from item in db.t_payment_channel
                               where item.BusinessCode == "Recharge" && item.Status == 1 && item.ChannelCode == "BankCard"
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400,"暂未开通");
                }

                var paymentAccount = (from item in db.t_payment_channel_account_rel
                                      join item2 in db.t_payment_account on item.PaymentAccountId equals item2.Id
                                      where item.ChannelCode == "BankCard" && item.BusinessCode == "Recharge" && item2.Status == 1 && item2.ChannelCode == "BankCard"
                                      select item).ToList().OrderBy(e => Guid.NewGuid()).FirstOrDefault();
                if (paymentAccount == null)
                {
                    throw new WebApiException(400, "暂未开通");
                }
                var parList = (from item in db.t_payment_account_settings
                               where item.PaymentAccountId == paymentAccount.PaymentAccountId
                               select item).ToList();
                foreach (var item in parList)
                {
                    if (item.SettingKey == "AccountName")
                    {
                        result.AccountName = item.SettingValue;
                    }
                    if (item.SettingKey == "Bank")
                    {
                        result.Bank = item.SettingValue;
                    }
                    if (item.SettingKey == "CardNumber")
                    {
                        result.CardNumber = item.SettingValue;
                    }
                    if (item.SettingKey == "Other")
                    {
                        result.Other = item.SettingValue;
                    }
                    if (item.SettingKey == "Mobile")
                    {
                        result.Mobile = item.SettingValue;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 开始充值
        /// </summary>
        public object StartRechargePay(StartRechargePayRequest request, HeadBase basedata) 
        {
            if (request.ChannelCode != "Alipay" && request.ChannelCode != "WeChat")
            {
                throw new WebApiException(400,"参数错误");
            }
            object result;
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    //创建订单
                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter orderIdDb = new ObjectParameter("orderId", 0);
                    db.P_CreateRechargeOrder(basedata.AccountId, request.ChannelCode, request.Amount,request.Amount,0,false, errorCodeDb, errorMessageDb, orderIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    long orderId = (long)orderIdDb.Value;

                    var order = (from item in db.t_recharge_record
                                 where item.Id == orderId
                                 select item).FirstOrDefault();
                    if (order == null)
                    {
                        throw new WebApiException(400,"意外出错");
                    }
                    int rechargeValidSecond = 1800;
                    string rechargeShowTitle = "饭票充值";
                    var sysPar = (from item in db.t_system_param
                                  where item.ParamName == "RechargeRules"
                                  select item).FirstOrDefault();
                    if (sysPar != null)
                    {
                        var temp=JsonConvert.DeserializeObject<dynamic>(sysPar.ParamValue);
                        rechargeValidSecond = temp.ValidSecond;
                        rechargeShowTitle = temp.ShowTitle;
                    }
                    var parList = (from item in db.t_payment_account_settings
                                   where item.PaymentAccountId == order.PaymentAccountId
                                   select item).ToList();
                    //开始支付
                    if (request.ChannelCode == "Alipay")
                    {
                        //获取支付宝支付参数
                        string app_id = "";
                        string rsaprivatekey = "";
                        string enable_pay_channels = "";
                        string disable_pay_channels = "";
                        string notify_url = "";
                        
                        foreach (var item in parList)
                        {
                            if (item.SettingKey == "app_id")
                            {
                                app_id = item.SettingValue;
                            }
                            if (item.SettingKey == "rsaprivatekey")
                            {
                                rsaprivatekey = item.SettingValue;
                            }
                            if (item.SettingKey == "enable_pay_channels")
                            {
                                enable_pay_channels = item.SettingValue;
                            }
                            if (item.SettingKey == "disable_pay_channels")
                            {
                                disable_pay_channels = item.SettingValue;
                            }
                            if (item.SettingKey == "notify_url")
                            {
                                notify_url = item.SettingValue;
                            }
                        }

                        IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do", app_id, rsaprivatekey, "json", "1.0", "RSA2", "", "utf-8", false);
                        //实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：alipay.trade.app.pay
                        AlipayTradeAppPayRequest req = new AlipayTradeAppPayRequest();
                        //SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。
                        AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
                        model.Subject = rechargeShowTitle;
                        model.TotalAmount = Math.Round(order.PayAmount * 1.0 / Singleton.Instance.PriceFormat, 2).ToString();
                        model.ProductCode = "QUICK_MSECURITY_PAY";
                        model.OutTradeNo = order.OrderSN;
                        model.TimeoutExpress = (rechargeValidSecond / 60).ToString() + "m";
                        model.GoodsType = "0";
                        model.PromoParams = "";
                        model.PassbackParams = order.PaymentAccountId.ToString();
                        model.EnablePayChannels = enable_pay_channels;
                        if (string.IsNullOrEmpty(enable_pay_channels))
                        {
                            model.DisablePayChannels = disable_pay_channels;
                        }
                        req.SetBizModel(model);
                        req.SetNotifyUrl(notify_url);
                        //这里和普通的接口调用不同，使用的是sdkExecute
                        AlipayTradeAppPayResponse response = client.SdkExecute(req);
                        result = response.Body;
                    }
                    else if (request.ChannelCode == "WeChat")
                    {
                        //获取微信支付参数
                        string appid = "";
                        string mch_id = "";
                        string notify_url = "";
                        string limit_pay = "";
                        string key = "";
                        foreach (var item in parList)
                        {
                            if (item.SettingKey == "appid")
                            {
                                appid = item.SettingValue;
                            }
                            if (item.SettingKey == "notify_url")
                            {
                                notify_url = item.SettingValue;
                            }
                            if (item.SettingKey == "mch_id")
                            {
                                mch_id = item.SettingValue;
                            }
                            if (item.SettingKey == "limit_pay")
                            {
                                limit_pay = item.SettingValue;
                            }
                            if (item.SettingKey == "key")
                            {
                                key = item.SettingValue;
                            }
                        }
                        WxPayData payModel = new WxPayData();//传入数据
                        WxPayData outPayModel = new WxPayData();//微信返回数据
                        try
                        {
                            payModel.SetValue("out_trade_no", order.OrderSN);//订单号
                            payModel.SetValue("body", rechargeShowTitle);//商品或支付单简要描述
                            payModel.SetValue("total_fee", Math.Round(order.PayAmount * 1.0 / Singleton.Instance.PriceFormat * 100, 0).ToString());//总额
                            payModel.SetValue("trade_type", "APP");//支付方式
                            payModel.SetValue("attach", order.PaymentAccountId.ToString());//附加数据，在查询API和支付通知中原样返回
                            payModel.SetValue("notify_url", notify_url);//获取配置中的通知链接
                            payModel.SetValue("appid", appid);//appid
                            payModel.SetValue("mch_id", mch_id);//mch_id
                            payModel.SetValue("device_info", "WEB");//device_info
                            payModel.SetValue("sign_type", "MD5");//sign_type
                            payModel.SetValue("spbill_create_ip", "8.8.8.8");//spbill_create_ip
                            payModel.SetValue("time_start", order.CreateTime.ToString("yyyyMMddHHmmss"));//time_start
                            payModel.SetValue("time_expire", order.CreateTime.AddSeconds(rechargeValidSecond).ToString("yyyyMMddHHmmss"));//time_expire
                            if (!string.IsNullOrEmpty(limit_pay))
                            {
                                payModel.SetValue("limit_pay", limit_pay);//limit_pay
                            }
                            payModel.SetValue("fee_type", "CNY");//fee_type
                            outPayModel = WxPayApi.UnifiedOrder(payModel, key);//调用微信接口获得返回数据
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        //将微信的数据赋值给model
                        if (outPayModel != null)
                        {
                            string ReturnCode = outPayModel.GetValue("return_code").ToString();//返回状态码 SUCCESS/FAIL
                            string ReturnMsg = outPayModel.GetValue("return_msg").ToString();//返回信息 OK/异常信息
                            string ResultCode = outPayModel.GetValue("result_code").ToString();//结果状态码 SUCCESS/FAIL
                            if (ReturnCode.Equals("SUCCESS") && ResultCode.Equals("SUCCESS"))
                            {
                                string temp_appid = outPayModel.GetValue("appid").ToString();//公众账号ID
                                string temp_noncestr = outPayModel.GetValue("nonce_str").ToString();//随机字符串
                                string temp_package = "Sign=WXPay";//扩展字段
                                string temp_partnerid = outPayModel.GetValue("mch_id").ToString();//商户号
                                string temp_prepayid = outPayModel.GetValue("prepay_id").ToString();//预支付交易会话ID
                                string temp_timestamp = WxPayApi.GenerateTimeStamp();//时间戳
                                string temp_sign = "";
                                if (outPayModel.CheckSign(key))
                                {
                                    WxPayData w = new WxPayData();
                                    w.SetValue("appid", temp_appid);
                                    w.SetValue("partnerid", temp_partnerid);
                                    w.SetValue("prepayid", temp_prepayid);
                                    w.SetValue("noncestr", temp_noncestr);
                                    w.SetValue("timestamp", temp_timestamp);
                                    w.SetValue("package", temp_package);
                                    w.SetValue("sign", w.MakeSign(key));
                                    if (w.CheckSign(key))
                                    {
                                        temp_sign = w.GetValue("sign").ToString();//签名
                                    }
                                    else
                                    {
                                        throw new WebApiException(400, "微信支付生成签名错误");
                                    }
                                }
                                else
                                {
                                    throw new WebApiException(400, "微信支付返回签名错误");
                                }
                                result = new
                                {
                                    appid = temp_appid,
                                    noncestr = temp_noncestr,
                                    package = temp_package,
                                    partnerid = temp_partnerid,
                                    prepayid = temp_prepayid,
                                    timestamp = temp_timestamp,
                                    sign = temp_sign,
                                    OrderSn=order.OrderSN,
                                    RechargeAmount=order.RechargeAmount,
                                    PayAmount=order.PayAmount
                                };
                            }
                            else
                            {
                                throw new WebApiException(400, "微信支付失败");
                            }
                        }
                        else 
                        {
                            throw new WebApiException(400, "微信支付失败:outPayModel空");
                        }
                    }
                    else
                    {
                        throw new WebApiException(400, "参数错误");
                    }

                    tran.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 查询充值支付是否成功
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PayIsSuccess RechargePayIsSuccess(RechargePayIsSuccessRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var recharge = (from item in db.t_recharge_record
                                where item.OrderSN == request.OrderSn
                                select item).FirstOrDefault();
                if (recharge == null)
                {
                    return new PayIsSuccess 
                    {
                        Status=2,
                        StatusRemark="支付不存在"
                    };
                }

                if(recharge.PayStatus==1)
                {
                    return new PayIsSuccess
                    {
                        Status = 0,
                        StatusRemark = ""
                    };
                }

                if (recharge.PayStatus == 2)
                {
                    return new PayIsSuccess
                    {
                        Status = 2,
                        StatusRemark = recharge.PayStatusDes
                    };
                }
                if (recharge.PayStatus == 3 || recharge.PayStatus == 4)
                {
                    return new PayIsSuccess
                    {
                        Status = 1,
                        StatusRemark = ""
                    };
                }
                return new PayIsSuccess
                {
                    Status = 2,
                    StatusRemark = ""
                };
            }
        }

        /// <summary>
        /// 查询充值记录列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="basedata"></param>
        /// <returns></returns>
        public PageRes<RechargeRecordInfo> GetRechargeRecord(GetRechargeRecordRequest request, HeadBase basedata) 
        {
            using (var db = new meal_ticketEntities())
            {
                var rechargeRecord = from item in db.t_recharge_record
                                     where item.AccountId == basedata.AccountId && item.PayStatus == 4
                                     select item;
                if (request.Year > 0)
                {
                    rechargeRecord = from item in rechargeRecord
                                     where SqlFunctions.DatePart("YEAR", item.RechargedTime)== request.Year
                                     select item;
                }
                if (request.Month > 0)
                {
                    rechargeRecord = from item in rechargeRecord
                                     where SqlFunctions.DatePart("MONTH", item.RechargedTime) == request.Month
                                     select item;
                }
                if (request.MaxId > 0)
                {
                    rechargeRecord = from item in rechargeRecord
                                     where item.Id <= request.MaxId
                                     select item;
                }

                int totalCount = rechargeRecord.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = rechargeRecord.Max(e => e.Id);
                }

                return new PageRes<RechargeRecordInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in rechargeRecord
                            orderby item.RechargedTime descending
                            select new RechargeRecordInfo
                            {
                                ChannelCode = item.ChannelCode,
                                RechargeAmount = item.RechargeAmount,
                                RechargedTime = item.RechargedTime.Value
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }
    }
}
