using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPlateSharesLinkageAllList
    {
    }

    public class PlateSharesLinkageAllInfo
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
        /// 板块列表
        /// </summary>
        public List<SharesNameInfo> SharesList { get; set; }
    }

    public class SharesNameInfo
    {
        /// <summary>
        /// 股票市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        public long SharesNum
        { 
            get 
            {
                return long.Parse(SharesCode) * 10 + Market;
            } 
        }
    }
}
