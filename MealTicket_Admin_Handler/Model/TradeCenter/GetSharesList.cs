using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesList
    {
    }

    public class GetSharesListRequest:PageRequest
    {
        /// <summary>
        /// 股票代码/股票名称
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 禁止状态0不禁 1禁止买入 2禁止卖出 3全禁止
        /// </summary>
        public int ForbidStatus { get; set; }

        /// <summary>
        /// 是否返回实时详情
        /// </summary>
        public bool ShowSharesQuotes { get; set; }
    }

    public class SharesInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
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
        /// 拼音简称
        /// </summary>
        public string SharesNamePY { get; set; }

        /// <summary>
        /// 公司上市时间
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
        /// 业务范围
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 禁止状态0不禁 1禁止买入 2禁止卖出 3全禁止
        /// </summary>
        public int ForbidStatus { get; set; }

        /// <summary>
        /// 是否停牌
        /// </summary>
        public bool IsSuspension { get; set; }

        /// <summary>
        /// 实时详情
        /// </summary>
        public SharesQuotes SharesQuotes { get; set; }
    }

    public class SharesQuotes 
    {
        /// <summary>
        /// 现价
        /// </summary>
        public long PresentPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public long MaxPrice { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public long MinPrice { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public double Rise
        {
            get
            {
                if (ClosedPrice <= 0 || PresentPrice <= 0)
                {
                    return 0;
                }
                return Math.Round((PresentPrice - ClosedPrice) * 100.0 / ClosedPrice, 2);
            }
        }

        /// <summary>
        /// 总量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 现量
        /// </summary>
        public int PresentCount { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public long TotalAmount { get; set; }
    }
}
