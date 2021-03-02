using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    public class PageRes<T>
    {
        /// <summary>
        /// 当前最大Id
        /// </summary>
        public long MaxId { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 列表数据
        /// </summary>
        public List<T> List { get; set; }
    }
}
