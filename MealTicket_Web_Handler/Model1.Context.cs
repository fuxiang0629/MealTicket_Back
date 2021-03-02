﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace MealTicket_Web_Handler
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class meal_ticketEntities : DbContext
    {
        public meal_ticketEntities()
            : base("name=meal_ticketEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<t_account_shares_optional> t_account_shares_optional { get; set; }
        public virtual DbSet<t_shares_quotes> t_shares_quotes { get; set; }
        public virtual DbSet<t_shares_all> t_shares_all { get; set; }
        public virtual DbSet<t_shares_monitor_trend_rel> t_shares_monitor_trend_rel { get; set; }
        public virtual DbSet<t_account_login_token_web> t_account_login_token_web { get; set; }
        public virtual DbSet<t_system_param> t_system_param { get; set; }
        public virtual DbSet<t_account_shares_optional_trend_rel> t_account_shares_optional_trend_rel { get; set; }
        public virtual DbSet<t_account_shares_optional_trend_rel_par> t_account_shares_optional_trend_rel_par { get; set; }
        public virtual DbSet<t_shares_monitor_trend> t_shares_monitor_trend { get; set; }
        public virtual DbSet<t_account_shares_seat> t_account_shares_seat { get; set; }
        public virtual DbSet<t_account_shares_optional_seat_rel> t_account_shares_optional_seat_rel { get; set; }
        public virtual DbSet<t_shares_today> t_shares_today { get; set; }
        public virtual DbSet<t_dim_time> t_dim_time { get; set; }
        public virtual DbSet<t_account_shares_optional_trend_rel_tri_record> t_account_shares_optional_trend_rel_tri_record { get; set; }
        public virtual DbSet<t_shares_limit_date> t_shares_limit_date { get; set; }
        public virtual DbSet<t_shares_limit_date_group> t_shares_limit_date_group { get; set; }
        public virtual DbSet<t_shares_limit_time> t_shares_limit_time { get; set; }
        public virtual DbSet<t_shares_monitor> t_shares_monitor { get; set; }
        public virtual DbSet<t_goods> t_goods { get; set; }
        public virtual DbSet<t_payment_channel> t_payment_channel { get; set; }
        public virtual DbSet<t_account_shares_optional_group> t_account_shares_optional_group { get; set; }
        public virtual DbSet<t_shares_markettime> t_shares_markettime { get; set; }
        public virtual DbSet<t_account_shares_optional_trend_rel_tri> t_account_shares_optional_trend_rel_tri { get; set; }
        public virtual DbSet<t_account_shares_optional_group_rel> t_account_shares_optional_group_rel { get; set; }
        public virtual DbSet<t_account_shares_optional_trend_rel_tri_record_statistic> t_account_shares_optional_trend_rel_tri_record_statistic { get; set; }
        public virtual DbSet<t_account_follow_rel> t_account_follow_rel { get; set; }
        public virtual DbSet<t_account_baseinfo> t_account_baseinfo { get; set; }
        public virtual DbSet<t_account_shares_hold> t_account_shares_hold { get; set; }
        public virtual DbSet<t_account_wallet> t_account_wallet { get; set; }
        public virtual DbSet<t_account_shares_entrust> t_account_shares_entrust { get; set; }
        public virtual DbSet<t_account_shares_entrust_follow> t_account_shares_entrust_follow { get; set; }
        public virtual DbSet<t_account_shares_entrust_manager> t_account_shares_entrust_manager { get; set; }
        public virtual DbSet<t_account_shares_entrust_manager_dealdetails> t_account_shares_entrust_manager_dealdetails { get; set; }
        public virtual DbSet<t_shares_limit_fundmultiple> t_shares_limit_fundmultiple { get; set; }
        public virtual DbSet<t_broker_account_info> t_broker_account_info { get; set; }
        public virtual DbSet<t_server> t_server { get; set; }
        public virtual DbSet<t_server_broker_account_rel> t_server_broker_account_rel { get; set; }
        public virtual DbSet<t_shares_limit_traderules> t_shares_limit_traderules { get; set; }
        public virtual DbSet<t_shares_limit_traderules_other> t_shares_limit_traderules_other { get; set; }
    
        public virtual int P_CheckAccountLogin_Web(string token, ObjectParameter errorCode, ObjectParameter errorMessage, ObjectParameter accountId)
        {
            var tokenParameter = token != null ?
                new ObjectParameter("token", token) :
                new ObjectParameter("token", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_CheckAccountLogin_Web", tokenParameter, errorCode, errorMessage, accountId);
        }
    
        public virtual int P_AccountLogin_Web(string mobile, string loginPassword, ObjectParameter errorCode, ObjectParameter errorMessage, ObjectParameter token, ObjectParameter accountId)
        {
            var mobileParameter = mobile != null ?
                new ObjectParameter("mobile", mobile) :
                new ObjectParameter("mobile", typeof(string));
    
            var loginPasswordParameter = loginPassword != null ?
                new ObjectParameter("loginPassword", loginPassword) :
                new ObjectParameter("loginPassword", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_AccountLogin_Web", mobileParameter, loginPasswordParameter, errorCode, errorMessage, token, accountId);
        }
    
        public virtual int P_ApplyTradeBuy(Nullable<long> accountId, Nullable<int> market, string sharesCode, Nullable<long> buyAmount, Nullable<int> fundMultiple, Nullable<long> entrustPrice, Nullable<System.DateTime> closingTime, Nullable<bool> isFollow, ObjectParameter errorCode, ObjectParameter errorMessage, ObjectParameter buyId)
        {
            var accountIdParameter = accountId.HasValue ?
                new ObjectParameter("accountId", accountId) :
                new ObjectParameter("accountId", typeof(long));
    
            var marketParameter = market.HasValue ?
                new ObjectParameter("market", market) :
                new ObjectParameter("market", typeof(int));
    
            var sharesCodeParameter = sharesCode != null ?
                new ObjectParameter("sharesCode", sharesCode) :
                new ObjectParameter("sharesCode", typeof(string));
    
            var buyAmountParameter = buyAmount.HasValue ?
                new ObjectParameter("buyAmount", buyAmount) :
                new ObjectParameter("buyAmount", typeof(long));
    
            var fundMultipleParameter = fundMultiple.HasValue ?
                new ObjectParameter("fundMultiple", fundMultiple) :
                new ObjectParameter("fundMultiple", typeof(int));
    
            var entrustPriceParameter = entrustPrice.HasValue ?
                new ObjectParameter("entrustPrice", entrustPrice) :
                new ObjectParameter("entrustPrice", typeof(long));
    
            var closingTimeParameter = closingTime.HasValue ?
                new ObjectParameter("closingTime", closingTime) :
                new ObjectParameter("closingTime", typeof(System.DateTime));
    
            var isFollowParameter = isFollow.HasValue ?
                new ObjectParameter("isFollow", isFollow) :
                new ObjectParameter("isFollow", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_ApplyTradeBuy", accountIdParameter, marketParameter, sharesCodeParameter, buyAmountParameter, fundMultipleParameter, entrustPriceParameter, closingTimeParameter, isFollowParameter, errorCode, errorMessage, buyId);
        }
    
        public virtual int P_ApplyTradeCancel(Nullable<long> accountId, Nullable<long> entrustId, ObjectParameter errorCode, ObjectParameter errorMessage)
        {
            var accountIdParameter = accountId.HasValue ?
                new ObjectParameter("accountId", accountId) :
                new ObjectParameter("accountId", typeof(long));
    
            var entrustIdParameter = entrustId.HasValue ?
                new ObjectParameter("entrustId", entrustId) :
                new ObjectParameter("entrustId", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_ApplyTradeCancel", accountIdParameter, entrustIdParameter, errorCode, errorMessage);
        }
    
        public virtual int P_ApplyTradeSell(Nullable<long> accountId, Nullable<long> holdId, Nullable<int> sellCount, Nullable<int> sellType, Nullable<long> sellPrice, Nullable<int> type, Nullable<bool> isFollow, ObjectParameter errorCode, ObjectParameter errorMessage, ObjectParameter sellId)
        {
            var accountIdParameter = accountId.HasValue ?
                new ObjectParameter("accountId", accountId) :
                new ObjectParameter("accountId", typeof(long));
    
            var holdIdParameter = holdId.HasValue ?
                new ObjectParameter("holdId", holdId) :
                new ObjectParameter("holdId", typeof(long));
    
            var sellCountParameter = sellCount.HasValue ?
                new ObjectParameter("sellCount", sellCount) :
                new ObjectParameter("sellCount", typeof(int));
    
            var sellTypeParameter = sellType.HasValue ?
                new ObjectParameter("sellType", sellType) :
                new ObjectParameter("sellType", typeof(int));
    
            var sellPriceParameter = sellPrice.HasValue ?
                new ObjectParameter("sellPrice", sellPrice) :
                new ObjectParameter("sellPrice", typeof(long));
    
            var typeParameter = type.HasValue ?
                new ObjectParameter("type", type) :
                new ObjectParameter("type", typeof(int));
    
            var isFollowParameter = isFollow.HasValue ?
                new ObjectParameter("isFollow", isFollow) :
                new ObjectParameter("isFollow", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_ApplyTradeSell", accountIdParameter, holdIdParameter, sellCountParameter, sellTypeParameter, sellPriceParameter, typeParameter, isFollowParameter, errorCode, errorMessage, sellId);
        }
    
        public virtual int P_MakeupDeposit(Nullable<long> accountId, Nullable<long> holdId, Nullable<long> deposit, ObjectParameter errorCode, ObjectParameter errorMessage)
        {
            var accountIdParameter = accountId.HasValue ?
                new ObjectParameter("accountId", accountId) :
                new ObjectParameter("accountId", typeof(long));
    
            var holdIdParameter = holdId.HasValue ?
                new ObjectParameter("holdId", holdId) :
                new ObjectParameter("holdId", typeof(long));
    
            var depositParameter = deposit.HasValue ?
                new ObjectParameter("deposit", deposit) :
                new ObjectParameter("deposit", typeof(long));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("P_MakeupDeposit", accountIdParameter, holdIdParameter, depositParameter, errorCode, errorMessage);
        }
    }
}
