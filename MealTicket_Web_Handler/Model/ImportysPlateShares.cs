using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ImportysPlateShares
    {
    }

    public class ImportysPlateSharesRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public List<string> KeyWord { get; set; }

        /// <summary>
        /// 自定义分组
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 排除其他分组存在的股票
        /// </summary>
        public bool RemoveOtherGroup { get; set; }
    }
}
