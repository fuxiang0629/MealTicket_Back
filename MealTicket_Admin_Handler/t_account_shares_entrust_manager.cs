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
    
    public partial class t_account_shares_entrust_manager
    {
        public long Id { get; set; }
        public long BuyId { get; set; }
        public string TradeAccountCode { get; set; }
        public string EntrustId { get; set; }
        public System.DateTime EntrustTime { get; set; }
        public int EntrustCount { get; set; }
        public long EntrustPrice { get; set; }
        public int DealCount { get; set; }
        public long DealPrice { get; set; }
        public long DealAmount { get; set; }
        public int CancelCount { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public int TradeType { get; set; }
        public int EntrustType { get; set; }
        public string StatusDes { get; set; }
        public int RealEntrustCount { get; set; }
        public int SimulateDealCount { get; set; }
        public bool CanClear { get; set; }
    }
}
