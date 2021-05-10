using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class AddAccountBuySettingPar
    {
    }

    public class AddAccountBuySettingParRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long SettingId { get; set; }

        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 最大可购买数量
        /// </summary>
        public long MaxBuyCount { get; set; }
    }
}
