using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesTagSettingDetails
    {
    }

    public class ModifySharesTagSettingDetailsRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 基础数量
        /// </summary>
        public int BaseCount { get; set; }

        /// <summary>
        /// 目标数量
        /// </summary>
        public int DisCount { get; set; }
    }
}
