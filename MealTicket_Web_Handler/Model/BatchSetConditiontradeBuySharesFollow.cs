using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BatchSetConditiontradeBuySharesFollow
    {
    }

    public class BatchSetConditiontradeBuySharesFollowRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 执行状态0全部 1已执行 2未执行
        /// </summary>
        public int ExecStatus { get; set; }

        /// <summary>
        /// 行业分组名称
        /// </summary>
        public long GroupId1 { get; set; }

        /// <summary>
        /// 地区分组名称
        /// </summary>
        public long GroupId2 { get; set; }

        /// <summary>
        /// 概念分组名称
        /// </summary>
        public long GroupId3 { get; set; }

        /// <summary>
        /// 自定义分组名称
        /// </summary>
        public long GroupId4 { get; set; }

        /// <summary>
        /// 跟投列表
        /// </summary>
        public List<long> FollowList { get; set; }
    }
}
