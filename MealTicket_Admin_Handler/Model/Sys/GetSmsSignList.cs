using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSmsSignList
    {
    }

    public class GetSmsSignListRequest:PageRequest
    {
        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 应用key
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 审核状态0.全部 1.等待审核 2.审核中 3.认证成功 4认证失败
        /// </summary>
        public int ExamineStatus { get; set; }
    }

    public class SmsSignInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string SignName { get; set; }

        /// <summary>
        /// 营业执照
        /// </summary>
        public string LicenceUrl { get; set; }

        /// <summary>
        /// 营业执照展示
        /// </summary>
        public string LicenceUrlShow 
        {
            get 
            {
                if (string.IsNullOrEmpty(LicenceUrl) || LicenceUrl.ToUpper().StartsWith("HTTP"))
                {
                    return "";
                }
                return ConfigurationManager.AppSettings["imgurl"];
            }
        }

        /// <summary>
        /// 审核状态1.等待审核 2.审核中 3.认证成功 4认证失败
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 审核失败原因
        /// </summary>
        public string ExamineFailReason { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// AppName
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 第三方签名Id
        /// </summary>
        public string ThirdSignId { get; set; }

        /// <summary>
        /// 申请审核时间
        /// </summary>
        public DateTime? ApplyTime { get; set; }

        /// <summary>
        /// 审核结果时间
        /// </summary>
        public DateTime? ExamineTime { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
