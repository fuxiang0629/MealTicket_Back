using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetOperateLogList
    {
    }

    public class GetOperateLogListRequest:PageRequest
    {
        /// <summary>
        /// 账户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录记录Id
        /// </summary>
        public long LoginLogId { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    public class OperateLogInfo
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public long LogId { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录记录Id
        /// </summary>
        public long LoginLogId { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string OperationDes { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
