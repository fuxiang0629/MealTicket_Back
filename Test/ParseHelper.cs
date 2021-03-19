using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class ParseHelper
    {
        /// <summary>
        /// 公司概况文件解析
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="update_date">更新时间</param>
        /// <param name="base_info_dic">公司概况</param>
        /// <param name="issue_dic">发行上市</param>
        public void ParseCompanyGeneral(string content,out string update_date,out Dictionary<string, string> base_info_dic,out Dictionary<string, string> issue_dic) 
        {
            var result = content.Replace("\r","").Split('\n');

            //更新日期
            update_date = string.Empty;
            int update_date_read = 0;//更新日期标记0未读 1即将读取 2已读取

            //更新基本资料
            base_info_dic = new Dictionary<string, string>();
            int base_info_read = 0;//基本资料标记位0未读取 1即将读取 2正在读取 3开始解析 4结束读取 5完结
            List<string> base_info_temp_key = new List<string>();
            List<string> base_info_temp_value = new List<string>();

            //更新发行上市
            issue_dic = new Dictionary<string, string>();
            int issue_read = 0;//发行上市标记位0未读取 1即将读取 2正在读取 3开始解析 4结束读取 5完结
            List<string> issue_temp_key = new List<string>();
            List<string> issue_temp_value = new List<string>();

            for (int i = 0; i < result.Length; i++)
            {
                string tempContent = result[i];
                //读取更新日期
                if (update_date_read == 1)
                {
                    if (tempContent.StartsWith("━━━") && tempContent.EndsWith("━━━"))
                    {
                        update_date_read = 2;
                    }
                    else
                    {
                        update_date_read = 0;
                        update_date = string.Empty;
                    }
                }
                if (tempContent.Contains("更新日期：") && tempContent.Length > 16 && update_date_read == 0)
                {
                    update_date_read = 1;
                    update_date = tempContent.Substring(5, 11);
                }


                //读取基本资料
                if (base_info_read == 1)//下一行一定是“━━━━━━━━━━━━━━━━━━━━━━”
                {
                    if (tempContent.StartsWith("━━━") && tempContent.EndsWith("━━━"))
                    {
                        base_info_read = 2;
                    }
                    else
                    {
                        base_info_read = 0;
                    }
                }
                if (tempContent.Contains("【基本资料】") && base_info_read == 0)
                {
                    base_info_read = 1;
                }
                if ((base_info_read == 2 || base_info_read == 3) && tempContent.StartsWith("└") && tempContent.EndsWith("┘"))//结束符
                {
                    base_info_read = 4;
                }
                //解析基本资料
                if (base_info_read == 3 || base_info_read == 4)
                {
                    if ((tempContent.StartsWith("├") && tempContent.EndsWith("┤")) || base_info_read == 4)
                    {
                        for (int k = 0; k < base_info_temp_key.Count(); k++)
                        {
                            base_info_dic.Add(base_info_temp_key[k].Trim(), base_info_temp_value[k].Trim());
                        }
                        base_info_temp_key = new List<string>();
                        base_info_temp_value = new List<string>();
                    }
                    else
                    {
                        var line = tempContent.Split('│');
                        for (int k = 1; k < line.Length - 1;)
                        {
                            if (base_info_temp_key.Count() < k / 2 + 1)
                            {
                                base_info_temp_key.Add(line[k]);
                                base_info_temp_value.Add(line[k + 1]);
                            }
                            else
                            {
                                base_info_temp_key[k / 2] = base_info_temp_key[k / 2] + line[k];
                                base_info_temp_value[k / 2] = base_info_temp_value[k / 2] + line[k + 1];
                            }
                            k = k + 2;
                        }
                    }
                }
                if (tempContent.StartsWith("┌") && tempContent.EndsWith("┐") && (base_info_read == 1 || base_info_read == 2))
                {
                    base_info_read = 3;
                }
                if (base_info_read == 4)
                {
                    base_info_read = 5;
                }

                //读取发行上市
                if (issue_read == 1)//下一行一定是“━━━━━━━━━━━━━━━━━━━━━━”
                {
                    if (tempContent.StartsWith("━━━") && tempContent.EndsWith("━━━"))
                    {
                        issue_read = 2;
                    }
                    else
                    {
                        issue_read = 0;
                    }
                }
                if (tempContent.Contains("【发行上市】") && issue_read == 0)
                {
                    issue_read = 1;
                }
                if ((issue_read == 2 || issue_read == 3) && tempContent.StartsWith("└") && tempContent.EndsWith("┘"))//结束符
                {
                    issue_read = 4;
                }
                //解析发行上市
                if (issue_read == 3 || issue_read == 4)
                {
                    if ((tempContent.StartsWith("├") && tempContent.EndsWith("┤")) || issue_read == 4)
                    {
                        for (int k = 0; k < issue_temp_key.Count(); k++)
                        {
                            issue_dic.Add(issue_temp_key[k].Trim(), issue_temp_value[k].Trim());
                        }
                        issue_temp_key = new List<string>();
                        issue_temp_value = new List<string>();
                    }
                    else
                    {
                        var line = tempContent.Split('│');
                        for (int k = 1; k < line.Length - 1;)
                        {
                            if (issue_temp_key.Count() < k / 2 + 1)
                            {
                                issue_temp_key.Add(line[k]);
                                issue_temp_value.Add(line[k + 1]);
                            }
                            else
                            {
                                issue_temp_key[k / 2] = issue_temp_key[k / 2] + line[k];
                                issue_temp_value[k / 2] = issue_temp_value[k / 2] + line[k + 1];
                            }
                            k = k + 2;
                        }
                    }
                }
                if (tempContent.StartsWith("┌") && tempContent.EndsWith("┐") && (issue_read == 1 || issue_read == 2))
                {
                    issue_read = 3;
                }
                if (issue_read == 4)
                {
                    issue_read = 5;
                }
            }

            //纠正更新日期
            if (update_date_read != 2)
            {
                update_date = string.Empty;
            }
            //纠正基本资料
            if (base_info_read != 5)
            {
                base_info_dic = new Dictionary<string, string>();
            }
            //纠正发行上市
            if (issue_read != 5)
            {
                issue_dic = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 高管文件解析
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="update_date">更新时间</param>
        /// <param name="manager_list">高管任职</param>
        /// <param name="change_list">高管持股变化</param>
        public void ParseCompanyManager(string content, out string update_date,out List<CompanyManagerInfo> manager_list,out List<CompanyManagerHoldChangeInfo> change_list)
        {
            var result = content.Replace("\r", "").Split('\n');

            //更新日期
            update_date = string.Empty;
            int update_date_read = 0;//更新日期标记0未读 1即将读取 2已读取

            //高管增减持
            change_list = new List<CompanyManagerHoldChangeInfo>(); 
            int change_read = 0;//高管任职标记位0未读取 1即将读取 2正在读取 3开始解析 4结束读取 5完结
            CompanyManagerHoldChangeInfo change_temp = new CompanyManagerHoldChangeInfo();

            //高管任职
            manager_list = new List<CompanyManagerInfo>(); 
            int manager_read = 0;//高管任职标记位0未读取 1即将读取 2正在读取 3开始解析 4结束读取 5完结
            CompanyManagerInfo manager_temp = new CompanyManagerInfo();

            for (int i = 0; i < result.Length; i++)
            {
                string tempContent = result[i];
                //读取更新日期
                if (update_date_read == 1)
                {
                    if (tempContent.StartsWith("━━━") && tempContent.EndsWith("━━━"))
                    {
                        update_date_read = 2;
                    }
                    else
                    {
                        update_date_read = 0;
                        update_date = string.Empty;
                    }
                }
                if (tempContent.Contains("更新日期：") && tempContent.Length > 16 && update_date_read == 0)
                {
                    update_date_read = 1;
                    update_date = tempContent.Substring(5, 11);
                }

                //读取高管增减持
                if (change_read == 1)//下一行一定是“━━━━━━━━━━━━━━━━━━━━━━”
                {
                    if (tempContent.StartsWith("━━━") && tempContent.EndsWith("━━━"))
                    {
                        change_read = 2;
                    }
                    else
                    {
                        change_read = 0;
                    }
                }
                if (tempContent.Contains("【高管增减持】") && change_read == 0)
                {
                    change_read = 1;
                }
                if ((change_read == 2 || change_read == 3) && tempContent.StartsWith("└") && tempContent.EndsWith("┘"))//结束符
                {
                    change_read = 4;
                }
                //解析高管增减持
                if (change_read == 3 || change_read == 4)
                {
                    if (tempContent.StartsWith("├") && tempContent.EndsWith("┤"))
                    {
                        continue;
                    }
                    if (change_read == 4)
                    {
                        change_list.Add(change_temp);
                        change_temp = new CompanyManagerHoldChangeInfo();
                    }
                    else
                    {
                        var line = tempContent.Split('｜');
                        if (!string.IsNullOrEmpty(line[1].Trim()))
                        {
                            if (!string.IsNullOrEmpty(change_temp.ChangePerson))
                            {
                                change_list.Add(change_temp);
                                change_temp = new CompanyManagerHoldChangeInfo();
                            }
                            change_temp.ChangePerson = line[1].Trim();
                            change_temp.ChangeDate = line[2].Trim();
                            change_temp.ChangeCount = line[3].Trim();
                            change_temp.DealPrice = line[4].Trim();
                            change_temp.RemainHoldCount = line[5].Trim();
                            change_temp.ChangeReason = line[6].Trim();
                            change_temp.ManagerName = line[7].Trim();
                            change_temp.Position = line[8].Trim();
                            change_temp.Relation = line[9].Trim();
                        }
                        else
                        {
                            change_temp.ChangePerson = change_temp.ChangePerson + line[1].Trim();
                            change_temp.ChangeDate = change_temp.ChangeDate + line[2].Trim();
                            change_temp.ChangeCount = change_temp.ChangeCount + line[3].Trim();
                            change_temp.DealPrice = change_temp.DealPrice + line[4].Trim();
                            change_temp.RemainHoldCount = change_temp.RemainHoldCount + line[5].Trim();
                            change_temp.ChangeReason = change_temp.ChangeReason + line[6].Trim();
                            change_temp.ManagerName = change_temp.ManagerName + line[7].Trim();
                            change_temp.Position = change_temp.Position + line[8].Trim();
                            change_temp.Relation = change_temp.Relation + line[9].Trim();
                        }
                    }
                }
                if (tempContent.StartsWith("┌") && tempContent.EndsWith("┐") && (change_read == 1 || change_read == 2))
                {
                    change_read = 3;
                }
                if (change_read == 4)
                {
                    change_read = 5;
                }



                //读取高管任职
                if (manager_read == 1)//下一行一定是“━━━━━━━━━━━━━━━━━━━━━━”
                {
                    if (tempContent.StartsWith("━━━") && tempContent.EndsWith("━━━"))
                    {
                        manager_read = 2;
                    }
                    else
                    {
                        manager_read = 0;
                    }
                }
                if (tempContent.Contains("【高管任职】") && manager_read == 0)
                {
                    manager_read = 1;
                }
                if ((manager_read == 2 || manager_read == 3) && tempContent.StartsWith("└") && tempContent.EndsWith("┘"))//结束符
                {
                    manager_read = 4;
                }
                //解析高管任职
                if (manager_read == 3 || manager_read == 4)
                {
                    if (tempContent.StartsWith("├") && tempContent.EndsWith("┤"))
                    {
                        continue;
                    }
                    if (manager_read == 4)
                    {
                        manager_list.Add(manager_temp);
                        manager_temp = new CompanyManagerInfo();
                    }
                    else
                    {
                        var line = tempContent.Split('｜');
                        if (!string.IsNullOrEmpty(line[1].Trim()))
                        {
                            if (!string.IsNullOrEmpty(manager_temp.Name))
                            {
                                manager_list.Add(manager_temp);
                                manager_temp = new CompanyManagerInfo();
                            }
                            manager_temp.Name = line[1].Trim();
                            manager_temp.Sex = line[2].Trim();
                            manager_temp.Position = line[3].Trim();
                            manager_temp.Education = line[4].Trim();
                            manager_temp.AnnualSalary = line[5].Trim();
                            manager_temp.HoldCount = line[6].Trim();
                            manager_temp.StartDate = line[7].Trim();
                        }
                        else
                        {
                            manager_temp.Name = manager_temp.Name + line[1].Trim();
                            manager_temp.Sex = manager_temp.Sex + line[2].Trim();
                            manager_temp.Position = manager_temp.Position + line[3].Trim();
                            manager_temp.Education = manager_temp.Education + line[4].Trim();
                            manager_temp.AnnualSalary = manager_temp.AnnualSalary + line[5].Trim();
                            manager_temp.HoldCount = manager_temp.HoldCount + line[6].Trim();
                            manager_temp.StartDate = manager_temp.StartDate + line[7].Trim();
                        }
                    }
                }
                if (tempContent.StartsWith("┌") && tempContent.EndsWith("┐") && (manager_read == 1 || manager_read == 2))
                {
                    manager_read = 3;
                }
                if (manager_read == 4)
                {
                    manager_read = 5;
                }
            }

            //纠正更新日期
            if (update_date_read != 2)
            {
                update_date = string.Empty;
            }
            //纠正高管任职
            if (manager_read != 5)
            {
                manager_list = new List<CompanyManagerInfo>();
            }
            if (manager_list.Count() > 0)
            {
                manager_list.RemoveAt(0);
            }
            //纠正高管持股变化
            if (change_read != 5)
            {
                change_list = new List<CompanyManagerHoldChangeInfo>();
            }
            if (change_list.Count() > 1)
            {
                change_list.RemoveAt(0);
                change_list.RemoveAt(0);
            }
        }
    }
}
