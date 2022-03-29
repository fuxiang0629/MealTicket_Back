using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesPlateOrderList
    {
    }

    public class GetSharesPlateOrderListRequest
    {
        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }
    }

    public class GetSharesPlateOrderListRes
    {
        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public List<PlateSharesRank> PlateSharesRank { get; set; }
    }
}
