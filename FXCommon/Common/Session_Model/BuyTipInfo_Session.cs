using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FXCommon.Common
{
    public class BuyTipInfo_Session
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 主导用户Id
        /// </summary>
        public long CreateAccountId { get; set; }

        /// <summary>
        /// 股票名称
        /// </summary>
        public string SharesName { get; set; }

        /// <summary>
        /// 股票代码
        /// </summary>
        public string SharesCode { get; set; }

        /// <summary>
        /// 市场
        /// </summary>
        public int Market { get; set; }

        /// <summary>
        /// 是否自动买入
        /// </summary>
        public bool BuyAuto { get; set; }

        /// <summary>
        /// 当前价
        /// </summary>
        public long CurrPrice { get; set; }

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public int RiseRate { get; set; }

        /// <summary>
        /// 涨跌价
        /// </summary>
        public long RisePrice { get; set; }

        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime? TriggerTime { get; set; }

        /// <summary>
        /// 状态 1等待买入 2提交失败 3委托中 4部撤 5已撤 6已成
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 委托金额
        /// </summary>
        public long EntrustAmount { get; set; }

        /// <summary>
        /// 委托价格档位0市价 1限价买一 2限价买二 3限价买三 4限价买四  5限价买五 6限价卖一 7限价卖二 8限价卖三 9限价卖四  10限价卖五 11涨停卖出 12跌停卖出
        /// </summary>
        public int EntrustPriceGear { get; set; }

        /// <summary>
        /// 跟投人员
        /// </summary>
        public List<long> FollowAccountList { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        public long ClosedPrice { get; set; }

        /// <summary>
        /// 板块列表
        /// </summary>
        public List<SharesPlateInfo_Session> PlateList { get; set; }

        /// <summary>
        /// 最大涨跌幅
        /// </summary>
        public long Range { get; set; }

        public int Fundmultiple { get; set; }

        /// <summary>
        /// 所属自定义分组
        /// </summary>
        public List<long> GroupList { get; set; }

        /// <summary>
        /// 同步账户手机号
        /// </summary>
        public string SyncroAccountMobile { get; set; }

        /// <summary>
        /// 同步账户昵称
        /// </summary>
        public string SyncroAccountName { get; set; }
    }
}
