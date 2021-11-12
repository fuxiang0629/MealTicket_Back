using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.SecurityBarsData
{
    /// <summary>
    /// 板块指数计算结果
    /// </summary>
    public class PlateKlineSession
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
        /// 时间值
        /// </summary>
        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 总开盘价
        /// </summary>
        public long TotalOpenedPrice { get; set; }

        /// <summary>
        /// 总收盘价
        /// </summary>
        public long TotalClosedPrice { get; set; }

        /// <summary>
        /// 总昨日收盘价
        /// </summary>
        public long TotalPreClosePrice { get; set; }

        /// <summary>
        /// 总昨日收盘价(1分钟k线用)
        /// </summary>
        public long TotalYestodayClosedPrice { get; set; }

        /// <summary>
        /// 总最低价
        /// </summary>
        public long TotalMinPrice { get; set; }

        /// <summary>
        /// 总最高价
        /// </summary>
        public long TotalMaxPrice { get; set; }

        /// <summary>
        /// 总成交量
        /// </summary>
        public long TotalTradeStock { get; set; }

        /// <summary>
        /// 总成交额
        /// </summary>
        public long TotalTradeAmount { get; set; }

        /// <summary>
        /// 之前成交量之和(1分钟k线用)
        /// </summary>
        public long TotalLastTradeStock { get; set; }

        /// <summary>
        /// 之前成交额之和(1分钟k线用)
        /// </summary>
        public long TotalLastTradeAmount { get; set; }

        /// <summary>
        /// 计算股票的数量
        /// </summary>
        public int CalCount { get; set; }

        /// <summary>
        /// 是否更新变动
        /// </summary>
        public bool IsUpdate { get; set; }
    }

    /// <summary>
    /// 股票K线实时数据
    /// </summary>
    public class SharesKlineData
    {
        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        public long SharesCodeNum 
        {
            get 
            {
                return long.Parse(SharesCode);
            }
        }

        /// <summary>
        /// 时间值
        /// </summary>
        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 成交量(笔)
        /// </summary>
        public long TradeStock { get; set; }

        /// <summary>
        /// 成交额(元*10000)
        /// </summary>
        public long TradeAmount { get; set; }

        /// <summary>
        /// 上一时段收盘价
        /// </summary>
        public long PreClosePrice { get; set; }

        /// <summary>
        /// 前时刻成交量之和
        /// </summary>
        public long LastTradeStock { get; set; }

        /// <summary>
        /// 前时刻成交额之和
        /// </summary>
        public long LastTradeAmount { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long YestodayClosedPrice { get; set; }

        /// <summary>
        /// 流通市值
        /// </summary>
        public long Tradable { get; set; }

        /// <summary>
        /// 总市值
        /// </summary>
        public long TotalCapital { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 加权类型
        /// </summary>
        public int WeightType { get; set; }

    }

    /// <summary>
    /// 股票K线实时数据容器
    /// </summary>
    public class SharesKlineDataContain 
    {
        /// <summary>
        /// K线类型
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 计算类型0板块需要自己计算 1板块不许计算
        /// </summary>
        public int CalType { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SharesKlineData> SharesKlineData { get; set; }
    }

    /// <summary>
    /// 股票信息
    /// </summary>
    public class SharesData 
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }

    /// <summary>
    /// 基准日信息缓存
    /// </summary>
    public class PlateBaseDataSession 
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 基准日
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 基准日加权市值平均
        /// </summary>
        public long WeightPrice { get; set; }

        /// <summary>
        /// 基准日不加权价格平均
        /// </summary>
        public long NoWeightPrice { get; set; }
    }

    public class PlateKlineData: SharesKlineData 
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 加权类型
        /// </summary>
        public int WeightType { get; set; }
    }
}
