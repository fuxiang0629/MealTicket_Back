using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySharesCorrigendum
    {
    }

    public class ModifySharesCorrigendumRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称拼音简称
        /// </summary>
        public string SharesPyjc { get; set; }
    }
}
