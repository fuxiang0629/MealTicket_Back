using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetConditiontradeTemplateBuyDetailsList
    {
    }

    public class GetConditiontradeTemplateBuyDetailsListRequest : PageRequest
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }
    }

    public class ConditiontradeTemplateBuyDetailsInfo
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
        public int? ConditionPriceType { get; set; }

        /// <summary>
        /// 基准价 1成昨日收盘价 2当前价
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
        /// 状态1有效 2无效 3条件触发
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 子条件列表
        /// </summary>
        public List<ConditionChild> ChildList { get; set; }

        /// <summary>
        /// 额外条件数量
        /// </summary>
        public int OtherConditionCount { get; set; }

        /// <summary>
        /// 转自动条件数量
        /// </summary>
        public int AutoConditionCount { get; set; }

        /// <summary>
        /// 是否自动买入
        /// </summary>
        public bool BuyAuto { get; set; }

        /// <summary>
        /// 前端用
        /// </summary>
        public long DisPrice { get; set; }

        /// <summary>
        /// 前端用
        /// </summary>
        public long DisAmount { get; set; }

        /// <summary>
        /// 前端用
        /// </summary>
        public double DisAmountRate { get; set; }

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

        public long AllAmount { get; set; }

        public int AllCount { get; set; }
    }
}
