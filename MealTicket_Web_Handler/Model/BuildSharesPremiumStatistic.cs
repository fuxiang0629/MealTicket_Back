using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BuildSharesPremiumStatistic
    {
    }

    public class BuildSharesPremiumStatisticRequest
    {
        public int Days { get; set; }

        public long ConditionId { get; set; }
    }
}
