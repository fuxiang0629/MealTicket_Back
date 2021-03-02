using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class AccountApplyRealName
    {
    }

    public class ApplyRealNameRequest
    {
        /// <summary>
        /// 身份证人头面图片
        /// </summary>
        public string ImgUrlFront { get; set; }

        /// <summary>
        /// 身份证国徽面图片
        /// </summary>
        public string ImgUrlBack { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 性别 1男 2女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 出生年月
        /// </summary>
        public string BirthDay { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        public string CardNo { get; set; }

        /// <summary>
        /// 住址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 签发机关
        /// </summary>
        public string CheckOrg { get; set; }

        /// <summary>
        /// 签发日期
        /// </summary>
        public string ValidDateFrom { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public string ValidDateTo { get; set; }
    }
}
