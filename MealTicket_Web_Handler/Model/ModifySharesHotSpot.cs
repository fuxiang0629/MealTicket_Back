using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifySharesHotSpot
    {
    }

    public class ModifySharesHotSpotRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 是否显示背景色
        /// </summary>
        public bool ShowBgColor { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public string BgColor { get; set; }

        /// <summary>
        /// 板块Id列表
        /// </summary>
        public List<long> PlateIdList { get; set; }

        /// <summary>
        /// 分组Id列表
        /// </summary>
        public List<long> GroupIdList { get; set; }
    }
}
