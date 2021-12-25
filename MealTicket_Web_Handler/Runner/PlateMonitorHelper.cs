using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class PlateMonitorHelper
    {
        //1.3天 2.5天 3.10天 4.15天
        int DaysType;

        public PlateMonitorHelper(int daysType) 
        {
            DaysType = daysType;
        }
    }
}
