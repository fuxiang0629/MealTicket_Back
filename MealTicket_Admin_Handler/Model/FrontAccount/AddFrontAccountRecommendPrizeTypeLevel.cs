﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddFrontAccountRecommendPrizeTypeLevel
    {
    }

    public class AddFrontAccountRecommendPrizeTypeLevelRequest
    {
        /// <summary>
        /// 类别Id
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// 级别值
        /// </summary>
        public int LevelValue { get; set; }

        /// <summary>
        /// 奖励比例
        /// </summary>
        public int PrizeRate { get; set; }
    }
}
