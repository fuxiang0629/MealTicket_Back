using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetSharesKLineTypeList
    {
    }

    public class GetSharesKLineTypeListRes
    {
        /// <summary>
        /// 默认展示
        /// </summary>
        public int DefaultShow { get; set; }

        /// <summary>
        /// 展示列表
        /// </summary>
        public List<int> SecurityBarsDataTypeShow { get; set; }
    }
}
