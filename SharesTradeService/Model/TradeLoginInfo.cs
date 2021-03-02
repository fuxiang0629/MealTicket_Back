using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService.Model
{
    public class TradeLoginInfo
    {
        /// <summary>
        /// 券商Id
        /// </summary>
        public int QsId { get; set; }

        /// <summary>
        /// 券商服务器列表
        /// </summary>
        public List<TradeLoginHost> TradeLoginHostList { get; set; }

        /// <summary>
        /// 客户端版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 营业部Id
        /// </summary>
        public int YybId { get; set; }

        /// <summary>
        /// 登录账号类型
        /// </summary>
        public int AccountType { get; set; }

        /// <summary>
        /// 登录账户
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// 深圳资金账户
        /// </summary>
        public string TradeAccountNo0 { get; set; }

        /// <summary>
        /// 上海资金账户
        /// </summary>
        public string TradeAccountNo1 { get; set; }

        /// <summary>
        /// 深圳股东代码
        /// </summary>
        public string Holder0 { get; set; }

        /// <summary>
        /// 上海股东代码
        /// </summary>
        public string Holder1 { get; set; }

        /// <summary>
        /// 交易密码
        /// </summary>
        public string JyPassword { get; set; }

        /// <summary>
        /// 通讯密码
        /// </summary>
        public string TxPassword { get; set; }

        /// <summary>
        /// 账户唯一码
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// 登入成功后的深圳链接Id
        /// </summary>
        public int TradeClientId0 { get; set; }

        /// <summary>
        /// 登入成功后的上海链接Id
        /// </summary>
        public int TradeClientId1 { get; set; }

        /// <summary>
        /// 初始金额
        /// </summary>
        public long InitialFunding { get; set; }
    }

    public class TradeLoginHost 
    {
        /// <summary>
        /// 券商服务器Ip
        /// </summary>
        public string QsHost { get; set; }

        /// <summary>
        /// 券商服务器端口
        /// </summary>
        public int QsPort { get; set; }
    }
}
