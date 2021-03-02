﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountTrendTriDetails
    {
    }

    public class GetAccountTrendTriDetailsRequest
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }
    }

    public class AccountTrendTriDetails 
    {
        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 触发类型
        /// </summary>
        public string TrendName { get; set; }

        /// <summary>
        /// 触发价格
        /// </summary>
        public long TrendPrice { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
