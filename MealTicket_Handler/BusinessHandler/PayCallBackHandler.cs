using Aop.Api.Util;
using FXCommon.Common;
using MealTicket_Handler.Model.PayCallBack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WxPayAPI;

namespace MealTicket_Handler.BusinessHandler
{
    public class PayCallBackHandler
    {
        /// <summary>
        /// 支付宝支付结果回调
        /// </summary>
        /// <returns></returns>
        public bool ZFBPayResultCallback(ZFBPayCallbackInfo callbackInfo) 
        {
            if (callbackInfo == null)
            {
                return false;
            }
            else if (callbackInfo.trade_status.Equals("TRADE_SUCCESS"))//支付成功
            {
                string orderSN = callbackInfo.out_trade_no;//订单号
                long total_amount = 0;//订单金额
                string content = JsonConvert.SerializeObject(callbackInfo);//支付宝回调数据
                using (var db = new meal_ticketEntities())
                {
                    try
                    {
                        db.t_pay_callback.Add(new t_pay_callback
                        {
                            BusinessCode = "Recharge",
                            CreateTime = DateTime.Now,
                            ChannelCode = "Alipay",
                            CallBackInfo = content
                        });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    { }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        long paymentAccountId;
                        if (!long.TryParse(callbackInfo.passback_params, out paymentAccountId))
                        {
                            return false;
                        }
                        //支付参数
                        string app_id = "";
                        string seller_id = "";
                        string alipay_public_key = "";
                        var zfb_settings = (from item in db.t_payment_account_settings
                                            where item.PaymentAccountId== paymentAccountId
                                            select item).ToList();
                        foreach (var item in zfb_settings)
                        {
                            if (item.SettingKey == "app_id")
                            {
                                app_id = item.SettingValue;
                            }
                            if (item.SettingKey == "seller_id")
                            {
                                seller_id = item.SettingValue;
                            }
                            if (item.SettingKey == "alipay_public_key")
                            {
                                alipay_public_key = item.SettingValue;
                            }
                        }

                        try
                        {
                            //开始验签
                            var parMap = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(content);//组成验签字符串
                            bool checkSign = AlipaySignature.RSACheckV1(parMap, alipay_public_key, callbackInfo.charset, callbackInfo.sign_type, false);
                            if (checkSign && app_id == callbackInfo.app_id && seller_id == callbackInfo.seller_id)//验签成功&appid一致&sellerid一致
                            {
                                total_amount = Convert.ToInt64(Math.Round(Convert.ToDouble(callbackInfo.total_amount) * 100));
                                db.P_Recharge_PayResultCallback(orderSN, total_amount * 100);
                                tran.Commit();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("支付宝回调操作异常", ex);
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 微信支付结果回调
        /// </summary>
        /// <returns></returns>
        public bool WXPayResultCallback(WxPayData callbackInfo)
        {
            if (callbackInfo == null)
            {
                return false;
            }
            else
            {
                string orderSN = callbackInfo.GetValue("out_trade_no").ToString();//订单号
                long total_amount = 0;//订单金额
                string content = callbackInfo.ToXml();//支付宝回调数据
                using (var db = new meal_ticketEntities())
                {
                    try
                    {
                        db.t_pay_callback.Add(new t_pay_callback
                        {
                            BusinessCode = "Recharge",
                            CreateTime = DateTime.Now,
                            ChannelCode = "WeChat",
                            CallBackInfo = content
                        });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    { }
                    using (var tran = db.Database.BeginTransaction())
                    {
                        long paymentAccountId;
                        if (!long.TryParse(callbackInfo.GetValue("attach").ToString(), out paymentAccountId))
                        {
                            return false;
                        }
                        //支付参数
                        string appid = "";
                        string mch_id = "";
                        string key = "";
                        var wx_settings = (from item in db.t_payment_account_settings
                                           where item.PaymentAccountId== paymentAccountId
                                           select item).ToList();
                        foreach (var item in wx_settings)
                        {
                            if (item.SettingKey == "appid")
                            {
                                appid = item.SettingValue;
                            }
                            if (item.SettingKey == "mch_id")
                            {
                                mch_id = item.SettingValue;
                            }
                            if (item.SettingKey == "key")
                            {
                                key = item.SettingValue;
                            }
                        }

                        try
                        {
                            //验签
                            bool checkSign = callbackInfo.CheckSign(key);
                            if (checkSign && callbackInfo.GetValue("appid").ToString() == appid && callbackInfo.GetValue("mch_id").ToString() == mch_id && callbackInfo.GetValue("result_code").ToString() == "SUCCESS")//验签通过&appid一致&mchid一致&结果为支付成功
                            {
                                total_amount = long.Parse(callbackInfo.GetValue("total_fee").ToString());
                                db.P_Recharge_PayResultCallback(orderSN, total_amount*100);
                                tran.Commit();
                                return true;
                            }
                            else
                            {
                                throw new Exception("支付失败");
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.WriteFileLog("微信支付回调报异常", ex);
                            return false;
                        }
                    }
                }
            }
        }
    }
}
