using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSharesCorrigendumList
    {
    }

    public class GetSharesCorrigendumListRequest : PageRequest
    {
        /// <summary>
        /// 股票代码/名称
        /// </summary>
        public string SharesCode { get; set; }
    }

    public class SharesCorrigendumInfo 
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 市场代码0深圳 1上海
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 股票名称简拼
        /// </summary>
        public string SharesPyjc { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
