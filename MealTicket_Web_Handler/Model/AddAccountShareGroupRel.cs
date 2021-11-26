using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountShareGroupRel
    {
    }

    public class AddAccountShareGroupRelRequest
    { 
        /// <summary>
        /// 分组Id列表
        /// </summary>
        public List<long> GroupIdList { get; set; }

        /// <summary>
        /// 是否清除原来分组
        /// </summary>
        public bool IsClearSource { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }
}
