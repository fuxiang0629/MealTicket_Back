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
    
    public partial class t_sys_conditiontrade_template_join
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public string Name { get; set; }
        public bool IsClearOriginal { get; set; }
        public int TimeCycleType { get; set; }
        public int TimeCycle { get; set; }
        public int Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
