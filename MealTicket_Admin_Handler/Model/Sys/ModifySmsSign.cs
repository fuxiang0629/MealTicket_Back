using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySmsSign
    {
    }

    public class ModifySmsSignRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string SignName { get; set; }

        /// <summary>
        /// 营业执照
        /// </summary>
        public string LicenceUrl { get; set; }

        /// <summary>
        /// 渠道code
        /// </summary>
        public string ChannelCode { get; set; }

        /// <summary>
        /// AppKey
        /// </summary>
        public string AppKey { get; set; }
    }
}
