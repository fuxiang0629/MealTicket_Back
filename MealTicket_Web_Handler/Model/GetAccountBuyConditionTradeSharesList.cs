﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountBuyConditionTradeSharesList
    {
    }

    public class GetAccountBuyConditionTradeSharesListRequest : DetailsPageRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 执行状态0全部 1已执行 2未执行
        /// </summary>
        public int ExecStatus { get; set; }

        /// <summary>
        /// 行业分组名称
        /// </summary>
        public long GroupId1 { get; set; }

        /// <summary>
        /// 地区分组名称
        /// </summary>
        public long GroupId2 { get; set; }

        /// <summary>
        /// 概念分组名称
        /// </summary>
        public long GroupId3 { get; set; }

        /// <summary>
        /// 自定义分组名称
        /// </summary>
        public long GroupId4 { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string OrderMethod { get; set; }

        /// <summary>
        /// 排序字段1时间 2涨幅
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 获取等级
        /// </summary>
        public int Level { get; set; }
    }

    public class AccountBuyConditionTradeSharesInfo
    {
        /// <summary>
        /// 条件买入Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 条件买入状态
        /// </summary>
        public int ConditionStatus { get; set; }

        /// <summary>
        /// 关系Id
        /// </summary>
        public long RelId { get; set; }

        /// <summary>
        /// 自定义分组Id
        /// </summary>
        public List<long> GroupList { get; set; }

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
        /// 涨跌幅(1/万)
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 总参数数量
        /// </summary>
        public int ParTotalCount { get; set; }

        /// <summary>
        /// 有效参数数量
        /// </summary>
        public int ParValidCount { get; set; }

        /// <summary>
        /// 已执行参数数量
        /// </summary>
        public int ParExecuteCount { get; set; }

        /// <summary>
        /// 最大涨跌幅
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// 杠杆倍数
        /// </summary>
        public int Fundmultiple { get; set; }

        /// <summary>
        /// 上市状态1正常 2退市
        /// </summary>
        public int MarketStatus { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }

        /// <summary>
        /// 是否存在股票监控列表
        /// </summary>
        public bool IsExists { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public long OpenedPrice { get; set; }

        /// <summary>
        /// 换手率
        /// </summary>
        public int TodayHandsRate
        {
            get
            {
                if (CirculatingCapital == 0)
                {
                    return 0;
                }
                return (int)((TodayDealCount * 1.0 / CirculatingCapital) * 10000);
            }
        }

        /// <summary>
        /// 流通股本
        /// </summary>
        public long CirculatingCapital { get; set; }

        /// <summary>
        /// 成交金额
        /// </summary>
        public long TodayDealAmount { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public long TodayDealCount { get; set; }

        /// <summary>
        /// 自选股分组数量
        /// </summary>
        public List<long> MySharesGroupList { get; set; }
    }
}
