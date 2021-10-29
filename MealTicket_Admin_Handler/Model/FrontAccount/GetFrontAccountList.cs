using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetFrontAccountList
    {
    }

    public class GetFrontAccountListRequest : PageRequest
    {
        /// <summary>
        /// 用户信息昵称/分享码/手机号
        /// </summary>
        public string AccountInfo { get; set; }

        /// <summary>
        /// 推荐人昵称/分享码/手机号
        /// </summary>
        public string ReferInfo { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class FrontAccountInfo
    {
        /// <summary>
        /// 账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string HeadUrl { get; set; }

        /// <summary>
        /// 头像地址（网络地址）
        /// </summary>
        public string HeadUrlShow
        {
            get
            {
                if (string.IsNullOrEmpty(HeadUrl) || HeadUrl.ToUpper().Contains("HTTP"))
                {
                    return HeadUrl;
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], HeadUrl);
            }
        }

        /// <summary>
        /// 性别0未知 1男 2女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 出生年月
        /// </summary>
        public string BirthDay { get; set; }

        /// <summary>
        /// 分享码
        /// </summary>
        public string RecommandCode { get; set; }

        /// <summary>
        /// 推荐人昵称
        /// </summary>
        public string ReferNickName { get; set; }

        /// <summary>
        /// 推荐人分享码
        /// </summary>
        public string ReferRecommandCode { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 禁止状态0不禁 1禁止买入 2禁止卖出 3禁止交易
        /// </summary>
        public int ForbidStatus { get; set; }

        /// <summary>
        /// 监控状态1有效 2无效
        /// </summary>
        public int MonitorStatus { get; set; }

        /// <summary>
        /// 走势权限状态
        /// </summary>
        public int TrendStatus { get; set; }

        /// <summary>
        /// 杠杆倍数设置状态
        /// </summary>
        public int MultipleChangeStatus { get; set; }

        /// <summary>
        /// 提现状态 1有效 2无效
        /// </summary>
        public int CashStatus { get; set; }

        /// <summary>
        /// 保证金余额
        /// </summary>
        public long Deposit { get; set; }

        /// <summary>
        /// 使用保证金
        /// </summary>
        public long UseDeposit { get; set; }

        /// <summary>
        /// 总市值
        /// </summary>
        public long TotalMarketValue { get; set; }

        /// <summary>
        /// 总借款
        /// </summary>
        public long TotalFundAmount { get; set; }

        /// <summary>
        /// 钱包状态1有效 2无效
        /// </summary>
        public int WalletStatus { get; set; }

        /// <summary>
        /// 银行卡数量
        /// </summary>
        public int BankCardCount { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否实名认证
        /// </summary>
        public bool IsRealName { get; set; }

        /// <summary>
        /// 跟投账户数量
        /// </summary>
        public int FollowAccountCount { get; set; }

        /// <summary>
        /// 操盘账户Id
        /// </summary>
        public long MainAccountId { get; set; }

        /// <summary>
        /// 操盘账户
        /// </summary>
        public string MainAccountInfo { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }
    }
}
