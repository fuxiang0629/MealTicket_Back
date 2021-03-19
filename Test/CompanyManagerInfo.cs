using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class CompanyManagerInfo
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }

        /// <summary>
        /// 职务
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 学历
        /// </summary>
        public string Education { get; set; }

        /// <summary>
        /// 年薪
        /// </summary>
        public string AnnualSalary{get;set;}

        /// <summary>
        /// 持股数
        /// </summary>
        public string HoldCount { get; set; }

        /// <summary>
        /// 任期开始日
        /// </summary>
        public string StartDate { get; set; }
    }
}
