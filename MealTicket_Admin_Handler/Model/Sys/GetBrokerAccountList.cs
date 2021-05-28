using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetBrokerAccountList
    {
    }

    public class GetBrokerAccountListRequest : PageRequest 
    { 
        /// <summary>
        /// 账户名称
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 服务器Id
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// 状态0全部 1有效 2无效
        /// </summary>
        public int Status { get; set; }
    }

    public class BrokerAccountInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

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
        /// 沪市资金账户
        /// </summary>
        public string TradeAccountNo1 { get; set; }

        /// <summary>
        /// 深市股东代码
        /// </summary>
        public string Holder0 { get; set; }

        /// <summary>
        /// 沪市股东代码
        /// </summary>
        public string Holder1 { get; set; }

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
        /// 券商名称
        /// </summary>
        public string BrokerName { get; set; }

        /// <summary>
        /// 营业部code
        /// </summary>
        public int DepartmentCode { get; set; }

        /// <summary>
        /// 营业部名称
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChecked { get; set; }

        /// <summary>
        /// 是否可以勾选
        /// </summary>
        public bool CanChecked { get; set; }

        /// <summary>
        /// 是否是有账户
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}
