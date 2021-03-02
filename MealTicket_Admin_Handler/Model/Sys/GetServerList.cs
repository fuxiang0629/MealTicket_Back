using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetServerList
    {
    }

    public class GetServerListRequest :PageRequest
    {
        /// <summary>
        /// 服务器代号
        /// </summary>
        public string ServerId { get; set; }
    }

    public class ServerInfo
    {
        /// <summary>
        /// 服务器代号
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string ServerDes { get; set; }

        /// <summary>
        /// 账号数量
        /// </summary>
        public int AccountCount { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
