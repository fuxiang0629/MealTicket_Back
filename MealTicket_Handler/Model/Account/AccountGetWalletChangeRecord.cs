using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountGetWalletChangeRecord
    {
    }

    public class AccountGetWalletChangeRecordRequest:PageRequest 
    {
        /// <summary>
        /// 年份（0表示所有）
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份（0表示所有）
        /// </summary>
        public int Month { get; set; }
    }

    public class AccountWalletChangeRecordInfo
    {
        /// <summary>
        /// 变更前金额
        /// </summary>
        public long PreAmount { get; set; }

        /// <summary>
        /// 变更金额
        /// </summary>
        public long ChangeAmount { get; set; }

        /// <summary>
        /// 变更后金额
        /// </summary>
        public long CurAmount { get; set; }

        /// <summary>
        /// 变更描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
