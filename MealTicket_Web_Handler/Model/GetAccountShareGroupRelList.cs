using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountShareGroupRelList
    {
    }

    public class GetAccountShareGroupRelListPageRequest:DetailsPageRequest
    {
        /// <summary>
        /// 排序方式
        /// </summary>
        public string OrderMethod { get; set; }

        /// <summary>
        /// 排序字段1时间 2涨幅
        /// </summary>
        public int OrderType { get; set; }
    }

    public class GetAccountShareGroupRelListRes
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 股票列表
        /// </summary>
        public List<AccountShareGroupRelInfo> List { get; set; }

        /// <summary>
        /// 分组内股票数量列表
        /// </summary>
        public List<AccountShareGroupCountInfo> GroupSharesCountList { get; set; }
    }
    public class AccountShareGroupRelInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// 市场
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
        /// 涨跌幅(1/万)
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (ClosedPrice == 0 || CurrPrice == 0)
                {
                    return 0;
                }
                return (int)((CurrPrice - ClosedPrice)*1.0/ ClosedPrice*10000+0.5);
            }
        }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice 
        {
            get
            {
                if (ClosedPrice == 0 || CurrPrice == 0)
                {
                    return 0;
                }
                return CurrPrice - ClosedPrice;
            }
        }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 条件买入Id
        /// </summary>
        public long ConditionId { get; set; }

        /// <summary>
        /// 条件买入状态
        /// </summary>
        public long ConditionStatus { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 自定义分组
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }
    }

    public class AccountShareGroupCountInfo
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }
    }
}
