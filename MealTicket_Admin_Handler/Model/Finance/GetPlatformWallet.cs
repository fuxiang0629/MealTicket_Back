using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetPlatformWallet
    {
    }

    public class PlatformWallet
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 类型code
        /// </summary>
        public string TypeCode { get; set; }

        /// <summary>
        /// 类型描述
        /// </summary>
        public string TypeDes { get; set; }

        /// <summary>
        /// 确认金额
        /// </summary>
        public long ConfirmDeposit { get; set; }

        /// <summary>
        /// 收入
        /// </summary>
        public long InCome { get; set; }

        /// <summary>
        /// 支出
        /// </summary>
        public long Expend { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
