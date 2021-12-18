using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    enum Enum_PlateTag_CalType
    {
        FORCE,//强势
        TRENDLIKE,//走势最像
    }
    enum Enum_SharesTag_CalType
    {
        LEADER=1,//龙头
        DAYLEADER=2,//日内龙头
        MAINARMY=3,//中军
    }
    enum Enum_PlateTag_DayType
    {
        FORCE_3DAYS = 1,
        FORCE_5DAYS = 2,
        FORCE_10DAYS = 3,
        FORCE_15DAYS = 4
    }
}
