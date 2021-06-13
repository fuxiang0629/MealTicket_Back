//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SharesHqService
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_shares_quotes_date
    {
        public long Id { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public long PresentPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long OpenedPrice { get; set; }
        public long MaxPrice { get; set; }
        public long MinPrice { get; set; }
        public long LimitUpPrice { get; set; }
        public long LimitDownPrice { get; set; }
        public System.DateTime LastModified { get; set; }
        public string Date { get; set; }
        public int LimitUpCount { get; set; }
        public int LimitDownCount { get; set; }
        public int LimitUpBombCount { get; set; }
        public int LimitDownBombCount { get; set; }
        public int PriceType { get; set; }
        public int TriLimitUpCount { get; set; }
        public int TriLimitDownCount { get; set; }
        public int TriLimitUpBombCount { get; set; }
        public int TriLimitDownBombCount { get; set; }
        public int TriPriceType { get; set; }
        public long TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public int TriNearLimitType { get; set; }
    }
}
