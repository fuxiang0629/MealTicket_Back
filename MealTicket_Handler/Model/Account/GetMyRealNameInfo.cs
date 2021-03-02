using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler.Model
{
    class GetMyRealNameInfo
    {
    }

    public class RealNameInfo
    {
        /// <summary>
        /// 身份证人头面图片
        /// </summary>
        public string ImgUrlFront { get; set; }

        /// <summary>
        /// 身份证人头面图片网络地址
        /// </summary>
        public string ImgUrlFrontShow
        {
            get 
            {
                if (string.IsNullOrEmpty(ImgUrlFront) || ImgUrlFront.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrlFront;
                }
                return string.Format("{0}/{1}",ConfigurationManager.AppSettings["imgurl"], ImgUrlFront);
            }
        }

        /// <summary>
        /// 身份证国徽面图片
        /// </summary>
        public string ImgUrlBack { get; set; }

        /// <summary>
        /// 身份证国徽面图片网络地址
        /// </summary>
        public string ImgUrlBackShow
        {
            get
            {
                if (string.IsNullOrEmpty(ImgUrlBack) || ImgUrlBack.ToUpper().StartsWith("HTTP"))
                {
                    return ImgUrlBack;
                }
                return string.Format("{0}/{1}", ConfigurationManager.AppSettings["imgurl"], ImgUrlBack);
            }
        }

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

        /// <summary>
        /// 审核状态1审核中 2审核成功 3审核失败
        /// </summary>
        public int ExamineStatus { get; set; }

        /// <summary>
        /// 审核状态说明
        /// </summary>
        public string ExamineStatusDes { get; set; }
    }
}
