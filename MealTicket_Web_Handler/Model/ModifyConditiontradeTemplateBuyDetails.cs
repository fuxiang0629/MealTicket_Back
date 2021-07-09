﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyConditiontradeTemplateBuyDetails
    {
    }

    public class ModifyConditiontradeTemplateBuyDetailsRequest
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
        /// 价格类型1绝对价格 2相对价格
        /// </summary>
        public int ConditionPriceType { get; set; }

        /// <summary>
        /// 基准价 1成本价 2当前价 3昨日收盘价
        /// </summary>
        public int? ConditionPriceBase { get; set; }

        /// <summary>
        /// 相对条件1.涨停 2跌停 3百分比
        /// </summary>
        public int? ConditionRelativeType { get; set; }

        /// <summary>
        /// 价格相对百分比 1/万
        /// </summary>
        public int? ConditionRelativeRate { get; set; }

        /// <summary>
        /// 是否大于价格
        /// </summary>
        public bool IsGreater { get; set; }

        /// <summary>
        /// 委托金额
        /// </summary>
        public int EntrustAmount { get; set; }

        /// <summary>
        /// 0相对金额 1绝对金额
        /// </summary>
        public int EntrustAmountType { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托价格档位0市价 1限价买一 2限价买二 3限价买三 4限价买四  5限价买五 6限价卖一 7限价卖二 8限价卖三 9限价卖四  10限价卖五 11涨停 12跌停
        /// </summary>
        public int EntrustPriceGear { get; set; }

        /// <summary>
        /// 禁止类型0无 1禁止涨停卖出
        /// </summary>
        public int ForbidType { get; set; }

        /// <summary>
        /// 是否自动买入
        /// </summary>
        public bool BuyAuto { get; set; }

        /// <summary>
        /// 子条件列表
        /// </summary>
        public List<ConditionChild> ChildList { get; set; }

        /// <summary>
        /// 是否判断涨停忽略
        /// </summary>
        public bool LimitUp { get; set; }

        /// <summary>
        /// 是否判断持仓不买入
        /// </summary>
        public bool IsHold { get; set; }

        /// <summary>
        /// 额外买入条件
        /// </summary>
        public int OtherConditionRelative { get; set; }
    }
}
