using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifySharesPremiumStatisticCondition
    {
    }

    public class ModifySharesPremiumStatisticConditionRequest
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int OrderIndex { get; set; }
    }
}
