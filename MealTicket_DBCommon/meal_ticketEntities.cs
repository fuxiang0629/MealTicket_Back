using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_DBCommon
{
    public partial class meal_ticketEntities : DbContext
    {
        public meal_ticketEntities(string nameOrConnectionString)
            :base(nameOrConnectionString)
        {
        }
    }
}
