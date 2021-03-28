using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseHelper helper = new ParseHelper();
            string update_date = string.Empty;
            Dictionary<string, string> base_info_dic = new Dictionary<string, string>();
            Dictionary<string, string> issue_dic = new Dictionary<string, string>();

            string text = File.ReadAllText("C:\\Users\\fuxiang\\Desktop\\000002\\公司概况.txt");
            helper.ParseCompanyGeneral(text, out update_date, out base_info_dic, out issue_dic);


            List<CompanyManagerInfo> manager_list = new List<CompanyManagerInfo>();
            List<CompanyManagerHoldChangeInfo> change_list = new List<CompanyManagerHoldChangeInfo>();
            text = File.ReadAllText("C:\\Users\\fuxiang\\Desktop\\000002\\高管动向.txt");
            helper.ParseCompanyManager(text, out update_date,out manager_list,out change_list);
        }
    }
}
