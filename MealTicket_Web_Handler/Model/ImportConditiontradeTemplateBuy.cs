using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ImportConditiontradeTemplateBuy
    {
    }

    public class ImportConditiontradeTemplateBuyRequest
    {
        /// <summary>
        /// 导入目标
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 1客户模板 2系统模板
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否清除
        /// </summary>
        public bool IsClear { get; set; }

        /// <summary>
        /// 股票列表
        /// </summary>
        public List<string> SharesList { get; set; }

        /// <summary>
        /// 跟投类型0指定账户 1分组轮询
        /// </summary>
        public int FollowType { get; set; }

        /// <summary>
        /// 跟投用户
        /// </summary>
        public List<long> FollowList { get; set; }

        /// <summary>
        /// 1单个导入（可修改价格） 2批量导入
        /// </summary>
        public int Model { get; set; }

        /// <summary>
        /// 详细数据
        /// </summary>
        public List<ConditiontradeTemplateBuyDetailsInfo> TemplateDetailsList { get; set; }
    }

    public class template_buy
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public string Name { get; set; }
        public int ConditionType { get; set; }
        public Nullable<int> ConditionPriceBase { get; set; }
        public Nullable<int> ConditionPriceType { get; set; }
        public Nullable<int> ConditionPriceRate { get; set; }
        public bool IsGreater { get; set; }
        public int EntrustAmount { get; set; }
        public int EntrustType { get; set; }
        public int EntrustPriceGear { get; set; }
        public int ForbidType { get; set; }
        public bool BuyAuto { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public bool LimitUp { get; set; }
        public int OtherConditionRelative { get; set; }
        public bool IsHold { get; set; }
    }

    public class buy_child
    {
        public long Id { get; set; }
        public long FatherId { get; set; }
        public long ChildId { get; set; }
        public int Status { get; set; }
    }

    public class buy_other
    {
        public long Id { get; set; }
        public long TemplateBuyId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_other_trend
    {
        public long Id { get; set; }
        public long OtherId { get; set; }
        public long TrendId { get; set; }
        public string TrendName { get; set; }
        public string TrendDescription { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_other_trend_par
    {
        public long Id { get; set; }
        public long OtherTrendId { get; set; }
        public string ParamsInfo { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_other_trend_other
    {
        public long Id { get; set; }
        public long OtherTrendId { get; set; }
        public long TrendId { get; set; }
        public string TrendName { get; set; }
        public string TrendDescription { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_other_trend_other_par
    {
        public long Id { get; set; }
        public long OtherTrendOtherId { get; set; }
        public string ParamsInfo { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_auto
    {
        public long Id { get; set; }
        public long TemplateBuyId { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_auto_trend
    {
        public long Id { get; set; }
        public long AutoId { get; set; }
        public long TrendId { get; set; }
        public string TrendName { get; set; }
        public string TrendDescription { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_auto_trend_par
    {
        public long Id { get; set; }
        public long AutoTrendId { get; set; }
        public string ParamsInfo { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_auto_trend_other
    {
        public long Id { get; set; }
        public long AutoTrendId { get; set; }
        public long TrendId { get; set; }
        public string TrendName { get; set; }
        public string TrendDescription { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }

    public class buy_auto_trend_other_par
    {
        public long Id { get; set; }
        public long AutoTrendOtherId { get; set; }
        public string ParamsInfo { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
