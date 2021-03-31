using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountBuyConditionTradeSharesGroupList
    {
    }

    public class AccountBuyConditionTradeSharesGroupInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }
    }

    public class GetAccountBuyConditionTradeSharesGroupListRequest:DetailsPageRequest 
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
