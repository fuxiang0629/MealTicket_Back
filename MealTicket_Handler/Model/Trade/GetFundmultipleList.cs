using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetFundmultipleList
    {
    }

    public class FundmultipleInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long FundmultipleId { get; set; }

        /// <summary>
        /// 市场名称
        /// </summary>
        public string MarketName { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        public int FundMultiple { get; set; }

        /// <summary>
        /// 最大倍数
        /// </summary>
        public int MaxFundMultiple { get; set; }
    }
}
