using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetMyBankCardList
    {
    }
    public class BankCardInfo
    {
        /// <summary>
        /// 银行卡Id
        /// </summary>
        public long BankCardId { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// 银行卡类型
        /// </summary>
        public string CardBreed { get; set; }

        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 银行图标
        /// </summary>
        public string BankIcon { get; set; }

        /// <summary>
        /// 银行图标
        /// </summary>
        public string BankIconUrl
        {
            get
            {
                if (string.IsNullOrEmpty(BankIcon) || BankIcon.ToUpper().StartsWith("HTTP"))
                {
                    return "";
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], BankIcon);
            }
            private set { }
        }

        /// <summary>
        /// 背景图片
        /// </summary>
        public string Background { get; set; }

        /// <summary>
        /// 背景图片
        /// </summary>
        public string BackgroundUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Background) || Background.ToUpper().StartsWith("HTTP"))
                {
                    return "";
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], Background);
            }
            private set { }
        }
    }
}
