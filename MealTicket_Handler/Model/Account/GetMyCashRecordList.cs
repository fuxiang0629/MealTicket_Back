using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetMyCashRecordList
    {
    }

    public class GetMyCashRecordListRequest : PageRequest
    {
        /// <summary>
        /// 年份（0表示所有年份）
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份（0表示所有月份）
        /// </summary>
        public int Month { get; set; }
    }
    public class CashRecordInfo
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 提现日期（2019-01）
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 提现申请时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 状态1.提交申请 2.申请处理中 3.提现失败 4.提现成功
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDes { get; set; }

        /// <summary>
        /// 提现申请金额
        /// </summary>
        public int ApplyAmount { get; set; }

        /// <summary>
        /// 提现服务费率
        /// </summary>
        public int ServiceFeeRate { get; set; }

        /// <summary>
        /// 提现服务费
        /// </summary>
        public int ServiceFee { get; set; }

        /// <summary>
        /// 实际到账金额
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 提现方式1退款优先 2转账优先 3无要求
        /// </summary>
        public int CashType { get; set; }
    }

}
