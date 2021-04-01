using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    public class MarketTimeInfo
    {
        /// <summary>
        /// 市场0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 上市时间
        /// </summary>
        public DateTime MarketTime { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 所属地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 所属概念
        /// </summary>
        public string Idea { get; set; }

        /// <summary>
        /// 主营业务
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 监控Id
        /// </summary>
        public long MonitorId { get; set; }
    }
}
