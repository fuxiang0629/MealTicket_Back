using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    [Serializable]
    public class Plate_Quotes_Session_Info
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 收盘价格
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 昨日收盘价格
        /// </summary>
        public long YestodayClosedPrice { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate 
        {
            get
            {
                if (YestodayClosedPrice == 0)
                {
                    return 0;
                }
                return (int)((ClosedPrice - YestodayClosedPrice) * 1.0 / YestodayClosedPrice * 10000 + 0.5);
            }
        }

        /// <summary>
        /// 真实天数（计算板块排名用）
        /// </summary>
        public int RealDays { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public int Rank { get; set; }
    }
}
