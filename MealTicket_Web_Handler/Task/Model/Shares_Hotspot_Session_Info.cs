using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler
{
    public class Shares_Hotspot_Session_Info
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int DataType { get; set; }

        public long AccountId { get; set; }

        public string BgColor { get; set; }

        public bool ShowBgColor { get; set; }

        public int Status { get; set; }

        public List<long> PlateIdList { get; set; }
    }
}
