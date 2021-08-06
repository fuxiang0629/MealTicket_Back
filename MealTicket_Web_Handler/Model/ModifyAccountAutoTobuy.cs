using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Web_Handler.Model
{
    class ModifyAccountAutoTobuy
    {
    }

    public class ModifyAccountAutoTobuyRequest
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
        /// 模板类型1用户模板 2系统模板
        /// </summary>
        public int TemplateType { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 是否清除原数据
        /// </summary>
        public bool IsClearOriginal { get; set; }

        /// <summary>
        /// 跟投类型0指定账户 1分组轮询
        /// </summary>
        public int FollowType { get; set; }

        /// <summary>
        /// 周期类型1小时 2天 3周 4月 5年
        /// </summary>
        public int TimeCycleType { get; set; }

        /// <summary>
        /// 周期次数
        /// </summary>
        public int TimeCycle { get; set; }

        /// <summary>
        /// 跟投账户列表
        /// </summary>
        public List<long> FollowAccountList { get; set; }

        /// <summary>
        /// 自定义分组列表
        /// </summary>
        public List<long> GroupIdList { get; set; }

        /// <summary>
        /// 应用分组Id
        /// </summary>
        public List<long> UseGroupIdList { get; set; }
    }
}
