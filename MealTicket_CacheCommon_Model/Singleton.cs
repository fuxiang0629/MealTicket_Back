using MealTicket_CacheCommon_Session.session;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MealTicket_CacheCommon_Session
{
    public sealed class Singleton
    {
        /// <summary>
        /// 单例对象
        /// </summary>
        private static readonly Singleton instance = new Singleton();

        //登入信息
        public Dictionary<string, LoginResult> loginDic = new Dictionary<string, LoginResult>();

        //缓存信息
        private Dictionary<string, Dictionary<string, string>> totalCacheDic = new Dictionary<string, Dictionary<string, string>>();
        private ReaderWriterLock _readWriteLock = new ReaderWriterLock();

        public string getCacheData(string key, string dataKey)
        {
            Dictionary<string, string> cacheDic = new Dictionary<string, string>();
            _readWriteLock.AcquireReaderLock(-1);
            if (!totalCacheDic.TryGetValue(key, out cacheDic))
            {
                cacheDic = new Dictionary<string, string>();
            }
            _readWriteLock.ReleaseReaderLock();
            string result = null;
            cacheDic.TryGetValue(dataKey, out result);
            return result;
        }

        public bool setCacheData(string key, string dataKey, string value)
        {
            _readWriteLock.AcquireWriterLock(-1);
            Dictionary<string, string> cacheDic = new Dictionary<string, string>();
            if (!totalCacheDic.TryGetValue(key, out cacheDic))
            {
                cacheDic = new Dictionary<string, string>();
            }
            cacheDic[dataKey] = value;
            totalCacheDic[key] = cacheDic;
            _readWriteLock.ReleaseWriterLock();

            return true;
        }

        public bool refreshCacheData(string key, string dataKey)
        {
            if (key != "mealTicket_baseData")
            {
                return false;
            }
            if (dataKey == "BuyTipSession")
            {
                _buyTipSession.UpdateSessionManual();
            }
            return true;
        }

        // 显式静态构造函数告诉C＃编译器
        // 不要将类型标记为BeforeFieldInit
        static Singleton()
        {
            
        }

        private Singleton()
        {
            //添加账户
            loginDic.Add("mealTicket_baseData", new LoginResult 
            {
                Username= "mealTicket_baseData",
                Password= "mealTicket_baseData",
                VisitKey= "mealTicket_baseData"
            });
        }

        public static Singleton Instance
        {
            get
            {
                return instance;
            }
        }

        public void Dispose() 
        {
            if (_sharesBaseSession != null)
            {
                _sharesBaseSession.Dispose();
            }
            if (_sharesQuotesSession != null)
            {
                _sharesQuotesSession.Dispose();
            }
            if (_accountRiseLimitTriSession != null)
            {
                _accountRiseLimitTriSession.Dispose();
            }
            if (_accountTrendTriSession != null)
            {
                _accountTrendTriSession.Dispose();
            }
            if (_sharesPlateSession != null)
            {
                _sharesPlateSession.Dispose();
            }
            if (_sharesPlateQuotesSession != null)
            {
                _sharesPlateQuotesSession.Dispose();
            }
            if (_sharesPlateRelSession != null)
            {
                _sharesPlateRelSession.Dispose();
            }
            if (_buyTipSession != null)
            {
                _buyTipSession.Dispose();
            }
            if (_sharesHisRiseRateSession != null)
            {
                _sharesHisRiseRateSession.Dispose();
            }
        }


        public void Init()
        {
            _sharesBaseSession.StartUpdate(TimeSpan.Parse("01:00:00"), TimeSpan.Parse("02:00:00"));
            _sharesQuotesSession.StartUpdate(3000);
            _accountRiseLimitTriSession.StartUpdate(3000);
            _accountTrendTriSession.StartUpdate(3000);
            _sharesPlateSession.StartUpdate(600000);
            _sharesPlateQuotesSession.StartUpdate(3000);
            _sharesPlateRelSession.StartUpdate(600000);
            _buyTipSession.StartUpdate(1000);
            _sharesHisRiseRateSession.StartUpdate(600000); 
        }

        #region===缓存===
        public SharesBaseSession _sharesBaseSession = new SharesBaseSession();
        public SharesQuotesSession _sharesQuotesSession = new SharesQuotesSession();
        public AccountRiseLimitTriSession _accountRiseLimitTriSession = new AccountRiseLimitTriSession();
        public AccountTrendTriSession _accountTrendTriSession = new AccountTrendTriSession();
        public SharesPlateSession _sharesPlateSession = new SharesPlateSession();
        public SharesPlateQuotesSession _sharesPlateQuotesSession = new SharesPlateQuotesSession();
        public SharesPlateRelSession _sharesPlateRelSession = new SharesPlateRelSession();
        public BuyTipSession _buyTipSession = new BuyTipSession();
        public SharesHisRiseRateSession _sharesHisRiseRateSession = new SharesHisRiseRateSession(); 
        #endregion
    }
}
