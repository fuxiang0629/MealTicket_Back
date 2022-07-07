using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityBarsDataUpdate
{
    public class SecurityBarsDataRes
    {
        /// <summary>
        /// 数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SecurityBarsDataInfo> DataList { get; set; }
    }

    public class SecurityBarsDataInfo
    {
        /// <summary>
        /// 数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 时间段
        /// </summary>
        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string TimeStr { get; set; }

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
        /// 是否最后一条
        /// </summary>
        public bool IsLast { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long YestodayClosedPrice { get; set; }

        /// <summary>
        /// 前一分钟成交量之和
        /// </summary>
        public long LastTradeStock { get; set; }

        /// <summary>
        /// 前一分钟成交额之和
        /// </summary>
        public long LastTradeAmount { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 加权类型1不加权 2总股本加权
        /// </summary>
        public int WeightType { get; set; }

        /// <summary>
        /// 是否有效数据（排除停牌和未上市）
        /// </summary>
        public bool IsVaild { get; set; }

        /// <summary>
        /// 是否错误数据
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// 1涨停 2跌停
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb { get; set; }

        /// <summary>
        /// 是否撬板
        /// </summary>
        public bool IsLimitDownBomb { get; set; }
    }

    public class SecurityBarsDataTaskInfo
    {
        /// <summary>
        /// 任务Guid
        /// </summary>
        public string TaskGuid { get; set; }

        /// <summary>
        /// 处理类型1当天实时数据 2历史补充数据 3指数当天实时数据 4指数历史补充数据
        /// </summary>
        public int HandlerType { get; set; }

        /// <summary>
        /// 以重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 总共可重试次数
        /// </summary>
        public int TotalRetryCount { get; set; }

        /// <summary>
        /// 数据包列表
        /// </summary>
        public List<SecurityBarsDataParList> PackageList { get; set; }
    }

    public class SecurityBarsDataParList
    {
        /// <summary>
        /// 每次获取数量（最大800）
        /// </summary>
        public int SecurityBarsGetCount { get; set; }

        /// <summary>
        /// 数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 股票数据列表
        /// </summary>
        public List<SecurityBarsDataPar> DataList { get; set; }
    }

    public class SecurityBarsDataPar
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 每次获取数量（最大800）
        /// </summary>
        public int SecurityBarsGetCount { get; set; }

        /// <summary>
        /// 获取数据起始时间（-1表示无开始时间）
        /// </summary>
        public long StartTimeKey { get; set; }

        /// <summary>
        /// 获取数据截止时间（-1表示无截止时间）
        /// </summary>
        public long EndTimeKey { get; set; }

        /// <summary>
        /// 上一时段收盘价
        /// </summary>
        public long PreClosePrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long YestodayClosedPrice { get; set; }

        /// <summary>
        /// 前一分钟成交量之和
        /// </summary>
        public long LastTradeStock { get; set; }

        /// <summary>
        /// 前一分钟成交额之和
        /// </summary>
        public long LastTradeAmount { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 加权类型1不加权 2总股本加权
        /// </summary>
        public int WeightType { get; set; }

        /// <summary>
        /// 涨跌停状态
        /// </summary>
        public int PriceType { get; set; }

        /// <summary>
        /// 是否炸板
        /// </summary>
        public bool IsLimitUpBomb { get; set; }

        /// <summary>
        /// 是否撬板
        /// </summary>
        public bool IsLimitDownBomb { get; set; }
    }

    public class MinutetimeData
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string TimeStr { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public long Price { get; set; }

        /// <summary>
        /// 均价
        /// </summary>
        public long AvgPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TradeStock { get; set; }

        /// <summary>
        /// 成交额
        /// </summary>
        public long TradeAmount { get; set; }
    }


    public class TaskDataInfo 
    {
        public SecurityBarsDataPar Data { get; set; }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public int HandlerType { get; set; }

        /// <summary>
        /// 信号量
        /// </summary>
        public Semaphore semap { get; set; }

        /// <summary>
        /// 等待句柄
        /// </summary>
        public ManualResetEvent WatiHandle { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 失败数据包
        /// </summary>
        public Dictionary<int, SecurityBarsDataParList> FailPackage { get; set; }

        public object FailLock { get; set; }

        /// <summary>
        /// 成功数据包
        /// </summary>
        public Dictionary<int, SecurityBarsDataParList> SuccessPackage { get; set; }

        public object SuccessLock { get; set; }

        /// <summary>
        /// 结果集
        /// </summary>
        public Dictionary<int, SecurityBarsDataRes> ResultList { get; set; }

        public Dictionary<int,int> downCounter { get; set; }

        public object DownLoadLock { get; set; }
    }
}
