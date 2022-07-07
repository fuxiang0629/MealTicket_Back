using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesPremiumStatisticResultList
    {
    }

    public class GetSharesPremiumStatisticResultListRequest
    {
        public DateTime Date { get; set; }

        public int PriceType { get; set; }
    }

    public class SharesPremiumStatisticResultInfo
    {
        public long ConditionId { get; set; }

        public string ConditionName { get; set; }

        public DateTime Date { get; set; }

        public int SharesCountToday { get; set; }

        public int SharesCountYes { get; set; }

        public int PremiumRateToday { get; set; }

        public string PremiumRate3daysPar { get; set; }

        public int PremiumRate3days { get; set; }

        public string PremiumRate5daysPar { get; set; }

        public int PremiumRate5days { get; set; }

        public DateTime LastModified { get; set; }
    }
}
