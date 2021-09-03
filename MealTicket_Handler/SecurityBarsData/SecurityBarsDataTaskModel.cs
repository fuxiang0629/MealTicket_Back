﻿using System;
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
        /// 数据列表最大下标
        /// </summary>
        public int DataIndex { get; set; }

        /// <summary>
        /// 数据类型 2.1分钟K线 3.5分钟K线 4.15分钟K线 5.30分钟K线 6.60分钟K线 7.日K 8.周K 9.月K 10.季度K 11.年K
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 处理类型1当天实时数据 2历史补充数据
        /// </summary>
        public int HandlerType { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SecurityBarsDataInfo> DataList { get; set; }
    }

    public class SecurityBarsDataInfo
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
        public int TradeStock { get; set; }

        /// <summary>
        /// 成交额(元*10000)
        /// </summary>
        public long TradeAmount { get; set; }

        /// <summary>
        /// 上一时段收盘价
        /// </summary>
        public long PreClosePrice { get; set; }
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
}
