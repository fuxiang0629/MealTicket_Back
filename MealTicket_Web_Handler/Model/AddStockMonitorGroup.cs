﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddStockMonitorGroup
    {
    }

    public class AddStockMonitorGroupRequest
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
