using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddSharesPlate
    {
    }

    public class AddSharesPlateRequest 
    {
        /// <summary>
        /// 1.行业管理 2区域管理 3概念管理
        /// </summary>
        public int Type { get; set; }

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
    }
}
