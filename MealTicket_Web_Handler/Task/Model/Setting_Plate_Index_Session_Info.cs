using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Setting_Plate_Index_Session_Info
    {
        /// <summary>
        /// 类型1龙头指数 2板块联动 3股票联动 4新高指数
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ValueJson { get; set; }

        /// <summary>
        /// 系数
        /// </summary>
        public int Coefficient { get; set; }

        /// <summary>
        /// 新高指数计算天数
        /// </summary>
        public int CalDays { get; set; }

        /// <summary>
        /// 新高指数计算价格类型1开盘价 2收盘价 3最高价 4最低价
        /// </summary>
        public int CalPriceType { get; set; }
    }
}
