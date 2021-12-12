using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesBasePlateList
    {
    }

    public class GetSharesBasePlateListRequest
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }
    }

    public class GetSharesBasePlateListRes
    {
        /// <summary>
        /// 是否允许自动修改
        /// </summary>
        public bool IsAuto { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<SharesBasePlateInfo> List { get; set; }
    }

    public class SharesBasePlateInfo
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 板块名称
        /// </summary>
        public string PlateName { get; set; }

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

    public class ForceInfo
    {
        /// <summary>
        /// 板块Id
        /// </summary>
        public long PlateId { get; set; }

        /// <summary>
        /// 强势类型1.3天 2.5天 3.10天 4.15天
        /// </summary>
        public int ForceType { get; set; }

        /// <summary>
        /// 是否强力1
        /// </summary>
        public bool IsForce1 { get; set; }

        /// <summary>
        /// 是否强力2
        /// </summary>
        public bool IsForce2 { get; set; }
    }
}
