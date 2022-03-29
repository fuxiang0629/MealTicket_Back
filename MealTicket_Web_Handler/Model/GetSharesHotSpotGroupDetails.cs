using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHotSpotGroupDetails
    {
    }

    public class SharesHotSpotGroupDetails
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }


        public List<HotSpotInfo> HotSpotList { get; set; }
    }

    public class HotSpotInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
