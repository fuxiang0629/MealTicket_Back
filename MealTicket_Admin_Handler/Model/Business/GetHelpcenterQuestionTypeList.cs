using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler.Model
{
    class GetHelpcenterQuestionTypeList
    {
    }

    public class GetHelpcenterQuestionTypeListRequest:PageRequest
    {
        /// <summary>
        /// 问题Id（传入该值会标记该问题的分类）
        /// </summary>
        public long QuestionId { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string TypeName { get; set; }
    }

    public class HelpcenterQuestionTypeInfo
    {
        /// <summary>
        /// 分类id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 分类图标（相对路径）
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// 分类图标(绝对路径)
        /// </summary>
        public string IconUrlShow 
        {
            get 
            {
                if (string.IsNullOrEmpty(IconUrl) || IconUrl.ToUpper().StartsWith("HTTP"))
                {
                    return IconUrl;
                }
                return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], IconUrl);
            }
        }

        /// <summary>
        /// 排序值
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// 数据创建时间 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsCheck { get; set; }
    }
}
