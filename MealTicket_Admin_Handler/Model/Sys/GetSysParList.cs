using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysParList
    {
    }

    public class GetSysParListRequest:PageRequest
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParName { get; set; }
    }

    public class SysParInfo
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string ParValue { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string ParDescription { get; set; }

        /// <summary>
        /// 是否Json格式数据
        /// </summary>
        public bool IsJson { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
