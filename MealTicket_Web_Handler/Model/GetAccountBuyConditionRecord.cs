using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountBuyConditionRecord
    {
    }

    public class GetAccountBuyConditionRecordRequest : PageRequest
    {
        /// <summary>
        /// 股票信息
        /// </summary>
        public string SharesInfo { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 委托状态1.申报中 2.交易中 3.已撤 4.部撤 5.已成 6.未申报
        /// </summary>
        public int EntrustStatus { get; set; }

        /// <summary>
        /// 触发方式1自动购买 2手动购买
        /// </summary>
        public int BusinessStatus { get; set; }
    }

    public class AccountBuyConditionRecordInfo 
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 账户昵称
        /// </summary>
        public string AccountNickName { get; set; }

        /// <summary>
        /// 账户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 股票code
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }  

        /// <summary>
        /// 委托Id
        /// </summary>
        public long EntrustId { get; set; }

        /// <summary>
        /// 委托价格（0表示市价委托）
        /// </summary>
        public long EntrustPrice { get; set; }

        /// <summary>
        /// 委托类型1市价 2限价
        /// </summary>
        public int EntrustType { get; set; }

        /// <summary>
        /// 委托数量
        /// </summary>
        public int EntrustCount { get; set; }

        /// <summary>
        /// 成交数量
        /// </summary>
        public int DealCount { get; set; }

        /// <summary>
        /// 委托状态
        /// </summary>
        public int EntrustStatus { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string EntrustStatusDes { get; set; }

        /// <summary>
        /// 业务状态1触发等待 2已手动确认 3已自动购买 4.已手动购买
        /// </summary>
        public int BusinessStatus { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime? TriggerTime { get; set; }
    }
}
