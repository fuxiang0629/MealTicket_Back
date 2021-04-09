using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountSharesPlateSharesList
    {
    }

    public class GetAccountSharesPlateSharesListRequest : DetailsPageRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 0系统分组 1自定义分组
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 执行状态0全部 1已执行 2未执行
        /// </summary>
        public int ExecStatus { get; set; }
    }
}
