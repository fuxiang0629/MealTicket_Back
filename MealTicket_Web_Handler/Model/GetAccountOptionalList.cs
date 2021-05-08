using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountOptionalList
    {
    }

    public class GetAccountOptionalListRequest:PageRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }
    }

    public class AccountOptionalInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

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
        /// 已绑定席位Id
        /// </summary>
        public long SeatId { get; set; }

        /// <summary>
        /// 已绑定席位名称
        /// </summary>
        public string SeatName { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public string ValidTime { get; set; }

        /// <summary>
        /// 所属行业
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// 业务范围
        /// </summary>
        public string Business { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
