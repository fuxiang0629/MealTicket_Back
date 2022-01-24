using MealTicket_Web_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetSharesPlateList
    {
    }

    public class SharesPlateInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }

        public int CalType { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 涨幅
        /// </summary>
        public long? RiseRate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 有效股票数量
        /// </summary>
        public int ValidCount { get; set; }

        /// <summary>
        /// 无效股票数量
        /// </summary>
        public int InValidCount { get; set; }

        /// <summary>
        /// 系统股票数量
        /// </summary>
        public int SysSharesCount { get; set; }

        /// <summary>
        /// 1允许买入 2不允许买入
        /// </summary>
        public int BuyStatus { get; set; }

        /// <summary>
        /// 账户买入板块设置Id
        /// </summary>
        public long BuyPlateId { get;set; }

        /// <summary>
        /// 涨停股票数量
        /// </summary>
        public int RiseLimitCount { get; set; }

        /// <summary>
        /// 跌停股票数量
        /// </summary>
        public int DownLimitCount { get; set; }

        /// <summary>
        /// 股票总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 涨跌幅排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 综合涨跌幅
        /// </summary>
        public int OverallRank { get; set; }

        public bool IsFocusOn { get; set; }

        public List<int> IsForce1 { get; set; }

        public List<int> IsForce2 { get; set; }

        public bool IsTrendLike { get; set; }
    }

    public class GetSharesPlateListRequest:DetailsPageRequest 
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 0所有 1所属行业 2所属地区 3所属概念
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 挑选状态0全部 1被挑选 2未被挑选
        /// </summary>
        public int ChooseStatus { get; set; }

        /// <summary>
        /// 不获取数量信息
        /// </summary>
        public bool NoGetCount { get; set; }

        /// <summary>
        /// 排序方式0时间 1.涨跌幅
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// ascending：升序  descending：降序
        /// </summary>
        public string OrderMethod { get; set; }

        /// <summary>
        /// 买入状态0全部 1可买入 2不可买入
        /// </summary>
        public int BuyStatus { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }
    }
}
