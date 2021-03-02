using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetInformationList
    {
    }

    public class InformationInfo
    {
        /// <summary>
        /// 资讯Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容简介
        /// </summary>
        public string ContentIntroduction { get; set; }

        /// <summary>
        /// 是否存在详情
        /// </summary>
        public bool IsHaveContent { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    public class InformationDetails: InformationInfo
    {
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}
