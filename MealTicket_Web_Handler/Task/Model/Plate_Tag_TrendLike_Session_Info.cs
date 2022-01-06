using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Plate_Tag_TrendLike_Session_Info
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
        /// 是否走势最相似
        /// </summary>
        public bool IsTrendLike { get; set; }

        /// <summary>
        /// 分数
        /// </summary>
        public int Score { get; set; }
    }

    public class Plate_Tag_TrendLike_Session_Obj
    {
        public Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> Plate_Shares_TrendLike_Session { get; set; }

        public Dictionary<long, Dictionary<long, Plate_Tag_TrendLike_Session_Info>> Shares_Plate_TrendLike_Session { get; set; }
    }
}
