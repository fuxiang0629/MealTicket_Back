using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MealTicket_Web_Handler.Model;

namespace MealTicket_Web_Handler
{
    public class AuthorityHandler
    {
        /// <summary>
        /// 添加访问日志
        /// </summary>
        /// <param name="log"></param>
        public void AddVisitLog(VisitLog log)
        {
            string sql = string.Format("insert into t_authority_account_visitlog([Function],[Ip],[Request],[Method],[Time],[Url],[UserAgent],[ResultCode],[ResultMessage],[AppVersion],[UserToken]) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7},'{8}','{9}','{10}')", log.Function, log.Ip, log.Request, log.Method, log.Time.ToString("yyyy-MM-dd HH:mm:ss.fff"), log.Url, log.Useragent, log.ResultCode, log.ResultMessage, log.AppVersion, log.UserToken);
            Singleton.Instance.sqlHelper.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 检查用户是否登录
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="baseData"></param>
        public void CheckAccountLogin(string userToken, HeadBase baseData)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {

                    ObjectParameter errorCodeDb = new ObjectParameter("errorCode", 0);
                    ObjectParameter errorMessageDb = new ObjectParameter("errorMessage", "");
                    ObjectParameter accountIdDb = new ObjectParameter("accountId", 0);
                    db.P_CheckAccountLogin_Web(userToken, errorCodeDb, errorMessageDb, accountIdDb);
                    int errorCode = (int)errorCodeDb.Value;
                    string errorMessage = errorMessageDb.Value.ToString();
                    long accountId = (long)accountIdDb.Value;
                    if (errorCode != 0)
                    {
                        throw new WebApiException(errorCode, errorMessage);
                    }
                    tran.Commit();
                    baseData.AccountId = accountId;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }
    }
}
