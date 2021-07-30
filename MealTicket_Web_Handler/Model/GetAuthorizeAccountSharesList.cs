using MealTicket_Web_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    class GetAuthorizeAccountSharesList
    {
    }

    public class AuthorizeAccountSharesInfo
    {
        /// <summary>
        /// 条件买入Id
        /// </summary>
        public long ConditiontradeBuyId { get; set; }

        /// <summary>
        /// 市场代码
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
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 当前价格
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 涨跌幅(1/万)
        /// </summary>
        public int RiseRate
        {
            get
            {
                if (ClosedPrice <= 0 || CurrPrice <= 0)
                {
                    return 0;
                }
                return (int)((CurrPrice - ClosedPrice)*1.0/ ClosedPrice*10000);
            }
        }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice 
        { 
            get 
            {
                if (ClosedPrice <= 0 || CurrPrice <= 0)
                {
                    return 0;
                }
                return CurrPrice - ClosedPrice;
            } 
        }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo> PlateList { get; set; }
    }
}
