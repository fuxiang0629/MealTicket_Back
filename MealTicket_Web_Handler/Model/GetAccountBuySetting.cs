using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountBuySetting
    {
    }

    public class GetAccountBuySettingRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }
    }

    public class AccountBuySettingInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 1保留余额 2最大持仓 3板块最大持仓
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public long ParValue { get; set; }

        /// <summary>
        /// 参数值json格式
        /// </summary>
        public string ParValueJson { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
