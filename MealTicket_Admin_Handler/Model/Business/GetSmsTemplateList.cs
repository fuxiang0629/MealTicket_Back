using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSmsTemplateList
    {
    }

    public class GetSmsTemplateListRequest : PageRequest
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

    public class SmsTempInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string TempName { get; set; }

        /// <summary>
        /// 模板类型2通知类 3营销类
        /// </summary>
        public int TempType { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        public string TempContent { get; set; }

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
        /// 第三方模板Id
        /// </summary>
        public string ThirdTempId { get; set; }

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
