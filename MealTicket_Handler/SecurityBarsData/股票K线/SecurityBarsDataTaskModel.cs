using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.SecurityBarsData
{
    public class SecurityBarsDataTaskQueueInfo
    {
        /// <summary>
        /// 任务Guid
        /// </summary>
        public string TaskGuid { get; set; }

        /// <summary>
        /// 总包数量
        /// </summary>
        public int TotalPacketCount { get; set; }

        /// <summary>
        /// 回调包数量
        /// </summary>
        public int CallBackPacketCount { get; set; }

        /// <summary>
        /// 数据包扔出时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int TaskTimeOut { get; set; }

        /// <summary>
        /// 处理类型1当天实时数据 2历史补充数据
        /// </summary>
        public int HandlerType { get; set; }

        /// <summary>
        /// 数据包列表
        /// </summary>
        public Dictionary<int,SecurityBarsDataType> PackageList { get; set; }

        /// <summary>
        /// 已重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 可重试次数
        /// </summary>
        public int TotalRetryCount { get; set; }

        /// <summary>
        /// 重试股票数据列表
        /// </summary>
        public List<SecurityBarsDataParList> FailPackageList { get; set; }

        /// <summary>
        /// 成功股票数据列表
        /// </summary>
        public List<SecurityBarsDataParList> SuccessPackageList { get; set; }
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
    }

    public class SecurityBarsDataType
    {
        /// <summary>
        /// 数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 数据列表最大下标
        /// </summary>
        public int DataIndex { get; set; }

        public long GroupTimeKey { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SecurityBarsDataInfo> DataList { get; set; }
    }

    public class SecurityBarsDataInfo
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj.GetType().Equals(this.GetType())) == false)
            {
                return false;
            }
            SecurityBarsDataInfo temp = (SecurityBarsDataInfo)obj;

            return this.Market.Equals(temp.Market) && this.SharesCode.Equals(temp.SharesCode) && this.GroupTimeKey.Equals(temp.GroupTimeKey);
        }

        public override int GetHashCode()
        {
            return this.Market.GetHashCode() + this.SharesCode.GetHashCode() + this.GroupTimeKey.GetHashCode();
        }

        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 分组时段
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

        public DateTime? Date 
        {
            get
            {
                if (Time == null)
                {
                    return null;
                }
                return Time.Value.Date;
            }
        }

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
        /// 前一分钟成交量之和
        /// </summary>
        public long LastTradeStock { get; set; }

        /// <summary>
        /// 前一分钟成交额之和
        /// </summary>
        public long LastTradeAmount { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long YestodayClosedPrice { get; set; }

        public long PlateId { get; set; }

        public int WeightType { get; set; }

        public long Tradable { get; set; }

        public long TotalCapital { get; set; }

        public bool IsVaild { get; set; }
    }

    public class SecurityBarsData_1minInfo
    {
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public DateTime Date{get;set;} 
        public DateTime Time { get; set; }
        public string TimeStr { get; set; }
        public long OpenedPrice { get; set; }
        public long ClosedPrice { get; set; }
        public long PreClosePrice { get; set; }
        public long MinPrice { get;set; }
        public long MaxPrice { get; set; }
        public int TradeStock { get; set; }
        public long TradeAmount { get; set; }
	    public long Tradable { get; set; }
        public long TotalCapital { get; set; }
        public int HandCount { get; set; }
    }

    public class QueueMsgObj
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int MsgId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public object MsgObj { get; set; }
    }

    public class SecurityBarsOtherData
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public DateTime Date { get; set; }

        public DateTime GroupTime { get; set; }

        public DateTime Time { get; set; }

        public string TimeStr { get; set; }

        public long OpenedPrice { get; set; }

        public long ClosedPrice { get; set; }

        public long PreClosePrice { get; set; }

        public long MinPrice { get; set; }

        public long MaxPrice { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }

        public int LastTradeStock { get; set; }

        public long LastTradeAmount { get; set; }

        public long Tradable { get; set; }

        public long TotalCapital { get; set; }

        public int HandCount { get; set; }

        public DateTime LastModified { get; set; }
    }

    public class SecurityBarsMaxTime
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public DateTime MaxTime { get; set; }

        public long Tradable { get; set; }

        public long TotalCapital { get; set; }

        public int HandCount { get; set; }
    }

    public class MinutetimeDataInfo
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public DateTime Date { get; set; }

        public DateTime Time { get; set; }

        public string TimeStr { get; set; }

        public long Price { get; set; }

        public long AvgPrice { get; set; }

        public long PreClosePrice { get; set; }

        public long TradeStock { get; set; }

        public long TradeAmount { get; set; }

        public long TotalTradeStock { get; set; }

        public long TotalTradeAmount { get; set; }

        public long Tradable { get; set; }

        public long TotalCapital { get; set; }

        public int HandCount { get; set; }

        public DateTime LastModified { get; set; }
    }

    public class MinutetimeToDataBaseInfo
    {
        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string Date { get; set; }

        public int HandlerType { get; set; }

        public override int GetHashCode()
        {
            return this.Market.GetHashCode() + this.SharesCode.GetHashCode() + this.Date.GetHashCode() + this.HandlerType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj.GetType().Equals(this.GetType())) == false)
            {
                return false;
            }
            MinutetimeToDataBaseInfo temp = (MinutetimeToDataBaseInfo)obj;
            return this.Market.Equals(temp.Market) && this.SharesCode.Equals(temp.SharesCode) && this.Date.Equals(temp.Date) && this.HandlerType.Equals(temp.HandlerType);
        }
    }

    public class SecurityBarsLastData
    {
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj.GetType().Equals(this.GetType())) == false)
            {
                return false;
            }
            SecurityBarsDataInfo temp = (SecurityBarsDataInfo)obj;

            return this.PlateId.Equals(temp.PlateId) && this.WeightType.Equals(temp.WeightType);
        }

        public override int GetHashCode()
        {
            return this.PlateId.GetHashCode() + this.WeightType.GetHashCode();
        }
        public long PlateId { get; set; }
        public int WeightType { get; set; }
        public int Market { get; set; }
        public string SharesCode { get; set; }
        public long GroupTimeKey { get; set; }
        public long PreClosePrice { get; set; }
        public long YestodayClosedPrice { get; set; }
        public long LastTradeStock { get; set; }
        public long LastTradeAmount { get; set; }
    }

    public class SecurityBarsLastDataGroup
    {
        public int DataType { get; set; }

        public long GroupTimeKey { get; set; }

        public List<SecurityBarsLastData> DataList { get; set; }
    }


    /// <summary>
    /// 板块内股票快照缓存
    /// </summary>
    public class PlateRelSnapshotInfo
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
    }

    public class PlateCloseInfo
    {
        public long PlateId { get; set; }

        public int WeightType { get; set; }

        public long ClosePrice { get; set; }
    }
}
