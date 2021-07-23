using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeBuyGroupSharesAccountList
    {
    }

    public class ConditiontradeBuyGroupSharesAccount
    {
        /// <summary>
        /// id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 是否下一批买入账户组
        /// </summary>
        public bool IsNextBuyGroup { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 买入账户数量
        /// </summary>
        public int AccountCount { get; set; }

        public long AuthorizedGroupId { get; set; }

        public string Mobile { get; set; }

        public string NickName { get; set; }

        public int Status { get; set; }
    }
}
