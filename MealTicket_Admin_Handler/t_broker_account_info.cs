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
    
    public partial class t_broker_account_info
    {
        public long Id { get; set; }
        public string AccountCode { get; set; }
        public int AccountType { get; set; }
        public string AccountNo { get; set; }
        public string TradeAccountNo0 { get; set; }
        public string TradeAccountNo1 { get; set; }
        public string Holder0 { get; set; }
        public string Holder1 { get; set; }
        public string JyPassword { get; set; }
        public string TxPassword { get; set; }
        public int BrokerCode { get; set; }
        public int DepartmentCode { get; set; }
        public long InitialFunding { get; set; }
        public int Status { get; set; }
        public int SynchronizationStatus { get; set; }
        public Nullable<System.DateTime> SynchronizationTime { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public bool IsPrivate { get; set; }
    }
}
