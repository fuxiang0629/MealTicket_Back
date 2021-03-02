using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharesTradeService
{
    public class Trade_Helper
    {
        /// <summary>
        /// 打开通达信实例
        /// </summary>
        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern int OpenTdx();

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void CloseTdx();

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern int Logon(string IP, short Port, string Version, short YybID, string AccountNo, string TradeAccount, string JyPassword, string TxPassword, StringBuilder ErrInfo);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void Logoff(int ClientID);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void QueryData(int ClientID, int Category, StringBuilder Result, StringBuilder ErrInfo);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]

        public static extern int QueryDataByePage(int ClientID, int Category, int BatchNum, StringBuilder PageMark, StringBuilder Result, StringBuilder ErrInfo);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void SendOrder(int ClientID, int Category, int PriceType, string Gddm, string Zqdm, float Price, int Quantity, StringBuilder Result, StringBuilder ErrInfo);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void CancelOrder(int ClientID, string ExchangeID, string hth, StringBuilder Result, StringBuilder ErrInfo);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void GetQuote(int ClientID, string Zqdm, StringBuilder Result, StringBuilder ErrInfo);

        [DllImport("trade.dll", CharSet = CharSet.Ansi)]
        public static extern void Repay(int ClientID, string Amount, StringBuilder Result, StringBuilder ErrInfo);
    }
}
