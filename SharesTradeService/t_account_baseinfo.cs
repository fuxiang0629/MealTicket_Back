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
    
    public partial class t_account_baseinfo
    {
        public long Id { get; set; }
        public string Mobile { get; set; }
        public string NickName { get; set; }
        public string HeadUrl { get; set; }
        public int Sex { get; set; }
        public string BirthDay { get; set; }
        public string RecommandCode { get; set; }
        public Nullable<long> ReferId { get; set; }
        public int ForbidStatus { get; set; }
        public int Status { get; set; }
        public string QrCodeUrl { get; set; }
        public string RegisterPageUrl { get; set; }
        public string PostersUrl { get; set; }
        public string PostersComposeUrl { get; set; }
        public string LoginPassword { get; set; }
        public string TransactionPassword { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
        public int CashStatus { get; set; }
        public int MonitorStatus { get; set; }
    }
}
