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
        /// 0系统分组 1自定义分组
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 分组Id
        /// </summary>
        public long PlateId { get; set; }
    }

    public class AccountBuyConditionTradeSharesInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
    }
}
