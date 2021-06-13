using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesPlate
    {
    }

    public class ModifySharesPlateRequest
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
        /// 0指定股票 2代码匹配
        /// </summary>
        public int SharesType { get; set; }

        /// <summary>
        /// 匹配市场
        /// </summary>
        public int SharesMarket { get; set; }

        /// <summary>
        /// 匹配代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 加权指数市场
        /// </summary>
        public int WeightMarket { get; set; }

        /// <summary>
        /// 加权指数code
        /// </summary>
        public string WeightSharesCode { get; set; }

        /// <summary>
        /// 不加权指数市场
        /// </summary>
        public int NoWeightMarket { get; set; }

        /// <summary>
        /// 计算类型0未知 1板块指数 2市场指数 3成分指数
        /// </summary>
        public int CalType { get; set; }

        /// <summary>
        /// 不加权指数code
        /// </summary>
        public string NoWeightSharesCode { get; set; }
    }
}
