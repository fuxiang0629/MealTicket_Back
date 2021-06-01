using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class BindAccountFollowGroup
    {
    }

    public class BindAccountFollowGroupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 跟投Id
        /// </summary>
        public List<long> FollowId { get; set; }
    }
}
