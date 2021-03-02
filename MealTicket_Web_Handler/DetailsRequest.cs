using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    public class DetailsRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
    }

    public class DetailsPageRequest: PageRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
    }
}
