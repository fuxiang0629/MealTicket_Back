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
    
    public partial class t_account_shares_entrust
    {
        public long Id { get; set; }
        public long HoldId { get; set; }
        public int TradeType { get; set; }
        public long AccountId { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public int SharesHandCount { get; set; }
        public int Status { get; set; }
        public string StatusDes { get; set; }
        public int EntrustCount { get; set; }
        public int EntrustType { get; set; }
        public long EntrustPrice { get; set; }
        public long EntrustAmount { get; set; }
        public int DealCount { get; set; }
        public int SimulateDealCount { get; set; }
        public long DealPrice { get; set; }
        public long DealAmount { get; set; }
        public int CancelCount { get; set; }
        public long SoldProfitAmount { get; set; }
        public long FreezeDeposit { get; set; }
        public long BuyService { get; set; }
        public long ProfitService { get; set; }
        public long AdministrationFee { get; set; }
        public long Commission { get; set; }
        public long HandlingFee { get; set; }
        public long SettlementFee { get; set; }
        public long StampFee { get; set; }
        public long TransferFee { get; set; }
        public Nullable<System.DateTime> ClosingTime { get; set; }
        public bool IsJoinCanSold { get; set; }
        public bool IsJoinService { get; set; }
        public bool IsExecuteClosing { get; set; }
        public int Type { get; set; }
        public int HandlerType { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
