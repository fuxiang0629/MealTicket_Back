//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MealTicket_Handler
{
    using System;
    using System.Collections.Generic;
    
    public partial class t_recharge_record
    {
        public long Id { get; set; }
        public string OrderSN { get; set; }
        public long AccountId { get; set; }
        public string ChannelCode { get; set; }
        public string ChannelName { get; set; }
        public long RechargeAmount { get; set; }
        public long PayAmount { get; set; }
        public long RefundedMoney { get; set; }
        public bool SupportRefund { get; set; }
        public int PayStatus { get; set; }
        public Nullable<System.DateTime> RechargedTime { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string PayStatusDes { get; set; }
        public long PaymentAccountId { get; set; }
    }
}
