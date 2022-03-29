using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
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
        /// 指数类型0未知 1板块指数 2市场指数 3成分指数
        /// </summary>
        public int CalType { get; set; }

        /// <summary>
        /// 基础板块状态
        /// </summary>
        public int BaseStatus { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        public int RiseRate { get; set; }
    }
}
