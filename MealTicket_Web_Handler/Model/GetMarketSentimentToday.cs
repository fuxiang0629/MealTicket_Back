using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetMarketSentimentToday
    {
    }

    public class MarketSentimentInfo
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        public string DateStr 
        {
            get 
            {
                return Date.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// 市场情绪得分值
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 是否合格
        /// </summary>
        public bool IsQualified { get; set; }

        public int LimitUpScore { get; set; }

        public int RiseUpScore { get; set; }

        public int LimitUpYesRiseRateScore { get; set; }

        public int LimitUpBombRateScore { get; set; }

        public int LimitDownBombRateScore { get; set; }

        public int HighMarkScore { get; set; }
    }

    public class GetMarketSentimentMtLineDailyListRequest
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
    }

    public class MarketSentimentDailyInfo
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        public string DateStr
        {
            get
            {
                return Date.ToString("yyyy-MM-dd");
            }
        }

        public DateTime Time { get; set; }

        public string TimeStr
        {
            get
            {
                return Time.ToString("HH:mm");
            }
        }

        public int Score { get; set; }
    }
}
