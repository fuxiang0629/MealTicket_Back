using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetSysPaymentChannelGroup
    {
    }

    public class SysPaymentChannelGroupInfo
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
        /// 标签描述
        /// </summary>
        public string TagDes
        {
            get
            {
                if (Tag == 1)
                {
                    return "推荐使用";
                }
                return "";
            }
        }

        /// <summary>
        /// 1线上 2线下
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 状态1有效 2无效
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
