using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountBuyCondition
    {
    }

    public class ModifyAccountBuyConditionRequest
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
        public int ConditionType { get; set; }

        /// <summary>
        /// 相对条件1.涨停 2跌停 3百分比
        /// </summary>
        public int ConditionRelativeType { get; set; }

        /// <summary>
        /// 价格相对百分比 1/万
        /// </summary>
        public int ConditionRelativeRate { get; set; }


        /// <summary>
        /// 条件价格
        /// </summary>
        public long ConditionPrice { get; set; }

        /// <summary>
        /// 是否大于价格
        /// </summary>
        public bool IsGreater { get; set; }

        /// <summary>
        /// 买入金额
        /// </summary>
        public long EntrustAmount { get; set; }

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
        /// 是否自动买入
        /// </summary>
        public bool BuyAuto { get; set; }

        /// <summary>
        /// 跟投用户Id列表
        /// </summary>
        public List<long> FollowAccountList { get; set; }

        /// <summary>
        /// 子条件列表
        /// </summary>
        public List<ConditionChild> ChildList { get; set; }

        /// <summary>
        /// 是否忽略当前涨停
        /// </summary>
        public bool LimitUp { get; set; }
    }
}
