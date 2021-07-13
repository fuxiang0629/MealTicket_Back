using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountOptionalGroup
    {
    }

    public class AccountOptionalGroupInfo
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDescription { get; set; }

        /// <summary>
        /// 分组排序值
        /// </summary>
        public long OrderIndex { get; set; }

        /// <summary>
        /// 股票数量
        /// </summary>
        public int SharesCount { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public bool GroupIsContinue { get; set; }

        public bool GroupChecked { get; set; }

        public string ValidTime { get; set; }

        public string lastPushTime { get; set; }
    }
}
