using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesConditionTrendParPlate
    {
    }

    public class GetSharesConditionTrendParPlatePageRequest:DetailsPageRequest
    {
        /// <summary>
        /// 总分组类型（1行业 2地区 3概念）
        /// </summary>
        public int GroupType { get; set; }

        /// <summary>
        /// 数据类型1限制 2涨跌幅
        /// </summary>
        public int DataType { get; set; }
    }
}
