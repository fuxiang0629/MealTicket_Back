﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    [Serializable]
    public class Shares_Tag_Leader_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 类型1.3天  2.5天  3.10天  4.15天
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 龙头类型
        /// </summary>
        public int LeaderType { get; set; }
    }
}
