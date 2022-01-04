using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class Plate_Base_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        /// <summary>
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 基础板块状态
        /// </summary>
        public int BaseStatus { get; set; }

        /// <summary>
        /// 计算类型0未知 1板块指数 2市场指数 3成分指数
        /// </summary>
        public int CalType { get; set; }

        /// <summary>
        /// 加权股票市场代码
        /// </summary>
        public int WeightMarket { get; set; }

        /// <summary>
        /// 加权股票代码
        /// </summary>
        public string WeightSharesCode { get; set; }

        /// <summary>
        /// 不加权股票市场代码
        /// </summary>
        public int NoWeightMarket { get; set; }

        /// <summary>
        /// 不加权股票代码
        /// </summary>
        public string NoWeightSharesCode { get; set; }

        /// <summary>
        /// 基准日
        /// </summary>
        public DateTime? BaseDate { get; set; }
    }
}
