using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountOptionalList
    {
    }

    public class GetFrontAccountOptionalListRequest:DetailsPageRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }
    }

    public class FrontAccountOptionalInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
        /// 是否有效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
