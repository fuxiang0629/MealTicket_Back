using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class ModifySysPaymentChannelGroup
    {
    }

    public class ModifySysPaymentChannelGroupRequest
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 标签类型
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// 1线上 2线下
        /// </summary>
        public int Type { get; set; }
    }
}
