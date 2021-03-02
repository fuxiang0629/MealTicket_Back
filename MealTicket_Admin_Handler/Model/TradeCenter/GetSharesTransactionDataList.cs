using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesTransactionDataList
    {
    }

    public class GetSharesTransactionDataListRequest : PageRequest 
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
        /// 查询起始日期
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 查询截止日期
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class SharesTransactionDataInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string TimeDate
        {
            get
            {
                return Time.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 时间值
        /// </summary>
        public string TimeStr { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public long Price { get; set; }

        /// <summary>
        /// 笔数
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 类型0买入 1卖出 2中性
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}
