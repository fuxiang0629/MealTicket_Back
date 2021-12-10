using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesBasePlateTag
    {
    }

    public class ModifySharesBasePlateTagRequest
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        public List<SharesBasePlateTagSetPar> ParList { get; set; }

        /// <summary>
        /// 是否允许自动修改
        /// </summary>
        public bool IsAuto { get; set; }
    }

    public class SharesBasePlateTagSetPar
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 是否重点关注
        /// </summary>
        public bool IsFocusOn { get; set; }

        /// <summary>
        /// 是否走势最像
        /// </summary>
        public bool IsTrendLike { get; set; }

        /// <summary>
        /// 强势上涨列表
        /// </summary>
        public List<ForceInfo> ForceList { get; set; }
    }
}
