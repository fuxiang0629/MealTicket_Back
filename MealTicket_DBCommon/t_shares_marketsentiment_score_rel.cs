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
    
    public partial class t_shares_marketsentiment_score_rel
    {
        public long Id { get; set; }
        public long GroupTimeKey { get; set; }
        public int RealScore { get; set; }
        public int MinScore { get; set; }
        public int MaxScore { get; set; }
        public System.DateTime CreateTime { get; set; }
    }
}
