using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesAllotRecord
    {
    }

    public class FrontAccountSharesAllotRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场0深圳 1上海
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
        /// 派股信息
        /// </summary>
        public string AllotSharesInfo { get; set; }

        /// <summary>
        /// 派息信息
        /// </summary>
        public string AllotBonusInfo { get; set; }

        /// <summary>
        /// 派息日期
        /// </summary>
        public DateTime AllotDate { get; set; }

        /// <summary>
        /// 处理状态1未处理 2处理中 3已处理
        /// </summary>
        public int HandleStatus { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? HandleTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
