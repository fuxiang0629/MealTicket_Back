using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountSharesDepositChangeRecord
    {
    }

    public class FrontAccountSharesDepositChangeRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 变更前
        /// </summary>
        public long PreAmount { get; set; }

        /// <summary>
        /// 变更
        /// </summary>
        public long ChangeAmount { get; set; }

        /// <summary>
        /// 变更后
        /// </summary>
        public long CurAmount { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
