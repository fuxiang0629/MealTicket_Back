using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetPlateKLineList
    {
    }

    public class GetPlateKLineListRequest
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 加权类型1不加权 2总股本加权
        /// </summary>
        public int WeightType { get; set; }

        /// <summary>
        /// 类型1.1分钟线 2.5分钟线 3.15分钟线 4.30分钟线 5.60分钟线 6.日线 7.周线 8.月线 9.季度线 10.年线
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 最小时间
        /// </summary>
        public long MinGroupTimeKey { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public long MaxGroupTimeKey { get; set; }

        /// <summary>
        /// 获取条数
        /// </summary>
        public int GetCount { get; set; }
    }

    public class GetPlateKLineListRes
    {
        /// <summary>
        /// 类型1最新数据（覆盖） 2最新数据累加 3以前数据累加
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 板块Id
        /// SharesCode*10+Market
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<PlateKLineInfo> DataList { get; set; }
    }

    public class PlateKLineInfo
    {
        /// <summary>
        /// 时间值
        /// </summary>
        public string DateKey { get; set; }

        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public string OpenedPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public string ClosedPrice { get; set; }

        /// <summary>
        /// 上一时段收盘价
        /// </summary>
        public string PreClosedPrice { get; set; }


        /// <summary>
        /// 最低价
        /// </summary>
        public string MinPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public string MaxPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TradeStockNum { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public string TradeStock { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public string TradeAmount { get; set; }

        /// <summary>
        /// 流通量
        /// </summary>
        public string Tradable { get; set; }

        /// <summary>
        /// 换手率
        /// </summary>
        public string TradableRate { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public string RiseRate { get; set; }
    }
}
