using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetAppVersionInfo
    {
    }

    public class AppVersionInfo
    {
        /// <summary>
        /// 最高版本排序值
        /// </summary>
        public int MaxVersionOrder { get; set; }

        /// <summary>
        /// 最高版本名称
        /// </summary>
        public string MaxVersionName { get; set; }

        /// <summary>
        /// 强制版本排序值
        /// </summary>
        public int ForceVersionOrder { get; set; }

        /// <summary>
        /// 版本更新地址
        /// </summary>
        public string MaxVersionUrl { get; set; }

        /// <summary>
        /// 当前版本排序值
        /// </summary>
        public int AppVersionOrder { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 首页嵌套webview url
        /// </summary>
        public string WebViewUrl { get; set; }

        /// <summary>
        /// 等待时间（秒）
        /// </summary>
        public int WaitSecond { get; set; }
    }
}
