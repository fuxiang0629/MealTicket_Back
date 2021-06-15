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
        /// 0 所有 1.行业管理 2.地区管理 3.概念管理
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 0全部 1被挑选 2未被挑选
        /// </summary>
        public int ChooseStatus { get; set; }

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
        /// 板块类型
        /// </summary>
        public int Type { get; set; }

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
        /// 指数
        /// </summary>
        public long? RiseIndex { get; set; }

        /// <summary>
        /// 加权涨幅
        /// </summary>
        public long? WeightRiseRate { get; set; }

        /// <summary>
        /// 加权指数
        /// </summary>
        public long? WeightRiseIndex { get; set; }

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
        /// 加权指数市场
        /// </summary>
        public int WeightMarket { get; set; }

        /// <summary>
        /// 加权指数code
        /// </summary>
        public string WeightSharesCode { get; set; }

        /// <summary>
        /// 加权指数名称
        /// </summary>
        public string WeightSharesName { get; set; }

        /// <summary>
        /// 不加权指数市场
        /// </summary>
        public int NoWeightMarket { get; set; }

        /// <summary>
        /// 不加权指数code
        /// </summary>
        public string NoWeightSharesCode { get; set; }

        /// <summary>
        /// 不加权指数名称
        /// </summary>
        public string NoWeightSharesName { get; set; }

        /// <summary>
        /// 计算类型0未知 1板块指数 2市场指数 3成分指数
        /// </summary>
        public int CalType { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
