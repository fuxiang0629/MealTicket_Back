using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySmsChannelApp
    {
    }

    public class ModifySmsChannelAppRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 应用AppKey
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 应用AppSecret
        /// </summary>
        public string AppSecret { get; set; }
    }
}
