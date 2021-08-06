using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ExportAccountAutoTobuy
    {
    }

    public class ExportAccountAutoTobuyRequest
    {
        /// <summary>
        /// 加入模板类型1用户模板 2系统模板
        /// </summary>
        public int TemplateType { get; set; }

        /// <summary>
        /// 加入模板Id
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 买入模板类型1用户模板 2系统模板
        /// </summary>
        public int BuyTemplateType { get; set; }

        /// <summary>
        /// 买入模板Id
        /// </summary>
        public long BuyTemplateId { get; set; }

        /// <summary>
        /// 跟投类型0指定账户 1分组轮询
        /// </summary>
        public int FollowType { get; set; }

        /// <summary>
        /// 跟投账户列表
        /// </summary>
        public List<long> FollowAccountList { get; set; }

        /// <summary>
        /// 自定义分组列表
        /// </summary>
        public List<long> GroupIdList { get; set; }

        /// <summary>
        /// 应用分组Id
        /// </summary>
        public List<long> UseGroupIdList { get; set; }
    }
}
