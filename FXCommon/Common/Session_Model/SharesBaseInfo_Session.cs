﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class SharesBase_Session
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        public List<bool> IsSuccess { get; set; }

        public bool Equals(SharesBase_Session x)
        {
            if (x == null)
            {
                return false;
            }
            return (x.Market == this.Market && x.SharesCode == this.SharesCode);
        }

        public int GetHashCode()
        {
            return this.Market.GetHashCode() + this.SharesCode.GetHashCode();
        }
    }

    public class SharesBaseInfo_Session:SharesBase_Session
    {
        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 股票拼音
        /// </summary>
        public string SharesPyjc { get; set; }

        /// <summary>
        /// 一手股数
        /// </summary>
        public int SharesHandCount { get; set; }

        /// <summary>
        /// 营业范围
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 总股本
        /// </summary>
        public long TotalCapital { get; set; }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

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
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 上市状态
        /// </summary>
        public int MarketStatus { get; set; }

        /// <summary>
        /// 上市时间
        /// </summary>
        public DateTime? MarketTime { get; set; }

    }
}
