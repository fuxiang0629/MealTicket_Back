using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesHighMarkList
    {
    }

    public class GetSharesHighMarkListRequest
    {
        
    }

    public class SharesHighMarkInfo
    {
        public DateTime Date { get; set; }

        public string DateStr 
        {
            get 
            {
                return Date.ToString("yyyy-MM-dd");
            }
        }

        public List<HighMarkShares> SharesList { get; set; }

        public List<HighMarkShares> ShowSharesList { get; set; }

    }

    public class HighMarkShares 
    {
        public long SharesKey { get; set; }

        public int Market { get; set; }

        public string SharesCode { get; set; }

        public string SharesName { get; set; }

        public int RiseRate { get; set; }

        public bool IsSuspension { get; set; }

        public int PriceType { get; set; }

        public int LimitUpCount { get; set; }

        public int LimitUpCountReal { get; set; }

        public int RiseLimitDays { get; set; }
    }
}
