using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_FocusOn_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 是否重点关注
        /// </summary>
        public bool IsFocusOn { get; set; }
    }

    public class Plate_Tag_FocusOn_Session_Obj 
    {
        public Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> Plate_Shares_FocusOn_Session { get; set; }
        public Dictionary<long, Dictionary<long, Plate_Tag_FocusOn_Session_Info>> Shares_Plate_FocusOn_Session { get; set; }
    }
}
