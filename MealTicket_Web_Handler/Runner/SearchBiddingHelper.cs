using FXCommon.Common;
using MealTicket_DBCommon;
using MealTicket_Web_Handler.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Runner
{
    public class SearchBiddingHelper
    {
        public static void Cal_SearchBidding()
        {
            DateTime date= DbHelper.GetLastTradeDate2(0, 0, 0, 0, DateTime.Now.Date);
            List<BiddingKlineTemplateInfo> templateList = new List<BiddingKlineTemplateInfo>();
            using (var db = new meal_ticketEntities())
            {
                templateList = (from item in db.t_sys_conditiontrade_template
                                join item2 in db.t_sys_conditiontrade_template_search on item.Id equals item2.TemplateId
                                where item.Status == 1 && item.Type == 7
                                select new BiddingKlineTemplateInfo
                                {
                                    Id = item.Id,
                                    Name = item.Name,
                                    BgColor = item.BgColor,
                                    TemplateContext = item2.TemplateContent
                                }).ToList();
                Dictionary<long, List<BiddingKlineTemplateInfo>> dic = new Dictionary<long, List<BiddingKlineTemplateInfo>>();
                SearchHelper searchHelper = new SearchHelper();
                List<long> searchSharesKeyList = new List<long>();
                object searchSharesKeyListLock = new object();
                int templateListCount = templateList.Count();
                if (templateListCount > 0)
                {
                    WaitHandle[] taskArr = new WaitHandle[templateListCount];
                    int idx = 0;
                    foreach (var item in templateList)
                    {
                        taskArr[idx] = TaskThread.CreateTask((e) =>
                        {
                            var searchList = searchHelper.ParseSearchCondition(item.TemplateContext);
                            var tempSharesList = searchHelper.DoSearchTemplate(searchList);
                            foreach (var shares in tempSharesList)
                            {
                                long sharesKey = long.Parse(shares.SharesCode) * 10 + shares.Market;
                                lock (searchSharesKeyListLock)
                                {
                                    searchSharesKeyList.Add(sharesKey);
                                    if (!dic.ContainsKey(sharesKey))
                                    {
                                        dic.Add(sharesKey, new List<BiddingKlineTemplateInfo>());
                                    }
                                    dic[sharesKey].Add(new BiddingKlineTemplateInfo
                                    {
                                        Id = item.Id,
                                        BgColor = item.BgColor,
                                        Name = item.Name
                                    });
                                }
                            }
                        }, item);
                        idx++;
                    }
                    TaskThread.WaitAll(taskArr, Timeout.Infinite);
                    TaskThread.CloseAllTasks(taskArr);
                }
                searchSharesKeyList = searchSharesKeyList.Distinct().ToList();

                foreach (long sharesKey in searchSharesKeyList)
                {
                    string ContextData = null;
                    if (dic.ContainsKey(sharesKey))
                    {
                        ContextData = JsonConvert.SerializeObject(dic[sharesKey]);
                    }
                    db.t_shares_bidding_sys.Add(new t_shares_bidding_sys 
                    { 
                        CreateTime=DateTime.Now,
                        SharesKey= sharesKey,
                        Date= date,
                        ContextData= ContextData
                    });
                }
                db.SaveChanges();
            }
        }
    }
}
