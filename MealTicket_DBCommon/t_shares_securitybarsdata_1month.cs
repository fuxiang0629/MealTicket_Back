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
    
    public partial class t_shares_securitybarsdata_1month
    {
        public long Id { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public long GroupTimeKey { get; set; }
        public System.DateTime Time { get; set; }
        public string TimeStr { get; set; }
        public long OpenedPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long PreClosePrice { get; set; }
        public long MinPrice { get; set; }
        public long MaxPrice { get; set; }
        public long TradeStock { get; set; }
        public long TradeAmount { get; set; }
        public long LastTradeStock { get; set; }
        public long LastTradeAmount { get; set; }
        public long Tradable { get; set; }
        public long TotalCapital { get; set; }
        public int HandCount { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
