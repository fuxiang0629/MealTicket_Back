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
    
    public partial class t_shares_hotspot_highmark_operation
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public System.DateTime Date { get; set; }
        public long SharesKey { get; set; }
        public int AutoType { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public int RiseLimitCountCustom { get; set; }
    }
}
