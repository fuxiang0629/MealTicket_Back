//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MealTicket_Web_Handler
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_account_shares_conditiontrade_buy_details
    {
        public long Id { get; set; }
        public long ConditionId { get; set; }
        public string Name { get; set; }
        public int ConditionType { get; set; }
        public long ConditionPrice { get; set; }
        public bool IsGreater { get; set; }
        public int ConditionRelativeType { get; set; }
        public int ConditionRelativeRate { get; set; }
        public long EntrustAmount { get; set; }
        public int EntrustType { get; set; }
        public int EntrustPriceGear { get; set; }
        public Nullable<System.DateTime> TriggerTime { get; set; }
        public int ForbidType { get; set; }
        public int SourceFrom { get; set; }
        public int Status { get; set; }
        public long EntrustId { get; set; }
        public bool BuyAuto { get; set; }
        public int BusinessStatus { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public long CreateAccountId { get; set; }
        public int ExecStatus { get; set; }
        public bool LimitUp { get; set; }
        public Nullable<System.DateTime> FirstExecTime { get; set; }
        public int OtherConditionRelative { get; set; }
        public bool IsHold { get; set; }
        public int FollowType { get; set; }
        public string EntrustErrorDes { get; set; }
    }
}
