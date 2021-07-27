using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetAuthorizeAccountSharesList
    {
    }

    public class AuthorizeAccountSharesInfo
    {
        /// <summary>
        /// 条件买入Id
        /// </summary>
        public long ConditiontradeBuyId { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
