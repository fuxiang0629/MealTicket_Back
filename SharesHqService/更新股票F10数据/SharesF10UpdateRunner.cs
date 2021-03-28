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
                StringBuilder sErrInfo;
                StringBuilder sResult;
                int hqClient = Singleton.Instance.GetHqClient();

                //获取解析公司概况
                //1.公司名称 2.英文名称 3.曾用简称 4.证券简称 5.证券代码 6.成立日期 7.上市日期 8.证券类别 9.经济性质 10.法人代表 11.总经理 12.公司董秘 13.证券代表 14.公司电话 15.公司传真 16.注册资本
                //17.公司规模 18.
                sErrInfo = new StringBuilder(256);
                sResult = new StringBuilder(1024 * 1024*100);

                TradeX_M.TdxHq_GetCompanyInfoCategory(hqClient, (byte)0, "000002", sResult, sErrInfo);

                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 0, 11707, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 11707, 8840, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 20547, 33479, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 54026, 86596, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 140622, 9261, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 149883, 8540, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 158423, 26302, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 184725, 47100, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 231825, 148267, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 380092, 33069, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 413161, 12305, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 425466, 14473, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 439939, 36956, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 476895, 32497, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 509392, 38327, sResult, sErrInfo);
                TradeX_M.TdxHq_GetCompanyInfoContent(hqClient, (byte)0, "000002", "000002.txt", 547719, 12392, sResult, sErrInfo);
            }
            catch (Exception ex)
            {
                Logger.WriteFileLog("SharesF10UpdateRunner出错", ex);
            }
        }
    }
}
