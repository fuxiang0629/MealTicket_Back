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
    
    public partial class t_account_shares_entrust_follow
    {
        public long Id { get; set; }
        public long MainAccountId { get; set; }
        public long MainEntrustId { get; set; }
        public long FollowAccountId { get; set; }
        public long FollowEntrustId { get; set; }
        public System.DateTime CreateTime { get; set; }
    }
}
