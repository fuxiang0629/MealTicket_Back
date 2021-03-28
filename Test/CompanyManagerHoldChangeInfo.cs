using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class CompanyManagerHoldChangeInfo
    {
        /// <summary>
        /// 股份变动人
        /// </summary>
        public string ChangePerson { get; set; }

        /// <summary>
        /// 变动日期
        /// </summary>
        public string ChangeDate { get; set; }

        /// <summary>
        /// 变动数量(股)
        /// </summary>
        public string ChangeCount { get; set; }

        /// <summary>
        /// 成交价格
        /// </summary>
        public string DealPrice { get; set; }

        /// <summary>
        /// 变动后持股(股)
        /// </summary>
        public string RemainHoldCount { get; set; }

        /// <summary>
        /// 变动原因
        /// </summary>
        public string ChangeReason { get; set; }

        /// <summary>
        /// 高管名称
        /// </summary>
        public string ManagerName { get; set; }

        /// <summary>
        /// 职务
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 关系
        /// </summary>
        public string Relation { get; set; }
    }
}
