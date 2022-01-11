using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Shares_Rel_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        public long SharesKey 
        {
            get 
            {
                return long.Parse(SharesCode) * 10 + Market;
            }
        }
    }

    public class Plate_Shares_Rel_Session_Obj
    {
        public Dictionary<long, List<Plate_Shares_Rel_Session_Info>> Plate_Shares_Rel_Session { get; set; }
        public Dictionary<long, List<Plate_Shares_Rel_Session_Info>> Shares_Plate_Rel_Session { get; set; }
    }
}
