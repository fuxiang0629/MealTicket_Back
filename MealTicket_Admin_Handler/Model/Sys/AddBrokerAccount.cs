using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class AddBrokerAccount
    {
    }

    public class AddBrokerAccountRequest
    {
        /// <summary>
        /// 账户Code
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 登录账号类型
        /// </summary>
        public int AccountType { get; set; }

        /// <summary>
        /// 账户号
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 深市资金账户
        /// </summary>
        public string TradeAccountNo0 { get; set; }

        /// <summary>
        /// 深市股东代码
        /// </summary>
        public string Holder0 { get; set; }

        /// <summary>
        /// 沪市股东代码
        /// </summary>
        public string Holder1 { get; set; }

        /// <summary>
        /// 沪市资金账户
        /// </summary>
        public string TradeAccountNo1 { get; set; }

        /// <summary>
        /// 初始资金
        /// </summary>
        public long InitialFunding { get; set; }

        /// <summary>
        /// 交易密码
        /// </summary>
        public string JyPassword { get; set; }

        /// <summary>
        /// 通讯密码
        /// </summary>
        public string TxPassword { get; set; }

        /// <summary>
        /// 券商code
        /// </summary>
        public int BrokerCode { get; set; }

        /// <summary>
        /// 营业部code
        /// </summary>
        public int DepartmentCode { get; set; }

        /// <summary>
        /// 是否是有账户
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}
