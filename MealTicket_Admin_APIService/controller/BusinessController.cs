using MealTicket_Admin_APIService.Filter;
using MealTicket_Admin_Handler;
using MealTicket_Admin_Handler.Model;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MealTicket_Admin_APIService.controller
{
    [RoutePrefix("business")]
    public class BusinessController:ApiController
    {
        private BusinessHandler businessHandler;

        public BusinessController()
        {
            businessHandler = WebApiManager.Kernel.Get<BusinessHandler>();
        }
        #region====页面管理====
        /// <summary>
        /// 查询页面配置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("page/setting/list"), HttpPost]
        [Description("查询页面配置列表")]
        public PageRes<PageSettingInfo> GetPageSettingList(GetPageSettingListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetPageSettingList(request);
        }

        /// <summary>
        /// 修改页面配置状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("page/setting/status/modify"), HttpPost]
        [Description("修改页面配置状态")]
        public object ModifyPageSettingStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyPageSettingStatus(request);
            return null;
        }

        /// <summary>
        /// 查询页面配置详细信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("page/setting/details"), HttpPost]
        [Description("查询页面配置详细信息")]
        public PageSettingDetails GetPageSettingDetails(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetPageSettingDetails(request);
        }

        /// <summary>
        /// 保存页面配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("page/setting/save"), HttpPost]
        [Description("保存页面配置")]
        public object SavePageSetting(SavePageSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.SavePageSetting(request);
            return null;
        }

        /// <summary>
        /// 查询客服配置列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("customerservice/setting/list"), HttpPost]
        [Description("查询客服配置列表")]
        public PageRes<CustomerServiceSettingInfo> GetCustomerServiceSettingList(GetCustomerServiceSettingListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetCustomerServiceSettingList(request);
        }

        /// <summary>
        /// 添加客服配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("customerservice/setting/add"), HttpPost]
        [Description("添加客服配置")]
        public object AddCustomerServiceSetting(AddCustomerServiceSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddCustomerServiceSetting(request);
            return null;
        }

        /// <summary>
        /// 编辑客服配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("customerservice/setting/modify"), HttpPost]
        [Description("编辑客服配置")]
        public object ModifyCustomerServiceSetting(ModifyCustomerServiceSettingRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyCustomerServiceSetting(request);
            return null;
        }

        /// <summary>
        /// 修改客服配置状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("customerservice/setting/status/modify"), HttpPost]
        [Description("修改客服配置状态")]
        public object ModifyCustomerServiceSettingStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyCustomerServiceSettingStatus(request);
            return null;
        }

        /// <summary>
        /// 删除客服配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("customerservice/setting/delete"), HttpPost]
        [Description("删除客服配置")]
        public object DeleteCustomerServiceSetting(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteCustomerServiceSetting(request);
            return null;
        }
        #endregion

        #region====帮助中心====
        /// <summary>
        /// 查询帮助中心问题分类列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/type/list"), HttpPost]
        [Description("查询帮助中心问题分类列表")]
        public PageRes<HelpcenterQuestionTypeInfo> GetHelpcenterQuestionTypeList(GetHelpcenterQuestionTypeListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetHelpcenterQuestionTypeList(request);
        }

        /// <summary>
        /// 添加帮助中心问题分类
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/type/add"), HttpPost]
        [Description("添加帮助中心问题分类")]
        public object AddHelpcenterQuestionType(AddHelpcenterQuestionTypeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddHelpcenterQuestionType(request);
            return null;
        }

        /// <summary>
        /// 编辑帮助中心问题分类
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/type/modify"), HttpPost]
        [Description("编辑帮助中心问题分类")]
        public object ModifyHelpcenterQuestionType(ModifyHelpcenterQuestionTypeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyHelpcenterQuestionType(request);
            return null;
        }

        /// <summary>
        /// 删除帮助中心问题分类
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/type/delete"), HttpPost]
        [Description("删除帮助中心问题分类")]
        public object DeleteHelpcenterQuestionType(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteHelpcenterQuestionType(request);
            return null;
        }

        /// <summary>
        /// 查询帮助中心问题列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/list"), HttpPost]
        [Description("查询帮助中心问题列表")]
        public PageRes<HelpcenterQuestionInfo> GetHelpcenterQuestionList(GetHelpcenterQuestionListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetHelpcenterQuestionList(request);
        }

        /// <summary>
        /// 添加帮助中心问题
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/add"), HttpPost]
        [Description("添加帮助中心问题")]
        public object AddHelpcenterQuestion(AddHelpcenterQuestionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddHelpcenterQuestion(request);
            return null;
        }

        /// <summary>
        /// 编辑帮助中心问题
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/modify"), HttpPost]
        [Description("编辑帮助中心问题")]
        public object ModifyHelpcenterQuestion(ModifyHelpcenterQuestionRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyHelpcenterQuestion(request);
            return null;
        }

        /// <summary>
        /// 修改帮助中心问题状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/status/modify"), HttpPost]
        [Description("修改帮助中心问题状态")]
        public object ModifyHelpcenterQuestionStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyHelpcenterQuestionStatus(request);
            return null;
        }

        /// <summary>
        /// 修改帮助中心问题是否置顶
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/istop/modify"), HttpPost]
        [Description("修改帮助中心问题是否置顶")]
        public object ModifyHelpcenterQuestionIsTop(ModifyHelpcenterQuestionIsTopRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyHelpcenterQuestionIsTop(request);
            return null;
        }

        /// <summary>
        /// 修改帮助中心问题是否常见问题
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/iscommon/modify"), HttpPost]
        [Description("修改帮助中心问题是否常见问题")]
        public object ModifyHelpcenterQuestionIsCommon(ModifyHelpcenterQuestionIsCommonRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyHelpcenterQuestionIsCommon(request);
            return null;
        }

        /// <summary>
        /// 绑定问题所属分类
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/type/bind"), HttpPost]
        [Description("绑定问题所属分类")]
        public object BindHelpcenterQuestionType(BindHelpcenterQuestionTypeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.BindHelpcenterQuestionType(request);
            return null;
        }

        /// <summary>
        /// 删除帮助中心问题
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("helpcenter/question/delete"), HttpPost]
        [Description("删除帮助中心问题")]
        public object DeleteHelpcenterQuestion(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteHelpcenterQuestion(request);
            return null;
        }
        #endregion

        #region====广告管理====
        /// <summary>
        /// 查询banner广告分组列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/group/list"),HttpPost]
        [Description("查询banner广告分组列表")]
        public PageRes<BannerGroupInfo> GetBannerGroupList(GetBannerGroupListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return businessHandler.GetBannerGroupList(request);
        }

        /// <summary>
        /// 编辑banner广告分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/group/modify"), HttpPost]
        [Description("编辑banner广告分组")]
        public object ModifyBannerGroup(ModifyBannerGroupRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyBannerGroup(request);
            return null;
        }

        /// <summary>
        /// 修改banner广告分组状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/group/status/modify"), HttpPost]
        [Description("修改banner广告分组状态")]
        public object ModifyBannerGroupStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyBannerGroupStatus(request);
            return null;
        }

        /// <summary>
        /// 查询banner广告列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/list"), HttpPost]
        [Description("查询banner广告列表")]
        public PageRes<BannerInfo> GetBannerList(GetBannerListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetBannerList(request);
        }

        /// <summary>
        /// 添加banner广告
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/add"), HttpPost]
        [Description("添加banner广告")]
        public object AddBanner(AddBannerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddBanner(request);
            return null;
        }

        /// <summary>
        /// 编辑banner广告
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/modify"), HttpPost]
        [Description("编辑banner广告")]
        public object ModifyBanner(ModifyBannerRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyBanner(request);
            return null;
        }

        /// <summary>
        /// 修改banner广告状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/status/modify"), HttpPost]
        [Description("修改banner广告状态")]
        public object ModifyBannerStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyBannerStatus(request);
            return null;
        }

        /// <summary>
        /// 删除banner广告
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("banner/delete"), HttpPost]
        [Description("删除banner广告")]
        public object DeleteBanner(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteBanner(request);
            return null;
        }
        #endregion

        #region====通知管理====
        /// <summary>
        /// 查询头条列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("information/list"),HttpPost]
        [Description("查询头条列表")]
        public PageRes<InformationInfo> GetInformationList(GetInformationListRequest request) 
        {
            if (request == null)
            {
                throw new WebApiException(400,"参数错误");
            }
            return businessHandler.GetInformationList(request);
        }

        /// <summary>
        /// 添加头条
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("information/add"), HttpPost]
        [Description("添加头条")]
        public object AddInformation(AddInformationRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddInformation(request);
            return null;
        }

        /// <summary>
        /// 编辑头条
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("information/modify"), HttpPost]
        [Description("编辑头条")]
        public object ModifyInformation(ModifyInformationRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyInformation(request);
            return null;
        }

        /// <summary>
        /// 修改头条状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("information/status/modify"), HttpPost]
        [Description("修改头条状态")]
        public object ModifyInformationStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyInformationStatus(request);
            return null;
        }

        /// <summary>
        /// 删除头条
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("information/delete"), HttpPost]
        [Description("删除头条")]
        public object DeleteInformation(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteInformation(request);
            return null;
        }

        /// <summary>
        /// 查询消息分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/list"), HttpPost]
        [Description("查询消息分组")]
        public PageRes<MessageInfo> GetMessageList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetMessageList(request);
        }

        /// <summary>
        /// 添加消息分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/add"), HttpPost]
        [Description("添加消息分组")]
        public object AddMessage(AddMessageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddMessage(request);
            return null;
        }

        /// <summary>
        /// 编辑消息分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/modify"), HttpPost]
        [Description("编辑消息分组")]
        public object ModifyMessage(ModifyMessageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyMessage(request);
            return null;
        }

        /// <summary>
        /// 修改消息分组状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/status/modify"), HttpPost]
        [Description("修改消息分组状态")]
        public object ModifyMessageStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyMessageStatus(request);
            return null;
        }

        /// <summary>
        /// 删除消息分组
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/delete"), HttpPost]
        [Description("删除消息分组")]
        public object DeleteMessage(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteMessage(request);
            return null;
        }

        /// <summary>
        /// 查询消息模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/template/list"), HttpPost]
        [Description("查询消息模板")]
        public PageRes<MessageTemplateInfo> GetMessageTemplateList(GetMessageTemplateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetMessageTemplateList(request);
        }

        /// <summary>
        /// 添加消息模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/template/add"), HttpPost]
        [Description("添加消息模板")]
        public object AddMessageTemplate(AddMessageTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddMessageTemplate(request);
            return null;
        }

        /// <summary>
        /// 编辑消息模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/template/modify"), HttpPost]
        [Description("编辑消息模板")]
        public object ModifyMessageTemplate(ModifyMessageTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyMessageTemplate(request);
            return null;
        }

        /// <summary>
        /// 修改消息模板状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/template/status/modify"), HttpPost]
        [Description("修改消息模板状态")]
        public object ModifyMessageTemplateStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyMessageTemplateStatus(request);
            return null;
        }

        /// <summary>
        /// 删除消息模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("message/template/delete"), HttpPost]
        [Description("删除消息模板")]
        public object DeleteMessageTemplate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteMessageTemplate(request);
            return null;
        }

        /// <summary>
        /// 查询通知列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/list"), HttpPost]
        [Description("查询通知列表")]
        public PageRes<NoticeInfo> GetNoticeList(PageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetNoticeList(request);
        }

        /// <summary>
        /// 添加通知
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/add"), HttpPost]
        [Description("添加通知")]
        public object AddNotice(addNoticeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddNotice(request);
            return null;
        }

        /// <summary>
        /// 编辑通知
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/modify"), HttpPost]
        [Description("编辑通知")]
        public object ModifyNotice(ModifyNoticeRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyNotice(request);
            return null;
        }

        /// <summary>
        /// 修改通知状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/status/modify"), HttpPost]
        [Description("修改通知状态")]
        public object ModifyNoticeStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyNoticeStatus(request);
            return null;
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/delete"), HttpPost]
        [Description("删除通知")]
        public object DeleteNotice(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteNotice(request);
            return null;
        }

        /// <summary>
        /// 查询通知短信发送记录
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/sms/send/record"), HttpPost]
        [Description("查询通知短信发送记录")]
        public PageRes<NoticeSmsSendRecordInfo> GetNoticeSmsSendRecord(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetNoticeSmsSendRecord(request);
        }

        /// <summary>
        /// 查询通知短信发送详情
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/sms/send/details"), HttpPost]
        [Description("查询通知短信发送详情")]
        public PageRes<NoticeSmsSendDetailsInfo> GetNoticeSmsSendDetails(DetailsPageRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetNoticeSmsSendDetails(request);
        }

        /// <summary>
        /// 查询触发api列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("notice/api/list"), HttpPost]
        [Description("查询触发api列表")]
        public List<NoticeApiInfo> GetNoticeApiList()
        {
            return businessHandler.GetNoticeApiList();
        }

        /// <summary>
        /// 查询短信模板列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/template/list"), HttpPost]
        [Description("查询短信模板列表")]
        public PageRes<SmsTempInfo> GetSmsTemplateList(GetSmsTemplateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetSmsTemplateList(request);
        }

        /// <summary>
        /// 添加短信模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/template/add"), HttpPost]
        [Description("添加短信模板")]
        public object AddSmsTemplate(AddSmsTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddSmsTemplate(request);
            return null;
        }

        /// <summary>
        /// 编辑短信模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/template/modify"), HttpPost]
        [Description("编辑短信模板")]
        public object ModifySmsTemplate(ModifySmsTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifySmsTemplate(request);
            return null;
        }

        /// <summary>
        /// 修改短信模板状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/template/status/modify"), HttpPost]
        [Description("修改短信模板状态")]
        public object ModifySmsTemplateStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifySmsTemplateStatus(request);
            return null;
        }

        /// <summary>
        /// 删除短信模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/template/delete"), HttpPost]
        [Description("修改短信模板状态")]
        public object DeleteSmsTemplate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeleteSmsTemplate(request);
            return null;
        }

        /// <summary>
        /// 短信模板提交审核
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("sms/template/examine/apply"), HttpPost]
        [Description("短信模板提交审核")]
        public object SmsTempApplyExamine(DetailsRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.SmsTempApplyExamine(request);
            return null;
        }

        /// <summary>
        /// 查询推送模板列表
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/template/list"), HttpPost]
        [Description("查询推送模板列表")]
        public PageRes<PushTemplateInfo> GetPushTemplateList(GetPushTemplateListRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            return businessHandler.GetPushTemplateList(request);
        }

        /// <summary>
        /// 添加推送模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/template/add"), HttpPost]
        [Description("添加推送模板")]
        public object AddPushTemplate(AddPushTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.AddPushTemplate(request);
            return null;
        }

        /// <summary>
        /// 编辑推送模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/template/modify"), HttpPost]
        [Description("编辑推送模板")]
        public object ModifyPushTemplate(ModifyPushTemplateRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyPushTemplate(request);
            return null;
        }

        /// <summary>
        /// 修改推送模板状态
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/template/status/modify"), HttpPost]
        [Description("修改推送模板状态")]
        public object ModifyPushTemplateStatus(ModifyStatusRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.ModifyPushTemplateStatus(request);
            return null;
        }

        /// <summary>
        /// 删除推送模板
        /// </summary>
        /// <returns></returns>
        [CheckUserPowerFilter]
        [Route("push/template/delete"), HttpPost]
        [Description("删除推送模板")]
        public object DeletePushTemplate(DeleteRequest request)
        {
            if (request == null)
            {
                throw new WebApiException(400, "参数错误");
            }
            businessHandler.DeletePushTemplate(request);
            return null;
        }
        #endregion
    }
}
