using FXCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeAPI;

namespace SharesHqService
{
    public class SharesF10UpdateRunner:Runner
    {
        public SharesF10UpdateRunner()
        {
            SleepTime = 3600000;
            Name = "SharesF10UpdateRunner";
        }

        public override bool Check
        {
            get
            {
                return true;
            }
        }

        public override void Execute()
        {
            try
            {
                int hqClient = Singleton.Instance.GetHqClient(); 
                StringBuilder sErrInfo = new StringBuilder(256);
                StringBuilder sResult = new StringBuilder(1024 * 1024*100);
                TradeX_M.TdxHq_GetCompanyInfoCategory(hqClient,(byte)0,"000002", sResult, sErrInfo);

                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 0, 12201, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 12201, 8690, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 20891, 33479, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 54370, 98371, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 152741, 9261, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 162002, 8540, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 170542, 33182, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 203724, 45407, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 249131, 1481744, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1730875, 33016, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1763891, 12305, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1776196, 14351, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1790547, 36956, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1827503, 32947, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1860450, 45562, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 1906012, 12392, sResult, sErrInfo);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SharesF10UpdateRunner出错", ex);
            }
        }
    }
}
