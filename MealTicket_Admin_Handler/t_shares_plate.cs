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
    
    public partial class t_shares_plate
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public int SharesType { get; set; }
        public int SharesMarket { get; set; }
        public string SharesCode { get; set; }
        public int ChooseStatus { get; set; }
        public int WeightMarket { get; set; }
        public string WeightSharesCode { get; set; }
        public int NoWeightMarket { get; set; }
        public string NoWeightSharesCode { get; set; }
        public int CalType { get; set; }
    }
}
