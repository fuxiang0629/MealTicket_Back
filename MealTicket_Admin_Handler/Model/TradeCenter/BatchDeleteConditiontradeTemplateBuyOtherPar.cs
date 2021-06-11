using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class BatchDeleteConditiontradeTemplateBuyOtherPar
    {
    }

    public class BatchDeleteConditiontradeTemplateBuyOtherParRequest
    {
        /// <summary>
        /// 额外走势iId
        /// </summary>
        public long OtherTrendId { get; set; }

        /// <summary>
        /// datatype*10+grouptype
        /// </summary>
        public long Id { get; set; }
    }
}
