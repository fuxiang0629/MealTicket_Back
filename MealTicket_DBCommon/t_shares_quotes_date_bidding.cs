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
    
    public partial class t_shares_quotes_date_bidding
    {
        public long Id { get; set; }
        public string Date { get; set; }
        public Nullable<long> DateKey { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public Nullable<long> SharesKey { get; set; }
        public long PresentPrice { get; set; }
        public long TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public int BuyCount1 { get; set; }
        public int PriceType { get; set; }
    }
}
