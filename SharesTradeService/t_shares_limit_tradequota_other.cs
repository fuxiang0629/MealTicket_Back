//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SharesTradeService
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_shares_limit_tradequota_other
    {
        public long Id { get; set; }
        public long QuotaId { get; set; }
        public string AccountCode { get; set; }
        public int Rate { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
