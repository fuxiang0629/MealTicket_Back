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
    
    public partial class t_account_wallet_change_record
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public int Type { get; set; }
        public long PreAmount { get; set; }
        public long ChangeAmount { get; set; }
        public long CurAmount { get; set; }
        public string Description { get; set; }
        public System.DateTime CreateTime { get; set; }
        public long ContextId { get; set; }
        public int ContextType { get; set; }
    }
}
