using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class GetAccountShareGroupList
    {
    }

    public class GetAccountShareGroupListRequest
    {
        /// <summary>
        /// 是否获取所有
        /// </summary>
        public bool GetAll { get; set; }
    }

    public class AccountShareGroupInfo
    {
        /// <summary>
        /// 分组Id
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 账户排序值
        /// </summary>
        public int AccountOrderIndex { get; set; }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 类型1.我的分组 2.共享给我的分组
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 分组所属账户Id
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// 分组所属账户
        /// </summary>
        public string AccountMobile { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
