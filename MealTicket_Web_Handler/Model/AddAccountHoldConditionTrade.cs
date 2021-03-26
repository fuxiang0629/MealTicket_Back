﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountHoldConditionTrade
    {
    }

    public class AddAccountHoldConditionTradeRequest
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 持仓Id
        /// </summary>
        public long HoldId { get; set; }

        /// <summary>
        /// 类型1.定时卖出 2.止盈卖出 3.止亏卖出
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 时间条件
        /// </summary>
        public DateTime? ConditionTime { get; set; }

        /// <summary>
        /// 价格条件
        /// </summary>
        public long? ConditionPrice { get; set; }

        /// <summary>
        /// 交易类型1买入 2卖出
        /// </summary>
        public int TradeType { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 禁止类型0无 1禁止涨停卖出
        /// </summary>
        public int ForbidType { get; set; }

        /// <summary>
        /// 委托价格档位0市价 1限价买一 2限价买二 3限价买三 4限价买四  5限价买五 6限价卖一 7限价卖二 8限价卖三 9限价卖四  10限价卖五
        /// </summary>
        public int EntrustPriceGear { get; set; }

        /// <summary>
        /// 子条件列表
        /// </summary>
        public List<ConditionChild> ChildList { get; set; }
    }

    public class ConditionChild
    {
        /// <summary>
        /// 子条件
        /// </summary>
        public long ChildId { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }
}
