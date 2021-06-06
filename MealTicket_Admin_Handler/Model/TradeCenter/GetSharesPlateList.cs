using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesPlateList
    {
    }

    public class GetSharesPlateListRequest:PageRequest
    {
        /// <summary>
        /// 名称搜索
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 1.行业管理 2.地区管理 3.概念管理
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 排序方式0时间 1.涨跌幅
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// ascending：升序  descending：降序
        /// </summary>
        public string OrderMethod { get; set; }
    }

    public class SharesPlateInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public long? RiseRate { get; set; }

        /// <summary>
        /// 0指定股票 1代码匹配
        /// </summary>
        public int SharesType { get; set; }

        /// <summary>
        /// 匹配市场
        /// </summary>
        public int SharesMarket { get; set; }

        /// <summary>
        /// 匹配代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
