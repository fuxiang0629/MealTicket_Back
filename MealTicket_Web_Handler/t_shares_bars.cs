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
    
    public partial class t_shares_bars
    {
        public long Id { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public System.DateTime Time { get; set; }
        public string TimeStr { get; set; }
        public int Type { get; set; }
        public long OpenedPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long MaxPrice { get; set; }
        public long MinPrice { get; set; }
        public int Volume { get; set; }
        public long Turnover { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
