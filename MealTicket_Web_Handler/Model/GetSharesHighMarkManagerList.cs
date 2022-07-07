using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHighMarkManagerList
    {
    }

    public class GetSharesHighMarkManagerListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
    }

    public class SharesHighMarkManagerInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 股票唯一值
        /// </summary>
        public long SharesKey { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        /// <summary>
        /// 自动/手动
        /// </summary>
        public int AutoType { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否符合
        /// </summary>
        public bool IsFit { get; set; }

        /// <summary>
        /// 是否今天数据
        /// </summary>
        public bool IsToday { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 虚拟连板数
        /// </summary>
        public int RiseLimitCountCustom { get; set; }
    }
}
