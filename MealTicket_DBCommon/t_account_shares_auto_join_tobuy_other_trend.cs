//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MealTicket_DBCommon
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_account_shares_auto_join_tobuy_other_trend
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
}
