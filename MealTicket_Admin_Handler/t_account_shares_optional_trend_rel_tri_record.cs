//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MealTicket_Admin_Handler
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_account_shares_optional_trend_rel_tri_record
    {
        public long Id { get; set; }
        public long OptionalId { get; set; }
        public long RelId { get; set; }
        public long AccountId { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public string SharesName { get; set; }
        public long TrendId { get; set; }
        public string TrendName { get; set; }
        public string TrendDescription { get; set; }
        public long TriPrice { get; set; }
        public bool IsPush { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string TriDesc { get; set; }
    }
}
