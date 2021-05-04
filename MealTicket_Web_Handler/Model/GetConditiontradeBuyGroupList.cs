using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeBuyGroupList
    {
    }

    public class GetConditiontradeBuyGroupListRequest:DetailsPageRequest
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型0我的分组 1共享给我的分组
        /// </summary>
        public int Type { get; set; }
    }

    public class ConditiontradeBuyGroupInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 有效股票数量
        /// </summary>
        public int ValidCount { get; set; }

        /// <summary>
        /// 无效股票数量
        /// </summary>
        public int InValidCount { get; set; }

        /// <summary>
        /// 分享用户数量
        /// </summary>
        public int SharesAccountCount { get; set; }

        /// <summary>
        /// 今天最大购买次数
        /// </summary>
        public int MaxBuyCount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
