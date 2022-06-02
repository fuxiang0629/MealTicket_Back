﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetStockSearchMarkList
    {
    }

    public class GetStockSearchMarkListRequest
    {
        public List<long> Id { get; set; }
    }

    public class StockSearchMarkTri
    {
        public long SharesKey { get; set; }

        public List<StockSearchMarkTriTemplateInfo> TemplateList { get; set; }
    }

    public class StockSearchMarkTriTemplateInfo 
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }
    }
}

