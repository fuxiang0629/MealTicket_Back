using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountBuyConditionOtherPar
    {
    }

    public class AccountBuyConditionOtherParInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 参数信息
        /// </summary>
        public string ParamsInfo { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    public class GetAccountBuyConditionOtherParPlateRequest:DetailsPageRequest
    {
        /// <summary>
        /// 1行业 2地区 3概念
        /// </summary>
        public int GroupType { get; set; }

        /// <summary>
        /// 1限制 2涨跌幅
        /// </summary>
        public int DataType { get; set; }
    }
}
