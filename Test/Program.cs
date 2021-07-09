using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static long accountId = 10000;
        static void Main(string[] args)
        {
            for (int j = 0; j < 100; j++)
            {
                Task[] tArr = new Task[5];
                for (int i = 0; i < 5; i++)
                {
                    tArr[i] = new Task(doTask);
                    tArr[i].Start();
                }
                Task.WaitAll(tArr);
                accountId++;
            }
            Console.ReadLine();
        }

        static void doTask() 
        {
            using (SqlConnection conn = new SqlConnection("Data Source=106.54.124.235;Initial Catalog=meal_ticket;user id=sa;password=Gudi!qaz2wsx"))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        string sql = string.Format(@"if(not exists(select top 1 1 from t_account_shares_buy_setting with(xlock) where AccountId={0} and [Type]=3)) 
begin  
    insert into t_account_shares_buy_setting(AccountId,[Type],Name,[Description],ParValue,CreateTime,LastModified)
    values({0},3,'跟投分组轮序位置','跟投分组轮序位置',0,getdate(),getdate())
end
select top 1 ParValue from t_account_shares_buy_setting with(xlock) where AccountId={0} and [Type]=3", accountId);
                        using (SqlCommand camm = conn.CreateCommand())
                        {
                            camm.Transaction = tran;
                            camm.CommandType = CommandType.Text;
                            camm.CommandText = sql;
                            camm.ExecuteNonQuery();


                            sql = string.Format(@"update t_account_shares_buy_setting set ParValue=ParValue+1  where AccountId={0} and [Type]=3", accountId);
                            camm.CommandText = sql;
                            camm.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }
    }
}
