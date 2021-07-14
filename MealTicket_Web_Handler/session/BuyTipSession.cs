using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MealTicket_Web_Handler.session
{
    public class BuyTipSession: Session<List<BuyTipInfo>>
    {
        public override List<BuyTipInfo> UpdateSession()
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            using (var db = new meal_ticketEntities())
            {
                var triDetails = from item in db.t_account_shares_conditiontrade_buy_details
                                 join item2 in db.t_account_shares_conditiontrade_buy on item.ConditionId equals item2.Id
                                 join item3 in db.t_account_baseinfo on item2.AccountId equals item3.Id
                                 select new { item, item3 };
                var result = (from item in db.t_account_shares_conditiontrade_buy_details
                              join item2 in db.t_account_shares_conditiontrade_buy on item.ConditionId equals item2.Id
                              join item3 in db.v_shares_quotes_last on new { item2.Market, item2.SharesCode } equals new { item3.Market, item3.SharesCode }
                              join item4 in db.t_account_baseinfo on item2.AccountId equals item4.Id
                              join item5 in db.t_shares_all on new { item2.Market, item2.SharesCode } equals new { item5.Market, item5.SharesCode }
                              join item6 in db.t_account_shares_entrust on item.EntrustId equals item6.Id into a
                              from ai in a.DefaultIfEmpty()
                              join item7 in triDetails on item.FatherId equals item7.item.Id into b
                              from bi in b.DefaultIfEmpty()
                              where item.Status == 1 && (item.BusinessStatus == 1 || item.BusinessStatus == 3 || item.BusinessStatus == 4) && item2.Status == 1 && item3.PresentPrice > 0 && item3.ClosedPrice > 0
                              orderby item.TriggerTime
                              select new BuyTipInfo
                              {
                                  SharesCode = item2.SharesCode,
                                  Market = item2.Market,
                                  CreateAccountId = item.CreateAccountId,
                                  SharesName = item5.SharesName,
                                  AccountMobile = item4.Mobile,
                                  AccountName = item4.NickName,
                                  CurrPrice = item3.PresentPrice,
                                  ClosedPrice = item3.ClosedPrice,
                                  TriggerTime = item.TriggerTime,
                                  EntrustPriceGear = item.EntrustPriceGear,
                                  EntrustAmount = item.EntrustAmount,
                                  Id = item.Id,
                                  AccountId = item2.AccountId,
                                  BuyAuto = item.BusinessStatus == 3 ? true : item.BusinessStatus == 4 ? false : item.BuyAuto,
                                  RisePrice = item3.PresentPrice - item3.ClosedPrice,
                                  RiseRate = (int)((item3.PresentPrice - item3.ClosedPrice) * 1.0 / item3.ClosedPrice * 10000),
                                  Status = item.BusinessStatus == 1 ? 1 : ai == null ? 2 : ai.Status != 3 ? 3 : ai.DealCount <= 0 ? 5 : ai.DealCount >= ai.EntrustCount ? 6 : 4,
                                  FollowAccountList = (from x in db.t_account_shares_conditiontrade_buy_details_follow
                                                       where x.DetailsId == item.Id
                                                       select x.FollowAccountId).ToList(),
                                  SyncroAccountMobile = bi == null ? "本账户" : bi.item3.Mobile,
                                  SyncroAccountName = bi == null ? "" : bi.item3.NickName
                              }).ToList();
                scope.Complete();
                return result;
            }
        }
    }
}
