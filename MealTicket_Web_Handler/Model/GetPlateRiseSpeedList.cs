using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateRiseSpeedList
    {
    }

    public class PlateRiseSpeedInfo
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

        public string PlateNameOnly { get; set; }

        /// <summary>
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 板块排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 涨停数量
        /// </summary>
        public int RiseUpLimitCount { get; set; }

        /// <summary>
        /// 跌停数量
        /// </summary>
        public int DownUpLimitCount { get; set; }

        /// <summary>
        /// 股票总数量
        /// </summary>
        public int SharesTotalCount { get; set; }

        /// <summary>
        /// 涨速
        /// </summary>
        public MinuteRiseInfo MinuteRise { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }
    }
}
