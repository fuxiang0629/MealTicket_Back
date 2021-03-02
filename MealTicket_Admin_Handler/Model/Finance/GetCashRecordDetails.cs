using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetCashRecordDetails
    {
    }

    public class CashRecordDetails
    {
        /// <summary>
        /// 申请提现金额
        /// </summary>
        public long ApplyAmount { get; set; }

        /// <summary>
        /// 已提现金额
        /// </summary>
        public long CashedAmount { get; set; }

        /// <summary>
        /// 提现详情列表
        /// </summary>
        public List<CashDetails> CashDetailsList { get; set; }

        /// <summary>
        /// 完成情况
        /// </summary>
        public CashFinishInfo FinishInfo { get; set; }
    }

    public class CashFinishInfo 
    {
        /// <summary>
        /// 操作人
        /// </summary>
        public string AdminAccountName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime FinishTime { get; set; }
    }

    public class CashDetails 
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 打款方式1.退款 2.转账
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public long ApplyAmount { get; set; }

        /// <summary>
        /// 实转金额
        /// </summary>
        public long CashedAmount { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        public long ServiceFee { get; set; }

        /// <summary>
        /// 打款账户
        /// </summary>
        public string PaymentAccountName { get; set; }

        /// <summary>
        /// 打款凭证
        /// </summary>
        [IgnoreDataMember]
        public string VoucherImg { get; set; }

        /// <summary>
        /// 打款凭证
        /// </summary>
        public string VoucherImgShow
        {
            get
            {
                if (string.IsNullOrEmpty(VoucherImg) || VoucherImg.ToUpper().Contains("HTTP"))
                {
                    return VoucherImg;
                }
                return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], VoucherImg);
            }
        }

        /// <summary>
        /// 退款单号
        /// </summary>
        public string RefundSn { get; set; }

        /// <summary>
        /// 状态1申请中 11处理中 2成功 3失败
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDes { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string AdminAccountName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? FinishTime { get; set; }
    }
}
