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
    
    public partial class t_shares_quotes_history
    {
        public long Id { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public string Date { get; set; }
        public int Type { get; set; }
        public int BusinessType { get; set; }
        public System.DateTime CreateTime { get; set; }
        public long PresentPrice { get; set; }
        public string SharesInfo { get; set; }
    }
}
