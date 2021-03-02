using FXCommon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoticeHandler
{
    public static class NoticeSender
    {
        public static void SendExecute(string apiUrl, long accountId, string tempPara)
        {
            sendDel sendDel = new sendDel(sendExecute);
            IAsyncResult taskAr = sendDel.BeginInvoke(apiUrl, accountId, tempPara, null, sendDel);
        }

        private delegate void sendDel(string apiUrl, long accountId, string tempPara);

        private static void sendExecute(string apiUrl, long accountId, string tempPara)
        {
            using (var db = new NoticeEntities())
            using (var tran=db.Database.BeginTransaction())
            {
                try
                {
                    if (!string.IsNullOrEmpty(tempPara))
                    {
                        var account = (from item in db.t_account_baseinfo
                                       where item.Id == accountId
                                       select item).FirstOrDefault();
                        var temp=JsonConvert.DeserializeObject<dynamic>(tempPara);
                        temp.accountmobile = account == null ? "" : account.Mobile;
                        tempPara = JsonConvert.SerializeObject(temp);
                    }

                    db.P_Notice_Execute(apiUrl, accountId, tempPara);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    Logger.WriteFileLog("通知发送判断出错",ex);
                    tran.Rollback();
                }
            }
        }
    }
}
