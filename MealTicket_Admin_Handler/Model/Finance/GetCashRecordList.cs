using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetCashRecordList
    {
    }

    public class GetCashRecordListRequest:PageRequest
    { 
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderSn { get; set; }

        /// <summary>
        /// 账户昵称/手机号
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        /// 状态0全部 1.提交申请 2.申请处理中 21.已开始提现 3.提现失败 31.拒绝申请 4.提现成功 5.部分提现成功 6.未处理 7已处理
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 提现方式1退款优先 2转账优先 3无要求
        /// </summary>
        public int CashType { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class CashRecordInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderSn { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 申请提现金额
        /// </summary>
        public long ApplyAmount { get; set; }

        /// <summary>
        /// 已提现金额
        /// </summary>
        public long CashedAmount { get; set; }

        /// <summary>
        /// 提现方式1退款优先 2转账优先 3无要求
        /// </summary>
        public int CashType { get; set; }

        /// <summary>
        /// 服务费率（1/万）
        /// </summary>
        public int ServiceFeeRate { get; set; }

        /// <summary>
        /// 已收取服务费
        /// </summary>
        public long ServiceFee { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 银行卡类型
        /// </summary>
        public string CardBreed { get; set;}

        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 状态1.提交申请 2.申请处理中 21.已开始提现 3.提现失败 31拒绝申请 4.提现成功 5.部分提现成功 
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 申请提现时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
