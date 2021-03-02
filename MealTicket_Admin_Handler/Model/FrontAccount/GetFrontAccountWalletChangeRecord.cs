using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountWalletChangeRecord
    {
    }

    public class GetFrontAccountWalletChangeRecordRequest : PageRequest 
    { 
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class FrontAccountWalletChangeRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
