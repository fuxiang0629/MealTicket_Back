using FXCommon.Common;
using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.BusinessHandler
{
    public class SmsCallbackHandler
    {
        /// <summary>
        /// 获取应用信息
        /// </summary>
        /// <param name="channelCode"></param>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public SmsAppInfo GetAppInfo(string channelCode,string appkey)
        {
            using (var db = new meal_ticketEntities())
            {
                var appInfo = (from item in db.t_sms_channel_app
                               where item.ChannelCode == channelCode && item.AppKey == appkey
                               select new SmsAppInfo 
                               {
                                   AppKey=item.AppKey,
                                   AppSecret=item.AppSecret
                               }).FirstOrDefault();
                return appInfo;
            }
        }

        /// <summary>
        /// 保存回调信息
        /// </summary>
        /// <param name="callbackInfo"></param>
        public void SaveCallbackInfo(string channelCode, string appkey,int type,string callbackInfo)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_sms_callback.Add(new t_sms_callback 
                { 
                    AppKey=appkey,
                    CallbackInfo=callbackInfo,
                    ChannelCode=channelCode,
                    CreateTime=DateTime.Now,
                    Type=type
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 签名审核回调处理
        /// </summary>
        /// <param name="channelCode"></param>
        /// <param name="appkey"></param>
        /// <param name="signId"></param>
        /// <param name="status"></param>
        /// <param name="refuseReason"></param>
        /// <param name="callbackInfoJson"></param>
        /// <returns></returns>
        public bool SignExamineCallback(string channelCode, string appkey,string signId,int status,string refuseReason)
        {
            using (var db = new meal_ticketEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    var sign = (from item in db.t_sms_sign
                                where item.ChannelCode == channelCode && item.AppKey == appkey && item.ThirdSignId == signId
                                select item).FirstOrDefault();
                    if (sign == null)
                    {
                        throw new Exception("签名不存在");
                    }
                    if (sign.ExamineStatus != 2)
                    {
                        throw new Exception("签名审核状态不匹配");
                    }
                    sign.ExamineTime = DateTime.Now;
                    sign.ExamineFailReason = refuseReason;
                    if (status == 1) 
                    {
                        sign.ExamineStatus = 3;
                    }
                    else
                    {
                        sign.ExamineStatus = 4;
                    }
                    db.SaveChanges();

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("签名回调业务处理异常",ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 模板审核回调处理
        /// </summary>
        /// <param name="channelCode"></param>
        /// <param name="appkey"></param>
        /// <param name="signId"></param>
        /// <param name="status"></param>
        /// <param name="refuseReason"></param>
        /// <returns></returns>
        public bool TemplateExamineCallback(string channelCode, string appkey, string signId, int status, string refuseReason)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var sign = (from item in db.t_sms_template
                                where item.ChannelCode == channelCode && item.AppKey == appkey && item.ThirdTempId == signId
                                select item).FirstOrDefault();
                    if (sign == null)
                    {
                        throw new Exception("模板不存在");
                    }
                    if (sign.ExamineStatus != 2)
                    {
                        throw new Exception("模板审核状态不匹配");
                    }
                    sign.ExamineTime = DateTime.Now;
                    sign.ExamineFailReason = refuseReason;
                    if (status == 1)
                    {
                        sign.ExamineStatus = 3;
                    }
                    else
                    {
                        sign.ExamineStatus = 4;
                    }
                    db.SaveChanges();

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("模板回调业务处理异常", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 短信下行结果回调
        /// </summary>
        /// <param name="channelCode"></param>
        /// <param name="appkey"></param>
        /// <param name="msgId"></param>
        /// <param name="status"></param>
        /// <param name="receiveTime"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        public bool SmsSendDownCallback(string channelCode, string appkey,string msgId,int status,long receiveTimeSpan,string phone) 
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime receiveTime= startTime.AddMilliseconds(receiveTimeSpan);
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var sms = (from item in db.t_notice_send_record_details
                               where item.SmsChannelCode == channelCode && item.SmsAppKey == appkey && item.ThirdMsgId == msgId
                               select item).FirstOrDefault();
                    if (sms == null)
                    {
                        throw new Exception("短信下发不存在");
                    }
                    if (sms.Status != 4000)
                    {
                        throw new Exception("短信下发状态不匹配");
                    }
                    sms.ReceiveTime = receiveTime;
                    sms.Status = status;
                    if (status == 4001)
                    {
                        sms.StatusDes = "发送成功";
                    }
                    else if (status == 4002)
                    {
                        sms.StatusDes = "被叫手机号码为运营商黑名单，需联系运营商处理";
                    }
                    else if (status == 4003)
                    {
                        sms.StatusDes = "手机终端问题，手机关机、停机等，请确认手机状态是否正常";
                    }
                    else if (status == 4004)
                    {
                        sms.StatusDes = "被叫手机号码为空号，请核实手机号码是否合规";
                    }
                    else if (status == 4005)
                    {
                        sms.StatusDes = "可发送短信余量不足";
                    }
                    else if (status == 4006)
                    {
                        sms.StatusDes = "发送超频，发送频率超过运营商限制";
                    }
                    else if (status == 4010)
                    {
                        sms.StatusDes = "敏感词拦截，因短信内容存在敏感词导致运营商拦截";
                    }
                    else
                    {
                        sms.StatusDes = "其他错误";
                    }
                    db.SaveChanges();

                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("模板回调业务处理异常", ex);
                    return true;
                }
            }
        }
    }
}
