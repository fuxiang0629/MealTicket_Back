using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetQuestionType
    {
    }

    /// <summary>
    /// 问题分类
    /// </summary>
    public class QuestionType
    {
        /// <summary>
        /// 分类Id
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 内部Icon
        /// </summary>
        [IgnoreDataMember]
        public string TypeIcon { get; set; }

        /// <summary>
        /// 对外Icon
        /// </summary>
        public string TypeIconUrl
        {
            get
            {
                if (string.IsNullOrEmpty(TypeIcon) || TypeIcon.ToUpper().Contains("HTTP"))
                {
                    return TypeIcon;
                }
                else
                {
                    return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], TypeIcon);
                }
            }
        }
    }
}
