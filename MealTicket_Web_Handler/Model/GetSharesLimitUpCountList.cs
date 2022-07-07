using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesLimitUpCountList
    {
    }

    public class GetSharesLimitUpCountListRequest
    {
        public DateTime Date { get; set; }
    }

    public class SharesLimitUpCountGroup
    {
        public int RiseLimitCount { get; set; }

        public List<SharesLimitUpCountInfo> List { get; set; }
    }

    public class SharesLimitUpCountInfo
    {
        public long SharesKey { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public int RiseLimitDays { get; set; }

        public int RiseLimitCount { get; set; }
    }
}
