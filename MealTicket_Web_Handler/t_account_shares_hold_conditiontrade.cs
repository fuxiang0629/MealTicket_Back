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
    
    public partial class t_account_shares_hold_conditiontrade
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long HoldId { get; set; }
        public int Type { get; set; }
        public Nullable<System.DateTime> ConditionTime { get; set; }
        public Nullable<long> ConditionPrice { get; set; }
        public int TradeType { get; set; }
        public int EntrustCount { get; set; }
        public int EntrustType { get; set; }
        public int EntrustPriceGear { get; set; }
        public Nullable<System.DateTime> TriggerTime { get; set; }
        public int SourceFrom { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public string Name { get; set; }
        public long FatherId { get; set; }
        public int ForbidType { get; set; }
        public int Status { get; set; }
        public long EntrustId { get; set; }
    }
}
