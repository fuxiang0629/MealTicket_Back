using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    [Serializable]
    public class Plate_Shares_Rel_Tag_Setting_Session_Info
    {
        /// <summary>
        /// 计算类型
        /// </summary>
        public int SettingType { get; set; }

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
