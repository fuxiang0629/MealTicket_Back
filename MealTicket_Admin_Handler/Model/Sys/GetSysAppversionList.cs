using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysAppversionList
    {
    }

    public class SysAppversionInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public string MarketCode { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 版本值
        /// </summary>
        public int AppVersionOrder { get; set; }

        /// <summary>
        /// 审核状态1正常上线 2审核中
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 首页webview Url
        /// </summary>
        public string WebViewUrl { get; set; }

        /// <summary>
        /// 启动时间（秒）
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
