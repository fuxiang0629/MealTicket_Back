using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using Aop.Api.Response;
using FXCommon.Common;
using MealTicket_Admin_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WxPayAPI;

namespace MealTicket_Admin_Handler
{
    public class ServiceHandler
    {
        /// <summary>
        /// 提现退款结果查询
        /// </summary>
        public void CashRefundResultQuery() 
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var recordDetails = (from item in db.t_account_cash_record_details
                                     where item.Status == 11
                                     select item).ToList();
                foreach (var item in recordDetails)
                {
                    try
                    {
                        var recharge = (from x in db.t_recharge_record
                                        where x.Id == item.RechargeId
                                        select x).FirstOrDefault();
                        if (recharge == null)
                        {
                            item.Status = 3;
                            item.LastModified = timeNow;
                            db.SaveChanges();
                            continue;
                        }
                        var record = (from x in db.t_account_cash_record
                                      where x.Id == item.RecordId
                                      select x).FirstOrDefault();
                        if (record == null)
                        {
                            item.Status = 3;
                            item.LastModified = timeNow;
                            recharge.RefundedMoney = recharge.RefundedMoney + item.ApplyAmount;
                            db.SaveChanges();
                            continue;
                        }

                        DateTime time = DateTime.Now;
                        //支付账号信息
                        var paymentSetting = (from x in db.t_payment_account
                                              join x2 in db.t_payment_account_settings on x.Id equals x2.PaymentAccountId
                                              where x.Id == item.PaymentAccountId
                                              select x2).ToList();
                        if (recharge.ChannelCode == "Alipay")//支付宝支付
                        {
                            string app_id = "";
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

                            IAopClient client = new DefaultAopClient("https://openapi.alipay.com/gateway.do", app_id, rsaprivatekey, "json", "1.0", "RSA2", "", "utf-8", false);
                            AlipayTradeFastpayRefundQueryRequest request = new AlipayTradeFastpayRefundQueryRequest();
                            AlipayTradeFastpayRefundQueryModel model = new AlipayTradeFastpayRefundQueryModel();
                            model.OutTradeNo = recharge.OrderSN;
                            model.OutRequestNo = item.RefundSn;
                            request.SetBizModel(model);
                            AlipayTradeFastpayRefundQueryResponse response = client.Execute(request);
                            var res = JsonConvert.DeserializeObject<ZFB_Refund_Res>(response.Body);
                            if (res.alipay_trade_fastpay_refund_query_response.code == "10000" && res.alipay_trade_fastpay_refund_query_response.sub_code == null && res.alipay_trade_fastpay_refund_query_response.refund_amount != null)
                            {
                                //修改退款状态为退款成功
                                item.Status = 2;
                                item.LastModified = timeNow;
                                item.CashedAmount = item.ApplyAmount;
                                db.SaveChanges();
                            }

                        }
                        else if (recharge.ChannelCode == "WeChat")//微信支付
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
                            WxPayData payModel = new WxPayData();//传入数据
                            WxPayData outPayModel = new WxPayData();//微信返回数据
                            payModel.SetValue("appid", appid);//appid
                            payModel.SetValue("mch_id", mch_id);//mch_id
                            payModel.SetValue("out_trade_no", recharge.OrderSN);//订单号
                            outPayModel = WxPayApi.RefundQuery(payModel, key);//调用微信接口获得返回数据

                            //将微信的数据赋值给model
                            if (outPayModel != null)
                            {
                                string ReturnCode = outPayModel.GetValue("return_code").ToString();//返回状态码 SUCCESS/FAIL
                                string ReturnMsg = outPayModel.GetValue("return_msg").ToString();//返回信息 OK/异常信息
                                string ResultCode = outPayModel.GetValue("result_code").ToString();//结果状态码 SUCCESS/FAIL
                                if (ReturnCode.Equals("SUCCESS") && ResultCode.Equals("SUCCESS"))
                                {
                                    string refund_status = outPayModel.GetValue("refund_status_0").ToString();//退款状态
                                    if (refund_status.Equals("SUCCESS"))
                                    {
                                        //修改退款状态为退款成功
                                        item.Status = 2;
                                        item.LastModified = timeNow;
                                        item.CashedAmount = item.ApplyAmount;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        item.Status = 3;
                                        item.LastModified = timeNow;
                                        recharge.RefundedMoney = recharge.RefundedMoney - item.ApplyAmount;
                                        record.Amount = record.Amount - item.ApplyAmount;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            item.Status = 3;
                            item.LastModified = timeNow;
                            recharge.RefundedMoney = recharge.RefundedMoney - item.ApplyAmount;
                            record.Amount = record.Amount - item.ApplyAmount;
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
        }
    }
}
