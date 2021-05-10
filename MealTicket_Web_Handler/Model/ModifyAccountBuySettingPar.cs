using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountBuySettingPar
    {
    }

    public class ModifyAccountBuySettingParRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 最大购买数量
        /// </summary>
        public long MaxBuyCount { get; set; }
    }
}
