using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class SharesPlateInfo_Session
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
        /// 板块类型
        /// </summary>
        public int PlateType { get; set; }

        /// <summary>
        /// 基础板块状态
        /// </summary>
        public int BaseStatus { get; set; }

        /// <summary>
        /// 业务板块状态
        /// </summary>
        public int ChooseStatus { get; set; }

        /// <summary>
        /// 是否包含基础板块
        /// </summary>
        public int IsBasePlate { get; set; }

        public int WeightMarket { get; set; }

        public string WeightSharesCode { get; set; }
        
        public int NoWeightMarket { get; set; }

        public string NoWeightSharesCode { get; set; }

        public DateTime? BaseDate { get; set; }
    }
}
